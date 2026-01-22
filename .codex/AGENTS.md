# AGENTS

## Scope
- This is a .NET solution targeting net10.0; use a .NET 10 SDK.
- Main app: `UserTrackerConsole`; shared code: `UserTrackerShared`.
- Helper app: `UserTracker.HistoryFileTesterConsole`.
- Tests live in `UserTracker.Tests` and `UserTracker.Tests.Shared` (xUnit).

## Build and test
- Build the solution: `dotnet build UserTrackerService.sln`
- Run tests: `dotnet test UserTracker.Tests/UserTracker.Tests.csproj`

## Running
- Main console app: `dotnet run --project UserTrackerConsole`
- History tester: `dotnet run --project UserTracker.HistoryFileTesterConsole`
- Config selection: set `APP_CONFIG_FILE` to a config file in the project folder.
  Example (PowerShell): `$env:APP_CONFIG_FILE='App.mmo.config'; dotnet run --project UserTrackerConsole`

## Configuration
- Uses `App*.config` XML files (not `appsettings.json`).
- Some configs contain credentials; avoid committing new secrets and prefer local overrides.
- Tests rely on `UserTracker.Tests/App.Live.config`; update local paths as needed.

## Local services
- `ComposeProject/` contains docker-compose files for local data stores.
  Start with: `docker compose -f ComposeProject/docker-compose.yml up`

## Repo hygiene
- Do not edit or commit generated `bin/` or `obj/` artifacts.
