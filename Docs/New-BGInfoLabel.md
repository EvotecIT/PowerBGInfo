---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version:
schema: 2.0.0
---

# New-BGInfoLabel

## SYNOPSIS
Provides ability to set label without value.
It can be used to separate different sections of information.

## SYNTAX

```
New-BGInfoLabel [[-Name] <String>] [[-Color] <Color>] [[-FontSize] <Single>] [[-FontFamilyName] <String>]
 [<CommonParameters>]
```

## DESCRIPTION
Provides ability to set label without value.
It can be used to separate different sections of information.

## EXAMPLES

### EXAMPLE 1
```
# Lets add Label, but without any values, kinf of like section starting


New-BGInfoLabel -Name "Drives" -Color LemonChiffon -FontSize 16 -FontFamilyName 'Calibri'
```

## PARAMETERS

### -Name
Name of the label/section

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Color
Color for the label.
If not provided it will be taken from the parent New-BGInfo command.

```yaml
Type: Color
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontSize
Font size for the label.
If not provided it will be taken from the parent New-BGInfo command.

```yaml
Type: Single
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontFamilyName
Font family name for the label.
If not provided it will be taken from the parent New-BGInfo command.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 4
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES
General notes

## RELATED LINKS
