Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1') -Force

$examplesPath = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$sampleImage = Join-Path -Path $examplesPath -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg'
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
    TileGlassTop           = '#F3132444'
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
    New-BGInfoVisualCanvasTile -Side Left -IconKind Storage -SurfaceStyle Glass -Label 'SYSTEM DRIVE' -Value '62% free' -Progress 0.62
    New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Glass -Label CPU -Value '{{CpuCores}} cores / {{CpuLogicalCores}} threads' -Progress 0.28
    New-BGInfoVisualCanvasTile -Side Right -IconKind Memory -SurfaceStyle Glass -Label RAM -Value '{{RAMSize}}' -Progress 0.41
    New-BGInfoVisualCanvasTile -Side Right -IconKind User -SurfaceStyle Glass -Label USER -Value '{{UserName}}'
    New-BGInfoVisualCanvasTile -Side Right -IconKind Domain -SurfaceStyle Glass -Label DOMAIN -Value '{{UserDNSDomain}}'
)

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoVisualCanvas `
        -Title 'PowerBGInfo' `
        -Subtitle 'Raised glass desktop sections over an existing wallpaper' `
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
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.VisualCanvas.RaisedSections.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black
