using UserTracker.HistoryFileTesterConsole;
using UserTrackerShared.DBClients;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;

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

while (true)
{
    foreach (var folder in Directory.GetDirectories(baseDirectory))
    {
        Console.WriteLine($"Checking {folder}");
        if (HasHistoryFilesLocation(folder))
        {
            var historyFolder = new HistoryFolderClass(baseDirectory, folder);
            await historyFolder.Start();
        }
    }

    Console.WriteLine("All folders checked. Waiting for next cycle...");
    Console.WriteLine("Waiting for 30 minutes before next check...");
    await Task.Delay(TimeSpan.FromMinutes(30));
}