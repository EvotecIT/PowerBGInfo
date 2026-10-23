---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://powerbginfo.com
schema: 2.0.0
---
# New-BGInfoTimelineItem
## SYNOPSIS
Creates one range for New-BGInfoTimeline.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoTimelineItem [-Lane] <string> [[-Label] <string>] -Start <datetime> [-End <datetime>] [-Duration <timespan>] [-Category <string>] [-Status <BgInfoStatus>] [-Color <Object>] [<CommonParameters>]
```

## DESCRIPTION
Creates one range for New-BGInfoTimeline.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoTimelineItem -Lane 'SQL-PROD-01' -Label 'Patching' -Start '2026-10-18 02:00' -Duration '02:00' -Category Maintenance
```


## PARAMETERS

### -Category
Legend category, for example Maintenance, Change freeze or On call.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Color
Explicit category color.

```yaml
Type: Object
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Duration
Range length, instead of -End.

```yaml
Type: TimeSpan
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -End
Range end, later than Start at the chart's 100-microsecond resolution. Without -End or -Duration the range stays open until now.

```yaml
Type: DateTime
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Label
Text drawn on the bar.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Lane
Lane name, for example a server, a team or On call.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Start
Range start.

```yaml
Type: DateTime
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Status
Status color for the category: Ok, Warning, Critical or Info.

```yaml
Type: BgInfoStatus
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Ok, Warning, Critical, Info

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

- `PowerBGInfo.BgInfoTimelineItem`

## RELATED LINKS

- None
