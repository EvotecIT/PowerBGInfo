---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# Export-BGInfoConfiguration
## SYNOPSIS
Exports a BGInfo configuration to JSON.

## SYNTAX
### __AllParameterSets
```powershell
Export-BGInfoConfiguration [-Path] <string> -InputObject <BgInfoConfiguration> [-Force] [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Writes a JSON file compatible with Invoke-BGInfo and the CLI.

## EXAMPLES

### EXAMPLE 1
```powershell
Export-BGInfoConfiguration -InputObject 'Value'
```


## PARAMETERS

### -Force
Overwrite the output file if it exists.

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

### -InputObject
Configuration object to export.

```yaml
Type: BgInfoConfiguration
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -PassThru
Return the output path.

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

### -Path
Output path for the JSON configuration file.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `PowerBGInfo.BgInfoConfiguration`

## OUTPUTS

- `System.String`

## RELATED LINKS

- None
