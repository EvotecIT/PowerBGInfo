---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://powerbginfo.com
schema: 2.0.0
---
# New-BGInfoTreemap
## SYNOPSIS
Creates a treemap panel: sizes as nested rectangles, for example disk usage by folder or VM.

## SYNTAX
### Objects (Default)
```powershell
New-BGInfoTreemap [-InputObject <Object>] [-NameProperty <string>] [-ValueProperty <string>] [-GroupProperty <string>] [-ValueSuffix <string>] [-NoValues] [-ColorProperty <string>] [-WarningThreshold <double>] [-CriticalThreshold <double>] [-ThresholdDirection <BgInfoThresholdDirection>] [-Title <string>] [-Note <string>] [-Id <string>] [-Width <int>] [-Height <int>] [-Region <BgInfoRegion>] [-Anchor <BgInfoTextPosition>] [-OffsetX <int>] [-OffsetY <int>] [-PositionX <int>] [-PositionY <int>] [-ThemeMode <VisualThemeMode>] [-BackgroundColor <Object>] [-BorderColor <Object>] [-CornerRadius <double>] [-Padding <int>] [-TitleColor <Object>] [-TitleFontSize <float>] [-TitleFontWeight <int>] [-TitleTextCase <TextCaseTransform>] [-TextColor <Object>] [-MutedColor <Object>] [-Accent <Object>] [-FontFamilyName <string>] [-Scale <double>] [<CommonParameters>]
```

### Data
```powershell
New-BGInfoTreemap -Data <IDictionary> [-ValueSuffix <string>] [-NoValues] [-WarningThreshold <double>] [-CriticalThreshold <double>] [-ThresholdDirection <BgInfoThresholdDirection>] [-Title <string>] [-Note <string>] [-Id <string>] [-Width <int>] [-Height <int>] [-Region <BgInfoRegion>] [-Anchor <BgInfoTextPosition>] [-OffsetX <int>] [-OffsetY <int>] [-PositionX <int>] [-PositionY <int>] [-ThemeMode <VisualThemeMode>] [-BackgroundColor <Object>] [-BorderColor <Object>] [-CornerRadius <double>] [-Padding <int>] [-TitleColor <Object>] [-TitleFontSize <float>] [-TitleFontWeight <int>] [-TitleTextCase <TextCaseTransform>] [-TextColor <Object>] [-MutedColor <Object>] [-Accent <Object>] [-FontFamilyName <string>] [-Scale <double>] [<CommonParameters>]
```

## DESCRIPTION
Pipe objects with Name, Value and an optional Group, or pass -Data @{ name = size }.

-ColorProperty colors each rectangle by a second measure; with -WarningThreshold and -CriticalThreshold the colors are the ok, warning and critical status colors.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoTreemap -Region Right -Title 'D: by folder' -ValueSuffix ' GB' -Data ([ordered]@{ Databases = 412; Logs = 96; Backups = 230 })
```


### EXAMPLE 2
```powershell
Get-VM | Select-Object Name, CPUUsage, @{ Name = 'MemoryGB'; Expression = { $_.MemoryAssigned / 1GB } } |
    New-BGInfoTreemap -Region Center -Title 'Memory by VM, colored by CPU' -ValueProperty MemoryGB -ValueSuffix ' GB' `
        -ColorProperty CPUUsage -WarningThreshold 70 -CriticalThreshold 90
```


## PARAMETERS

### -Accent
Accent for series and highlights.

```yaml
Type: Object
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Anchor
Anchor used without -Region.

```yaml
Type: BgInfoTextPosition
Parameter Sets: Objects, Data
Aliases: None
Possible values: TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BackgroundColor
Panel fill.

```yaml
Type: Object
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BorderColor
Panel outline.

```yaml
Type: Object
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ColorProperty
Property with a second number that colors each rectangle, for example CPU while the size is memory.

```yaml
Type: String
Parameter Sets: Objects
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CornerRadius
Panel corner radius.

```yaml
Type: Double
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CriticalThreshold
Color value at which a rectangle takes the critical color. Without -ColorProperty the size is compared.

```yaml
Type: Double
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Data
Sizes as a hashtable: keys are names, values are numbers.

```yaml
Type: IDictionary
Parameter Sets: Data
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontFamilyName
Font family for content and header.

```yaml
Type: String
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GroupProperty
Property that holds the optional group.

```yaml
Type: String
Parameter Sets: Objects
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Height
Height in pixels. With -Region and no height, the widget shares the column height or takes the band height.

```yaml
Type: Int32
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
Optional identifier.

```yaml
Type: String
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Objects with a name, a size and an optional group.

```yaml
Type: Object
Parameter Sets: Objects
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -MutedColor
Secondary content color.

```yaml
Type: Object
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NameProperty
Property that holds the name.

```yaml
Type: String
Parameter Sets: Objects
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Note
Short right-aligned header note, for example a date range or a count.

```yaml
Type: String
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoValues
Hide values inside the rectangles.

```yaml
Type: SwitchParameter
Parameter Sets: Objects, Data
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
Parameter Sets: Objects, Data
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
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Padding
Space between the panel edge and its content.

```yaml
Type: Int32
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PositionX
Absolute X position; use together with -PositionY.

```yaml
Type: Int32
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PositionY
Absolute Y position; use together with -PositionX.

```yaml
Type: Int32
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Region
Region to flow into: Left, Right, Top, Bottom or Center.

```yaml
Type: BgInfoRegion
Parameter Sets: Objects, Data
Aliases: None
Possible values: None, Left, Right, Top, Bottom, Center

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Scale
Content density from 1 to 4. Larger values enlarge text and marks inside the same panel. Defaults to 1.3 times the theme scale.

```yaml
Type: Double
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TextColor
Primary content color.

```yaml
Type: Object
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ThemeMode
Light or Dark content.

```yaml
Type: VisualThemeMode
Parameter Sets: Objects, Data
Aliases: None
Possible values: Light, Dark

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ThresholdDirection
Above: higher values are worse. Below: lower values are worse.

```yaml
Type: BgInfoThresholdDirection
Parameter Sets: Objects, Data
Aliases: None
Possible values: Above, Below

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Title
Panel header text.

```yaml
Type: String
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleColor
Header color.

```yaml
Type: Object
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleFontSize
Header font size.

```yaml
Type: Single
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleFontWeight
Header font weight from 100 through 900.

```yaml
Type: Int32
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TitleTextCase
Header casing.

```yaml
Type: TextCaseTransform
Parameter Sets: Objects, Data
Aliases: None
Possible values: None, Uppercase, Lowercase, TitleCase, SentenceCase, ToggleCase

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueProperty
Property that holds the size.

```yaml
Type: String
Parameter Sets: Objects
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueSuffix
Suffix after values, for example ' GB'.

```yaml
Type: String
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WarningThreshold
Color value at which a rectangle takes the warning color. Without -ColorProperty the size is compared.

```yaml
Type: Double
Parameter Sets: Objects, Data
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Width
Width in pixels. With -Region and no width, the widget fills the region width or shares a band.

```yaml
Type: Int32
Parameter Sets: Objects, Data
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

- `System.Object`

## OUTPUTS

- `PowerBGInfo.BgInfoTreemapWidget`

## RELATED LINKS

- None
