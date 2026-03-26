# Package Upgrade

## Strategy
All-at-Once — all 6 projects updated simultaneously, tasks split by risk level.

## Preferences
- **Flow Mode**: Automatic
- **Commit Strategy**: After Each Task
- **Target Framework**: net10.0 (unchanged — user chose to stay on .NET 10)
- **Paid-license packages**: Skip any paid/commercial-license package (none found in this solution)
- **TFM**: Do NOT change TargetFramework in any project file

## Decisions
- No TFM upgrade — only available target was .NET 11 Preview; user declined
- Update all packages to latest compatible with net10.0
- All 38 packages are open-source (MIT/Apache/BSD) — no packages skipped for licensing
