using Newtonsoft.Json;
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
        private string _historyFilesLocation;
        private string _badFilesPath;
        private string _totalChangesTextPath;
        private string _badFilesErrorsPath;
        private string _goodFilesPath;
        private bool _isWriting = false;
        private long _totalChanges = 0;
        private long _originalTotalChanges = 0;
        private int _fileProcessedCount = 0;
        private ConcurrentBag<long> _totalChangesToBeWritten = new();
        private ConcurrentBag<string> _linesToBeWrittenGood = new();
        private ConcurrentBag<string> _linesToBeWrittenBad = new();
        private ConcurrentBag<string> _linesToBeWrittenBadErrors = new();
        private IEnumerable<string> _files;

        public HistoryFolderClass(string basePath)
        {
            _historyFilesLocation = Path.Combine(basePath, "History");
            _badFilesPath = Path.Combine(basePath, "bad.txt");
            _totalChangesTextPath = Path.Combine(basePath, "totalChanges.txt");
            _badFilesErrorsPath = Path.Combine(basePath, "badErrors.txt");
            _goodFilesPath = Path.Combine(basePath, "good.txt");

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

            HashSet<string> goodFilesText = new HashSet<string>(File.ReadLines(_goodFilesPath));
            HashSet<string> badFilesText = new HashSet<string>(File.ReadLines(_badFilesPath));

            _fileProcessedCount = 0;
            _originalTotalChanges = Convert.ToInt64(File.ReadAllText(_totalChangesTextPath));
            _totalChanges = Convert.ToInt64(_originalTotalChanges);

            _files = Directory.EnumerateFiles(_historyFilesLocation)
                .Concat(Directory.GetDirectories(_historyFilesLocation)
                    .SelectMany(subdir => Directory.EnumerateFiles(subdir)))
                .OrderBy(File.GetCreationTimeUtc)
                .Where(file =>
                (HistoryConfigSettingsState.InluceGoodFiles && goodFilesText.Contains(file))
                || (HistoryConfigSettingsState.IncludeBadFiles && badFilesText.Contains(file))
                || (HistoryConfigSettingsState.IncludeUnknownFiles && !goodFilesText.Contains(file) && !badFilesText.Contains(file)))
                .ToList();

            var filesCount = _files.Count();
            Console.WriteLine($"Found {filesCount} files to parse, started at {DateTime.Now.ToLongTimeString()}");

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


            _fileProcessedCount += _totalChangesToBeWritten.Count + _linesToBeWrittenGood.Count;
            using StreamWriter goodWriter = new StreamWriter(_goodFilesPath, true);
            while (_linesToBeWrittenGood.TryTake(out var file))
            {
                goodWriter.WriteLine(file);
            }
            var goodWriteTime = writeStopwatch.Elapsed;

            using StreamWriter badWriter = new StreamWriter(_badFilesPath, true);
            while (_linesToBeWrittenBad.TryTake(out var file))
            {
                badWriter.WriteLine(file);
            }
            var badWriteTime = writeStopwatch.Elapsed;

            using StreamWriter badErrorsWriter = new StreamWriter(_badFilesErrorsPath, true);
            while (_linesToBeWrittenBadErrors.TryTake(out var error))
            {
                badErrorsWriter.WriteLine(error);
            }
            var badErrorWriteTime = writeStopwatch.Elapsed;

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
            Console.WriteLine($"Write times: Good {goodWriteTime.TotalMilliseconds} ms, Bad {badWriteTime.TotalMilliseconds - goodWriteTime.TotalMilliseconds} ms, Bad Errors {badErrorWriteTime.TotalMilliseconds - badWriteTime.TotalMilliseconds} ms, TotalLines {totalWriteTime.TotalMilliseconds - badErrorWriteTime.TotalMilliseconds} ms at {DateTime.Now.ToLongTimeString()}");
            Console.WriteLine($"Changes processed {_totalChanges - _originalTotalChanges} ({changesProcessedThisSync}) in {_fileProcessedCount} ({filesChangesProcessedThisSync}) files (out of {_files.Count()} files)");
            _isWriting = false;
        }

        private Task ParseFile(string file)
        {
            try
            {
                var changesProcessed = HistoryFileChecker.ParseFile(file);
                _totalChangesToBeWritten.Add(changesProcessed);
                _linesToBeWrittenGood.Add(file);
            }
            catch (JsonReaderException)
            {

            }
            catch (Exception e)
            {
                _linesToBeWrittenBad.Add(file);
                _linesToBeWrittenBadErrors.Add(e.Message + Environment.NewLine + e.StackTrace);
            }

            return Task.CompletedTask;
        }

        public async Task Start()
        {
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
                case "paralelFor":
                    Parallel.For(0, _files.Count(), i => ParseFile(_files.ElementAt(i)));
                    break;
                case "paralelForEach":
                    //Parallel.ForEach(files, Execute);
                    break;
                case "semaphore":
                    var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
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
