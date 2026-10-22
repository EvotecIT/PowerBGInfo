param([string] $OutputDirectory = "$env:TEMP\PowerBGInfo")

#region Data: your logic goes here
function Get-MyWallpaperData {
    # A source that never answers: a CMDB that is down, a share that hangs.
    Start-Sleep -Seconds 60
    [pscustomobject] @{ Team = 'too late' }
}

# What the wallpaper shows when the source fails or runs out of time.
$fallback = [pscustomobject] @{
    Team = 'STATUS UNAVAILABLE'; Name = $env:COMPUTERNAME
    User = 'unknown'; Windows = 'unknown'; IPv4 = 'unknown'
    Cpu = @(0); Memory = 0; Volumes = @{ 'C:' = 0 }; Network = @(0)
    Tasks = @(@{ Text = 'Status unknown'; Checked = $false })
}
$source = @{
    Name = 'Wallpaper data'
    TimeoutSeconds = 10
    ScriptBlock = ${function:Get-MyWallpaperData}
    Default = $fallback
}
$data = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $data
$backdrop = @{
    Width = 1920
    Height = 1080
}

New-BGInfo -Theme Aurora -ThemeMode Dark -Target File {
    New-BGInfoBackground @backdrop

    New-BGInfoLabel -Name $data.Team -Accent -FontWeight 700
    New-BGInfoLabel -Name $data.Name -FontSize 64 -Bold
    New-BGInfoValue -Name 'User' -Value $data.User
    New-BGInfoValue -Name 'Windows' -Value $data.Windows
    New-BGInfoValue -Name 'IP address' -Value $data.IPv4
    New-BGInfoValue -Name 'Support' -Value 'ext. 4357'

    New-BGInfoChart -Title 'CPU' -Kind Area -Values $data.Cpu `
        -ValueSuffix ' %' -NoHistory `
        -Region Right
    New-BGInfoMetric -Region Right -Label 'Memory' -Unit '%' `
        -Value $data.Memory -Caption 'of 32 GB' -Height 150 `
        -WarningThreshold 80 -CriticalThreshold 90

    New-BGInfoChart -Region Bottom -Title 'Volumes used' -Kind ProgressBar `
        -Values @($data.Volumes.Values) -Labels @($data.Volumes.Keys) `
        -Maximum 100 `
        -ValueSuffix ' %' -NoProgressHandles -ShowLatestValue:$false `
        -NoHistory -WarningThreshold 80 -CriticalThreshold 90
    New-BGInfoChart -Region Bottom -Title 'Network' -Kind Line `
        -Values $data.Network -ValueSuffix ' Mbps' -NoHistory
    New-BGInfoList -Region Bottom -Title 'This week' -Item $data.Tasks
} -ConfigurationDirectory $OutputDirectory -OutputFileName 'wallpaper.png'
#endregion
