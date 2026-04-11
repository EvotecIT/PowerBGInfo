---
title: "Generate a background from a script"
description: "Use PowerBGInfo to author endpoint background information in PowerShell."
layout: docs
---

This pattern is useful when endpoints need a visible inventory surface generated from local data.

It comes from the source example at `Examples/Run.BGInfo1.ps1`.

## When to use this pattern

- You need a generated endpoint information background.
- You want PowerShell control over what values appear.
- You need repeatable output for deployment or testing.

## Example

```powershell
Import-Module .\PowerBGInfo.psd1 -Force

New-BGInfo -MonitorIndex 0 {
    New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20 -FontFamilyName 'Calibri'
    New-BGInfoValue -BuiltinValue FullUserName -Name 'User Name' -Color White
    New-BGInfoValue -BuiltinValue CpuName
    New-BGInfoValue -BuiltinValue RAMSize
} -FilePath "$PSScriptRoot\Samples\Background.jpg" `
  -ConfigurationDirectory "$PSScriptRoot\Output" `
  -WallpaperFit Center `
  -Target Both
```

## What this demonstrates

- building the layout in PowerShell
- using built-in system values
- separating source image and generated output

## Source

- [Run.BGInfo1.ps1](https://github.com/EvotecIT/PowerBGInfo/blob/v2-speedygonzales/Examples/Run.BGInfo1.ps1)

