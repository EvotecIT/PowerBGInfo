## Unreleased

### Added

- Added `New-BGInfoBackground` and `New-BGInfoBackgroundGlow` for generated backgrounds built from gradients, soft glows, dot or grid patterns, and an optional image with a scrim. A background replaces the wallpaper image and slideshow sources, and round-trips through JSON.
- Added `-CornerRadius` and `-BorderColor` to `New-BGInfoChart`, plus the same panel parameters, header styling and `-Scale` to `New-BGInfoTopology`, so charts, diagrams and visual canvas tiles share one panel style.
- Added four showcase scenarios (server status, help desk, lab or classroom, admin workstation) with a shared HUD theme and `Examples/Run.BGInfo.Showcase.ps1`, which renders all of them in light and dark with a manifest.
- Added independent bold and underline controls for labels, values, chart headers, global defaults, and JSON configurations.

### Fixed

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
