param(
    [string] $SampleFileName = 'TapC-Evotec-2560x1080.jpg',
    [string] $OutputFileName = 'PowerBGInfo.VisualCanvas.CenterRight.jpg'
)

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1') -Force

$examplesPath = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$sampleImage = Join-Path -Path $examplesPath -ChildPath "Samples\$SampleFileName"
$outputDirectory = Join-Path -Path $examplesPath -ChildPath 'Output'

$tiles = @(
    New-BGInfoVisualCanvasTile -Lane Center -IconKind Computer -SurfaceStyle Raised -Label HOSTNAME -Value '{{HostName}}' -Detail 'primary workstation' -Accent '#2DD4BFFF'
    New-BGInfoVisualCanvasTile -Lane Center -IconKind Network -SurfaceStyle Raised -Label 'IP ADDRESS' -Value '{{IPv4Address}}' -Detail 'primary adapter' -Accent '#60A5FAFF'
    New-BGInfoVisualCanvasTile -Lane Center -IconKind OperatingSystem -SurfaceStyle Raised -Label 'OPERATING SYSTEM' -Value '{{OSName}}' -Detail '{{OSVersion}}' -Accent '#A78BFAFF'
    New-BGInfoVisualCanvasTile -Lane Right -IconKind Cpu -SurfaceStyle Raised -Label CPU -Value '{{CpuCores}} cores / {{CpuLogicalCores}} threads' -Accent '#F97316FF'
    New-BGInfoVisualCanvasTile -Lane Right -IconKind Memory -SurfaceStyle Raised -Label RAM -Value '{{RAMSize}}' -Accent '#EC4899FF'
    New-BGInfoVisualCanvasTile -Lane Right -IconKind User -SurfaceStyle Raised -Label USER -Value '{{UserName}}' -Detail '{{UserDNSDomain}}' -Accent '#38BDF8FF'
)

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoVisualCanvas `
        -NoHeroContent `
        -Tile $tiles `
        -TileHeight 118 `
        -TileGap 24 `
        -CenterTileWidth 460 `
        -RightTileWidth 460 `
        -TileGlassTop '#E807152C' `
        -TileGlassBottom '#DC030A17' `
        -TileLabelColor '#C4D4EC' `
        -TileValueColor '#F8FAFC' `
        -TileDetailColor '#A8BAD4'
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName $OutputFileName `
    -WallpaperFit Fill `
    -BackgroundColor Black
