using UserTracker.HistoryFileTesterConsole;
using UserTrackerShared.Helpers;
using UserTrackerStates.DBClients;

ConfigSettingsState.Init();
HistoryConfigSettingsState.Init();
DBClient.Init();

static bool HasHistoryFilesLocation(string basePath)
{
    string historyFilesLocation = Path.Combine(basePath, "History");
    return Directory.Exists(historyFilesLocation);
}

var baseDirectory = HistoryConfigSettingsState.HistoryBasePath;
if (HasHistoryFilesLocation(baseDirectory))
{
    var historyFolder = new HistoryFolderClass(baseDirectory, baseDirectory);
    await historyFolder.Start();
}

foreach (var folder in Directory.GetDirectories(baseDirectory))
{
    Console.WriteLine($"Checking {folder}");
    if (HasHistoryFilesLocation(folder))
    {
        var historyFolder = new HistoryFolderClass(baseDirectory, folder);
        await historyFolder.Start();
    }
}

while (true)
{
    foreach (var folder in Directory.GetDirectories(baseDirectory))
    {
        Console.WriteLine($"Checking {folder}");
        if (HasHistoryFilesLocation(folder))
        {
            var historyFolder = new HistoryFolderClass(baseDirectory,folder);
            await historyFolder.Start();
        }
    }

    await Task.Delay(TimeSpan.FromMinutes(30));
}