---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# Get-BGInfoMonitor
## SYNOPSIS
Lists the monitors PowerBGInfo renders for.

## SYNTAX
### __AllParameterSets
```powershell
Get-BGInfoMonitor [-Primary] [<CommonParameters>]
```

## DESCRIPTION
Each object has the Index that New-BGInfo -MonitorIndex takes, the device name, the bounds in desktop coordinates, the backend display identity, whether it is the primary monitor, and the display scale when the backend reports one. Loop over the output to render one image per monitor.

The cmdlet only reads the display configuration, needs no elevation and works in Constrained Language Mode. It returns nothing when physical display geometry is unavailable, for example on a headless host or the current Linux adapters. Supply explicit background dimensions in those cases.

## EXAMPLES

### EXAMPLE 1
```powershell
foreach ($monitor in Get-BGInfoMonitor) {
    New-BGInfo -MonitorIndex $monitor.Index -Target File `
        -ConfigurationDirectory ([IO.Path]::GetTempPath()) `
        -OutputFileName "wallpaper-$($monitor.Index).png" {
        New-BGInfoBackground -Width $monitor.Width -Height $monitor.Height
        New-BGInfoValue -Name 'Size' -Value "$($monitor.Width) x $($monitor.Height)"
    }
}
```


## PARAMETERS

### -Primary
Return only the primary monitor.

```yaml
Type: SwitchParameter
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `PowerBGInfo.BgInfoMonitor`

## RELATED LINKS

- None
