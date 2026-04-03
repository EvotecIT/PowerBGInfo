<p align="center">
  <a href="https://dev.azure.com/evotecpl/PowerBGInfo/_build/results?buildId=latest"><img src="https://img.shields.io/azure-devops/build/evotecpl/39c74615-8f34-4af0-a835-68dc33f9214f/14?label=Azure%20Pipelines&style=flat-square"></a>
  <a href="https://github.com/EvotecIT/PowerBGInfo/actions/workflows/test-dotnet.yml"><img src="https://github.com/EvotecIT/PowerBGInfo/actions/workflows/test-dotnet.yml/badge.svg?branch=v2-speedygonzales&style=flat-square"></a>
  <a href="https://codecov.io/gh/EvotecIT/PowerBGInfo"><img src="https://codecov.io/gh/EvotecIT/PowerBGInfo/branch/v2-speedygonzales/graph/badge.svg?style=flat-square"></a>
  <a href="https://www.powershellgallery.com/packages/PowerBGInfo"><img src="https://img.shields.io/powershellgallery/v/PowerBGInfo.svg?style=flat-square"></a>
  <a href="https://www.powershellgallery.com/packages/PowerBGInfo"><img src="https://img.shields.io/powershellgallery/vpre/PowerBGInfo.svg?label=powershell%20gallery%20preview&colorB=yellow&style=flat-square"></a>
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

**PowerBGInfo** is a PowerShell module that is used to generate background information for Windows machines.
It's a replacement for [BGInfo](https://technet.microsoft.com/en-us/sysinternals/bginfo.aspx) that doesn't require installation and is much more flexible.

You can read about this project on this [blog post](https://evotec.xyz/powerbginfo-powershell-alternative-to-sysinternals-bginfo/) that tells a little backstory and shows few things.

## Installation

Install from [PowerShell Gallery](https://www.powershellgallery.com/packages/PowerBGInfo) is as easy as:

```powershell
Install-Module PowerBGInfo -Force -Verbose
```

When there's an update you can update it using same command which will install new module version.

## Known Issues

This module will work fine for PowerShell 5.1 and PowerShell 7+.
**Currently the module has a problem when running in VSCode PowerShell extension when on PowerShell 5.1 (other versions work fine!)**
It works fine when running in PowerShell 5.1 console, or ISE (shrug!).

## Usage

Here's a small taste of the code:

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

Here's how the wallpaper will look like:

![PowerBGInfo](https://raw.githubusercontent.com/EvotecIT/PowerBGInfo/master/Examples/Output/PrzemyslawKlysAndKulkozaurr.jpg)

## Additional options

- Solid-color desktops: when no wallpaper file exists, PowerBGInfo renders a blank image using the desktop background color or `-BackgroundColor`.
- Refresh behavior: by default the wallpaper is refreshed to avoid Windows caching issues; use `-DisableWallpaperRefresh` to keep the old behavior.
- All users: use `-AllUsers` (requires elevation). Use `-ExcludeDefaultUserProfile` to skip the default profile.
- Logon screen: use `-Target LogonScreen` or `-Target Both` (requires elevation).
- Long values: use `-ValueWrapWidth` or JSON `ValueWrapWidth` to wrap value text such as long AD descriptions.
- Stable output path: use `-OutputFileName` to control the output name and path.
- Charts: use `New-BGInfoChart` to add sparklines or bar charts with optional history.

## Charts (MVP)

```powershell
New-BGInfo -MonitorIndex 0 {
    New-BGInfoValue -BuiltinValue HostName -Color White
    New-BGInfoChart -Id "cpu" -Title "CPU" -Value 42 -Kind Sparkline -MaxPoints 60 -OffsetX 20 -OffsetY 20 -Anchor BottomLeft
} -ConfigurationDirectory $PSScriptRoot\Output -BackgroundColor Black
```

Built-in metric sources can be used when no explicit values are provided:

```powershell
New-BGInfo -MonitorIndex 0 {
    New-BGInfoValue -BuiltinValue HostName -Color White
    New-BGInfoChart -Id "cpu" -Title "CPU" -Metric CpuPercent -ValueSuffix "%" -Kind Sparkline -MaxPoints 60 -OffsetX 20 -OffsetY 20 -Anchor BottomLeft
    New-BGInfoChart -Id "disk" -Title "Disk C" -Metric DiskFreePercent -MetricArgument "C:" -ValueSuffix "%" -Kind Bar -MaxPoints 30 -OffsetX 20 -OffsetY 130 -Anchor BottomLeft
} -ConfigurationDirectory $PSScriptRoot\Output -BackgroundColor Black
```

## JSON configuration + CLI

PowerBGInfo supports JSON-driven configurations that are shared by the CLI and PowerShell.

Sample file: `Examples/Configuration/PowerBGInfo.Sample.json`

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

PowerShell (thin wrapper):

```powershell
Invoke-BGInfo -Path $PSScriptRoot\bginfo.json
```

Export from PowerShell:

```powershell
$config = New-BGInfoConfiguration -Target File
Export-BGInfoConfiguration -InputObject $config -Path $PSScriptRoot\bginfo.json -Force
```

CLI:

```powershell
PowerBGInfo.Cli --config $PSScriptRoot\bginfo.json
```

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
