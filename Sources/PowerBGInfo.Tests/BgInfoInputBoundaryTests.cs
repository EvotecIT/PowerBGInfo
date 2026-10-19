namespace PowerBGInfo.Tests;

public sealed class BgInfoInputBoundaryTests {
    [Theory]
    [InlineData("Ops", "2026-10-12")]
    [InlineData("", "2026-10-14")]
    [InlineData("Ops", "2026-10-13")]
    [InlineData("Ops", "2026-10-13T00:00:00.0000001")]
    public void InvalidPersistedTimelineIntervalsFailDuringLoading(string lane, string end) {
        var json = $"{{\"Widgets\":[{{\"Kind\":\"Timeline\",\"TimelineItems\":[{{\"Lane\":\"{lane}\",\"Start\":\"2026-10-13\",\"End\":\"{end}\"}}]}}]}}";
        WithJson(json, path => Assert.Throws<InvalidDataException>(() => BgInfoConfigurationJson.Load(path)));
    }

    [Theory]
    [InlineData("equal")]
    [InlineData("short")]
    [InlineData("epoch")]
    [InlineData("mixed")]
    public void TimelineModelRejectsUnrenderableOwnerRanges(string kind) {
        var start = new DateTime(2026, 10, 13, 8, 0, 0, DateTimeKind.Utc);
        var item = new BgInfoTimelineItem { Lane = "Ops", Start = start, End = start };
        if (kind == "short") item.End = start.AddTicks(1);
        if (kind == "epoch") { item.Start = new DateTime(1800, 1, 1); item.End = null; }
        if (kind == "mixed") item.End = start.ToLocalTime();
        var widget = new BgInfoTimelineWidget();
        widget.Items.Add(item);
        Assert.Throws<InvalidOperationException>(() => widget.Validate());
    }

    [Fact]
    public void ValidMixedZoneAndOpenTimelineItemsReachTheNativeRenderer() {
        var utc = new DateTime(2026, 10, 13, 8, 0, 0, DateTimeKind.Utc);
        var widget = new BgInfoTimelineWidget { Now = utc.AddHours(4) };
        widget.Items.Add(new BgInfoTimelineItem { Lane = "Ops", Start = utc.ToLocalTime(), End = utc.AddHours(1) });
        widget.Items.Add(new BgInfoTimelineItem { Lane = "Ops", Start = utc.AddHours(2) });
        using var rendered = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 640, 240);
        Assert.Equal(640, rendered.Width);
        Assert.Equal(240, rendered.Height);
        Assert.Contains(rendered.ToRgbaImage().Pixels, value => value != 0);
    }

    [Theory]
    [InlineData("Charts", "")]
    [InlineData("Topologies", "")]
    [InlineData("VisualCanvases", "")]
    [InlineData("Widgets", "\"Kind\":\"Metric\",")]
    public void UnknownRegionCannotTurnAHostedPanelIntoAnUnanchoredPanel(string collection, string fields) {
        WithJson($"{{\"{collection}\":[{{{fields}\"Region\":\"Rigth\"}}]}}", path => {
            var error = Assert.Throws<InvalidDataException>(() => BgInfoConfigurationJson.Load(path));
            Assert.Contains("Rigth", error.Message);
        });
    }

    [Theory]
    [InlineData("Start")]
    [InlineData("End")]
    [InlineData("Now")]
    public void InvalidPersistedTimelineDatesCannotBecomeOpenRanges(string property) {
        var json = property == "Now"
            ? "{\"Widgets\":[{\"Kind\":\"Timeline\",\"Now\":\"N/A\"}]}"
            : "{\"Widgets\":[{\"Kind\":\"Timeline\",\"TimelineItems\":[{\"Lane\":\"Ops\",\"Start\":\""
                + (property == "Start" ? "N/A" : "2026-10-12") + "\",\"End\":\""
                + (property == "End" ? "N/A" : "2026-10-13") + "\"}]}]}";
        WithJson(json, path => Assert.Throws<InvalidDataException>(() => BgInfoConfigurationJson.Load(path)));
    }

    private static void WithJson(string json, Action<string> check) {
        var folder = Path.Combine(Path.GetTempPath(), "bginfo-input-boundary-" + Path.GetRandomFileName());
        Directory.CreateDirectory(folder);
        try {
            var path = Path.Combine(folder, "config.json");
            File.WriteAllText(path, json);
            check(path);
        } finally { Directory.Delete(folder, true); }
    }
}
