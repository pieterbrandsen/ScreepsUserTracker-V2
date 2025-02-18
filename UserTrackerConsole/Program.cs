using UserTrackerShared;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;
using UserTrackerStates;

ThreadPool.SetMinThreads(5000, 5000);
ThreadPool.SetMaxThreads(5000, 5000);

ConfigSettingsState.Init();
Screen.Init();
InfluxDBClientState.Init();
await GameState.InitAsync();

while (true)
{
    Thread.Sleep(1000);
}