---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoLayout
## SYNOPSIS
Sets the safe area and region sizes that -Region panels flow into.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoLayout [-Margin <int>] [-MarginX <int>] [-MarginY <int>] [-Gap <int>] [-ColumnWidth <int>] [-BandHeight <int>] [-TaskbarReserve <int>] [-IconAreaWidth <int>] [-MaximumPanelHeight <int>] [-ColumnAlignment <BgInfoColumnAlignment>] [<CommonParameters>]
```

## DESCRIPTION
Emit it inside the New-BGInfo script block. Without it, regions use the theme margins or the defaults.

Regions: Top and Bottom are full-width bands that flow left to right. Left and Right are columns between the bands. Center fills the space between the columns. A panel without -Width or -Height shares the space left in its region.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfo -Theme Graphite -Target File {
    New-BGInfoLayout -Margin 48 -ColumnWidth 480 -IconAreaWidth 120
    New-BGInfoChart -Region Right -Title 'CPU' -Kind Area -Values 12, 30, 22
    New-BGInfoChart -Region Right -Title 'Memory' -Kind Gauge -Values 64
} -ConfigurationDirectory $PSScriptRoot -OutputFileName 'regions.png'
```


## PARAMETERS

### -BandHeight
Height of the Top and Bottom bands for panels without -Height. Zero uses 26 % of the height, clamped to 200-340 pixels.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ColumnAlignment
Where Left and Right stacks sit when their panels do not fill the column: Top, Center or Bottom.

```yaml
Type: BgInfoColumnAlignment
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Top, Center, Bottom

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ColumnWidth
Width of the Left and Right columns. Zero uses 24 % of the width, clamped to 340-620 pixels.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Gap
Gap between panels and regions.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IconAreaWidth
Width kept free on the left for desktop icons.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Margin
Distance from every screen edge. -MarginX and -MarginY override one axis.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MarginX
Distance from the left and right edges.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MarginY
Distance from the top and bottom edges.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaximumPanelHeight
Largest height an unsized panel takes when it shares a column or the center. Zero uses 36 % of the image height.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TaskbarReserve
Height kept free at the bottom for the taskbar. Unset estimates it from the image height (48 px at 1080, 64 px at 1440). Use 0 when the taskbar auto-hides or sits elsewhere.

```yaml
Type: Int32
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

- `PowerBGInfo.BgInfoLayout`

## RELATED LINKS

- None
