<#
    NOC or on-call screen. The question for anyone in the room: who is on
    call right now, and how does an incident escalate from here?

    Built for a wall display: it also renders well at ultrawide sizes. The
    data block returns plain values; the presentation block only reads them.
    Comments name the system that supplies each value.
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
function Get-OnCallData {
    # The rota export of your paging tool (for example its schedules API),
    # flattened to one row per shift: one lane per person, colored by role.
    $monday = Get-Date '2026-10-12'
    function Shift($lane, $category, $start, $hours) {
        [pscustomobject] @{
            Lane = $lane; Category = $category
            Start = $monday.AddHours($start)
            End = $monday.AddHours($start + $hours)
        }
    }
    $shifts = @(
        Shift 'A. Novak' 'Primary' 9 72
        Shift 'A. Novak' 'Secondary' 81 87
        Shift 'J. Smith' 'Primary' 81 87
        Shift 'R. Haddad' 'Secondary' 9 72
        Shift 'S. Ortiz' 'Commander' 0 168
        # The change calendar adds the freeze and the maintenance window.
        Shift 'Changes' 'Change freeze' 90 36
        Shift 'Changes' 'Maintenance' 142 6
    )
    [pscustomobject] @{
        Now       = Get-Date '2026-10-16 10:30'
        Primary   = @{ Name = 'J. Smith'; Phone = '555-0142' }
        Secondary = 'A. Novak · 555-0117'
        Commander = 'S. Ortiz · 555-0108'
        Bridge    = 'Teams · NOC bridge'
        Handover  = 'Monday 09:00'
        Shifts    = $shifts
        # Your ticketing API: open incidents for the NOC queue
        Incidents = @(
            @{ Text = 'INC-20417  VPN latency, EU gateways'
                Status = 'Warning'; Value = 'P2 · 38 min' }
            @{ Text = 'INC-20409  Disk alert, FS-01'
                Status = 'Info'; Value = 'P3 · 2 h' }
            @{ Text = 'INC-20398  Badge reader, door B2'
                Status = 'Info'; Value = 'P4 · 1 d' }
        )
        # The escalation policy, kept as Mermaid text next to the runbook.
        Escalation = @'
flowchart LR
    A([Alert]) --> T[NOC triage<br/>within 5 min]
    T --> P[Primary<br/>J. Smith]
    P -- no ack in 15 min --> S[Secondary<br/>A. Novak]
    S -- no ack in 15 min --> C[Commander<br/>S. Ortiz]
    C --> B([Major incident<br/>bridge])
'@
        Runbook   = 'https://runbooks.contoso.com/noc/escalation'
    }
}

$fallback = [pscustomobject] @{
    Now = Get-Date; Primary = @{ Name = 'See the rota'; Phone = '555-0100' }
    Secondary = 'unknown'; Commander = 'unknown'; Bridge = 'unknown'
    Handover = 'unknown'; Shifts = @(); Incidents = @()
    Escalation = "flowchart LR`n    A([Alert]) --> N[NOC]"
    Runbook = 'https://runbooks.contoso.com/noc'
}
$source = @{
    Name           = 'On-call data'
    TimeoutSeconds = 15
    ScriptBlock    = ${function:Get-OnCallData}
    Default        = $fallback
}
$noc = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $noc
$scale = [Math]::Round($Height / 1080, 2)
# Midnight with a red accent: an operations room, not an office desktop.
$dark = $Theme -eq 'Dark'
$accent = if ($dark) { '#FB7185' } else { '#E11D48' }
$second = if ($dark) { '#A78BFA' } else { '#7C3AED' }
$look = New-BGInfoTheme Midnight -Mode $Theme -Scale $scale -Accent $accent `
    -SecondaryAccent $second
$glow = if ($dark) { '#E11D4833' } else { '#FDA4AF66' }
$link = [uri] $noc.Runbook
# One color per role; the freeze is amber and maintenance stays quiet.
$roles = @{
    Primary         = $look.Accent
    Secondary       = $look.SecondaryAccent
    Commander       = $look.Info
    'Change freeze' = $look.Warning
    Maintenance     = $look.Muted
}
$rota = $noc.Shifts | Select-Object *, @{
    Name = 'Color'; Expression = { "$($roles[$_.Category].ToHex())" }
}

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height -Glow @(
        New-BGInfoBackgroundGlow -X 0.08 -Y 0.05 -Radius 0.5 -Color $glow
        New-BGInfoBackgroundGlow -X 0.9 -Y 1 -Radius 0.45 -Color $glow
    )

    New-BGInfoLabel -Name 'NETWORK OPERATIONS CENTER  ·  ON CALL NOW' -Accent `
        -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $noc.Primary.Name -FontSize ([int](72 * $scale)) `
        -FontWeight 600
    New-BGInfoLabel -Name "Call $($noc.Primary.Phone)" -Accent `
        -FontSize ([int](34 * $scale)) -FontWeight 600
    New-BGInfoValue -Name 'Secondary' -Value $noc.Secondary
    New-BGInfoValue -Name 'Commander' -Value $noc.Commander
    New-BGInfoValue -Name 'Bridge' -Value $noc.Bridge
    New-BGInfoValue -Name 'Handover' -Value $noc.Handover

    # Right column: the runbook on a phone, then what is open right now.
    New-BGInfoQRCode -Region Right -Url $noc.Runbook -Title 'Runbook' `
        -Caption "$($link.Host)$($link.AbsolutePath)" `
        -Height ([int](330 * $scale))
    if (@($noc.Incidents).Count -gt 0) {
        New-BGInfoList -Region Right -Title 'Open incidents' `
            -Note "$(@($noc.Incidents).Count) open" -Item $noc.Incidents
    }

    # Center: the escalation path. Bottom band: this week's rota.
    New-BGInfoMermaid -Region Center -Title 'Escalation path' `
        -Note 'P1 goes straight to the commander' -Definition $noc.Escalation
    if (@($rota).Count -gt 0) {
        New-BGInfoTimeline -Region Bottom -Title 'This week' `
        -Note 'Mon 12 – Sun 18 Oct' -Item $rota -Now $noc.Now `
            -Height ([int](360 * $scale))
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "on-call-$($Theme.ToLower()).png"
#endregion
