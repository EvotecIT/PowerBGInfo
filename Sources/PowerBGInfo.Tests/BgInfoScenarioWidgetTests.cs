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

    [Theory]
    [InlineData("Green")]
    [InlineData("None")]
    public void StatusMapRejectsUnsupportedStatusInJson(string status)
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-status-map-bad-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "config.json");
        File.WriteAllText(path, $$"""
            {
              "Widgets": [{
                "Kind": "Table",
                "Columns": ["User", "State"],
                "Rows": [["anna", "Active"]],
                "StatusColumn": "State",
                "StatusMap": { "Active": "{{status}}" }
              }]
            }
            """);

        try
        {
            var exception = Assert.ThrowsAny<Exception>(() => BgInfoConfigurationJson.Load(path));
            Assert.Contains($"'{status}'", exception.Message);
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

        table.StatusMap["Active"] = (BgInfoStatus)99;
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
        var chart = new BgInfoChart { Kind = kind, Labels = labels, UseHistory = false };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, new[] { 4d, 9, 2, 6 }, new BgInfoConfiguration(), 520, 220);
        var texts = XDocument.Parse(rendered.ToSvg()).Descendants()
            .Where(node => node.Name.LocalName == "text")
            .Select(node => node.Value)
            .ToArray();

        foreach (var label in labels) Assert.Contains(label, texts);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CategoryLabelsFollowCurrentValuesAfterHistoryAppendOrTrim(bool trimmed)
    {
        var chart = new BgInfoChart
        {
            Kind = BgInfoChartKind.Bar,
            Values = new[] { 10d, 20d },
            Labels = new[] { "Mar", "Apr" }
        };
        var retained = trimmed ? new[] { 20d } : new[] { 2d, 3d, 10d, 20d };
        var expected = trimmed ? new[] { "Apr" } : new[] { "1", "2", "Mar", "Apr" };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, retained, new BgInfoConfiguration(), 520, 220);
        Assert.Equal(expected, rendered.Options.XAxisLabels.Select(label => label.Text));
    }

    [Fact]
    public void TrendChartsHonourTheConfiguredMaximum()
    {
        var chart = new BgInfoChart { Kind = BgInfoChartKind.Line, Maximum = 100 };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, new[] { 41d, 47, 52 }, new BgInfoConfiguration(), 520, 220);

        Assert.Equal(100, rendered.Options.YAxis.Maximum);
    }

    [Theory]
    [InlineData(12, 40, 25, 0)]
    [InlineData(-1000, 1, 0, -1000)]
    public void LabelledBarsLeaveRoomForLabelsAboveTheTallestBar(double first, double peak, double third, double minimum)
    {
        var chart = new BgInfoChart { Kind = BgInfoChartKind.Bar, ShowDataLabels = true };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, new[] { first, peak, third }, new BgInfoConfiguration(), 520, 220);

        Assert.Equal(minimum, rendered.Options.YAxis.Minimum);
        Assert.True(rendered.Options.YAxis.Maximum - peak >= (peak - minimum) * 0.18 - 0.001);
    }

    [Fact]
    public void LabelledBarsRetainNegativeValuesWithAutomaticOrExplicitMaximum()
    {
        foreach (var maximum in new double?[] { null, 20 })
        {
            var chart = new BgInfoChart { Kind = BgInfoChartKind.Bar, ShowDataLabels = true, Maximum = maximum };
            var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, new[] { -5d, 10 }, new BgInfoConfiguration(), 520, 220);

            Assert.Equal(-5, rendered.Options.YAxis.Minimum);
            Assert.True(rendered.Options.YAxis.Maximum > 10);
        }
    }

    [Fact]
    public void BatchedStatusRowsKeepTheirStatusDotsInOneColumn()
    {
        var table = new BgInfoTableWidget { StatusColumn = "State" };
        table.Columns.AddRange(new[] { "User", "State" });
        for (var row = 0; row < 48; row++)
            table.Rows.Add(new[] { row < 24 ? "a" : "a-much-longer-account-name", "OK" });

        using var rendered = BgInfoWidgetRenderer.Render(table, new BgInfoConfiguration(), 400, 300);
        var image = rendered.ToRgbaImage();
        var xs = new List<int>();
        for (var y = 0; y < image.Height; y++)
            for (var x = 0; x < image.Width; x++)
            {
                var offset = (y * image.Width + x) * 4;
                if (image.Pixels[offset] < 100 && image.Pixels[offset + 1] > 150 &&
                    image.Pixels[offset + 2] < 200 && image.Pixels[offset + 3] > 200)
                    xs.Add(x);
            }

        Assert.True(xs.Count > 48, "Expected visible green status dots from the rendered rows.");
        Assert.InRange(xs.Max() - xs.Min(), 1, 20);
    }

    [Theory]
    [InlineData(DateTimeKind.Unspecified, 9)]
    [InlineData(DateTimeKind.Utc, 9)]
    [InlineData(DateTimeKind.Local, 9)]
    [InlineData(DateTimeKind.Unspecified, 22)]
    public void TimelineTicksStartAtTheFirstDayAndThinOutToFit(DateTimeKind kind, int hour)
    {
        var first = new DateTime(2026, 10, 3, hour, 0, 0, kind);
        var last = new DateTime(2026, 10, 16, 18, 0, 0, kind);
        var firstInstant = kind == DateTimeKind.Local ? first.ToUniversalTime() : first;
        var lastInstant = kind == DateTimeKind.Local ? last.ToUniversalTime() : last;

        var wide = BgInfoWidgetRenderer.TimelineDayTicks(first, last, 1400);
        Assert.Equal(firstInstant, wide[0]);
        Assert.InRange(wide.Count, 2, 14);
        Assert.All(wide.Zip(wide.Skip(1), (a, b) => (b - a).TotalDays),
            gap => Assert.True(gap >= (lastInstant - firstInstant).TotalDays / 17));

        var narrow = BgInfoWidgetRenderer.TimelineDayTicks(first, last, 1000);
        Assert.Equal(firstInstant, narrow[0]);
        Assert.All(narrow.Zip(narrow.Skip(1), (a, b) => (b - a).TotalDays),
            gap => Assert.True(gap * (1000 - 140) / (lastInstant - firstInstant).TotalDays >= 72));
        Assert.All(wide, tick => Assert.InRange(tick, firstInstant, lastInstant));
        Assert.True(narrow.Count <= (1000 - 140) / 72);
    }

    [Fact]
    public void LongTimelinesUseWeeklyTicksAndKeepTheStartDay()
    {
        var first = new DateTime(2026, 10, 1); // Thursday
        var last = new DateTime(2026, 12, 31);

        var ticks = BgInfoWidgetRenderer.TimelineDayTicks(first, last, 900);

        Assert.Equal(first, ticks[0]);
        Assert.True(ticks.Count > 1);
        Assert.All(ticks.Skip(1), tick => Assert.Equal(DayOfWeek.Monday, tick.DayOfWeek));
        Assert.All(ticks.Zip(ticks.Skip(1), (earlier, later) => later - earlier),
            gap => Assert.True(gap >= TimeSpan.FromDays(9)));
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

    [Fact]
    public void CalendarScaleUsesDailyTotalsForRepeatedDates()
    {
        var accent = ChartColor.FromHex("#2F6FEB");
        var widget = new BgInfoCalendarHeatmapWidget { Color = accent };
        var day = new DateTime(2026, 10, 1);
        widget.Items.Add(new BgInfoDatedValue { Date = day, Value = 2 });
        widget.Items.Add(new BgInfoDatedValue { Date = day.AddDays(1), Value = 3 });
        for (var record = 0; record < 3; record++)
            widget.Items.Add(new BgInfoDatedValue { Date = day.AddDays(2).AddHours(record), Value = 3 });

        var scale = BgInfoWidgetRenderer.CalendarScale(widget, new BgInfoWidgetStyle(widget, new BgInfoConfiguration()));

        Assert.Equal(accent, scale.ColorFor(9));
        Assert.True(scale.ColorFor(3).A < scale.ColorFor(9).A);
    }
}
