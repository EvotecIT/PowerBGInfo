Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1') -Force

$examplesPath = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$sampleImage = Join-Path -Path $examplesPath -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg'
$outputDirectory = Join-Path -Path $examplesPath -ChildPath 'Output'

$tiles = @(
    New-BGInfoVisualCanvasTile -Side Left -IconKind Computer -SurfaceStyle Raised -Label HOSTNAME -Value '{{HostName}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind Network -SurfaceStyle Raised -Label 'IP ADDRESS' -Value '{{IPv4Address}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind OperatingSystem -SurfaceStyle Raised -Label 'OPERATING SYSTEM' -Value '{{OSName}}' -Detail '{{OSVersion}}'
    New-BGInfoVisualCanvasTile -Side Left -IconKind Shield -SurfaceStyle Raised -Label PATCHING -Value '89% compliant' -Detail '6 devices pending' -MiniChartKind Bars -MiniChartValues 70,75,79,83,86,89 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu -SurfaceStyle Raised -Label 'CPU LOAD' -Value '33% active' -Detail '{{CpuCores}} cores / {{CpuLogicalCores}} threads' -MiniChartKind Area -MiniChartValues 18,26,22,37,48,43,51,35,29,41,33 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Memory -SurfaceStyle Raised -Label 'MEMORY USE' -Value '13.2 GB used' -Detail '32 GB installed' -MiniChartKind Bars -MiniChartValues 36,39,41,42,44,43,41 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Storage -SurfaceStyle Raised -Label 'SYSTEM DRIVE' -Value '62% free' -Detail 'C: 238 GB available' -MiniChartKind Sparkline -MiniChartValues 66,65,64,64,63,62,62 -MiniChartMaximum 100
    New-BGInfoVisualCanvasTile -Side Right -IconKind Domain -SurfaceStyle Raised -Label DOMAIN -Value '{{UserDNSDomain}}'
)

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoVisualCanvas `
        -Title 'PowerBGInfo' `
        -Subtitle 'Raised sections on a dark background' `
        -Opaque `
        -NoTechBackdrop `
        -Tile $tiles
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.VisualCanvas.Raised3D.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black
