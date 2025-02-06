using System.Collections.Concurrent;
using System.Diagnostics;
using UserTracker.Tests.RoomHistory;

string HistoryFilesLocations = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\Histories";
var files = Directory.EnumerateFiles(HistoryFilesLocations).Take(1000).ToList();
Console.WriteLine($"Found {files.Count} files to parse");

Task.Run(static () =>
{
    while (true)
    {
        GC.Collect();  // Force garbage collection
        GC.WaitForPendingFinalizers(); // Wait for finalizers to complete
        Thread.Sleep(5000); // Wait for 1 second
    }
});


// Increase the maximum threads in the thread pool (if needed)
int maxThreads = Environment.ProcessorCount * 2; // or any other value
//ThreadPool.SetMinThreads(maxThreads, maxThreads);  // Set minimum thread pool size to match maxThreads
//ThreadPool.SetMaxThreads(maxThreads * 40, maxThreads * 40); // Set maximum threads in the thread pool


var stopwatch = new Stopwatch();
stopwatch.Start();

foreach (var file in files)
{
    Console.WriteLine("Started");
    HistoryFileChecker.ParseFile(file);
}

stopwatch.Stop();
TimeSpan elapsedTime = stopwatch.Elapsed;
Console.WriteLine($"Elapsed Time: {elapsedTime.TotalMilliseconds} ms");