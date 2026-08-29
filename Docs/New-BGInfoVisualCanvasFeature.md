---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoVisualCanvasFeature
## SYNOPSIS
Creates a BGInfo visual canvas feature-strip item.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoVisualCanvasFeature -Label <string> [-Icon <string>] [<CommonParameters>]
```

## DESCRIPTION
Feature-strip items are compact labels shown in the optional visual canvas footer strip.

## EXAMPLES

### EXAMPLE 1
```powershell
$features = @(
    New-BGInfoVisualCanvasFeature -Icon 'A+' -Label 'light contrast boxes'
    New-BGInfoVisualCanvasFeature -Icon 'JSON' -Label 'portable config'
)
```


## PARAMETERS

### -Icon
Compact item icon or symbol.

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

### -Label
Feature label.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `PowerBGInfo.BgInfoVisualCanvasFeature`

## RELATED LINKS

- None
