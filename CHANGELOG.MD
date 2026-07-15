## Unreleased

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
