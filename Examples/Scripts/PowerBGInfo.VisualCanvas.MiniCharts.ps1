Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1') -Force

$examplesPath = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$sampleImage = Join-Path -Path $examplesPath -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg'
$logoImage = Join-Path -Path $examplesPath -ChildPath 'Samples\LogoEvotec.png'
$outputDirectory = Join-Path -Path $examplesPath -ChildPath 'Output'

$palette = @{
    Accent                 = '#38BDF8'
    SecondaryAccent        = '#60A5FA'
    TitleColor             = '#F8FAFC'
    TitleAccentColor       = '#38BDF8'
    SubtitleColor          = '#E2E8F0'
    TileLabelColor         = '#C7D2FE'
    TileValueColor         = '#FFFFFF'
    TileDetailColor        = '#B6C4DA'
    TileGlassTop           = '#EE132444'
    TileGlassBottom        = '#D9040A18'
    TileProgressTrackColor = '#F0263A5E'
    HeroBadgeTop           = '#13284C'
    HeroBadgeBottom        = '#061225'
    HeroBadgeTextColor     = '#F8FBFF'
}

$chart = @{
    Background = '#A6081224'
    Text       = '#F8FAFC'
    Grid       = '#342F80FF'
    Cpu        = '#38BDF8'
    Memory     = '#60A5FA'
    Disk       = '#34D399'
    Patch      = '#FBBF24'
}

$tiles = @(
    New-BGInfoVisualCanvasTile -Side Left -IconKind Computer -SurfaceStyle Glass -Label HOSTNAME -Value '{{HostName}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind Network -SurfaceStyle Glass -Label 'IP ADDRESS' -Value '{{IPv4Address}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind OperatingSystem -SurfaceStyle Glass -Label 'OPERATING SYSTEM' -Value '{{OSName}}' -Detail '{{OSVersion}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind Shield -SurfaceStyle Glass -Label PATCHING -Value '89% compliant' -Progress 0.89
    New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Glass -Label CPU -Value '{{CpuCores}} cores / {{CpuLogicalCores}} threads' -Progress 0.28
    New-BGInfoVisualCanvasTile -Side Right -IconKind Memory -SurfaceStyle Glass -Label RAM -Value '{{RAMSize}}' -Progress 0.41
    New-BGInfoVisualCanvasTile -Side Right -IconKind Storage -SurfaceStyle Glass -Label 'SYSTEM DRIVE' -Value '62% free' -Progress 0.62
    New-BGInfoVisualCanvasTile -Side Right -IconKind Domain -SurfaceStyle Glass -Label DOMAIN -Value '{{UserDNSDomain}}'
)

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoVisualCanvas `
        -Title 'PowerBGInfo' `
        -Subtitle 'Mini charts stay below the hero and away from the side rails' `
        -Accent $palette.Accent `
        -SecondaryAccent $palette.SecondaryAccent `
        -TitleColor $palette.TitleColor `
        -TitleAccentColor $palette.TitleAccentColor `
        -SubtitleColor $palette.SubtitleColor `
        -TileLabelColor $palette.TileLabelColor `
        -TileValueColor $palette.TileValueColor `
        -TileDetailColor $palette.TileDetailColor `
        -TileGlassTop $palette.TileGlassTop `
        -TileGlassBottom $palette.TileGlassBottom `
        -TileProgressTrackColor $palette.TileProgressTrackColor `
        -HeroBadgeTop $palette.HeroBadgeTop `
        -HeroBadgeBottom $palette.HeroBadgeBottom `
        -HeroBadgeTextColor $palette.HeroBadgeTextColor `
        -Tile $tiles

    New-BGInfoChart -Id 'visual-cpu-spark' -Title 'CPU burst' -Kind Area -Values 18,26,22,37,48,43,51,35,29,41,33 -ValueSuffix '%' -Width 260 -Height 112 -Anchor BottomCenter -OffsetX -450 -OffsetY 210 -LineColor $chart.Cpu -FillColor $chart.Cpu -TextColor $chart.Text -BackgroundColor $chart.Background -GridColor $chart.Grid -ShowGrid -GridLineCount 3 -NoHistory
    New-BGInfoChart -Id 'visual-ram-ring' -Title 'Memory load' -Kind RadialBar -Values 41 -ValueSuffix '%' -Width 230 -Height 112 -Anchor BottomCenter -OffsetX -150 -OffsetY 210 -LineColor $chart.Memory -TextColor $chart.Text -BackgroundColor $chart.Background -Maximum 100 -NoHistory
    New-BGInfoChart -Id 'visual-disk-progress' -Title 'Disk free' -Kind ProgressBar -Values 62 -Labels 'C:' -ValueSuffix '%' -Width 260 -Height 112 -Anchor BottomCenter -OffsetX 145 -OffsetY 210 -LineColor $chart.Disk -TextColor $chart.Text -BackgroundColor $chart.Background -Maximum 100 -ProgressBarThicknessRatio 0.22 -NoProgressHandles -NoHistory
    New-BGInfoChart -Id 'visual-patch-target' -Title 'Patch target' -Kind Bullet -Values 89 -Target 95 -RangeEnds 70,85 -ValueSuffix '%' -Width 280 -Height 112 -Anchor BottomCenter -OffsetX 455 -OffsetY 210 -LineColor $chart.Patch -TextColor $chart.Text -BackgroundColor $chart.Background -Maximum 100 -ShowLatestValue:$false -NoHistory

    New-BGInfoImage -Path $logoImage -Width 210 -Anchor BottomRight -OffsetX 72 -OffsetY 54 -Opacity 0.92
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.VisualCanvas.MiniCharts.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black
