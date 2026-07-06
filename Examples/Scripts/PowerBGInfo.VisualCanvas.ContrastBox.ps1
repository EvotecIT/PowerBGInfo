Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1') -Force

$examplesPath = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$configurationDirectory = Join-Path -Path $examplesPath -ChildPath 'Configuration'
$configurationPath = Join-Path -Path $configurationDirectory -ChildPath 'PowerBGInfo.VisualCanvas.ContrastBox.json'

$palette = @{
    Accent                 = '#0F766EFF'
    SecondaryAccent        = '#F97316FF'
    TitleColor             = '#F8FAFCFF'
    TitleAccentColor       = '#5EEAD4FF'
    SubtitleColor          = '#E2E8F0FF'
    TileLabelColor         = '#475569FF'
    TileValueColor         = '#0F172AFF'
    TileDetailColor        = '#334155FF'
    TileGlassTop           = '#FFF7EDD9'
    TileGlassBottom        = '#DBEAFECC'
    TileProgressTrackColor = '#94A3B8A6'
    HeroBadgeTop           = '#0F766EFF'
    HeroBadgeBottom        = '#134E4AFF'
    HeroBadgeTextColor     = '#FFF7EDFF'
}

$tiles = @(
    New-BGInfoVisualCanvasTile -Side Left -IconKind Computer -SurfaceStyle Raised -Label HOSTNAME -Value '{{HostName}}' -Detail 'production desktop' -Accent '#0F766EFF'
    New-BGInfoVisualCanvasTile -Side Left -IconKind Network -SurfaceStyle Raised -Label 'IP ADDRESS' -Value '{{IPv4Address}}' -Detail 'primary adapter' -Accent '#2563EBFF'
    New-BGInfoVisualCanvasTile -Side Left -IconKind OperatingSystem -SurfaceStyle Raised -Label 'OPERATING SYSTEM' -Value '{{OSName}}' -Detail '{{OSVersion}}' -TextFitPolicy WrapThenShrink -Accent '#7C3AEDFF'
    New-BGInfoVisualCanvasTile -Side Left -IconKind Shield -SurfaceStyle Raised -Label 'PATCH STATUS' -Value '94% compliant' -Detail 'last scan 08:42' -Progress 0.94 -Accent '#16A34AFF'
    New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Raised -Label 'CPU LOAD' -Value '31% active' -Detail '{{CpuCores}} cores / {{CpuLogicalCores}} threads' -MiniChartKind Area -MiniChartValues 22,28,25,36,31,42,38,34,31 -MiniChartMaximum 100 -Accent '#F97316FF'
    New-BGInfoVisualCanvasTile -Side Right -IconKind Memory -SurfaceStyle Raised -Label 'MEMORY USE' -Value '11.8 GB used' -Detail '{{RAMSize}} installed' -MiniChartKind Bars -MiniChartValues 36,38,42,41,45,43,39 -MiniChartMaximum 100 -Accent '#DB2777FF'
    New-BGInfoVisualCanvasTile -Side Right -IconKind Storage -SurfaceStyle Raised -Label 'SYSTEM DRIVE' -Value '62% free' -Detail 'C: 238 GB available' -Progress 0.62 -Accent '#0EA5E9FF'
    New-BGInfoVisualCanvasTile -Side Right -IconKind User -SurfaceStyle Raised -Label USER -Value '{{UserName}}' -Detail '{{UserDNSDomain}}' -Accent '#CA8A04FF'
)

$features = @(
    New-BGInfoVisualCanvasFeature -Icon 'A+' -Label 'light contrast boxes'
    New-BGInfoVisualCanvasFeature -Icon 'CFX' -Label 'ChartForgeX canvas'
    New-BGInfoVisualCanvasFeature -Icon 'JSON' -Label 'portable config'
)

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoVisualCanvas `
        -Title 'PowerBGInfo' `
        -Subtitle 'High-contrast information boxes over a real wallpaper' `
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
        -LayoutPreset WideRails `
        -FeatureAnchor BottomRight `
        -FeatureWidth 610 `
        -FeatureOffsetX 165 `
        -FeatureOffsetY 120 `
        -TileWidth 420 `
        -TileHeight 132 `
        -TileGap 22 `
        -RightTileWidth 390 `
        -TileTextFitPolicy WrapThenShrink `
        -Tile $tiles `
        -Feature $features
} -FilePath '..\Samples\TapC-Evotec-2560x1080.jpg' `
    -ConfigurationDirectory '..\Output' `
    -OutputFileName 'PowerBGInfo.VisualCanvas.ContrastBox.jpg' `
    -JsonPath $configurationPath `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -ExportOnly | Out-Null

$json = [System.IO.File]::ReadAllText($configurationPath) -replace "`r`n", "`n"
[System.IO.File]::WriteAllText($configurationPath, $json, [System.Text.UTF8Encoding]::new($false))

Invoke-BGInfo -Path $configurationPath -NoApply
