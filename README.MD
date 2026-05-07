<p align="center">
  <a href="https://dev.azure.com/evotecpl/PowerBGInfo/_build/results?buildId=latest"><img src="https://img.shields.io/azure-devops/build/evotecpl/39c74615-8f34-4af0-a835-68dc33f9214f/14?label=Azure%20Pipelines&style=flat-square"></a>
  <a href="https://github.com/EvotecIT/PowerBGInfo/actions/workflows/test-dotnet.yml"><img src="https://github.com/EvotecIT/PowerBGInfo/actions/workflows/test-dotnet.yml/badge.svg?branch=v2-speedygonzales&style=flat-square"></a>
  <a href="https://codecov.io/gh/EvotecIT/PowerBGInfo"><img src="https://codecov.io/gh/EvotecIT/PowerBGInfo/branch/v2-speedygonzales/graph/badge.svg?style=flat-square"></a>
  <a href="https://www.powershellgallery.com/packages/PowerBGInfo"><img src="https://img.shields.io/powershellgallery/v/PowerBGInfo.svg?style=flat-square"></a>
  <a href="https://www.powershellgallery.com/packages/PowerBGInfo"><img src="https://img.shields.io/powershellgallery/v/PowerBGInfo.svg?label=powershell%20gallery%20preview&colorB=yellow&style=flat-square&include_prereleases"></a>
  <a href="https://github.com/EvotecIT/PowerBGInfo"><img src="https://img.shields.io/github/license/EvotecIT/PowerBGInfo.svg?style=flat-square"></a>
</p>

<p align="center">
  <a href="https://www.powershellgallery.com/packages/PowerBGInfo"><img src="https://img.shields.io/powershellgallery/p/PowerBGInfo.svg?style=flat-square"></a>
  <a href="https://github.com/EvotecIT/PowerBGInfo"><img src="https://img.shields.io/github/languages/top/evotecit/PowerBGInfo.svg?style=flat-square"></a>
  <a href="https://github.com/EvotecIT/PowerBGInfo"><img src="https://img.shields.io/github/languages/code-size/evotecit/PowerBGInfo.svg?style=flat-square"></a>
  <a href="https://www.powershellgallery.com/packages/PowerBGInfo"><img src="https://img.shields.io/powershellgallery/dt/PowerBGInfo.svg?style=flat-square"></a>
</p>

<p align="center">
  <a href="https://twitter.com/PrzemyslawKlys"><img src="https://img.shields.io/twitter/follow/PrzemyslawKlys.svg?label=Twitter%20%40PrzemyslawKlys&style=flat-square&logo=twitter"></a>
  <a href="https://evotec.xyz/hub"><img src="https://img.shields.io/badge/Blog-evotec.xyz-2A6496.svg?style=flat-square"></a>
  <a href="https://www.linkedin.com/in/pklys"><img src="https://img.shields.io/badge/LinkedIn-pklys-0077B5.svg?logo=LinkedIn&style=flat-square"></a>
  <a href="https://evo.yt/discord"><img alt="Discord" src="https://img.shields.io/discord/508328927853281280?style=flat-square&label=discord%20chat"></a>
</p>

**PowerBGInfo** is a Windows background generator with a CLI-first workflow.
Use `PowerBGInfo.Cli` to run either a portable JSON configuration or a PowerShell authoring script, then reuse the same shared renderer for wallpaper, file output, or logon screen generation.

You can read about this project on this [blog post](https://evotec.xyz/powerbginfo-powershell-alternative-to-sysinternals-bginfo/) that tells a little backstory and shows few things.

## Installation

Install the PowerShell module for configuration authoring from [PowerShell Gallery](https://www.powershellgallery.com/packages/PowerBGInfo):

```powershell
Install-Module PowerBGInfo -Force -Verbose
```

Build or publish the CLI from this repository when you want the fastest execution path:

```powershell
dotnet publish .\Sources\PowerBGInfo.Cli\PowerBGInfo.Cli.csproj -c Release -f net8.0-windows -r win-x64 --self-contained false -p:PublishSingleFile=true
```

When there's an update you can update the module using the same `Install-Module` command.

## Known Issues

This module will work fine for PowerShell 5.1 and PowerShell 7+.
**Currently the module has a problem when running in VSCode PowerShell extension when on PowerShell 5.1 (other versions work fine!)**
It works fine when running in PowerShell 5.1 console, or ISE (shrug!).

## Recommended workflow

There are now two first-class ways to run PowerBGInfo from the CLI:

1. `PowerBGInfo.Cli --script ...`
   Best when you want full PowerShell authoring, loops, `Get-Disk`, CIM/WMI, or other custom collection logic.
2. `PowerBGInfo.Cli --config ...`
   Best when you want a portable artifact for packaging, scheduled tasks, RMM tools, or endpoints that should only render/apply a saved configuration.

The usual flow is:

1. Author the layout in PowerShell.
2. Run it directly with `PowerBGInfo.Cli --script ...` while iterating.
3. Optionally export JSON from that same script with `--export-json ...` when you want a reusable JSON snapshot for later runs.

## Quick start

Build the current checkout first so both the PowerShell module and CLI examples use the latest source:

```powershell
dotnet build .\Sources\PowerBGInfo.sln -c Debug
```

Run the script-backed CLI example directly:

```powershell
.\Sources\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe `
  --script .\Examples\Scripts\PowerBGInfo.Cli.Sample.ps1 `
  --no-apply
```

Export JSON from that same script while rendering it:

```powershell
.\Sources\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe `
  --script .\Examples\Scripts\PowerBGInfo.Cli.Sample.ps1 `
  --export-json .\Examples\Configuration\PowerBGInfo.Script.Generated.json `
  --no-apply
```

The repository contains the same flow as a ready-made example in `Examples\Run.BGInfo6.ps1`.

If you want to rerun that generated JSON later without PowerShell logic:

```powershell
.\Sources\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe --config .\Examples\Configuration\PowerBGInfo.Script.Generated.json --no-apply
```

You can still run the same JSON through PowerShell when needed:

```powershell
Invoke-BGInfo -Path .\Examples\Configuration\PowerBGInfo.Generated.json -NoApply
```

## Validate the current checkout

These are the repo-level commands that prove the PowerShell and CLI paths stay in sync:

```powershell
dotnet test .\Sources\PowerBGInfo.sln
Invoke-Pester -Path .\Sources\PowerBGInfo.Tests\Cmdlet.Tests.ps1 -Output Detailed
pwsh -NoLogo -NoProfile -File .\Examples\Run.BGInfo4.ps1
pwsh -NoLogo -NoProfile -File .\Examples\Run.BGInfo6.ps1
```

The last two commands validate both CLI modes:

- PowerShell-authored JSON consumed by the CLI
- PowerShell script consumed directly by the CLI

For a fuller release-proof pass, use the reusable validation script:

```powershell
pwsh -NoLogo -NoProfile -File .\Build\Validate-PowerBGInfo.ps1 -Configuration Release
```

That script verifies:

- solution build and .NET tests
- PowerShell/Pester coverage
- CLI `--script` output
- CLI `--config` output from exported JSON
- `Invoke-BGInfo` output from the same JSON
- published single-file CLI output parity
- NativeAOT CLI smoke output for `Target File` / `--no-apply`

Use `-KeepArtifacts` if you want the rendered files and published executables left on disk for inspection.

## Manual Validation

These scenarios still deserve a real machine check because they change Windows state, may need elevation, or depend on shell behavior:

1. Current-user wallpaper apply:
   Run `New-BGInfo` or `PowerBGInfo.Cli --config ...` without `--no-apply` and confirm the wallpaper changes immediately.
2. All users:
   Run elevated with `-AllUsers`, then confirm both an existing secondary profile and a newly created profile receive the staged wallpaper.
3. Logon screen:
   Run elevated with `-Target LogonScreen` or `-Target Both`, lock/sign out, and confirm the generated image is used.
4. Windows 11 24H2 refresh:
   Run wallpaper apply, sign out/sign back in, and confirm the refresh workaround still prevents Windows from reusing the previous cached wallpaper.
5. Terminal server / RDS:
   Validate in a real user session that per-user values such as `FullUserName` still resolve correctly when run inside the session.

## Script-backed CLI

When you want the EXE to keep full PowerShell flexibility, return a configuration object from your script:

```powershell
Import-Module .\PowerBGInfo.psd1 -Force

New-BGInfo {
    New-BGInfoValue -BuiltinValue HostName
    foreach ($Disk in Get-Disk) {
        foreach ($Volume in ($Disk | Get-Partition | Get-Volume)) {
            New-BGInfoValue -Name "Drive $($Volume.DriveLetter)" -Value $Volume.SizeRemaining
        }
    }
} -FilePath .\Examples\Samples\TapC-Evotec-2560x1080.jpg `
  -ConfigurationDirectory .\Examples\Output `
  -Target File `
  -PassThru
```

Run that script through the CLI:

```powershell
PowerBGInfo.Cli --script .\bginfo.ps1 --no-apply
```

When the script does not import `PowerBGInfo` itself, you can point the CLI at a built module directly:

```powershell
PowerBGInfo.Cli --script .\bginfo.ps1 --module .\PowerBGInfo.PowerShell.dll --no-apply
```

Or export a JSON snapshot from the same script:

```powershell
PowerBGInfo.Cli --script .\bginfo.ps1 --export-json .\bginfo.json --export-only
```

`--script` accepts scripts that return either:

- `[PowerBGInfo.BgInfoConfiguration]` objects, typically via `New-BGInfo -PassThru`
- a path to a JSON file, for example from existing `-JsonPath -ExportOnly` flows

Use direct JSON authoring and export when you need a fully hand-portable config file with carefully controlled relative paths. `--script --export-json` is best for capturing the configuration that the EXE just executed.

## What JSON preserves

`-JsonPath` on `New-BGInfo` preserves the configuration and the dynamic tokens that the shared engine understands:

- builtin values such as `HostName`, `FullUserName`, `OSName`
- chart metrics such as `CpuPercent`, `MemoryPercent`, `DiskFreePercent`
- native variables/providers such as `Volumes`
- loop/template entries via `-ForEach`
- layout, colors, fonts, spacing, target, wallpaper options, and chart settings

What it does not preserve today is arbitrary PowerShell source code. For example, a `foreach ($Disk in Get-Disk) { ... }` block is executed during export, and the produced entries are written to JSON as normal values. The JSON does not currently embed executable user PowerShell.

That means there are two supported patterns today:

- use builtin values and chart metrics when you want the CLI to recalculate data on each run
- use custom PowerShell to compute values once, export the resulting entries, and let the CLI render that snapshot later

There is now a third middle-ground pattern for reusable native loops:

- define a provider-backed variable in PowerShell
- reference it with `-ForEach`
- let the CLI expand it at runtime without PowerShell

Example:

```powershell
New-BGInfo {
    New-BGInfoVariable -Name Volumes -Provider Volumes
    New-BGInfoValue -ForEach Volumes -Name 'Drive {{DriveLetter}}' -Value '{{SizeRemaining}} free ({{FreePercent}}%)'
} -ConfigurationDirectory .\Output -Target File -JsonPath .\bginfo-volumes.json -ExportOnly
```

That exports JSON the CLI can execute directly. See `Examples\Run.BGInfo5.ps1` and `Examples\Configuration\PowerBGInfo.Volumes.json`.

## PowerShell inline usage

You can also render everything inline from PowerShell when you're exploring or authoring a layout:

```powershell
New-BGInfo -MonitorIndex 0 {
    # Lets add computer name, but lets use builtin values for that
    New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20 -FontFamilyName 'Calibri'
    # Lets add user name, but lets use builtin values for that
    New-BGInfoValue -BuiltinValue FullUserName -Name "FullUserName" -Color White
    New-BGInfoValue -BuiltinValue CpuName
    New-BGInfoValue -BuiltinValue CpuLogicalCores
    New-BGInfoValue -BuiltinValue RAMSize
    New-BGInfoValue -BuiltinValue RAMSpeed

    # Lets add Label, but without any values, kind of like section starting
    New-BGInfoLabel -Name "Drives" -Color LemonChiffon -FontSize 16 -FontFamilyName 'Calibri'

    # Lets get all drives and their labels
    foreach ($Disk in (Get-Disk)) {
        $Volumes = $Disk | Get-Partition | Get-Volume
        foreach ($V in $Volumes) {
            New-BGInfoValue -Name "Drive $($V.DriveLetter)" -Value $V.SizeRemaining
        }
    }
} -FilePath $PSScriptRoot\Samples\PrzemyslawKlysAndKulkozaurr.jpg -ConfigurationDirectory $PSScriptRoot\Output -PositionX 100 -PositionY 100 -WallpaperFit Center
```

You can export the same inline authoring flow to JSON for the CLI by adding `-JsonPath` and, when you only want the config file, `-ExportOnly`.
Here's how the wallpaper will look like:

![PowerBGInfo](https://raw.githubusercontent.com/EvotecIT/PowerBGInfo/master/Examples/Output/PrzemyslawKlysAndKulkozaurr.jpg)

## Additional options

- Solid-color desktops: when no wallpaper file exists, PowerBGInfo renders a blank image using the desktop background color or `-BackgroundColor`.
- Refresh behavior: by default the wallpaper is refreshed to avoid Windows caching issues; use `-DisableWallpaperRefresh` to keep the old behavior.
- All users: use `-AllUsers` (requires elevation). Use `-ExcludeDefaultUserProfile` to skip the default profile.
- Logon screen: use `-Target LogonScreen` or `-Target Both` (requires elevation).
- Long values: use `-ValueWrapWidth` or JSON `ValueWrapWidth` to wrap value text such as long AD descriptions.
- Stable output path: use `-OutputFileName` to control the output name and path.
- Charts: use `New-BGInfoChart` to add ChartForgeX-backed transparent chart overlays with optional history.

## Deployment recipes

Custom text works on both sides of the layout, so you can compute the left column yourself instead of relying only on builtin labels:

```powershell
$bgHostname = $env:COMPUTERNAME
$bgUsername = if ($user = (Get-CimInstance Win32_ComputerSystem).UserName) {
    $user.Split('\')[-1]
} else {
    'Unknown'
}

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoValue -Name $bgHostname -Value $bgUsername -Color LemonChiffon -ValueColor White -FontSize 24 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Support' -Value 'helpdesk@contoso.com'
}
```

Apply the generated wallpaper for existing profiles and the default profile used by new users:

```powershell
New-BGInfo -MonitorIndex 0 -AllUsers {
    New-BGInfoValue -BuiltinValue HostName
    New-BGInfoValue -BuiltinValue OSName -Name 'OS'
} -ConfigurationDirectory 'C:\ProgramData\PowerBGInfo'
```

Skip the default profile if you only want current user profiles updated:

```powershell
New-BGInfo -MonitorIndex 0 -AllUsers -ExcludeDefaultUserProfile {
    New-BGInfoValue -BuiltinValue HostName
} -ConfigurationDirectory 'C:\ProgramData\PowerBGInfo'
```

Apply to the logon/lock screen, or to both the desktop wallpaper and logon screen in one run:

```powershell
New-BGInfo -MonitorIndex 0 -Target Both {
    New-BGInfoValue -BuiltinValue HostName
    New-BGInfoValue -BuiltinValue OSName -Name 'OS'
} -ConfigurationDirectory 'C:\ProgramData\PowerBGInfo'
```

For Remote Desktop Session Host / terminal server scenarios, run PowerBGInfo in each user session when you need per-user values such as `FullUserName`. `-AllUsers` is best for staging the same generated wallpaper to many profiles, not for rendering unique user data offline.

On Windows 11 24H2, keep the default wallpaper refresh behavior enabled unless you have a specific reason to disable it. PowerBGInfo writes a temporary refresh file first to avoid Windows reusing a cached wallpaper path after relogon.

## Charts

PowerBGInfo can place ChartForgeX charts directly on the generated wallpaper. The chart itself is rendered as a transparent overlay, then composited into the final image, so it works well on real wallpapers without a heavy dashboard frame.

![PowerBGInfo ChartForgeX showcase](Examples/Output/PowerBGInfo.ChartForgeX.Showcase.jpg)

The example above is generated by `Examples/Run.BGInfo.ChartForgeX.ps1`. It combines:

- `Area` and `Line` charts for short performance trends.
- `Donut` with point labels, legend, palette, and center text.
- `ProgressBar` for compliance or rollout status.
- `Pictorial` for simple status counts.

For a smaller operational layout, use stacked charts anchored to one corner:

![PowerBGInfo compact ChartForgeX overlay](Examples/Output/PowerBGInfo.ChartForgeX.Compact.jpg)

If you want the chart ink to melt into the wallpaper, omit `-BackgroundColor` on the chart or set it to `Transparent` / `'#00000000'`. The lines, labels, legend, grid, and data labels remain, but PowerBGInfo does not draw a panel behind them:

![PowerBGInfo transparent ChartForgeX overlay](Examples/Output/PowerBGInfo.ChartForgeX.Transparent.jpg)

```powershell
New-BGInfo -MonitorIndex 0 {
    New-BGInfoValue -BuiltinValue HostName -Color White
    New-BGInfoChart -Id "cpu" -Title "CPU" -Value 42 -Kind Sparkline -LineColor '#2DD4BF' -TextColor White -GridColor '#E6D2DCE8' -ShowGrid -MaxPoints 60 -OffsetX 20 -OffsetY 20 -Anchor BottomLeft
} -ConfigurationDirectory $PSScriptRoot\Output -BackgroundColor Black
```

Built-in metric sources can be used when no explicit values are provided. This is most useful with `-UseHistory` enabled, because each run appends a new sample:

```powershell
New-BGInfo -MonitorIndex 0 {
    New-BGInfoValue -BuiltinValue HostName -Color White
    New-BGInfoChart -Id "cpu" -Title "CPU" -Metric CpuPercent -ValueSuffix "%" -Kind Sparkline -MaxPoints 60 -OffsetX 20 -OffsetY 20 -Anchor BottomLeft
    New-BGInfoChart -Id "disk" -Title "Disk C" -Metric DiskFreePercent -MetricArgument "C:" -ValueSuffix "%" -Kind Bar -MaxPoints 30 -OffsetX 20 -OffsetY 130 -Anchor BottomLeft
} -ConfigurationDirectory $PSScriptRoot\Output -BackgroundColor Black
```

Charts are rendered by ChartForgeX into transparent overlays before PowerBGInfo composites them into the wallpaper. Supported kinds are `Sparkline`, `Line`, `Area`, `Bar`, `HorizontalBar`, `Gauge`, `Circle`, `RadialBar`, `Pie`, `Donut`, `ProgressBar`, and `Pictorial`.

Point-based charts can use `-Labels`, `-Palette`, `-ShowLegend`, `-ShowPointLegend`, `-LegendPosition`, and `-ShowDataLabels`. Gauge-like charts can use `-Minimum` and `-Maximum`; donut/progress/pictorial charts expose focused options such as `-DonutInnerRadiusRatio`, `-DonutCenterValue`, `-NoProgressHandles`, `-PictorialSymbol`, and `-PictorialColumns`.

PowerShell chart color parameters accept `System.Drawing.Color`, known color names, `#RRGGBB`, and `#AARRGGBB` strings, so examples can stay dependency-light and copy/paste friendly.

```powershell
$panel = '#AC0A101C'
$green = '#34D399'
$red = '#F87171'

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoValue -Name 'PowerBGInfo' -Value 'ChartForgeX overlays' -Color LemonChiffon -ValueColor White -FontSize 24
    New-BGInfoChart -Id 'disk-donut' -Title 'Disk C' -Kind Donut -Values 72,28 -Labels 'Used','Free' -ValueSuffix '%' -Palette $red,$green -ShowLegend -ShowPointLegend -LegendPosition Right -ShowDataLabels -Maximum 100 -DonutCenterValue '72%' -DonutCenterLabel 'Used' -ShowLatestValue:$false -BackgroundColor $panel -Anchor BottomRight -OffsetX 32 -OffsetY 32 -Width 350 -Height 240 -NoHistory
    New-BGInfoChart -Id 'service-pictorial' -Title 'Critical services' -Kind Pictorial -Values 9,1 -Labels 'Running','Stopped' -Palette $green,$red -PictorialSymbol Person -PictorialColumns 10 -ShowDataLabels -Maximum 10 -ShowLatestValue:$false -BackgroundColor $panel -Anchor TopLeft -OffsetX 32 -OffsetY 266 -Width 455 -Height 180 -NoHistory
} -FilePath $PSScriptRoot\Samples\TapC-Evotec-2560x1080.jpg -ConfigurationDirectory $PSScriptRoot\Output -OutputFileName 'PowerBGInfo.ChartForgeX.Custom.png' -WallpaperFit Fill
```

Use history for rolling metrics and use static values for status snapshots:

| Scenario | Recommended kind | Notes |
| --- | --- | --- |
| CPU or memory over time | `Sparkline`, `Line`, `Area` | Use `-Metric CpuPercent`, `-Metric MemoryPercent`, and keep history enabled. |
| Disk used/free | `Donut`, `Pie`, `Bar` | Use explicit `-Values` and `-Labels` when you want a clean split. |
| Compliance, deployment, backup status | `ProgressBar` | Use `-Maximum 100`, `-NoProgressHandles`, and `-ShowDataLabels`. |
| Running/stopped counts | `Pictorial` | Use `-PictorialSymbol Person` or `Circle` with a small palette. |
| One current KPI | `Gauge`, `Circle`, `RadialBar` | Use `-Minimum` and `-Maximum` for stable scaling. |

```powershell
New-BGInfo -MonitorIndex 0 {
    New-BGInfoValue -BuiltinValue HostName -Color White
    New-BGInfoChart -Id "disk" -Title "Disk C" -Values 72,28 -ValueSuffix "%" -Kind Donut -Labels "Used","Free" -Palette Crimson,MediumSeaGreen -ShowLegend -ShowPointLegend -LegendPosition Right -Maximum 100 -DonutCenterLabel "Disk C" -OffsetX 20 -OffsetY 20 -Anchor BottomLeft -NoHistory
    New-BGInfoChart -Id "services" -Title "Services" -Kind Pictorial -Values 8,2 -Labels "Running","Stopped" -Palette MediumSeaGreen,Gray -PictorialSymbol Person -PictorialColumns 10 -OffsetX 20 -OffsetY 150 -Anchor BottomLeft
} -ConfigurationDirectory $PSScriptRoot\Output -BackgroundColor Black
```

The same showcase can be generated from JSON and the CLI:

```powershell
.\Sources\PowerBGInfo.Cli\bin\Release\net8.0-windows\PowerBGInfo.Cli.exe `
    --config .\Examples\Configuration\PowerBGInfo.ChartForgeX.Showcase.json `
    --no-apply
```

## JSON configuration + CLI

PowerBGInfo supports JSON-driven configurations that are shared by the CLI and PowerShell. This is the recommended way to operationalize the tool.

Sample file: `Examples/Configuration/PowerBGInfo.Sample.json`

ChartForgeX chart showcase: `Examples/Configuration/PowerBGInfo.ChartForgeX.Showcase.json`

When hand-editing JSON on Windows, remember that backslashes inside string values must be escaped as `\\`.

```json
{
  "ConfigurationDirectory": "Output",
  "FilePath": "Samples\\TapC-Evotec-2560x1080.jpg",
  "MonitorIndex": 0,
  "Target": "Wallpaper",
  "WallpaperFit": "Fill",
  "Color": "#FFFFFFFF",
  "ValueColor": "#FF00FFFF",
  "ValueWrapWidth": 320,
  "ChartLayout": "Stack",
  "ChartStackAnchor": "BottomLeft",
  "ChartStackDirection": "Vertical",
  "ChartStackSpacing": 12,
  "ChartStackOffsetX": 10,
  "ChartStackOffsetY": 10,
  "ChartStackAlignToTextBlock": true,
  "ChartStackOutsideTextBlock": true,
  "Entries": [
    { "Type": "Value", "Name": "Host", "BuiltinValue": "HostName" },
    { "Type": "Value", "Name": "User", "BuiltinValue": "FullUserName" },
    { "Type": "Label", "Name": "Performance" }
  ],
  "Charts": [
    {
      "Id": "cpu",
      "Title": "CPU",
      "Kind": "Sparkline",
      "Metric": "CpuPercent",
      "ValueSuffix": "%",
      "ShowGrid": true,
      "GridLineCount": 3,
      "MaxPoints": 60,
      "Anchor": "BottomLeft",
      "OffsetX": 20,
      "OffsetY": 20
    }
  ]
}
```

PowerShell authoring and rendering:

```powershell
Invoke-BGInfo -Path $PSScriptRoot\bginfo.json
```

Export JSON from PowerShell:

```powershell
$config = New-BGInfoConfiguration -Target File
Export-BGInfoConfiguration -InputObject $config -Path $PSScriptRoot\bginfo.json -Force
```

CLI runtime:

```powershell
PowerBGInfo.Cli --config $PSScriptRoot\bginfo.json
```

The repository CI validates this workflow on every PR:

- PowerShell 5.1 module tests
- PowerShell 7 module tests
- PowerShell exports JSON that the CLI consumes successfully
- single-file CLI publish and execution
- NativeAOT CLI smoke publish and `Target File` execution

Publish a single-file CLI build:

```powershell
dotnet publish .\Sources\PowerBGInfo.Cli\PowerBGInfo.Cli.csproj -c Release -f net8.0-windows -r win-x64 --self-contained false -p:PublishSingleFile=true
```

Publish a NativeAOT CLI build for file-generation scenarios:

```powershell
dotnet publish .\Sources\PowerBGInfo.Cli\PowerBGInfo.Cli.csproj -c Release -f net8.0-windows -r win-x64 -p:PublishAot=true
```

NativeAOT is currently best for `--no-apply` / `Target File` workflows with an explicit `FilePath`. Windows desktop APIs from `DesktopManager` and `System.Management` still emit AOT warnings for wallpaper and system-query paths.
Treat NativeAOT as a file-generation smoke path, not yet as a strict pixel-for-pixel parity target for every machine/runtime combination.

You can also use only builtin values

```powershell
New-BGInfo -MonitorIndex 0 {
    # Lets add computer name, but lets use builtin values for that
    New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20 -FontFamilyName 'Calibri'
    New-BGInfoValue -BuiltinValue FullUserName -Color White
    New-BGInfoValue -BuiltinValue CpuName -Color White
    New-BGInfoValue -BuiltinValue CpuLogicalCores -Color White -ValueColor Red
    New-BGInfoValue -BuiltinValue RAMSize -Color White
    New-BGInfoValue -BuiltinValue RAMSpeed -Color White -ValueColor ([System.Drawing.Color]::Aquamarine)
    New-BGInfoValue -BuiltinValue RAMPartNumber -Color White
    New-BGInfoValue -BuiltinValue BiosVersion -Color White
    New-BGInfoValue -BuiltinValue BiosManufacturer -Color White
    New-BGInfoValue -BuiltinValue BiosReleaseDate -Color White
    New-BGInfoValue -BuiltinValue OSName -Color White -Name "Operating System"
    New-BGInfoValue -BuiltinValue OSVersion -Color White
    New-BGInfoValue -BuiltinValue OSArchitecture -Color White
    New-BGInfoValue -BuiltinValue OSBuild -Color White
    New-BGInfoValue -BuiltinValue OSInstallDate -Color White
    New-BGInfoValue -BuiltinValue OSLastBootUpTime -Color White

} -FilePath "C:\Support\GitHub\PowerBGInfo\Examples\Samples\TapN-Evotec-1600x900.jpg" -ConfigurationDirectory $PSScriptRoot\Output -PositionX 75 -PositionY 75 -WallpaperFit Fit
```

Here's the output from command above

![PowerBGInfo](https://raw.githubusercontent.com/EvotecIT/PowerBGInfo/master/Examples/Output/TapN-Evotec-1600x900.jpg)
