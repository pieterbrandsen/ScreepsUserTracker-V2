using UserTrackerShared;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;
using UserTrackerStates.DBClients;

ConfigSettingsState.Init();
Screen.Init();
DBClient.Init();
await GameState.InitAsync();

GC.SuppressFinalize(typeof(object));
while (true)
{
    Thread.Sleep(10000); // Wait for 10 seconds
    GC.Collect();        // Force garbage collection
    GC.WaitForPendingFinalizers(); // Ensure all finalizers are executed
}
