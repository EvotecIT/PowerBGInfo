namespace PowerBGInfo.Tests;

public sealed class BgInfoThresholdConfigurationTests {
    [Fact]
    public void TypedThresholdValidationRejectsAnUndefinedDirection() {
        var threshold = new BgInfoThreshold { Critical = 10, Direction = (BgInfoThresholdDirection)99 };
        Assert.Throws<ArgumentOutOfRangeException>(() => threshold.Validate());
    }

    [Theory]
    [InlineData("Status", "Crtical")]
    [InlineData("HistoryKind", "Barrs")]
    [InlineData("Anchor", "BotomRight")]
    public void LoadRejectsUnknownRelatedWidgetEnumNames(string property, string value) {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-widget-enum-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "config.json");
        File.WriteAllText(path, "{\"Widgets\":[{\"Kind\":\"Metric\",\"Value\":\"42\",\"" + property + "\":\"" + value + "\"}]}");
        try { Assert.Contains(value, Assert.Throws<InvalidDataException>(() => BgInfoConfigurationJson.Load(path)).Message); }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("Charts", "Belwo")]
    [InlineData("Widgets", "Belwo")]
    [InlineData("Widgets", "1")]
    public void LoadRejectsUnknownThresholdDirectionInsteadOfChangingItsMeaning(string collection, string direction) {
        WithJson(collection, direction, path => {
            var error = Assert.Throws<InvalidDataException>(() => BgInfoConfigurationJson.Load(path));
            Assert.Contains(direction, error.Message);
            Assert.Contains("Direction", error.Message);
        });
    }

    [Theory]
    [InlineData("Charts", "below", BgInfoThresholdDirection.Below)]
    [InlineData("Widgets", "below", BgInfoThresholdDirection.Below)]
    [InlineData("Widgets", "", BgInfoThresholdDirection.Above)]
    public void LoadRetainsValidThresholdDirectionAndDefault(string collection, string direction, BgInfoThresholdDirection expected) {
        WithJson(collection, direction, path => {
            var config = BgInfoConfigurationJson.Load(path);
            var threshold = collection == "Charts" ? config.Charts[0].Threshold : ((BgInfoMetricWidget)config.Widgets[0]).Threshold;
            Assert.Equal(expected, threshold!.Direction);
            Assert.Equal(expected == BgInfoThresholdDirection.Below ? BgInfoStatus.Critical : BgInfoStatus.Ok, threshold.Evaluate(1));
        });
    }

    private static void WithJson(string collection, string direction, Action<string> assertion) {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-threshold-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "config.json");
        var kind = collection == "Charts" ? "Bar" : "Metric";
        File.WriteAllText(path, "{\"" + collection + "\":[{\"Kind\":\"" + kind + "\",\"Value\":\"42\",\"Values\":[42],\"Threshold\":{\"Critical\":10,\"Direction\":\"" + direction + "\"}}]}");
        try { assertion(path); }
        finally { Directory.Delete(directory, true); }
    }
}
