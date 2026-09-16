import express from 'express';
import axios from 'axios';
import cors from 'cors';
import bodyParser from 'body-parser';
import http from 'http';
import https from 'https';
import pg from 'pg';

const { Pool } = pg;

const app = express();
app.use(bodyParser.text({ type: '*/*', limit: '500mb' }));

const PORT = 9001;
const QUESTDB_URL = 'http://questdb:9000';

const pgPool = new Pool({
  host: process.env.QUESTDB_PG_HOST || 'questdb',
  port: Number.parseInt(process.env.QUESTDB_PG_PORT || '8812'),
  user: process.env.QUESTDB_PG_USER,
  password: process.env.QUESTDB_PG_PASSWORD,
  database: process.env.QUESTDB_PG_DATABASE || 'qdb',
  max: 20,
  idleTimeoutMillis: 30000,
  connectionTimeoutMillis: 600000,
  query_timeout: 600000,
  statement_timeout: 600000
});

const httpAgent = new http.Agent({
  keepAlive: true,
  keepAliveMsecs: 300000,
  maxSockets: 100,
  maxFreeSockets: 10,
  timeout: 600000
});

const httpsAgent = new https.Agent({
  keepAlive: true,
  keepAliveMsecs: 300000,
  maxSockets: 100,
  maxFreeSockets: 10,
  timeout: 600000
});

app.use(cors());
app.use(express.json({ limit: '500mb' }));

const globalDataTypeFilter = "AND shard IN ({{shards}})";
const userDataTypeFilter = "AND shard IN ({{shards}}) AND user IN ({{users}})";
const roomDataTypeFilter = "AND shard IN ({{shards}}) AND user IN ({{users}}) AND room IN ({{rooms}})";
const MAX_INDIVIDUAL_SERIES = 100;

export const baseQuery = {
  query: `SELECT timestamp as time, max(tick) AS tick, {{data}}, {{metric}}
FROM {{datasource}}_{{usedDataType}}_history
WHERE timestamp >= '{{fromTime}}' AND timestamp <= '{{toTime}}'
{{usedDataTypeFilter}}
SAMPLE BY {{sampleInterval}}
ALIGN TO CALENDAR`,
  params: [
    'datasource',
    'metric',
    'data',
    'dataNames',
    'usedDataType',
    'shards',
    'users',
    'rooms',
    'fromTime',
    'toTime',
    'sampleInterval'
  ],
};

function parseLooseBody(bodyText) {
  if (!bodyText || typeof bodyText !== 'string') return {};

  try {
    return JSON.parse(bodyText);
  } catch { }

  let normalized = bodyText;

  function quoteListItems(inner) {
    return inner
      .split(',')
      .map(v => v.trim())
      .filter(Boolean)
      .map(v => `"${v.replace(/^["']|["']$/g, '').replace(/"/g, '\\"')}"`)
      .join(',');
  }

  try {
    // "shards": shard -> "shards": ["shard"]
    normalized = normalized.replace(
      /"shards"\s*:\s*([A-Za-z0-9_.\-]+)(\s*[,}])/g,
      (_, value, end) => `"shards": ["${value}"]${end}`
    );

    // "shards": {shard1,shard2} -> "shards": ["shard1","shard2"]
    normalized = normalized.replace(
      /"shards"\s*:\s*\{([^{}]*)\}/gs,
      (_, inner) => `"shards": [${quoteListItems(inner)}]`
    );

    // "users": {a,b,c} -> "users": ["a","b","c"]
    normalized = normalized.replace(
      /"users"\s*:\s*\{([^{}]*)\}/gs,
      (_, inner) => `"users": [${quoteListItems(inner)}]`
    );

    // "rooms": {E1N1,E2N2} -> "rooms": ["E1N1","E2N2"]
    normalized = normalized.replace(
      /"rooms"\s*:\s*\{([^{}]*)\}/gs,
      (_, inner) => `"rooms": [${quoteListItems(inner)}]`
    );

    normalized = normalized.replace(/,(\s*[\]}])/g, '$1');

    return JSON.parse(normalized);
  } catch (err) {
    console.error('❌ Still failed to normalize partial JSON:', err.message);
    console.error('Normalized body start:', normalized.slice(0, 1000));
    console.error('Normalized body end:', normalized.slice(-1000));
    return {};
  }
}

function escapeSqlString(value) {
  return String(value).replaceAll(/'/g, "''");
}

function formatArrayParam(value) {
  if (Array.isArray(value)) {
    return value.map(v => `'${escapeSqlString(v)}'`).join(',');
  }

  return `'${escapeSqlString(value)}'`;
}

export function hasTeams(params) {
  return params.teams && typeof params.teams === 'object' && Object.keys(params.teams).length > 0;
}

export function selectionSize(selection) {
  return Array.isArray(selection) ? selection.length : selection ? 1 : 0;
}

export function shouldAggregateSeries(params) {
  if (hasTeams(params)) {
    return false;
  }

  if (params.usedDataType === 'user') {
    return selectionSize(params.users) > MAX_INDIVIDUAL_SERIES;
  }

  if (params.usedDataType === 'room') {
    return selectionSize(params.users) > MAX_INDIVIDUAL_SERIES ||
      selectionSize(params.rooms) > MAX_INDIVIDUAL_SERIES;
  }

  return false;
}

function getTeamUsers(teams) {
  return [...new Set(Object.values(teams).flat())];
}

function buildTeamCase(teams) {
  const cases = Object.entries(teams).map(([teamName, users]) => {
    return `WHEN user IN (${formatArrayParam(users)}) THEN '${escapeSqlString(teamName)}'`;
  });

  return `CASE ${cases.join(' ')} ELSE 'unknown_team' END`;
}

export function getMetric(params) {
  if (params.aggregateSeries) {
    return 'shard';
  }

  if (hasTeams(params)) {
    const teamCase = buildTeamCase(params.teams);

    switch (params.usedDataType) {
      case 'global':
        return 'shard';
      case 'user':
        return `${teamCase} AS team`;
      case 'room':
        return `${teamCase} AS team, room`;
      default:
        return 'unknown_metric';
    }
  }

  switch (params.usedDataType) {
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

export function applyTeamParams(params) {
  if (hasTeams(params)) {
    params.users = getTeamUsers(params.teams);
  }

  params.aggregateSeries = shouldAggregateSeries(params);
  if (params.aggregateSeries) {
    params.users = [];
    params.rooms = [];
  }

  params.metric = getMetric(params);
  return params;
}

function getRequiredParams(params) {
  const required = [...baseQuery.params];

  if (params.usedDataType === 'global') {
    return required.filter(param => !['users', 'rooms'].includes(param));
  }

  if (params.usedDataType === 'user') {
    return required.filter(param => param !== 'rooms');
  }

  return required;
}

export function substituteParameters(query, params) {
  let substituted = query;

  if (params.aggregateSeries || params.usedDataType === 'global') {
    substituted = substituted.replaceAll('{{usedDataTypeFilter}}', globalDataTypeFilter);
  } else if (params.usedDataType === 'user') {
    substituted = substituted.replaceAll('{{usedDataTypeFilter}}', userDataTypeFilter);
  } else if (params.usedDataType === 'room') {
    substituted = substituted.replaceAll('{{usedDataTypeFilter}}', roomDataTypeFilter);
  }

  Object.keys(params).forEach(key => {
    let value = params[key];
    const placeholder = `{{${key}}}`;

    if (Array.isArray(value) || (typeof value === 'string' && ['shards', 'users', 'rooms'].includes(key))) {
      value = Array.isArray(value) ? formatArrayParam(value) : `'${escapeSqlString(value)}'`;
    }

    if (key === 'data') {
      const dataNames = params.dataNames.split(',').map(v => v.trim());
      const dataFields = value.split(',').map(v => v.trim());

      value = dataFields
        .map((field, i) => `avg(${field}) AS '${escapeSqlString(dataNames[i])}'`)
        .join(', ');
    }

    substituted = substituted.replaceAll(new RegExp(placeholder, 'g'), value);
  });

  return substituted;
}

export function transformData(params, dataArray) {
  const dataNames = params.dataNames.split(',').map(v => v.trim());
  const dataList = [];

  for (const entry of dataArray) {
    const dataEntry = { time: entry[0], tick: entry[1] };
    const metricParts = [];
    let valueIndex = 0;

    for (let i = 2; i < entry.length; i++) {
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
  res.json({
    ...baseQuery,
    optionalParams: ['teams'],
    teamsExample: {
      teamA: ['user1', 'user2'],
      teamB: ['user3']
    }
  });
});

app.post('/api/execute', async (req, res) => {
  req.setTimeout(900000);
  res.setTimeout(900000);

  const params = applyTeamParams(parseLooseBody(req.body));
  let finalQuery = '';

  try {
    const requiredParams = getRequiredParams(params);
    const missingParams = requiredParams.filter(param => !params[param]);

    if (missingParams.length > 0) {
      console.log('Missing required parameters', missingParams);

      return res.status(400).json({
        error: 'Missing required parameters',
        missing: missingParams,
        required: requiredParams,
      });
    }

    finalQuery = substituteParameters(baseQuery.query, params);
    console.log('\r\n\r\nExecuting Query Length:', finalQuery.trim().length, 'chars');

    const queryLength = finalQuery.trim().length;

    if (queryLength > 8000) {
      console.log('Using PostgreSQL wire protocol (query too long for REST API)');

      const pgResult = await pgPool.query(finalQuery.trim());
      const dataArray = pgResult.rows.map(row => Object.values(row));

      console.log(`Query executed successfully via PG, returned ${dataArray.length} rows.`);

      res.json({
        executedQuery: finalQuery.trim(),
        parameters: params,
        data: transformData(params, dataArray)
      });
    } else {
      console.log('Using REST API');

      const questResponse = await axios.get(`${QUESTDB_URL}/exec`, {
        params: {
          query: finalQuery.trim(),
          count: true
        },
        timeout: 600000,
        maxContentLength: Infinity,
        maxBodyLength: Infinity,
        httpAgent,
        httpsAgent
      });

      const dataArray = questResponse.data?.dataset || questResponse.data || [];

      console.log(`Query executed successfully via REST, returned ${dataArray.length} rows.`);

      res.json({
        executedQuery: finalQuery.trim(),
        parameters: params,
        data: transformData(params, dataArray)
      });
    }
  } catch (error) {
    console.error('Query execution error:', error.message);
    console.error('Executed Query:', finalQuery.trim());

    if (error.response) {
      res.status(error.response.status).json({
        error: 'QuestDB error',
        message: error.response.data?.error || error.message,
        executedQuery: finalQuery.trim()
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
    const params = applyTeamParams({ ...req.query });

    if (typeof params.teams === 'string') {
      params.teams = JSON.parse(params.teams);
      applyTeamParams(params);
    }

    const requiredParams = getRequiredParams(params);
    const missingParams = requiredParams.filter(param => !params[param]);

    if (missingParams.length > 0) {
      return res.status(400).json({
        error: 'Missing required parameters for debug',
        missing: missingParams,
        required: requiredParams,
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

if (process.env.NODE_ENV !== 'test') {
  const server = app.listen(PORT, () => {
    console.log(`Query Middleware running on port ${PORT}`);
    console.log(`Available endpoints:`);
    console.log(`  GET /api/query - Get base query template info`);
    console.log(`  POST /api/execute - Execute query with parameters`);
    console.log(`  GET /api/debug - Debug query substitution`);
    console.log(`\nScreeps Base Query Template:`);
    console.log(`\nParameters: ${baseQuery.params.join(', ')}`);
  });

  server.timeout = 900000;
  server.keepAliveTimeout = 900000;
  server.headersTimeout = 910000;
}
