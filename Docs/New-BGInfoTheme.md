---
external help file: PowerBGInfo-help.xml
Module Name: PowerBGInfo
online version: https://powerbginfo.com
schema: 2.0.0
---
# New-BGInfoTheme
## SYNOPSIS
Creates a wallpaper theme: text, panel, status and backdrop colors plus spacing and type scale.

## SYNTAX
### __AllParameterSets
```powershell
New-BGInfoTheme [[-Preset] <BgInfoThemePreset>] [-Mode <VisualThemeMode>] [-Accent <Object>] [-SecondaryAccent <Object>] [-Scale <double>] [-FontFamilyName <string>] [-Surface <Object>] [-CornerRadius <double>] [<CommonParameters>]
```

## DESCRIPTION
Pass the result, or just a preset name, to New-BGInfo -Theme. Every cmdlet inside the script block then uses the theme for parameters you do not set.

## EXAMPLES

### EXAMPLE 1
```powershell
$theme = New-BGInfoTheme -Preset Aurora -Mode Light -Accent '#0EA5E9' -Scale 1.33
New-BGInfo -Theme $theme -Target File {
    New-BGInfoBackground -Width 2560 -Height 1440
    New-BGInfoValue -Name 'Host' -Value $env:COMPUTERNAME
    New-BGInfoChart -Region Bottom -Title 'CPU' -Kind Area -Values 12, 30, 22
} -ConfigurationDirectory $PSScriptRoot -OutputFileName 'themed.png'
```


## PARAMETERS

### -Accent
Accent that replaces the preset accent.

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

### -CornerRadius
Panel corner radius before scaling.

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

### -FontFamilyName
Font family for text, panel headers and widgets.

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

### -Mode
Light or Dark variant.

```yaml
Type: VisualThemeMode
Parameter Sets: __AllParameterSets
Aliases: ThemeMode
Possible values: Light, Dark

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Preset
Named look: Graphite, Midnight, Aurora, Ember, Orchid or Corporate.

```yaml
Type: BgInfoThemePreset
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Graphite, Midnight, Aurora, Ember, Orchid, Corporate

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Scale
Multiplier for type, padding, radius and margins, from 0.5 to 4. Use about 1.33 for 2560x1440 and 2 for 3840x2160.

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

### -SecondaryAccent
Second accent for contrasting series.

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

### -Surface
Panel fill that replaces the preset glass.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `PowerBGInfo.BgInfoTheme`

## RELATED LINKS

- None
