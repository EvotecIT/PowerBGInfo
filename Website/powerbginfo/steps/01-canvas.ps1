param([string] $OutputDirectory = "$env:TEMP\PowerBGInfo")

$backdrop = @{
    Width = 1920
    Height = 1080
    Color = '#0B1222'
}

New-BGInfo -Target File {
    New-BGInfoBackground @backdrop
} -ConfigurationDirectory $OutputDirectory -OutputFileName 'wallpaper.png'
