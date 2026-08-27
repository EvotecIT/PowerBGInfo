---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoValue
## SYNOPSIS
Creates a BGInfo value entry.

## SYNTAX
### Values (Default)
```powershell
New-BGInfoValue -Name <string> -Value <string> [-Color <Object>] [-FontSize <float>] [-FontFamilyName <string>] [-Bold] [-FontWeight <int>] [-Italic] [-Underline] [-UnderlineStyle <TextDecorationStyle>] [-StrikethroughStyle <TextDecorationStyle>] [-Baseline <TextBaseline>] [-TextCase <TextCaseTransform>] [-ValueColor <Object>] [-ValueFontSize <float>] [-ValueFontFamilyName <string>] [-ValueBold] [-ValueFontWeight <int>] [-ValueItalic] [-ValueUnderline] [-ValueUnderlineStyle <TextDecorationStyle>] [-ValueStrikethroughStyle <TextDecorationStyle>] [-ValueBaseline <TextBaseline>] [-ValueTextCase <TextCaseTransform>] [<CommonParameters>]
```

### Builtin
```powershell
New-BGInfoValue -BuiltinValue <string> [-Name <string>] [-Color <Object>] [-FontSize <float>] [-FontFamilyName <string>] [-Bold] [-FontWeight <int>] [-Italic] [-Underline] [-UnderlineStyle <TextDecorationStyle>] [-StrikethroughStyle <TextDecorationStyle>] [-Baseline <TextBaseline>] [-TextCase <TextCaseTransform>] [-ValueColor <Object>] [-ValueFontSize <float>] [-ValueFontFamilyName <string>] [-ValueBold] [-ValueFontWeight <int>] [-ValueItalic] [-ValueUnderline] [-ValueUnderlineStyle <TextDecorationStyle>] [-ValueStrikethroughStyle <TextDecorationStyle>] [-ValueBaseline <TextBaseline>] [-ValueTextCase <TextCaseTransform>] [<CommonParameters>]
```

### Template
```powershell
New-BGInfoValue -Name <string> -Value <string> -ForEach <string> [-Color <Object>] [-FontSize <float>] [-FontFamilyName <string>] [-Bold] [-FontWeight <int>] [-Italic] [-Underline] [-UnderlineStyle <TextDecorationStyle>] [-StrikethroughStyle <TextDecorationStyle>] [-Baseline <TextBaseline>] [-TextCase <TextCaseTransform>] [-ValueColor <Object>] [-ValueFontSize <float>] [-ValueFontFamilyName <string>] [-ValueBold] [-ValueFontWeight <int>] [-ValueItalic] [-ValueUnderline] [-ValueUnderlineStyle <TextDecorationStyle>] [-ValueStrikethroughStyle <TextDecorationStyle>] [-ValueBaseline <TextBaseline>] [-ValueTextCase <TextCaseTransform>] [<CommonParameters>]
```

## DESCRIPTION
Creates a BGInfo value entry.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoValue -Name 'Name' -Value 'Value'
```


### EXAMPLE 2
```powershell
New-BGInfoValue -BuiltinValue 'Value'
```


### EXAMPLE 3
```powershell
New-BGInfoValue -Name 'Name' -Value 'Value' -ForEach 'Value'
```


## PARAMETERS

### -Baseline
Subscript or superscript placement for the label.

```yaml
Type: TextBaseline
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values: Normal, Superscript, Subscript

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Bold
Render the label with a bold font weight.

```yaml
Type: SwitchParameter
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BuiltinValue
Built-in token to resolve to a value.

```yaml
Type: String
Parameter Sets: Builtin
Aliases: None
Possible values: UserName, HostName, FullUserName, CpuName, CpuMaxClockSpeed, CpuCores, CpuLogicalCores, RAMSize, RAMSpeed, RAMPartNumber, BiosVersion, BiosManufacturer, BiosReleaseDate, OSName, OSVersion, OSArchitecture, OSBuild, OSInstallDate, OSLastBootUpTime, UserDNSDomain, FQDN, IPv4Address, IPv6Address

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Color
Label color override.

```yaml
Type: Object
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontFamilyName
Label font family override.

```yaml
Type: String
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontSize
Label font size override.

```yaml
Type: Single
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontWeight
Numeric label font weight from 100 through 900.

```yaml
Type: Int32
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ForEach
Variable name used to expand this entry multiple times.

```yaml
Type: String
Parameter Sets: Template
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Italic
Render the label with italic text.

```yaml
Type: SwitchParameter
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Label text to render.

```yaml
Type: String
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StrikethroughStyle
Strikethrough pattern for the label.

```yaml
Type: TextDecorationStyle
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TextCase
Display-time casing transform for the label.

```yaml
Type: TextCaseTransform
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values: None, Uppercase, Lowercase, TitleCase, SentenceCase, ToggleCase

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Underline
Underline the label.

```yaml
Type: SwitchParameter
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UnderlineStyle
Underline pattern for the label.

```yaml
Type: TextDecorationStyle
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Value
Explicit value to render.

```yaml
Type: String
Parameter Sets: Values, Template
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueBaseline
Subscript or superscript placement for the value.

```yaml
Type: TextBaseline
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values: Normal, Superscript, Subscript

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueBold
Render the value with a bold font weight.

```yaml
Type: SwitchParameter
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueColor
Value color override.

```yaml
Type: Object
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueFontFamilyName
Value font family override.

```yaml
Type: String
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueFontSize
Value font size override.

```yaml
Type: Single
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueFontWeight
Numeric value font weight from 100 through 900.

```yaml
Type: Int32
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueItalic
Render the value with italic text.

```yaml
Type: SwitchParameter
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueStrikethroughStyle
Strikethrough pattern for the value.

```yaml
Type: TextDecorationStyle
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueTextCase
Display-time casing transform for the value.

```yaml
Type: TextCaseTransform
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values: None, Uppercase, Lowercase, TitleCase, SentenceCase, ToggleCase

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueUnderline
Underline the value.

```yaml
Type: SwitchParameter
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueUnderlineStyle
Underline pattern for the value.

```yaml
Type: TextDecorationStyle
Parameter Sets: Values, Builtin, Template
Aliases: None
Possible values: None, Single, Double, Dotted, Dashed, Wavy

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

- `PowerBGInfo.BgInfoEntry`

## RELATED LINKS

- None
