## Unreleased

### Added

- Added wallpaper themes: `New-BGInfoTheme` and `New-BGInfo -Theme` / `-ThemeMode` with six presets (Graphite, Midnight, Aurora, Ember, Orchid, Corporate) in light and dark. Cmdlets inside a themed script block use the theme for every parameter you leave out, including the `New-BGInfoBackground` backdrop.
- Added layout regions: `-Region Left|Right|Top|Bottom|Center` on charts, topology, visual canvases and widgets, and `New-BGInfoLayout` for margins, gap, column width, band height, taskbar reserve, desktop-icon area, panel height cap and column alignment.
- Added widget cmdlets on ChartForgeX: `New-BGInfoMetric`, `New-BGInfoTable`, `New-BGInfoList`, `New-BGInfoCalendarHeatmap`, `New-BGInfoTimeline` with `New-BGInfoTimelineItem`, `New-BGInfoTreemap`, `New-BGInfoMermaid` (ChartForgeX.Mermaid) and `New-BGInfoQRCode` (CodeGlyphX).
- Added status thresholds: `-WarningThreshold`, `-CriticalThreshold` and `-ThresholdDirection` on `New-BGInfoChart`, `New-BGInfoMetric` and `New-BGInfoVisualCanvasTile` (with `-StatusValue`), and `-Status`, `-Accent` and `-Muted` color roles on `New-BGInfoLabel`.
- Added `Get-BGInfoSystemInfo`, a read-only snapshot of uptime, OS, IPv4, disks, last update, Defender, BitLocker and the signed-in user, and the built-in value tokens `Uptime`, `LastPatch`, `DefenderStatus`, `BitLockerStatus`, `LoggedOnUser`, `Domain` and `DiskSummary`.
- Added `Get-BGInfoMonitor`, which lists each monitor's index, device name, bounds, primary flag and display scale, so per-monitor scripts no longer need DesktopManager types (blocked in Constrained Language Mode). `Examples/Run.BGInfo.PerMonitor.ps1` renders one image per monitor.
- Added `Invoke-BGInfoDataSource`, which runs data-collection code with a time limit and a fallback value.
- Added one example per feature under `Examples/Features`, `Examples/Run.BGInfo.Features.ps1` to render them all, and the fill-in template `Examples/Run.BGInfo.Template.ps1`.
- JSON configurations now carry `Layout`, `Widgets`, regions, region order and thresholds.
- Added `New-BGInfoBackground` and `New-BGInfoBackgroundGlow` for generated backgrounds built from gradients, soft glows, dot or grid patterns, and an optional image with a scrim. A background replaces the wallpaper image and slideshow sources, and round-trips through JSON.
- Added `-CornerRadius` and `-BorderColor` to `New-BGInfoChart`, plus the same panel parameters, header styling and `-Scale` to `New-BGInfoTopology`, so charts, diagrams and visual canvas tiles share one panel style.
- Added four showcase scenarios (server status, help desk, lab or classroom, admin workstation) with a shared HUD theme and `Examples/Run.BGInfo.Showcase.ps1`, which renders all of them in light and dark with a manifest.
- Added eight more showcase scenarios: domain controller, Hyper-V host, NOC and on call, branch office PC, meeting room, build agent, patch day, and kiosk or signage. `Examples/Run.BGInfo.Showcase.ps1` renders all twelve at every `-Size`, plus an ultrawide on-call and a portrait signage render, and its manifest now records the preset, size and PowerBGInfo features of each render.
- `New-BGInfoTreemap -ColorProperty` colors each rectangle by a second measure, for example CPU while the size is memory. With `-WarningThreshold`, `-CriticalThreshold` and `-ThresholdDirection` the colors are the status colors (healthy rectangles muted); without them they shade from a faint to the full accent. Thresholds without `-ColorProperty` compare the sizes. Color values and thresholds round-trip through JSON.
- Timeline items piped as objects or hashtables may carry a `Color`, like `New-BGInfoTimelineItem -Color`.
- Added independent bold and underline controls for labels, values, chart headers, global defaults, and JSON configurations.

### Fixed

- Timeline axes read `Wed 14` or `14:00` instead of ISO dates such as `2026-10-14`.
- Info items in lists, tables and metric cards use the info color instead of the accent, so an Info row under a red or orange accent no longer looks like an alert. List numbers, checks and bullets use one color instead of cycling through the palette.
- A region column whose panels add up to more than its height shrinks them to fit instead of running past the taskbar or off the image.
- The showcase and template scripts are saved as UTF-8 with a byte-order mark, so Windows PowerShell 5.1 reads characters such as `·` and `–` correctly, and their lines fit in 80 columns.

- Repeated runs no longer draw the text again on top of the previous run. PowerBGInfo records each image it writes, with the base it was drawn on, in `PowerBgInfo.outputs.txt` next to the image; when the current wallpaper or a slideshow image is one of those outputs, it draws on the recorded original. An output without a record is not used, and a warning says so.
- `LoggedOnUser` is the user of the session the wallpaper is rendered for, read through `WTSQuerySessionInformation`, instead of the console user. In session 0 (a SYSTEM task) it is the console session's user.
- `-Target LogonScreen` under PowerShell 7 warns on editions other than Enterprise, Education and Server: there the lock screen is set through the policy value `LockScreenImage`, which those editions ignore.
- Section labels without a value span both text columns instead of widening the label column.
- Progress, gauge, circle and pictorial chart tracks stay visible on transparent and panelled charts.
- `New-BGInfoTopology` accepts `-ThemeMode` like charts and visual canvases.
- Honored global value color, font size, and font family defaults when an entry does not provide its own overrides.

### Breaking changes

- Removed production `System.Drawing` usage. Public color values now use ChartForgeX colors and PowerShell color parameters accept names, design tokens, or hex strings.
- Unified topology edge direction values on ChartForgeX's product-neutral `VisualLinkDirection` contract.
- Updated the ChartForgeX dependency to 1.0.0 and removed compatibility with the retired duplicate direction and drawing contracts.

### Changed

- Routed text measurement, image composition, GIF input, chart placement, topology placement, and visual-canvas overlays through reusable ChartForgeX primitives.
- Kept PowerBGInfo as a wallpaper-specific consumer while ChartForgeX owns rendering, typography, color, raster, and topology behavior.
- The showcase scenarios use `-Theme`, `-Region` and thresholds, and start with a data block. `Examples/Showcase/HudTheme.ps1` reads its values from `New-BGInfoTheme`, so scripts that splat it keep working.
- PowerBGInfo now depends on `ChartForgeX.Mermaid` and `CodeGlyphX`. No existing parameter or default changed; every new behavior is opt-in.

### 0.0.5 - 2024.11.19
#### What's new
- Fixes PowerShell 5.1 support

**Full Changelog**: https://github.com/EvotecIT/PowerBGInfo/compare/v0.0.4...v0.0.5

### 0.0.4 - 2024.11.14
#### What's Changed
* Avoid overwrite of original image and add new BuiltInValues by @dejil06 in https://github.com/EvotecIT/PowerBGInfo/pull/12

#### New Contributors
* @dejil06 made their first contribution in https://github.com/EvotecIT/PowerBGInfo/pull/12

**Full Changelog**: https://github.com/EvotecIT/PowerBGInfo/compare/v0.0.3...v0.0.4

### 0.0.3 - 2022.12.31
- Added more built-in values:
  - 'BiosVersion', 'BiosManufacturer', 'BiosReleaseDate'
  - 'OSName', 'OSVersion', 'OSArchitecture', 'OSBuild', 'OSInstallDate', 'OSLastBootUpTime'
- Added ability to rename Label when using BuiltinValues

### 0.0.2 - 2022.12.31
- Fixes an issue with background not changing when given different file

### 0.0.1
- First release
