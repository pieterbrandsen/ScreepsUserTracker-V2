using UserTracker.HistoryFileTesterConsole;
using UserTrackerShared.DBClients;
using UserTrackerShared.Helpers;
using UserTrackerShared.States;

ConfigSettingsState.Init();
HistoryConfigSettingsState.Init();
DBClient.Init();

var baseDirectory = HistoryConfigSettingsState.HistoryBasePath;
ZipPartHandler.Initialize(baseDirectory);

var pendingParts = ZipPartHandler.GetPendingParts();
foreach (var part in pendingParts)
{
   await ZipPartHandler.Handle(part);
}
Console.ReadLine();