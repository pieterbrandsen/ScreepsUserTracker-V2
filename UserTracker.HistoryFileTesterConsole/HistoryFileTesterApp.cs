using UserTrackerShared.DBClients;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;

namespace UserTracker.HistoryFileTesterConsole;

internal sealed class HistoryFileTesterApp
{
    public async Task<int> RunAsync(CancellationToken token)
    {
        try
        {
            await InitializeAsync();
            await ProcessPendingPartsAsync(token);
            await WaitForExitAsync(token);
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
        HistoryConfigSettingsState.Init();
        await DBClient.InitAsync();
    }

    private static async Task ProcessPendingPartsAsync(CancellationToken token)
    {
        var baseDirectory = HistoryConfigSettingsState.HistoryBasePath;
        ZipPartHandler.Initialize(baseDirectory);

        var pendingParts = ZipPartHandler.GetPendingParts();
        foreach (var part in pendingParts)
        {
            token.ThrowIfCancellationRequested();
            await ZipPartHandler.Handle(part);
        }
    }

    private static async Task WaitForExitAsync(CancellationToken token)
    {
        try
        {
            await Task.Run(Console.ReadLine, token);
        }
        catch (OperationCanceledException)
        {
            // Shutdown requested.
        }
    }
}
