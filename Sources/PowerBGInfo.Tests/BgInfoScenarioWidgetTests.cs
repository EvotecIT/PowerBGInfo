using ChartForgeX;
using ChartForgeX.Primitives;
using ChartForgeX.Themes;
using System.Globalization;
using System.Xml.Linq;

namespace PowerBGInfo.Tests;

public class BgInfoScenarioWidgetTests
{
    private static BgInfoTableWidget SessionTable()
    {
        var table = new BgInfoTableWidget { StatusColumn = "State" };
        table.Columns.AddRange(new[] { "User", "State" });
        table.Rows.Add(new[] { "anna", "Active" });
        table.Rows.Add(new[] { "piotr", "Disconnected" });
        return table;
    }

    [Fact]
    public void StatusMapSurvivesJsonRoundTrip()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-status-map-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "config.json");
        var table = SessionTable();
        table.StatusMap["Active"] = BgInfoStatus.Ok;
        table.StatusMap["Disconnected"] = BgInfoStatus.Warning;
        var configuration = new BgInfoConfiguration();
        configuration.Widgets.Add(table);

        try
        {
            BgInfoConfigurationJson.Save(configuration, path);
            var loaded = Assert.IsType<BgInfoTableWidget>(BgInfoConfigurationJson.Load(path).Widgets[0]);

            Assert.Equal(BgInfoStatus.Ok, loaded.StatusMap["active"]);
            Assert.Equal(BgInfoStatus.Warning, loaded.StatusMap["Disconnected"]);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void StatusMapRejectsUnknownStatusInJson()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-status-map-bad-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "config.json");
        File.WriteAllText(path, """
            {
              "Widgets": [{
                "Kind": "Table",
                "Columns": ["User", "State"],
                "Rows": [["anna", "Active"]],
                "StatusColumn": "State",
                "StatusMap": { "Active": "Green" }
              }]
            }
            """);

        try
        {
            var exception = Assert.ThrowsAny<Exception>(() => BgInfoConfigurationJson.Load(path));
            Assert.Contains("Green", exception.Message);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void StatusMapNeedsAStatusColumnAndARealStatus()
    {
        var table = SessionTable();
        table.StatusColumn = null;
        table.StatusMap["Active"] = BgInfoStatus.Ok;
        Assert.Throws<InvalidOperationException>(() => table.Validate());

        table = SessionTable();
        table.StatusMap["Active"] = BgInfoStatus.None;
        Assert.Throws<InvalidOperationException>(() => table.Validate());
    }

    [Fact]
    public void StatusMapColorsWordsTheBuiltInListDoesNotKnow()
    {
        var plain = SessionTable();
        var mapped = SessionTable();
        mapped.StatusMap["Active"] = BgInfoStatus.Ok;
        mapped.StatusMap["Disconnected"] = BgInfoStatus.Warning;

        using var before = BgInfoWidgetRenderer.Render(plain, new BgInfoConfiguration(), 420, 140);
        using var after = BgInfoWidgetRenderer.Render(mapped, new BgInfoConfiguration(), 420, 140);

        Assert.False(before.ToRgbaImage().Pixels.SequenceEqual(after.ToRgbaImage().Pixels));
    }

    [Theory]
    [InlineData(BgInfoChartKind.Bar)]
    [InlineData(BgInfoChartKind.Line)]
    [InlineData(BgInfoChartKind.Area)]
    public void CategoryTrendChartsShowTheirLabels(BgInfoChartKind kind)
    {
        var labels = new[] { "Thu 8", "Fri 9", "Sat 10", "Sun 11" };
        var chart = new BgInfoChart { Kind = kind, Labels = labels };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, new[] { 4d, 9, 2, 6 }, new BgInfoConfiguration(), 520, 220);
        var texts = XDocument.Parse(rendered.ToSvg()).Descendants()
            .Where(node => node.Name.LocalName == "text")
            .Select(node => node.Value)
            .ToArray();

        foreach (var label in labels) Assert.Contains(label, texts);
    }

    [Fact]
    public void TrendChartsHonourTheConfiguredMaximum()
    {
        var chart = new BgInfoChart { Kind = BgInfoChartKind.Line, Maximum = 100 };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, new[] { 41d, 47, 52 }, new BgInfoConfiguration(), 520, 220);

        Assert.Equal(100, rendered.Options.YAxis.Maximum);
    }

    [Fact]
    public void LabelledBarsLeaveRoomForLabelsAboveTheTallestBar()
    {
        var chart = new BgInfoChart { Kind = BgInfoChartKind.Bar, ShowDataLabels = true };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, new[] { 12d, 40, 25 }, new BgInfoConfiguration(), 520, 220);

        Assert.True(rendered.Options.YAxis.Maximum >= 40 * 1.18 - 0.001);
        Assert.Equal(0, rendered.Options.YAxis.Minimum);
    }

    [Fact]
    public void TimelineTicksStartAtTheFirstDayAndThinOutToFit()
    {
        var first = new DateTime(2026, 10, 3, 9, 0, 0);
        var last = new DateTime(2026, 10, 16, 18, 0, 0);

        var wide = BgInfoWidgetRenderer.TimelineDayTicks(first, last, 1400);
        Assert.Equal(first.Date, wide[0]);
        Assert.Equal(14, wide.Count);

        var narrow = BgInfoWidgetRenderer.TimelineDayTicks(first, last, 1000);
        Assert.Equal(first.Date, narrow[0]);
        Assert.All(narrow.Zip(narrow.Skip(1), (a, b) => (b - a).TotalDays), gap => Assert.Equal(2, gap));
        Assert.True(narrow.Count <= (1000 - 140) / 72);
    }

    [Fact]
    public void LongTimelinesUseWeeklyTicksAndKeepTheStartDay()
    {
        var first = new DateTime(2026, 10, 1); // Thursday
        var last = new DateTime(2026, 12, 31);

        var ticks = BgInfoWidgetRenderer.TimelineDayTicks(first, last, 900);

        Assert.Equal(first, ticks[0]);
        Assert.All(ticks.Skip(1), tick => Assert.Equal(DayOfWeek.Monday, tick.DayOfWeek));
        Assert.True(ticks.Count <= (900 - 140) / 72 + 1);
    }

    [Fact]
    public void TimelineDayLabelsFollowTheCurrentCulture()
    {
        var before = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.Equal("Thu 8", BgInfoWidgetRenderer.FormatTimelineDay(new DateTime(2026, 10, 8)));
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            Assert.Equal("Do 8", BgInfoWidgetRenderer.FormatTimelineDay(new DateTime(2026, 10, 8)));
        }
        finally { CultureInfo.CurrentCulture = before; }
    }

    [Theory]
    [InlineData(VisualThemeMode.Light)]
    [InlineData(VisualThemeMode.Dark)]
    public void CalendarZeroDaysGetANeutralCellAndTheBusiestDayTheFullColor(VisualThemeMode mode)
    {
        var accent = ChartColor.FromHex("#2F6FEB");
        var widget = new BgInfoCalendarHeatmapWidget { ThemeMode = mode, Color = accent };
        var day = new DateTime(2026, 10, 1);
        foreach (var value in new[] { 0d, 3, 4, 5, 4, 0, 40 }) {
            widget.Items.Add(new BgInfoDatedValue { Date = day, Value = value });
            day = day.AddDays(1);
        }
        var style = new BgInfoWidgetStyle(widget, new BgInfoConfiguration());

        var scale = BgInfoWidgetRenderer.CalendarScale(widget, style);
        var zero = scale.ColorFor(0);
        var lowest = scale.ColorFor(3);
        var typical = scale.ColorFor(4);
        var busiest = scale.ColorFor(40);

        Assert.Equal(style.Muted.R, zero.R);
        Assert.Equal(style.Muted.G, zero.G);
        Assert.Equal(style.Muted.B, zero.B);
        Assert.Equal(accent, busiest);
        Assert.Equal(accent.R, lowest.R);
        Assert.True(lowest.A < typical.A, "The median day should be clearly stronger than the quietest day.");
        Assert.True(typical.A < busiest.A);
    }
}
