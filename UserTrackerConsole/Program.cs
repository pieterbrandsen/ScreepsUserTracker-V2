using UserTrackerShared;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;
using UserTrackerStates.DBClients;

ConfigSettingsState.Init();
Screen.Init();
DBClient.Init();
await GameState.InitAsync();

while (true)
{
    Thread.Sleep(10000); // Wait for 10 seconds
}
