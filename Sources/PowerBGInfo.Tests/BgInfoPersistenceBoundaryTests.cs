using System.Globalization;

namespace PowerBGInfo.Tests;

public class BgInfoPersistenceBoundaryTests {
    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void CalendarSaveUsesAZoneFreeDayAcrossDateTimeKinds(DateTimeKind kind) {
        var day = new DateTime(2026, 10, 13, 0, 0, 0, kind);
        var calendar = new BgInfoCalendarHeatmapWidget();
        calendar.Items.Add(new BgInfoDatedValue { Date = day, Value = 7 });
        var configuration = new BgInfoConfiguration();
        configuration.Widgets.Add(calendar);
        var path = Path.Combine(Path.GetTempPath(), "bginfo-calendar-" + Path.GetRandomFileName() + ".json");
        try {
            BgInfoConfigurationJson.Save(configuration, path);
            using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
            var date = json.RootElement.GetProperty("Widgets")[0].GetProperty("Dates")[0].GetProperty("Date").GetString();
            Assert.Equal("2026-10-13", date);
            var loaded = Assert.IsType<BgInfoCalendarHeatmapWidget>(BgInfoConfigurationJson.Load(path).Widgets[0]);
            Assert.Equal(day.Date, loaded.Items[0].Date);
            Assert.Equal(DateTimeKind.Unspecified, loaded.Items[0].Date.Kind);
            Assert.Equal(7, loaded.Items[0].Value);
        } finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Theory]
    [InlineData(DateTimeKind.Utc, 2000)]
    [InlineData(DateTimeKind.Local, 5000000)]
    [InlineData(DateTimeKind.Unspecified, 1000)]
    public void TimelineSaveLoadPreservesFractionalBoundsKindsAndNow(DateTimeKind kind, long durationTicks) {
        var start = new DateTime(2026, 10, 13, 8, 0, 0, kind).AddTicks(1234567);
        var timeline = new BgInfoTimelineWidget { Now = start.AddTicks(7654321) };
        timeline.Items.Add(new BgInfoTimelineItem { Lane = "Ops", Label = "Short operation", Start = start, End = start.AddTicks(durationTicks) });
        timeline.Items.Add(new BgInfoTimelineItem { Lane = "Ops", Label = "Open operation", Start = start.AddSeconds(1) });
        var configuration = new BgInfoConfiguration();
        configuration.Widgets.Add(timeline);
        var path = Path.Combine(Path.GetTempPath(), "bginfo-roundtrip-" + Path.GetRandomFileName() + ".json");
        var oldCulture = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
            BgInfoConfigurationJson.Save(configuration, path);
            var loaded = Assert.IsType<BgInfoTimelineWidget>(BgInfoConfigurationJson.Load(path).Widgets[0]);
            Assert.Equal(start.Ticks, loaded.Items[0].Start.Ticks);
            Assert.Equal(kind, loaded.Items[0].Start.Kind);
            Assert.Equal(start.AddTicks(durationTicks).Ticks, loaded.Items[0].End!.Value.Ticks);
            Assert.Equal(kind, loaded.Items[0].End!.Value.Kind);
            Assert.Equal(timeline.Now!.Value.Ticks, loaded.Now!.Value.Ticks);
            Assert.Equal(kind, loaded.Now!.Value.Kind);
            Assert.Null(loaded.Items[1].End);
            loaded.Validate();
            using var image = BgInfoWidgetRenderer.Render(loaded, configuration, 640, 240);
            Assert.Equal(640, image.Width);
        } finally {
            CultureInfo.CurrentCulture = oldCulture;
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
