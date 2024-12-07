using UserTrackerConsole;

var screen = new Screen("Main");

var a = 1;
while (true)
{
    a += 1;
    screen.LogsPart.AddLog($"h {a}");
    Thread.Sleep(10);
}

Console.ReadLine();
