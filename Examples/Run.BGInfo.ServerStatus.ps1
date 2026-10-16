<#
    Server status wallpaper: who the server is, how long it has been up,
    and whether disks, services and the network need attention.

    The data block returns plain values; the presentation block only reads
    them. Sample values keep the render reproducible, and each one notes the
    command that supplies it on a real server.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output')
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
function Get-ServerStatus {
    [pscustomobject] @{
        Name        = 'SQL-PROD-01'
        Role        = 'Primary replica · AG-FINANCE'
        Owner       = 'Data Platform team'
        Maintenance = 'Sunday 02:00–04:00 UTC'
        OnCall      = 'dba-oncall@contoso.com'
        # (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
        Uptime      = New-TimeSpan -Days 41 -Hours 6
        # Get-Volume | Where-Object DriveLetter
        Volumes     = [ordered] @{ 'C: System' = 61; 'D: Data' = 88; 'L: Logs' = 47 }
        # Get-Counter '\Processor(_Total)\% Processor Time'
        Cpu         = @(12, 28, 19, 44, 31, 38, 27, 23)
        Memory      = @{ UsedGB = 100; TotalGB = 128 }
        # Get-Counter '\Network Interface(*)\Bytes Total/sec'
        Network     = @(180, 210, 260, 240, 310, 420, 380, 350, 300, 312)
        # Get-Service for the services that matter on this server
        Services    = @{ Running = 11; Stopped = 1 }
    }
}

# A slow or broken source must not block logon: time-box it and fall back.
$server = Invoke-BGInfoDataSource -Name 'Server status' -TimeoutSeconds 15 -ScriptBlock ${function:Get-ServerStatus} -Default ([pscustomobject] @{
        Name = $env:COMPUTERNAME; Role = 'unknown'; Owner = 'unknown'; Maintenance = 'unknown'; OnCall = 'unknown'
        Uptime = [timespan]::Zero; Volumes = @{ 'C:' = 0 }; Cpu = @(0); Memory = @{ UsedGB = 0; TotalGB = 1 }; Network = @(0)
        Services = @{ Running = 0; Stopped = 0 }
    })
#endregion

#region Presentation: reads only $server
$scale = [Math]::Round($Height / 1080, 2)
$look = New-BGInfoTheme Aurora -Mode $Theme -Scale $scale
$fullest = $server.Volumes.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 1
$memory = $server.Memory.UsedGB / $server.Memory.TotalGB

$tile = @{ SurfaceStyle = 'Glass' }
$tiles = @(
    New-BGInfoVisualCanvasTile @tile -IconKind Terminal -Label 'UPTIME' `
        -Value "$($server.Uptime.Days) days $($server.Uptime.Hours) h" `
        -Detail 'Since the September patch window' -Status Ok
    New-BGInfoVisualCanvasTile @tile -IconKind Cpu -Label 'CPU' `
        -Value "$($server.Cpu[-1]) %" -Detail '16 cores · 2.9 GHz' `
        -MiniChartKind Area -MiniChartValues $server.Cpu -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile @tile -IconKind Memory -Label 'MEMORY' `
        -Value "$($server.Memory.UsedGB) of $($server.Memory.TotalGB) GB" -Detail 'SQL Server buffer pool' `
        -Progress $memory -StatusValue ($memory * 100) -WarningThreshold 75 -CriticalThreshold 90
    New-BGInfoVisualCanvasTile @tile -IconKind Network -Label 'NETWORK' `
        -Value "$($server.Network[-1]) Mbps" -Detail '10.40.12.21 · 10 GbE' `
        -MiniChartKind Sparkline -MiniChartValues $server.Network
)

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height

    New-BGInfoLabel -Name 'PRODUCTION  ·  SQL SERVER 2022' -Accent -FontWeight 700 -FontSize ([int](15 * $scale))
    New-BGInfoLabel -Name $server.Name -FontSize ([int](72 * $scale)) -FontWeight 600
    if ($fullest.Value -ge 80) {
        New-BGInfoLabel -Name "●  1 warning · $($fullest.Key) is $($fullest.Value) % full" -Status Warning -FontWeight 600
    }
    New-BGInfoValue -Name 'Role' -Value $server.Role
    New-BGInfoValue -Name 'Owner' -Value $server.Owner
    New-BGInfoValue -Name 'Maintenance' -Value $server.Maintenance
    New-BGInfoValue -Name 'On call' -Value $server.OnCall

    # The tiles stack in the right column; three panels share the bottom band.
    New-BGInfoVisualCanvas -Region Right -Tile $tiles

    $band = @{ Region = 'Bottom'; Height = [int](300 * $scale); NoHistory = $true }
    New-BGInfoChart @band -Id 'volumes' -Title 'Volumes used' -Kind ProgressBar `
        -Values @($server.Volumes.Values) -Labels @($server.Volumes.Keys) -Maximum 100 `
        -ValueSuffix ' %' -NoProgressHandles -ShowLatestValue:$false -WarningThreshold 80 -CriticalThreshold 90
    New-BGInfoChart @band -Id 'network' -Title 'Network' -Kind Area -Values $server.Network `
        -ShowGrid -GridLineCount 4 -ValueSuffix ' Mbps'
    New-BGInfoChart @band -Id 'services' -Title 'Critical services' -Kind Pictorial `
        -Values $server.Services.Running, $server.Services.Stopped -Labels 'Running', 'Stopped' `
        -Palette $look.Ok, $look.Critical -PictorialSymbol Circle -PictorialColumns 12 `
        -Maximum ($server.Services.Running + $server.Services.Stopped) -ShowDataLabels -ShowLatestValue:$false
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "server-status-$($Theme.ToLower()).png"
#endregion
