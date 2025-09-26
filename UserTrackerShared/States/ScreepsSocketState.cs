using System.Text.Json;
using System.Text.Json.Serialization;
using Socket.Io.Client.Core;

namespace UserTrackerShared.States
{
    public static class ScreepsSocket
    {
        private static SocketIoClient? _socket;
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task InitAsync()
        {
            string socketPath = "/socket";
            var token = ConfigSettingsState.ScreepsToken;
            var baseUrl = ConfigSettingsState.ScreepsHttpsUrl;
            if (_socket != null)
            {
                return;
            }

            _socket = new SocketIoClient(baseUrl.TrimEnd('/'), new SocketIoOptions
            {
                Path = socketPath,
                Transports = new[] { Transport.WebSocket, Transport.Polling }
            });

            _socket.OnConnected += async (_, __) =>
            {
                await _socket.Emit("auth", token);
                await _socket.Emit("subscribe", "console");
            };

            _socket.On("console", (_, args) => HandleConsoleArgs(args));

            await _socket.ConnectAsync();
        }

        public static async Task DisconnectAsync()
        {
            if (_socket == null) return;

            await _socket.DisconnectAsync();
            _socket.Dispose();
            _socket = null;
        }

        private static void HandleConsole(IReadOnlyList<string> logs, IReadOnlyList<string> results, string shard)
        {
            if (results.Count > 0)
            {
                foreach (var res in results)
                {
                    if (!res.Contains("orderBookTracker")) continue;
                    var orderBookResponse = JsonConvert.DeserializeObject<MarketOrderBookResponse>(res);
                    CentralOrderBookState.UpdateMarketOrderBook(new MarketOrderBook(shard, orderBookResponse));
                }
            }
        }

        private static void HandleConsoleArgs(object?[] args)
        {
            if (args is null || args.Length == 0 || args[0] is null)
                return;

            string json = args[0] switch
            {
                string s => s,
                JsonElement je => je.GetRawText(),
                _ => JsonSerializer.Serialize(args[0])
            };

            ConsoleEvent? ev;
            try
            {
                ev = JsonSerializer.Deserialize<ConsoleEvent>(json, _jsonOpts);
            }
            catch
            {
                return; // ignore malformed messages
            }

            if (ev == null || ev.Shard == null) return;

            var logs = (IReadOnlyList<string>)(ev.Messages?.Log ?? new List<string>());
            var results = (IReadOnlyList<string>)(ev.Messages?.Results ?? new List<string>());

            HandleConsole(logs, results, ev.Shard);
        }
    }
}