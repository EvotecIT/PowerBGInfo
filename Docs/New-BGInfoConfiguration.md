---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoConfiguration
## SYNOPSIS
Creates a BGInfo configuration object.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoConfiguration [-FilePath <string>] [-OutputFileName <string>] [-ConfigurationDirectory <string>] [-FontFamilyName <string>] [-Color <Object>] [-BackgroundColor <Object>] [-FontSize <float>] [-Bold] [-Underline] [-ValueColor <Object>] [-ValueFontSize <float>] [-ValueFontFamilyName <string>] [-ValueBold] [-ValueUnderline] [-ValueWrapWidth <int>] [-SpaceBetweenLines <int>] [-SpaceBetweenColumns <int>] [-PositionX <float>] [-PositionY <float>] [-MonitorIndex <int>] [-SpaceX <int>] [-SpaceY <int>] [-WallpaperFit <DesktopWallpaperPosition>] [-TextPosition <BgInfoTextPosition>] [-Target <BgInfoTarget>] [-ChartLayout <BgInfoChartLayoutMode>] [-ChartStackAnchor <BgInfoTextPosition>] [-ChartStackDirection <BgInfoChartStackDirection>] [-ChartStackSpacing <int>] [-ChartStackOffsetX <int>] [-ChartStackOffsetY <int>] [-ChartStackAlignToTextBlock] [-ChartStackOutsideTextBlock] [-AllUsers] [-ExcludeDefaultUserProfile] [-DisableWallpaperRefresh] [-DisableWallpaperSlideshow] [-UseScreenCoordinates] [-Entries <BgInfoEntry[]>] [-Variables <BgInfoVariable[]>] [-Charts <BgInfoChart[]>] [-Topologies <BgInfoTopology[]>] [-Images <BgInfoImage[]>] [-VisualCanvases <BgInfoVisualCanvas[]>] [<CommonParameters>]
```

## DESCRIPTION
Use this to build reusable configurations that can be exported to JSON.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoConfiguration -FilePath 'C:\Path'
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

### -Charts
Charts to include in the configuration.

```yaml
Type: BgInfoChart[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

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
Place stacked charts outside of the text block.

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

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisableWallpaperRefresh
Disable wallpaper refresh (keep old behavior).

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

### -Entries
Entries to include in the configuration.

```yaml
Type: BgInfoEntry[]
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

### -FilePath
Optional base wallpaper file path.

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

### -Images
Image overlays to include in the configuration.

```yaml
Type: BgInfoImage[]
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

### -PositionX
Legacy position X placeholder.

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
Legacy position Y placeholder.

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

### -TextPosition
Layout anchor position.

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

### -Topologies
Topology diagrams to include in the configuration.

```yaml
Type: BgInfoTopology[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

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

### -UseScreenCoordinates
Use screen coordinates for layout positioning.

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

### -Variables
Variables to include in the configuration.

```yaml
Type: BgInfoVariable[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -VisualCanvases
Visual canvas overlays to include in the configuration.

```yaml
Type: BgInfoVisualCanvas[]
Parameter Sets: __AllParameterSets
Aliases: VisualCanvas
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

- `PowerBGInfo.BgInfoConfiguration`

## RELATED LINKS

- None
