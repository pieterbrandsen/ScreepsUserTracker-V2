using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;
using Xunit;

namespace UserTracker.Tests.Helpers;

public class FileWriterManagerTests
{
    [Fact]
    public void GenerateHistoryFile_AddsEntryToCacheOnlyOnce()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "UserTrackerTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempRoot);
        ConfigSettingsState.ObjectsFolder = tempRoot;

        var historyCache = (ConcurrentDictionary<string, JObject>)typeof(FileWriterManager)
            .GetField("HistoryCache", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;
        historyCache.Clear();

        var roomData = new JObject
        {
            ["room"] = "E1S1",
            ["base"] = 123
        };

        FileWriterManager.GenerateHistoryFile(roomData);
        Assert.Single(historyCache);
        var key = historyCache.Keys.Single();
        Assert.Equal("123/E1S1", key);

        FileWriterManager.GenerateHistoryFile(roomData);
        Assert.Single(historyCache);

        try
        {
            Directory.Delete(tempRoot, true);
        }
        catch
        {
        }
        historyCache.Clear();
        ConfigSettingsState.ObjectsFolder = string.Empty;
    }
}
