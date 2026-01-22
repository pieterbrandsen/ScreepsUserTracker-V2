namespace UserTrackerConsole;

internal static class Program
{
    private static async Task<int> Main()
    {
        using var lifetime = new ConsoleLifetime();
        var app = new ConsoleApp();
        return await app.RunAsync(lifetime.Token);
    }
}
