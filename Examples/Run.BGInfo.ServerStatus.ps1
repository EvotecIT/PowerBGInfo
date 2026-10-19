<#
    Server status wallpaper: who the server is, how long it has been up,
    and whether disks, services and the network need attention.

    Sample values keep the render reproducible. Each one notes the command
    that supplies it on a real server.
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
$accent = if ($dark) { '#2DD4BF' } else { '#0D9488' }

# (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
$uptime = New-TimeSpan -Days 41 -Hours 6

# Get-Volume | Where-Object DriveLetter
$volumes = [ordered]@{ 'C: System' = 61; 'D: Data' = 88; 'L: Logs' = 47 }

# Get-Counter '\Processor(_Total)\% Processor Time'
$cpu = 12, 28, 19, 44, 31, 38, 27, 23

# Get-Counter '\Network Interface(*)\Bytes Total/sec'
$network = 180, 210, 260, 240, 310, 420, 380, 350, 300, 312

$background = @{
    Width          = $Width
    Height         = $Height
    Color          = if ($dark) { '#04101A' } else { '#EAF4F4' }
    Gradient       = if ($dark) { '#062A33', '#050A14' } else {
        '#F7FBFB', '#DCEBEF'
    }
    Pattern        = 'Grid'
    PatternColor   = if ($dark) { '#FFFFFF0A' } else { '#0F172A0D' }
    PatternSpacing = 64
}
$glow = if ($dark) { '#14B8A655' } else { '#5EEAD480' }

$tile = @{ Side = 'Right'; SurfaceStyle = 'Glass' }
$tiles = @(
    New-BGInfoVisualCanvasTile @tile -IconKind Terminal -Label 'UPTIME' `
        -Value "$($uptime.Days) days $($uptime.Hours) h" `
        -Detail 'Since the September patch window' -Accent $ok
    New-BGInfoVisualCanvasTile @tile -IconKind Cpu -Label 'CPU' `
        -Value "$($cpu[-1]) %" -Detail '16 cores · 2.9 GHz' `
        -Accent $accent -MiniChartKind Area -MiniChartValues $cpu `
        -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile @tile -IconKind Memory -Label 'MEMORY' `
        -Value '100 of 128 GB' -Detail 'SQL Server buffer pool' `
        -Progress 0.78 -Accent $warn
    New-BGInfoVisualCanvasTile @tile -IconKind Network -Label 'NETWORK' `
        -Value "$($network[-1]) Mbps" -Detail '10.40.12.21 · 10 GbE' `
        -Accent $accent -MiniChartKind Sparkline -MiniChartValues $network
)

$text = $hud.Text
$canvas = $hud.Canvas
$panel = $hud.Panel
# Three equal panels share the bottom row with 24 px gutters.
$third = [int](($Width - 2 * $hud.MarginX - 48) / 3)
$bottom = @{ Width = $third; Height = 300; OffsetY = $hud.MarginY }

New-BGInfo -Target File {
    New-BGInfoBackground @background -Glow @(
        New-BGInfoBackgroundGlow -X 0.9 -Y 0.05 -Radius 0.55 -Color $glow
        New-BGInfoBackgroundGlow -X 0.05 -Y 1 -Radius 0.5 -Color $glow
    )

    New-BGInfoLabel -Name 'PRODUCTION  ·  SQL SERVER 2022' -Color $accent `
        -FontSize 15 -FontWeight 700
    New-BGInfoLabel -Name 'SQL-PROD-01' -Color $hud.Ink -FontSize 72 `
        -FontWeight 600
    New-BGInfoLabel -Name '●  1 warning · D: Data is 88 % full' `
        -Color $warn -FontSize 17 -FontWeight 600
    New-BGInfoValue -Name 'Role' -Value 'Primary replica · AG-FINANCE'
    New-BGInfoValue -Name 'Owner' -Value 'Data Platform team'
    New-BGInfoValue -Name 'Maintenance' -Value 'Sunday 02:00–04:00 UTC'
    New-BGInfoValue -Name 'On call' -Value 'dba-oncall@contoso.com'

    New-BGInfoVisualCanvas @canvas -Tile $tiles -RightTileWidth 440

    New-BGInfoChart @panel @bottom -Id 'volumes' -Title 'Volumes used' `
        -Kind ProgressBar -Values @($volumes.Values) `
        -Labels @($volumes.Keys) -Palette $ok, $warn, $ok -Maximum 100 `
        -ValueSuffix ' %' -NoProgressHandles -ShowLatestValue:$false `
        -Anchor BottomLeft -OffsetX $hud.MarginX
    New-BGInfoChart @panel @bottom -Id 'network' -Title 'Network' `
        -Kind Area -Values $network -LineColor $accent -ShowGrid `
        -GridLineCount 4 -ValueSuffix ' Mbps' `
        -Anchor BottomCenter -OffsetX 0
    New-BGInfoChart @panel @bottom -Id 'services' -Title 'Critical services' `
        -Kind Pictorial -Values 11, 1 -Labels 'Running', 'Stopped' `
        -Palette $ok, $bad -PictorialSymbol Circle -PictorialColumns 12 `
        -Maximum 12 -ShowDataLabels -ShowLatestValue:$false `
        -Anchor BottomRight -OffsetX $hud.MarginX
} @text -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "server-status-$($Theme.ToLower()).png"
