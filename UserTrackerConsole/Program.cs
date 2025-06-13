using System;
using UserTrackerShared.DBClients;
using UserTrackerShared.States;
using UserTrackerShared.Utilities;


ConfigSettingsState.Init();
Screen.Init();
DBClient.Init();
await GameState.InitAsync();

while (true)
{
    Thread.Sleep(10000); // Wait for 10 seconds
}
