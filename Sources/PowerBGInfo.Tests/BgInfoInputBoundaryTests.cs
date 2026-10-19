namespace PowerBGInfo.Tests;

public sealed class BgInfoInputBoundaryTests {
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
