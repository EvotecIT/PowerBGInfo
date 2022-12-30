---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version:
schema: 2.0.0
---

# New-BGInfo

## SYNOPSIS
Provides a simple way to create PowerBGInfo configuration.

## SYNTAX

```
New-BGInfo [-BGInfoContent] <ScriptBlock> [[-FilePath] <String>] [-ConfigurationDirectory] <String>
 [[-FontFamilyName] <String>] [[-Color] <Color>] [[-FontSize] <Int32>] [[-ValueColor] <Color>]
 [[-ValueFontSize] <Single>] [[-ValueFontFamilyName] <String>] [[-SpaceBetweenLines] <Int32>]
 [[-SpaceBetweenColumns] <Int32>] [[-PositionX] <Int32>] [[-PositionY] <Int32>] [[-MonitorIndex] <Int32>]
 [[-WallpaperFit] <String>] [<CommonParameters>]
```

## DESCRIPTION
Provides a simple way to create PowerBGInfo configuration.
It allows writting useful information on your desktop background.
Every time the script is run, it will update existing image with new information.

## EXAMPLES

### EXAMPLE 1
```
New-BGInfo -MonitorIndex 0 {


# Lets add computer name, but lets use builtin values for that
    New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20 -FontFamilyName 'Calibri'
    New-BGInfoValue -BuiltinValue FullUserName
    New-BGInfoValue -BuiltinValue CpuName
    New-BGInfoValue -BuiltinValue CpuLogicalCores
    New-BGInfoValue -BuiltinValue RAMSize
    New-BGInfoValue -BuiltinValue RAMSpeed

    # Lets add Label, but without any values, kinf of like section starting
    New-BGInfoLabel -Name "Drives" -Color LemonChiffon -FontSize 16 -FontFamilyName 'Calibri'

    # Lets get all drives and their labels
    foreach ($Disk in (Get-Disk)) {
        $Volumes = $Disk | Get-Partition | Get-Volume
        foreach ($V in $Volumes) {
            New-BGInfoValue -Name "Drive $($V.DriveLetter)" -Value $V.SizeRemaining
        }
    }
} -FilePath $PSScriptRoot\Samples\PrzemyslawKlysAndKulkozaurr.jpg -ConfigurationDirectory $PSScriptRoot\Output -PositionX 100 -PositionY 100 -WallpaperFit Center
```

## PARAMETERS

### -BGInfoContent
Special parameter that works as a scriptblock.
It takes input and converts it into configuration.
By using New-BGInfoLabel and New-BGInfoValue along with other supported PowerShell commands you can create your own configuration.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FilePath
Path to the image that will be used as a background.
If not provided current Desktop Background will be used.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConfigurationDirectory
Path to the directory where configuration will be stored, and where image for desktop background will be placed.
If not provided, it will be stored in C:\TEMP

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontFamilyName
Font family name that will be used to display information for Label.
It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
If ValueFontFamilyName is not provided it will be used as a default value for that property as well

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 4
Default value: Calibri
Accept pipeline input: False
Accept wildcard characters: False
```

### -Color
Color that will be used to display information for Label.
It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
If ValueColor is not provided it will be used as a default value for that property as well

```yaml
Type: Color
Parameter Sets: (All)
Aliases:

Required: False
Position: 5
Default value: [SixLabors.ImageSharp.Color]::Black
Accept pipeline input: False
Accept wildcard characters: False
```

### -FontSize
Font size that will be used to display information for Label.
It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
If ValueFontSize is not provided it will be used as a default value for that property as well

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 6
Default value: 16
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueColor
Color that will be used to display information for Value.
It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
If not provided it will be taken from Color property.

```yaml
Type: Color
Parameter Sets: (All)
Aliases:

Required: False
Position: 7
Default value: [SixLabors.ImageSharp.Color]::Black
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueFontSize
Font size that will be used to display information for Value.
It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
If not provided it will be taken from FontSize property.

```yaml
Type: Single
Parameter Sets: (All)
Aliases:

Required: False
Position: 8
Default value: 16
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueFontFamilyName
Font family name that will be used to display information for Value.
It's only used if New-BGInfoLabel or New-BGIInfoValue doesn't provide it's own value.
If not provided it will be taken from FontFamilyName property.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 9
Default value: Calibri
Accept pipeline input: False
Accept wildcard characters: False
```

### -SpaceBetweenLines
Length of the space between lines

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 10
Default value: 10
Accept pipeline input: False
Accept wildcard characters: False
```

### -SpaceBetweenColumns
Length of the space between columns (Label and Value)

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 11
Default value: 30
Accept pipeline input: False
Accept wildcard characters: False
```

### -PositionX
Position of the first column on the X axis.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 12
Default value: 10
Accept pipeline input: False
Accept wildcard characters: False
```

### -PositionY
Position of the first column on the Y axis.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 13
Default value: 10
Accept pipeline input: False
Accept wildcard characters: False
```

### -MonitorIndex
Index of the monitor that will be used to display the background image.
By default it will be 0 (first monitor)

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 14
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -WallpaperFit
WHat to do with the image if it is not the same size as the monitor resolution.
It can be one of the following: 'Center', 'Fit', 'Stretch', 'Tile', 'Span', 'Fill'

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 15
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
