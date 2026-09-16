import assert from 'node:assert/strict';
import test from 'node:test';
import {
  applyTeamParams,
  baseQuery,
  substituteParameters,
  transformData,
} from './app.js';

function createParams(overrides = {}) {
  return {
    datasource: 'mmo',
    data: 'structurecount',
    dataNames: 'Total',
    usedDataType: 'user',
    shards: ['shard0', 'shard1'],
    users: ['user1'],
    rooms: ['E1N1'],
    fromTime: '2026-09-01T00:00:00.000Z',
    toTime: '2026-09-02T00:00:00.000Z',
    sampleInterval: '4h',
    ...overrides,
  };
}

test('keeps individual user series at the selection limit', () => {
  const users = Array.from({ length: 100 }, (_, index) => `user${index}`);
  const params = applyTeamParams(createParams({ users }));

  assert.equal(params.aggregateSeries, false);
  assert.equal(params.metric, 'shard, user');
  assert.equal(params.users.length, 100);
});

test('aggregates every selected user above the selection limit', () => {
  const users = Array.from({ length: 101 }, (_, index) => `user${index}`);
  const params = applyTeamParams(createParams({ users, rooms: ['E1N1', 'E2N2'] }));
  const query = substituteParameters(baseQuery.query, params);

  assert.equal(params.aggregateSeries, true);
  assert.equal(params.metric, 'shard');
  assert.deepEqual(params.users, []);
  assert.deepEqual(params.rooms, []);
  assert.match(query, /max\(tick\) AS tick/);
  assert.match(query, /shard IN \('shard0','shard1'\)/);
  assert.doesNotMatch(query, /user IN/);
  assert.doesNotMatch(query, /room IN/);
});

test('keeps team queries grouped by team regardless of member count', () => {
  const users = Array.from({ length: 101 }, (_, index) => `user${index}`);
  const params = applyTeamParams(createParams({
    users: ['ignored'],
    teams: { alpha: users },
  }));

  assert.equal(params.aggregateSeries, false);
  assert.equal(params.metric.includes('AS team'), true);
  assert.equal(params.users.length, 101);
});

test('maps sampled rows to Grafana data points', () => {
  const data = transformData(createParams(), [[
    '2026-09-01T00:00:00.000000Z',
    123456,
    42.5,
    'shard0',
    'user1',
  ]]);

  assert.deepEqual(data, [{
    time: '2026-09-01T00:00:00.000000Z',
    tick: 123456,
    Total: 42.5,
    metric: 'shard0 user1',
  }]);
});
