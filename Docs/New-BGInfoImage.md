---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoImage
## SYNOPSIS
Creates a BGInfo image overlay definition.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoImage [-Path] <string> [-Width <int>] [-Height <int>] [-Anchor <BgInfoTextPosition>] [-OffsetX <int>] [-OffsetY <int>] [-PositionX <int>] [-PositionY <int>] [-Opacity <double>] [-Fit <BgInfoImageFit>] [<CommonParameters>]
```

## DESCRIPTION
Creates a BGInfo image overlay definition.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoImage -Path 'C:\Path'
```


## PARAMETERS

### -Anchor
Anchor position for placement.

```yaml
Type: BgInfoTextPosition
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Fit
How the image is fitted inside the destination rectangle.

```yaml
Type: BgInfoImageFit
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Stretch, Contain, Cover, Center, Tile

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Height
Target image height in pixels. Omit with Width to preserve aspect ratio.

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

### -OffsetX
Horizontal offset from the anchor.

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

### -OffsetY
Vertical offset from the anchor.

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

### -Opacity
Image opacity from zero to one.

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

### -Path
Path to the image file.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: FullName
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PositionX
Absolute X position for placement.

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

### -PositionY
Absolute Y position for placement.

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

### -Width
Target image width in pixels. Omit with Height to preserve aspect ratio.

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

- `PowerBGInfo.BgInfoImage`

## RELATED LINKS

- None
