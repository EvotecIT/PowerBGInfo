$discoveryConfiguration = if ([string]::IsNullOrWhiteSpace($Env:PowerBGInfoDevelopmentConfiguration)) { 'Debug' } else { $Env:PowerBGInfoDevelopmentConfiguration }
$discoveryFallbackConfiguration = if ($discoveryConfiguration -eq 'Debug') { 'Release' } else { 'Debug' }
$discoveryFramework = if ($PSVersionTable.PSEdition -eq 'Desktop') { 'net472' } else { 'net8.0-windows' }
$modulePath = $Env:POWERBGINFO_TEST_MODULE
if ([string]::IsNullOrWhiteSpace($modulePath)) {
    $modulePath = Join-Path -Path $PSScriptRoot -ChildPath "..\PowerBGInfo.PowerShell\bin\$discoveryConfiguration\$discoveryFramework\PowerBGInfo.PowerShell.dll"
    if (-not (Test-Path -LiteralPath $modulePath)) {
        $modulePath = Join-Path -Path $PSScriptRoot -ChildPath "..\PowerBGInfo.PowerShell\bin\$discoveryFallbackConfiguration\$discoveryFramework\PowerBGInfo.PowerShell.dll"
    }
}
Import-Module -Name $modulePath -Force

Describe 'Wallpaper widget data and theme contracts' {
    It 'skips malformed calendar values while counting only missing values' {
        $warnings = @()
        $calendar = @(
            @{ Date = '2026-10-12'; Value = 'N/A' }
            @{ Date = '2026-10-13' }
            @{ Date = '2026-10-14'; Value = 0 }
            @{ Date = '2026-10-15'; Value = $null }
        ) | New-BGInfoCalendarHeatmap -WarningVariable warnings -WarningAction SilentlyContinue
        $calendar.Items.Count | Should -Be 2
        $calendar.Items[0].Value | Should -Be 1
        $calendar.Items[1].Value | Should -Be 0
        $warnings.Count | Should -Be 2
        $data = New-BGInfoCalendarHeatmap -Data ([ordered] @{ '2026-10-12' = 'N/A'; '2026-10-13' = 0 }) -WarningAction SilentlyContinue
        $data.Items.Count | Should -Be 1
        $data.Items[0].Value | Should -Be 0
        $treemap = New-BGInfoTreemap -Data ([ordered] @{ Invalid = 'N/A'; Valid = 3 }) -WarningAction SilentlyContinue
        $treemap.Items.Count | Should -Be 1
        $treemap.Items[0].Value | Should -Be 3
    }

    It 'infers ordered dictionary columns and preserves their values' {
        $table = [ordered] @{ Service = 'Spooler'; State = 'OK' } | New-BGInfoTable
        ($table.Columns -join ',') | Should -Be 'Service,State'
        ($table.Rows[0] -join ',') | Should -Be 'Spooler,OK'
    }

    It 'preserves explicit timeline colours from dictionary and object inputs' {
        $timeline = @(
            @{ Lane = 'Ops'; Start = '2026-10-12'; Color = '#FF0000' }
            [pscustomobject] @{ Lane = 'Ops'; Start = '2026-10-13'; Color = '#00FF00' }
        ) | New-BGInfoTimeline
        $timeline.Items[0].Color.ToHex() | Should -Be '#FF0000'
        $timeline.Items[1].Color.ToHex() | Should -Be '#00FF00'
    }

    It 'retains blank lines supplied by the documented Mermaid pipeline' {
        $diagram = 'flowchart LR', '', '  A --> B' | New-BGInfoMermaid -ErrorAction Stop
        $diagram.Definition | Should -Match "flowchart LR`r?`n`r?`n  A --> B"
    }

    It 'uses Gregorian ISO dates under a non-Gregorian authoring culture' {
        $before = [Globalization.CultureInfo]::CurrentCulture
        try {
            [Globalization.CultureInfo]::CurrentCulture = [Globalization.CultureInfo]::GetCultureInfo('th-TH')
            $calendar = @{ Date = '2026-10-12'; Value = 3 } | New-BGInfoCalendarHeatmap
            $timeline = @{ Lane = 'Ops'; Start = '2026-10-12T02:00:00' } | New-BGInfoTimeline
            $calendar.Items[0].Date.Year | Should -Be 2026
            $timeline.Items[0].Start.Year | Should -Be 2026
        } finally { [Globalization.CultureInfo]::CurrentCulture = $before }
    }

    It 'preserves supported diagram overrides and inherits canvas typography through JSON' {
        $theme = New-BGInfoTheme Midnight
        $theme.FontFamilyName = 'Courier New'
        $config = New-BGInfo -Theme $theme -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoTopology -Theme Light -TitleColor '#FF0000' -TopologyDefinition { New-BGInfoTopologyNode -Id A -Label A }
            New-BGInfoVisualCanvas -Tile (New-BGInfoVisualCanvasTile -Label 'Status' -Value 'OK')
        }
        $config.Topologies[0].TitleColor.ToHex() | Should -Be '#FF0000'
        $config.Topologies[0].Theme | Should -Be 'Light'
        $config.VisualCanvases[0].FontFamilyName | Should -Be 'Courier New'
        $path = Join-Path $TestDrive 'typography.json'
        Export-BGInfoConfiguration -InputObject $config -Path $path -Force
        $json = Get-Content $path -Raw | ConvertFrom-Json
        $json.VisualCanvases[0].FontFamilyName | Should -Be 'Courier New'
    }

    It 'warns about visible hero overlap but not themed or region tile lanes' {
        $warnings = @()
        $tile = New-BGInfoVisualCanvasTile -Side Center -Label 'State' -Value 'OK'
        New-BGInfoVisualCanvas -Tile $tile -WarningVariable warnings -WarningAction SilentlyContinue | Out-Null
        $warnings.Count | Should -Be 1
        $regionWarnings = @()
        New-BGInfoVisualCanvas -Region Right -Tile $tile -WarningVariable regionWarnings -WarningAction SilentlyContinue | Out-Null
        $regionWarnings.Count | Should -Be 0
        $themeWarnings = @()
        New-BGInfo -Theme Midnight -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoVisualCanvas -Tile $tile -WarningVariable themeWarnings -WarningAction SilentlyContinue
        } | Out-Null
        $themeWarnings.Count | Should -Be 0
    }
}
