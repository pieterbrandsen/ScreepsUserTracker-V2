using UserTrackerShared.Helpers;

namespace UserTrackerConsole;

internal static class Program
{
    private static async Task<int> Main()
    {
        using var lifetime = new ConsoleLifetime();
        return await ConsoleApp.RunAsync(lifetime.Token);
    }
}
