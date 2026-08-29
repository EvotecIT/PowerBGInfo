---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoVisualCanvasTile
## SYNOPSIS
Creates a BGInfo visual canvas tile definition.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoVisualCanvasTile -Label <string> -Value <string> [-Side <BgInfoVisualCanvasSide>] [-Icon <string>] [-Detail <string>] [-Width <int>] [-Height <int>] [-Accent <Object>] [-Progress <Double>] [-SurfaceStyle <BgInfoVisualCanvasTileSurfaceStyle>] [-IconKind <BgInfoVisualCanvasTileIconKind>] [-MiniChartKind <BgInfoVisualCanvasTileMiniChartKind>] [-TextFitPolicy <BgInfoVisualCanvasTileTextFitPolicy>] [-MiniChartValues <double[]>] [-MiniChartMaximum <Double>] [<CommonParameters>]
```

## DESCRIPTION
Tiles are the readable lane-based information boxes used by New-BGInfoVisualCanvas.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoVisualCanvasTile -Side Left -IconKind Computer -SurfaceStyle Raised -Label HOSTNAME -Value '{{HostName}}' -Detail 'production desktop' -Accent '#0F766EFF'
```


### EXAMPLE 2
```powershell
New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Raised -Label 'CPU LOAD' -Value '31% active' -MiniChartKind Area -MiniChartValues 22,28,25,36,31 -MiniChartMaximum 100
```


## PARAMETERS

### -Accent
Optional accent color.

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

### -Detail
Optional detail text. Templates are resolved at render time.

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

### -Height
Optional tile height in pixels. Zero uses the visual canvas default or template default.

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

### -Icon
Compact tile icon or symbol.

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

### -IconKind
Built-in icon to render instead of the Icon text.

```yaml
Type: BgInfoVisualCanvasTileIconKind
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Text, Computer, Network, OperatingSystem, Cpu, Memory, User, Domain, Terminal, Storage, Shield

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Label
Tile label. Templates such as {{HostName}} are resolved at render time.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MiniChartKind
Compact chart kind rendered inside the tile.

```yaml
Type: BgInfoVisualCanvasTileMiniChartKind
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Sparkline, Area, Bars

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MiniChartMaximum
Optional compact chart maximum.

```yaml
Type: Double
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MiniChartValues
Compact chart values rendered inside the tile.

```yaml
Type: Double[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Progress
Optional progress value from zero to one.

```yaml
Type: Double
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Side
Tile lane placement.

```yaml
Type: BgInfoVisualCanvasSide
Parameter Sets: __AllParameterSets
Aliases: Lane
Possible values: Left, Right, Center

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SurfaceStyle
Tile surface style.

```yaml
Type: BgInfoVisualCanvasTileSurfaceStyle
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Glass, Outline, Raised

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TextFitPolicy
Tile-specific text fitting policy. Auto inherits the visual canvas setting.

```yaml
Type: BgInfoVisualCanvasTileTextFitPolicy
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Auto, SingleLineEllipsis, Wrap, ShrinkToFit, WrapThenShrink

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Value
Primary tile value. Templates such as {{HostName}} are resolved at render time.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Width
Optional tile width in pixels. Zero uses the visual canvas default or template default.

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

- `PowerBGInfo.BgInfoVisualCanvasTile`

## RELATED LINKS

- None
