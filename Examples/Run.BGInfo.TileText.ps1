<#
    Shows the default tile text beside larger text. The theme scale follows
    the requested display size; explicit tile settings take precedence.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [ValidateRange(0.5, 4)]
    [double] $ThemeScale = 1,
    [ValidateRange(0.5, 4)]
    [double] $TileScale = 1.25,
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $BackgroundPath,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output')
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force

$data = [pscustomobject] @{
    HostName = 'DEV-WKS-01'
    OperatingSystem = 'Windows 11 Enterprise'
    Processor = 'Intel Core i7'
    Load = @(18, 26, 22, 37, 29, 23)
}

$look = New-BGInfoTheme Graphite -Mode $Theme -Scale $ThemeScale
$scaleName = $TileScale.ToString(
    '0.##', [Globalization.CultureInfo]::InvariantCulture)
$outputName = "tile-text-$($Theme.ToLower())-${Width}x${Height}-$scaleName.png"
$tiles = @(
    New-BGInfoVisualCanvasTile -Side Left -IconKind Computer `
        -Label 'HOSTNAME' -Value $data.HostName `
        -Detail $data.OperatingSystem
    New-BGInfoVisualCanvasTile -Side Right -IconKind Cpu `
        -Label 'PROCESSOR' -Value $data.Processor `
        -Detail 'Average load 23%' -TextScale $TileScale `
        -MiniChartKind Sparkline -MiniChartValues $data.Load
)

New-BGInfo -Theme $look -Target File -FilePath $BackgroundPath {
    New-BGInfoVisualCanvas -Width $Width -Height $Height -Tile $tiles
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName $outputName
