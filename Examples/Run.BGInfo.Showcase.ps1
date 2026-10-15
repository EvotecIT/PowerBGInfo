<#
    Renders every showcase scenario in light and dark to image files and
    writes manifest.json describing each render. Nothing is applied to the
    desktop: every scenario uses -Target File.
#>
param(
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output\Showcase'),
    [int] $Width = 1920,
    [int] $Height = 1080
)

$scenarios = @(
    [ordered]@{
        id       = 'server-status'
        title    = 'Server status'
        purpose  = 'Uptime, disks, services and network of a production server.'
        script   = 'Examples/Run.BGInfo.ServerStatus.ps1'
        features = @(
            'Generated gradient background with glows and a grid'
            'Visual canvas info tiles with mini charts'
            'Progress bar, area and pictorial charts on shared panels'
        )
    }
    [ordered]@{
        id       = 'helpdesk'
        title    = 'Help desk'
        purpose  = 'How to get help, plus the details support asks for.'
        script   = 'Examples/Run.BGInfo.HelpDesk.ps1'
        features = @(
            'Generated gradient background with glows and dots'
            'Visual canvas info tiles with built-in icons'
            'Shared text styles for headings, values and status'
        )
    }
    [ordered]@{
        id       = 'classroom'
        title    = 'Lab or classroom'
        purpose  = 'Seat name, reset schedule and lab progress for students.'
        script   = 'Examples/Run.BGInfo.Classroom.ps1'
        features = @(
            'Generated gradient background with three glows'
            'Gauge, pictorial and bar charts on shared panels'
            'Large display typography'
        )
    }
    [ordered]@{
        id       = 'admin-workstation'
        title    = 'Admin workstation'
        purpose  = 'Dense privileged-access HUD with the allowed admin path.'
        script   = 'Examples/Run.BGInfo.AdminWorkstation.ps1'
        features = @(
            'Topology diagram with groups, status and replication'
            'Visual canvas info tiles with mini charts and progress'
            'Bar, area and progress bar charts on shared panels'
        )
    }
)

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent

$renders = foreach ($scenario in $scenarios) {
    foreach ($theme in 'Light', 'Dark') {
        $render = @{
            Theme           = $theme
            Width           = $Width
            Height          = $Height
            OutputDirectory = $OutputDirectory
        }
        $script = Join-Path $repositoryRoot $scenario.script
        $path = & $script @render | Select-Object -Last 1
        Write-Verbose "Rendered $path"

        [ordered]@{
            id                  = "$($scenario.id)-$($theme.ToLower())"
            scenario            = $scenario.id
            title               = $scenario.title
            purpose             = $scenario.purpose
            script              = $scenario.script
            theme               = $theme.ToLower()
            width               = $Width
            height              = $Height
            file                = Split-Path -Path $path -Leaf
            chartForgeXFeatures = $scenario.features
        }
    }
}

$manifest = [ordered]@{
    generator = 'PowerBGInfo'
    renders   = @($renders)
}
$manifestPath = Join-Path $OutputDirectory 'manifest.json'
$manifest | ConvertTo-Json -Depth 4 | Set-Content -Path $manifestPath
Get-ChildItem -Path $OutputDirectory -Filter '*.png'
