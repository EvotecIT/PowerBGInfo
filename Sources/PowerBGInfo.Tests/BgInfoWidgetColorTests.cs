using System.Globalization;
using ChartForgeX.Primitives;
using ChartForgeX.Raster;
using ChartForgeX.Themes;
using static PowerBGInfo.Tests.BgInfoBackgroundTests;

namespace PowerBGInfo.Tests;

/// <summary>Treemap color values, timeline axis captions, status-block palettes and column overflow.</summary>
public class BgInfoWidgetColorTests
{
    private static readonly ChartColor Panel = ChartColor.FromRgb(12, 18, 34);

    private static BgInfoWidgetStyle Style(BgInfoWidget widget) => new(widget, new BgInfoConfiguration());

    [Theory]
    [InlineData(10, "ok")]
    [InlineData(60, "warning")]
    [InlineData(84.9, "warning")]
    [InlineData(85, "critical")]
    public void TreemapThresholdsAboveMapColorValuesToStatusBands(double value, string band)
    {
        var widget = new BgInfoTreemapWidget { Threshold = new BgInfoThreshold { Warning = 60, Critical = 85 } };
        var style = Style(widget);
        var scale = BgInfoWidgetRenderer.CreateTreemapColorScale(widget, style);
        var color = scale.ColorFor(value);

        if (band == "warning") Assert.Equal(style.Status.Warning, color);
        else if (band == "critical") Assert.Equal(style.Status.Critical, color);
        else
        {
            // Healthy rectangles are a muted version of the ok color, not the full status color.
            Assert.NotEqual(style.Status.Ok, color);
            Assert.True(color.G > color.R, "The healthy band keeps a green hue.");
        }
    }

    [Theory]
    [InlineData(5, "critical")]
    [InlineData(10, "critical")]
    [InlineData(10.5, "warning")]
    [InlineData(30, "warning")]
    [InlineData(31, "ok")]
    public void TreemapThresholdsBelowPutValuesAtTheThresholdInTheWorseBand(double value, string band)
    {
        var widget = new BgInfoTreemapWidget {
            Threshold = new BgInfoThreshold { Warning = 30, Critical = 10, Direction = BgInfoThresholdDirection.Below }
        };
        var style = Style(widget);
        var color = BgInfoWidgetRenderer.CreateTreemapColorScale(widget, style).ColorFor(value);

        if (band == "critical") Assert.Equal(style.Status.Critical, color);
        else if (band == "warning") Assert.Equal(style.Status.Warning, color);
        else Assert.NotEqual(style.Status.Warning, color);
    }

    [Fact]
    public void TreemapWithOnlyACriticalThresholdHasTwoBands()
    {
        var widget = new BgInfoTreemapWidget { Threshold = new BgInfoThreshold { Critical = 90 } };
        var style = Style(widget);
        var scale = BgInfoWidgetRenderer.CreateTreemapColorScale(widget, style);

        Assert.Equal(style.Status.Critical, scale.ColorFor(95));
        Assert.NotEqual(style.Status.Critical, scale.ColorFor(50));
    }

    [Fact]
    public void TreemapColorValuesPaintTheWarningRectangle()
    {
        var widget = new BgInfoTreemapWidget {
            BackgroundColor = Panel, ThemeMode = VisualThemeMode.Dark,
            Threshold = new BgInfoThreshold { Warning = 60, Critical = 85 }
        };
        widget.Items.Add(new BgInfoTreemapItem { Name = "SQL", Value = 128, ColorValue = 71 });
        widget.Items.Add(new BgInfoTreemapItem { Name = "WEB", Value = 16, ColorValue = 9 });
        var warning = Style(widget).Status.Warning;

        using var rendered = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 480, 260);
        var pixels = rendered.ToRgbaImage();

        Assert.True(Count(pixels, warning) > 2000, "The large rectangle above the warning threshold must take the warning color.");
    }

    [Fact]
    public void TreemapRejectsColorValuesThatAreNotFinite()
    {
        var widget = new BgInfoTreemapWidget();
        widget.Items.Add(new BgInfoTreemapItem { Name = "A", Value = 1, ColorValue = double.NaN });

        Assert.Throws<InvalidOperationException>(() => widget.Validate());
    }

    [Fact]
    public void TimelineTicksReadAsDaysOrTimesInsteadOfIsoDates()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var friday = new DateTime(2026, 10, 16).ToOADate();
            Assert.Equal("Fri 16", BgInfoWidgetRenderer.FormatTimelineTick(friday, multiDay: true));
            Assert.Equal("Fri 12:00", BgInfoWidgetRenderer.FormatTimelineTick(friday + 0.5, multiDay: true));
            // Floating-point ticks just short of the hour still read as the hour.
            Assert.Equal("14:00", BgInfoWidgetRenderer.FormatTimelineTick(friday + 14 / 24d - 1e-9, multiDay: false));
            Assert.Equal(string.Empty, BgInfoWidgetRenderer.FormatTimelineTick(double.NaN, multiDay: false));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void StatusBlocksUseTheInfoColorOnlyWhenTheyShowAnInfoStatus()
    {
        var widget = new BgInfoListWidget { Accent = ChartColor.FromHex("#E11D48"), ThemeMode = VisualThemeMode.Dark };
        var style = Style(widget);

        Assert.Equal(new[] { style.Status.Info }, style.StatusBlockPalette(showsInfo: true));
        Assert.Equal(new[] { style.Accent }, style.StatusBlockPalette(showsInfo: false));
    }

    [Fact]
    public void InfoListItemsTakeTheInfoColorUnderARedAccent()
    {
        var widget = new BgInfoListWidget {
            BackgroundColor = Panel, ThemeMode = VisualThemeMode.Dark, Accent = ChartColor.FromHex("#E11D48")
        };
        widget.Items.Add(new BgInfoListItem { Text = "INC-1 Disk alert", Status = BgInfoStatus.Info, Value = "P3" });
        widget.Items.Add(new BgInfoListItem { Text = "INC-2 Badge reader", Status = BgInfoStatus.Info, Value = "P4" });
        var style = Style(widget);

        using var rendered = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 420, 160);
        var pixels = rendered.ToRgbaImage();

        Assert.True(Count(pixels, style.Status.Info) > 20, "Info markers must use the info color.");
        Assert.True(Count(pixels, widget.Accent!.Value) < 5, "Info markers must not use the red accent.");
    }

    [Fact]
    public void ColumnStacksTallerThanTheColumnShrinkToStayInsideTheSafeArea()
    {
        var layout = new BgInfoLayout();
        var safe = BgInfoLayoutEngine.GetSafeArea(1920, 1080, layout);
        var slots = BgInfoLayoutEngine.Resolve(1920, 1080, layout, null, new[] {
            new BgInfoLayoutRequest(BgInfoRegion.Right, 0, 500),
            new BgInfoLayoutRequest(BgInfoRegion.Right, 0, 400),
            new BgInfoLayoutRequest(BgInfoRegion.Right, 0, 300)
        });

        var last = slots[2];
        Assert.True(last.Y + last.Height <= safe.Y + safe.Height + 0.5, "The last panel must end inside the safe area.");
        Assert.True(slots[0].Height > slots[1].Height && slots[1].Height > slots[2].Height, "Panels keep their proportions.");
        Assert.True(slots[1].Y >= slots[0].Y + slots[0].Height, "Panels must not overlap.");
    }

    [Fact]
    public void TreemapColorValuesAndThresholdsRoundTripThroughJson()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-treemap-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "config.json");
        var config = new BgInfoConfiguration { ConfigurationDirectory = directory, Target = BgInfoTarget.File };
        var treemap = new BgInfoTreemapWidget { Threshold = new BgInfoThreshold { Warning = 60, Critical = 85 } };
        treemap.Items.Add(new BgInfoTreemapItem { Name = "SQL", Value = 128, Group = "Databases", ColorValue = 71 });
        treemap.Items.Add(new BgInfoTreemapItem { Name = "WEB", Value = 16 });
        config.Widgets.Add(treemap);

        try
        {
            BgInfoConfigurationJson.Save(config, path);
            var loaded = (BgInfoTreemapWidget)BgInfoConfigurationJson.Load(path).Widgets[0];

            Assert.Equal(71, loaded.Items[0].ColorValue);
            Assert.Null(loaded.Items[1].ColorValue);
            Assert.Equal(60, loaded.Threshold!.Warning);
            Assert.Equal(85, loaded.Threshold.Critical);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static int Count(RgbaImage pixels, ChartColor color)
    {
        var count = 0;
        for (var y = 0; y < pixels.Height; y++)
        {
            for (var x = 0; x < pixels.Width; x++)
            {
                var pixel = Pixel(pixels, x, y);
                if (Math.Abs(pixel.R - color.R) + Math.Abs(pixel.G - color.G) + Math.Abs(pixel.B - color.B) < 24) count++;
            }
        }
        return count;
    }
}
