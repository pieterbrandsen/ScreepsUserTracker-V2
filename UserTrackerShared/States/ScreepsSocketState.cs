using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserTrackerShared.Models.ScreepsAPI;
using Websocket.Client;

namespace UserTrackerShared.States
{
    public static class ScreepsSocket
    {
        private static Uri? _uri;
        private static WebsocketClient? _client;

        private static bool _authed;
        private static string? _token => ConfigSettingsState.ScreepsToken;
        private static string? _userId => "5ba4aa669fc1f31f7c8f0809";

        // Public state
        public static bool IsConnected => _client?.IsRunning ?? false;
        public static bool IsAuthed => _authed;

        public static void Init()
        {
            var baseUrl = ConfigSettingsState.ScreepsHttpUrl;
            var wsUrl = new Uri(new Uri(baseUrl.Replace("http", "wss")), "socket/websocket");
            _uri = wsUrl;

            _client = new WebsocketClient(_uri)
            {
                ReconnectTimeout = TimeSpan.FromSeconds(60),
                ErrorReconnectTimeout = TimeSpan.FromSeconds(5)
            };

            _client.MessageReceived.Subscribe(msg =>
            {
                if (!string.IsNullOrWhiteSpace(msg.Text))
                {
                    try
                    {
                        if (!msg.Text.StartsWith("[")) return;
                        var arr = JsonConvert.DeserializeObject<object[]>(msg.Text);
                        if (arr?.Length == 2 && arr[1] is JObject obj)
                        {
                            var eventData = obj.ToObject<ConsoleEvent>();
                            if (eventData?.Shard != null)
                                HandleMessage(eventData);
                        }

                    }
                    catch (Exception e)
                    {
                        // accept
                    }
                }
            });

            _client.ReconnectionHappened.Subscribe(_ =>
            {
                _authed = false;
                if (!string.IsNullOrEmpty(_token))
                {
                    var discard = AuthAndSubscribeAsync(_token);
                }
            });

            _client.DisconnectionHappened.Subscribe(_ =>
            {
                _authed = false;
            });
        }

        public static async Task ConnectAsync(CancellationToken ct = default)
        {
            if (_client == null) throw new InvalidOperationException("Call Init() first.");
            await _client.Start();

            if (!string.IsNullOrEmpty(_token))
            {
                await AuthAndSubscribeAsync(_token);
            }
        }

        public static async Task DisconnectAsync()
        {
            if (_client == null) return;
            await _client.Stop(WebSocketCloseStatus.NormalClosure, "Closed by client");
        }

        private static async Task AuthAndSubscribeAsync(string token)
        {
            if (_client == null) throw new InvalidOperationException("Socket not initialized.");
            if (_authed) return;

            await _client.SendInstant($"auth {token}");

            _client.MessageReceived
                .Where(m => m.Text != null && m.Text.StartsWith("auth"))
                .Take(1)
                .Subscribe(async m =>
                {
                    var parts = m.Text!.Split(' ');
                    if (parts.Length >= 2 && parts[1] == "ok")
                    {
                        _authed = true;

                        if (!string.IsNullOrEmpty(_userId))
                        {
                            await _client.SendInstant($"subscribe user:{_userId}/console");
                        }
                    }
                });
        }

        private static void HandleMessage(ConsoleEvent consoleEvent)
        {
            if (consoleEvent.Messages == null) return;
            foreach (var result in consoleEvent.Messages.Results)
            {
                if (!result.Contains("orderBookTracker")) return;

                var orderbookResponse = JsonConvert.DeserializeObject<MarketOrderBookResponse>(result);
                if (orderbookResponse == null) return;
                var orderBook = new MarketOrderBook(consoleEvent.Shard, orderbookResponse);
                CentralOrderBookTrackerState.UpdateMarketOrderBook(orderBook);
            }
        }

        public static void Dispose()
        {
            _client?.Dispose();
        }
    }
}
