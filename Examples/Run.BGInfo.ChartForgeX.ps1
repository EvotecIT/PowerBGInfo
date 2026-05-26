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
