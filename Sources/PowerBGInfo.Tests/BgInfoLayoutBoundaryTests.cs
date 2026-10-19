using System.Globalization;
using ChartForgeX.Primitives;

namespace PowerBGInfo.Tests;

public class BgInfoLayoutBoundaryTests {
    [Theory]
    [InlineData(400, false, 24)]
    [InlineData(400, true, 24)]
    [InlineData(1440, false, 24)]
    [InlineData(120, false, 500)]
    public void OpposingColumnsReserveTheirGapWithOrWithoutCenterContent(int width, bool withCenter, int gap) {
        var layout = new BgInfoLayout { MarginX = 20, MarginY = 20, TaskbarReserve = 0, Gap = gap, ColumnWidth = 1000 };
        var requests = new List<BgInfoLayoutRequest> {
            new(BgInfoRegion.Left, 0, 100), new(BgInfoRegion.Right, 0, 100)
        };
        if (withCenter) requests.Add(new(BgInfoRegion.Center, 0, 100));
        var safe = BgInfoLayoutEngine.GetSafeArea(width, 900, layout);
        var slots = BgInfoLayoutEngine.Resolve(width, 900, layout, null, requests);
        Assert.Equal(safe.X, slots[0].X);
        Assert.Equal(safe.X + safe.Width, slots[1].X + slots[1].Width);
        var expectedGap = Math.Min(gap, safe.Width - 2);
        if (withCenter) {
            Assert.True(slots[2].X - (slots[0].X + slots[0].Width) >= expectedGap - 1);
            Assert.True(slots[1].X - (slots[2].X + slots[2].Width) >= expectedGap - 1);
        } else Assert.True(slots[1].X - (slots[0].X + slots[0].Width) >= expectedGap - 1);
    }

    [Fact]
    public void TreemapDecimalCaptionsFollowTheCurrentWallpaperCulture() {
        var before = CultureInfo.CurrentCulture;
        var widget = new BgInfoTreemapWidget { ValueSuffix = " GB", ShowValues = true };
        widget.Items.Add(new BgInfoTreemapItem { Name = "Data", Value = 412.5 });
        try {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            using var english = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 400, 300);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            using var german = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 400, 300);
            Assert.False(english.ToRgbaImage().Pixels.SequenceEqual(german.ToRgbaImage().Pixels));
        } finally { CultureInfo.CurrentCulture = before; }
    }

    [Theory]
    [InlineData(BgInfoRegion.Top, -250)]
    [InlineData(BgInfoRegion.Bottom, -250)]
    [InlineData(BgInfoRegion.Top, 900)]
    [InlineData(BgInfoRegion.Bottom, 900)]
    public void TextOutsideTheSafeSpanDoesNotMoveBandPanelsIntoMargins(BgInfoRegion region, int textX) {
        var layout = new BgInfoLayout { MarginX = 100, MarginY = 20, Gap = 20, TaskbarReserve = 0 };
        var requests = new[] { new BgInfoLayoutRequest(region, 0, 80) };
        var plain = BgInfoLayoutEngine.Resolve(800, 480, layout, null, requests)[0];
        var withText = BgInfoLayoutEngine.Resolve(800, 480, layout, new ChartRect(textX, 0, 100, 480), requests)[0];
        Assert.Equal(plain, withText);
        Assert.InRange(withText.X, 100, 700);
        Assert.True(withText.X + withText.Width <= 700);
    }

    [Theory]
    [InlineData("th-TH")]
    [InlineData("ar-SA")]
    [InlineData("pl-PL")]
    public void PersistedIsoDatesStayGregorianUnderEveryCurrentCalendar(string cultureName) {
        var old = CultureInfo.CurrentCulture;
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-iso-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "config.json");
        try {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            File.WriteAllText(path, """
                { "Widgets": [
                  { "Kind": "CalendarHeatmap", "Dates": [ { "Date": "2026-10-12", "Value": 4 } ] },
                  { "Kind": "Timeline", "TimelineItems": [ { "Lane": "Ops", "Label": "Patch", "Start": "2026-10-12T02:00:00", "End": "2026-10-12T03:00:00" } ] }
                ] }
                """);
            var config = BgInfoConfigurationJson.Load(path);
            Assert.Equal(new DateTime(2026, 10, 12), Assert.IsType<BgInfoCalendarHeatmapWidget>(config.Widgets[0]).Items[0].Date);
            Assert.Equal(new DateTime(2026, 10, 12, 2, 0, 0), Assert.IsType<BgInfoTimelineWidget>(config.Widgets[1]).Items[0].Start);
        } finally {
            CultureInfo.CurrentCulture = old;
            Directory.Delete(directory, true);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LastRowRemainsPaintedWhenARegionCompressesNaturalHeight(bool table) {
        BgInfoWidget widget;
        if (table) {
            var value = new BgInfoTableWidget();
            value.Columns.Add("Service");
            for (var i = 0; i < 20; i++) value.Rows.Add(new[] { "Row " + i });
            widget = value;
        } else {
            var value = new BgInfoListWidget();
            for (var i = 0; i < 20; i++) value.Items.Add(new BgInfoListItem { Text = "Row " + i });
            widget = value;
        }
        using var first = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 400, 240);
        if (widget is BgInfoTableWidget grid) grid.Rows[19][0] = "LAST SERVICE";
        else ((BgInfoListWidget)widget).Items[19].Text = "LAST SERVICE";
        using var changed = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 400, 240);
        Assert.False(first.ToRgbaImage().Pixels.SequenceEqual(changed.ToRgbaImage().Pixels));
    }
}
