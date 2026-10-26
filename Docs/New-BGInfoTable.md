---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://powerbginfo.com
schema: 2.0.0
---
# New-BGInfoTable
## SYNOPSIS
Creates a table panel from objects or explicit rows.

## SYNTAX
### Objects (Default)
```powershell
New-BGInfoTable [[-Property] <Object[]>] [-InputObject <Object>] [-StatusColumn <string>] [-StatusMap <Hashtable>] [-Dense] [-NoHeader] [-NoStriping] [-Title <string>] [-Note <string>] [-Id <string>] [-Width <int>] [-Height <int>] [-Region <BgInfoRegion>] [-Anchor <BgInfoTextPosition>] [-OffsetX <int>] [-OffsetY <int>] [-PositionX <int>] [-PositionY <int>] [-ThemeMode <VisualThemeMode>] [-BackgroundColor <Object>] [-BorderColor <Object>] [-CornerRadius <double>] [-Padding <int>] [-TitleColor <Object>] [-TitleFontSize <float>] [-TitleFontWeight <int>] [-TitleTextCase <TextCaseTransform>] [-TextColor <Object>] [-MutedColor <Object>] [-Accent <Object>] [-FontFamilyName <string>] [-Scale <double>] [<CommonParameters>]
```

### Rows
```powershell
New-BGInfoTable -Column <string[]> -Row <Object[]> [-StatusColumn <string>] [-StatusMap <Hashtable>] [-Dense] [-NoHeader] [-NoStriping] [-Title <string>] [-Note <string>] [-Id <string>] [-Width <int>] [-Height <int>] [-Region <BgInfoRegion>] [-Anchor <BgInfoTextPosition>] [-OffsetX <int>] [-OffsetY <int>] [-PositionX <int>] [-PositionY <int>] [-ThemeMode <VisualThemeMode>] [-BackgroundColor <Object>] [-BorderColor <Object>] [-CornerRadius <double>] [-Padding <int>] [-TitleColor <Object>] [-TitleFontSize <float>] [-TitleFontWeight <int>] [-TitleTextCase <TextCaseTransform>] [-TextColor <Object>] [-MutedColor <Object>] [-Accent <Object>] [-FontFamilyName <string>] [-Scale <double>] [<CommonParameters>]
```

## DESCRIPTION
Pipe objects in and pick columns with -Property, or pass -Column and -Row. A -StatusColumn adds a colored dot for values such as OK, Warning or Failed; -StatusMap adds your own words.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-Process | Sort-Object CPU -Descending | Select-Object -First 5 |
    New-BGInfoTable -Region Right -Title 'Top processes' -Property Name, Id, @{ Name = 'CPU s'; Expression = { [math]::Round($_.CPU) } }
```

### EXAMPLE 2
```powershell
@(
    [pscustomobject]@{ User = 'anna'; State = 'Active' }
    [pscustomobject]@{ User = 'piotr'; State = 'Disconnected' }
) | New-BGInfoTable -Region Right -Title 'Sessions' -StatusColumn State -StatusMap @{ Active = 'Ok'; Disconnected = 'Warning' }
```


## PARAMETERS

### -Accent
Accent for series and highlights.

```yaml
Type: Object
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Column
Column headers for explicit rows.

```yaml
Type: String[]
Parameter Sets: Rows
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CornerRadius
Panel corner radius.

```yaml
Type: Double
Parameter Sets: Objects, Rows
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Dense
Use compact rows.

```yaml
Type: SwitchParameter
Parameter Sets: Objects, Rows
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontFamilyName
Font family for content and header.

```yaml
Type: String
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Objects to show, one row each.

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
Parameter Sets: Objects, Rows
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoHeader
Hide the column header row.

```yaml
Type: SwitchParameter
Parameter Sets: Objects, Rows
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoStriping
Do not shade every other row.

```yaml
Type: SwitchParameter
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Property
Columns to read from the objects: property names or calculated properties (@{ Name = ...; Expression = { ... } }). Defaults to the properties of the first object.

```yaml
Type: Object[]
Parameter Sets: Objects
Aliases: None
Possible values:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Region
Region to flow into: Left, Right, Top, Bottom or Center.

```yaml
Type: BgInfoRegion
Parameter Sets: Objects, Rows
Aliases: None
Possible values: None, Left, Right, Top, Bottom, Center

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Row
Explicit rows: arrays of values, or hashtables keyed by column.

```yaml
Type: Object[]
Parameter Sets: Rows
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Scale
Content density from 1 to 4. Larger values enlarge text and marks inside the same panel. Defaults to 1.3 times the theme scale.

```yaml
Type: Double
Parameter Sets: Objects, Rows
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StatusColumn
Column whose values (OK, Warning, Failed...) get a status dot.

```yaml
Type: String
Parameter Sets: Objects, Rows
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StatusMap
Your own status words for the status column, for example @{ Active = 'Ok'; Disconnected = 'Warning'; Listening = 'Info' }. Values are Ok, Warning, Critical or Info. Words not in the map keep their built-in meaning.

```yaml
Type: Hashtable
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
Aliases: None
Possible values: Light, Dark

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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
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
Parameter Sets: Objects, Rows
Aliases: None
Possible values: None, Uppercase, Lowercase, TitleCase, SentenceCase, ToggleCase

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
Parameter Sets: Objects, Rows
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

- `PowerBGInfo.BgInfoTableWidget`

## RELATED LINKS

- None
