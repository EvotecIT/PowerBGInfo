---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# Get-BGInfoCapability
## SYNOPSIS
Reports the current desktop backend and the wallpaper operations it supports.

## SYNTAX
### __AllParameterSets
```powershell
Get-BGInfoCapability [<CommonParameters>]
```

## DESCRIPTION
Reads capabilities without changing the desktop. File rendering works independently of desktop availability. Linux backends may support wallpaper changes while requiring explicit image dimensions.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-BGInfoCapability
```

Shows the selected Windows, macOS, GNOME, KDE, or unsupported backend and its limits.

## PARAMETERS

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `DesktopManager.DesktopCapabilities`

## RELATED LINKS

- None
