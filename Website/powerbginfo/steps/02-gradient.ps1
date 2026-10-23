param([string] $OutputDirectory = "$env:TEMP\PowerBGInfo")

$backdrop = @{
    Width = 1920
    Height = 1080
    Gradient = '#0B3B46', '#050A14'
}

New-BGInfo -Target File {
    New-BGInfoBackground @backdrop
} -ConfigurationDirectory $OutputDirectory -OutputFileName 'wallpaper.png'
