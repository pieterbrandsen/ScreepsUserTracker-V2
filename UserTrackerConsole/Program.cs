using UserTrackerConsole;
using UserTrackerShared.States;

var screen = new Screen("Main");

GameState.Init();

var a = 1;
while (true)
{
    a += 1;
    screen.LogsPart.AddLog($"h {a}");
    Thread.Sleep(1000);
}

Console.ReadLine();
