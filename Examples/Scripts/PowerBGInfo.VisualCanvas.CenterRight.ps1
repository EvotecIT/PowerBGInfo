param(
    [string] $SampleFileName = 'TapC-Evotec-2560x1080.jpg',
    [string] $OutputFileName = 'PowerBGInfo.VisualCanvas.CenterRight.jpg'
)

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1') -Force

$examplesPath = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$sampleImage = Join-Path -Path $examplesPath -ChildPath "Samples\$SampleFileName"
$outputDirectory = Join-Path -Path $examplesPath -ChildPath 'Output'

$tiles = @(
    New-BGInfoVisualCanvasTile -Lane Center -IconKind Computer -SurfaceStyle Raised -Label HOSTNAME -Value '{{HostName}}' -Detail 'primary workstation'
    New-BGInfoVisualCanvasTile -Lane Center -IconKind Network -SurfaceStyle Raised -Label 'IP ADDRESS' -Value '{{IPv4Address}}' -Detail 'primary adapter'
    New-BGInfoVisualCanvasTile -Lane Center -IconKind OperatingSystem -SurfaceStyle Raised -Label 'OPERATING SYSTEM' -Value '{{OSName}}' -Detail '{{OSVersion}}'
    New-BGInfoVisualCanvasTile -Lane Right -IconKind Cpu -SurfaceStyle Raised -Label CPU -Value '{{CpuCores}} cores / {{CpuLogicalCores}} threads'
    New-BGInfoVisualCanvasTile -Lane Right -IconKind Memory -SurfaceStyle Raised -Label RAM -Value '{{RAMSize}}'
    New-BGInfoVisualCanvasTile -Lane Right -IconKind User -SurfaceStyle Raised -Label USER -Value '{{UserName}}' -Detail '{{UserDNSDomain}}'
)

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoVisualCanvas `
        -NoHeroContent `
        -Tile $tiles `
        -TileHeight 118 `
        -TileGap 24 `
        -CenterTileWidth 460 `
        -RightTileWidth 460
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName $OutputFileName `
    -WallpaperFit Fill `
    -BackgroundColor Black
