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
