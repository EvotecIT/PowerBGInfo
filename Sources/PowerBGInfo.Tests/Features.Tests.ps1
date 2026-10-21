$discoveryConfiguration = if ([string]::IsNullOrWhiteSpace($Env:PowerBGInfoDevelopmentConfiguration)) { 'Debug' } else { $Env:PowerBGInfoDevelopmentConfiguration }
$discoveryFallbackConfiguration = if ($discoveryConfiguration -eq 'Debug') { 'Release' } else { 'Debug' }
$discoveryFramework = if ($PSVersionTable.PSEdition -eq 'Desktop') { 'net472' } else { 'net8.0' }
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
        $theme.FontFamilyName = 'Arial'
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
        $config.Topologies[0].Accent | Should -Be $theme.Accent
        $config.Topologies[0].FontFamilyName | Should -Be 'Arial'
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

    It 'preserves tile status until the owning canvas supplies the color' {
        $tile = New-BGInfoVisualCanvasTile -Label 'Disk' -Value '91 %' -StatusValue 91 -WarningThreshold 80 -CriticalThreshold 90
        $tile.Status.ToString() | Should -Be 'Critical'
        $tile.Accent | Should -BeNullOrEmpty
        $explicit = New-BGInfoVisualCanvasTile -Label 'Disk' -Value '91 %' -Status Critical -Accent '#123456'
        $explicit.Accent.ToHex() | Should -Be '#123456'
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

    It 'parses textual checkbox values instead of treating False as truthy' {
        $list = New-BGInfoList -Item @(
            @{ Text = 'enabled'; Checked = 'True' },
            @{ Text = 'disabled'; Checked = 'False' }
        )

        $list.Items[0].Checked | Should -BeTrue
        $list.Items[1].Checked | Should -BeFalse
    }

    It 'collects calendar values from objects, dates and hashtables' {
        $fromObjects = @([pscustomobject] @{ Date = [datetime] '2026-10-01'; Value = 3 }) | New-BGInfoCalendarHeatmap
        $fromDates = [datetime] '2026-10-01', [datetime] '2026-10-02' | New-BGInfoCalendarHeatmap
        $fromData = New-BGInfoCalendarHeatmap -Data @{ '2026-10-01' = 5 }
        $fromObjects.Items[0].Value | Should -Be 3
        $fromDates.Items.Count | Should -Be 2
        $fromData.Items[0].Date | Should -Be ([datetime] '2026-10-01')
    }

    It 'uses <Culture> dates consistently in calendar and timeline inputs' -TestCases @(
        @{ Culture = 'en-GB' }
        @{ Culture = 'pl-PL' }
    ) {
        param($Culture)
        $previousCulture = [Globalization.CultureInfo]::CurrentCulture
        try {
            [Globalization.CultureInfo]::CurrentCulture = [Globalization.CultureInfo]::GetCultureInfo($Culture)
            $expectedStart = [datetime]::new(2026, 11, 10)
            $expectedEnd = [datetime]::new(2026, 11, 12)
            $fromObjects = [pscustomobject] @{ Date = '10/11/2026'; Value = 3 } | New-BGInfoCalendarHeatmap
            $fromData = New-BGInfoCalendarHeatmap -Data ([ordered] @{ '10/11/2026' = 5; '2026-11-12' = 6 })
            $timeline = New-BGInfoTimeline -Item @{ Lane = 'Server'; Start = '10/11/2026'; End = '12/11/2026' }

            $fromObjects.Items[0].Date | Should -Be $expectedStart
            $fromData.Items[0].Date | Should -Be $expectedStart
            $fromData.Items[1].Date | Should -Be $expectedEnd
            $timeline.Items[0].Start | Should -Be $expectedStart
            $timeline.Items[0].End | Should -Be $expectedEnd
        } finally {
            [Globalization.CultureInfo]::CurrentCulture = $previousCulture
        }
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

    It 'colors treemap items by -ColorProperty and its thresholds' {
        $vms = @(
            [pscustomobject] @{ Name = 'SQL'; MemoryGB = 128; Cpu = 71 }
            [pscustomobject] @{ Name = 'WEB'; MemoryGB = 16; Cpu = 9 }
        )
        $treemap = $vms | New-BGInfoTreemap -ValueProperty MemoryGB `
            -ColorProperty Cpu -WarningThreshold 60 -CriticalThreshold 85
        $treemap.Items[0].Value | Should -Be 128
        $treemap.Items[0].ColorValue | Should -Be 71
        $treemap.Threshold.Warning | Should -Be 60
        $treemap.HasColorValues | Should -BeTrue
        # Thresholds alone compare the sizes.
        $sized = New-BGInfoTreemap -Data @{ A = 3 } -WarningThreshold 2
        $sized.Items[0].ColorValue | Should -Be 3
        # A property that is not a number warns and keeps the default colors.
        $plain = $vms | New-BGInfoTreemap -ValueProperty MemoryGB `
            -ColorProperty Missing -WarningVariable warned -WarningAction SilentlyContinue
        $plain.HasColorValues | Should -BeFalse
        $warned | Should -Not -BeNullOrEmpty
    }

    It 'reads a timeline item color from piped objects' {
        $typedColor = (New-BGInfoTheme Graphite -Mode Light).Accent
        $timeline = New-BGInfoTimeline -Item @(
            [pscustomobject] @{ Lane = 'A'; Start = '2026-10-16'
                End = '2026-10-17'; Color = '#F59E0B' }
            [pscustomobject] @{ Lane = 'B'; Start = '2026-10-16'
                End = '2026-10-17'; Color = '' }
            [pscustomobject] @{ Lane = 'C'; Start = '2026-10-16'
                End = '2026-10-17'; Color = '  ' }
            [pscustomobject] @{ Lane = 'D'; Start = '2026-10-16'
                End = '2026-10-17'; Color = $typedColor }
        )
        $timeline.Items[0].Color.ToHex() | Should -Be '#F59E0B'
        $timeline.Items[1].Color | Should -BeNullOrEmpty
        $timeline.Items[2].Color | Should -BeNullOrEmpty
        $timeline.Items[3].Color | Should -Be $typedColor
        { New-BGInfoTimeline -Item @{ Lane = 'Bad'; Start = '2026-10-16'; Color = 'not-a-color' } } |
            Should -Throw '*Color*'
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
            New-BGInfoTreemap -Region Center -Data @{ A = 1; B = 2 } `
                -WarningThreshold 2
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

Describe 'Per-monitor example' {
    It 'validates the requested output target before running' {
        $example = Join-Path $PSScriptRoot '..\..\Examples\Run.BGInfo.PerMonitor.ps1'
        { & $example -Target Invalid } | Should -Throw
    }
}

Describe 'Data helpers' {
    It 'returns system facts as one plain object without throwing' {
        $facts = Get-BGInfoSystemInfo
        $facts.ComputerName | Should -Be ([Environment]::MachineName)
        $facts.UptimeText | Should -Not -BeNullOrEmpty
        $facts.DefenderStatus | Should -Not -BeNullOrEmpty
        $facts.BitLockerStatus | Should -Not -BeNullOrEmpty
        , $facts.Disks | Should -Not -BeNullOrEmpty
    }

    It 'reports the user of this session as LoggedOnUser' {
        $isWindowsHost = [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT
        $session = (Get-Process -Id $PID).SessionId
        if ($isWindowsHost -and $session -eq 0) {
            Set-ItResult -Skipped -Because 'Windows session 0 has no signed-in user to compare with'
            return
        }
        $expectedUser = if ($isWindowsHost) {
            "$env:USERDOMAIN\$env:USERNAME"
        } else {
            $desktopSession = [DesktopManager.PortableDesktopService]::new().GetSession()
            if ($desktopSession.HasGraphicalSession) { [Environment]::UserName } else { 'Unknown' }
        }
        (Get-BGInfoSystemInfo).LoggedOnUser | Should -Be $expectedUser
        $entry = New-BGInfoValue -BuiltinValue LoggedOnUser
        $entry.BuiltinValue | Should -Be 'LoggedOnUser'
        $entry.Value | Should -BeNullOrEmpty
        $provider = $entry.GetType().Assembly.GetType('PowerBGInfo.SystemInfoProvider')
        $provider.GetMethod('GetValue').Invoke($null, @('LoggedOnUser')) |
            Should -Be $expectedUser
    }

    It 'preserves the new dynamic built-in tokens without querying them during authoring' -ForEach @(
        @{ Token = 'Uptime' }, @{ Token = 'LastPatch' }, @{ Token = 'DefenderStatus' }, @{ Token = 'BitLockerStatus' },
        @{ Token = 'LoggedOnUser' }, @{ Token = 'Domain' }, @{ Token = 'DiskSummary' }
    ) {
        $entry = New-BGInfoValue -BuiltinValue $Token
        $entry.BuiltinValue | Should -Be $Token
        $entry.Value | Should -BeNullOrEmpty
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

Describe 'Repeated runs' {
    BeforeAll {
        $folder = Join-Path ([System.IO.Path]::GetTempPath()) ('bginfo-repeat-' + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $folder | Out-Null
        $base = New-BGInfo -Target File -ConfigurationDirectory $folder -OutputFileName 'base.png' -BackgroundColor Navy {
            New-BGInfoBackground -Width 320 -Height 180 -Color Navy
        }
        function Invoke-Render {
            [CmdletBinding()]
            param([string] $FilePath, [string] $OutputFileName)
            New-BGInfo -Target File -FilePath $FilePath -ConfigurationDirectory $folder -OutputFileName $OutputFileName {
                New-BGInfoValue -Name 'Host' -Value 'LAB-01' -Color White
            }
        }
    }

    AfterAll {
        Remove-Item -LiteralPath $folder -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'draws on the original when -FilePath is the image the run overwrites' {
        $first = Invoke-Render $base 'wallpaper.png'
        $hash = (Get-FileHash -LiteralPath $first).Hash
        $second = Invoke-Render $first 'wallpaper.png' -WarningVariable warnings
        $second | Should -Be $first
        (Get-FileHash -LiteralPath $second).Hash | Should -Be $hash
        $warnings | Should -BeNullOrEmpty
        Get-Content -LiteralPath (Join-Path $folder 'PowerBgInfo.outputs.txt') | Should -Contain "wallpaper.png|$base"
    }

    It 'keeps an explicit in-place image when its original record is missing' {
        $first = Invoke-Render $base 'again.png'
        Remove-Item -LiteralPath (Join-Path $folder 'PowerBgInfo.outputs.txt')
        $second = Invoke-Render $first 'again.png' -WarningVariable warnings -WarningAction SilentlyContinue
        $warnings | Should -BeNullOrEmpty
        $record = Get-Content -LiteralPath (Join-Path $folder 'PowerBgInfo.outputs.txt') | Where-Object { $_.StartsWith('again.png|') }
        $source = $record.Substring('again.png|'.Length)
        $source | Should -Not -Be $second
        Test-Path -LiteralPath $source | Should -BeTrue

        $hash = (Get-FileHash -LiteralPath $second).Hash
        $third = Invoke-Render $second 'again.png'
        (Get-FileHash -LiteralPath $third).Hash | Should -Be $hash
    }
}

Describe 'Get-BGInfoMonitor' {
    It 'returns plain monitor objects with the index New-BGInfo -MonitorIndex takes' {
        $monitors = @(Get-BGInfoMonitor)
        foreach ($monitor in $monitors) {
            $monitor.Index | Should -BeGreaterOrEqual 0
            $monitor.Width | Should -BeGreaterThan 0
            $monitor.Height | Should -BeGreaterThan 0
            $monitor.PSObject.Properties.Name | Should -Contain 'DeviceName'
            $monitor.PSObject.Properties.Name | Should -Contain 'Scale'
        }
        @($monitors | Where-Object Primary).Count | Should -BeLessOrEqual 1
        @($monitors.Index | Sort-Object -Unique).Count | Should -Be $monitors.Count
    }

    It 'filters to the primary monitor' {
        @(Get-BGInfoMonitor -Primary | Where-Object { -not $_.Primary }).Count | Should -Be 0
    }

    It 'works from a script in Constrained Language Mode, where DesktopManager types do not' {
        $module = (Get-Command Get-BGInfoMonitor).Module.Path
        $shell = (Get-Process -Id $PID).Path
        $script = @"
Import-Module -Name '$module'
`$ExecutionContext.SessionState.LanguageMode = 'ConstrainedLanguage'
`$count = @(Get-BGInfoMonitor | ForEach-Object { `$_.Width }).Count
try { [DesktopManager.Monitors]::new() | Out-Null; `$typed = 'allowed' } catch { `$typed = 'blocked' }
"`$(`$ExecutionContext.SessionState.LanguageMode)|`$count|`$typed"
"@
        $encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($script))
        $result = & $shell -NoLogo -NoProfile -NonInteractive -EncodedCommand $encoded
        $parts = "$(@($result)[-1])".Trim().Split('|')
        $parts[0] | Should -Be 'ConstrainedLanguage'
        [int] $parts[1] | Should -Be @(Get-BGInfoMonitor).Count
        $parts[2] | Should -Be 'blocked'
    }
}
