Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1') -Force

$examplesPath = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$configurationDirectory = Join-Path -Path $examplesPath -ChildPath 'Configuration'
$configurationPath = Join-Path -Path $configurationDirectory -ChildPath 'PowerBGInfo.VisualCanvas.ContrastBox.json'

$darkTokens = [ChartForgeX.Themes.VisualDesignTokens]::GraphiteDark()
$lightTokens = [ChartForgeX.Themes.VisualDesignTokens]::GraphiteLight()
# Light cards keep their own dark text; hero text remains readable over the dark wallpaper.
$palette = @{
    ThemeMode        = 'Light'
    TitleColor       = $darkTokens.Foreground
    TitleAccentColor = $darkTokens.Accent
    SubtitleColor    = $darkTokens.MutedForeground
}
$tiles = @(
    New-BGInfoVisualCanvasTile -Side Left -IconKind Computer -SurfaceStyle Raised -Label HOSTNAME -Value '{{HostName}}' -Detail 'production desktop'
    New-BGInfoVisualCanvasTile -Side Left -IconKind Network -SurfaceStyle Raised -Label 'IP ADDRESS' -Value '{{IPv4Address}}' -Detail 'primary adapter'
    New-BGInfoVisualCanvasTile -Side Left -IconKind OperatingSystem -SurfaceStyle Raised -Label 'OPERATING SYSTEM' -Value '{{OSName}}' -Detail '{{OSVersion}}' -TextFitPolicy WrapThenShrink
    New-BGInfoVisualCanvasTile -Side Left -IconKind Shield -SurfaceStyle Raised -Label 'PATCH STATUS' -Value '94% compliant' -Detail 'last scan 08:42' -Progress 0.94 -Accent $lightTokens.Positive
    New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Raised -Label 'CPU LOAD' -Value '31% active' -Detail '{{CpuCores}} cores / {{CpuLogicalCores}} threads' -MiniChartKind Area -MiniChartValues 22,28,25,36,31,42,38,34,31 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Memory -SurfaceStyle Raised -Label 'MEMORY USE' -Value '11.8 GB used' -Detail '{{RAMSize}} installed' -MiniChartKind Bars -MiniChartValues 36,38,42,41,45,43,39 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Storage -SurfaceStyle Raised -Label 'SYSTEM DRIVE' -Value '62% free' -Detail 'C: 238 GB available' -Progress 0.62
    New-BGInfoVisualCanvasTile -Side Right -IconKind User -SurfaceStyle Raised -Label USER -Value '{{UserName}}' -Detail '{{UserDNSDomain}}'
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
        -LayoutPreset WideRails `
        @palette `
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
