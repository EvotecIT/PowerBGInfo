<#
    Domain controller health. The question for the administrator at this
    console: is the directory replicating, does this DC hold the roles it
    should, is its clock right, and has anything unusual hit it lately?

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
function Get-DomainControllerHealth {
    $today = Get-Date '2026-10-16'
    [pscustomobject] @{
        Available = $true
        # $env:COMPUTERNAME and (Get-ADDomainController).Site
        Name     = 'DC01'
        Site     = 'AMS-HQ'
        Domain   = 'corp.contoso.com'
        # (Get-CimInstance Win32_OperatingSystem).Caption
        Os       = 'Windows Server 2025 Datacenter'
        # wbadmin get versions, or your backup product's API
        Backup   = 'System state 02:10 today'
        # Get-ADForest (SchemaMaster, DomainNamingMaster) and
        # Get-ADDomain (PDCEmulator, RIDMaster, InfrastructureMaster)
        Fsmo     = [ordered] @{
            'Schema master'        = 'DC01'
            'Domain naming master' = 'DC01'
            'PDC emulator'         = 'DC01'
            'RID master'           = 'DC02'
            'Infrastructure'       = 'DC02'
        }
        # w32tm /stripchart /computer:time.contoso.com /samples:8 /dataonly
        TimeOffsetMs = 3.4
        TimeHistory  = @(2.1, 2.8, 4.0, 3.1, 2.6, 3.9, 3.2, 3.4)
        TimeSource   = 'time.contoso.com, stratum 2'
        # Get-ADReplicationSite and Get-ADReplicationSiteLink
        Sites    = @(
            [pscustomobject] @{
                Name = 'AMS-HQ'; Servers = 'DC01 · DC02'; Status = 'Healthy'
            }
            [pscustomobject] @{
                Name = 'WAW-BR'; Servers = 'DC03'; Status = 'Healthy'
                Interval = '15 min'
            }
            [pscustomobject] @{
                Name = 'NYC-BR'; Servers = 'DC04'; Status = 'Warning'
                Interval = '3 h late'
            }
            [pscustomobject] @{
                Name = 'LIS-BR'; Servers = 'RODC01'; Status = 'Healthy'
                Interval = '1 h'
            }
        )
        # dcdiag /q, or dcdiag /test:<name> per line
        DiagnosedAt  = 'today 06:00'
        Diagnostics  = @(
            @{ Text = 'Replications'; Status = 'Warning'; Value = 'DC04' }
            @{ Text = 'Connectivity, advertising'; Status = 'Ok'
                Value = 'passed' }
            @{ Text = 'SYSVOL and NetLogons'; Status = 'Ok'; Value = 'passed' }
            @{ Text = 'DNS registration'; Status = 'Ok'; Value = 'passed' }
        )
        # Get-ADReplicationPartnerMetadata -Target $env:COMPUTERNAME
        # and Get-ADReplicationFailure -Target $env:COMPUTERNAME
        Partners = @(
            [pscustomobject] @{
                Partner = 'DC02'; Site = 'AMS-HQ'
                Last = '2 min ago'; Failures = 0; State = 'OK'
            }
            [pscustomobject] @{
                Partner = 'DC03'; Site = 'WAW-BR'
                Last = '11 min ago'; Failures = 0; State = 'OK'
            }
            [pscustomobject] @{
                Partner = 'DC04'; Site = 'NYC-BR'
                Last = '3 h 12 min ago'; Failures = 4; State = 'Warning'
            }
            [pscustomobject] @{
                Partner = 'RODC01'; Site = 'LIS-BR'
                Last = '9 min ago'; Failures = 0; State = 'OK'
            }
        )
        # Failed sign-ins per day: Security log events 4625 and 4771.
        #   Get-WinEvent -FilterHashtable @{
        #       LogName = 'Security'; Id = 4625, 4771
        #       StartTime = (Get-Date).AddDays(-126) } |
        #   Group-Object { $_.TimeCreated.Date }
        Failures = foreach ($day in 0..125) {
            $date = $today.AddDays(-$day)
            $weekend = $date.DayOfWeek -in 'Saturday', 'Sunday'
            $value = if ($weekend) { 2 + $day % 4 } else { 9 + ($day * 7) % 13 }
            # A password-spray attempt three weeks ago stands out.
            if ($day -eq 22) { $value = 96 }
            if ($day -eq 21) { $value = 41 }
            [pscustomobject] @{ Date = $date; Value = $value }
        }
    }
}

$fallback = [pscustomobject] @{
    Available = $false
    Name = $env:COMPUTERNAME; Site = 'unknown'; Domain = 'unknown'
    Os = 'unknown'; Backup = 'unknown'; Fsmo = [ordered] @{}
    TimeOffsetMs = 0; TimeHistory = @(); TimeSource = 'unknown'
    Sites = @(); DiagnosedAt = ''; Diagnostics = @(); Partners = @()
    Failures = @([pscustomobject] @{ Date = (Get-Date).Date; Value = 0 })
}
$source = @{
    Name           = 'Domain controller health'
    TimeoutSeconds = 20
    ScriptBlock    = ${function:Get-DomainControllerHealth}
    Default        = $fallback
}
$dc = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $dc
$scale = [Math]::Round($Height / 1080, 2)
$look = New-BGInfoTheme Graphite -Mode $Theme -Scale $scale
$failing = @($dc.Partners | Where-Object State -NE 'OK')
$held = @($dc.Fsmo.GetEnumerator() | Where-Object Value -EQ $dc.Name)

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height

    New-BGInfoLabel -Name "DOMAIN CONTROLLER  ·  $($dc.Domain.ToUpper())" `
        -Accent -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $dc.Name -FontSize ([int](72 * $scale)) `
        -FontWeight 600
    if ($dc.Partners.Count -eq 0) {
        New-BGInfoLabel -Name '●  Replication data unavailable' -Status Info `
            -FontWeight 600
    } elseif ($failing.Count -gt 0) {
        $partner = $failing[0]
        $text = "Replication from $($partner.Partner) last succeeded " +
            $partner.Last
        New-BGInfoLabel -Name "●  $text" -Status Warning -FontWeight 600
    } else {
        New-BGInfoLabel -Name '●  Replicating with every partner' -Status Ok `
            -FontWeight 600
    }
    New-BGInfoValue -Name 'Site' -Value $dc.Site

    New-BGInfoValue -Name 'System' -Value $dc.Os
    New-BGInfoValue -Name 'Backup' -Value $dc.Backup

    # Right column: who holds each FSMO role, and how far the clock drifts.
    $roles = foreach ($role in $dc.Fsmo.GetEnumerator()) {
        @{ Text = $role.Key; Value = $role.Value; Status = 'Ok' }
    }
    if ($dc.Available) {
        New-BGInfoList -Region Right -Title 'FSMO roles' `
            -Note "$($held.Count) of 5 here" -Item $roles
        New-BGInfoMetric -Region Right -Label 'Time offset' `
            -Value $dc.TimeOffsetMs -Unit 'ms' -Caption $dc.TimeSource `
            -History $dc.TimeHistory -WarningThreshold 100 -CriticalThreshold 1000 `
            -Height ([int](150 * $scale))
    }

    # Center: every inbound partner, warnings first.
    if ($dc.Partners.Count -gt 0) {
        $dc.Partners | Sort-Object { $_.State -eq 'OK' } |
            New-BGInfoTable -Region Center -Title 'Inbound replication' `
                -Note "$($dc.Partners.Count) partners" -StatusColumn State
    }

    # Bottom band: the site links, dcdiag and the failed sign-in history.
    $band = @{ Region = 'Bottom'; Height = [int](290 * $scale) }
    if ($dc.Sites.Count -gt 0) {
        New-BGInfoTopology @band -Title 'Site links' -Subtitle 'from AMS-HQ' `
            -Layout HubAndSpoke -NodeDisplayMode Card `
            -Width ([int](600 * $scale)) -TopologyDefinition {
            foreach ($site in $dc.Sites) {
                New-BGInfoTopologyNode -Id $site.Name -Label $site.Name `
                    -Subtitle $site.Servers -Kind Location -Status $site.Status
            }
            foreach ($site in $dc.Sites | Where-Object Interval) {
                New-BGInfoTopologyEdge -SourceNodeId $dc.Site `
                    -TargetNodeId $site.Name -Label $site.Interval `
                    -Kind Replication -Status $site.Status
            }
        }
    }
    if ($dc.Available) {
        New-BGInfoList @band -Title 'dcdiag' -Note $dc.DiagnosedAt `
            -Item $dc.Diagnostics
        $dc.Failures | New-BGInfoCalendarHeatmap @band -Title 'Failed sign-ins' `
            -Note 'events 4625 and 4771' -Width ([int](660 * $scale))
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "domain-controller-$($Theme.ToLower()).png"
#endregion
