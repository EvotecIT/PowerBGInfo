---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version:
schema: 2.0.0
---

# New-BGInfoValue

## SYNOPSIS
Special function that provides a way to create a value that will be displayed on the background image.

## SYNTAX

### Values (Default)
```
New-BGInfoValue -Name <String> -Value <String> [-Color <Color>] [-FontSize <Single>] [-FontFamilyName <String>]
 [-ValueColor <Color>] [-ValueFontSize <Single>] [-ValueFontFamilyName <String>] [<CommonParameters>]
```

### Builtin
```
New-BGInfoValue [-Name <String>] -BuiltinValue <String> [-Color <Color>] [-FontSize <Single>]
 [-FontFamilyName <String>] [-ValueColor <Color>] [-ValueFontSize <Single>] [-ValueFontFamilyName <String>]
 [<CommonParameters>]
```

## DESCRIPTION
Special function that provides a way to create a value that will be displayed on the background image.
It allows using builtin values, or custom values depending on user needs.

## EXAMPLES

### EXAMPLE 1
```
New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20 -FontFamilyName 'Calibri'


New-BGInfoValue -BuiltinValue FullUserName
New-BGInfoValue -BuiltinValue CpuName
```

### EXAMPLE 2
```
# Lets get all drives and their labels


foreach ($Disk in (Get-Disk)) {
    $Volumes = $Disk | Get-Partition | Get-Volume
    foreach ($V in $Volumes) {
        New-BGInfoValue -Name "Drive $($V.DriveLetter)" -Value $V.SizeRemaining
    }
}
```

## PARAMETERS

### -Name
Label that will be used on the left side of the value.

```yaml
Type: String
Parameter Sets: Values
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: Builtin
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Value
Cystom Value that will be displayed on the right side of the label.

```yaml
Type: String
Parameter Sets: Values
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BuiltinValue
Builtin value that will be displayed on the right side of the label.
It can be one of the following:
- UserName - Current user name
- HostName - Current host name
- FullUserName - Current user name with domain
- CpuName - CPU name
- CpuMaxClockSpeed - CPU max clock speed
- CpuCores - CPU cores
- CpuLogicalCores - CPU logical cores
- RAMSize - RAM size
- RAMSpeed - RAM speed
- RAMPartNumber - RAM part number
- BiosVersion - BIOS version
- BiosManufacturer - BIOS manufacturer
- BiosReleaseDate - BIOS release date
- OSName - OS name
- OSVersion - OS version
- OSArchitecture - OS architecture
- OSBuild - OS build
- OSInstallDate - OS install date
- OSLastBootUpTime - OS last boot up time
- UserDNSDomain - User DNS domain
- FQDN - Fully qualified domain name

```yaml
Type: String
Parameter Sets: Builtin
Aliases:

Required: True
Position: Named
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
Position: Named
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
Position: Named
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
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueColor
Color for the value.
If not provided it will be taken first from Color of the label and if that is not provided from the parent New-BGInfo command.

```yaml
Type: Color
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueFontSize
Font size for the value.
If not provided it will be taken first from FontSize of the label and if that is not provided from the parent New-BGInfo command.

```yaml
Type: Single
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueFontFamilyName
Font family name for the value.
If not provided it will be taken first from FontFamilyName of the label and if that is not provided from the parent New-BGInfo command.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
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
