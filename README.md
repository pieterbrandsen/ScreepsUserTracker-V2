# ScreepsUserTracker

ScreepsUserTracker is a tool designed to track and analyze user activity in the game Screeps. It collects data from various sources, processes it, and provides insights into player behavior, performance, and trends over time.

## History windows

`TICKS_IN_FILE` must match the number of ticks in each history file produced by the server. It controls the history request step and sync boundary. `TICKS_IN_OBJECT` controls the number of ticks averaged into each room, user, and global row.

Both settings must be positive. The output window must be either a multiple or an exact divisor of the file size; other combinations fail at startup.

| TICKS_IN_FILE | TICKS_IN_OBJECT | Output |
| --- | --- | --- |
| 100 | 10 | Ten separate 10-tick windows per file |
| 100 | 100 | One window per file |
| 100 | 1000 | One window after ten files |
| 100 | 30 or 150 | Invalid: windows do not align with files |

Each room's history file is fetched once, even when it contains multiple output windows. History state carries across those windows, and a multi-file window is retained between sync runs until its end is reached. Initial lookback rounds down to a boundary shared by files and output windows so the first window is complete.

The row's `tick` identifies the start of its window. Timestamps remain the source history-file timestamps (the final file for a multi-file window). Sub-file windows therefore share a timestamp; use `tick` to distinguish them. The existing 500-tick safety delay still applies, and finer output windows do not make history files available sooner.

## QuestDB detail

Add this optional setting to the server's app config (for Panda, `UserTrackerConsole/App.PandaServer.config`) and rebuild/restart that tracker:

```xml
<add key="QUESTDB_DETAILED_ENABLED" value="true" />
```

It defaults to `false` when absent. With detail enabled, room, user, and global history retain their existing summary columns and also include every numeric metric in the parsed structures, creep groups, and ground resources. Paths become lowercase columns with underscores, for example:

- `structures_rampart_hits`, `structures_extension_energycapacity`
- `structures_constructionsite_progress`, `structures_constructionsite_typesbuilding_spawn`
- `structures_storage_store_xgh2o`, `structures_terminal_store_xuh2o`
- `creeps_ownedcreeps_actionlog_harvest_inflow`, `creeps_enemycreeps_actionlog_attack_damage`
- `groundresources_energy`

Detailed `storetotals_*` covers all 84 resources currently declared by `Store`, including boost compounds and commodities. Totals cover storage, terminal, containers, and link energy; creep stores are exported separately under `creeps_*_store_*`. All detailed store-total resources are emitted, including zero stock. Compact mode retains the original 12-resource selection. `storetotal` sums the selected resources, so its coverage increases with detail enabled.

Summary metrics preserve fractional window averages, including rare intents and controller levels. All numeric history columns continue to be written as QuestDB doubles. Detailed columns are added by ingestion; disabling detail omits them from future rows without removing existing columns. Earlier rows are not retroactively enriched. Individual detailed store fields and dynamic dictionaries can be absent; a null in an older or compact row is not proof of zero stock.

To retain individual ticks, set `TICKS_IN_OBJECT=1` while keeping `TICKS_IN_FILE` at the server's actual file size. This works independently of detailed mode and creates 100 times as many rows as a 100-tick window. It still has the history publication/safety delay. A window mean cannot establish consecutive-tick behavior. Structure hit values are means of room-wide sums, not the minimum HP of an individual structure, and action amounts are the parser's estimates from body parts and boosts.

See [the September 2026 investigation](docs/tracker-data-2026-09-16.md) for the throughput diagnosis, validation, and measurement limits.
