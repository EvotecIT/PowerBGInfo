param([string] $OutputDirectory = "$env:TEMP\PowerBGInfo")

$backdrop = @{
    Width = 1920
    Height = 1080
}

New-BGInfo -Theme Aurora -ThemeMode Dark -Target File {
    New-BGInfoBackground @backdrop

    New-BGInfoLabel -Name 'FINANCE · FLOOR 3' -Accent -FontWeight 700
    New-BGInfoLabel -Name 'WS-FIN-0042' -FontSize 64 -Bold
    New-BGInfoValue -Name 'User' -Value 'CONTOSO\anowak'
    New-BGInfoValue -Name 'Windows' -Value 'Windows 11 Enterprise'
    New-BGInfoValue -Name 'IP address' -Value '10.20.4.37'
    New-BGInfoValue -Name 'Support' -Value 'ext. 4357'
} -ConfigurationDirectory $OutputDirectory -OutputFileName 'wallpaper.png'
