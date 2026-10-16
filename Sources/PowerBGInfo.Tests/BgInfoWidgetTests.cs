using ChartForgeX.Primitives;
using ChartForgeX.Raster;
using ChartForgeX.Themes;
using static PowerBGInfo.Tests.BgInfoBackgroundTests;

namespace PowerBGInfo.Tests;

public class BgInfoWidgetTests
{
    private static readonly ChartColor Panel = ChartColor.FromRgb(12, 18, 34);
    private static readonly DateTime Now = new(2026, 10, 16, 10, 30, 0);

    public static IEnumerable<object[]> AllWidgets()
    {
        yield return new object[] { new BgInfoMetricWidget { Label = "CPU", Value = "38", Unit = "%", History = new double[] { 50, 44, 38 }, NumericValue = 38, Threshold = new BgInfoThreshold { Warning = 70 } } };
        var table = new BgInfoTableWidget { StatusColumn = "State" };
        table.Columns.AddRange(new[] { "Service", "State" });
        table.Rows.Add(new[] { "Spooler", "OK" });
        table.Rows.Add(new[] { "W32Time", "Failed" });
        yield return new object[] { table };
        var list = new BgInfoListWidget();
        list.Items.Add(new BgInfoListItem { Text = "Backups verified", Checked = true });
        list.Items.Add(new BgInfoListItem { Text = "DR test", Checked = false, Value = "Oct 30" });
        yield return new object[] { list };
        var calendar = new BgInfoCalendarHeatmapWidget();
        for (var i = 0; i < 60; i++) calendar.Items.Add(new BgInfoDatedValue { Date = Now.AddDays(-i), Value = i % 5 });
        yield return new object[] { calendar };
        var timeline = new BgInfoTimelineWidget { Now = Now };
        timeline.Items.Add(new BgInfoTimelineItem { Lane = "On call", Label = "A. Novak", Start = Now.AddDays(-1), End = Now.AddDays(1), Category = "On call" });
        timeline.Items.Add(new BgInfoTimelineItem { Lane = "SQL-01", Label = "Patch", Start = Now.AddDays(2), End = Now.AddDays(2).AddHours(4), Category = "Maintenance", Status = BgInfoStatus.Warning });
        yield return new object[] { timeline };
        var treemap = new BgInfoTreemapWidget { ValueSuffix = " GB" };
        treemap.Items.Add(new BgInfoTreemapItem { Name = "Databases", Value = 412 });
        treemap.Items.Add(new BgInfoTreemapItem { Name = "Logs", Value = 96, Group = "Ops" });
        yield return new object[] { treemap };
        yield return new object[] { new BgInfoMermaidWidget { Definition = "flowchart LR\n  A[Desk] --> B[On call]" } };
        yield return new object[] { new BgInfoQrCodeWidget { Text = "https://help.contoso.com", Caption = "help.contoso.com" } };
    }

    [Theory]
    [MemberData(nameof(AllWidgets))]
    public void EveryWidgetRendersAtItsSlotSizeInsideItsPanel(BgInfoWidget widget)
    {
        widget.BackgroundColor = Panel;
        widget.Title = "Panel";
        widget.ThemeMode = VisualThemeMode.Dark;

        using var rendered = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 480, 260);
        var pixels = rendered.ToRgbaImage();

        Assert.Equal(480, pixels.Width);
        Assert.Equal(260, pixels.Height);
        AssertNear(Panel, Pixel(pixels, 4, 130));
        Assert.True(CountDistinctFromPanel(pixels) > 40, $"{widget.Kind} must draw content inside the panel.");
    }

    [Fact]
    public void QrCodeDrawsDarkModulesOnALightField()
    {
        var widget = new BgInfoQrCodeWidget { Text = "https://help.contoso.com", BackgroundColor = Panel };

        using var rendered = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 240, 240);
        var pixels = rendered.ToRgbaImage();
        var light = 0;
        var dark = 0;
        for (var y = 40; y < 200; y++)
        {
            for (var x = 40; x < 200; x++)
            {
                var pixel = Pixel(pixels, x, y);
                if (pixel.R > 230 && pixel.G > 230) light++;
                if (pixel.R < 40 && pixel.G < 40 && pixel.B < 60) dark++;
            }
        }

        Assert.True(light > 2000, "The quiet zone and light modules must be white.");
        Assert.True(dark > 2000, "The dark modules must be drawn.");
    }

    [Fact]
    public void MetricStatusFollowsThresholds()
    {
        var metric = new BgInfoMetricWidget { NumericValue = 92, Threshold = new BgInfoThreshold { Warning = 80, Critical = 90 }, Status = BgInfoStatus.Ok };
        Assert.Equal(BgInfoStatus.Critical, metric.ResolveStatus());

        metric.NumericValue = null;
        Assert.Equal(BgInfoStatus.Ok, metric.ResolveStatus());
    }

    [Fact]
    public void MetricThresholdColorsOverrideTheDefaultStatusPalette()
    {
        var colors = new BgInfoStatusColors {
            Ok = ChartColor.FromHex("#112233"),
            Warning = ChartColor.FromHex("#445566"),
            Critical = ChartColor.FromHex("#778899"),
            Info = ChartColor.FromHex("#AABBCC")
        };
        var metric = new BgInfoMetricWidget {
            NumericValue = 95,
            Threshold = new BgInfoThreshold { Warning = 80, Critical = 90, Colors = colors }
        };

        var style = new BgInfoWidgetStyle(metric, new BgInfoConfiguration());

        Assert.Equal(colors.Critical, style.Status.Critical);
        Assert.Equal(colors.Warning, style.Status.Warning);
        Assert.Equal(colors.Ok, style.Status.Ok);
    }

    [Fact]
    public void MetricPanelRendersItsSharedHeaderAboveTheMetricBody()
    {
        var withoutHeader = BgInfoWidgetRenderer.Render(new BgInfoMetricWidget { Label = "CPU", Value = "38", BackgroundColor = Panel }, new BgInfoConfiguration(), 320, 200).ToRgbaImage();
        var withHeader = BgInfoWidgetRenderer.Render(new BgInfoMetricWidget { Label = "CPU", Value = "38", Title = "System status", Note = "Updated just now", BackgroundColor = Panel }, new BgInfoConfiguration(), 320, 200).ToRgbaImage();

        var differences = 0;
        for (var y = 0; y < 70; y++) {
            for (var x = 0; x < 320; x++) {
                if (!Pixel(withoutHeader, x, y).Equals(Pixel(withHeader, x, y))) differences++;
            }
        }
        Assert.True(differences > 100, "Title and note text should be rendered in the metric panel header.");
    }

    [Fact]
    public void InvalidWidgetDataIsReportedBeforeRendering()
    {
        var table = new BgInfoTableWidget();
        table.Columns.Add("Name");
        table.Rows.Add(new[] { "a", "b" });

        Assert.Throws<InvalidOperationException>(table.Validate);
        Assert.Throws<InvalidOperationException>(new BgInfoTimelineWidget().Validate);
        Assert.Throws<InvalidOperationException>(new BgInfoQrCodeWidget().Validate);
        Assert.Throws<InvalidOperationException>(() => BgInfoWidgetRenderer.Render(new BgInfoMermaidWidget { Definition = "notADiagram here" }, new BgInfoConfiguration(), 300, 200));
    }

    [Fact]
    public void ChartThresholdsColorEachRowAndTheLatestValue()
    {
        var chart = new BgInfoChart {
            Kind = BgInfoChartKind.ProgressBar,
            ThemeMode = VisualThemeMode.Dark,
            Threshold = new BgInfoThreshold { Warning = 80, Critical = 90 }
        };
        var defaults = BgInfoStatusColors.Default(VisualThemeMode.Dark);

        var palette = BgInfoChartRenderer.ResolvePalette(chart, new double[] { 40, 85, 95 });

        Assert.Equal(new[] { defaults.Ok, defaults.Warning, defaults.Critical }, palette);
        chart.Palette = new[] { ChartColor.White };
        Assert.Equal(new[] { ChartColor.White }, BgInfoChartRenderer.ResolvePalette(chart, new double[] { 95 }));
        Assert.Equal(defaults.Critical, BgInfoChartRenderer.ResolveThresholdColor(chart, 99));
    }

    [Fact]
    public void ContentScaleRendersAtALowerLogicalSize()
    {
        var widget = new BgInfoListWidget { Scale = 2 };
        var (width, height, output) = BgInfoWidgetRenderer.Density(widget, 400, 200);

        Assert.Equal(200, width);
        Assert.Equal(100, height);
        Assert.Equal(2, output);
        Assert.Throws<ArgumentOutOfRangeException>(() => widget.Scale = 0.5);
    }

    [Fact]
    public void RegionVisualCanvasStacksItsTilesAtTheRegionWidth()
    {
        var visual = new BgInfoVisualCanvas { TileHeight = 100, TileGap = 10, Transparent = true };
        visual.Tiles.Add(new BgInfoVisualCanvasTile { Label = "A", Value = "1", Side = BgInfoVisualCanvasSide.Left });
        visual.Tiles.Add(new BgInfoVisualCanvasTile { Label = "B", Value = "2", Side = BgInfoVisualCanvasSide.Right, Height = 60 });

        Assert.Equal(170, BgInfoVisualCanvasRenderer.MeasureRegionHeight(visual, 1080));
        using var rendered = BgInfoVisualCanvasRenderer.RenderRegion(visual, 300, 170, 1080);
        Assert.Equal(300, rendered.Width);
    }

    private static int CountDistinctFromPanel(RgbaImage pixels)
    {
        var count = 0;
        for (var y = 30; y < pixels.Height - 20; y += 2)
        {
            for (var x = 20; x < pixels.Width - 20; x += 2)
            {
                var pixel = Pixel(pixels, x, y);
                if (Math.Abs(pixel.R - Panel.R) + Math.Abs(pixel.G - Panel.G) + Math.Abs(pixel.B - Panel.B) > 60) count++;
            }
        }
        return count;
    }
}
