---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoVisualCanvas
## SYNOPSIS
Creates a BGInfo visual canvas definition backed by ChartForgeX.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoVisualCanvas [-Template <BgInfoVisualCanvasTemplate>] [-LayoutPreset <BgInfoVisualCanvasLayoutPreset>] [-Title <string>] [-Subtitle <string>] [-Width <int>] [-Height <int>] [-PositionX <int>] [-PositionY <int>] [-BackgroundTop <Object>] [-BackgroundBottom <Object>] [-Accent <Object>] [-SecondaryAccent <Object>] [-TitleColor <Object>] [-TitleAccentColor <Object>] [-SubtitleColor <Object>] [-TileGlassTop <Object>] [-TileGlassBottom <Object>] [-TileLabelColor <Object>] [-TileValueColor <Object>] [-TileDetailColor <Object>] [-TileProgressTrackColor <Object>] [-HeroBadgeTop <Object>] [-HeroBadgeBottom <Object>] [-HeroBadgeTextColor <Object>] [-NoHeroBadge] [-NoHeroContent] [-HeroBadgeText <string>] [-HeroBadgeImagePath <string>] [-HeroBadgeImageFit <BgInfoImageFit>] [-HeroBadgeImagePadding <int>] [-HeroBadgeImageOpacity <double>] [-FeatureAnchor <BgInfoTextPosition>] [-FeatureWidth <int>] [-FeatureHeight <int>] [-TileWidth <int>] [-TileHeight <int>] [-TileGap <int>] [-LeftTileWidth <int>] [-RightTileWidth <int>] [-CenterTileWidth <int>] [-LeftTileOffsetX <int>] [-LeftTileOffsetY <int>] [-RightTileOffsetX <int>] [-RightTileOffsetY <int>] [-CenterTileOffsetX <int>] [-CenterTileOffsetY <int>] [-TileTextFitPolicy <BgInfoVisualCanvasTileTextFitPolicy>] [-FeatureOffsetX <int>] [-FeatureOffsetY <int>] [-NoTechBackdrop] [-Opaque] [-Tile <BgInfoVisualCanvasTile[]>] [-Feature <BgInfoVisualCanvasFeature[]>] [<CommonParameters>]
```

## DESCRIPTION
Visual canvases render a reusable HUD-style overlay with a central title, side information boxes, and an optional feature strip.

## EXAMPLES

### EXAMPLE 1
```powershell
$tiles = @(
    New-BGInfoVisualCanvasTile -Side Left -IconKind Computer -SurfaceStyle Raised -Label HOSTNAME -Value '{{HostName}}'
    New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Raised -Label 'CPU LOAD' -Value '31% active' -MiniChartKind Area -MiniChartValues 22,28,25,36,31 -MiniChartMaximum 100
)

New-BGInfo -Target File {
    New-BGInfoVisualCanvas -Title 'PowerBGInfo' -Subtitle 'High-contrast information boxes' -Tile $tiles -TileGlassTop '#FFF7EDD9' -TileGlassBottom '#DBEAFECC' -TileValueColor '#0F172AFF'
} -FilePath .\Examples\Samples\TapC-Evotec-2560x1080.jpg -ConfigurationDirectory .\Examples\Output -OutputFileName 'PowerBGInfo.VisualCanvas.ContrastBox.jpg' -WallpaperFit Fill
```


### EXAMPLE 2
```powershell
New-BGInfoVisualCanvas -Title 'PowerBGInfo' -Feature $features -FeatureAnchor BottomRight -FeatureWidth 610 -FeatureOffsetX 165 -FeatureOffsetY 120
```


## PARAMETERS

### -Accent
Primary accent color.

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

### -BackgroundBottom
Bottom background color.

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

### -BackgroundTop
Top background color.

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

### -CenterTileOffsetX
Horizontal centered-lane offset in pixels. Positive values move the lane to the right.

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

### -CenterTileOffsetY
Vertical centered-lane offset in pixels.

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

### -CenterTileWidth
Default centered-lane tile width in pixels. Zero uses TileWidth or the template default.

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

### -Feature
Feature strip item definitions.

```yaml
Type: BgInfoVisualCanvasFeature[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FeatureAnchor
Optional feature-strip anchor. When omitted, the template keeps its default centered strip placement.

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

### -FeatureHeight
Optional feature-strip height in pixels. Zero uses the template default height.

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

### -FeatureOffsetX
Horizontal feature-strip offset. For right anchors, positive values inset from the right edge.

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

### -FeatureOffsetY
Vertical feature-strip offset. For bottom anchors, positive values inset from the bottom edge.

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

### -FeatureWidth
Optional feature-strip width in pixels. Zero uses the template default width.

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

### -Height
Canvas height in pixels. Zero uses the target wallpaper height.

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

### -HeroBadgeBottom
Hero badge bottom fill color.

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

### -HeroBadgeImageFit
How the hero badge image is fitted inside the badge.

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

### -HeroBadgeImageOpacity
Hero badge image opacity from zero to one.

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

### -HeroBadgeImagePadding
Padding inside the hero badge image area.

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

### -HeroBadgeImagePath
Optional image path rendered inside the central hero badge.

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

### -HeroBadgeText
Text rendered in the central hero badge when no image is configured.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: HeroBadgeSymbol
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HeroBadgeTextColor
Hero badge symbol color.

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

### -HeroBadgeTop
Hero badge top fill color.

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

### -LayoutPreset
Responsive side-rail sizing preset.

```yaml
Type: BgInfoVisualCanvasLayoutPreset
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Default, Compact, Comfortable, WideRails, Dense

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LeftTileOffsetX
Horizontal left side-rail offset in pixels.

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

### -LeftTileOffsetY
Vertical left side-rail offset in pixels.

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

### -LeftTileWidth
Default left side-rail tile width in pixels. Zero uses TileWidth or the template default.

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

### -NoHeroBadge
Hide the central hero badge while keeping the title, subtitle, tiles, and feature strip.

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

### -NoHeroContent
Hide the central hero badge, title, and subtitle while keeping tiles and the feature strip.

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

### -NoTechBackdrop
Disable the built-in technology backdrop.

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

### -Opaque
Render a full ChartForgeX background instead of floating over the wallpaper.

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
Explicit X position on the generated wallpaper.

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
Explicit Y position on the generated wallpaper.

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

### -RightTileOffsetX
Horizontal right side-rail inset in pixels.

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

### -RightTileOffsetY
Vertical right side-rail offset in pixels.

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

### -RightTileWidth
Default right side-rail tile width in pixels. Zero uses TileWidth or the template default.

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

### -SecondaryAccent
Secondary accent color for badge and backdrop highlights.

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

### -Subtitle
Canvas subtitle text.

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

### -SubtitleColor
Subtitle text color.

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

### -Template
Visual canvas template.

```yaml
Type: BgInfoVisualCanvasTemplate
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: PowerBgInfoHero

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Tile
Tile lane definitions.

```yaml
Type: BgInfoVisualCanvasTile[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TileDetailColor
Tile detail text color.

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

### -TileGap
Default vertical gap between tiles in pixels. Zero uses the template default gap.

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

### -TileGlassBottom
Glass tile bottom color.

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

### -TileGlassTop
Glass tile top color.

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

### -TileHeight
Default tile height in pixels. Zero uses the template default height.

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

### -TileLabelColor
Tile label text color.

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

### -TileProgressTrackColor
Tile progress track color.

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

### -TileTextFitPolicy
Default tile text fitting policy.

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

### -TileValueColor
Tile value text color.

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

### -TileWidth
Default tile width in pixels. Zero uses the template default width.

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

### -Title
Canvas title or brand text.

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

### -TitleAccentColor
Accent hero title color.

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

### -TitleColor
Primary hero title color.

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

### -Width
Canvas width in pixels. Zero uses the target wallpaper width.

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

- `PowerBGInfo.BgInfoVisualCanvas`

## RELATED LINKS

- None
