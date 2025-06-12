using System.Collections.Concurrent;
using System.Diagnostics;
using System.Timers;
using UserTracker.Tests.RoomHistory;
using UserTrackerShared.Helpers;
using Timer = System.Timers.Timer;

namespace UserTracker.HistoryFileTesterConsole
{
    public class HistoryFolderClass
    {
        private readonly string _historyFilesLocation;
        private readonly string _badFilesPath;
        private readonly string _totalChangesTextPath;
        private readonly string _badFilesErrorsPath;
        private readonly string _seenPropertiesPath;
        private readonly string _goodFilesPath;

        private bool _isWriting = false;
        private readonly long _originalTotalChanges = 0;
        private long _totalChanges = 0;
        private int _fileProcessedCount = 0;

        private readonly ConcurrentBag<long> _totalChangesToBeWritten = new();
        private readonly ConcurrentBag<string> _linesToBeWrittenGood = new();
        private readonly ConcurrentBag<string> _linesToBeWrittenBad = new();
        private readonly IEnumerable<string> _files;

        private readonly HashSet<string> _goodFiles = new HashSet<string>();
        private readonly HashSet<string> _badFiles = new HashSet<string>();
        private readonly ConcurrentDictionary<string, int> _badFileErrorCounts = new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<string, long> _seenPropertiesDict = new ConcurrentDictionary<string, long>();

        private void LoadExistingErrorCounts(string path)
        {
            if (!File.Exists(path)) return;

            var blocks = File.ReadAllText(path)
                             .Split(new[] { Environment.NewLine + Environment.NewLine },
                                    StringSplitOptions.RemoveEmptyEntries);

            foreach (var block in blocks)
            {
                var lines = block.Split(new[] { Environment.NewLine },
                                        StringSplitOptions.None);
                if (lines.Length < 2) continue;  // must have at least one error line + count

                var countLine = lines[^1];
                if (!countLine.StartsWith("Count:")) continue;  // skip malformed
                if (!int.TryParse(countLine.Substring("Count:".Length).Trim(), out int count))
                    continue;

                var errorText = string.Join(Environment.NewLine, lines.Take(lines.Length - 1));

                _badFileErrorCounts[errorText] = count;
            }
        }

        public HistoryFolderClass(string basePath, string readFolder)
        {
            _badFilesPath = Path.Combine(basePath, "bad.txt");
            _totalChangesTextPath = Path.Combine(basePath, "totalChanges.txt");
            _seenPropertiesPath = Path.Combine(basePath, "seenProperties.txt");
            _badFilesErrorsPath = Path.Combine(basePath, "badErrors.txt");

            _historyFilesLocation = Path.Combine(readFolder, "History");
            _goodFilesPath = Path.Combine(readFolder, "good.txt");

            if (!Directory.Exists(_historyFilesLocation))
                Directory.CreateDirectory(_historyFilesLocation);

            if (!File.Exists(_totalChangesTextPath))
                File.WriteAllText(_totalChangesTextPath, "0");
            if (!File.Exists(_badFilesPath))
                File.Create(_badFilesPath).Close();
            if (!File.Exists(_badFilesErrorsPath))
                File.Create(_badFilesErrorsPath).Close();
            if (!File.Exists(_goodFilesPath))
                File.Create(_goodFilesPath).Close();
            if (!File.Exists(_seenPropertiesPath))
                File.Create(_seenPropertiesPath).Close();

            _goodFiles = new HashSet<string>(File.ReadLines(_goodFilesPath));
            _badFiles = new HashSet<string>(File.ReadLines(_badFilesPath)).Where(f => !_goodFiles.Contains(f)).ToHashSet();
            File.WriteAllLines(_badFilesPath, _badFiles);

            LoadExistingErrorCounts(_badFilesErrorsPath);
            var seenPropertiesLines = File.ReadLines(_seenPropertiesPath);
            foreach (var line in seenPropertiesLines)
            {
                var keyValueSplit = line.Split(" : ");
                var key = keyValueSplit[0].Trim();
                var value = int.Parse(keyValueSplit[1].Trim());
                _seenPropertiesDict[key] = value;
            }


            _fileProcessedCount = 0;
            _originalTotalChanges = Convert.ToInt64(File.ReadAllText(_totalChangesTextPath));
            _totalChanges = _originalTotalChanges;

            _files = Directory.EnumerateFiles(_historyFilesLocation)
                .Concat(Directory.GetDirectories(_historyFilesLocation)
                    .SelectMany(subdir => Directory.EnumerateFiles(subdir)))
                .OrderBy(File.GetCreationTimeUtc)
                .Where(file =>
                (HistoryConfigSettingsState.InluceGoodFiles && _goodFiles.Contains(file))
                || (HistoryConfigSettingsState.IncludeBadFiles && _badFiles.Contains(file))
                || (HistoryConfigSettingsState.IncludeUnknownFiles && !_goodFiles.Contains(file) && !_badFiles.Contains(file)))
                .ToList();

            var filesCount = _files.Count();
            Console.WriteLine($"Found {filesCount} files to parse in {readFolder}, started at {DateTime.Now.ToLongTimeString()}");

            Timer? onSave = new Timer(60 * 1000);
            onSave.Elapsed += OnSaveTimer;
            onSave.AutoReset = true;
            onSave.Enabled = true;
        }

        private void OnSaveTimer(Object? source, ElapsedEventArgs? e)
        {
            if (_isWriting || _totalChangesToBeWritten.Count + _linesToBeWrittenGood.Count + _linesToBeWrittenBad.Count < 1) return;
            _isWriting = true;
            var writeStopwatch = new Stopwatch();
            writeStopwatch.Start();


            var newGoodFilesCount = _linesToBeWrittenGood.Count;
            var badFilesCount = _linesToBeWrittenBad.Count;
            _fileProcessedCount += newGoodFilesCount + badFilesCount;

            using StreamWriter goodWriter = new StreamWriter(_goodFilesPath, true);
            while (_linesToBeWrittenGood.TryTake(out var file))
            {
                if (_goodFiles.Contains(file)) continue;
                goodWriter.WriteLine(file);
            }
            var goodWriteTime = writeStopwatch.Elapsed;

            using StreamWriter badWriter = new StreamWriter(_badFilesPath, true);
            while (_linesToBeWrittenBad.TryTake(out var file))
            {
                if (_badFiles.Contains(file)) continue;
                badWriter.WriteLine(file);
            }
            var badWriteTime = writeStopwatch.Elapsed;

            using (var writer = new StreamWriter(_badFilesErrorsPath, append: false))
            {
                var sorted = _badFileErrorCounts
                             .OrderByDescending(kvp => kvp.Value)
                             .ThenBy(kvp => kvp.Key, StringComparer.Ordinal);

                foreach (var kv in sorted)
                {
                    writer.WriteLine(kv.Key);
                    writer.WriteLine($"Count: {kv.Value}");
                    writer.WriteLine();
                    writer.WriteLine();
                }
            }
            var badErrorWriteTime = writeStopwatch.Elapsed;

            if (_seenPropertiesDict.Count > 0)
            {
                var sortedProperties = _seenPropertiesDict.OrderBy(p => p.Key);
                using StreamWriter seenPropertiesWriter = new StreamWriter(_seenPropertiesPath, false);
                int maxPropertyNameLength = sortedProperties.Max(p => p.Key.Length);
                foreach (var property in sortedProperties)
                {
                    var formattedLine = $"{property.Key.PadRight(maxPropertyNameLength)} : {property.Value}";
                    seenPropertiesWriter.WriteLine(formattedLine);
                }
            }
            var seenPropertiesWriteTime = writeStopwatch.Elapsed;

            long changesProcessedThisSync = 0;
            long filesChangesProcessedThisSync = 0;
            while (_totalChangesToBeWritten.TryTake(out var total))
            {
                filesChangesProcessedThisSync += 1;
                changesProcessedThisSync += total;

                _totalChanges += total;
            }
            File.WriteAllText(_totalChangesTextPath, Convert.ToString(_totalChanges));
            var totalWriteTime = writeStopwatch.Elapsed;

            writeStopwatch.Stop();
            Console.WriteLine();
            Console.WriteLine($"//====== {DateTime.Now.ToLongTimeString()}");
            Console.WriteLine($"Writing files took {Math.Round(seenPropertiesWriteTime.TotalMilliseconds, 2)}ms");
            Console.WriteLine($"Changes processed {changesProcessedThisSync} in {newGoodFilesCount} good files, {badFilesCount} new bad files");
            Console.WriteLine($"total seen properties {_seenPropertiesDict.Count}");
            Console.WriteLine($"Progress {_fileProcessedCount}/{_files.Count()} files, total changes now {_totalChanges}");
            _isWriting = false;
        }

        private Task ParseFile(string file)
        {
            try
            {
                var (changesProcessed, seenProperties) = HistoryFileChecker.ParseFile(file);
                _totalChangesToBeWritten.Add(changesProcessed);
                foreach (var property in seenProperties)
                {
                    _seenPropertiesDict.AddOrUpdate(
                        property.Key,
                        addValue: property.Value,
                        updateValueFactory: (key, oldValue) => oldValue + property.Value
                    );
                }
                _linesToBeWrittenGood.Add(file);
            }
            catch (Exception e)
            {
                _linesToBeWrittenBad.Add(file);
                _badFileErrorCounts.AddOrUpdate(
                   e.Message + Environment.NewLine + e.StackTrace,
                   addValue: 1,
                   updateValueFactory: (key, oldValue) => oldValue + 1
                );
                if (HistoryConfigSettingsState.ThrowOnBadFile)
                {
                    Console.WriteLine($"Error processing file {file}: {e.Message}");
                    throw;
                }
            }

            return Task.CompletedTask;
        }

        public async Task Start()
        {
            var processorCount = Environment.ProcessorCount * 2;
            switch (HistoryConfigSettingsState.LoopStrategy)
            {
                case "for":
                    for (int i = 0; i < _files.Count(); i++)
                    {
                        await ParseFile(_files.ElementAt(i));
                    }
                    break;
                case "forEach":
                    foreach (var file in _files)
                    {
                        await ParseFile(file);
                    }
                    break;
                case "parallelFor":
                    Parallel.For(0, _files.Count(), i => ParseFile(_files.ElementAt(i)));
                    break;
                case "parallelForEach":
                    var filesList = _files.ToList();
                    var parallelOptions = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = processorCount
                    };

                    await Parallel.ForEachAsync(filesList, parallelOptions, async (file, ct) =>
                    {
                        await ParseFile(file);
                    });
                    break;
                case "semaphore":
                    var semaphore = new SemaphoreSlim(processorCount);
                    var tasks = new List<Task>();
                    foreach (var file in _files)
                    {
                        await semaphore.WaitAsync();

                        tasks.Add(
                            Task.Run(() =>
                            {
                                try
                                {
                                    ParseFile(file);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            })
                        );
                    }
                    Console.WriteLine($"Waiting for tasks to finish, started {tasks.Count} tasks");
                    await Task.WhenAll(tasks);
                    break;
                default:
                    break;
            }

            OnSaveTimer(null, null);
        }
    }
}
