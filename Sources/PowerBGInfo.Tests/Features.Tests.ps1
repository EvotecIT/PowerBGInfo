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

Describe 'New-BGInfoTheme and New-BGInfo -Theme' {
    It 'creates a theme from a preset, mode, accent and scale' {
        $theme = New-BGInfoTheme -Preset Ember -Mode Light -Accent '#FF0000' -Scale 2
        $theme.Preset | Should -Be 'Ember'
        $theme.Mode | Should -Be 'Light'
        $theme.Accent.ToHex() | Should -Be '#FF0000'
        $theme.FontSize | Should -Be 34
    }

    It 'fills unbound chart, topology, canvas and widget settings from the theme' {
        $theme = New-BGInfoTheme -Preset Midnight -Mode Light
        $config = New-BGInfo -Theme $theme -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoChart -Title 'CPU' -Values 1, 2 -NoHistory
            New-BGInfoChart -Title 'Explicit' -Values 1 -CornerRadius 3 -BackgroundColor '#000000' -NoHistory
            New-BGInfoTopology -Title 'Path' -TopologyDefinition { New-BGInfoTopologyNode -Id 'a' -Label 'A' }
            New-BGInfoVisualCanvas -Tile (New-BGInfoVisualCanvasTile -Label 'A' -Value '1')
            New-BGInfoMetric -Label 'CPU' -Value 10
        }

        $config.Color | Should -Be $theme.Muted
        $config.ValueColor | Should -Be $theme.Ink
        $config.SpaceX | Should -Be $theme.MarginX
        $config.Layout.MarginX | Should -Be $theme.MarginX
        $config.Charts[0].BackgroundColor | Should -Be $theme.Surface
        $config.Charts[0].ThemeMode | Should -Be 'Light'
        $config.Charts[0].LineColor | Should -Be $theme.Accent
        $config.Charts[1].CornerRadius | Should -Be 3
        $config.Charts[1].BackgroundColor.ToHex() | Should -Be '#000000'
        $config.Topologies[0].Theme | Should -Be 'Light'
        $config.Topologies[0].BackgroundColor | Should -Be $theme.Surface
        $config.VisualCanvases[0].Accent | Should -Be $theme.Accent
        $config.VisualCanvases[0].HeroContentVisible | Should -BeFalse
        $config.Widgets[0].BackgroundColor | Should -Be $theme.Surface
        $config.Widgets[0].StatusColors.Critical | Should -Be $theme.Critical
    }

    It 'preserves explicitly requested hero content inside a themed visual canvas' {
        $theme = New-BGInfoTheme -Preset Midnight -Mode Light
        $config = New-BGInfo -Theme $theme -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoVisualCanvas -Title 'Operations' -Subtitle 'Current environment' -HeroBadgeText 'OPS'
        }

        $visual = $config.VisualCanvases[0]
        $visual.HeroContentVisible | Should -BeTrue
        $visual.Title | Should -Be 'Operations'
        $visual.Subtitle | Should -Be 'Current environment'
        $visual.HeroBadgeText | Should -Be 'OPS'
    }

    It 'accepts a preset name and switches its mode with -ThemeMode' {
        $config = New-BGInfo -Theme Aurora -ThemeMode Light -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoChart -Values 1 -NoHistory
        }
        $config.Charts[0].ThemeMode | Should -Be 'Light'
        $config.Charts[0].BackgroundColor | Should -Be (New-BGInfoTheme Aurora -Mode Light).Surface
    }

    It 'rejects -ThemeMode for a customized theme object' {
        $theme = New-BGInfoTheme -Preset Aurora -Mode Dark -Accent '#FF0000'
        { New-BGInfo -Theme $theme -ThemeMode Light -PassThru -ConfigurationDirectory $TestDrive { } } | Should -Throw '*ThemeMode*'
    }

    It 'rejects unknown preset names' {
        { New-BGInfo -Theme 'NoSuchTheme' -PassThru -ConfigurationDirectory $TestDrive { } } | Should -Throw
    }

    It 'leaves cmdlets outside a themed block untouched' {
        New-BGInfo -Theme Graphite -PassThru -ConfigurationDirectory $TestDrive { } | Out-Null
        $chart = New-BGInfoChart -Values 1 -NoHistory
        $chart.BackgroundColor | Should -BeNullOrEmpty
        $chart.ThemeMode | Should -Be 'Light'
    }

    It 'paints the theme backdrop and colors labels by role' {
        $theme = New-BGInfoTheme -Preset Orchid -Mode Dark
        $config = New-BGInfo -Theme $theme -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoBackground -Width 640 -Height 360
            New-BGInfoLabel -Name 'Heading'
            New-BGInfoLabel -Name 'Accent' -Accent
            New-BGInfoLabel -Name 'Note' -Muted
            New-BGInfoLabel -Name 'Warning' -Status Warning
            New-BGInfoLabel -Name 'Explicit' -Color '#123456'
        }

        $config.Background.GradientStart | Should -Be $theme.BackdropGradientStart
        $config.Background.Pattern | Should -Be 'Dots'
        $config.Entries[0].Color | Should -Be $theme.Ink
        $config.Entries[1].Color | Should -Be $theme.Accent
        $config.Entries[2].Color | Should -Be $theme.Muted
        $config.Entries[3].Color | Should -Be $theme.Warning
        $config.Entries[4].Color.ToHex() | Should -Be '#123456'
    }
}

Describe 'New-BGInfoLayout and -Region' {
    It 'emits a layout that New-BGInfo keeps' {
        $config = New-BGInfo -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoLayout -Margin 40 -Gap 12 -ColumnWidth 400 -TaskbarReserve 0 -IconAreaWidth 90 -ColumnAlignment Center
        }
        $config.Layout.MarginX | Should -Be 40
        $config.Layout.MarginY | Should -Be 40
        $config.Layout.ColumnWidth | Should -Be 400
        $config.Layout.TaskbarReserve | Should -Be 0
        $config.Layout.IconAreaWidth | Should -Be 90
        $config.Layout.ColumnAlignment | Should -Be 'Center'
    }

    It 'makes unsized region items share the region and numbers them in emission order' {
        $config = New-BGInfo -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoChart -Region Right -Values 1 -NoHistory
            New-BGInfoMetric -Region Right -Label 'A' -Value 1
            New-BGInfoChart -Region Bottom -Values 1 -Width 300 -NoHistory
            New-BGInfoChart -Values 1 -NoHistory
        }
        $config.Charts[0].Width | Should -Be 0
        $config.Charts[0].Height | Should -Be 0
        $config.Charts[0].RegionOrder | Should -Be 0
        $config.Widgets[0].RegionOrder | Should -Be 1
        $config.Charts[1].Width | Should -Be 300
        $config.Charts[2].Region | Should -Be 'None'
        $config.Charts[2].Width | Should -Be 240
    }

    It 'renders region panels at 1920x1080, 2560x1440, ultrawide and portrait' -ForEach @(
        @{ Width = 1920; Height = 1080 }, @{ Width = 2560; Height = 1440 }, @{ Width = 3440; Height = 1440 }, @{ Width = 1080; Height = 1920 }
    ) {
        $path = New-BGInfo -Theme Graphite -Target File -ConfigurationDirectory $TestDrive -OutputFileName "regions-$Width.png" {
            New-BGInfoBackground -Width $Width -Height $Height
            New-BGInfoValue -Name 'Host' -Value 'TEST'
            New-BGInfoChart -Region Left -Values 1, 3, 2 -Kind Area -NoHistory
            New-BGInfoMetric -Region Right -Label 'CPU' -Value 40
            New-BGInfoList -Region Center -Item 'one', 'two'
            New-BGInfoChart -Region Bottom -Values 4, 5 -Kind Bar -NoHistory
            New-BGInfoChart -Region Top -Values 4 -Kind Gauge -NoHistory
        }
        Test-Path -LiteralPath $path | Should -BeTrue
    }

    It 'stacks visual canvas tiles in a region' {
        $tiles = 1..3 | ForEach-Object { New-BGInfoVisualCanvasTile -Label "T$_" -Value $_ -Side Left }
        $config = New-BGInfo -Theme Aurora -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoVisualCanvas -Region Right -Tile $tiles
        }
        $config.VisualCanvases[0].Region | Should -Be 'Right'
        $config.VisualCanvases[0].RailMarginX | Should -Be 64
    }
}

Describe 'Thresholds' {
    It 'stores chart thresholds with theme status colors' {
        $theme = New-BGInfoTheme Aurora -Mode Light
        $config = New-BGInfo -Theme $theme -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoChart -Kind ProgressBar -Values 50, 85, 95 -WarningThreshold 80 -CriticalThreshold 90 -NoHistory
        }
        $chart = $config.Charts[0]
        $chart.Threshold.Warning | Should -Be 80
        $chart.Threshold.Critical | Should -Be 90
        $chart.Threshold.Colors.Critical | Should -Be $theme.Critical
        # Thresholds pick the series color, so the theme accent is not forced on the line.
        $chart.LineColor | Should -BeNullOrEmpty
    }

    It 'rejects thresholds in the wrong order' {
        { New-BGInfoChart -Values 1 -WarningThreshold 90 -CriticalThreshold 80 } | Should -Throw
        { New-BGInfoChart -Values 1 -WarningThreshold 20 -CriticalThreshold 10 -ThresholdDirection Below } | Should -Not -Throw
    }

    It 'colors tiles from a status value' {
        $tile = New-BGInfoVisualCanvasTile -Label 'Disk' -Value '91 %' -StatusValue 91 -WarningThreshold 80 -CriticalThreshold 90
        $tile.Accent | Should -Be ([PowerBGInfo.BgInfoStatusColors]::Default('Dark').Critical)
        { New-BGInfoVisualCanvasTile -Label 'Disk' -Value 'x' -WarningThreshold 80 } | Should -Throw '*StatusValue*'
    }

    It 'evaluates metric status from a numeric value' {
        $metric = New-BGInfoMetric -Label 'Free' -Value 4 -WarningThreshold 15 -CriticalThreshold 5 -ThresholdDirection Below
        $metric.ResolveStatus() | Should -Be 'Critical'
        $metric.NumericValue | Should -Be 4

        $csvMetric = New-BGInfoMetric -Label 'CPU' -Value '91' -WarningThreshold 80 -CriticalThreshold 90
        $csvMetric.Value | Should -Be '91'
        $csvMetric.NumericValue | Should -Be 91
        $csvMetric.ResolveStatus() | Should -Be 'Critical'
    }
}

Describe 'Widget cmdlets' {
    It 'builds a table from piped objects and calculated columns' {
        $rows = @(
            [pscustomobject] @{ Name = 'a'; Size = 1GB; State = 'OK' }
            [pscustomobject] @{ Name = 'b'; Size = 2GB; State = 'Failed' }
        )
        $table = $rows | New-BGInfoTable -Title 'T' -StatusColumn State -Property Name, @{ Name = 'GB'; Expression = { $_.Size / 1GB } }, State
        $table.Columns | Should -Be @('Name', 'GB', 'State')
        $table.Rows.Count | Should -Be 2
        $table.Rows[1][1] | Should -Be '2'
        $table.StatusColumn | Should -Be 'State'
    }

    It 'builds a table from explicit rows and hashtables' {
        $table = New-BGInfoTable -Column 'Name', 'Value' -Row @(@('a', 1), @{ Name = 'b'; Value = 2 })
        $table.Rows[0] | Should -Be @('a', '1')
        $table.Rows[1] | Should -Be @('b', '2')
    }

    It 'rejects a status column that is not a column' {
        { New-BGInfoTable -Column 'Name' -Row @(, @('a')) -StatusColumn 'State' } | Should -Throw '*State*'
    }

    It 'reads list items from strings, hashtables and objects' {
        $list = New-BGInfoList -Item 'plain', @{ Text = 'done'; Checked = $true }, ([pscustomobject] @{ Name = 'vpn'; Status = 'degraded'; Value = 'slow' })
        $list.Items.Count | Should -Be 3
        $list.Items[1].Checked | Should -BeTrue
        $list.Items[2].Text | Should -Be 'vpn'
        $list.Items[2].Status | Should -Be 'Warning'
        $list.Marker | Should -Be 'Auto'
    }

    It 'collects calendar values from objects, dates and hashtables' {
        $fromObjects = @([pscustomobject] @{ Date = [datetime] '2026-10-01'; Value = 3 }) | New-BGInfoCalendarHeatmap
        $fromDates = [datetime] '2026-10-01', [datetime] '2026-10-02' | New-BGInfoCalendarHeatmap
        $fromData = New-BGInfoCalendarHeatmap -Data @{ '2026-10-01' = 5 }
        $fromObjects.Items[0].Value | Should -Be 3
        $fromDates.Items.Count | Should -Be 2
        $fromData.Items[0].Date | Should -Be ([datetime] '2026-10-01')
    }

    It 'builds timeline items with -Duration and from hashtables' {
        $item = New-BGInfoTimelineItem 'Server' 'Patch' -Start '2026-10-18 02:00' -Duration '02:00' -Category Maintenance -Status Warning
        $item.End | Should -Be ([datetime] '2026-10-18 04:00')
        { New-BGInfoTimelineItem 'S' -Start '2026-10-18' -End '2026-10-17' } | Should -Throw
        $timeline = New-BGInfoTimeline -Item $item, @{ Lane = 'On call'; Label = 'A'; Start = '2026-10-16'; End = '2026-10-17' } -Now '2026-10-16 12:00'
        $timeline.Items.Count | Should -Be 2
        $timeline.Now | Should -Be ([datetime] '2026-10-16 12:00')
    }

    It 'collects treemap items from objects and hashtables' {
        $fromData = New-BGInfoTreemap -Data ([ordered] @{ A = 3; B = 1 }) -ValueSuffix ' GB'
        $fromObjects = [pscustomobject] @{ Name = 'x'; Value = 4; Group = 'g' } | New-BGInfoTreemap
        $fromData.Items.Count | Should -Be 2
        $fromObjects.Items[0].Group | Should -Be 'g'
    }

    It 'validates Mermaid source when the widget is created' {
        (New-BGInfoMermaid -Definition "flowchart LR`n  A --> B").Definition | Should -Match 'A --> B'
        $piped = 'flowchart LR', '  A --> B' | New-BGInfoMermaid
        $piped.Definition | Should -Match "flowchart LR`r?`n\s+A --> B"
        { New-BGInfoMermaid -Definition 'not a diagram' } | Should -Throw
    }

    It 'creates a QR code from a URL' {
        $qr = New-BGInfoQRCode -Url 'https://help.contoso.com' -Caption 'help'
        $qr.Text | Should -Be 'https://help.contoso.com'
    }

    It 'renders every widget kind to a file' {
        $path = New-BGInfo -Theme Corporate -Target File -ConfigurationDirectory $TestDrive -OutputFileName 'widgets.png' {
            New-BGInfoBackground -Width 1600 -Height 900
            New-BGInfoMetric -Region Right -Label 'CPU' -Value 40 -History 1, 2, 3
            New-BGInfoTable -Region Right -Column 'A', 'B' -Row @(, @('1', '2'))
            New-BGInfoList -Region Left -Item 'one'
            New-BGInfoCalendarHeatmap -Region Bottom -Data @{ '2026-10-01' = 1; '2026-10-08' = 4 }
            New-BGInfoTimeline -Region Bottom -Item @{ Lane = 'L'; Start = '2026-10-16'; End = '2026-10-17' }
            New-BGInfoTreemap -Region Center -Data @{ A = 1; B = 2 }
            New-BGInfoMermaid -Region Center -Definition "flowchart LR`n  A --> B"
            New-BGInfoQRCode -Region Left -Url 'https://help.contoso.com'
        }
        Test-Path -LiteralPath $path | Should -BeTrue
    }

    It 'round-trips widgets through Export-BGInfoConfiguration JSON' {
        $config = New-BGInfo -Theme Aurora -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoLayout -IconAreaWidth 50
            New-BGInfoMetric -Region Right -Label 'CPU' -Value 40 -WarningThreshold 80
            New-BGInfoQRCode -Url 'https://help'
        }
        $json = Join-Path $TestDrive 'widgets.json'
        Export-BGInfoConfiguration -InputObject $config -Path $json -Force
        $text = Get-Content -LiteralPath $json -Raw
        $text | Should -Match '"Widgets"'
        $text | Should -Match '"IconAreaWidth"'
        $text | Should -Match 'QrCode'
    }
}

Describe 'Data helpers' {
    It 'returns system facts as one plain object without throwing' {
        $facts = Get-BGInfoSystemInfo
        $facts.ComputerName | Should -Be $env:COMPUTERNAME
        $facts.UptimeText | Should -Not -BeNullOrEmpty
        $facts.DefenderStatus | Should -Not -BeNullOrEmpty
        $facts.BitLockerStatus | Should -Not -BeNullOrEmpty
        , $facts.Disks | Should -Not -BeNullOrEmpty
    }

    It 'resolves the new built-in value tokens' -ForEach @(
        @{ Token = 'Uptime' }, @{ Token = 'LastPatch' }, @{ Token = 'DefenderStatus' }, @{ Token = 'BitLockerStatus' },
        @{ Token = 'LoggedOnUser' }, @{ Token = 'Domain' }, @{ Token = 'DiskSummary' }
    ) {
        (New-BGInfoValue -BuiltinValue $Token).Value | Should -Not -BeNullOrEmpty
    }

    It 'returns the script block output' {
        Invoke-BGInfoDataSource -ScriptBlock { 'value' } -Default 'fallback' | Should -Be 'value'
    }

    It 'passes arguments to the script block' {
        Invoke-BGInfoDataSource -ScriptBlock { param($a, $b) $a + $b } -ArgumentList 2, 3 | Should -Be 5
    }

    It 'falls back when the source throws' {
        Invoke-BGInfoDataSource -Name 'Broken' -ScriptBlock { throw 'down' } -Default 'unknown' -WarningVariable warnings -WarningAction SilentlyContinue | Should -Be 'unknown'
        $warnings | Should -Match 'Broken'
    }

    It 'falls back when the source returns nothing' {
        Invoke-BGInfoDataSource -ScriptBlock { } -Default 'empty' -WarningAction SilentlyContinue | Should -Be 'empty'
    }

    It 'falls back when the source runs out of time and does not wait for it' {
        $watch = [System.Diagnostics.Stopwatch]::StartNew()
        Invoke-BGInfoDataSource -ScriptBlock { Start-Sleep -Seconds 30; 'late' } -TimeoutSeconds 1 -Default 'timeout' -WarningAction SilentlyContinue | Should -Be 'timeout'
        $watch.Elapsed.TotalSeconds | Should -BeLessThan 10
    }
}
