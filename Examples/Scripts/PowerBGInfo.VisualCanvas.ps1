Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1') -Force

$examplesPath = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$sampleImage = Join-Path -Path $examplesPath -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg'
$outputDirectory = Join-Path -Path $examplesPath -ChildPath 'Output'

$palette = @{
    Accent                 = '#2F80FF'
    SecondaryAccent        = '#22A7FF'
    TitleColor             = '#F8FAFC'
    TitleAccentColor       = '#2F80FF'
    SubtitleColor          = '#D8E3F4'
    TileLabelColor         = '#C4D4EC'
    TileValueColor         = '#F8FAFC'
    TileDetailColor        = '#A8BAD4'
    TileGlassTop           = '#E807152C'
    TileGlassBottom        = '#DC030A17'
    TileProgressTrackColor = '#EA18345D'
    HeroBadgeTop           = '#0B1C3A'
    HeroBadgeBottom        = '#051021'
    HeroBadgeTextColor     = '#E8F1FF'
}

function New-VisualCanvasTiles {
    param(
        [PowerBGInfo.BgInfoVisualCanvasTileSurfaceStyle] $SurfaceStyle,
        [switch] $UseIcons
    )

    $computerIcon = if ($UseIcons) { 'Computer' } else { 'Text' }
    $networkIcon = if ($UseIcons) { 'Network' } else { 'Text' }
    $osIcon = if ($UseIcons) { 'OperatingSystem' } else { 'Text' }
    $cpuIcon = if ($UseIcons) { 'Cpu' } else { 'Text' }
    $memoryIcon = if ($UseIcons) { 'Memory' } else { 'Text' }
    $userIcon = if ($UseIcons) { 'User' } else { 'Text' }
    $domainIcon = if ($UseIcons) { 'Domain' } else { 'Text' }

    @(
        New-BGInfoVisualCanvasTile -Side Left -Icon PC -IconKind $computerIcon -SurfaceStyle $SurfaceStyle -Label HOSTNAME -Value '{{HostName}}'
        New-BGInfoVisualCanvasTile -Side Left -Icon NET -IconKind $networkIcon -SurfaceStyle $SurfaceStyle -Label 'IP ADDRESS' -Value '{{IPv4Address}}'
        New-BGInfoVisualCanvasTile -Side Left -Icon OS -IconKind $osIcon -SurfaceStyle $SurfaceStyle -Label 'OPERATING SYSTEM' -Value '{{OSName}}' -Detail '{{OSVersion}}'
        New-BGInfoVisualCanvasTile -Side Right -Icon CPU -IconKind $cpuIcon -SurfaceStyle $SurfaceStyle -Label CPU -Value '{{CpuCores}} cores / {{CpuLogicalCores}} threads'
        New-BGInfoVisualCanvasTile -Side Right -Icon RAM -IconKind $memoryIcon -SurfaceStyle $SurfaceStyle -Label RAM -Value '{{RAMSize}}'
        New-BGInfoVisualCanvasTile -Side Right -Icon USER -IconKind $userIcon -SurfaceStyle $SurfaceStyle -Label USER -Value '{{UserName}}'
        New-BGInfoVisualCanvasTile -Side Right -Icon DNS -IconKind $domainIcon -SurfaceStyle $SurfaceStyle -Label DOMAIN -Value '{{UserDNSDomain}}'
    )
}

$variants = @(
    [pscustomobject]@{ Name = 'GlassText'; Tiles = New-VisualCanvasTiles -SurfaceStyle Glass; Output = 'PowerBGInfo.VisualCanvas.GlassText.jpg' }
    [pscustomobject]@{ Name = 'OutlineText'; Tiles = New-VisualCanvasTiles -SurfaceStyle Outline; Output = 'PowerBGInfo.VisualCanvas.OutlineText.jpg' }
    [pscustomobject]@{ Name = 'OutlineIcons'; Tiles = New-VisualCanvasTiles -SurfaceStyle Outline -UseIcons; Output = 'PowerBGInfo.VisualCanvas.OutlineIcons.jpg' }
)

foreach ($variant in $variants) {
    New-BGInfo -MonitorIndex 0 -Target File {
        New-BGInfoVisualCanvas `
            -Title 'PowerBGInfo' `
            -Subtitle 'Desktop background insights for Windows and PowerShell' `
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
            -Tile $variant.Tiles
    } -FilePath $sampleImage `
        -ConfigurationDirectory $outputDirectory `
        -OutputFileName $variant.Output `
        -WallpaperFit Fill `
        -BackgroundColor Black
}
