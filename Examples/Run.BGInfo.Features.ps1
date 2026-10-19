<#
    Renders every feature example under Examples\Features to image files in
    light and dark, at 1920x1080 and 2560x1440, and writes manifest.json.
    Regions are also rendered ultrawide (3440x1440) and portrait (1080x1920).
    Nothing is applied to the desktop: every example uses -Target File.
#>
param(
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output\Features'),
    [int[][]] $Sizes = @(@(1920, 1080), @(2560, 1440)),
    [string[]] $Themes = @('Light', 'Dark'),
    [string] $SourceCommit = ''
)

if (-not $SourceCommit) {
    $SourceCommit = (git -C $PSScriptRoot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Cannot read the feature source commit.' }
}
if ($SourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'SourceCommit must be the full Git commit SHA used to render the features.' }

# Fixed formatting keeps the published renders identical on every machine.
[System.Threading.Thread]::CurrentThread.CurrentCulture = [System.Globalization.CultureInfo]::GetCultureInfo('en-US')
[System.Threading.Thread]::CurrentThread.CurrentUICulture = [System.Globalization.CultureInfo]::GetCultureInfo('en-US')

$features = [ordered] @{
    'themes'           = 'Feature.Themes.ps1'
    'regions'          = 'Feature.Regions.ps1'
    'metrics'          = 'Feature.Metrics.ps1'
    'tables'           = 'Feature.Tables.ps1'
    'lists'            = 'Feature.Lists.ps1'
    'calendar-heatmap' = 'Feature.CalendarHeatmap.ps1'
    'timeline'         = 'Feature.Timeline.ps1'
    'treemap'          = 'Feature.Treemap.ps1'
    'mermaid'          = 'Feature.Mermaid.ps1'
    'qrcode'           = 'Feature.QRCode.ps1'
    'thresholds'       = 'Feature.Thresholds.ps1'
    'system-info'      = 'Feature.SystemInfo.ps1'
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$renders = [System.Collections.Generic.List[object]]::new()

function Add-Render([string] $Feature, [string] $Script, [hashtable] $Arguments) {
    $path = & (Join-Path $PSScriptRoot "Features\$Script") @Arguments -OutputDirectory $OutputDirectory | Select-Object -Last 1
    $renders.Add([ordered] @{
            feature = $Feature
            script  = "Examples/Features/$Script"
            theme   = $Arguments.Theme.ToLower()
            width   = $Arguments.Width
            height  = $Arguments.Height
            file    = Split-Path -Path $path -Leaf
        })
}

foreach ($feature in $features.GetEnumerator()) {
    foreach ($size in $Sizes) {
        foreach ($theme in $Themes) {
            Add-Render $feature.Key $feature.Value @{ Theme = $theme; Width = $size[0]; Height = $size[1] }
        }
    }
}

# Regions must hold on ultrawide and portrait screens too.
foreach ($size in @(@(3440, 1440), @(1080, 1920))) {
    foreach ($theme in $Themes) {
        Add-Render 'regions' 'Feature.Regions.ps1' @{ Theme = $theme; Width = $size[0]; Height = $size[1] }
    }
}

# Every theme preset in both modes at 1920x1080.
foreach ($preset in 'Graphite', 'Midnight', 'Ember', 'Orchid', 'Corporate') {
    foreach ($theme in $Themes) {
        Add-Render 'themes' 'Feature.Themes.ps1' @{ Preset = $preset; Theme = $theme; Width = 1920; Height = 1080 }
    }
}

$manifest = [ordered] @{ generator = 'PowerBGInfo'; sourceCommit = $SourceCommit; renders = @($renders) }
$manifest | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $OutputDirectory 'manifest.json')
Get-ChildItem -Path $OutputDirectory -Filter '*.png'
