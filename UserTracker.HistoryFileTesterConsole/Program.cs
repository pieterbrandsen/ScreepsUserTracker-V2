using UserTrackerShared.Helpers;

namespace UserTracker.HistoryFileTesterConsole;

internal static class Program
{
    private static async Task<int> Main()
    {
        using var lifetime = new ConsoleLifetime();
        var app = new HistoryFileTesterApp();
        return await app.RunAsync(lifetime.Token);
    }
}
