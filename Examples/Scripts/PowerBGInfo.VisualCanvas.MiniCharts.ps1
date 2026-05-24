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

$tiles = @(
    New-BGInfoVisualCanvasTile -Side Left -IconKind Computer -SurfaceStyle Glass -Label HOSTNAME -Value '{{HostName}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind Network -SurfaceStyle Glass -Label 'IP ADDRESS' -Value '{{IPv4Address}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind OperatingSystem -SurfaceStyle Glass -Label 'OPERATING SYSTEM' -Value '{{OSName}}' -Detail '{{OSVersion}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind Shield -SurfaceStyle Glass -Label PATCHING -Value '89% compliant' -Progress 0.89 -MiniChartKind Bars -MiniChartValues 70,75,79,83,86,89 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Glass -Label CPU -Value '{{CpuCores}} cores / {{CpuLogicalCores}} threads' -Progress 0.28 -MiniChartKind Area -MiniChartValues 18,26,22,37,48,43,51,35,29,41,33 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Memory -SurfaceStyle Glass -Label RAM -Value '{{RAMSize}}' -Progress 0.41 -MiniChartKind Bars -MiniChartValues 36,39,41,42,44,43,41 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Storage -SurfaceStyle Glass -Label 'SYSTEM DRIVE' -Value '62% free' -Progress 0.62 -MiniChartKind Sparkline -MiniChartValues 66,65,64,64,63,62,62 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Domain -SurfaceStyle Glass -Label DOMAIN -Value '{{UserDNSDomain}}'
)

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoVisualCanvas `
        -Title 'PowerBGInfo' `
        -Subtitle 'Mini charts live inside the matching desktop sections' `
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

    New-BGInfoImage -Path $logoImage -Width 210 -Anchor BottomRight -OffsetX 72 -OffsetY 54 -Opacity 0.92
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.VisualCanvas.MiniCharts.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black
