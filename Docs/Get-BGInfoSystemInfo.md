---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://powerbginfo.com
schema: 2.0.0
---
# Get-BGInfoSystemInfo
## SYNOPSIS
Returns read-only facts about this machine as one plain object.

## SYNTAX
### __AllParameterSets
```powershell
Get-BGInfoSystemInfo [<CommonParameters>]
```

## DESCRIPTION
Uptime, OS and build, IPv4, fixed disks, last installed update, Defender and BitLocker state, and the user signed in to this session (LoggedOnUser; under SYSTEM in session 0, the console user). Every probe is read-only and fails on its own: a missing WMI class, a timeout or a lack of elevation leaves that field Unknown or empty instead of failing the wallpaper.

The object is plain data. Replace it with your own function that returns the same property names and the presentation code keeps working.

## EXAMPLES

### EXAMPLE 1
```powershell
$facts = Get-BGInfoSystemInfo
New-BGInfoValue -Name 'Uptime' -Value $facts.UptimeText
$facts.Disks | New-BGInfoTable -Title 'Disks' -Property Drive, FreeGB, UsedPercent
```


## PARAMETERS

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `PowerBGInfo.BgInfoSystemInfo`

## RELATED LINKS

- None
