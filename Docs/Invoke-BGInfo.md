---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# Invoke-BGInfo
## SYNOPSIS
Runs BGInfo from a JSON configuration file.

## SYNTAX
### __AllParameterSets
```powershell
Invoke-BGInfo [-Path] <string> [-OutputFileName <string>] [-ConfigurationDirectory <string>] [-MonitorIndex <int>] [-Target <BgInfoTarget>] [-NoApply] [<CommonParameters>]
```

## DESCRIPTION
Runs BGInfo from a JSON configuration file.

## EXAMPLES

### EXAMPLE 1
```powershell
Invoke-BGInfo -Path 'C:\Path'
```


## PARAMETERS

### -ConfigurationDirectory
Override configuration output directory.

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

### -MonitorIndex
Override monitor index.

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

### -NoApply
Generate the image without applying it to the wallpaper.

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

### -OutputFileName
Override output file name.

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

### -Path
Path to the JSON configuration file.

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

### -Target
Override output target.

```yaml
Type: BgInfoTarget
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Wallpaper, LogonScreen, Both, File

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

- `System.String`

## RELATED LINKS

- None
