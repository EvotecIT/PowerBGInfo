$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = (Resolve-Path (Join-Path -Path $scriptRoot -ChildPath '..')).Path
$solutionPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.sln'
$modulePath = Join-Path -Path $repositoryRoot -ChildPath 'PowerBGInfo.psd1'
$examplesRoot = Join-Path -Path $repositoryRoot -ChildPath 'Examples'
$outputDirectory = Join-Path -Path $examplesRoot -ChildPath 'Output'

& dotnet build $solutionPath -c Debug --nologo -m:1 -nr:false | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for $solutionPath"
}

foreach ($module in Get-Module -Name PowerBGInfo -ErrorAction SilentlyContinue) {
    if ($module.Path -ne $modulePath) {
        Remove-Module -ModuleInfo $module -Force
    }
}
Import-Module -Name $modulePath -Force

New-BGInfo -MonitorIndex 0 -Target File {
    $c = @{
        Ink = '#F8FAFCFF'; Soft = '#CBD5E1E6'; Panel = '#08111FCC'
        Cyan = '#2DD4BFFF'; Blue = '#60A5FAFF'; Green = '#34D399FF'; Amber = '#F59E0BFF'
    }

    $tiles = @(
        New-BGInfoVisualCanvasTile -Side Left -IconKind Shield -SurfaceStyle Raised -Label 'SECURITY POSTURE' -Value '96% protected' -Detail '2 policy drifts' -Progress 0.96 -MiniChartKind Bars -MiniChartValues 82,85,88,91,94,96 -MiniChartMaximum 100 -Accent $c.Green
        New-BGInfoVisualCanvasTile -Side Left -IconKind Network -SurfaceStyle Raised -Label 'EDGE HEALTH' -Value 'All gateways online' -Detail 'Berlin / Warsaw / Azure' -MiniChartKind Sparkline -MiniChartValues 31,28,29,27,30,26 -MiniChartMaximum 100 -Accent $c.Cyan
        New-BGInfoVisualCanvasTile -Side Left -IconKind Domain -SurfaceStyle Raised -Label 'TENANT' -Value 'Production' -Detail 'break-glass checked' -Accent $c.Blue
        New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Raised -Label 'SIGNAL VOLUME' -Value '1.8k/min' -Detail 'normal window' -Progress 0.42 -MiniChartKind Area -MiniChartValues 18,22,28,25,33,29 -MiniChartMaximum 100 -Accent $c.Amber
        New-BGInfoVisualCanvasTile -Side Right -IconKind Memory -SurfaceStyle Raised -Label 'QUEUE' -Value '11 pending' -Detail 'SLA green' -Progress 0.18 -MiniChartKind Bars -MiniChartValues 18,14,13,17,12,11 -MiniChartMaximum 60 -Accent $c.Cyan
        New-BGInfoVisualCanvasTile -Side Right -IconKind Storage -SurfaceStyle Raised -Label 'EVIDENCE' -Value '7 days local' -Detail 'shipper active' -Progress 0.74 -Accent $c.Green
    )
    $canvasMode = @{
        # Opaque = $true
    }

    New-BGInfoVisualCanvas @canvasMode `
        -Title 'PowerBGInfo' `
        -Subtitle 'Security operations desktop with live-looking local status' `
        -Tile $tiles `
        -Feature @(
            New-BGInfoVisualCanvasFeature -Icon 'SOC' -Label 'shift handover'
            New-BGInfoVisualCanvasFeature -Icon 'MFA' -Label 'admin-ready'
            New-BGInfoVisualCanvasFeature -Icon 'SIEM' -Label 'signals flowing'
        ) `
        -FeatureAnchor BottomCenter `
        -FeatureWidth 880 `
        -FeatureHeight 72 `
        -FeatureOffsetY 42 `
        -BackgroundTop '#06111DFF' `
        -BackgroundBottom '#102D3AFF' `
        -Accent $c.Cyan `
        -SecondaryAccent $c.Amber `
        -TileGlassTop '#12334ACC' `
        -TileGlassBottom '#061421E8' `
        -TileLabelColor $c.Soft `
        -TileValueColor $c.Ink `
        -TileDetailColor '#94A3B8FF' `
        -NoTechBackdrop
        # -Opaque
} -FilePath (Join-Path -Path $examplesRoot -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg') `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.Cool.SecurityOps.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black

New-BGInfo -MonitorIndex 0 -Target File {
    $c = @{
        Ink = '#F8FAFCFF'; Soft = '#D2DCE8E6'; Panel = '#0A101CCC'
        Cyan = '#38BDF8FF'; Green = '#22C55EFF'; Amber = '#F97316FF'; Red = '#F87171FF'
    }

    New-BGInfoValue -Name 'Build Agent 07' -Value 'Windows release validation' -Color LemonChiffon -ValueColor $c.Ink -FontSize 25 -ValueFontSize 17 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Pool' -Value 'Release-Windows' -Color $c.Soft -ValueColor $c.Ink
    New-BGInfoValue -Name 'Last run' -Value 'PowerBGInfo #4821 passed' -Color $c.Green -ValueColor $c.Ink
    New-BGInfoValue -Name 'Workspace' -Value 'Cleaned after package build' -Color $c.Soft -ValueColor $c.Ink

    New-BGInfoChart -Id 'cool-build-queue' -Title 'Queue depth' -Kind Area -Values 8,6,7,5,3,4,2,2,1,2 -Width 430 -Height 150 -Anchor BottomLeft -OffsetX 34 -OffsetY 34 -LineColor $c.Cyan -FillColor $c.Cyan -TextColor $c.Ink -BackgroundColor $c.Panel -ShowGrid -GridLineCount 3 -NoHistory
    New-BGInfoChart -Id 'cool-build-tests' -Title 'Pass target' -Kind Bullet -Values 98 -Target 97 -RangeEnds 85,94 -Width 460 -Height 150 -Anchor BottomRight -OffsetX 34 -OffsetY 34 -LineColor $c.Green -TextColor $c.Ink -BackgroundColor $c.Panel -Maximum 100 -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'cool-build-disk' -Title 'Workspace disk' -Kind Donut -Values 61,39 -Labels 'Used','Free' -ValueSuffix '%' -Width 360 -Height 225 -Anchor BottomRight -OffsetX 34 -OffsetY 196 -Palette $c.Amber,$c.Green -TextColor $c.Ink -BackgroundColor $c.Panel -ShowLegend -ShowPointLegend -LegendPosition Right -ShowDataLabels -Maximum 100 -DonutCenterValue '61%' -DonutCenterLabel 'Used' -ShowLatestValue:$false -NoHistory
} -FilePath (Join-Path -Path $examplesRoot -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg') `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.Cool.BuildAgent.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color '#D2DCE8E6' `
    -ValueColor White `
    -ValueWrapWidth 430 `
    -TextPosition TopLeft `
    -SpaceX 42 `
    -SpaceY 42

New-BGInfo -MonitorIndex 0 -Target File {
    $c = @{
        Ink = '#F8FAFCFF'; Soft = '#E0F2F1E6'; Panel = '#021916CC'
        Mint = '#5EEAD4FF'; Green = '#34D399FF'; Blue = '#7DD3FCFF'; Pink = '#F9A8D4FF'
    }

    New-BGInfoValue -Name 'Training Station 12' -Value 'GPO troubleshooting exercise' -Color LemonChiffon -ValueColor $c.Ink -FontSize 25 -ValueFontSize 17 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Goal' -Value 'Find policy precedence break' -Color $c.Soft -ValueColor $c.Ink
    New-BGInfoValue -Name 'Start here' -Value 'gpresult /h report.html' -Color $c.Mint -ValueColor $c.Ink
    New-BGInfoValue -Name 'Hint' -Value 'Check OU link order before editing anything.' -Color $c.Soft -ValueColor $c.Ink

    New-BGInfoChart -Id 'cool-training-progress' -Title 'Exercise progress' -Kind ProgressBar -Values 40 -Labels 'Complete' -Width 460 -Height 132 -Anchor BottomRight -OffsetX 36 -OffsetY 36 -LineColor $c.Pink -TextColor $c.Ink -BackgroundColor $c.Panel -Maximum 100 -NoProgressHandles -ShowDataLabels -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'cool-training-checks' -Title 'Checkpoints' -Kind Pictorial -Values 2,3 -Labels 'Done','Open' -Width 460 -Height 150 -Anchor BottomRight -OffsetX 36 -OffsetY 188 -Palette $c.Green,$c.Blue -TextColor $c.Ink -BackgroundColor $c.Panel -PictorialSymbol Diamond -PictorialColumns 5 -ShowDataLabels -Maximum 5 -ShowLatestValue:$false -NoHistory
    New-BGInfoImage -Path (Join-Path -Path $examplesRoot -ChildPath 'Samples\LogoEvotec.png') -Width 130 -Anchor BottomLeft -OffsetX 46 -OffsetY 46 -Opacity 0.82
} -FilePath (Join-Path -Path $examplesRoot -ChildPath 'Samples\TapN-Evotec-1600x900.jpg') `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.Cool.TrainingKiosk.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color '#E0F2F1E6' `
    -ValueColor White `
    -ValueWrapWidth 430 `
    -TextPosition TopLeft `
    -SpaceX 38 `
    -SpaceY 38

New-BGInfo -MonitorIndex 0 -Target File {
    $c = @{
        Ink = '#FFFFFFFF'; Soft = '#D8DEE9E6'; Panel = '#101827D9'
        Violet = '#A78BFAFF'; Cyan = '#22D3EEFF'; Green = '#4ADE80FF'; Amber = '#FBBF24FF'
    }

    New-BGInfoValue -Name 'Executive Brief' -Value 'Platform reliability snapshot' -Color LemonChiffon -ValueColor $c.Ink -FontSize 25 -ValueFontSize 17 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Service score' -Value '99.94% available' -Color $c.Green -ValueColor $c.Ink
    New-BGInfoValue -Name 'Risk' -Value 'One storage cluster nearing threshold' -Color $c.Amber -ValueColor $c.Ink
    New-BGInfoValue -Name 'Next action' -Value 'Capacity review Wednesday 10:00' -Color $c.Soft -ValueColor $c.Ink

    New-BGInfoChart -Id 'cool-exec-slo' -Title 'SLO' -Kind Gauge -Values 99.94 -ValueSuffix '%' -Width 360 -Height 165 -Anchor BottomLeft -OffsetX 42 -OffsetY 42 -LineColor $c.Green -TextColor $c.Ink -BackgroundColor $c.Panel -Maximum 100 -NoHistory
    New-BGInfoChart -Id 'cool-exec-cost' -Title 'Cost trend' -Kind Line -Values 42,41,43,46,45,47,46,44,43 -Width 420 -Height 150 -Anchor BottomCenter -OffsetX 0 -OffsetY 42 -LineColor $c.Cyan -TextColor $c.Ink -BackgroundColor $c.Panel -ShowGrid -GridLineCount 3 -NoHistory
    New-BGInfoChart -Id 'cool-exec-risk' -Title 'Risk mix' -Kind Pie -Values 72,18,10 -Labels 'Healthy','Watch','Action' -Width 390 -Height 250 -Anchor BottomRight -OffsetX 42 -OffsetY 42 -Palette $c.Green,$c.Amber,$c.Violet -TextColor $c.Ink -BackgroundColor $c.Panel -ShowLegend -ShowPointLegend -LegendPosition Right -ShowDataLabels -ShowLatestValue:$false -NoHistory
} -FilePath (Join-Path -Path $examplesRoot -ChildPath 'Samples\TapN-Evotec-2048x1536.jpg') `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.Cool.ExecutiveBrief.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color '#D8DEE9E6' `
    -ValueColor White `
    -ValueWrapWidth 460 `
    -TextPosition TopCenter `
    -SpaceX 0 `
    -SpaceY 56

New-BGInfo -MonitorIndex 0 -Target File {
    $c = @{
        Ink = '#F8FAFCFF'; Soft = '#CDE7F0E6'; Panel = '#07111FCC'
        Cyan = '#22D3EEFF'; Green = '#34D399FF'; Amber = '#FB923CFF'; Red = '#F87171FF'
    }

    New-BGInfoValue -Name 'Topology Desk' -Value 'Branch edge and service dependencies' -Color LemonChiffon -ValueColor $c.Ink -FontSize 24 -ValueFontSize 17 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Mode' -Value 'Ops view for service desk handover' -Color $c.Soft -ValueColor $c.Ink

    New-BGInfoTopology -Title 'Branch flow' -Subtitle 'Gateway, API, SQL, cache' -Width 760 -Height 360 -Anchor MiddleCenter -OffsetX 0 -OffsetY 24 -Theme Dark -ShowLegend -TopologyDefinition {
        New-BGInfoTopologyGroup -Id 'branch' -Label 'Branch' -Status Healthy -Symbol EDGE
        New-BGInfoTopologyGroup -Id 'core' -Label 'Core services' -Status Warning -Symbol CORE
        New-BGInfoTopologyNode -Id 'gw' -Label 'Gateway' -Kind Network -Status Healthy -GroupId 'branch' -Symbol GW
        New-BGInfoTopologyNode -Id 'api' -Label 'API' -Kind Service -Status Healthy -GroupId 'core' -Symbol API
        New-BGInfoTopologyNode -Id 'sql' -Label 'SQL' -Kind Database -Status Warning -GroupId 'core' -Symbol SQL
        New-BGInfoTopologyNode -Id 'cache' -Label 'Cache' -Kind Service -Status Healthy -GroupId 'core' -Symbol CACHE
        New-BGInfoTopologyEdge -SourceNodeId 'gw' -TargetNodeId 'api' -Label 'TLS' -Kind Connectivity -Status Healthy -Direction Forward
        New-BGInfoTopologyEdge -SourceNodeId 'api' -TargetNodeId 'sql' -Label '24 ms' -Kind Dependency -Status Warning -Direction Forward
        New-BGInfoTopologyEdge -SourceNodeId 'api' -TargetNodeId 'cache' -Label '4 ms' -Kind Dependency -Status Healthy -Direction Forward
    }
    New-BGInfoChart -Id 'cool-topology-latency' -Title 'Latency' -Kind Area -Values 18,21,19,24,27,24,22 -ValueSuffix ' ms' -Width 360 -Height 145 -Anchor TopRight -OffsetX 36 -OffsetY 36 -LineColor $c.Amber -FillColor $c.Amber -TextColor $c.Ink -BackgroundColor $c.Panel -ShowGrid -GridLineCount 3 -NoHistory
    New-BGInfoChart -Id 'cool-topology-health' -Title 'Node health' -Kind Pictorial -Values 3,1 -Labels 'Healthy','Warning' -Width 360 -Height 145 -Anchor BottomRight -OffsetX 36 -OffsetY 36 -Palette $c.Green,$c.Amber -TextColor $c.Ink -BackgroundColor $c.Panel -PictorialSymbol Circle -PictorialColumns 4 -ShowDataLabels -Maximum 4 -ShowLatestValue:$false -NoHistory
} -FilePath (Join-Path -Path $examplesRoot -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg') `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.Cool.TopologyDesk.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color '#CDE7F0E6' `
    -ValueColor White `
    -ValueWrapWidth 440 `
    -TextPosition TopLeft `
    -SpaceX 42 `
    -SpaceY 42
