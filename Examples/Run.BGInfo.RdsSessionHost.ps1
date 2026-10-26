<#
    Remote Desktop session host. The question for the administrator who
    signs in to this host: is it healthy, and who is on it? The status line
    says whether the broker still sends new users here; the right column
    lists the sessions, heaviest first; the bottom band shows how full the
    host is and how long sign-ins take.

    The data block returns plain values; the presentation block only reads
    them. Comments name the command or source that supplies each value.
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
function Get-RdsSessionHostInfo {
    $today = Get-Date '2026-10-16 10:20'
    [pscustomobject] @{
        Available  = $true
        # $env:COMPUTERNAME and (Get-CimInstance Win32_OperatingSystem).Caption
        Name       = 'RDSH-FIN-03'
        Os         = 'Windows Server 2025 Datacenter'
        # Get-RDSessionHost -CollectionName 'Finance Desktops'
        #     -ConnectionBroker rdcb01.corp.contoso.com
        # gives NewConnectionAllowed (Yes, NotUntilReboot or No) per host.
        Collection = 'Finance Desktops'
        Hosts      = 4
        Broker     = 'rdcb01.corp.contoso.com'
        Accepting  = 'Yes'
        # Your sizing limit per host; the broker spreads users by
        # Get-RDSessionHost ... |
        #     Select-Object SessionHost, NewConnectionAllowed
        # and the load-balancing weight from
        # Get-RDSessionCollectionConfiguration -LoadBalancing
        # (SessionLimit per host).
        SessionLimit = 25
        # Get-ItemProperty 'HKLM:\SOFTWARE\FSLogix\Profiles' (VHDLocations,
        # SizeInMBs) and Get-Volume on the profile share's file server
        ProfileShare = '\\fs01\fslogix$'
        ProfileCapGB = 30
        ProfileUsedTB = 1.9
        ProfileSizeTB = 3
        # Last restart: (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
        Restart    = 'Sunday 03:00, drained from Saturday 22:00'
        # Get-RDUserSession -ConnectionBroker rdcb01.corp.contoso.com |
        #     Where-Object HostServer -Like 'RDSH-FIN-03*'
        # (UserName, SessionState, IdleTime), or qwinsta on the host. CPU and
        # memory per session: Get-Process -IncludeUserName grouped by SessionId.
        Sessions   = @(
            [pscustomobject] @{ User = 'jonas.w'; State = 'Active'
                Idle = '1 min'; Cpu = 31; MemoryGB = 3.6 }
            [pscustomobject] @{ User = 'lena.p'; State = 'Active'
                Idle = 'now'; Cpu = 12; MemoryGB = 2.1 }
            [pscustomobject] @{ User = 'ines.m'; State = 'Active'
                Idle = 'now'; Cpu = 9; MemoryGB = 1.8 }
            [pscustomobject] @{ User = 'tomas.r'; State = 'Active'
                Idle = '2 min'; Cpu = 4; MemoryGB = 1.4 }
            [pscustomobject] @{ User = 'noor.h'; State = 'Active'
                Idle = 'now'; Cpu = 3; MemoryGB = 1.2 }
            [pscustomobject] @{ User = 'karel.d'; State = 'Active'
                Idle = '5 min'; Cpu = 2; MemoryGB = 0.9 }
            [pscustomobject] @{ User = 'priya.n'; State = 'Active'
                Idle = '14 min'; Cpu = 1; MemoryGB = 0.7 }
            [pscustomobject] @{ User = 'marta.s'; State = 'Disconnected'
                Idle = '26 min'; Cpu = 0; MemoryGB = 1.1 }
            [pscustomobject] @{ User = 'olaf.b'; State = 'Disconnected'
                Idle = '2 h 10 min'; Cpu = 0; MemoryGB = 0.9 }
        )
        # Session totals: Get-Counter '\Terminal Services\Active Sessions'
        # and '\Terminal Services\Inactive Sessions'
        Active       = 16
        Disconnected = 3
        # The same counters every 15 minutes since 07:30, from your
        # monitoring system or a scheduled Get-Counter -SampleInterval 900
        SessionHistory = @(2, 5, 9, 12, 14, 16, 17, 18, 18, 19, 19, 19)
        MemoryHistory  = @(31, 38, 45, 50, 54, 57, 58, 60, 61, 61, 62, 62)
        # Get-Counter '\Processor(_Total)\% Processor Time' sampled every
        # 15 minutes, and '\Memory\% Committed Bytes In Use'
        Cpu          = @(22, 31, 38, 44, 52, 47, 41, 49, 56, 48, 43, 46)
        MemoryGB     = 79
        TotalGB      = 128
        # Average logon time per day. Event 1 and 2 of Microsoft-Windows-
        # User Profile Service/Operational, or the Winlogon 4624 to
        # explorer.exe start in the Security log:
        #   Get-WinEvent -FilterHashtable @{
        #       LogName = 'Microsoft-Windows-User Profile Service/Operational'
        #       Id = 1, 2; StartTime = (Get-Date).AddDays(-14) }
        Logon        = @(19, 18, 21, 20, 19, 17, 18, 22, 41, 26, 20, 19, 18, 19)
        LogonDays    = foreach ($day in 13..0) {
            $today.AddDays(-$day).ToString('dd')
        }
        # Container size per user: Get-ChildItem $ProfileShare -Recurse
        # -Filter *.vhdx, or
        # Get-Content C:\ProgramData\FSLogix\Logs\Profile\*.log
        Containers   = [ordered] @{
            'jonas.w' = 27.6
            'lena.p'  = 24.1
            'olaf.b'  = 19.8
            'ines.m'  = 14.2
            'tomas.r' = 11.5
        }
    }
}

$fallback = [pscustomobject] @{
    Available = $false; Name = $env:COMPUTERNAME; Os = 'unknown'
    Collection = 'unknown'; Hosts = 0; Broker = 'unknown'
    Accepting = 'unknown'; SessionLimit = 0; ProfileShare = 'unknown'
    ProfileCapGB = 0; ProfileUsedTB = 0; ProfileSizeTB = 0
    Restart = 'unknown'; Sessions = @(); Active = 0; Disconnected = 0
    SessionHistory = @(); MemoryHistory = @(); Cpu = @(0); MemoryGB = 0
    TotalGB = 0; Logon = @(); LogonDays = @(); Containers = [ordered] @{}
}
$source = @{
    Name           = 'RDS session host'
    TimeoutSeconds = 20
    ScriptBlock    = ${function:Get-RdsSessionHostInfo}
    Default        = $fallback
}
$rds = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $rds
$scale = [Math]::Round($Height / 1080, 2)
$look = New-BGInfoTheme Graphite -Mode $Theme -Scale $scale
$sessions = $rds.Active + $rds.Disconnected
$memory = if ($rds.TotalGB) {
    [Math]::Round(100 * $rds.MemoryGB / $rds.TotalGB)
} else { 0 }

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height -Pattern Dots `
        -PatternSpacing ([int](28 * $scale))
    # A wide right column so the session table keeps every column readable.
    New-BGInfoLayout -ColumnWidth ([int](620 * $scale))

    New-BGInfoLabel -Name "RDS SESSION HOST  ·  $($rds.Collection.ToUpper())" `
        -Accent -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $rds.Name -FontSize ([int](72 * $scale)) `
        -FontWeight 600
    switch ($rds.Accepting) {
        'Yes' {
            $broker = $rds.Broker.Split('.')[0].ToUpper()
            New-BGInfoLabel -Name "●  Accepting new sessions from $broker" `
                -Status Ok -FontWeight 600
        }
        'unknown' {
            New-BGInfoLabel -Name '●  Broker state unavailable' -Status Info `
                -FontWeight 600
        }
        default {
            New-BGInfoLabel -Name '●  Drain mode: no new sessions' `
                -Status Warning -FontWeight 600
        }
    }
    New-BGInfoValue -Name 'Collection' `
        -Value "$($rds.Collection), $($rds.Hosts) hosts"
    New-BGInfoValue -Name 'Broker' -Value $rds.Broker
    New-BGInfoValue -Name 'System' -Value $rds.Os
    New-BGInfoValue -Name 'Profiles' -Value "FSLogix on $($rds.ProfileShare)"
    New-BGInfoValue -Name 'Restart' -Value $rds.Restart

    # Right column: who is on the host, heaviest first, then whose profile
    # container is close to its cap (amber from 80 %, red from 90 %).
    if (@($rds.Sessions).Count -gt 0) {
        $rds.Sessions | Sort-Object Cpu -Descending |
            Select-Object User, State, Idle,
                @{ Name = 'CPU'; Expression = { "$($_.Cpu) %" } },
                @{ Name = 'Memory'; Expression = { "$($_.MemoryGB) GB" } } |
            New-BGInfoTable -Region Right -Title 'Who is on it' `
                -Note "$(@($rds.Sessions).Count) of $sessions by CPU" `
                -StatusColumn State -StatusMap @{ Active = 'Ok'; Disconnected = 'Warning' }
    }
    if ($rds.Containers.Count -gt 0) {
        $percent = foreach ($size in $rds.Containers.Values) {
            [Math]::Round(100 * $size / $rds.ProfileCapGB)
        }
        $title = 'Largest FSLogix containers  ·  ' +
            "cap $($rds.ProfileCapGB) GB"
        New-BGInfoChart -Region Right -Id 'containers' -Title $title `
            -Kind ProgressBar -Values $percent -Labels @($rds.Containers.Keys) `
            -Maximum 100 -ValueSuffix ' %' -NoHistory -NoProgressHandles `
            -ShowLatestValue:$false -ShowDataLabels `
            -WarningThreshold 80 -CriticalThreshold 90
    }

    # Center, under the text: how long sign-ins took over two weeks. A spike
    # usually means a GPO, profile or antivirus change.
    if (@($rds.Logon).Count -gt 0) {
        New-BGInfoChart -Region Center -Id 'logon' `
            -Title 'Average logon  ·  last 14 days' `
            -Kind Bar -Values $rds.Logon -Labels $rds.LogonDays -NoHistory `
            -ValueSuffix ' s' -ShowGrid -GridLineCount 3 -Minimum 0 `
            -WarningThreshold 30 -CriticalThreshold 60
    }

    # Bottom band: seats used against the limit, CPU and memory. Sessions
    # turn amber at 80 % of the limit and red at 95 %.
    if ($rds.Available) {
        $card = @{ Region = 'Bottom'; Height = [int](180 * $scale) }
        $split = "$($rds.Active) active, $($rds.Disconnected) disconnected"
        New-BGInfoMetric @card -Label 'Sessions' -Value $sessions `
            -Unit "of $($rds.SessionLimit)" -Caption $split `
            -History $rds.SessionHistory -HistoryMaximum $rds.SessionLimit `
            -WarningThreshold ($rds.SessionLimit * 0.8) `
            -CriticalThreshold ($rds.SessionLimit * 0.95)
        New-BGInfoMetric @card -Label 'CPU' -Value $rds.Cpu[-1] -Unit '%' `
            -Caption 'last 3 hours' -History $rds.Cpu -HistoryMaximum 100 `
            -WarningThreshold 70 -CriticalThreshold 85
        New-BGInfoMetric @card -Label 'Memory' -Value $memory -Unit '%' `
            -Caption "$($rds.MemoryGB) of $($rds.TotalGB) GB in use" `
            -History $rds.MemoryHistory -HistoryMaximum 100 `
            -WarningThreshold 80 -CriticalThreshold 90
        if ($rds.ProfileSizeTB) {
            $used = [Math]::Round(100 * $rds.ProfileUsedTB / $rds.ProfileSizeTB)
            $usage = "$($rds.ProfileUsedTB) of $($rds.ProfileSizeTB) TB on " +
                $rds.ProfileShare
            New-BGInfoMetric @card -Label 'Profile share' -Unit '%' `
                -Value $used -Caption $usage `
                -WarningThreshold 75 -CriticalThreshold 90
        }
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "rds-session-host-$($Theme.ToLower()).png"
#endregion
