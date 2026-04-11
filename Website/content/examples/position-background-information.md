---
title: "Position background information"
description: "Use PowerBGInfo layout options to control where generated information appears."
layout: docs
---

This pattern is useful when wallpaper text must avoid logos, taskbars, or other visual elements.

It comes from the source example at `Examples/Run.BGInfo3.ps1`.

## When to use this pattern

- You need consistent placement across machines.
- The background image already has visual constraints.
- You want an authoring script rather than a manual editor.

## Example

```powershell
Import-Module .\PowerBGInfo.psd1 -Force

New-BGInfo -MonitorIndex 0 {
    New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20
    New-BGInfoValue -BuiltinValue OSName -Color White -Name 'Operating System'
    New-BGInfoValue -BuiltinValue OSVersion -Color White
    New-BGInfoValue -BuiltinValue OSLastBootUpTime -Color White
} -FilePath 'C:\Images\CompanyWallpaper.jpg' `
  -ConfigurationDirectory "$PSScriptRoot\Output" `
  -PositionX 75 `
  -PositionY 75 `
  -WallpaperFit Fit
```

## What this demonstrates

- using explicit X/Y placement
- showing operating system values
- keeping the output directory repeatable

## Source

- [Run.BGInfo3.ps1](https://github.com/EvotecIT/PowerBGInfo/blob/v2-speedygonzales/Examples/Run.BGInfo3.ps1)

