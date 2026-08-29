---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfo
## SYNOPSIS
Creates a BGInfo overlay image and optionally applies it as wallpaper.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfo [-BGInfoContent] <scriptblock> -ConfigurationDirectory <string> [-FilePath <string>] [-OutputFileName <string>] [-FontFamilyName <string>] [-Color <Object>] [-BackgroundColor <Object>] [-FontSize <int>] [-Bold] [-FontWeight <int>] [-Italic] [-Underline] [-UnderlineStyle <TextDecorationStyle>] [-StrikethroughStyle <TextDecorationStyle>] [-Baseline <TextBaseline>] [-TextCase <TextCaseTransform>] [-ValueColor <Object>] [-ValueFontSize <float>] [-ValueFontFamilyName <string>] [-ValueBold] [-ValueFontWeight <int>] [-ValueItalic] [-ValueUnderline] [-ValueUnderlineStyle <TextDecorationStyle>] [-ValueStrikethroughStyle <TextDecorationStyle>] [-ValueBaseline <TextBaseline>] [-ValueTextCase <TextCaseTransform>] [-ValueWrapWidth <int>] [-SpaceBetweenLines <int>] [-SpaceBetweenColumns <int>] [-PositionX <float>] [-PositionY <float>] [-MonitorIndex <int>] [-SpaceX <int>] [-SpaceY <int>] [-WallpaperFit <DesktopWallpaperPosition>] [-TextPosition <BgInfoTextPosition>] [-Target <BgInfoTarget>] [-ChartLayout <BgInfoChartLayoutMode>] [-ChartStackAnchor <BgInfoTextPosition>] [-ChartStackDirection <BgInfoChartStackDirection>] [-ChartStackSpacing <int>] [-ChartStackOffsetX <int>] [-ChartStackOffsetY <int>] [-ChartStackAlignToTextBlock] [-ChartStackOutsideTextBlock] [-AllUsers] [-ExcludeDefaultUserProfile] [-DisableWallpaperRefresh] [-DisableWallpaperSlideshow] [-UseScreenCoordinates] [-JsonPath <string>] [-ExportOnly] [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Use the script block to emit label/value entries.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfo -ConfigurationDirectory 'Value'
```


## PARAMETERS

### -AllUsers
Apply wallpaper for all user profiles.

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

### -BackgroundColor
Background color to use when no wallpaper image is available.

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

### -Baseline
Default label subscript or superscript placement.

```yaml
Type: TextBaseline
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Normal, Superscript, Subscript

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BGInfoContent
Script block that outputs BGInfo entries.

```yaml
Type: ScriptBlock
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Bold
Render labels with a bold font weight by default.

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

### -ChartLayout
Chart layout mode.

```yaml
Type: BgInfoChartLayoutMode
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Manual, Stack

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ChartStackAlignToTextBlock
Align stacked charts to the text block.

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

### -ChartStackAnchor
Anchor used when stacking charts.

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

### -ChartStackDirection
Direction used when stacking charts.

```yaml
Type: BgInfoChartStackDirection
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Vertical, Horizontal

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ChartStackOffsetX
Horizontal offset for stacked charts.

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

### -ChartStackOffsetY
Vertical offset for stacked charts.

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

### -ChartStackOutsideTextBlock
Place stacked charts outside the text block.

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

### -ChartStackSpacing
Spacing between stacked charts.

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

### -Color
Default label color.

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

### -ConfigurationDirectory
Output directory for generated BGInfo images.

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

### -DisableWallpaperRefresh
Disable the forced wallpaper refresh after generation.

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

### -DisableWallpaperSlideshow
Disable automatic preservation of the current Windows wallpaper slideshow.

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

### -ExcludeDefaultUserProfile
Exclude the default user profile when applying to all users.

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

### -ExportOnly
Export JSON only and skip image generation/application. Requires JsonPath.

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

### -FilePath
Optional base wallpaper file path. When omitted, current wallpaper is used.

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

### -FontFamilyName
Default label font family.

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

### -FontSize
Default label font size.

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

### -FontWeight
Default numeric label font weight from 100 through 900.

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

### -Italic
Render labels with italic text by default.

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

### -JsonPath
Optional path where the generated configuration JSON should be saved.

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

### -MonitorIndex
Monitor index to target for wallpaper operations.

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

### -OutputFileName
Optional output file name for the generated image.

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

### -PassThru
Return the generated configuration object instead of rendering the image.

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

### -PositionX
Legacy position X placeholder (reserved for future layout strategies).

```yaml
Type: Single
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
Legacy position Y placeholder (reserved for future layout strategies).

```yaml
Type: Single
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SpaceBetweenColumns
Spacing between label and value columns.

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

### -SpaceBetweenLines
Vertical spacing between rows.

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

### -SpaceX
X padding used for layout positioning.

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

### -SpaceY
Y padding used for layout positioning.

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

### -StrikethroughStyle
Default label strikethrough pattern.

```yaml
Type: TextDecorationStyle
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Target
Output target (Wallpaper, File, LogonScreen, or Both).

```yaml
Type: BgInfoTarget
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Wallpaper, LogonScreen, Both, File

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TextCase
Default display-time label casing transform.

```yaml
Type: TextCaseTransform
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Uppercase, Lowercase, TitleCase, SentenceCase, ToggleCase

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TextPosition
Layout anchor position (for example TopLeft, TopCenter, BottomRight).

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

### -Underline
Underline labels by default.

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

### -UnderlineStyle
Default label underline pattern.

```yaml
Type: TextDecorationStyle
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UseScreenCoordinates
Use screen coordinates for placement calculations.

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

### -ValueBaseline
Default value subscript or superscript placement.

```yaml
Type: TextBaseline
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Normal, Superscript, Subscript

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueBold
Render values with a bold font weight by default.

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

### -ValueColor
Default value color.

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

### -ValueFontFamilyName
Default value font family.

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

### -ValueFontSize
Default value font size.

```yaml
Type: Single
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueFontWeight
Default numeric value font weight from 100 through 900.

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

### -ValueItalic
Render values with italic text by default.

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

### -ValueStrikethroughStyle
Default value strikethrough pattern.

```yaml
Type: TextDecorationStyle
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueTextCase
Default display-time value casing transform.

```yaml
Type: TextCaseTransform
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Uppercase, Lowercase, TitleCase, SentenceCase, ToggleCase

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueUnderline
Underline values by default.

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

### -ValueUnderlineStyle
Default value underline pattern.

```yaml
Type: TextDecorationStyle
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueWrapWidth
Maximum width used when wrapping value text. Set to 0 to disable wrapping.

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

### -WallpaperFit
Wallpaper fit mode used after generation.

```yaml
Type: DesktopWallpaperPosition
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Center, Tile, Stretch, Fit, Fill, Span

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

- `System.String`
- `PowerBGInfo.BgInfoConfiguration`

## RELATED LINKS

- None
