using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Timers;
using UserTracker.Tests.RoomHistory;
using UserTrackerShared.Helpers;
using UserTrackerStates;
using Timer = System.Timers.Timer;

ConfigSettingsState.Init();
HistoryConfigSettingsState.Init();
InfluxDBClientState.Init();


string HistoryFilesLocations = @$"{HistoryConfigSettingsState.HistoryFilesLocation}";
string BasePath = @"C:\Users\Pieter\source\repos\ScreepsUserTracker-V2\UserTrackerConsole\Objects";
string BadFilesPath = @$"{BasePath}\bad.txt";
string TotalChangesTextPath = @$"{BasePath}\totalChanges.txt";
string BadFilesErrorsPath = @$"{BasePath}\badErrors.txt";
string GoodFilesPath = @$"{BasePath}\good.txt";
if (!File.Exists(TotalChangesTextPath))
{
    File.WriteAllText(TotalChangesTextPath, "0");
}
if (!File.Exists(BadFilesPath))
{
    File.Create(BadFilesPath).Close();
}
if (!File.Exists(BadFilesErrorsPath))
{
    File.Create(BadFilesErrorsPath).Close();
}
if (!File.Exists(GoodFilesPath))
{
    File.Create(GoodFilesPath).Close();
}

HashSet<string> goodFilesText = new HashSet<string>(File.ReadLines(GoodFilesPath));
HashSet<string> badFilesText = new HashSet<string>(File.ReadLines(BadFilesPath));

var fileProcessedCount = 0;
long originalTotalChanges = Convert.ToInt64(File.ReadAllText(TotalChangesTextPath));
long totalChanges = Convert.ToInt64(originalTotalChanges);

var files = Directory.EnumerateFiles(HistoryFilesLocations)
    .Concat(Directory.GetDirectories(HistoryFilesLocations)
        .SelectMany(subdir => Directory.EnumerateFiles(subdir)))
    .OrderBy(File.GetCreationTimeUtc)
    .Where(file => goodFilesText.Contains(file) == HistoryConfigSettingsState.InluceGoodFiles)
    .Where(file => badFilesText.Contains(file) == HistoryConfigSettingsState.IncludeBadFiles)
    .Where(file => (goodFilesText.Contains(file) && badFilesText.Contains(file)) != HistoryConfigSettingsState.IncludeUnknownFiles)
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
        long changesProcessedThisSync = 0;
        long filesChangesProcessedThisSync = 0;
        while (totalChangesToBeWritten.TryTake(out var total))
        {
            filesChangesProcessedThisSync += 1;
            changesProcessedThisSync += total;

            totalChanges += total;
        }
        File.WriteAllText(TotalChangesTextPath, Convert.ToString(totalChanges));
        Console.WriteLine($"Changes processed {totalChanges - originalTotalChanges} ({changesProcessedThisSync}) in {fileProcessedCount} ({filesChangesProcessedThisSync}) files");
    }
}

Timer? onSave = new Timer(30 * 1000);
onSave.Elapsed += OnSaveTimer;
onSave.AutoReset = true;
onSave.Enabled = true; ;


var stopwatch = new Stopwatch();
stopwatch.Start();

void Execute(string file)
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
}

switch (HistoryConfigSettingsState.LoopStrategy)
{
    case "for":
        for (int i = 0; i < files.Count; i++)
        {
            Execute(files[i]);
        }
        break;
    case "forEach":
        foreach (var file in files)
        {
            Execute(file);
        }
        break;
    case "paralelFor":
        Parallel.For(0, files.Count, i => Execute(files[i]));
        break;
    case "paralelForEach":
        Parallel.ForEach(files, Execute);
        break;
    case "semaphore":
        var semaphore = new SemaphoreSlim(1000);
        var tasks = new List<Task>();
        foreach (var file in files)
        {
            await semaphore.WaitAsync();

            tasks.Add(
                Task.Run(() =>
                {
                    try
                    {
                        Execute(file);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
            );
        }
        break;
    default:
        break;
}

stopwatch.Stop();
TimeSpan elapsedTime = stopwatch.Elapsed;
Console.WriteLine($"Elapsed Time: {elapsedTime.TotalMilliseconds} ms");
OnSaveTimer(null, null);