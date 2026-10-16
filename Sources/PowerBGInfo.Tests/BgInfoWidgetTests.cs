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

    [Fact]
    public void RegionVisualCanvasHonorsPerTileTextScale()
    {
        var baseline = RenderRegionTypography(scale: 1);
        foreach (var scale in new[] { 1.25, 1.5 })
        {
            var scaled = RenderRegionTypography(scale);
            Assert.InRange(scaled.Height / (double)baseline.Height,
                scale - 0.01, scale + 0.01);
            var baselineGlyphs = MeasureBrightGlyphRows(baseline.Image);
            var scaledGlyphs = MeasureBrightGlyphRows(scaled.Image);
            Assert.Equal(3, baselineGlyphs.Count);
            Assert.Equal(3, scaledGlyphs.Count);
            for (var index = 0; index < 3; index++)
            {
                Assert.True(scaledGlyphs[index] > baselineGlyphs[index],
                    $"Glyph row {index} did not grow at scale {scale}: " +
                    $"{baselineGlyphs[index]}px to {scaledGlyphs[index]}px.");
                Assert.InRange(Math.Abs(scaledGlyphs[index] - baselineGlyphs[index] * scale),
                    0, 2);
            }
        }
    }

    [Fact]
    public void CanvasDefaultTileScaleGrowsMeasuredGlyphAndTileHeight()
    {
        var baseline = RenderRegionTypography(scale: 1, useCanvasScale: true);
        foreach (var scale in new[] { 1.25, 1.5 })
        {
            var scaled = RenderRegionTypography(scale, useCanvasScale: true);
            Assert.InRange(scaled.Height / (double)baseline.Height,
                scale - 0.01, scale + 0.01);
            var baselineGlyphs = MeasureBrightGlyphRows(baseline.Image);
            var scaledGlyphs = MeasureBrightGlyphRows(scaled.Image);
            Assert.Equal(3, baselineGlyphs.Count);
            Assert.Equal(3, scaledGlyphs.Count);
            for (var index = 0; index < 3; index++)
            {
                Assert.True(scaledGlyphs[index] > baselineGlyphs[index]);
                Assert.InRange(Math.Abs(scaledGlyphs[index] - baselineGlyphs[index] * scale),
                    0, 2);
            }
        }
    }

    [Fact]
    public void RegionVisualCanvasHonorsPerTileValueFontSize()
    {
        var defaultPixels = RenderRegionTypography(scale: 1).Image.Pixels;
        var sizedPixels = RenderRegionTypography(scale: 1, valueFontSize: 30).Image.Pixels;

        Assert.False(defaultPixels.SequenceEqual(sizedPixels));
    }

    private static (RgbaImage Image, int Height) RenderRegionTypography(
        double scale, double? valueFontSize = null, bool useCanvasScale = false)
    {
        var visual = new BgInfoVisualCanvas {
            Transparent = true,
            TileTextScale = useCanvasScale ? scale : 1
        };
        visual.Tiles.Add(new BgInfoVisualCanvasTile {
            Label = "HOSTNAME",
            Value = "WORKSTATION-01",
            Detail = "Windows desktop",
            TextScale = useCanvasScale ? null : scale,
            ValueFontSize = valueFontSize
        });

        var height = BgInfoVisualCanvasRenderer.MeasureRegionHeight(visual, 1080);
        using var rendered = BgInfoVisualCanvasRenderer.RenderRegion(visual, 480, height, 1080);
        return (rendered.ToRgbaImage(), height);
    }

    private static List<int> MeasureBrightGlyphRows(RgbaImage image)
    {
        var runs = new List<(int Start, int End)>();
        var firstInkRow = -1;
        var lastInkRow = -1;
        for (var y = 0; y < image.Height; y++)
        {
            var pixels = 0;
            for (var x = 50; x < image.Width - 16; x++)
            {
                var index = (y * image.Width + x) * 4;
                if (image.Pixels[index + 3] > 0 &&
                    image.Pixels[index] > 170 &&
                    image.Pixels[index + 1] > 170 &&
                    image.Pixels[index + 2] > 170)
                {
                    pixels++;
                }
            }

            if (pixels >= 2)
            {
                if (firstInkRow < 0) firstInkRow = y;
                lastInkRow = y;
            }
            else if (firstInkRow >= 0 && y - lastInkRow > 3)
            {
                runs.Add((firstInkRow, lastInkRow));
                firstInkRow = -1;
                lastInkRow = -1;
            }
        }

        if (firstInkRow >= 0) runs.Add((firstInkRow, lastInkRow));
        return runs.Select(run => run.End - run.Start + 1).ToList();
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
