---
title: PowerBGInfo - machine facts on the desktop and lock screen
description: PowerBGInfo draws machine details, health, charts and diagrams onto desktop and lock-screen wallpapers from PowerShell or JSON. With DesktopManager and InformationBox, Evotec's desktop tools for IT teams.
slug: index
layout: home
meta.seo_title: PowerBGInfo - machine facts on the desktop and lock screen
meta.source_code.name: PowerBGInfo
meta.source_code.code_repository: https://github.com/EvotecIT/PowerBGInfo
meta.source_code.programming_language: PowerShell
meta.source_code.license: https://github.com/EvotecIT/PowerBGInfo/blob/v2-speedygonzales/License
---

## Install the module

```powershell
Install-Module PowerBGInfo -Scope CurrentUser
```

The module works in Windows PowerShell 5.1 and in PowerShell 7.4 or later. It needs no separate helper modules. Use `Update-Module PowerBGInfo` after testing a new version.

## Render a preview to a file

```powershell
New-BGInfo -Theme Aurora -ThemeMode Dark -Target File {
    New-BGInfoValue -BuiltinValue HostName -Name 'Computer'
    New-BGInfoValue -BuiltinValue FullUserName -Name 'User'
    New-BGInfoValue -BuiltinValue OSName -Name 'Windows'
    New-BGInfoValue -BuiltinValue IPv4Address -Name 'IP address'
    New-BGInfoValue -BuiltinValue Uptime -Name 'Up for'
    New-BGInfoChart -Region Right -Kind Gauge -Metric MemoryPercent `
        -Title 'Memory used' -Maximum 100 -ValueSuffix ' %'
} -ConfigurationDirectory C:\Temp\PowerBGInfo -OutputFileName preview.png
```

`-Target File` writes `preview.png` and leaves the desktop alone. The built-in values read the machine the script runs on, so every machine gets its own wallpaper from the same script.

## Set it as the wallpaper

```powershell
New-BGInfo -Theme Aurora {
    New-BGInfoValue -BuiltinValue HostName -Name 'Computer'
    New-BGInfoValue -BuiltinValue OSName -Name 'Windows'
} -ConfigurationDirectory C:\ProgramData\PowerBGInfo -WallpaperFit Fill
```

Without `-Target`, `New-BGInfo` sets the wallpaper for the account that runs it. On Windows, `-AllUsers` stages it for every profile, and `-Target LogonScreen` or `-Target Both` sets the lock screen too; those need an elevated session. One run applies one image; run it again to refresh the values.
