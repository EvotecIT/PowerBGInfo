$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = (Resolve-Path (Join-Path -Path $scriptRoot -ChildPath '..')).Path
$solutionPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.sln'
$modulePath = Join-Path -Path $repositoryRoot -ChildPath 'PowerBGInfo.psd1'
$samplePath = Join-Path -Path $scriptRoot -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg'
$outputDir = Join-Path -Path $scriptRoot -ChildPath 'Output'

# Build the current checkout so the example uses the latest source instead of stale binaries.
& dotnet build $solutionPath -c Debug --nologo -m:1 -nr:false | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for $solutionPath"
}

$loadedModule = Get-Module -Name PowerBGInfo -ErrorAction SilentlyContinue
foreach ($module in $loadedModule) {
    if ($module.Path -ne $modulePath) {
        Remove-Module -ModuleInfo $module -Force
    }
}
Import-Module -Name $modulePath -Force

$white = 'White'
$muted = '#D2DCE8E6'
$panel = '#0A101CAC'
$cyan = '#2DD4BF'
$blue = '#60A5FA'
$green = '#34D399'
$orange = '#FB923C'
$red = '#F87171'
$purple = '#A78BFA'

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoValue -Name 'PowerBGInfo' -Value 'ChartForgeX overlays' -Color LemonChiffon -ValueColor $white -FontSize 24 -ValueFontSize 18 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Host' -Value 'RDS-APP-042' -Color $muted -ValueColor $white
    New-BGInfoValue -Name 'Owner' -Value 'Platform Operations' -Color $muted -ValueColor $white

    New-BGInfoChart -Id 'cpu-area' -Title 'CPU trend' -Kind Area -Values 31,42,37,55,68,61,74,58,49,63 -ValueSuffix '%' -Width 390 -Height 155 -Anchor BottomLeft -OffsetX 32 -OffsetY 32 -LineColor $cyan -FillColor $cyan -TextColor $white -BackgroundColor $panel -ShowGrid -GridLineCount 3 -NoHistory
    New-BGInfoChart -Id 'memory-line' -Title 'Memory trend' -Kind Line -Values 48,51,55,57,60,62,59,64,66,69 -ValueSuffix '%' -Width 390 -Height 155 -Anchor BottomLeft -OffsetX 442 -OffsetY 32 -LineColor $blue -TextColor $white -BackgroundColor $panel -ShowGrid -GridLineCount 3 -NoHistory
    New-BGInfoChart -Id 'disk-donut' -Title 'Disk C' -Kind Donut -Values 72,28 -Labels 'Used','Free' -ValueSuffix '%' -Width 350 -Height 240 -Anchor BottomRight -OffsetX 32 -OffsetY 32 -Palette $red,$green -TextColor $white -BackgroundColor $panel -ShowLegend -ShowPointLegend -LegendPosition Right -ShowDataLabels -Maximum 100 -DonutCenterValue '72%' -DonutCenterLabel 'Used' -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'patch-target' -Title 'Fleet patch compliance' -Kind Bullet -Values 89 -Target 95 -RangeEnds 70,85 -Width 455 -Height 210 -Anchor TopLeft -OffsetX 32 -OffsetY 32 -LineColor $orange -TextColor $white -BackgroundColor $panel -Maximum 100 -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'service-pictorial' -Title 'Critical services' -Kind Pictorial -Values 9,1 -Labels 'Running','Stopped' -Width 455 -Height 180 -Anchor TopLeft -OffsetX 32 -OffsetY 266 -Palette $green,$red -TextColor $white -BackgroundColor $panel -PictorialSymbol Person -PictorialColumns 10 -ShowDataLabels -Maximum 10 -ShowLatestValue:$false -NoHistory
} -FilePath $samplePath `
    -ConfigurationDirectory $outputDir `
    -OutputFileName 'PowerBGInfo.ChartForgeX.Showcase.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color $muted `
    -ValueColor $white `
    -TextPosition TopRight `
    -SpaceX 42 `
    -SpaceY 42

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoValue -Name 'NODE-23' -Value 'Production gateway' -Color LemonChiffon -ValueColor $white -FontSize 22 -ValueFontSize 16 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Window' -Value 'Maintenance 22:00-23:00 UTC' -Color $muted -ValueColor $white

    New-BGInfoChart -Id 'compact-cpu' -Title 'CPU' -Kind Gauge -Values 64 -ValueSuffix '%' -Width 310 -Height 150 -LineColor $orange -TextColor $white -BackgroundColor $panel -Maximum 100 -NoHistory
    New-BGInfoChart -Id 'compact-memory' -Title 'Memory' -Kind Circle -Values 71 -ValueSuffix '%' -Width 310 -Height 150 -LineColor $purple -TextColor $white -BackgroundColor $panel -Maximum 100 -NoHistory
    New-BGInfoChart -Id 'compact-io' -Title 'Network I/O' -Kind Sparkline -Values 12,19,16,31,28,34,42,37,45,39 -ValueSuffix ' Mbps' -Width 310 -Height 110 -LineColor $cyan -TextColor $white -BackgroundColor $panel -ShowGrid -GridLineCount 3 -NoHistory
} -FilePath $samplePath `
    -ConfigurationDirectory $outputDir `
    -OutputFileName 'PowerBGInfo.ChartForgeX.Compact.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color $muted `
    -ValueColor $white `
    -TextPosition TopLeft `
    -SpaceX 42 `
    -SpaceY 42 `
    -ChartLayout Stack `
    -ChartStackAnchor BottomRight `
    -ChartStackDirection Vertical `
    -ChartStackSpacing 12 `
    -ChartStackOffsetX 32 `
    -ChartStackOffsetY 32

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoValue -Name 'EDGE-07' -Value 'Transparent chart ink' -Color LemonChiffon -ValueColor $white -FontSize 22 -ValueFontSize 16 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Mode' -Value 'No chart panels' -Color $muted -ValueColor $white

    New-BGInfoChart -Id 'melt-cpu' -Title 'CPU trend' -Kind Area -Values 28,34,31,44,52,49,57,63,58,66 -ValueSuffix '%' -Width 430 -Height 155 -Anchor BottomLeft -OffsetX 34 -OffsetY 34 -LineColor $cyan -FillColor $cyan -TextColor $white -GridColor $muted -ShowGrid -GridLineCount 3 -NoHistory
    New-BGInfoChart -Id 'melt-disk' -Title 'Disk C' -Kind Donut -Values 72,28 -Labels 'Used','Free' -ValueSuffix '%' -Width 350 -Height 240 -Anchor BottomRight -OffsetX 42 -OffsetY 36 -Palette $red,$green -TextColor $white -ShowLegend -ShowPointLegend -LegendPosition Right -ShowDataLabels -Maximum 100 -DonutCenterValue '72%' -DonutCenterLabel 'Used' -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'melt-patch-target' -Title 'Fleet patch compliance' -Kind Bullet -Values 89 -Target 95 -RangeEnds 70,85 -Width 480 -Height 205 -Anchor TopLeft -OffsetX 34 -OffsetY 34 -LineColor $orange -TextColor $white -Maximum 100 -ShowLatestValue:$false -NoHistory
} -FilePath $samplePath `
    -ConfigurationDirectory $outputDir `
    -OutputFileName 'PowerBGInfo.ChartForgeX.Transparent.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color $muted `
    -ValueColor $white `
    -TextPosition TopRight `
    -SpaceX 42 `
    -SpaceY 42

$canvasTiles = @(
    New-BGInfoVisualCanvasTile -Side Left -IconKind Computer -SurfaceStyle Raised -Label HOSTNAME -Value 'CFX-DEMO-01' -Detail 'chartforgex raster path' -Accent $cyan
    New-BGInfoVisualCanvasTile -Side Left -IconKind Network -SurfaceStyle Raised -Label NETWORK -Value '10.42.7.24' -Detail 'gateway healthy' -Accent $blue
    New-BGInfoVisualCanvasTile -Side Left -IconKind Shield -SurfaceStyle Raised -Label POSTURE -Value '96% compliant' -Detail '2 findings muted' -Progress 0.96 -MiniChartKind Bars -MiniChartValues 70,74,82,89,92,96 -MiniChartMaximum 100 -Accent $green
    New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Raised -Label CPU -Value '34% active' -Detail '8 cores / 16 threads' -Progress 0.34 -MiniChartKind Area -MiniChartValues 18,22,31,28,35,34 -MiniChartMaximum 100 -Accent $orange
    New-BGInfoVisualCanvasTile -Side Right -IconKind Memory -SurfaceStyle Raised -Label MEMORY -Value '41% used' -Detail '32 GB installed' -Progress 0.41 -MiniChartKind Sparkline -MiniChartValues 35,36,38,42,40,41 -MiniChartMaximum 100 -Accent $purple
    New-BGInfoVisualCanvasTile -Side Right -IconKind Storage -SurfaceStyle Raised -Label STORAGE -Value '62% free' -Detail 'C: healthy' -Progress 0.62 -MiniChartKind Bars -MiniChartValues 66,64,63,62,62,62 -MiniChartMaximum 100 -Accent $cyan
)

$canvasFeatures = @(
    New-BGInfoVisualCanvasFeature -Icon 'PNG' -Label 'ChartForgeX composition'
    New-BGInfoVisualCanvasFeature -Icon 'HUD' -Label 'mini charts'
    New-BGInfoVisualCanvasFeature -Icon 'A+' -Label 'high contrast'
)

New-BGInfo -MonitorIndex 0 -Target File {
    $canvasMode = @{
        # Opaque = $true
    }

    New-BGInfoVisualCanvas @canvasMode `
        -Title 'PowerBGInfo' `
        -Subtitle 'Transparent ChartForgeX HUD overlay rendered through PowerBGInfo' `
        -Width 2560 `
        -Height 1080 `
        -Tile $canvasTiles `
        -Feature $canvasFeatures `
        -FeatureAnchor BottomCenter `
        -FeatureWidth 920 `
        -FeatureHeight 72 `
        -FeatureOffsetY 44 `
        -BackgroundTop '#07111EFF' `
        -BackgroundBottom '#123042FF' `
        -Accent $cyan `
        -SecondaryAccent $orange `
        -TileGlassTop '#13324ACC' `
        -TileGlassBottom '#081827E6' `
        -TileLabelColor '#B7D3E7FF' `
        -TileValueColor '#FFFFFFFF' `
        -TileDetailColor '#9EB2C4FF' `
        -NoTechBackdrop
} -FilePath $samplePath `
    -ConfigurationDirectory $outputDir `
    -OutputFileName 'PowerBGInfo.ChartForgeX.CanvasDashboard.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color $muted `
    -ValueColor $white

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoValue -Name 'PowerBGInfo' -Value 'anchor + topology confirmation' -Color LemonChiffon -ValueColor $white -FontSize 23 -ValueFontSize 17 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Engine' -Value 'ChartForgeX image composition' -Color $muted -ValueColor $white

    New-BGInfoChart -Id 'placement-top-left' -Title 'Top left area' -Kind Area -Values 18,25,22,31,44,38,52 -ValueSuffix '%' -Width 360 -Height 145 -Anchor TopLeft -OffsetX 34 -OffsetY 34 -LineColor $cyan -FillColor $cyan -TextColor $white -BackgroundColor $panel -ShowGrid -GridLineCount 3 -NoHistory
    New-BGInfoChart -Id 'placement-top-right' -Title 'Top right bullet' -Kind Bullet -Values 91 -Target 95 -RangeEnds 70,85 -Width 410 -Height 145 -Anchor TopRight -OffsetX 34 -OffsetY 34 -LineColor $orange -TextColor $white -BackgroundColor $panel -Maximum 100 -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'placement-bottom-left' -Title 'Bottom left bars' -Kind Bar -Values 12,18,16,23,20 -Labels 'A','B','C','D','E' -Width 360 -Height 145 -Anchor BottomLeft -OffsetX 34 -OffsetY 34 -LineColor $blue -TextColor $white -BackgroundColor $panel -ShowDataLabels -NoHistory
    New-BGInfoChart -Id 'placement-bottom-right' -Title 'Bottom right donut' -Kind Donut -Values 64,36 -Labels 'Used','Free' -ValueSuffix '%' -Width 360 -Height 230 -Anchor BottomRight -OffsetX 34 -OffsetY 34 -Palette $red,$green -TextColor $white -BackgroundColor $panel -ShowLegend -ShowPointLegend -LegendPosition Right -ShowDataLabels -Maximum 100 -DonutCenterValue '64%' -DonutCenterLabel 'Used' -ShowLatestValue:$false -NoHistory

    New-BGInfoTopology -Title 'Service flow' -Subtitle 'anchored topology layer' -Width 680 -Height 330 -Anchor MiddleCenter -OffsetX 0 -OffsetY 26 -Layout Layered -Direction LeftToRight -Theme Dark -ShowLegend -TopologyDefinition {
        New-BGInfoTopologyGroup -Id 'edge' -Label 'Edge' -Status Healthy -Symbol EDGE
        New-BGInfoTopologyGroup -Id 'core' -Label 'Core' -Status Warning -Symbol CORE
        New-BGInfoTopologyNode -Id 'gw' -Label 'Gateway' -Kind Network -Status Healthy -GroupId 'edge' -Symbol GW
        New-BGInfoTopologyNode -Id 'api' -Label 'API' -Kind Service -Status Healthy -GroupId 'core' -Symbol API
        New-BGInfoTopologyNode -Id 'db' -Label 'SQL' -Kind Database -Status Warning -GroupId 'core' -Symbol SQL
        New-BGInfoTopologyEdge -SourceNodeId 'gw' -TargetNodeId 'api' -Label 'TLS' -Kind Connectivity -Status Healthy -Direction Forward
        New-BGInfoTopologyEdge -SourceNodeId 'api' -TargetNodeId 'db' -Label '18 ms' -Kind Dependency -Status Warning -Direction Forward
    }
} -FilePath $samplePath `
    -ConfigurationDirectory $outputDir `
    -OutputFileName 'PowerBGInfo.ChartForgeX.PlacementProof.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color $muted `
    -ValueColor $white `
    -TextPosition TopCenter `
    -SpaceX 0 `
    -SpaceY 220
