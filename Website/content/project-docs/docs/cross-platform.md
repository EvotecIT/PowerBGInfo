---
title: Cross-platform rendering and desktops
description: Generate wallpapers on Windows, macOS and Linux, and inspect supported desktop operations.
---

# Cross-platform rendering and desktops

PowerBGInfo separates collecting facts, rendering an image, and applying it to
a desktop. `-Target File` generates an image without reading or changing the
current wallpaper. It also works on a server or CI host without a graphical
session. Supply background dimensions when no monitor geometry is available.

```powershell
$facts = Get-BGInfoSystemInfo
New-BGInfo -Target File -Theme Graphite -ConfigurationDirectory ([IO.Path]::GetTempPath()) -OutputFileName 'machine.png' {
    New-BGInfoBackground -Width 1920 -Height 1080
    New-BGInfoLabel -Name $facts.ComputerName -FontSize 56
    New-BGInfoValue -Name 'OS' -Value $facts.OSName
    New-BGInfoValue -Name 'Uptime' -Value $facts.UptimeText
    $facts.Disks | New-BGInfoTable -Region Right -Title 'Volumes' -Property Drive, FreeGB, UsedPercent
}
```

`Examples/Run.BGInfo.CrossPlatform.ps1` provides a parameterized version. It
renders a file by default; select `-Target Wallpaper` to apply it deliberately.
The CLI accepts the same JSON configuration on every platform and provides
`--doctor` for desktop diagnostics. `--no-apply` and `Invoke-BGInfo -NoApply`
render file previews even when a saved Windows configuration retains all-user
options; those options are inactive for the file target.

## Desktop capabilities

Run `Get-BGInfoCapability` before selecting a desktop target. A successful image
render does not establish that a desktop accepted or displayed it.

| Backend | Current-user wallpaper | Physical display sizing | Scope |
| --- | --- | --- | --- |
| Windows | Yes, through DesktopManager's existing Windows implementation | Attached display snapshots | Existing lock-screen, all-user, and slideshow operations remain Windows-specific |
| macOS | Public AppKit API through the system JavaScript bridge | AppKit/CoreGraphics | Current user and reported screens; all-Spaces behavior requires separate qualification |
| GNOME | GSettings background settings | Not reported by this adapter | Light/dark settings; use explicit image dimensions |
| KDE Plasma | Plasma scripting through the session D-Bus utility | Not reported by this adapter | Current activity/screens; use explicit image dimensions |
| Other/headless Linux | No desktop adapter | Not reported | File rendering and available system facts |

Desktop application requires the intended user's graphical session and the
platform facilities reported by DesktopManager. A system account, SSH session,
or administrator token is not a substitute for that session. Unsupported
lock-screen or all-user requests fail before rendering or changing wallpaper.
Managed-device policies can still deny an otherwise supported operation.

Display indices describe the current topology. They can change after docking
or reconnecting a screen. The monitor output also carries the backend display
identity. GNOME's ordinary background setting does not select individual
screens; use its desktop target at monitor index zero. KDE uses the addressable
wallpaper targets reported by DesktopManager, even when physical geometry is
unavailable. A changed activity, target identity or known geometry rejects
application of an image generated for the earlier target.

When neither `-FilePath` nor `New-BGInfoBackground` selects a base, PowerBGInfo
uses the effective desktop image and preserves its native fit. An explicitly
bound `-WallpaperFit` takes precedence. Unknown fit or opacity, unreadable
native state and missing geometry required by screen-coordinate layout stop
the run before application. Known disabled-image colors and gradients can be
reproduced when their color syntax and physical target dimensions are available.
Supply an explicit background and dimensions when those metadata are unavailable.

## Facts and history

`Get-BGInfoSystemInfo` preserves its existing properties and adds `FactStates`,
which distinguishes available, unavailable, and unsupported facts. Windows uses
the existing WMI/Registry provider; Linux uses platform files and common .NET
APIs; macOS uses system APIs. Unix volume identities are mount paths, rather
than Windows drive letters.

Windows-specific patch, Defender, and BitLocker facts remain `Unknown` on Unix.
They are not translated into superficially similar platform security states.
`CollectedAt` describes the snapshot time; it does not assert that cached
hardware metadata was probed again at that instant.

When a built-in chart metric cannot be collected, PowerBGInfo retains its saved
history, omits the current chart, and emits a warning. Historic values are not
presented as a new healthy reading. The CLI writes warnings to standard error
while keeping the generated image path on standard output.

## Images, fonts, and portable configuration

CFX supplies dependency-free raster rendering and supported PNG, JPEG, GIF,
BMP, PPM, and TIFF input. Some TIFF variants remain unsupported. An unsupported
desktop background, including HEIC, reports an error; it is never silently
replaced by a plain backdrop. Choose a supported image or a generated background
for portable layouts. A rendered still image does not preserve aerial animation,
dynamic wallpaper schedules or all-Spaces assignments.

With screen-coordinate layout, `Fill` preserves the source aspect ratio and crops
its edges to cover the display; `Stretch` scales the two axes independently.

Fonts are resolved on the rendering host. Install the same licensed font assets
where matching typography is required. Scaling a design does not automatically
reflow its content for a portrait or ultrawide display; use regions and suitable
panel sizes for those layouts.

Relative paths in JSON accept slash or backslash separators. A Windows-rooted
path cannot be used as a local path on macOS/Linux. Use relative paths or
platform-local absolute paths instead. Output ownership on Unix preserves
case-sensitive file identities. Output paths use the same host-aware rules as
input paths, including explicit parent-relative paths.

The output provenance file uses versioned escaped fields so native `#`, pipe and
newline filenames round-trip. Unambiguous older records are migrated on the
next write; malformed or ambiguous records cannot establish an original image.

## Execution and ownership

PowerBGInfo performs one run. Schedule refreshes in the intended user session
using the host's normal scheduling facilities. The CLI's PowerShell authoring
process has a 120-second outer deadline, configurable through
`--script-timeout`; data-source deadlines remain separate.

The portable PowerShell 7 payload requires PowerShell 7.4 or later and uses
`net8.0`; Windows PowerShell 5.1 keeps
the `net472` payload. Windows facts are selected at runtime so the portable
payload preserves Windows WMI functionality. NativeAOT validation generates
files with `--no-apply`; it does not qualify desktop changes. Use PowerShell or
the managed CLI for Windows desktop application, where legacy COM/history paths
remain unqualified under NativeAOT. Package qualification remains distinct from
ordinary managed execution.

The source owners are:

- **PowerBGInfo:** fact providers, bindings, configuration, wallpaper regions,
  health policy, history, output ownership, and run coordination.
- **ChartForgeX:** charts, factual blocks, typography, panels, backgrounds,
  patterns, and image composition.
- **DesktopManager:** display/session discovery, platform wallpaper APIs,
  readback, capabilities, and operation results.
- **PowerForge:** shared build, packaging, generated help, and publishing.

The generator is split into rendering, text, desktop, slideshow, output, and
history responsibilities. PowerShell and the CLI call that same engine.
