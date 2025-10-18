import express from 'express';
import axios from 'axios';
import cors from 'cors';
import bodyParser from 'body-parser';

const app = express();
app.use(bodyParser.text({ type: '*/*' }));
const PORT = 9001;
const QUESTDB_URL = 'http://questdb:9000';

app.use(cors());
app.use(express.json());

const globalDataTypeFilter = "AND shard IN ({{shards}})";
const userDataTypeFilter = "AND shard IN ({{shards}}) AND user IN ({{users}})";
const roomDataTypeFilter = "AND shard IN ({{shards}}) AND room IN ({{rooms}})";

const baseQuery = {
  query: `SELECT timestamp as time, {{data}}, {{metric}}
FROM mmo_{{usedDataType}}_history
WHERE timestamp >= '{{fromTime}}' AND timestamp <= '{{toTime}}'
{{usedDataTypeFilter}}
SAMPLE BY {{sampleInterval}}
ALIGN TO CALENDAR`,
  params: ['metric', 'data', 'dataNames', 'usedDataType', 'shards', 'users', 'rooms', 'fromTime', 'toTime', 'sampleInterval'],
};

function parseLooseBody(bodyText) {
  if (!bodyText || typeof bodyText !== "string") return {};

  // 1️⃣ Try valid JSON
  try {
    return JSON.parse(bodyText);
  } catch { }

  let normalized = bodyText;

  // --- PREVENT date/time breakage ---
  // Wrap timestamps temporarily with a placeholder
  const timestamps = [];
  normalized = normalized.replace(
    /(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z)/g,
    (match) => {
      const matchWithoutZ = match.replace(/Z$/, '');
      timestamps.push(matchWithoutZ);
      return `__TS${timestamps.length - 1}__`;
    }
  );

  // 2️⃣ Convert {a,b,c} → ["a","b","c"]
  normalized = normalized.replace(
    /:\s*\{([^}]*)\}/gs,
    (_, inner) => {
      const items = inner
        .split(/,(?![^"]*")/)
        .map(v => v.trim())
        .filter(Boolean)
        .map(v => `"${v.replace(/^["']|["']$/g, '').replace(/"/g, '\\"')}"`)
        .join(',');
      return `: [${items}]`;
    }
  );

  // 3️⃣ Quote bare values like shards: shard3
  normalized = normalized.replace(
    /:\s*([A-Za-z0-9_.\-]+)/g,
    (m, val) => {
      if (/^["[{]/.test(val)) return m;
      return `: "${val}"`;
    }
  );

  // 4️⃣ Remove trailing commas
  normalized = normalized.replace(/,(\s*[\]}])/g, '$1');

  // --- RESTORE timestamps ---
  normalized = normalized.replace(/"__TS(\d+)__"/g, (_, idx) => `"${timestamps[idx]}"`);

  try {
    return JSON.parse(normalized);
  } catch (err) {
    console.error('❌ Still failed to normalize partial JSON:', err.message);
    const pos = err.position || 0;
    console.error('Snippet around error:', normalized.slice(Math.max(0, pos - 100), pos + 100));
    return {};
  }
}

// Helper function to format array parameters for SQL IN clauses
function formatArrayParam(value) {
  if (Array.isArray(value)) {
    return value.map(v => `'${v.replace(/'/g, "''")}'`).join(',');
  }
  return `'${value.replace(/'/g, "''")}'`;
}

function getMetric(usedDataType) {
  switch (usedDataType) {
    case 'global':
      return 'shard';
    case 'user':
      return 'shard, user';
    case 'room':
      return 'shard, room';
    default:
      return 'unknown_metric';
  }
}

// Helper function to substitute parameters in query
function substituteParameters(query, params) {
  let substituted = query;
  if (params.usedDataType === 'global') {
    substituted = substituted.replace('{{usedDataTypeFilter}}', globalDataTypeFilter);
  } else if (params.usedDataType === 'user') {
    substituted = substituted.replace('{{usedDataTypeFilter}}', userDataTypeFilter);
  } else if (params.usedDataType === 'room') {
    substituted = substituted.replace('{{usedDataTypeFilter}}', roomDataTypeFilter);
  }

  Object.keys(params).forEach(key => {
    let value = params[key];
    const placeholder = `{{${key}}}`;

    if (Array.isArray(value) || (typeof value === 'string' && ['shards', 'users', 'rooms'].includes(key))) {
      value = Array.isArray(value) ? formatArrayParam(value) : `'${value}'`;
    }
    if (key == 'data') {
      const dataNames = params['dataNames'].split(',').map(v => v.trim());
      let updatedValue = '';
      const dataFields = value.split(',').map(v => v.trim());
      for (let i = 0; i < dataFields.length; i++) {
        updatedValue += `avg(${dataFields[i]}) AS ${dataNames[i]}`;
        if (i < dataFields.length - 1) {
          updatedValue += ', ';
        }
      }
      value = updatedValue;
    }
    substituted = substituted.replace(new RegExp(placeholder, 'g'), value);
  });

  return substituted;
}

function transformData(params, dataArray) {
  const dataNames = params['dataNames'].split(',').map(v => v.trim());
  const dataList = [];

  for (const entry of dataArray) {
    const dataEntry = { time: entry[0] };
    let metricParts = [];
    let valueIndex = 0;

    for (let i = 1; i < entry.length; i++) {
      const item = entry[i];

      if (typeof item === 'number') {
        dataEntry[dataNames[valueIndex]] = item;
        valueIndex++;
      } else if (typeof item === 'string') {
        metricParts.push(item);
      }
    }

    dataEntry.metric = metricParts.join(' ').trim();
    dataList.push(dataEntry);
  }

  return dataList;
}

app.get('/api/query', (req, res) => {
  res.json(baseQuery);
});

app.post('/api/execute', async (req, res) => {
  const params = parseLooseBody(req.body);
  params.metric = getMetric(params.usedDataType);
  let finalQuery = '';

  try {
    const missingParams = baseQuery.params.filter(param => !params[param]);
    if (missingParams.length > 0) {
      console.log("Missing required parameters", missingParams)
      return res.status(400).json({
        error: 'Missing required parameters',
        missing: missingParams,
        required: baseQuery.params,
      });
    }

    finalQuery = substituteParameters(baseQuery.query, params);
    console.log('Executing Query:', finalQuery.trim());
    const questResponse = await axios.get(`${QUESTDB_URL}/exec`, {
      params: { query: finalQuery.trim() },
      timeout: 30000
    });

    const dataArray = questResponse.data?.dataset || questResponse.data || [];

    res.json({
      executedQuery: finalQuery.trim(),
      parameters: params,
      data: transformData(params, dataArray)
    });
  } catch (error) {
    console.error('Query execution error:', error.message);
    console.error('Executed Query:', finalQuery.trim());

    if (error.response) {
      res.status(error.response.status).json({
        error: 'QuestDB error',
        message: error.response.data?.error || error.message,
        executedQuery: params.usedDataType || 'unknown'
      });
    } else {
      res.status(500).json({
        error: 'Internal server error',
        message: error.message
      });
    }
  }
});

app.get('/api/debug', (req, res) => {
  try {
    const params = req.query;

    const missingParams = baseQuery.params.filter(param => !params[param]);
    if (missingParams.length > 0) {
      return res.status(400).json({
        error: 'Missing required parameters for debug',
        missing: missingParams,
        required: baseQuery.params,
      });
    }

    const finalQuery = substituteParameters(baseQuery.query, params);
    res.json({
      parameters: params,
      template: baseQuery.query,
      finalQuery: finalQuery.trim(),
      params: baseQuery.params,
    });

  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});

app.listen(PORT, () => {
  console.log(`Query Middleware running on port ${PORT}`);
  console.log(`Available endpoints:`);
  console.log(`  GET /api/query - Get base query template info`);
  console.log(`  GET /api/execute - Execute query with parameters`);
  console.log(`  GET /api/debug - Debug query substitution`);
  console.log(`\nScreeps Base Query Template:`);
  console.log(`\nParameters: ${baseQuery.params.join(', ')}`);
});