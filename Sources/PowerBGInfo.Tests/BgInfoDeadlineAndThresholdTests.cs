using ChartForgeX.Primitives;

namespace PowerBGInfo.Tests;

public sealed class BgInfoDeadlineAndThresholdTests {
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void InvalidThresholdExportPreservesTheExistingUsableFile(bool chart, bool emptyThreshold) {
        var path = Path.Combine(Path.GetTempPath(), "bginfo-threshold-save-" + Path.GetRandomFileName() + ".json");
        var threshold = new BgInfoThreshold { Warning = 80, Critical = 90 };
        var config = new BgInfoConfiguration();
        if (chart) config.Charts.Add(new BgInfoChart { Threshold = threshold, Values = new[] { 85d } });
        else config.Widgets.Add(new BgInfoMetricWidget { Threshold = threshold, NumericValue = 85, Value = "85" });
        try {
            BgInfoConfigurationJson.Save(config, path);
            var before = File.ReadAllBytes(path);
            if (emptyThreshold) { threshold.Warning = null; threshold.Critical = null; threshold.Direction = (BgInfoThresholdDirection)99; }
            else threshold.Critical = 70;
            Assert.ThrowsAny<ArgumentException>(() => BgInfoConfigurationJson.Save(config, path));
            Assert.Equal(before, File.ReadAllBytes(path));
            var loaded = BgInfoConfigurationJson.Load(path);
            Assert.Equal(90, chart ? loaded.Charts[0].Threshold!.Critical : Assert.IsType<BgInfoMetricWidget>(loaded.Widgets[0]).Threshold!.Critical);
        } finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingMetricValuesDoNotBypassThresholdStatusValidation(bool emptyThreshold) {
        var threshold = emptyThreshold
            ? new BgInfoThreshold { Direction = (BgInfoThresholdDirection)99 }
            : new BgInfoThreshold { Warning = 80, Critical = 70 };
        var metric = new BgInfoMetricWidget { Status = BgInfoStatus.Ok, Threshold = threshold };
        Assert.ThrowsAny<ArgumentException>(() => metric.ResolveStatus());
        threshold.Direction = BgInfoThresholdDirection.Above;
        threshold.Warning = 80; threshold.Critical = 90;
        Assert.Equal(BgInfoStatus.Ok, metric.ResolveStatus());
    }

    [Theory]
    [InlineData("Charts")]
    [InlineData("Widgets")]
    public void EmptyThresholdInputStillRejectsUnknownDirection(string collection) {
        var path = Path.Combine(Path.GetTempPath(), "bginfo-empty-threshold-" + Path.GetRandomFileName() + ".json");
        var kind = collection == "Charts" ? "Bar" : "Metric";
        File.WriteAllText(path, "{\"" + collection + "\":[{\"Kind\":\"" + kind + "\",\"Threshold\":{\"Direction\":\"Belwo\"}}]}");
        try { Assert.Throws<InvalidDataException>(() => BgInfoConfigurationJson.Load(path)); }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(BgInfoThresholdDirection.Above, "Validate")]
    [InlineData(BgInfoThresholdDirection.Above, "Metric")]
    [InlineData(BgInfoThresholdDirection.Above, "Chart")]
    [InlineData(BgInfoThresholdDirection.Above, "Evaluate")]
    [InlineData(BgInfoThresholdDirection.Below, "Validate")]
    [InlineData(BgInfoThresholdDirection.Below, "Metric")]
    [InlineData(BgInfoThresholdDirection.Below, "Chart")]
    [InlineData(BgInfoThresholdDirection.Below, "Evaluate")]
    public void MutatedNestedThresholdsAreRejectedByEveryConsumer(BgInfoThresholdDirection direction, string consumer) {
        var threshold = new BgInfoThreshold { Warning = 80, Critical = 90, Direction = direction };
        if (direction == BgInfoThresholdDirection.Above) threshold.Warning = 100;
        var metric = new BgInfoMetricWidget { Label = "CPU", Value = "85", NumericValue = 85, Threshold = threshold };
        var chart = new BgInfoChart { Threshold = threshold, LineColor = ChartColor.White, Palette = new[] { ChartColor.White } };
        Assert.Throws<ArgumentException>(() => {
            switch (consumer) {
                case "Validate": metric.Validate(); break;
                case "Metric": using (BgInfoWidgetRenderer.Render(metric, new BgInfoConfiguration(), 320, 150)) { } break;
                case "Chart": using (BgInfoChartRenderer.Render(chart, new[] { 85d }, new BgInfoConfiguration())) { } break;
                default: threshold.Evaluate(85); break;
            }
        });
    }

    [Fact]
    public void MissingMetricValueDoesNotHideAnInvalidNestedThreshold() {
        var metric = new BgInfoMetricWidget { Threshold = new BgInfoThreshold { Warning = double.NaN } };
        Assert.Throws<ArgumentOutOfRangeException>(() => metric.Validate());
    }

    [Fact]
    public void ExhaustedWmiBudgetDoesNotStartWorkAndLeavesCapacityAvailable() {
        var executor = new SystemInfoQueryExecutor();
        var calls = 0;
        Assert.Null(executor.Run(() => { Interlocked.Increment(ref calls); return "late"; }, TimeSpan.Zero));
        Assert.Equal("recovered", executor.Run(() => { Interlocked.Increment(ref calls); return "recovered"; }, TimeSpan.FromSeconds(2)));
        Assert.Equal(1, Volatile.Read(ref calls));
    }
}
