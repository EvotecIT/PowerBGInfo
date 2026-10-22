<#
    Renders every showcase scenario in light and dark to image files and
    writes manifest.json describing each render. Nothing is applied to the
    desktop: every scenario uses -Target File.

    Each scenario renders at every -Size, plus the extra sizes it is built
    for (an ultrawide wall display, a portrait sign). Files at 1920x1080 are
    named <scenario>-<theme>.png; other sizes add -<width>x<height>.
#>
param(
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output\Showcase'),
    [string[]] $Size = @('1920x1080', '2560x1440'),
    # Scenario ids to render, for example 'on-call'. Empty renders all.
    [string[]] $Scenario = @(),
    [switch] $NoExtraSizes,
    # Renders use one culture so day names and numbers match the website.
    [string] $Culture = 'en-US',
    [string] $SourceCommit = ''
)

if (-not $SourceCommit) {
    $SourceCommit = (git -C $PSScriptRoot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Cannot read the showcase source commit.' }
}
if ($SourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'SourceCommit must be the full Git commit SHA used to render the showcase.' }

$scenarios = @(
    [ordered]@{
        id         = 'server-status'
        title      = 'Server status'
        purpose    = 'Uptime, disks, services and network of a ' +
            'production server.'
        script     = 'Examples/Run.BGInfo.ServerStatus.ps1'
        preset     = 'Aurora'
        features   = @('-Theme', '-Region', 'thresholds', 'visual canvas tiles')
        chartForgeX = @(
            'Generated gradient background with glows and a grid'
            'Visual canvas info tiles with mini charts'
            'Progress bar, area and pictorial charts on shared panels'
        )
    }
    [ordered]@{
        id         = 'helpdesk'
        title      = 'Help desk'
        purpose    = 'How to get help, plus the details support asks for.'
        script     = 'Examples/Run.BGInfo.HelpDesk.ps1'
        preset     = 'Orchid'
        features   = @('-Theme', '-Region', 'New-BGInfoLayout', 'status labels')
        chartForgeX = @(
            'Generated gradient background with glows and dots'
            'Visual canvas info tiles with built-in icons'
            'Shared text styles for headings, values and status'
        )
    }
    [ordered]@{
        id         = 'classroom'
        title      = 'Lab or classroom'
        purpose    = 'Seat name, reset schedule and lab progress for students.'
        script     = 'Examples/Run.BGInfo.Classroom.ps1'
        preset     = 'Ember'
        features   = @('-Theme', '-Region', 'thresholds (below)')
        chartForgeX = @(
            'Generated gradient background with three glows'
            'Gauge, pictorial and bar charts on shared panels'
            'Large display typography'
        )
    }
    [ordered]@{
        id         = 'admin-workstation'
        title      = 'Admin workstation'
        purpose    = 'Dense privileged-access HUD with the allowed admin path.'
        script     = 'Examples/Run.BGInfo.AdminWorkstation.ps1'
        preset     = 'Midnight'
        features   = @('-Theme', 'anchors', 'thresholds (below)', 'topology')
        chartForgeX = @(
            'Topology diagram with groups, status and replication'
            'Visual canvas info tiles with mini charts and progress'
            'Bar, area and progress bar charts on shared panels'
        )
    }
    [ordered]@{
        id         = 'domain-controller'
        title      = 'Domain controller'
        purpose    = 'Replication, FSMO roles, time sync and sign-in failures.'
        script     = 'Examples/Run.BGInfo.DomainController.ps1'
        preset     = 'Graphite'
        features   = @(
            'New-BGInfoTable', 'New-BGInfoList', 'New-BGInfoMetric'
            'New-BGInfoCalendarHeatmap', 'New-BGInfoTopology', 'Center region'
        )
        chartForgeX = @(
            'Calendar heatmap of failed sign-ins'
            'Hub-and-spoke topology of AD sites'
            'Status table, status list and a metric card with a sparkline'
        )
    }
    [ordered]@{
        id         = 'hyperv-host'
        title      = 'Hyper-V host'
        purpose    = 'Room for another VM, and which VMs use the host.'
        script     = 'Examples/Run.BGInfo.HyperVHost.ps1'
        preset     = 'Corporate'
        features   = @(
            'New-BGInfoTreemap -ColorProperty', 'New-BGInfoMetric'
            'Top region', 'thresholds', 'New-BGInfoLayout'
        )
        chartForgeX = @(
            'Treemap sized by memory and colored by CPU status bands'
            'Metric cards with status bars and a sparkline'
            'Horizontal cluster memory bars with node labels and a palette'
            'A table of live migrations and a list of old checkpoints'
        )
    }
    [ordered]@{
        id         = 'on-call'
        title      = 'NOC and on call'
        purpose    = 'Who is on call now, and how an incident escalates.'
        script     = 'Examples/Run.BGInfo.OnCall.ps1'
        preset     = 'Midnight'
        extraSizes = @('3440x1440')
        features   = @(
            'New-BGInfoTimeline', 'New-BGInfoMermaid', 'New-BGInfoQRCode'
            'New-BGInfoList', 'custom accent and glows'
        )
        chartForgeX = @(
            'Gantt lanes with a Now marker for the on-call rota'
            'Mermaid flowchart of the escalation path'
            'QR code for the runbook'
        )
    }
    [ordered]@{
        id         = 'branch-office'
        title      = 'Branch office PC'
        purpose    = 'Which site, is the link up, and how to get help.'
        script     = 'Examples/Run.BGInfo.BranchOffice.ps1'
        preset     = 'Aurora'
        features   = @(
            'New-BGInfoTopology -Scale', 'New-BGInfoQRCode', 'New-BGInfoTable'
            'Center region', 'dot pattern'
        )
        chartForgeX = @(
            'Hub-and-spoke topology of the local network'
            'QR code that opens a ticket'
            'Opening hours table'
        )
    }
    [ordered]@{
        id         = 'meeting-room'
        title      = 'Meeting room'
        purpose    = 'Which room, is it free, and how to share a screen.'
        script     = 'Examples/Run.BGInfo.MeetingRoom.ps1'
        preset     = 'Orchid'
        features   = @(
            'New-BGInfoTimeline', 'New-BGInfoQRCode (Wi-Fi)'
            'New-BGInfoList -Marker Number', 'status label'
        )
        chartForgeX = @(
            'Single-day Gantt lanes for this room and the rooms next door'
            'Wi-Fi QR code'
            'Numbered list'
        )
    }
    [ordered]@{
        id         = 'build-agent'
        title      = 'Build agent'
        purpose    = 'Busy or broken, what ran last, and how much disk is left.'
        script     = 'Examples/Run.BGInfo.BuildAgent.ps1'
        preset     = 'Graphite'
        features   = @(
            'New-BGInfoMermaid (generated)', 'New-BGInfoTable -StatusColumn'
            'New-BGInfoMetric -ThresholdDirection Below', 'dot pattern'
        )
        chartForgeX = @(
            'Mermaid pipeline with classDef styling for the current stage'
            'Status table of recent builds'
            'Metric cards for free disk space'
        )
    }
    [ordered]@{
        id         = 'patch-day'
        title      = 'Patch day'
        purpose    = 'When this PC restarts, and how the rollout is going.'
        script     = 'Examples/Run.BGInfo.PatchDay.ps1'
        preset     = 'Ember'
        features   = @(
            'New-BGInfoTimeline (item colors)', 'New-BGInfoChart -Kind Gauge'
            'New-BGInfoList (checklist)', 'thresholds (below)'
        )
        chartForgeX = @(
            'Gantt lanes for update rings with a Now marker'
            'Gauge countdown and progress bars per ring'
            'Checklist'
        )
    }
    [ordered]@{
        id         = 'signage'
        title      = 'Kiosk or signage'
        purpose    = 'One bold message and a code to scan for the details.'
        script     = 'Examples/Run.BGInfo.Signage.ps1'
        preset     = 'Corporate'
        extraSizes = @('1080x1920')
        features   = @(
            'New-BGInfoQRCode', 'portrait-aware regions', 'widget -Scale'
            'New-BGInfoLayout'
        )
        chartForgeX = @(
            'Display typography on a generated gradient'
            'QR code'
            'List rendered at a larger density'
        )
    }
    [ordered]@{
        id         = 'environment-marker'
        title      = 'Production or test'
        purpose    = 'Which environment this is, and what you may change on it.'
        script     = 'Examples/Run.BGInfo.EnvironmentMarker.ps1'
        preset     = 'Midnight'
        features   = @(
            'New-BGInfoTheme -Accent', 'New-BGInfoQRCode', 'New-BGInfoList'
            'New-BGInfoTimeline', 'New-BGInfoLayout'
        )
        chartForgeX = @(
            'The environment colour drives the accent, glows and headline'
            'Colour-coded list of the same service in every environment'
            'Change-window timeline with approval states'
            'QR code to the change policy'
        )
    }
    [ordered]@{
        id         = 'rds-session-host'
        title      = 'RDS session host'
        purpose    = 'Whether the broker still sends users here, who is on ' +
            'the host and how long logons take.'
        script     = 'Examples/Run.BGInfo.RdsSessionHost.ps1'
        preset     = 'Graphite'
        features   = @(
            'New-BGInfoLayout -ColumnWidth', 'New-BGInfoTable'
            'New-BGInfoChart -Kind Bar, ProgressBar', 'New-BGInfoMetric'
            'thresholds'
        )
        chartForgeX = @(
            'Session table in a wide right column'
            'Logon-time bars with the spike in amber'
            'FSLogix container bars against the cap'
            'Four metric cards with sparklines'
        )
    }
    [ordered]@{
        id         = 'sql-server'
        title      = 'SQL Server'
        purpose    = 'Whether every database has recent backups and a ' +
            'current replica.'
        script     = 'Examples/Run.BGInfo.SqlServer.ps1'
        preset     = 'Corporate'
        features   = @(
            'New-BGInfoTreemap -ColorProperty', 'New-BGInfoTable -StatusColumn'
            'New-BGInfoList', 'New-BGInfoChart -Kind Area, ProgressBar'
        )
        chartForgeX = @(
            'Treemap of database size coloured by backup age'
            'Status table of databases, problems first'
            'Workload area chart'
            'Space bars for data, logs and backups'
        )
    }
    [ordered]@{
        id         = 'file-server'
        title      = 'File server'
        purpose    = 'Which shares are close to their quota and when the ' +
            'fullest one runs out.'
        script     = 'Examples/Run.BGInfo.FileServer.ps1'
        preset     = 'Aurora'
        features   = @(
            'New-BGInfoTreemap -GroupProperty -ColorProperty'
            'New-BGInfoMetric -HistoryKind Bars', 'New-BGInfoList'
            'New-BGInfoTable -StatusColumn', 'New-BGInfoChart -Kind ProgressBar'
        )
        chartForgeX = @(
            'Grouped treemap of shares coloured by quota use'
            'Volume progress bars'
            'Weekly growth bars with a forecast'
            'DFS-R backlog status table'
        )
    }
    [ordered]@{
        id         = 'decommission-notice'
        title      = 'Server being retired'
        purpose    = 'How long this server has left, and where to work instead.'
        script     = 'Examples/Run.BGInfo.DecommissionNotice.ps1'
        preset     = 'Ember'
        features   = @(
            'New-BGInfoTable -StatusColumn', 'New-BGInfoQRCode'
            'New-BGInfoList', 'New-BGInfoChart -Kind Bar, Area', 'Center region'
        )
        chartForgeX = @(
            'Display-size countdown headline'
            'Bars of data left to move, night by night'
            'Area chart of SMB sessions still connected'
            'Table showing where each share moved'
        )
    }
    [ordered]@{
        id         = 'change-freeze'
        title      = 'Change freeze'
        purpose    = 'Whether changes are allowed now, until when, and who ' +
            'approves an exception.'
        script     = 'Examples/Run.BGInfo.ChangeFreeze.ps1'
        preset     = 'Orchid'
        features   = @(
            'New-BGInfoTimeline', 'New-BGInfoList -Marker Check'
            'New-BGInfoQRCode', 'New-BGInfoTable', 'dot pattern and glows'
        )
        chartForgeX = @(
            'Change-calendar timeline lanes with a now marker'
            'Allowed and not allowed checklist'
            'QR code for the exception process'
        )
    }
    [ordered]@{
        id         = 'certificate-authority'
        title      = 'Certificate authority'
        purpose    = 'What expires soon on this CA, and whether CRLs and ' +
            'AIA/CDP paths are healthy.'
        script     = 'Examples/Run.BGInfo.CertificateAuthority.ps1'
        preset     = 'Midnight'
        features   = @(
            'New-BGInfoCalendarHeatmap'
            'New-BGInfoChart -Kind HorizontalBar, Bar'
            'New-BGInfoMetric -ThresholdDirection Below'
            'New-BGInfoTable -StatusColumn'
        )
        chartForgeX = @(
            'Calendar of expiry dates over the next 90 days'
            '30, 60 and 90-day expiry bars'
            'Failed requests per day, coloured by threshold'
            'Template table with renewal state'
        )
    }
    [ordered]@{
        id         = 'backup-server'
        title      = 'Backup server'
        purpose    = 'Whether last night''s backups worked, and which jobs ' +
            'need a retry before tonight.'
        script     = 'Examples/Run.BGInfo.BackupServer.ps1'
        preset     = 'Graphite'
        features   = @(
            'New-BGInfoTable -StatusColumn', 'New-BGInfoCalendarHeatmap'
            'New-BGInfoMetric', 'thresholds', 'background glow'
        )
        chartForgeX = @(
            'A year of nights as a calendar heatmap'
            'Success-rate bars coloured by threshold'
            'Repository progress bars at 80 and 90 %'
            'Status list for the copies'
        )
    }
    [ordered]@{
        id         = 'jump-host'
        title      = 'Jump host'
        purpose    = 'The rules of a privileged access host: who is ' +
            'elevated, what is recorded and what is never allowed.'
        script     = 'Examples/Run.BGInfo.JumpHost.ps1'
        preset     = 'Midnight'
        features   = @(
            'status labels', 'New-BGInfoQRCode', 'New-BGInfoList'
            'New-BGInfoTable -StatusColumn'
        )
        chartForgeX = @(
            'Large rule labels as a poster'
            'QR code to the access policy'
            'Allowed tools and baseline status lists'
            'Admin sessions table'
        )
    }
    [ordered]@{
        id         = 'linux-server'
        title      = 'Linux server'
        purpose    = 'What this Linux box is, rendered headless under ' +
            'PowerShell 7.'
        script     = 'Examples/Run.BGInfo.LinuxServer.ps1'
        preset     = 'Aurora'
        features   = @(
            '-FontFamilyName (monospace)', 'New-BGInfoTable', 'New-BGInfoList'
            'New-BGInfoChart -Kind Area, ProgressBar', 'dots pattern'
        )
        chartForgeX = @(
            'Area chart of load over 6 hours'
            'Mount progress bars at 80 and 90 %'
            'systemd unit and package status lists'
            'Listening ports table'
        )
    }
)

if ($Scenario.Count -gt 0) {
    $unknown = @($Scenario | Where-Object { $_ -notin $scenarios.id })
    if ($unknown.Count) { throw "Unknown scenario: $($unknown -join ', ')." }
    $scenarios = @($scenarios | Where-Object { $_.id -in $Scenario })
}
foreach ($dimensions in $Size) {
    if ($dimensions -notmatch '^([1-9][0-9]*)x([1-9][0-9]*)$' -or
        [double] $Matches[1] -gt 16384 -or [double] $Matches[2] -gt 16384) {
        throw "Invalid size '$dimensions'. Use WIDTHxHEIGHT, with both dimensions from 1 to 16384."
    }
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$staging = Join-Path $OutputDirectory ('.staging-' + [guid]::NewGuid().ToString('N'))
$previousCulture = [System.Threading.Thread]::CurrentThread.CurrentCulture

try {
    $culture = [System.Globalization.CultureInfo]::GetCultureInfo($Culture)
    [System.Threading.Thread]::CurrentThread.CurrentCulture = $culture
    $renders = foreach ($scene in $scenarios) {
        $sizes = @($Size)
        if (-not $NoExtraSizes -and $scene.extraSizes) {
            $sizes += $scene.extraSizes
        }
        foreach ($dimensions in $sizes) {
            $width, $height = [int[]] ($dimensions -split 'x')
            foreach ($theme in 'Light', 'Dark') {
                # Scripts name their file by theme; stage it, then name it.
                New-Item -ItemType Directory -Path $staging -Force | Out-Null
                $render = @{
                    Theme           = $theme
                    Width           = $width
                    Height          = $height
                    OutputDirectory = $staging
                }
                $script = Join-Path $repositoryRoot $scene.script
                $path = & $script @render | Select-Object -Last 1
                $suffix = if ($dimensions -eq '1920x1080') { '' } else {
                    "-$dimensions"
                }
                $id = "$($scene.id)-$($theme.ToLower())$suffix"
                $file = "$id.png"
                Move-Item -Path $path -Destination (
                    Join-Path $OutputDirectory $file) -Force
                Write-Verbose "Rendered $file"

                [ordered]@{
                    id                  = $id
                    scenario            = $scene.id
                    title               = $scene.title
                    purpose             = $scene.purpose
                    script              = $scene.script
                    theme               = $theme.ToLower()
                    preset              = $scene.preset
                    size                = $dimensions
                    width               = $width
                    height              = $height
                    file                = $file
                    features            = $scene.features
                    chartForgeXFeatures = $scene.chartForgeX
                }
            }
        }
    }
} finally {
    [System.Threading.Thread]::CurrentThread.CurrentCulture = $previousCulture
    if (Test-Path -LiteralPath $staging) {
        Remove-Item -LiteralPath $staging -Recurse
    }
}

$manifest = [ordered]@{
    generator    = 'PowerBGInfo'
    sourceCommit = $SourceCommit
    renders      = @($renders)
}
$manifestName = if ($Scenario.Count -gt 0) { 'manifest.filtered.json' } else { 'manifest.json' }
$manifestPath = Join-Path $OutputDirectory $manifestName
$manifest | ConvertTo-Json -Depth 4 | Set-Content -Path $manifestPath
Get-ChildItem -Path $OutputDirectory -Filter '*.png'
