---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# New-BGInfoVariable
## SYNOPSIS
Creates a reusable BGInfo variable backed by a built-in provider.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoVariable -Name <string> -Provider <BgInfoVariableProvider> [-Argument <string>] [<CommonParameters>]
```

## DESCRIPTION
Creates a reusable BGInfo variable backed by a built-in provider.

## EXAMPLES

### EXAMPLE 1
```powershell
New-BGInfoVariable -Name 'Name' -Provider 'Value'
```


## PARAMETERS

### -Argument
Optional provider argument for filtering/customization.

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

### -Name
Name used by -ForEach references.

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

### -Provider
Built-in provider used to populate the variable.

```yaml
Type: BgInfoVariableProvider
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Volumes

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

- `PowerBGInfo.BgInfoVariable`

## RELATED LINKS

- None
