param([string] $OutputDirectory = "$env:TEMP\PowerBGInfo")

$backdrop = @{
    Width = 1920
    Height = 1080
    Gradient = '#0B3B46', '#050A14'
    Glow = @(
        New-BGInfoBackgroundGlow -X 0.85 -Y 0.1 -Radius 0.6 -Color '#2DD4BF40'
        New-BGInfoBackgroundGlow -X 0.05 -Y 0.95 -Radius 0.5 -Color '#3B82F633'
    )
    Pattern = 'Grid'
    PatternSpacing = 64
}

New-BGInfo -Target File {
    New-BGInfoBackground @backdrop

    New-BGInfoLabel -Name 'FINANCE · FLOOR 3' -Color '#2DD4BF' -FontWeight 700
    New-BGInfoLabel -Name 'WS-FIN-0042' -Color White -FontSize 64 -Bold
    New-BGInfoValue -Name 'User' -Value 'CONTOSO\anowak'
    New-BGInfoValue -Name 'Windows' -Value 'Windows 11 Enterprise'
    New-BGInfoValue -Name 'IP address' -Value '10.20.4.37'
    New-BGInfoValue -Name 'Support' -Value 'ext. 4357'
} -Color '#94A3B8' -ValueColor White -FontSize 18 -SpaceX 64 -SpaceY 56 `
    -ConfigurationDirectory $OutputDirectory -OutputFileName 'wallpaper.png'
