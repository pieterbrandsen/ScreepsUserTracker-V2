using UserTrackerShared;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;
using UserTrackerStates;

ConfigSettingsState.Init();
Screen.Init();
InfluxDBClientState.Init();
await GameState.InitAsync();

//var a = 1;
//while (true)
//{
//    //a += 1;
//    //Screen.AddLog($"h {a}");
//    Thread.Sleep(1000);
//}
Console.ReadLine();