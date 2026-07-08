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
    HeroBadgeTop           = '#F8FAFC'
    HeroBadgeBottom        = '#CBD5E1'
    HeroBadgeTextColor     = '#0F172A'
}

$tiles = @(
    New-BGInfoVisualCanvasTile -Side Left -IconKind Computer -SurfaceStyle Glass -Label HOSTNAME -Value '{{HostName}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind Network -SurfaceStyle Glass -Label 'IP ADDRESS' -Value '{{IPv4Address}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind OperatingSystem -SurfaceStyle Glass -Label 'OPERATING SYSTEM' -Value '{{OSName}}' -Detail '{{OSVersion}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind Shield -SurfaceStyle Glass -Label PATCHING -Value '89% compliant' -Detail '6 devices pending' -MiniChartKind Bars -MiniChartValues 70,75,79,83,86,89 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Glass -Label 'CPU LOAD' -Value '33% active' -Detail '{{CpuCores}} cores / {{CpuLogicalCores}} threads' -MiniChartKind Area -MiniChartValues 18,26,22,37,48,43,51,35,29,41,33 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Memory -SurfaceStyle Glass -Label 'MEMORY USE' -Value '13.2 GB used' -Detail '32 GB installed' -MiniChartKind Bars -MiniChartValues 36,39,41,42,44,43,41 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Storage -SurfaceStyle Glass -Label 'SYSTEM DRIVE' -Value '62% free' -Detail 'C: 238 GB available' -MiniChartKind Sparkline -MiniChartValues 66,65,64,64,63,62,62 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Domain -SurfaceStyle Glass -Label DOMAIN -Value '{{UserDNSDomain}}'
)

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoVisualCanvas `
        -Title 'PowerBGInfo' `
        -Subtitle 'Each section pairs the current value with a matching recent trend' `
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
        -HeroBadgeImagePath $logoImage `
        -HeroBadgeImageFit Contain `
        -HeroBadgeImagePadding 14 `
        -Tile $tiles
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.VisualCanvas.MiniCharts.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black
