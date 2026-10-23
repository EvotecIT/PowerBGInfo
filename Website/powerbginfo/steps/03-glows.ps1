param([string] $OutputDirectory = "$env:TEMP\PowerBGInfo")

$backdrop = @{
    Width = 1920
    Height = 1080
    Gradient = '#0B3B46', '#050A14'
    Glow = @(
        New-BGInfoBackgroundGlow -X 0.85 -Y 0.1 -Radius 0.6 -Color '#2DD4BF40'
        New-BGInfoBackgroundGlow -X 0.05 -Y 0.95 -Radius 0.5 -Color '#3B82F633'
    )
}

New-BGInfo -Target File {
    New-BGInfoBackground @backdrop
} -ConfigurationDirectory $OutputDirectory -OutputFileName 'wallpaper.png'
