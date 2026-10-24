---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://powerbginfo.com
schema: 2.0.0
---
# New-BGInfoBackgroundGlow
## SYNOPSIS
Creates a soft radial light for New-BGInfoBackground.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoBackgroundGlow -Color <Object> [-X <double>] [-Y <double>] [-Radius <double>] [<CommonParameters>]
```

## DESCRIPTION
Positions are fractions of the backdrop, so the same glow works at any resolution.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoBackgroundGlow -X 0.85 -Y 0.9 -Radius 0.6 -Color '#06B6D455'
```


## PARAMETERS

### -Color
Center color. The glow fades to transparent at the radius, so use alpha to set its strength.

```yaml
Type: Object
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Radius
Radius as a fraction of the longest backdrop side.

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

### -X
Horizontal center from 0 (left edge) to 1 (right edge).

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

### -Y
Vertical center from 0 (top edge) to 1 (bottom edge).

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `PowerBGInfo.BgInfoBackgroundGlow`

## RELATED LINKS

- None
