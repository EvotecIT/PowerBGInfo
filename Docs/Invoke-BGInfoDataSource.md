---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://github.com/EvotecIT/PowerBGInfo
schema: 2.0.0
---
# Invoke-BGInfoDataSource
## SYNOPSIS
Runs your data-collection code with a time limit and a fallback value.

## SYNTAX
### __AllParameterSets
```powershell
Invoke-BGInfoDataSource [-ScriptBlock] <scriptblock> [-Name <string>] [-Default <Object>] [-TimeoutSeconds <int>] [-ArgumentList <Object[]>] [<CommonParameters>]
```

## DESCRIPTION
A wallpaper must never fail or hang at logon because one source is slow or down. The script block runs in its own runspace; when it throws, returns nothing or exceeds -TimeoutSeconds, the cmdlet writes a warning and returns -Default instead.

Variables of the calling script are not visible inside the script block. Pass what it needs with -ArgumentList and read it with param().

## EXAMPLES

### EXAMPLE 1
```powershell
$patch = Invoke-BGInfoDataSource -Name 'Patch level' -TimeoutSeconds 10 -Default 'unknown' -ScriptBlock {
    (Get-HotFix | Sort-Object InstalledOn -Descending | Select-Object -First 1).HotFixID
}
$tickets = Invoke-BGInfoDataSource -Name 'Open tickets' -Default 0 -ArgumentList $uri -ScriptBlock {
    param($uri) (Invoke-RestMethod -Uri $uri -TimeoutSec 5).count
}
```


## PARAMETERS

### -ArgumentList
Arguments passed to the script block's param() block.

```yaml
Type: Object[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Default
Value returned when the code fails, returns nothing or runs out of time.

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

### -Name
Name used in warnings, for example 'Patch level'.

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

### -ScriptBlock
Your data-collection code.

```yaml
Type: ScriptBlock
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TimeoutSeconds
Time limit in seconds, from 1 to 600.

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

- `System.Object`

## RELATED LINKS

- None
