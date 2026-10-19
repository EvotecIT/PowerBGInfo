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
    It 'expires a queued source without running it and recovers capacity' {
        $entered = [Threading.ManualResetEventSlim]::new($false)
        $release = [Threading.ManualResetEventSlim]::new($false)
        $unexpected = [Threading.ManualResetEventSlim]::new($false)
        $first = [powershell]::Create()
        try {
            $null = $first.AddScript({
                param($module, $entered, $release)
                Import-Module $module
                Invoke-BGInfoDataSource -TimeoutSeconds 10 -ScriptBlock {
                    param($entered, $release)
                    $entered.Set(); $release.Wait(); 'first completed'
                } -ArgumentList $entered,$release
            }).AddArgument((Get-Command Invoke-BGInfoDataSource).Module.Path).AddArgument($entered).AddArgument($release)
            $handle = $first.BeginInvoke()
            $entered.Wait(5000) | Should -BeTrue
            $warnings = @()
            Invoke-BGInfoDataSource -TimeoutSeconds 1 -Default 'expired' -WarningVariable warnings -WarningAction SilentlyContinue -ScriptBlock {
                param($unexpected) $unexpected.Set(); 'must not run'
            } -ArgumentList $unexpected | Should -Be 'expired'
            $warnings.Count | Should -Be 1
            $unexpected.IsSet | Should -BeFalse
            $release.Set()
            $handle.AsyncWaitHandle.WaitOne(5000) | Should -BeTrue
            @($first.EndInvoke($handle))[0] | Should -Be 'first completed'
            Invoke-BGInfoDataSource -TimeoutSeconds 5 -ScriptBlock { 'recovered' } | Should -Be 'recovered'
        } finally {
            $release.Set(); $first.Dispose()
            $entered.Dispose(); $release.Dispose(); $unexpected.Dispose()
        }
    }

    It 'rejects thresholds mutated after metric creation through the public model' {
        $metric = New-BGInfoMetric -Label CPU -Value 85 -WarningThreshold 80 -CriticalThreshold 90
        $metric.ResolveStatus().ToString() | Should -Be 'Warning'
        $metric.Threshold.Critical = 70
        { $metric.Validate() } | Should -Throw '*critical threshold*'
        { $metric.ResolveStatus() } | Should -Throw '*critical threshold*'
        $metric.NumericValue = $null
        { $metric.ResolveStatus() } | Should -Throw '*critical threshold*'
        $metric.Threshold.Critical = 90
        $metric.NumericValue = 85
        $metric.Validate()
        $metric.ResolveStatus().ToString() | Should -Be 'Warning'
        $metric.NumericValue = $null; $metric.Status = 'Ok'
        $metric.ResolveStatus().ToString() | Should -Be 'Ok'
    }

    It 'preserves an existing usable export when a nested threshold becomes invalid' {
        $core = (New-BGInfoMetric -Label CPU -Value 85).GetType().Assembly
        $loader = $core.GetType('PowerBGInfo.BgInfoConfigurationJson')
        foreach ($kind in @('Metric', 'Chart')) {
            $item = if ($kind -eq 'Metric') { New-BGInfoMetric -Label CPU -Value 85 -WarningThreshold 80 -CriticalThreshold 90 }
                else { New-BGInfoChart -Values 85 -WarningThreshold 80 -CriticalThreshold 90 }
            $config = [Activator]::CreateInstance($core.GetType('PowerBGInfo.BgInfoConfiguration'))
            if ($kind -eq 'Metric') { $config.Widgets.Add($item) } else { $config.Charts.Add($item) }
            $path = Join-Path $TestDrive "$kind-preserved.json"
            Export-BGInfoConfiguration -InputObject $config -Path $path
            $before = (Get-FileHash $path).Hash
            $item.Threshold.Critical = 70
            { Export-BGInfoConfiguration -InputObject $config -Path $path -Force } | Should -Throw '*critical threshold*'
            (Get-FileHash $path).Hash | Should -Be $before
            $loaded = $loader::Load($path)
            $threshold = if ($kind -eq 'Metric') { $loaded.Widgets[0].Threshold } else { $loaded.Charts[0].Threshold }
            $threshold.Critical | Should -Be 90
        }
    }

    It 'preserves interleaved panel emission order without mutating reusable definitions' {
        $core = (New-BGInfoMetric -Label QUEUE -Value 42).GetType().Assembly
        $loader = $core.GetType('PowerBGInfo.BgInfoConfigurationJson')
        foreach ($case in @(
            @{ Type='BgInfoMetricWidget'; Collection='Widgets' }, @{ Type='BgInfoChart'; Collection='Charts' },
            @{ Type='BgInfoTopology'; Collection='Topologies' }, @{ Type='BgInfoVisualCanvas'; Collection='VisualCanvases' }
        )) {
            $a = [Activator]::CreateInstance($core.GetType("PowerBGInfo.$($case.Type)")); $a.RegionOrder = 17
            $b = [Activator]::CreateInstance($core.GetType("PowerBGInfo.$($case.Type)")); $b.RegionOrder = 18
            $path = Join-Path $TestDrive "$($case.Type)-order.json"
            New-BGInfo -ExportOnly -JsonPath $path -ConfigurationDirectory $TestDrive { $a; $b; $a } | Out-Null
            $config = $loader::Load($path)
            (@($config.($case.Collection) | ForEach-Object { $_.RegionOrder }) -join ',') | Should -Be '0,1,2'
            $a.RegionOrder | Should -Be 17
            $b.RegionOrder | Should -Be 18
        }
        $a = New-BGInfoMetric -Label QUEUE -Value 42
        $b = New-BGInfoVisualCanvas -NoHeroContent -NoTechBackdrop
        $path = Join-Path $TestDrive 'mixed-order.json'
        New-BGInfo -ExportOnly -JsonPath $path -ConfigurationDirectory $TestDrive { $a; $b; $a } | Out-Null
        $mixed = $loader::Load($path)
        ($mixed.Widgets.RegionOrder -join ',') | Should -Be '0,2'
        $mixed.VisualCanvases[0].RegionOrder | Should -Be 1
    }

    It 'rejects nonfinite metric history before emitting a widget' {
        foreach ($number in @([double]::NaN, [double]::PositiveInfinity, [double]::NegativeInfinity)) {
            { New-BGInfoMetric -Label QUEUE -Value 42 -History 1,$number } | Should -Throw '*finite*'
            { New-BGInfoMetric -Label QUEUE -Value 42 -History 1,2 -HistoryMaximum $number } | Should -Throw '*finite*'
        }
        $metric = New-BGInfoMetric -Label QUEUE -Value 42 -History 0,1.25,42 -HistoryMaximum 100
        $metric.History[1] | Should -Be 1.25
        $metric.HistoryMaximum | Should -Be 100
        { New-BGInfoMetric -Label QUEUE -Value 42 -History 42 } | Should -Throw '*at least two*'
        (New-BGInfoMetric -Label QUEUE -Value 42 -History 42 -HistoryKind Bars).History[0] | Should -Be 42
    }

    It 'round trips table and list configurations larger than the legacy serializer limit' {
        $core = (New-BGInfoMetric -Label QUEUE -Value 42).GetType().Assembly
        $serializer = $core.GetType('PowerBGInfo.BgInfoConfigurationJson')
        foreach ($kind in @('Table', 'List')) {
            $config = [Activator]::CreateInstance($core.GetType('PowerBGInfo.BgInfoConfiguration'))
            $widget = [Activator]::CreateInstance($core.GetType("PowerBGInfo.BgInfo${kind}Widget"))
            $text = 'x' * 2200
            if ($kind -eq 'Table') { $widget.Columns.Add('Fact') }
            for ($i = 0; $i -lt 1100; $i++) {
                if ($kind -eq 'Table') { $widget.Rows.Add([string[]]@("$i$text")) }
                else {
                    $item = [Activator]::CreateInstance($core.GetType('PowerBGInfo.BgInfoListItem'))
                    $item.Text = "$i$text"
                    $widget.Items.Add($item)
                }
            }
            $config.Widgets.Add($widget)
            $path = Join-Path $TestDrive "$kind-large.json"
            $serializer::Save($config, $path)
            (Get-Item $path).Length | Should -BeGreaterThan 2097152
            $loaded = $serializer::Load($path)
            if ($kind -eq 'Table') { $loaded.Widgets[0].Rows[1099][0] | Should -Be "1099$text" }
            else { $loaded.Widgets[0].Items[1099].Text | Should -Be "1099$text" }
        }
    }

    It 'cancels provider and capacity waits without releasing a running provider' {
        # BeginStop uses an engine-owned stop thread that can race pipeline disposal.
        # Keep the same bounded stop contract through the synchronous API on a task.
        if (-not ('PowerBGInfoTests.PipelineStop' -as [type])) {
            Add-Type -ReferencedAssemblies ([powershell].Assembly.Location) -TypeDefinition @"
namespace PowerBGInfoTests {
    public static class PipelineStop {
        public static System.Threading.Tasks.Task Request(System.Management.Automation.PowerShell pipeline) {
            return System.Threading.Tasks.Task.Run(() => pipeline.Stop());
        }
    }
}
"@
        }
        $release = [System.Threading.ManualResetEventSlim]::new($false)
        $started = [System.Threading.ManualResetEventSlim]::new($false)
        $finished = [System.Threading.ManualResetEventSlim]::new($false)
        $unexpected = [System.Threading.ManualResetEventSlim]::new($false)
        $first = [powershell]::Create()
        $second = [powershell]::Create()
        try {
            $module = (Get-Command Invoke-BGInfoDataSource).Module.Path
            $null = $first.AddScript({
                param($module, $started, $release, $finished)
                Import-Module $module
                Invoke-BGInfoDataSource -TimeoutSeconds 10 -ScriptBlock {
                    param($started, $release, $finished)
                    $started.Set()
                    try { $release.Wait(); 'late' } finally { $finished.Set() }
                } -ArgumentList $started,$release,$finished
            }).AddArgument($module).AddArgument($started).AddArgument($release).AddArgument($finished)
            $firstHandle = $first.BeginInvoke()
            $started.Wait(5000) | Should -BeTrue
            $firstStop = $([PowerBGInfoTests.PipelineStop]::Request($first))
            $firstStop.Wait(2000) | Should -BeTrue
            $null = $second.AddScript({
                param($module, $unexpected)
                Import-Module $module
                Invoke-BGInfoDataSource -TimeoutSeconds 10 -ScriptBlock { param($entered) $entered.Set() } -ArgumentList $unexpected
            }).AddArgument($module).AddArgument($unexpected)
            $secondHandle = $second.BeginInvoke()
            # Let the second pipeline enter its capacity wait before cancelling it.
            [Threading.Thread]::Sleep(150)
            $secondStop = $([PowerBGInfoTests.PipelineStop]::Request($second))
            $secondStop.Wait(2000) | Should -BeTrue
            $unexpected.IsSet | Should -BeFalse
        } finally {
            $release.Set()
            $finished.Wait(5000) | Should -BeTrue
            $first.Dispose()
            $second.Dispose()
            $release.Dispose(); $started.Dispose(); $finished.Dispose(); $unexpected.Dispose()
        }
        Invoke-BGInfoDataSource -TimeoutSeconds 5 -ScriptBlock { 'recovered' } | Should -Be 'recovered'
    }

    It 'uses localized numeric grouping and decimals in metric calendar and treemap inputs' {
        $previous = [Globalization.CultureInfo]::CurrentCulture
        try {
            foreach ($case in @(
                @{ Culture='de-DE'; Number='1.234'; Expected=1234 }
                @{ Culture='de-DE'; Number='1.234,5'; Expected=1234.5 }
                @{ Culture='pl-PL'; Number='1234,5'; Expected=1234.5 }
                @{ Culture='en-US'; Number='1,234.5'; Expected=1234.5 }
            )) {
                [Globalization.CultureInfo]::CurrentCulture = [Globalization.CultureInfo]::GetCultureInfo($case.Culture)
                $metric = New-BGInfoMetric -Label CPU -Value $case.Number
                $metric.NumericValue | Should -Be $case.Expected
                $calendar = @{ Date='2026-10-12'; Value=$case.Number } | New-BGInfoCalendarHeatmap
                $calendar.Items[0].Value | Should -Be $case.Expected
                $treemap = New-BGInfoTreemap -Data @{ Service=$case.Number }
                $treemap.Items[0].Value | Should -Be $case.Expected
            }
        } finally { [Globalization.CultureInfo]::CurrentCulture = $previous }
    }

    It 'bounds pending data source cleanup and recovers after a noncooperative call exits' {
        $release = [System.Threading.ManualResetEventSlim]::new($false)
        $finished = [System.Threading.ManualResetEventSlim]::new($false)
        $secondStarted = [System.Threading.ManualResetEventSlim]::new($false)
        try {
            Invoke-BGInfoDataSource -ScriptBlock {
                param($release, $finished)
                try { $release.Wait(); 'late' } finally { $finished.Set() }
            } -ArgumentList $release,$finished -TimeoutSeconds 1 -Default 'timeout' -WarningAction SilentlyContinue | Should -Be 'timeout'
            Invoke-BGInfoDataSource -ScriptBlock {
                param($started) $started.Set(); 'unexpected extra worker'
            } -ArgumentList $secondStarted -TimeoutSeconds 1 -Default 'busy' -WarningAction SilentlyContinue | Should -Be 'busy'
            $secondStarted.IsSet | Should -BeFalse
        } finally {
            $release.Set()
            $finished.Wait(5000) | Should -BeTrue
            $release.Dispose()
            $finished.Dispose()
            $secondStarted.Dispose()
        }
        Invoke-BGInfoDataSource -ScriptBlock { 'recovered' } -TimeoutSeconds 5 -Default 'still busy' -WarningAction SilentlyContinue | Should -Be 'recovered'
    }

    It 'uses both theme accents for categorical charts while preserving explicit paint overrides' {
        $theme = New-BGInfoTheme Ember -Mode Dark -Accent '#ED1545' -SecondaryAccent '#17BAA5'
        $config = New-BGInfo -Theme $theme -PassThru -ConfigurationDirectory $TestDrive {
            New-BGInfoChart -Kind Pie -Values 30,70 -NoHistory
            New-BGInfoChart -Kind Donut -Values 30,70 -NoHistory
            New-BGInfoChart -Kind Pie -Values 30,70 -Palette '#123456','#ABCDEF' -NoHistory
            New-BGInfoChart -Kind Bar -Values 30,70 -LineColor '#765432' -NoHistory
        }
        foreach ($index in 0,1) {
            $config.Charts[$index].Palette.Count | Should -BeGreaterOrEqual 2
            $config.Charts[$index].Palette[0].ToHex() | Should -Be '#ED1545'
            $config.Charts[$index].Palette[1].ToHex() | Should -Be '#17BAA5'
        }
        ($config.Charts[2].Palette | ForEach-Object ToHex) -join ',' | Should -Be '#123456,#ABCDEF'
        $config.Charts[3].LineColor.ToHex() | Should -Be '#765432'
        $config.Charts[3].Palette.Count | Should -Be 0
        (New-BGInfoChart -Kind Pie -Values 30,70 -NoHistory).Palette.Count | Should -Be 0
    }

    It 'skips reversed and unlabelled timeline records across typed and pipeline forms' {
        $typed = New-BGInfoTimelineItem -Lane 'Ops' -Start ([datetime]'2026-10-13') -End ([datetime]'2026-10-14')
        $typed.End = [datetime]'2026-10-12'
        $warnings = @()
        $timeline = @(
            @{ Lane='Ops'; Start='2026-10-13'; End='2026-10-12' }
            [pscustomobject]@{ Lane='Ops'; Start='2026-10-13'; To='2026-10-12' }
            $typed
            @{ Lane=''; Start='2026-10-13' }
            @{ Lane='Ops'; Start='2026-10-13'; End='2026-10-13' }
            @{ Lane='Ops'; Start=[datetime]'2026-10-13'; End=([datetime]'2026-10-13').AddTicks(1) }
            @{ Lane='Ops'; Start='1800-01-01' }
            @{ Lane='Ops'; Start='2026-10-13'; End='2026-10-14' }
            @{ Lane='Ops'; Start='2026-10-13' }
        ) | New-BGInfoTimeline -WarningVariable warnings -WarningAction SilentlyContinue
        $timeline.Items.Count | Should -Be 2
        $warnings.Count | Should -Be 7
        $timeline.Validate()
        $timeline.Items[0].End | Should -BeGreaterThan $timeline.Items[0].Start
        $timeline.Items[1].End | Should -BeNullOrEmpty
    }

    It 'rejects unrenderable helper ranges and renders valid mixed-zone and open intervals' {
        $utc = [datetime]::SpecifyKind([datetime]'2026-10-13 08:00:00', [DateTimeKind]::Utc)
        { New-BGInfoTimelineItem -Lane Ops -Start $utc -End $utc } | Should -Throw
        { New-BGInfoTimelineItem -Lane Ops -Start $utc -Duration ([timespan]::FromTicks(1)) } | Should -Throw
        { New-BGInfoTimelineItem -Lane Ops -Start ([datetime]'1800-01-01') } | Should -Throw
        $items = @(
            New-BGInfoTimelineItem -Lane Ops -Start $utc.ToLocalTime() -End $utc.AddHours(1)
            New-BGInfoTimelineItem -Lane Ops -Start $utc.AddHours(2)
        )
        $timeline = $items | New-BGInfoTimeline -Now $utc.AddHours(4)
        New-BGInfo -Target File -ConfigurationDirectory $TestDrive -OutputFileName 'timeline.png' {
            New-BGInfoBackground -Width 640 -Height 360
            $timeline
        } | Out-Null
        (Get-Item (Join-Path $TestDrive 'timeline.png')).Length | Should -BeGreaterThan 100
    }

    It 'uses the requested property before its aliases regardless of dictionary key order' {
        $calendar = [ordered] @{ Count = 99; Value = 3; Date = '2026-10-12' } | New-BGInfoCalendarHeatmap
        $calendar.Items[0].Value | Should -Be 3
        $calendar = @([ordered] @{ Count = 99; Value = $null; Date = '2026-10-12' }, @{ Date = '2026-10-13'; Value = 0 }) | New-BGInfoCalendarHeatmap -WarningAction SilentlyContinue
        $calendar.Items.Count | Should -Be 1
        $calendar.Items[0].Value | Should -Be 0
    }

    It 'rejects malformed range ends while preserving blank and omitted open ends' {
        $warnings = @()
        $timeline = @(
            @{ Lane = 'Ops'; Start = '2026-10-12'; End = 'N/A' }
            [pscustomobject] @{ Lane = 'Ops'; Start = '2026-10-12'; EndTime = 'bad date' }
            @{ Lane = 'Ops'; Start = '2026-10-12'; End = '' }
            @{ Lane = 'Ops'; Start = '2026-10-12' }
            @{ Lane = 'Ops'; Start = '2026-10-12'; End = '2026-10-13' }
        ) | New-BGInfoTimeline -WarningVariable warnings -WarningAction SilentlyContinue
        $timeline.Items.Count | Should -Be 3
        $warnings.Count | Should -Be 2
        $timeline.Items[0].End | Should -BeNullOrEmpty
        $timeline.Items[1].End | Should -BeNullOrEmpty
        $timeline.Items[2].End.Day | Should -Be 13
    }

    It 'returns only the fallback when a source emits data and a nonterminating error' {
        $warnings = @()
        $value = @(Invoke-BGInfoDataSource -ScriptBlock { 'partial'; Write-Error 'source unavailable' } -Default 'fallback' -WarningVariable warnings -WarningAction SilentlyContinue)
        $value.Count | Should -Be 1
        $value[0] | Should -Be 'fallback'
        $warnings.Count | Should -Be 1
    }

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
