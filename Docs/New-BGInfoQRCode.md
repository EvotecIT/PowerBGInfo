---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoQRCode
## SYNOPSIS
Creates a QR code panel, for example a link to the help-desk portal.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoQRCode [-Text] <string> [-Caption <string>] [-ModuleColor <Object>] [-FieldColor <Object>] [-Title <string>] [-Note <string>] [-Id <string>] [-Width <int>] [-Height <int>] [-Region <BgInfoRegion>] [-Anchor <BgInfoTextPosition>] [-OffsetX <int>] [-OffsetY <int>] [-PositionX <int>] [-PositionY <int>] [-ThemeMode <VisualThemeMode>] [-BackgroundColor <Object>] [-BorderColor <Object>] [-CornerRadius <double>] [-Padding <int>] [-TitleColor <Object>] [-TitleFontSize <float>] [-TitleFontWeight <int>] [-TitleTextCase <TextCaseTransform>] [-TextColor <Object>] [-MutedColor <Object>] [-Accent <Object>] [-FontFamilyName <string>] [-Scale <double>] [<CommonParameters>]
```

## DESCRIPTION
The code is encoded with CodeGlyphX and drawn with whole-pixel modules on a light field so phone cameras read it reliably.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoQRCode -Url 'https://help.contoso.com/new?asset=CT-04217' -Title 'Scan for help' -Caption 'help.contoso.com'
```


## PARAMETERS

### -Accent
Accent for series and highlights.

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

### -Anchor
Anchor used without -Region.

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

### -BackgroundColor
Panel fill.

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

### -BorderColor
Panel outline.

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

### -Caption
Text under the code, for example the short URL.

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

### -CornerRadius
Panel corner radius.

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

### -FieldColor
Field and quiet-zone color.

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

### -FontFamilyName
Font family for content and header.

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
Height in pixels. With -Region and no height, the widget shares the column height or takes the band height.

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

### -Id
Optional identifier.

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

### -ModuleColor
Module color. Keep it dark on a light field.

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

### -MutedColor
Secondary content color.

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

### -Note
Short right-aligned header note, for example a date range or a count.

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

### -Padding
Space between the panel edge and its content.

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

### -PositionX
Absolute X position; use together with -PositionY.

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
Absolute Y position; use together with -PositionX.

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

### -Region
Region to flow into: Left, Right, Top, Bottom or Center.

```yaml
Type: BgInfoRegion
Parameter Sets: __AllParameterSets
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
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Text
Text to encode, usually a URL.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: Url
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TextColor
Primary content color.

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

### -ThemeMode
Light or Dark content.

```yaml
Type: VisualThemeMode
Parameter Sets: __AllParameterSets
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
Header color.

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

### -TitleFontSize
Header font size.

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

### -TitleFontWeight
Header font weight from 100 through 900.

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

### -TitleTextCase
Header casing.

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

### -Width
Width in pixels. With -Region and no width, the widget fills the region width or shares a band.

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

- `PowerBGInfo.BgInfoQrCodeWidget`

## RELATED LINKS

- None
