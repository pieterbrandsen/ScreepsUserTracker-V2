using UserTrackerShared.DBClients;
using UserTrackerShared.States;
using UserTrackerShared.Utilities;

namespace UserTrackerConsole;

internal sealed class ConsoleApp
{
    public async Task<int> RunAsync(CancellationToken token)
    {
        try
        {
            await InitializeAsync();
            await GameState.InitAsync();
            await WaitForShutdownAsync(token);
            return 0;
        }
        catch (OperationCanceledException)
        {
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static async Task InitializeAsync()
    {
        ConfigSettingsState.Init();
        Screen.Init();
        await DBClient.InitAsync();
    }

    private static Task WaitForShutdownAsync(CancellationToken token)
    {
        return Task.Delay(Timeout.InfiniteTimeSpan, token);
    }
}
