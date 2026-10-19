using ChartForgeX.Primitives;
using ChartForgeX.Topology;
using DesktopManager;

namespace PowerBGInfo.Tests;

public sealed class BgInfoHistoryAndOccurrenceTests {
    [Theory]
    [InlineData(BgInfoMetricHistoryKind.Sparkline)]
    [InlineData(BgInfoMetricHistoryKind.Bars)]
    public void SingleSampleHistoryHasAnExplicitSupportedContract(BgInfoMetricHistoryKind kind) {
        var metric = new BgInfoMetricWidget { Label = "QUEUE", Value = "42", History = new[] { 42d }, HistoryKind = kind };
        if (kind == BgInfoMetricHistoryKind.Sparkline) Assert.Throws<ArgumentException>(() => metric.Validate());
        else {
            metric.Validate();
            using var image = BgInfoWidgetRenderer.Render(metric, new BgInfoConfiguration(), 320, 150);
            Assert.Equal(320, image.Width);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void RepeatedChartOccurrencesKeepTheirOwnPersistedHistories(bool stack, bool emptySecond) {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-history-" + Path.GetRandomFileName());
        Directory.CreateDirectory(Path.Combine(directory, "Charts"));
        try {
            var chart = new BgInfoChart { Region = stack ? BgInfoRegion.None : BgInfoRegion.Center, Width = 300, Height = 140 };
            var config = new BgInfoConfiguration {
                Target = BgInfoTarget.File, ConfigurationDirectory = directory, OutputFileName = "repeated.png",
                Background = new BgInfoBackground { Width = 640, Height = 480, Color = ChartColor.FromHex("#101827") },
                ChartLayout = stack ? BgInfoChartLayoutMode.Stack : BgInfoChartLayoutMode.Manual,
                Layout = new BgInfoLayout { MarginX = 12, MarginY = 12, IconAreaWidth = 0, TaskbarReserve = 0, Gap = 12 }
            };
            config.Charts.Add(chart); config.Charts.Add(chart);
            var date = new DateTimeOffset(2026, 10, 19, 0, 0, 0, TimeSpan.Zero);
            ChartHistoryStore.Save(Path.Combine(directory, "Charts", "chart_0.txt"), new[] { new ChartSample(date, 1), new ChartSample(date.AddMinutes(1), 3) });
            ChartHistoryStore.Save(Path.Combine(directory, "Charts", "chart_1.txt"), emptySecond ? Array.Empty<ChartSample>() : new[] { new ChartSample(date, 9), new ChartSample(date.AddMinutes(1), 2) });
            var generator = new BgInfoGenerator(new ImageService(), new FakeWallpaperService());
            var repeated = File.ReadAllBytes(generator.Generate(config));
            var path = Path.Combine(directory, "config.json");
            BgInfoConfigurationJson.Save(config, path);
            var distinct = BgInfoConfigurationJson.Load(path);
            distinct.OutputFileName = "distinct.png";
            Assert.Equal(File.ReadAllBytes(generator.Generate(distinct)), repeated);
        } finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(double.NaN, false)]
    [InlineData(double.PositiveInfinity, false)]
    [InlineData(double.NegativeInfinity, false)]
    [InlineData(double.NaN, true)]
    [InlineData(double.PositiveInfinity, true)]
    [InlineData(double.NegativeInfinity, true)]
    public void MetricValidationRejectsNonfiniteHistoryAndScale(double number, bool maximum) {
        var metric = new BgInfoMetricWidget { History = new[] { 1d, 2d } };
        if (maximum) metric.HistoryMaximum = number;
        else metric.History = new[] { 1d, number };
        Assert.Throws<ArgumentOutOfRangeException>(() => metric.Validate());
    }

    [Fact]
    public void FiniteHistoryRetainsValuesAndRenders() {
        var history = new[] { 0d, 1.25d, 10d };
        var metric = new BgInfoMetricWidget { Label = "QUEUE", Value = "10", History = history, HistoryMaximum = 20 };
        metric.Validate();
        using var image = BgInfoWidgetRenderer.Render(metric, new BgInfoConfiguration(), 320, 150);
        Assert.Equal(history, metric.History);
        Assert.Equal(320, image.Width);
    }

    [Theory]
    [InlineData("Metric")]
    [InlineData("Chart")]
    [InlineData("Topology")]
    [InlineData("Canvas")]
    public void RepeatedPanelInstancesRenderLikeDistinctEquivalentPanels(string kind) {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-occurrence-" + Path.GetRandomFileName());
        try {
            var config = new BgInfoConfiguration {
                Target = BgInfoTarget.File, ConfigurationDirectory = directory, OutputFileName = "repeated.png",
                Background = new BgInfoBackground { Width = 640, Height = 480, Color = ChartColor.FromHex("#101827") },
                Layout = new BgInfoLayout { MarginX = 12, MarginY = 12, IconAreaWidth = 0, TaskbarReserve = 0, Gap = 12 }
            };
            switch (kind) {
                case "Metric":
                    var metric = new BgInfoMetricWidget { Region = BgInfoRegion.Center, Width = 0, Height = 0, Label = "QUEUE", Value = "42" };
                    config.Widgets.Add(metric); config.Widgets.Add(metric); break;
                case "Chart":
                    var chart = new BgInfoChart { Region = BgInfoRegion.Center, Width = 0, Height = 0, Values = new[] { 2d, 5d }, UseHistory = false };
                    config.Charts.Add(chart); config.Charts.Add(chart); break;
                case "Topology":
                    var topology = new BgInfoTopology { Region = BgInfoRegion.Center, Width = 0, Height = 0 };
                    topology.Nodes.Add(new TopologyNode { Id = "queue", Label = "QUEUE" });
                    config.Topologies.Add(topology); config.Topologies.Add(topology); break;
                default:
                    var canvas = new BgInfoVisualCanvas { Region = BgInfoRegion.Center, Width = 0, Height = 180, HeroContentVisible = false, TechBackdrop = false };
                    canvas.Tiles.Add(new BgInfoVisualCanvasTile { Label = "QUEUE", Value = "42" });
                    config.VisualCanvases.Add(canvas); config.VisualCanvases.Add(canvas); break;
            }
            var wallpaper = new FakeWallpaperService();
            var generator = new BgInfoGenerator(new ImageService(), wallpaper);
            var repeated = File.ReadAllBytes(generator.Generate(config));
            var path = Path.Combine(directory, "config.json");
            BgInfoConfigurationJson.Save(config, path);
            var cloned = BgInfoConfigurationJson.Load(path);
            cloned.OutputFileName = "distinct.png";
            var distinct = File.ReadAllBytes(generator.Generate(cloned));
            Assert.Equal(distinct, repeated);
            Assert.Equal(0, wallpaper.Calls);
        } finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LargeWidgetConfigurationRetainsEveryRowThroughSaveLoad(bool list) {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-large-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        try {
            var config = new BgInfoConfiguration();
            var text = new string('x', 2200);
            if (list) {
                var widget = new BgInfoListWidget();
                for (var i = 0; i < 1100; i++) widget.Items.Add(new BgInfoListItem { Text = i + text });
                config.Widgets.Add(widget);
            } else {
                var widget = new BgInfoTableWidget(); widget.Columns.Add("Fact");
                for (var i = 0; i < 1100; i++) widget.Rows.Add(new[] { i + text });
                config.Widgets.Add(widget);
            }
            var path = Path.Combine(directory, "config.json");
            BgInfoConfigurationJson.Save(config, path);
            Assert.True(new FileInfo(path).Length > 2097152);
            var loaded = Assert.Single(BgInfoConfigurationJson.Load(path).Widgets);
            if (list) Assert.Equal("1099" + text, Assert.IsType<BgInfoListWidget>(loaded).Items[1099].Text);
            else Assert.Equal("1099" + text, Assert.IsType<BgInfoTableWidget>(loaded).Rows[1099][0]);
        } finally { Directory.Delete(directory, true); }
    }
}
