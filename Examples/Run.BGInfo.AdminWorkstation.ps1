<#
    Privileged access workstation. A dense, dark HUD for administrators:
    health tiles with recent trends, the tier 0 path this machine is allowed
    to reach, and the privileged activity worth a second look.

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
function Get-AdminWorkstationData {
    [pscustomobject] @{
        Available     = $true
        Name          = 'PAW-T0-03'
        Operator      = 'adm.jsmith'
        Domain        = 'corp.contoso.com'
        # Get-Counter '\Processor(_Total)\% Processor Time'
        Cpu           = @(9, 14, 31, 22, 12, 18)
        Memory        = @(28, 30, 29, 31, 30, 30)
        SystemFreeGB  = 212
        SystemUsed    = 0.41
        # Get-MpComputerStatus
        Defender      = 'Protected'
        # Get-HotFix | Sort-Object InstalledOn | Select-Object -Last 1
        PatchLevel    = 'October 2026'
        # Get-ChildItem Cert:\CurrentUser\My | Sort-Object NotAfter
        SmartCardDays = 27
        Elevation     = 'JIT until 16:40'
        # Last 7 days from the Security log, event 4672.
        SignIns       = @(4, 6, 3, 9, 5, 2, 7)
        Changes       = @(2, 1, 4, 3, 8, 5, 3, 2, 6, 4)
        Alerts        = [ordered] @{ High = 1; Medium = 3; Low = 6 }
        ReplicaLag    = '14 min lag'
    }
}

$paw = Invoke-BGInfoDataSource -Name 'Workstation data' -TimeoutSeconds 15 -ScriptBlock ${function:Get-AdminWorkstationData} -Default ([pscustomobject] @{
        Available = $false; Name = $env:COMPUTERNAME; Operator = $env:USERNAME; Domain = 'unknown'; Cpu = @(0); Memory = @(0); SystemFreeGB = 0; SystemUsed = 0
        Defender = 'unknown'; PatchLevel = 'unknown'; SmartCardDays = 0; Elevation = 'unknown'; SignIns = @(0); Changes = @(0)
        Alerts = [ordered] @{ High = 0 }; ReplicaLag = ''
    })
#endregion

#region Presentation: reads only $paw
$scale = [Math]::Round($Height / 1080, 2)
$look = New-BGInfoTheme Midnight -Mode $Theme -Scale $scale

# Seven tiles fill the left rail from the top margin to the bottom margin.
$railWidth = [int](460 * $scale)
$tileHeight = [Math]::Floor(($Height - 2 * $look.MarginY - 6 * $look.TileGap) / 7)
# The text block and the content column start right of the rail.
$textX = 2 * $look.MarginX + $railWidth
$column = $Width - $textX - $look.MarginX
$half = [int](($column - $look.Gap) / 2)
$bottomHeight = [int](210 * $scale)
$diagramTop = [int](360 * $scale)
$diagramHeight = $Height - $diagramTop - $look.MarginY - $bottomHeight - 32

$smartCard = if ($paw.Available) {
    @{
        Value = "Expires in $($paw.SmartCardDays) days"
        Detail = 'Renew before 11 Nov'
        StatusValue = $paw.SmartCardDays
        WarningThreshold = 30
        CriticalThreshold = 7
        ThresholdDirection = 'Below'
    }
} else {
    @{ Value = 'Unknown'; Detail = 'Expiry unavailable'; Status = 'Info' }
}
$defenderStatus = switch ($paw.Defender) {
    'Protected' { 'Ok' }
    'Disabled' { 'Critical' }
    'Real-time protection off' { 'Critical' }
    default { 'Info' }
}
$heading = if ($paw.Available) { 'TIER 0  ·  PRIVILEGED ACCESS WORKSTATION' } else { 'DATA UNAVAILABLE  ·  VALUES ARE FALLBACKS' }

New-BGInfo -Theme $look -SpaceX $textX -Target File {
    $tile = @{ SurfaceStyle = 'Glass' }
    $tiles = @(
        New-BGInfoVisualCanvasTile @tile -IconKind Cpu -Label 'CPU' -Value "$($paw.Cpu[-1]) %" `
            -Detail '8 cores · Credential Guard on' -MiniChartKind Area -MiniChartValues $paw.Cpu -MiniChartMaximum 100
        New-BGInfoVisualCanvasTile @tile -IconKind Memory -Label 'MEMORY' -Value '9.6 of 32 GB' `
            -Detail 'Hyper-V isolation' -MiniChartKind Bars -MiniChartValues $paw.Memory -MiniChartMaximum 100
        New-BGInfoVisualCanvasTile @tile -IconKind Storage -Label 'SYSTEM DRIVE' -Value "$($paw.SystemFreeGB) GB free" `
            -Detail 'BitLocker · TPM + PIN' -Progress $paw.SystemUsed
        New-BGInfoVisualCanvasTile @tile -IconKind Shield -Label 'DEFENDER' -Value $paw.Defender `
            -Detail ($(if ($paw.Defender -eq 'unknown') { 'Status unavailable' } else { 'Signatures 2 h old' })) -Status $defenderStatus
        New-BGInfoVisualCanvasTile @tile -IconKind OperatingSystem -Label 'PATCH LEVEL' -Value $paw.PatchLevel `
            -Detail 'Ring 0 · pilot' -Status ($(if ($paw.PatchLevel -eq 'unknown') { 'Info' } else { 'Ok' }))
        # Fewer than 30 days left is a warning, fewer than 7 is critical.
        New-BGInfoVisualCanvasTile @tile @smartCard -IconKind Domain -Label 'SMART CARD'
        New-BGInfoVisualCanvasTile @tile -IconKind Terminal -Label 'ELEVATION' -Value $paw.Elevation `
            -Detail ($(if ($paw.Available) { 'Approved by S. Ortiz' } else { 'Approval unavailable' })) -Status ($(if ($paw.Available) { 'Warning' } else { 'Info' }))
    )
    New-BGInfoBackground -Width $Width -Height $Height

    New-BGInfoLabel -Name $heading -Status ($(if ($paw.Available) { 'Critical' } else { 'Warning' })) -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $paw.Name -FontSize ([int](64 * $scale)) -FontWeight 600
    New-BGInfoLabel -Name 'No email, no browsing, no personal accounts.' -FontSize ([int](19 * $scale)) -FontWeight 600
    New-BGInfoValue -Name 'Operator' -Value $paw.Operator
    New-BGInfoValue -Name 'Domain' -Value $paw.Domain
    New-BGInfoValue -Name 'Reachable' -Value 'Jump host → tier 0 only'

    # The theme puts the canvas rails on its margins; only the tile size is set here.
    New-BGInfoVisualCanvas -Tile ($tiles | ForEach-Object { $_.Side = 'Left'; $_ }) -LeftTileWidth $railWidth -TileHeight $tileHeight

    New-BGInfoChart -Id 'signins' -Title 'Privileged sign-ins' -Kind Bar -Values $paw.SignIns `
        -ShowGrid -GridLineCount 3 -ValueSuffix ' today' -NoHistory `
        -Width ([int](560 * $scale)) -Height ([int](270 * $scale)) -Anchor TopRight -OffsetX $look.MarginX -OffsetY $look.MarginY

    New-BGInfoTopology -Title 'Allowed admin path' -Subtitle '2 hops to tier 0' -Layout DenseGrouped `
        -Scale ($(if ($Height -ge 1440) { 2 } else { 1 })) -NodeDisplayMode Card -Width $column -Height $diagramHeight `
        -Anchor TopRight -OffsetX $look.MarginX -OffsetY $diagramTop -TopologyDefinition {
        $node = @{ Status = 'Healthy' }
        New-BGInfoTopologyGroup -Id 'paw' -Label 'This workstation'
        New-BGInfoTopologyGroup -Id 'jump' -Label 'Jump hosts'
        New-BGInfoTopologyGroup -Id 't0' -Label 'Tier 0'
        New-BGInfoTopologyNode @node -Id 'paw' -Label $paw.Name -Subtitle 'Windows 11' -Kind Endpoint -GroupId 'paw'
        New-BGInfoTopologyNode @node -Id 'jmp' -Label 'JMP-T0-01' -Subtitle 'Server 2025' -Kind Gateway -GroupId 'jump'
        New-BGInfoTopologyNode @node -Id 'dc1' -Label 'DC01' -Subtitle 'PDC emulator' -Kind Server -GroupId 't0'
        New-BGInfoTopologyNode @node -Id 'ca' -Label 'CA01' -Subtitle 'Enterprise CA' -Kind Certificate -GroupId 't0'
        New-BGInfoTopologyNode -Id 'dc2' -Label 'DC02' -Subtitle 'Replica' -Kind Server -GroupId 't0' -Status Warning
        $link = @{ Kind = 'Connectivity'; Status = 'Healthy' }
        New-BGInfoTopologyEdge @link -SourceNodeId 'paw' -TargetNodeId 'jmp' -Label 'RDP + MFA' -Direction Forward
        New-BGInfoTopologyEdge @link -SourceNodeId 'jmp' -TargetNodeId 'dc1' -Direction Forward
        New-BGInfoTopologyEdge @link -SourceNodeId 'jmp' -TargetNodeId 'ca' -Direction Forward
        New-BGInfoTopologyEdge -SourceNodeId 'dc1' -TargetNodeId 'dc2' -Label $paw.ReplicaLag -Kind Replication -Status Warning
    }

    $bottom = @{ Anchor = 'BottomRight'; Width = $half; Height = $bottomHeight; OffsetY = $look.MarginY; NoHistory = $true }
    New-BGInfoChart @bottom -Id 'changes' -Title 'Tier 0 changes' -Kind Area -Values $paw.Changes `
        -ShowGrid -GridLineCount 3 -ValueSuffix ' this hour' -OffsetX ($look.MarginX + $half + $look.Gap)
    New-BGInfoChart @bottom -Id 'alerts' -Title 'Open security alerts' -Kind ProgressBar -Values @($paw.Alerts.Values) `
        -Labels @($paw.Alerts.Keys) -Palette $look.Critical, $look.Warning, $look.Accent -Maximum 10 `
        -NoProgressHandles -ShowLatestValue:$false -OffsetX $look.MarginX
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "admin-workstation-$($Theme.ToLower()).png"
#endregion
