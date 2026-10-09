Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1') -Force

$examplesPath = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path
$sampleImage = Join-Path -Path $examplesPath -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg'
$outputDirectory = Join-Path -Path $examplesPath -ChildPath 'Output'

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
        -Tile $tiles
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.VisualCanvas.RaisedSections.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black
