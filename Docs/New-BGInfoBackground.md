---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://powerbginfo.com
schema: 2.0.0
---
# New-BGInfoBackground
## SYNOPSIS
Creates a generated backdrop that replaces the wallpaper image.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoBackground [-Width <int>] [-Height <int>] [-Color <Object>] [-Gradient <Object[]>] [-GradientAngle <double>] [-ImagePath <string>] [-ImageFit <BgInfoImageFit>] [-ImageOpacity <double>] [-Glow <BgInfoBackgroundGlow[]>] [-Pattern <BgInfoBackgroundPattern>] [-PatternColor <Object>] [-PatternSpacing <int>] [<CommonParameters>]
```

## DESCRIPTION
Emit the result inside the New-BGInfo script block. Layers paint in order: color, image, gradient, glows, pattern.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfo -Target File {
    New-BGInfoBackground -Width 1920 -Height 1080 -Gradient '#0B1022', '#05070F' -Glow @(
        New-BGInfoBackgroundGlow -X 0.15 -Y 0.1 -Radius 0.55 -Color '#4F46E566'
    ) -Pattern Dots
    New-BGInfoValue -Name 'Host' -Value $env:COMPUTERNAME
} -ConfigurationDirectory $PSScriptRoot -OutputFileName 'Generated.png'
```


## PARAMETERS

### -Color
Base fill painted before every other layer. Defaults to black.

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

### -Glow
Soft radial lights created with New-BGInfoBackgroundGlow.

```yaml
Type: BgInfoBackgroundGlow[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Gradient
Two gradient colors: start and end. Translucent colors tint an image underneath.

```yaml
Type: Object[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GradientAngle
Gradient direction in degrees clockwise from the rightward horizontal axis. 0 paints left to right; 90 paints top to bottom.

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

### -Height
Output height in pixels. Zero uses the target monitor height.

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

### -ImageFit
How the image fills the backdrop. Cover fills the whole area and crops the overflow.

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

### -ImageOpacity
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

### -ImagePath
Optional image painted over the base color, for example a company wallpaper.

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

### -Pattern
Optional repeating pattern painted last.

```yaml
Type: BgInfoBackgroundPattern
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Dots, Grid

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PatternColor
Pattern ink. Keep the alpha low so the pattern stays behind the content.

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

### -PatternSpacing
Distance between pattern dots or lines in pixels.

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
Output width in pixels. Zero uses the target monitor width.

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

- `PowerBGInfo.BgInfoBackground`

## RELATED LINKS

- None
