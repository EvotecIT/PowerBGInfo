using System.Collections.Generic;
using ChartForgeX.Primitives;
using ChartForgeX.Themes;

namespace PowerBGInfo.Tests;

public class BgInfoThemeAndLayoutTests
{
    [Theory]
    [InlineData(BgInfoThemePreset.Graphite)]
    [InlineData(BgInfoThemePreset.Midnight)]
    [InlineData(BgInfoThemePreset.Aurora)]
    [InlineData(BgInfoThemePreset.Ember)]
    [InlineData(BgInfoThemePreset.Orchid)]
    [InlineData(BgInfoThemePreset.Corporate)]
    public void EveryPresetHasReadableInkInBothModes(BgInfoThemePreset preset)
    {
        var dark = BgInfoTheme.Create(preset, VisualThemeMode.Dark);
        var light = BgInfoTheme.Create(preset, VisualThemeMode.Light);

        Assert.True(Luminance(dark.Ink) - Luminance(dark.BackdropColor) > 0.6, "Dark ink must stand out from the dark backdrop.");
        Assert.True(Luminance(light.BackdropColor) - Luminance(light.Ink) > 0.6, "Light ink must stand out from the light backdrop.");
        Assert.NotEqual(dark.Accent, light.Accent);
        Assert.Equal(preset, dark.Preset);
    }

    [Fact]
    public void ThemeAccentOverrideAndScaleApply()
    {
        var accent = ChartColor.FromHex("#FF00AA");
        var theme = BgInfoTheme.Create(BgInfoThemePreset.Aurora, VisualThemeMode.Dark, accent, 2);

        Assert.Equal(accent, theme.Accent);
        Assert.Equal(34f, theme.FontSize);
        Assert.Equal(128, theme.MarginX);
        Assert.Equal(36, theme.CornerRadius);
        Assert.Throws<ArgumentOutOfRangeException>(() => BgInfoTheme.Create(BgInfoThemePreset.Aurora, scale: 9));
    }

    [Fact]
    public void ThemeBackdropCarriesGradientPatternAndGlows()
    {
        var theme = BgInfoTheme.Create(BgInfoThemePreset.Orchid, VisualThemeMode.Dark);
        var background = theme.CreateBackground(800, 600);

        Assert.Equal(800, background.Width);
        Assert.Equal(theme.BackdropGradientStart, background.GradientStart);
        Assert.Equal(BgInfoBackgroundPattern.Dots, background.Pattern);
        Assert.Equal(2, background.Glows.Count);
        Assert.NotSame(theme.BackdropGlows[0], background.Glows[0]);
    }

    [Theory]
    [InlineData(80, BgInfoThresholdDirection.Above, BgInfoStatus.Warning)]
    [InlineData(95, BgInfoThresholdDirection.Above, BgInfoStatus.Critical)]
    [InlineData(20, BgInfoThresholdDirection.Above, BgInfoStatus.Ok)]
    public void ThresholdsAboveTurnHighValuesWorse(double value, BgInfoThresholdDirection direction, BgInfoStatus expected)
    {
        var threshold = new BgInfoThreshold { Warning = 80, Critical = 90, Direction = direction };
        Assert.Equal(expected, threshold.Evaluate(value));
    }

    [Fact]
    public void ThresholdsBelowTurnLowValuesWorseAndValidateOrder()
    {
        var threshold = new BgInfoThreshold { Warning = 20, Critical = 10, Direction = BgInfoThresholdDirection.Below };
        threshold.Validate();

        Assert.Equal(BgInfoStatus.Critical, threshold.Evaluate(5));
        Assert.Equal(BgInfoStatus.Warning, threshold.Evaluate(20));
        Assert.Equal(BgInfoStatus.Ok, threshold.Evaluate(50));
        Assert.Throws<ArgumentException>(() => new BgInfoThreshold { Warning = 10, Critical = 20, Direction = BgInfoThresholdDirection.Below }.Validate());
        Assert.Null(new BgInfoThreshold().ResolveColor(50, VisualThemeMode.Dark));
    }

    [Fact]
    public void ThresholdColorsComeFromTheGivenStatusColors()
    {
        var colors = BgInfoTheme.Create(BgInfoThemePreset.Midnight, VisualThemeMode.Light).StatusColors;
        var threshold = new BgInfoThreshold { Warning = 70, Critical = 90, Colors = colors };

        Assert.Equal(colors.Critical, threshold.ResolveColor(91, VisualThemeMode.Dark));
        Assert.Equal(colors.Ok, threshold.ResolveColor(10, VisualThemeMode.Dark));
    }

    [Fact]
    public void SafeAreaKeepsTheTaskbarAndIconAreaFree()
    {
        var safe = BgInfoLayoutEngine.GetSafeArea(1920, 1080, new BgInfoLayout { MarginX = 64, MarginY = 56, TaskbarReserve = 48, IconAreaWidth = 100 });

        Assert.Equal(164, safe.X);
        Assert.Equal(56, safe.Y);
        Assert.Equal(1920 - 164 - 64, safe.Width);
        // The bottom margin is at least the taskbar plus 16 pixels.
        Assert.Equal(1080 - 56 - 64, safe.Height);
    }

    [Fact]
    public void SafeAreaClampsItsOriginAndSizeInsideTheImageWhenReservesConsumeIt()
    {
        var safe = BgInfoLayoutEngine.GetSafeArea(640, 480, new BgInfoLayout {
            MarginX = int.MaxValue, MarginY = int.MaxValue, TaskbarReserve = int.MaxValue, IconAreaWidth = int.MaxValue
        });

        Assert.Equal(639, safe.X);
        Assert.Equal(479, safe.Y);
        Assert.Equal(1, safe.Width);
        Assert.Equal(1, safe.Height);
        Assert.True(safe.X + safe.Width <= 640);
        Assert.True(safe.Y + safe.Height <= 480);
    }

    [Fact]
    public void TaskbarReserveIsEstimatedFromTheImageHeight()
    {
        var hd = BgInfoLayoutEngine.GetSafeArea(1920, 1080, new BgInfoLayout { MarginY = 0 });
        var qhd = BgInfoLayoutEngine.GetSafeArea(2560, 1440, new BgInfoLayout { MarginY = 0 });

        Assert.Equal(1080 - 48 - 16, hd.Height);
        Assert.Equal(1440 - 64 - 16, qhd.Height);
    }

    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(2560, 1440)]
    [InlineData(3440, 1440)]
    [InlineData(1080, 1920)]
    public void RegionsNeverOverlapAndStayInsideTheSafeArea(int width, int height)
    {
        var layout = new BgInfoLayout();
        var requests = new[] {
            new BgInfoLayoutRequest(BgInfoRegion.Top, 0, 0),
            new BgInfoLayoutRequest(BgInfoRegion.Left, 0, 0),
            new BgInfoLayoutRequest(BgInfoRegion.Left, 0, 200),
            new BgInfoLayoutRequest(BgInfoRegion.Right, 0, 0),
            new BgInfoLayoutRequest(BgInfoRegion.Center, 0, 0),
            new BgInfoLayoutRequest(BgInfoRegion.Bottom, 0, 0),
            new BgInfoLayoutRequest(BgInfoRegion.Bottom, 400, 0)
        };

        var slots = BgInfoLayoutEngine.Resolve(width, height, layout, null, requests);
        var safe = BgInfoLayoutEngine.GetSafeArea(width, height, layout);

        foreach (var slot in slots)
        {
            Assert.True(slot.X >= safe.X - 1 && slot.Y >= safe.Y - 1, $"{slot} starts outside the safe area {safe}.");
            Assert.True(slot.X + slot.Width <= safe.X + safe.Width + 1 && slot.Y + slot.Height <= safe.Y + safe.Height + 1, $"{slot} ends outside the safe area {safe}.");
        }
        for (var i = 0; i < slots.Length; i++)
        {
            for (var j = i + 1; j < slots.Length; j++)
            {
                Assert.False(Intersects(slots[i], slots[j]), $"Slots {i} {slots[i]} and {j} {slots[j]} overlap.");
            }
        }
        Assert.Equal(400, slots[6].Width);
        Assert.Equal(200, slots[2].Height);
    }

    [Fact]
    public void TopAndBottomBandsShareTheHeightLeftAfterTheirGaps()
    {
        var layout = new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0, Gap = 24 };
        var safe = BgInfoLayoutEngine.GetSafeArea(800, 800, layout);
        var slots = BgInfoLayoutEngine.Resolve(800, 800, layout, null, new[] {
            new BgInfoLayoutRequest(BgInfoRegion.Top, 0, 500),
            new BgInfoLayoutRequest(BgInfoRegion.Bottom, 0, 500)
        });

        Assert.True(slots[0].Y + slots[0].Height <= slots[1].Y);
        Assert.Equal(24, slots[1].Y - (slots[0].Y + slots[0].Height));
        Assert.Equal(safe.Y + safe.Height, slots[1].Y + slots[1].Height);
    }

    [Fact]
    public void ColumnsStartBelowTheTextBlockTheyShare()
    {
        var text = new ChartRect(64, 56, 500, 300);
        var slots = BgInfoLayoutEngine.Resolve(1920, 1080, new BgInfoLayout(), text, new[] {
            new BgInfoLayoutRequest(BgInfoRegion.Left, 0, 120),
            new BgInfoLayoutRequest(BgInfoRegion.Right, 0, 120)
        });

        Assert.Equal(356 + 24, slots[0].Y);
        Assert.Equal(56, slots[1].Y);
    }

    [Fact]
    public void BandsFlowBesideATextBlockInTheirRow()
    {
        var text = new ChartRect(64, 56, 500, 120);
        var slots = BgInfoLayoutEngine.Resolve(1920, 1080, new BgInfoLayout(), text, new[] {
            new BgInfoLayoutRequest(BgInfoRegion.Top, 0, 150)
        });

        Assert.True(slots[0].X >= 564, "The top band must start after the text block.");
    }

    [Fact]
    public void ShareKeepsFixedSizesAndSplitsTheRest()
    {
        var sizes = BgInfoLayoutEngine.Share(new double[] { 100, 0, 0 }, 520, 10);

        Assert.Equal(100, sizes[0]);
        Assert.Equal(200, sizes[1]);
        Assert.Equal(200, sizes[2]);
    }

    [Fact]
    public void RoundedSharedEdgesKeepFractionalRowSlotsDisjoint()
    {
        var slots = BgInfoLayoutEngine.Resolve(101, 40,
            new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0, BandHeight = 10, Gap = 0 }, null, new[] {
                new BgInfoLayoutRequest(BgInfoRegion.Top, 0, 10),
                new BgInfoLayoutRequest(BgInfoRegion.Top, 0, 10),
                new BgInfoLayoutRequest(BgInfoRegion.Top, 0, 10)
            });

        Assert.Equal(0, slots[0].X);
        Assert.Equal(101, slots[2].X + slots[2].Width);
        Assert.True(slots[0].X + slots[0].Width <= slots[1].X);
        Assert.True(slots[1].X + slots[1].Width <= slots[2].X);
    }

    [Fact]
    public void ShareScalesFixedPanelsWhenTheirCombinedSizeExceedsTheRegion()
    {
        var sizes = BgInfoLayoutEngine.Share(new double[] { 500, 500 }, 800, 24);

        Assert.Equal(388, sizes[0]);
        Assert.Equal(388, sizes[1]);
        Assert.Equal(800, sizes.Sum() + 24);
    }

    [Fact]
    public void ShareReducesGapsWhenMinimumPanelsWouldOtherwiseOverflow()
    {
        var sizes = BgInfoLayoutEngine.Share(new double[] { 4, 4, 4 }, 10, 4);

        Assert.Equal(new double[] { 1, 1, 1 }, sizes);
        Assert.Equal(10, sizes.Sum() + 2 * 3.5);
    }

    [Fact]
    public void TopBandReducesGapsWhenMinimumPanelsWouldOtherwiseOverflow()
    {
        var slots = BgInfoLayoutEngine.Resolve(10, 10,
            new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0, BandHeight = 1, Gap = 4 }, null, new[] {
                new BgInfoLayoutRequest(BgInfoRegion.Top, 4, 1),
                new BgInfoLayoutRequest(BgInfoRegion.Top, 4, 1),
                new BgInfoLayoutRequest(BgInfoRegion.Top, 4, 1)
            });

        Assert.All(slots, slot => Assert.True(slot.Width >= 1 && slot.X >= 0 && slot.X + slot.Width <= 10));
        Assert.Equal(10, slots[^1].X + slots[^1].Width);
    }

    [Fact]
    public void CenterColumnRetainsRoomWhenBothSideColumnsArePresent()
    {
        var slots = BgInfoLayoutEngine.Resolve(640, 480, new BgInfoLayout { MarginX = 64, MarginY = 24, Gap = 24 }, null, new[] {
            new BgInfoLayoutRequest(BgInfoRegion.Left, 0, 100),
            new BgInfoLayoutRequest(BgInfoRegion.Center, 0, 100),
            new BgInfoLayoutRequest(BgInfoRegion.Right, 0, 100)
        });

        Assert.True(slots[1].X >= slots[0].X + slots[0].Width + 24);
        Assert.True(slots[1].X + slots[1].Width + 24 <= slots[2].X);
        Assert.True(slots[1].Width >= 120);
    }

    [Fact]
    public void LargeConfiguredGapIsFittedInsideSafeWidthForThreeColumns()
    {
        var layout = new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0, IconAreaWidth = 0, Gap = 1024 };
        var requests = new[] {
            new BgInfoLayoutRequest(BgInfoRegion.Left, 0, 100),
            new BgInfoLayoutRequest(BgInfoRegion.Center, 0, 100),
            new BgInfoLayoutRequest(BgInfoRegion.Right, 0, 100)
        };

        var slots = BgInfoLayoutEngine.Resolve(640, 480, layout, null, requests);
        var safe = BgInfoLayoutEngine.GetSafeArea(640, 480, layout);

        Assert.All(slots, slot => Assert.True(slot.X >= safe.X && slot.X + slot.Width <= safe.X + safe.Width, $"{slot} must remain inside {safe}."));
        Assert.True(slots[0].X + slots[0].Width <= slots[1].X);
        Assert.True(slots[1].X + slots[1].Width <= slots[2].X);
        Assert.True(slots[1].Width > 0);
    }

    [Fact]
    public void EmptyRegionChartsDoNotReserveSpaceForOtherPanels()
    {
        var chart = new BgInfoChart { Region = BgInfoRegion.Right, Values = System.Array.Empty<double>(), UseHistory = false };
        var metric = new BgInfoMetricWidget { Region = BgInfoRegion.Right, Label = "CPU", Value = "Unknown" };
        var config = new BgInfoConfiguration();
        config.Charts.Add(chart);
        config.Widgets.Add(metric);
        var values = new Dictionary<BgInfoChart, IReadOnlyList<double>> { [chart] = System.Array.Empty<double>() };

        var placements = BgInfoGenerator.ResolveRegionPlacements(1280, 720, config, default, values);

        Assert.False(placements.TryGet(chart, out _));
        Assert.True(placements.TryGet(metric, out _));
    }

    [Fact]
    public void EmptyRegionTopologiesDoNotReserveSpaceForOtherPanels()
    {
        var empty = new BgInfoTopology { Region = BgInfoRegion.Right, Width = 900, Height = 400 };
        var metric = new BgInfoMetricWidget { Region = BgInfoRegion.Right, Label = "CPU", Value = "Unknown" };
        var withEmpty = new BgInfoConfiguration();
        withEmpty.Topologies.Add(empty);
        withEmpty.Widgets.Add(metric);
        var withoutEmpty = new BgInfoConfiguration();
        var expectedMetric = new BgInfoMetricWidget { Region = BgInfoRegion.Right, Label = "CPU", Value = "Unknown" };
        withoutEmpty.Widgets.Add(expectedMetric);

        var placements = BgInfoGenerator.ResolveRegionPlacements(1280, 720, withEmpty, default);
        var expected = BgInfoGenerator.ResolveRegionPlacements(1280, 720, withoutEmpty, default);

        Assert.False(placements.TryGet(empty, out _));
        Assert.True(placements.TryGet(metric, out var actualSlot));
        Assert.True(expected.TryGet(expectedMetric, out var expectedSlot));
        Assert.Equal(expectedSlot, actualSlot);
    }

    [Fact]
    public void TilelessRegionVisualCanvasDoesNotReserveSpace()
    {
        var empty = new BgInfoVisualCanvas { Region = BgInfoRegion.Right, Height = 400 };
        var metric = new BgInfoMetricWidget { Region = BgInfoRegion.Right, Label = "CPU", Value = "Unknown" };
        var config = new BgInfoConfiguration();
        config.VisualCanvases.Add(empty);
        config.Widgets.Add(metric);

        var placements = BgInfoGenerator.ResolveRegionPlacements(1280, 720, config, default);

        Assert.False(placements.TryGet(empty, out _));
        Assert.True(placements.TryGet(metric, out _));
    }

    [Fact]
    public void ColumnSlotsAreOmittedWhenReservedTextConsumesTheColumn()
    {
        var layout = new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0, IconAreaWidth = 0, Gap = 24 };
        var slots = BgInfoLayoutEngine.Resolve(640, 480, layout, new ChartRect(0, 0, 300, 480), new[] {
            new BgInfoLayoutRequest(BgInfoRegion.Left, 300, 100)
        });

        Assert.Equal(0, slots[0].Width);
        Assert.Equal(0, slots[0].Height);
    }

    [Fact]
    public void OmittedColumnSlotsDoNotBecomeRegionPlacements()
    {
        var visual = new BgInfoVisualCanvas { Region = BgInfoRegion.Right };
        visual.Tiles.Add(new BgInfoVisualCanvasTile { Label = "Health", Value = "OK" });
        var metric = new BgInfoMetricWidget { Region = BgInfoRegion.Left, Label = "CPU", Value = "Unknown" };
        var config = new BgInfoConfiguration { Layout = new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0, IconAreaWidth = 0 } };
        config.VisualCanvases.Add(visual);
        config.Widgets.Add(metric);

        var placements = BgInfoGenerator.ResolveRegionPlacements(640, 480, config, new ChartRect(0, 0, 640, 480));

        Assert.False(placements.TryGet(visual, out _));
        Assert.False(placements.TryGet(metric, out _));
    }

    [Fact]
    public void BandsAreOmittedWhenReservedTextCoversTheirFullWidth()
    {
        var layout = new BgInfoLayout { MarginX = 0, MarginY = 0, TaskbarReserve = 0, IconAreaWidth = 0, Gap = 24 };
        var slots = BgInfoLayoutEngine.Resolve(640, 480, layout, new ChartRect(0, 0, 640, 480), new[] {
            new BgInfoLayoutRequest(BgInfoRegion.Top, 200, 80),
            new BgInfoLayoutRequest(BgInfoRegion.Bottom, 200, 80)
        });

        Assert.All(slots, slot => {
            Assert.Equal(0, slot.Width);
            Assert.Equal(0, slot.Height);
        });

        var exhaustedBands = BgInfoLayoutEngine.Resolve(640, 1, layout, null, new[] {
            new BgInfoLayoutRequest(BgInfoRegion.Top, 200, 80),
            new BgInfoLayoutRequest(BgInfoRegion.Bottom, 200, 80)
        });
        Assert.All(exhaustedBands, slot => Assert.Equal(0, slot.Height));
    }

    [Fact]
    public void MachineDomainPrefersComputerMembershipOverCurrentUserDomain()
    {
        Assert.Equal("machine.example", SystemInfoProvider.ResolveMachineDomain("machine.example"));
        Assert.Equal(SystemInfoProvider.Unknown, SystemInfoProvider.ResolveMachineDomain(null));
    }

    [Fact]
    public void VisualCanvasUsesItsExplicitRegionHeight()
    {
        var visual = new BgInfoVisualCanvas { Region = BgInfoRegion.Right, Width = 280, Height = 80, TileHeight = 100 };
        visual.Tiles.Add(new BgInfoVisualCanvasTile { Label = "Health", Value = "OK" });
        var config = new BgInfoConfiguration();
        config.VisualCanvases.Add(visual);

        var placements = BgInfoGenerator.ResolveRegionPlacements(1280, 720, config, default);

        Assert.True(placements.TryGet(visual, out var slot));
        Assert.Equal(80, slot.Height);
    }

    [Fact]
    public void GeneratorDrawsRegionChartsInTheirSlot()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-region-" + Path.GetRandomFileName());
        var fill = ChartColor.FromRgb(200, 30, 30);
        var config = new BgInfoConfiguration {
            ConfigurationDirectory = directory,
            OutputFileName = "regions.png",
            Target = BgInfoTarget.File,
            Background = new BgInfoBackground { Width = 1280, Height = 720, Color = ChartColor.Black },
            Layout = new BgInfoLayout { MarginX = 40, MarginY = 40, TaskbarReserve = 0 }
        };
        config.Charts.Add(new BgInfoChart {
            Kind = BgInfoChartKind.Bar, Values = new double[] { 1, 2 }, UseHistory = false, ShowLatestValue = false,
            Region = BgInfoRegion.Right, Width = 0, Height = 0, BackgroundColor = fill
        });

        try
        {
            var path = new BgInfoGenerator(new ImageService(), new FakeWallpaperService()).Generate(config);
            using var output = BgInfoRasterImage.Load(path);
            var pixels = output.ToRgbaImage();

            // Right column: 24 % of 1280 clamped to 340, from x = 900 to 1240. An unsized panel
            // takes at most 36 % of the image height (259 px), so it ends at y = 299.
            BgInfoBackgroundTests.AssertNear(fill, BgInfoBackgroundTests.Pixel(pixels, 905, 45));
            BgInfoBackgroundTests.AssertNear(fill, BgInfoBackgroundTests.Pixel(pixels, 1235, 290));
            BgInfoBackgroundTests.AssertNear(ChartColor.Black, BgInfoBackgroundTests.Pixel(pixels, 1235, 320));
            BgInfoBackgroundTests.AssertNear(ChartColor.Black, BgInfoBackgroundTests.Pixel(pixels, 880, 200));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private static bool Intersects(ChartRect a, ChartRect b) =>
        a.X < b.X + b.Width && b.X < a.X + a.Width && a.Y < b.Y + b.Height && b.Y < a.Y + a.Height;

    private static double Luminance(ChartColor color) => (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255d;
}
