using UserTrackerShared;
using UserTrackerShared.States;
using UserTrackerStates;

Screen.Init();
InfluxDBClientState.Init();
var gameState = new GameState();
await gameState.InitAsync();

//var a = 1;
//while (true)
//{
//    //a += 1;
//    //Screen.AddLog($"h {a}");
//    Thread.Sleep(1000);
//}
Console.ReadLine();