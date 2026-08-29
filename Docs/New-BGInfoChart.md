---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoChart
## SYNOPSIS
Creates a BGInfo chart definition.

## SYNTAX
### Single (Default)
```powershell
New-BGInfoChart [-Title <string>] [-Id <string>] [-Kind <BgInfoChartKind>] [-Value <double>] [-Labels <string[]>] [-Target <double>] [-RangeEnds <double[]>] [-Metric <BgInfoChartMetric>] [-MetricArgument <string>] [-Width <int>] [-Height <int>] [-Anchor <BgInfoTextPosition>] [-OffsetX <int>] [-OffsetY <int>] [-PositionX <int>] [-PositionY <int>] [-MaxPoints <int>] [-NoHistory] [-ReplaceHistory] [-LineColor <Object>] [-FillColor <Object>] [-Palette <Object[]>] [-BackgroundColor <Object>] [-TextColor <Object>] [-TitleColor <Object>] [-ValueColor <Object>] [-FontFamilyName <string>] [-TitleFontSize <float>] [-ValueFontSize <float>] [-TitleBold] [-TitleFontWeight <int>] [-TitleItalic] [-TitleUnderline] [-TitleUnderlineStyle <TextDecorationStyle>] [-TitleStrikethroughStyle <TextDecorationStyle>] [-TitleBaseline <TextBaseline>] [-TitleTextCase <TextCaseTransform>] [-ValueBold] [-ValueFontWeight <int>] [-ValueItalic] [-ValueUnderline] [-ValueUnderlineStyle <TextDecorationStyle>] [-ValueStrikethroughStyle <TextDecorationStyle>] [-ValueBaseline <TextBaseline>] [-ValueTextCase <TextCaseTransform>] [-ShowLatestValue] [-ValueFormat <string>] [-ValueSuffix <string>] [-BarGap <float>] [-Padding <int>] [-ShowGrid] [-GridColor <Object>] [-GridLineCount <int>] [-ShowLegend] [-ShowPointLegend] [-LegendPosition <BgInfoChartLegendPosition>] [-ShowDataLabels] [-Minimum <double>] [-Maximum <double>] [-NoDonutCenterLabel] [-DonutInnerRadiusRatio <double>] [-DonutCenterValue <string>] [-DonutCenterLabel <string>] [-NoRadialBarCenterLabel] [-NoCircleStatusLabel] [-NoProgressValues] [-NoProgressHandles] [-ProgressBarThicknessRatio <double>] [-PictorialSymbol <BgInfoChartPictorialSymbol>] [-PictorialColumns <int>] [<CommonParameters>]
```

### Multiple
```powershell
New-BGInfoChart [-Title <string>] [-Id <string>] [-Kind <BgInfoChartKind>] [-Values <double[]>] [-Labels <string[]>] [-Target <double>] [-RangeEnds <double[]>] [-Metric <BgInfoChartMetric>] [-MetricArgument <string>] [-Width <int>] [-Height <int>] [-Anchor <BgInfoTextPosition>] [-OffsetX <int>] [-OffsetY <int>] [-PositionX <int>] [-PositionY <int>] [-MaxPoints <int>] [-NoHistory] [-ReplaceHistory] [-LineColor <Object>] [-FillColor <Object>] [-Palette <Object[]>] [-BackgroundColor <Object>] [-TextColor <Object>] [-TitleColor <Object>] [-ValueColor <Object>] [-FontFamilyName <string>] [-TitleFontSize <float>] [-ValueFontSize <float>] [-TitleBold] [-TitleFontWeight <int>] [-TitleItalic] [-TitleUnderline] [-TitleUnderlineStyle <TextDecorationStyle>] [-TitleStrikethroughStyle <TextDecorationStyle>] [-TitleBaseline <TextBaseline>] [-TitleTextCase <TextCaseTransform>] [-ValueBold] [-ValueFontWeight <int>] [-ValueItalic] [-ValueUnderline] [-ValueUnderlineStyle <TextDecorationStyle>] [-ValueStrikethroughStyle <TextDecorationStyle>] [-ValueBaseline <TextBaseline>] [-ValueTextCase <TextCaseTransform>] [-ShowLatestValue] [-ValueFormat <string>] [-ValueSuffix <string>] [-BarGap <float>] [-Padding <int>] [-ShowGrid] [-GridColor <Object>] [-GridLineCount <int>] [-ShowLegend] [-ShowPointLegend] [-LegendPosition <BgInfoChartLegendPosition>] [-ShowDataLabels] [-Minimum <double>] [-Maximum <double>] [-NoDonutCenterLabel] [-DonutInnerRadiusRatio <double>] [-DonutCenterValue <string>] [-DonutCenterLabel <string>] [-NoRadialBarCenterLabel] [-NoCircleStatusLabel] [-NoProgressValues] [-NoProgressHandles] [-ProgressBarThicknessRatio <double>] [-PictorialSymbol <BgInfoChartPictorialSymbol>] [-PictorialColumns <int>] [<CommonParameters>]
```

## DESCRIPTION
Creates a BGInfo chart definition.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoChart -Anchor 'Value'
```


## PARAMETERS

### -Anchor
Anchor position for placement.

```yaml
Type: BgInfoTextPosition
Parameter Sets: Single, Multiple
Aliases: None
Possible values: TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BackgroundColor
Background color for the chart block.

```yaml
Type: Object
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BarGap
Gap between bars (0-1).

```yaml
Type: Single
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DonutCenterLabel
Donut center label text.

```yaml
Type: String
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DonutCenterValue
Donut center value text.

```yaml
Type: String
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DonutInnerRadiusRatio
Donut inner radius ratio.

```yaml
Type: Double
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FillColor
Fill color for sparklines.

```yaml
Type: Object
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontFamilyName
Font family for title and value.

```yaml
Type: String
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GridColor
Grid line color.

```yaml
Type: Object
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GridLineCount
Number of horizontal grid lines.

```yaml
Type: Int32
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Height
Chart height in pixels.

```yaml
Type: Int32
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
Chart identifier used for history storage.

```yaml
Type: String
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Kind
Chart kind to render, such as Sparkline, Line, Area, Bar, HorizontalBar, Gauge, Circle, RadialBar, Bullet, Pie, Donut, ProgressBar, or Pictorial.

```yaml
Type: BgInfoChartKind
Parameter Sets: Single, Multiple
Aliases: None
Possible values: Sparkline, Line, Area, Bar, HorizontalBar, Gauge, Circle, RadialBar, Bullet, Pie, Donut, ProgressBar, Pictorial

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Labels
Optional labels used by point-based charts.

```yaml
Type: String[]
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LegendPosition
Chart legend position.

```yaml
Type: BgInfoChartLegendPosition
Parameter Sets: Single, Multiple
Aliases: None
Possible values: Bottom, Top, Left, Right

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LineColor
Line or bar color.

```yaml
Type: Object
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Maximum
Optional maximum scale value.

```yaml
Type: Double
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaxPoints
Maximum number of samples to keep in history.

```yaml
Type: Int32
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Metric
Built-in metric source used when no explicit values are provided.

```yaml
Type: BgInfoChartMetric
Parameter Sets: Single, Multiple
Aliases: None
Possible values: None, CpuPercent, MemoryPercent, DiskFreePercent, DiskUsedPercent, DiskFreeGb, UptimeHours, UptimeDays

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MetricArgument
Optional metric argument (for example drive letter).

```yaml
Type: String
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Minimum
Optional minimum scale value.

```yaml
Type: Double
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoCircleStatusLabel
Hide circle status label.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoDonutCenterLabel
Hide donut center label.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoHistory
Disable history storage and render only provided values.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoProgressHandles
Hide progress handles.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoProgressValues
Hide progress values.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoRadialBarCenterLabel
Hide radial-bar center label.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
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
Parameter Sets: Single, Multiple
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
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Padding
Padding inside the chart.

```yaml
Type: Int32
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Palette
Palette colors for point-based charts.

```yaml
Type: Object[]
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PictorialColumns
Pictorial symbols per row.

```yaml
Type: Int32
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PictorialSymbol
Pictorial chart symbol.

```yaml
Type: BgInfoChartPictorialSymbol
Parameter Sets: Single, Multiple
Aliases: None
Possible values: Circle, Square, Diamond, Triangle, Star, Person

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PositionX
Absolute X position for placement.

```yaml
Type: Int32
Parameter Sets: Single, Multiple
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
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressBarThicknessRatio
Progress-bar thickness ratio.

```yaml
Type: Double
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RangeEnds
Qualitative range ends used by bullet charts.

```yaml
Type: Double[]
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ReplaceHistory
Replace history instead of appending values.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShowDataLabels
Show supported data labels.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShowGrid
Show chart grid lines.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShowLatestValue
Show the latest value text.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShowLegend
Show the chart legend.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShowPointLegend
Show point-level legend entries.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Target
Target value used by bullet charts.

```yaml
Type: Double
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TextColor
Text color for title/value.

```yaml
Type: Object
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Title
Chart title displayed above the plot.

```yaml
Type: String
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleBaseline
Subscript or superscript placement for the chart title.

```yaml
Type: TextBaseline
Parameter Sets: Single, Multiple
Aliases: None
Possible values: Normal, Superscript, Subscript

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleBold
Render the chart title with a bold font weight.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleColor
Independent chart title color.

```yaml
Type: Object
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleFontSize
Title font size.

```yaml
Type: Single
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleFontWeight
Numeric title font weight from 100 through 900.

```yaml
Type: Int32
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleItalic
Render the chart title with italic text.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleStrikethroughStyle
Strikethrough pattern for the chart title.

```yaml
Type: TextDecorationStyle
Parameter Sets: Single, Multiple
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleTextCase
Display-time casing transform for the chart title.

```yaml
Type: TextCaseTransform
Parameter Sets: Single, Multiple
Aliases: None
Possible values: None, Uppercase, Lowercase, TitleCase, SentenceCase, ToggleCase

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleUnderline
Underline the chart title.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleUnderlineStyle
Underline pattern for the chart title.

```yaml
Type: TextDecorationStyle
Parameter Sets: Single, Multiple
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Value
Single value to append.

```yaml
Type: Double
Parameter Sets: Single
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueBaseline
Subscript or superscript placement for the latest value.

```yaml
Type: TextBaseline
Parameter Sets: Single, Multiple
Aliases: None
Possible values: Normal, Superscript, Subscript

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueBold
Render the latest value with a bold font weight.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueColor
Independent latest-value color.

```yaml
Type: Object
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueFontSize
Value font size.

```yaml
Type: Single
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueFontWeight
Numeric latest-value font weight from 100 through 900.

```yaml
Type: Int32
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueFormat
Format string for the latest value.

```yaml
Type: String
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueItalic
Render the latest value with italic text.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Values
Multiple values to append or replace.

```yaml
Type: Double[]
Parameter Sets: Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueStrikethroughStyle
Strikethrough pattern for the latest value.

```yaml
Type: TextDecorationStyle
Parameter Sets: Single, Multiple
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueSuffix
Suffix appended to the latest value.

```yaml
Type: String
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueTextCase
Display-time casing transform for the latest value.

```yaml
Type: TextCaseTransform
Parameter Sets: Single, Multiple
Aliases: None
Possible values: None, Uppercase, Lowercase, TitleCase, SentenceCase, ToggleCase

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueUnderline
Underline the latest value.

```yaml
Type: SwitchParameter
Parameter Sets: Single, Multiple
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueUnderlineStyle
Underline pattern for the latest value.

```yaml
Type: TextDecorationStyle
Parameter Sets: Single, Multiple
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Width
Chart width in pixels.

```yaml
Type: Int32
Parameter Sets: Single, Multiple
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

- `PowerBGInfo.BgInfoChart`

## RELATED LINKS

- None
