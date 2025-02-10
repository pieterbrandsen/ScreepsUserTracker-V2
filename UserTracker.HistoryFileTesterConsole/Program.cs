using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Timers;
using UserTracker.Tests.RoomHistory;
using UserTrackerStates;
using Timer = System.Timers.Timer;

InfluxDBClientState.Init();

//string HistoryFilesLocations = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\History";
string HistoryFilesLocations = @"F:\delete\history2";

string BadFilesPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\bad.txt";
string TotalChangesTextPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\totalChanges.txt";
string BadFilesErrorsPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\badErrors.txt";
string GoodFilesPath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects\good.txt";

HashSet<string> goodFilesText = new HashSet<string>(File.ReadLines(GoodFilesPath));
HashSet<string> badFilesText = new HashSet<string>(File.ReadLines(BadFilesPath));

var fileProcessedCount = 0;
long originalChanges = Convert.ToInt64(File.ReadAllText(TotalChangesTextPath));
long totalChanges = Convert.ToInt64(File.ReadAllText(TotalChangesTextPath));

var files = Directory.EnumerateFiles(HistoryFilesLocations)
    .Concat(Directory.GetDirectories(HistoryFilesLocations)
        .SelectMany(subdir => Directory.EnumerateFiles(subdir)))
    .OrderBy(File.GetCreationTimeUtc)
    .Where(file => !goodFilesText.Contains(file))
    .Where(file => !badFilesText.Contains(file))
    .ToList();


Console.WriteLine($"Found {files.Count} files to parse");

var totalChangesToBeWritten = new ConcurrentBag<long>();
var linesToBeWrittenGood = new ConcurrentBag<string>();
var linesToBeWrittenBad = new ConcurrentBag<string>();
var linesToBeWrittenBadErrors = new ConcurrentBag<string>();

async void OnSaveTimer(Object? source, ElapsedEventArgs e)
{
    fileProcessedCount += linesToBeWrittenGood.Count + linesToBeWrittenBad.Count;
    lock (linesToBeWrittenGood)
    {
        using StreamWriter writer = new StreamWriter(GoodFilesPath, true);
        while (linesToBeWrittenGood.TryTake(out var file))
        {
            writer.WriteLine(file);
        }
    }
    lock (linesToBeWrittenBad)
    {
        using StreamWriter writer = new StreamWriter(BadFilesPath, true);
        while (linesToBeWrittenBad.TryTake(out var file))
        {
            writer.WriteLine(file);
        }
    }
    lock (linesToBeWrittenBadErrors)
    {
        using StreamWriter writer = new StreamWriter(BadFilesErrorsPath, true);
        while (linesToBeWrittenBadErrors.TryTake(out var error))
        {
            writer.WriteLine(error);
        }
    }

    lock (totalChangesToBeWritten)
    {
        while (totalChangesToBeWritten.TryTake(out var total))
        {
            totalChanges += total;
        }
        File.WriteAllText(TotalChangesTextPath, Convert.ToString(totalChanges));
        Console.WriteLine($"Changes processed {totalChanges - originalChanges} in {fileProcessedCount} files");
    }
}

Timer? onSave = new Timer(60 * 1000);
onSave.Elapsed += OnSaveTimer;
onSave.AutoReset = true;
onSave.Enabled = true; ;



var stopwatch = new Stopwatch();
stopwatch.Start();


//foreach (var file in files)
//{
//    try
//    {
//        HistoryFileChecker.ParseFile(file);
//    }
//    catch (JsonReaderException e)
//    {

//    }
//    catch (Exception e)
//    {
//        throw;
//    }
//}
Parallel.ForEach(files, file =>
{
    try
    {
        var changesProcessed = HistoryFileChecker.ParseFile(file);
        totalChangesToBeWritten.Add(changesProcessed);
        linesToBeWrittenGood.Add(file);
    }
    catch (JsonReaderException e)
    {

    }
    catch (Exception e)
    {
        linesToBeWrittenBad.Add(file);
        linesToBeWrittenBadErrors.Add(e.Message);
    }
});

stopwatch.Stop();
TimeSpan elapsedTime = stopwatch.Elapsed;
Console.WriteLine($"Elapsed Time: {elapsedTime.TotalMilliseconds} ms");
OnSaveTimer(null, null);