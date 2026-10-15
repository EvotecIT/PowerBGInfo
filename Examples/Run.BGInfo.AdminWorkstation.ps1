<#
    Privileged access workstation. A dense, dark HUD for administrators:
    health tiles with recent trends, the tier 0 path this machine is allowed
    to reach, and the privileged activity worth a second look.

    Sample values keep the render reproducible. Comments name the command
    or source that supplies each value in a real environment.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output')
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force
$hudTheme = Join-Path $PSScriptRoot 'Showcase\HudTheme.ps1'
$hud = & $hudTheme -Theme $Theme -Width $Width -Height $Height
$dark = $Theme -eq 'Dark'
$ok, $warn, $bad = $hud.Status.Ok, $hud.Status.Warn, $hud.Status.Bad
$accent = if ($dark) { '#818CF8' } else { '#4F46E5' }

$background = @{
    Width          = $Width
    Height         = $Height
    Color          = if ($dark) { '#05060B' } else { '#F3F4F8' }
    Gradient       = if ($dark) { '#0D1020', '#05060B' } else {
        '#FAFAFD', '#E4E7F1'
    }
    GradientAngle  = 100
    Pattern        = 'Grid'
    PatternColor   = if ($dark) { '#FFFFFF08' } else { '#1E1B4B0C' }
    PatternSpacing = 32
}
$glows = if ($dark) { '#4F46E544', '#DC262622' } else {
    '#A5B4FC80', '#FCA5A555'
}

# Seven tiles fill the left rail from the top margin to the bottom margin.
$canvas = $hud.Canvas.Clone()
$rail = $Height - 2 * $hud.MarginY - 6 * $canvas.TileGap
$canvas.TileHeight = [Math]::Floor($rail / 7)
$tile = @{ Side = 'Left'; SurfaceStyle = 'Glass'; Accent = $accent }
$tiles = @(
    # Get-Counter '\Processor(_Total)\% Processor Time'
    New-BGInfoVisualCanvasTile @tile -IconKind Cpu -Label 'CPU' `
        -Value '18 %' -Detail '8 cores · Credential Guard on' `
        -MiniChartKind Area -MiniChartValues 9, 14, 31, 22, 12, 18 `
        -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile @tile -IconKind Memory -Label 'MEMORY' `
        -Value '9.6 of 32 GB' -Detail 'Hyper-V isolation' `
        -MiniChartKind Bars -MiniChartValues 28, 30, 29, 31, 30, 30 `
        -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile @tile -IconKind Storage -Label 'SYSTEM DRIVE' `
        -Value '212 GB free' -Detail 'BitLocker · TPM + PIN' `
        -Progress 0.41
    # Get-MpComputerStatus
    New-BGInfoVisualCanvasTile @tile -IconKind Shield -Label 'DEFENDER' `
        -Value 'Protected' -Detail 'Signatures 2 h old' -Accent $ok
    # Get-HotFix | Sort-Object InstalledOn | Select-Object -Last 1
    New-BGInfoVisualCanvasTile @tile -IconKind OperatingSystem `
        -Label 'PATCH LEVEL' -Value 'October 2026' -Detail 'Ring 0 · pilot' `
        -Accent $ok
    # Get-ChildItem Cert:\CurrentUser\My | Sort-Object NotAfter
    New-BGInfoVisualCanvasTile @tile -IconKind Domain -Label 'SMART CARD' `
        -Value 'Expires in 27 days' -Detail 'Renew before 11 Nov' `
        -Accent $warn
    New-BGInfoVisualCanvasTile @tile -IconKind Terminal -Label 'ELEVATION' `
        -Value 'JIT until 16:40' -Detail 'Approved by S. Ortiz' `
        -Accent $warn
)

$text = $hud.Text.Clone()
$text.SpaceX = 2 * $hud.MarginX + 460
$panel = $hud.Panel

# The content column runs from the text block to the right margin.
$column = $Width - $text.SpaceX - $hud.MarginX
$half = ($column - 24) / 2
$bottom = @{ Anchor = 'BottomRight'; Width = $half; Height = 210 }
# The diagram fills the space between the top row and the bottom row.
$diagramTop = 360
$diagramHeight = $Height - $diagramTop - $hud.MarginY - 210 - 32
# Render nodes and labels twice as large on 1440p and bigger screens.
$diagramScale = if ($Height -ge 1440) { 2 } else { 1 }

New-BGInfo -Target File {
    New-BGInfoBackground @background -Glow @(
        New-BGInfoBackgroundGlow -X 0.65 -Y 0.25 -Radius 0.6 -Color $glows[0]
        New-BGInfoBackgroundGlow -X 1 -Y 0 -Radius 0.3 -Color $glows[1]
    )

    New-BGInfoLabel -Name 'TIER 0  ·  PRIVILEGED ACCESS WORKSTATION' `
        -Color $bad -FontSize 15 -FontWeight 700
    New-BGInfoLabel -Name 'PAW-T0-03' -Color $hud.Ink -FontSize 64 `
        -FontWeight 600
    New-BGInfoLabel -Name 'No email, no browsing, no personal accounts.' `
        -Color $hud.Ink -FontSize 19 -FontWeight 600
    New-BGInfoValue -Name 'Operator' -Value 'adm.jsmith'
    New-BGInfoValue -Name 'Domain' -Value 'corp.contoso.com'
    New-BGInfoValue -Name 'Reachable' -Value 'Jump host → tier 0 only'

    New-BGInfoVisualCanvas @canvas -Tile $tiles -LeftTileWidth 460

    # Last 7 days from the Security log, event 4672.
    New-BGInfoChart @panel -Id 'signins' -Title 'Privileged sign-ins' `
        -Kind Bar -Values 4, 6, 3, 9, 5, 2, 7 -LineColor $accent `
        -ShowGrid -GridLineCount 3 -ValueSuffix ' today' `
        -Width 560 -Height 270 -Anchor TopRight `
        -OffsetX $hud.MarginX -OffsetY $hud.MarginY

    $diagram = $hud.Diagram
    New-BGInfoTopology @diagram -Title 'Allowed admin path' `
        -Subtitle '2 hops to tier 0' -Layout DenseGrouped -Scale $diagramScale `
        -NodeDisplayMode Card -Width $column -Height $diagramHeight `
        -Anchor TopRight -OffsetX $hud.MarginX -OffsetY $diagramTop `
        -TopologyDefinition {
        $node = @{ Status = 'Healthy' }
        New-BGInfoTopologyGroup -Id 'paw' -Label 'This workstation'
        New-BGInfoTopologyGroup -Id 'jump' -Label 'Jump hosts'
        New-BGInfoTopologyGroup -Id 't0' -Label 'Tier 0'
        New-BGInfoTopologyNode @node -Id 'paw' -Label 'PAW-T0-03' `
            -Subtitle 'Windows 11' -Kind Endpoint -GroupId 'paw'
        New-BGInfoTopologyNode @node -Id 'jmp' -Label 'JMP-T0-01' `
            -Subtitle 'Server 2025' -Kind Gateway -GroupId 'jump'
        New-BGInfoTopologyNode @node -Id 'dc1' -Label 'DC01' `
            -Subtitle 'PDC emulator' -Kind Server -GroupId 't0'
        New-BGInfoTopologyNode @node -Id 'ca' -Label 'CA01' `
            -Subtitle 'Enterprise CA' -Kind Certificate -GroupId 't0'
        New-BGInfoTopologyNode -Id 'dc2' -Label 'DC02' -Subtitle 'Replica' `
            -Kind Server -GroupId 't0' -Status Warning
        $link = @{ Kind = 'Connectivity'; Status = 'Healthy' }
        New-BGInfoTopologyEdge @link -SourceNodeId 'paw' -TargetNodeId 'jmp' `
            -Label 'RDP + MFA' -Direction Forward
        New-BGInfoTopologyEdge @link -SourceNodeId 'jmp' -TargetNodeId 'dc1' `
            -Direction Forward
        New-BGInfoTopologyEdge @link -SourceNodeId 'jmp' -TargetNodeId 'ca' `
            -Direction Forward
        New-BGInfoTopologyEdge -SourceNodeId 'dc1' -TargetNodeId 'dc2' `
            -Label '14 min lag' -Kind Replication -Status Warning
    }
    New-BGInfoChart @panel @bottom -Id 'changes' -Title 'Tier 0 changes' `
        -Kind Area -Values 2, 1, 4, 3, 8, 5, 3, 2, 6, 4 -LineColor $accent `
        -ShowGrid -GridLineCount 3 -ValueSuffix ' this hour' `
        -OffsetX ($hud.MarginX + $half + 24) -OffsetY $hud.MarginY
    New-BGInfoChart @panel @bottom -Id 'alerts' -Title 'Open security alerts' `
        -Kind ProgressBar -Values 1, 3, 6 -Labels 'High', 'Medium', 'Low' `
        -Palette $bad, $warn, $accent -Maximum 10 -NoProgressHandles `
        -ShowLatestValue:$false `
        -OffsetX $hud.MarginX -OffsetY $hud.MarginY
} @text -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "admin-workstation-$($Theme.ToLower()).png"
