<#
    Patch day. The question for the person at this PC: when will it
    restart, and what should I do first? For IT on the same screen: is the
    rollout on track ring by ring?

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
function Get-PatchDayData {
    $tuesday = Get-Date '2026-10-13'
    function Window($ring, $day, $from, $hours, $state) {
        [pscustomobject] @{
            Lane = $ring; Category = $state
            Start = $tuesday.AddDays($day).AddHours($from)
            End = $tuesday.AddDays($day).AddHours($from + $hours)
        }
    }
    [pscustomobject] @{
        # (Get-Date); the restart time comes from the update policy, for
        # example the deadline in Get-WUSettings or your RMM's schedule.
        Now       = Get-Date '2026-10-16 18:45'
        RestartAt = Get-Date '2026-10-16 21:00'
        GraceMin  = 240
        Computer  = 'FIN-LT-0457'
        Ring      = 'Ring 2'
        # Get-WindowsUpdate (PSWindowsUpdate), or the WSUS / Intune report
        Updates   = '2026-10 cumulative update · .NET 10.0.3'
        # The update rings: when each ring installs and when it restarts.
        # Windows Update for Business deferrals and deadlines, or the
        # change calendar for servers.
        Windows   = @(
            Window 'Ring 0 · IT' 0 19 12 'Installed'
            Window 'Ring 1 · pilot' 1 9 22 'Installed'
            Window 'Ring 2 · broad' 2 9 36 'Installing'
            Window 'Ring 2 · broad' 3 21 2 'Restart'
            Window 'Ring 3 · servers' 4 22 5 'Planned'
        )
        # Compliance per ring from Intune or WSUS: percent installed.
        Rings     = [ordered] @{
            'Ring 0' = 100; 'Ring 1' = 97; 'Ring 2' = 41; 'Ring 3' = 0
        }
        Checklist = @(
            @{ Text = 'Updates downloaded'; Checked = $true; Value = '1.2 GB' }
            @{ Text = 'Restart scheduled'; Checked = $true; Value = '21:00' }
            @{ Text = 'Save and close your documents'; Checked = $false }
            @{ Text = 'Leave the laptop on power'; Checked = $false }
        )
    }
}

$fallback = [pscustomobject] @{
    Now = Get-Date; RestartAt = $null; GraceMin = 240
    Computer = $env:COMPUTERNAME; Ring = 'unknown'; Updates = 'unknown'
    Windows = @(); Rings = [ordered] @{ 'Ring' = 0 }; Checklist = @()
}
$source = @{
    Name           = 'Patch day data'
    TimeoutSeconds = 20
    ScriptBlock    = ${function:Get-PatchDayData}
    Default        = $fallback
}
$patch = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $patch
$scale = [Math]::Round($Height / 1080, 2)
$look = New-BGInfoTheme Ember -Mode $Theme -Scale $scale
$minutes = $null
$restartTitle = 'Restart schedule unavailable'
$restartDetail = 'Check your update policy for the restart schedule.'
if ($null -ne $patch.RestartAt) {
    $left = $patch.RestartAt - $patch.Now
    if ($left.TotalMinutes -le 0) {
        $restartTitle = 'Restart overdue'
        $restartDetail = ('Scheduled restart was at {0}. ' +
            'Check update status.') -f $patch.RestartAt.ToString('HH:mm')
    } else {
        $minutes = [int] [Math]::Ceiling($left.TotalMinutes)
        $hours = [Math]::Floor($minutes / 60)
        $countdown = '{0} h {1:00} min' -f $hours, ($minutes % 60)
        $restartTitle = "Restart in $countdown"
        $restartDetail = ('This PC restarts at {0} for about ' +
            '20 minutes.') -f $patch.RestartAt.ToString('HH:mm')
    }
}
# Installed is green, installing takes the accent, restarts are amber and
# planned windows stay quiet.
$colors = @{
    Installed  = $look.Ok
    Installing = $look.Accent
    Restart    = $look.Warning
    Planned    = $look.Muted
}
$windows = $patch.Windows | Select-Object *, @{
    Name = 'Color'; Expression = { "$($colors[$_.Category].ToHex())" }
}

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height

    New-BGInfoLabel -Name 'PATCH TUESDAY  ·  OCTOBER 2026' -Accent `
        -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $restartTitle `
        -FontSize ([int](64 * $scale)) -FontWeight 600
    New-BGInfoLabel -Name $restartDetail `
        -FontSize ([int](22 * $scale)) -FontWeight 600
    New-BGInfoValue -Name 'This PC' -Value "$($patch.Computer) · $($patch.Ring)"
    New-BGInfoValue -Name 'Installing' -Value $patch.Updates
    if ($null -ne $minutes) {
        New-BGInfoValue -Name 'Postpone' -Value 'Once, up to 4 h, from the tray'
    }

    # Right column: the countdown turns amber in the last hour and red in
    # the last 15 minutes. Then what to do before it happens.
    if ($null -ne $minutes) {
        New-BGInfoChart -Region Right -Id 'restart' -Title 'Until restart' `
            -Kind Gauge -Values $minutes -Maximum $patch.GraceMin `
            -ValueSuffix ' min' -ShowLatestValue:$false -NoHistory `
            -WarningThreshold 60 -CriticalThreshold 15 `
            -ThresholdDirection Below `
            -Height ([int](250 * $scale))
    }
    if ($patch.Checklist.Count -gt 0) {
        New-BGInfoList -Region Right -Title 'Before you go' `
            -Item $patch.Checklist
    }

    # Bottom band: the windows of the week and how far each ring got.
    $band = @{ Region = 'Bottom'; Height = [int](290 * $scale) }
    if (@($windows).Count -gt 0) {
        New-BGInfoTimeline @band -Title 'Maintenance windows' `
            -Note 'Tue 13 – Sun 18 Oct' -Item $windows -Now $patch.Now
        New-BGInfoChart @band -Id 'rings' -Title 'Installed per ring' `
            -Kind ProgressBar -Values @($patch.Rings.Values) `
            -Labels @($patch.Rings.Keys) -Maximum 100 -ValueSuffix ' %' `
            -NoProgressHandles -ShowLatestValue:$false -NoHistory `
            -Width ([int](560 * $scale))
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "patch-day-$($Theme.ToLower()).png"
#endregion
