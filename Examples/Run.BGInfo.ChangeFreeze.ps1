<#
    Change freeze. The question for the administrator who just signed in:
    am I allowed to change this now? It says so calmly: which freeze, until
    when, what is still allowed, who approves an exception and how to reach
    the people on call.

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
function Get-ChangeFreeze {
    $day = Get-Date '2026-10-16'
    function Window($lane, $category, $from, $to) {
        [pscustomobject] @{
            Lane = $lane; Category = $category
            Start = Get-Date $from; End = Get-Date $to
        }
    }
    [pscustomobject] @{
        Available  = $true
        # (Get-Date)
        Now        = $day.AddHours(10).AddMinutes(15)
        # $env:COMPUTERNAME and the service from your CMDB
        Name       = 'FIN-SQL-03'
        Service    = 'Finance ledger database'
        # The active blackout window from your change calendar API, for
        # example ServiceNow:
        #   Invoke-RestMethod ('https://contoso.service-now.com/api/now/' +
        #       'table/cmn_schedule_blackout?...')
        Freeze     = 'Quarter-end close'
        Start      = Get-Date '2026-10-15 18:00'
        End        = Get-Date '2026-11-02 08:00'
        Approver   = 'Finance IT duty manager'
        Exception  = 'https://change.contoso.com/freeze/exception'
        # Who is on call now: PagerDuty GET https://api.pagerduty.com/oncalls
        # or Opsgenie GET https://api.opsgenie.com/v2/schedules/<id>/on-calls
        OnCall     = 'Alex Morgan · +31 20 555 0142'
        Bridge     = 'bridge.contoso.com/finance · PIN 4410'
        # The last approved change for this configuration item:
        # change_request?cmdb_ci=<sys_id>&state=closed&ORDERBYDESCclosed_at
        LastChange = 'CHG0043188 · TLS certificate · Wed 14 Oct'
        # The change calendar around today: the freeze, the business events
        # it protects and the work that waits for it to end.
        Calendar   = @(
            Window 'Change freeze' 'Freeze' `
                '2026-10-15 18:00' '2026-11-02 08:00'
            Window 'Ledger close' 'Business' `
                '2026-10-29 00:00' '2026-11-01 23:00'
            Window 'Payroll run' 'Business' `
                '2026-10-23 06:00' '2026-10-23 20:00'
            Window 'Approved changes' 'Done' `
                '2026-10-13 21:00' '2026-10-14 01:00'
            Window 'Approved changes' 'Planned' `
                '2026-11-03 21:00' '2026-11-04 02:00'
        )
        # Changes for this configuration item scheduled after the freeze:
        # change_request?cmdb_ci=<sys_id>&state=scheduled
        Queued     = @(
            [pscustomobject] @{ Change = 'CHG0043242'
                What = 'SQL Server cumulative update'; When = 'Tue 3 Nov 21:00'
                State = 'Approved' }
            [pscustomobject] @{ Change = 'CHG0043260'
                What = 'Index maintenance job'; When = 'Wed 4 Nov 01:00'
                State = 'Approved' }
            [pscustomobject] @{ Change = 'CHG0043297'
                What = 'Add read replica'; When = 'Thu 5 Nov 20:00'
                State = 'Pending' }
        )
        # Your freeze policy: what stays allowed while it runs
        Allowed    = @(
            @{ Text = 'Emergency fixes for P1 and P2'; Checked = $true
                Value = 'with approval' }
            @{ Text = 'Read-only checks and reports'; Checked = $true
                Value = 'no ticket' }
            @{ Text = 'Standard changes'; Checked = $false
                Value = 'after 2 Nov' }
            @{ Text = 'Patches and releases'; Checked = $false
                Value = 'from 3 Nov' }
        )
    }
}

$fallback = [pscustomobject] @{
    Available = $false; Now = Get-Date; Name = $env:COMPUTERNAME
    Service = 'unknown'; Freeze = ''; Start = $null; End = $null
    Approver = 'unknown'; Exception = ''
    OnCall = 'unknown'; Bridge = 'unknown'; LastChange = 'unknown'
    Calendar = @(); Queued = @(); Allowed = @()
}
$source = @{
    Name           = 'Change freeze'
    TimeoutSeconds = 15
    ScriptBlock    = ${function:Get-ChangeFreeze}
    Default        = $fallback
}
$freeze = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $freeze
$scale = [Math]::Round($Height / 1080, 2)
$look = New-BGInfoTheme Orchid -Mode $Theme -Scale $scale
$dark = $Theme -eq 'Dark'
$glow = if ($dark) { '#A78BFA2E' } else { '#C4B5FD66' }

$active = $freeze.Freeze -and $null -ne $freeze.End -and
    $freeze.Now -ge $freeze.Start -and $freeze.Now -lt $freeze.End
if ($active) {
    $headline = 'Change freeze'
    $window = '{0}  ·  {1} – {2}' -f $freeze.Freeze,
        $freeze.Start.ToString('ddd d MMM'),
        $freeze.End.ToString('ddd d MMM HH:mm')
    $rule = "Emergency changes only, approved by the $($freeze.Approver)"
    $left = $freeze.End - $freeze.Now
    $ends = '{0} days {1} h, on {2}' -f $left.Days, $left.Hours,
        $freeze.End.ToString('dddd d MMMM, HH:mm')
} else {
    $headline = 'No change freeze'
    $window = 'Normal change process applies'
    $rule = 'Changes follow the usual CAB approval'
    $ends = ''
}
# The freeze takes the accent; business events stay informational, finished
# changes are green and the work waiting for the freeze stays quiet.
$colors = @{
    Freeze   = $look.Accent
    Business = $look.Info
    Done     = $look.Ok
    Planned  = $look.Muted
}
$calendar = $freeze.Calendar | Select-Object *, @{
    Name = 'Color'; Expression = { "$($colors[$_.Category].ToHex())" }
}

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height -GradientAngle 200 `
        -Glow @(
            New-BGInfoBackgroundGlow -X 0.25 -Y 0.0 -Radius 0.6 -Color $glow
            New-BGInfoBackgroundGlow -X 1.0 -Y 0.9 -Radius 0.4 -Color $glow
        ) -Pattern Dots -PatternSpacing ([int](40 * $scale))
    New-BGInfoLayout -ColumnWidth ([int](460 * $scale))

    $heading = "$($freeze.Name)  ·  $($freeze.Service.ToUpper())"
    New-BGInfoLabel -Name $heading -Accent `
        -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $headline -FontSize ([int](96 * $scale)) `
        -FontWeight 700
    New-BGInfoLabel -Name $window -Accent -FontSize ([int](28 * $scale)) `
        -FontWeight 600
    New-BGInfoLabel -Name "●  $rule" -Status Info -FontWeight 600
    if ($ends) { New-BGInfoValue -Name 'Ends in' -Value $ends }
    New-BGInfoValue -Name 'On call' -Value $freeze.OnCall
    New-BGInfoValue -Name 'Bridge' -Value $freeze.Bridge
    New-BGInfoValue -Name 'Last change' -Value $freeze.LastChange

    # Right column: how to ask for an exception, then what stays allowed.
    if ($active -and $freeze.Exception) {
        $link = [uri] $freeze.Exception
        New-BGInfoQRCode -Region Right -Text $freeze.Exception `
            -Title 'Request an exception' `
            -Caption "$($link.Host)$($link.AbsolutePath)" `
            -Height ([int](380 * $scale))
    }
    if (@($freeze.Allowed).Count -gt 0) {
        New-BGInfoList -Region Right -Title 'During the freeze' -Marker Check `
            -Item $freeze.Allowed
    }

    # Center: the work already waiting for the freeze to end.
    if (@($freeze.Queued).Count -gt 0) {
        $freeze.Queued | New-BGInfoTable -Region Center `
            -Title 'Waiting for the freeze' `
            -Note "$(@($freeze.Queued).Count) changes" -StatusColumn State
    }

    # Bottom band: the calendar around the freeze, so the end is in sight.
    if (@($calendar).Count -gt 0) {
        New-BGInfoTimeline -Region Bottom -Title 'Change calendar' `
            -Note '13 Oct – 4 Nov, local time' -Item $calendar `
            -Now $freeze.Now -Height ([int](300 * $scale))
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "change-freeze-$($Theme.ToLower()).png"
#endregion
