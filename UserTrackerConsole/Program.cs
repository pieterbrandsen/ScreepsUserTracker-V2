using UserTrackerShared;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;
using UserTrackerStates;

ConfigSettingsState.Init();
Screen.Init();
InfluxDBClientState.Init();
await GameState.InitAsync();

while (true)
{
    Thread.Sleep(1000);
}