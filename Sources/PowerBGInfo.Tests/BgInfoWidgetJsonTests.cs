using ChartForgeX.Primitives;
using ChartForgeX.Themes;
using System.Globalization;

namespace PowerBGInfo.Tests;

public class BgInfoWidgetJsonTests
{
    [Fact]
    public void LoadRejectsUnknownWidgetKindsInsteadOfDroppingConfigurationData()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-unknown-widget-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "config.json");
        File.WriteAllText(path, """
            {
              "Widgets": [{
                "Kind": "FutureWidget",
                "Id": "important-widget",
                "Definition": "preserve this widget"
              }]
            }
            """);

        try
        {
            var exception = Assert.Throws<InvalidDataException>(() => BgInfoConfigurationJson.Load(path));
            Assert.Contains("FutureWidget", exception.Message);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void PartialStatusPaletteUsesTheConfiguredLightModeDefaults()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-partial-status-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "config.json");
        File.WriteAllText(path, """
            {
              "Widgets": [{
                "Kind": "Metric",
                "ThemeMode": "Light",
                "StatusCritical": "#123456",
                "Threshold": { "Warning": 70, "Critical": 90, "CriticalColor": "#123456" }
              }]
            }
            """);

        try
        {
            var loaded = BgInfoConfigurationJson.Load(path);
            var widget = Assert.IsType<BgInfoMetricWidget>(Assert.Single(loaded.Widgets));
            var light = BgInfoStatusColors.Default(VisualThemeMode.Light);

            Assert.Equal(light.Warning, widget.StatusColors!.Warning);
            Assert.Equal(light.Ok, widget.StatusColors.Ok);
            Assert.Equal(ChartColor.FromHex("#123456"), widget.StatusColors.Critical);
            Assert.Equal(light.Warning, widget.Threshold!.Colors!.Warning);
            Assert.Equal(light.Ok, widget.Threshold.Colors.Ok);
            Assert.Equal(ChartColor.FromHex("#123456"), widget.Threshold.Colors.Critical);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void LayoutWidgetsRegionsAndThresholdsRoundTripThroughJson()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-widgets-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "config.json");
        var theme = BgInfoTheme.Create(BgInfoThemePreset.Ember, VisualThemeMode.Light);
        var config = new BgInfoConfiguration { ConfigurationDirectory = directory, Target = BgInfoTarget.File };
        config.Layout = new BgInfoLayout { MarginX = 48, MarginY = 40, Gap = 16, ColumnWidth = 420, TaskbarReserve = 0, IconAreaWidth = 90 };
        config.Charts.Add(new BgInfoChart {
            Kind = BgInfoChartKind.Gauge, Values = new double[] { 91 }, Region = BgInfoRegion.Right, RegionOrder = 3,
            Threshold = new BgInfoThreshold { Warning = 80, Critical = 90, Colors = theme.StatusColors }
        });
        config.Topologies.Add(new BgInfoTopology { Region = BgInfoRegion.Center, RegionOrder = 4 });
        config.VisualCanvases.Add(new BgInfoVisualCanvas { Region = BgInfoRegion.Left, RailMarginX = 64, RailMarginY = 56 });

        var metric = new BgInfoMetricWidget {
            Label = "CPU", Value = "38", Unit = "%", NumericValue = 38, History = new double[] { 1, 2, 3 },
            HistoryKind = BgInfoMetricHistoryKind.Bars, Threshold = new BgInfoThreshold { Warning = 70, Direction = BgInfoThresholdDirection.Below },
            Region = BgInfoRegion.Right, RegionOrder = 1, BackgroundColor = theme.Surface, StatusColors = theme.StatusColors, Scale = 1.5
        };
        var table = new BgInfoTableWidget { StatusColumn = "State", Dense = true, Region = BgInfoRegion.Bottom, Title = "Services" };
        table.Columns.AddRange(new[] { "Name", "State" });
        table.Rows.Add(new[] { "Spooler", "OK" });
        var list = new BgInfoListWidget { Marker = BgInfoListMarker.Number };
        list.Items.Add(new BgInfoListItem { Text = "Call", Value = "4357", Status = BgInfoStatus.Info, Checked = true });
        var calendar = new BgInfoCalendarHeatmapWidget { FirstDayOfWeek = DayOfWeek.Sunday, ShowLegend = true, Color = theme.Accent };
        calendar.Items.Add(new BgInfoDatedValue { Date = new DateTime(2026, 10, 1), Value = 3 });
        var timeline = new BgInfoTimelineWidget { Now = new DateTime(2026, 10, 16, 9, 0, 0), ShowLegend = false };
        timeline.Items.Add(new BgInfoTimelineItem { Lane = "On call", Label = "A", Start = new DateTime(2026, 10, 15), End = null, Category = "Rota", Status = BgInfoStatus.Ok });
        var treemap = new BgInfoTreemapWidget { ValueSuffix = " GB", ShowValues = false };
        treemap.Items.Add(new BgInfoTreemapItem { Name = "Logs", Value = 96, Group = "D:" });
        config.Widgets.AddRange(new BgInfoWidget[] {
            metric, table, list, calendar, timeline, treemap,
            new BgInfoMermaidWidget { Definition = "flowchart LR\n A --> B" },
            new BgInfoQrCodeWidget { Text = "https://help", Caption = "help", ModuleColor = ChartColor.Black }
        });

        try
        {
            BgInfoConfigurationJson.Save(config, path);
            var loaded = BgInfoConfigurationJson.Load(path);

            Assert.NotNull(loaded.Layout);
            Assert.Equal(48, loaded.Layout!.MarginX);
            Assert.Equal(420, loaded.Layout.ColumnWidth);
            Assert.Equal(0, loaded.Layout.TaskbarReserve);
            Assert.Equal(90, loaded.Layout.IconAreaWidth);
            Assert.Equal(BgInfoRegion.Right, loaded.Charts[0].Region);
            Assert.Equal(3, loaded.Charts[0].RegionOrder);
            Assert.Equal(90, loaded.Charts[0].Threshold!.Critical);
            Assert.Equal(theme.Critical, loaded.Charts[0].Threshold!.Colors!.Critical);
            Assert.Equal(BgInfoRegion.Center, loaded.Topologies[0].Region);
            Assert.Equal(64, loaded.VisualCanvases[0].RailMarginX);
            Assert.Equal(BgInfoRegion.Left, loaded.VisualCanvases[0].Region);

            Assert.Equal(8, loaded.Widgets.Count);
            var loadedMetric = Assert.IsType<BgInfoMetricWidget>(loaded.Widgets[0]);
            Assert.Equal("CPU", loadedMetric.Label);
            Assert.Equal(38, loadedMetric.NumericValue);
            Assert.Equal(BgInfoMetricHistoryKind.Bars, loadedMetric.HistoryKind);
            Assert.Equal(BgInfoThresholdDirection.Below, loadedMetric.Threshold!.Direction);
            Assert.Equal(BgInfoRegion.Right, loadedMetric.Region);
            Assert.Equal(1.5, loadedMetric.Scale);
            Assert.Equal(theme.Surface, loadedMetric.BackgroundColor);
            Assert.Equal(theme.Warning, loadedMetric.StatusColors!.Warning);

            var loadedTable = Assert.IsType<BgInfoTableWidget>(loaded.Widgets[1]);
            Assert.Equal(new[] { "Name", "State" }, loadedTable.Columns);
            Assert.Equal("OK", loadedTable.Rows[0][1]);
            Assert.True(loadedTable.Dense);
            Assert.Equal("Services", loadedTable.Title);

            var loadedList = Assert.IsType<BgInfoListWidget>(loaded.Widgets[2]);
            Assert.Equal(BgInfoListMarker.Number, loadedList.Marker);
            Assert.Equal(BgInfoStatus.Info, loadedList.Items[0].Status);
            Assert.True(loadedList.Items[0].Checked);

            var loadedCalendar = Assert.IsType<BgInfoCalendarHeatmapWidget>(loaded.Widgets[3]);
            Assert.Equal(new DateTime(2026, 10, 1), loadedCalendar.Items[0].Date);
            Assert.Equal(DayOfWeek.Sunday, loadedCalendar.FirstDayOfWeek);

            var loadedTimeline = Assert.IsType<BgInfoTimelineWidget>(loaded.Widgets[4]);
            Assert.Equal(new DateTime(2026, 10, 16, 9, 0, 0), loadedTimeline.Now);
            Assert.Null(loadedTimeline.Items[0].End);
            Assert.Equal(BgInfoStatus.Ok, loadedTimeline.Items[0].Status);
            Assert.False(loadedTimeline.ShowLegend);

            var loadedTreemap = Assert.IsType<BgInfoTreemapWidget>(loaded.Widgets[5]);
            Assert.Equal("D:", loadedTreemap.Items[0].Group);
            Assert.False(loadedTreemap.ShowValues);

            Assert.Contains("A --> B", Assert.IsType<BgInfoMermaidWidget>(loaded.Widgets[6]).Definition);
            var loadedQr = Assert.IsType<BgInfoQrCodeWidget>(loaded.Widgets[7]);
            Assert.Equal("https://help", loadedQr.Text);
            Assert.Equal(ChartColor.Black, loadedQr.ModuleColor);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }
}

public class BgInfoSystemInfoTests
{
    [Fact]
    public void SnapshotAlwaysReturnsWithoutThrowing()
    {
        var snapshot = SystemInfoProvider.GetSnapshot();

        Assert.Equal(Environment.MachineName, snapshot.ComputerName);
        Assert.False(string.IsNullOrWhiteSpace(snapshot.UptimeText));
        Assert.False(string.IsNullOrWhiteSpace(snapshot.DefenderStatus));
        Assert.False(string.IsNullOrWhiteSpace(snapshot.BitLockerStatus));
        Assert.NotNull(snapshot.Disks);
        Assert.All(snapshot.Disks, disk => Assert.InRange(disk.UsedPercent, 0, 100));
    }

    [Theory]
    [InlineData(0, 0, 5, "5 min")]
    [InlineData(0, 3, 12, "3 h 12 min")]
    [InlineData(1, 2, 0, "1 day 2 h")]
    [InlineData(41, 6, 0, "41 days 6 h")]
    public void UptimeIsShortAndReadable(int days, int hours, int minutes, string expected)
    {
        Assert.Equal(expected, SystemInfoProvider.FormatUptime(new TimeSpan(days, hours, minutes, 0)));
        Assert.Equal(SystemInfoProvider.Unknown, SystemInfoProvider.FormatUptime(null));
    }

    [Theory]
    [InlineData("10/14/2026", 2026, 10, 14)]
    [InlineData("23-10-2013", 2013, 10, 23)]
    [InlineData("20261014", 2026, 10, 14)]
    [InlineData("2026-10-14", 2026, 10, 14)]
    public void PatchDatesParseInTheFormatsWindowsStores(string text, int year, int month, int day)
    {
        Assert.Equal(new DateTime(year, month, day), SystemInfoProvider.ParsePatchDate(text));
    }

    [Theory]
    [InlineData("en-US", 3, 4)]
    [InlineData("en-GB", 4, 3)]
    public void AmbiguousPatchDatesFollowTheCurrentShortDateCulture(string cultureName, int month, int day)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            Assert.Equal(new DateTime(2013, month, day), SystemInfoProvider.ParsePatchDate("03/04/2013"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void LocalizedDayFirstPatchDatesFollowTheCurrentCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            Assert.Equal(new DateTime(2013, 10, 23), SystemInfoProvider.ParsePatchDate("23/10/2013"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void UnreadablePatchDatesAreIgnored()
    {
        Assert.Null(SystemInfoProvider.ParsePatchDate(null));
        Assert.Null(SystemInfoProvider.ParsePatchDate("not a date"));
    }

    [Fact]
    public void NewBuiltInTokensResolve()
    {
        foreach (var token in new[] { "Uptime", "LastPatch", "DefenderStatus", "BitLockerStatus", "LoggedOnUser", "Domain", "DiskSummary" })
        {
            Assert.False(string.IsNullOrWhiteSpace(SystemInfoProvider.GetValue(token)), token + " must resolve to a value or Unknown.");
        }
    }

    [Fact]
    public void DiskSummaryListsEachDrive()
    {
        var summary = SystemInfoProvider.FormatDiskSummary(new[] {
            new BgInfoDiskInfo { Drive = "C:", UsedPercent = 61.2 },
            new BgInfoDiskInfo { Drive = "D:", UsedPercent = 88 }
        });

        Assert.Contains("C: 61 %", summary);
        Assert.Contains("D: 88 %", summary);
        Assert.Equal(SystemInfoProvider.Unknown, SystemInfoProvider.FormatDiskSummary(Array.Empty<BgInfoDiskInfo>()));
    }
}
