using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UserTrackerShared.DBClients;
using Xunit;

namespace UserTracker.Tests.DBClients
{
    public class GraphiteBatchClientTests : IDisposable
    {
        private readonly TcpListener _listener;

        public GraphiteBatchClientTests()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
        }

        [Fact]
        public async Task Flush_SendsBufferedMetricsOverTcp()
        {
            var acceptTask = AcceptPayloadAsync();
            using var client = new GraphiteBatchClient(IPAddress.Loopback.ToString(), GetPort(), batchSize: 2);

            client.AddMetric("unit.metric.one", 1, 1000);
            client.AddMetric("unit.metric.two", 2, 2000);

            var payload = await acceptTask;

            Assert.Equal("unit.metric.one 1 1\nunit.metric.two 2 2\n", payload);
        }

        private int GetPort() => ((IPEndPoint)_listener.LocalEndpoint).Port;

        private async Task<string> AcceptPayloadAsync()
        {
            using var tcpClient = await _listener.AcceptTcpClientAsync();
            using var stream = tcpClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            return await reader.ReadToEndAsync();
        }

        public void Dispose()
        {
            _listener.Stop();
        }
    }
}
