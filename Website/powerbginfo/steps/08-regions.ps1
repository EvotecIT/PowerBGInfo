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

    New-BGInfoChart -Title 'CPU' -Kind Area -Values 12, 18, 25, 22, 30, 27 `
        -ValueSuffix ' %' -NoHistory `
        -Region Right
    New-BGInfoMetric -Region Right -Label 'Memory' -Value 86 -Unit '%' `
        -Caption 'of 32 GB' -Height 150

    New-BGInfoChart -Region Bottom -Title 'Volumes used' -Kind ProgressBar `
        -Values 63, 93 -Labels 'C: System', 'D: Data' -Maximum 100 `
        -ValueSuffix ' %' -NoProgressHandles -ShowLatestValue:$false `
        -NoHistory
    New-BGInfoChart -Region Bottom -Title 'Network' -Kind Line `
        -Values 120, 180, 160, 220, 260, 240 -ValueSuffix ' Mbps' -NoHistory
    New-BGInfoList -Region Bottom -Title 'This week' -Item @(
        @{ Text = 'Month-end close'; Checked = $false; Value = 'Fri' }
        @{ Text = 'Backups verified'; Checked = $true; Value = '02:10' }
    )
} -ConfigurationDirectory $OutputDirectory -OutputFileName 'wallpaper.png'
