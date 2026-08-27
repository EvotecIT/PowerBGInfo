---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoLabel
## SYNOPSIS
Creates a BGInfo label entry.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoLabel -Name <string> [-ForEach <string>] [-Color <Object>] [-FontSize <float>] [-FontFamilyName <string>] [-Bold] [-FontWeight <int>] [-Italic] [-Underline] [-UnderlineStyle <TextDecorationStyle>] [-StrikethroughStyle <TextDecorationStyle>] [-Baseline <TextBaseline>] [-TextCase <TextCaseTransform>] [<CommonParameters>]
```

## DESCRIPTION
Creates a BGInfo label entry.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoLabel -Name 'Name'
```


## PARAMETERS

### -Baseline
Subscript or superscript placement for the label.

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

### -Bold
Render the label with a bold font weight.

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

### -Color
Label color override.

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
Label font family override.

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
Label font size override.

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

### -FontWeight
Numeric label font weight from 100 through 900.

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

### -ForEach
Variable name used to expand this label multiple times.

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

### -Italic
Render the label with italic text.

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

### -Name
Label text to render.

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

### -StrikethroughStyle
Strikethrough pattern for the label.

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

### -TextCase
Display-time casing transform for the label.

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

### -Underline
Underline the label.

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
Underline pattern for the label.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `PowerBGInfo.BgInfoEntry`

## RELATED LINKS

- None
