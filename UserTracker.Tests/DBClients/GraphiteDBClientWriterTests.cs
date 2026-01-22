using System;
using System.Collections.Generic;
using UserTrackerShared.DBClients;
using Xunit;

namespace UserTracker.Tests.DBClients
{
    public class GraphiteDBClientWriterTests : IDisposable
    {
        private readonly RecordingGraphiteBatchClient _recorder;

        public GraphiteDBClientWriterTests()
        {
            _recorder = new RecordingGraphiteBatchClient();
            GraphiteDBClientWriter.SetClientForTesting(_recorder);
        }

        [Fact]
        public void UploadData_FlattenNumericValues_SendsEveryNumericField()
        {
            var payload = new
            {
                Total = 5,
                Stats = new
                {
                    Score = 10,
                    Label = "ignored"
                },
                Items = new[]
                {
                    new { Amount = 3 }
                }
            };

            GraphiteDBClientWriter.UploadData("prefix.", payload, 9999);

            Assert.Equal(3, _recorder.Metrics.Count);
            Assert.Contains(_recorder.Metrics, m => m.Path == "prefix.Total" && m.Value == 5 && m.Timestamp == 9999);
            Assert.Contains(_recorder.Metrics, m => m.Path == "prefix.Stats.Score" && m.Value == 10 && m.Timestamp == 9999);
            Assert.Contains(_recorder.Metrics, m => m.Path == "prefix.Items[0].Amount" && m.Value == 3 && m.Timestamp == 9999);
        }

        [Fact]
        public void UploadData_WithShardRoomUsername_PrependsShardInfo()
        {
            var payload = new { Rcl = 8 };

            GraphiteDBClientWriter.UploadData("history.", "shard0", "E1S1", 222, "player", payload);

            Assert.Single(_recorder.Metrics);
            var metric = _recorder.Metrics[0];
            Assert.Equal("history.shard0.player.E1S1.Rcl", metric.Path);
            Assert.Equal(8, metric.Value);
            Assert.Equal(222, metric.Timestamp);
        }

        public void Dispose()
        {
            GraphiteDBClientWriter.ResetClientForTesting();
        }

        private sealed class RecordingGraphiteBatchClient : IGraphiteBatchClient
        {
            public List<RecordedMetric> Metrics { get; } = new();

            public void AddMetric(string metricPath, double value, long timestamp)
            {
                Metrics.Add(new RecordedMetric(metricPath, value, timestamp));
            }

            public void Flush()
            {
            }
        }

        private readonly record struct RecordedMetric(string Path, double Value, long Timestamp);
    }
}
