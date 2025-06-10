using System.Configuration;
using UserTracker.Tests.RoomHistory;
using UserTrackerShared.Helpers;

namespace UserTracker.Tests.Patcher
{
    public class BatchDynamicPatcherTests
    {
        public static IEnumerable<object[]> FilesData()
        {
            var map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                   "App.Live.config")
            };
            var cfg = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            var historyFilesLocation = cfg.AppSettings.Settings["HISTORY_BASE_PATH"]?.Value;
            if (string.IsNullOrEmpty(historyFilesLocation)) throw new Exception("Missing base path");

            int.TryParse(cfg.AppSettings.Settings["MAX_FILES"]?.Value, out int maxFiles);
            var files = Directory.EnumerateFiles(historyFilesLocation)
                .Concat(Directory.GetDirectories(historyFilesLocation)
                    .SelectMany(subdir => Directory.EnumerateFiles(subdir)))
                .OrderBy(File.GetCreationTimeUtc)
                .ToList();

            return files.Take(maxFiles).Select(file => new[] { file });
        }


        [Theory]
        [MemberData(nameof(FilesData), MemberType = typeof(BatchDynamicPatcherTests))]
        public void FilesData_RunWithoutFail(string path)
        {
            HistoryFileChecker.ParseFile(path);
        }
    }
}
