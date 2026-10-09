using System.Text;
using ChartForgeX.Primitives;
using ChartForgeX.Raster;
using ChartForgeX.Topology;
using ChartForgeX.Typography;
using ChartForgeX.Themes;
using DesktopManager;

namespace PowerBGInfo.Tests;

public partial class BgInfoGeneratorTests
{
    [Theory]
    [InlineData(null, null, null)]
    [InlineData("Calibri", "Consolas", null)]
    [InlineData("Arial", "Consolas", "Calibri")]
    public void SlideshowCompositionPreservesInheritedAndExplicitChartFonts(string? labelFont, string? valueFont, string? chartFont)
    {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-slideshow-fonts-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        try {
            var sourcePath = Path.Combine(directory, "source.png");
            using (var source = new BgInfoRasterImage()) {
                source.Create(sourcePath, 380, 220, ChartColors.Black);
                source.Save(sourcePath);
            }
            var chart = new BgInfoChart {
                Title = "Memory working set",
                Kind = BgInfoChartKind.Bar,
                ThemeMode = VisualThemeMode.Dark,
                FontFamilyName = chartFont,
                Values = new[] { 42d, 65d },
                ShowDataLabels = true,
                UseHistory = false,
                Width = 320,
                Height = 180,
                PositionX = 20,
                PositionY = 20
            };
            var configuration = new BgInfoConfiguration {
                ConfigurationDirectory = directory,
                OutputFileName = "slideshow.png",
                Target = BgInfoTarget.Wallpaper,
                UseScreenCoordinates = false
            };
            if (labelFont != null) configuration.FontFamilyName = labelFont;
            if (valueFont != null) configuration.ValueFontFamilyName = valueFont;
            configuration.Charts.Add(chart);
            using var expected = BgInfoRasterImage.Load(sourcePath);
            using (var overlay = BgInfoChartRenderer.Render(chart, chart.Values, configuration)) {
                expected.DrawImage(overlay, 20, 20);
            }
            var wallpaper = new FakeWallpaperService {
                Slideshow = new DesktopWallpaperSlideshow {
                    ImagePaths = new[] { sourcePath },
                    State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow
                }
            };
            var outputPath = new BgInfoGenerator(new ImageService(), wallpaper).Generate(configuration);
            using var actual = BgInfoRasterImage.Load(outputPath);
            Assert.Equal(1, wallpaper.SlideshowCalls);
            Assert.Equal(outputPath, Assert.Single(wallpaper.SlideshowPaths));
            Assert.Equal(expected.ToRgbaImage().Pixels, actual.ToRgbaImage().Pixels);
        } finally {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(".png")]
    [InlineData(".jpg")]
    [InlineData(".gif")]
    public void GenerateFileComposesSyntheticWallpaperLayersAndPreservesItsCanvas(string extension)
    {
        const int width = 1200;
        const int height = 630;
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-wallpaper-" + Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        try
        {
            var sourcePath = Path.Combine(directory, "source.png");
            using (var source = new BgInfoRasterImage())
            {
                source.Create(sourcePath, width, height, ChartColors.Navy);
                source.Save(sourcePath);
            }

            var configuration = new BgInfoConfiguration {
                FilePath = sourcePath,
                ConfigurationDirectory = directory,
                OutputFileName = "wallpaper" + extension,
                Target = BgInfoTarget.File,
                UseScreenCoordinates = false,
                Color = ChartColors.White,
                ValueColor = ChartColors.Cyan,
                FontFamilyName = "Consolas",
                ValueFontFamilyName = "Consolas",
                FontSize = 20,
                ValueFontSize = 20,
                PositionX = 20,
                PositionY = 20
            };
            configuration.Entries.Add(new BgInfoEntry {
                Type = BgInfoEntryType.Value,
                Name = "Machine",
                Value = "LAB-01",
                Bold = true,
                ValueUnderlineStyle = TextDecorationStyle.Double
            });
            configuration.Charts.Add(new BgInfoChart {
                Title = "CPU",
                Kind = BgInfoChartKind.Line,
                Values = new[] { 10d, 40d, 25d, 65d, 42d },
                UseHistory = false,
                Width = 240,
                Height = 120,
                Anchor = BgInfoTextPosition.TopRight,
                OffsetX = 20,
                OffsetY = 20,
                LineColor = ChartColors.Cyan,
                TitleColor = ChartColors.White,
                ValueColor = ChartColors.White
            });
            var visual = new BgInfoVisualCanvas {
                Title = "Synthetic Lab",
                Subtitle = "File export fixture",
                Transparent = true,
                TileWidth = 220,
                TileHeight = 90,
                RightTileOffsetY = 160
            };
            visual.Tiles.Add(new BgInfoVisualCanvasTile {
                Side = BgInfoVisualCanvasSide.Left,
                Label = "Memory",
                Value = "64 GB",
                Detail = "Synthetic machine values",
                IconKind = BgInfoVisualCanvasTileIconKind.Memory,
                SurfaceStyle = BgInfoVisualCanvasTileSurfaceStyle.Raised,
                MiniChartKind = BgInfoVisualCanvasTileMiniChartKind.Area,
                MiniChartValues = new[] { 18d, 30d, 24d, 40d, 35d },
                MiniChartMaximum = 64
            });
            visual.Tiles.Add(new BgInfoVisualCanvasTile {
                Side = BgInfoVisualCanvasSide.Right,
                Label = "Storage",
                Value = "42%",
                Detail = "Static fixture",
                IconKind = BgInfoVisualCanvasTileIconKind.Storage,
                SurfaceStyle = BgInfoVisualCanvasTileSurfaceStyle.Outline,
                Progress = .42,
                MiniChartKind = BgInfoVisualCanvasTileMiniChartKind.Bars,
                MiniChartValues = new[] { 25d, 35d, 42d },
                MiniChartMaximum = 100
            });
            configuration.VisualCanvases.Add(visual);
            var topology = new BgInfoTopology {
                Title = "Lab topology",
                Width = 560,
                Height = 310,
                Anchor = BgInfoTextPosition.BottomLeft,
                OffsetX = 24,
                OffsetY = 24,
                Transparent = true
            };
            topology.Groups.Add(new TopologyGroup { Id = "lab", Label = "Lab", Status = TopologyHealthStatus.Healthy });
            topology.Nodes.Add(new TopologyNode {
                Id = "gateway", Label = "Gateway", GroupId = "lab",
                Kind = TopologyNodeKind.Network, Status = TopologyHealthStatus.Healthy
            });
            topology.Nodes.Add(new TopologyNode {
                Id = "api", Label = "API", GroupId = "lab",
                Kind = TopologyNodeKind.Service, Status = TopologyHealthStatus.Warning
            });
            topology.Edges.Add(new TopologyEdge {
                Id = "gateway-api", SourceNodeId = "gateway", TargetNodeId = "api",
                Label = "HTTPS", Kind = TopologyEdgeKind.Connectivity, Status = TopologyHealthStatus.Healthy
            });
            configuration.Topologies.Add(topology);
            var wallpaperService = new FakeWallpaperService();
            var generator = new BgInfoGenerator(new ImageService(), wallpaperService);

            var outputPath = generator.Generate(configuration);

            Assert.Equal(Path.Combine(directory, "wallpaper" + extension), outputPath);
            Assert.True(File.Exists(outputPath));
            if (extension == ".gif")
            {
                Assert.Equal("GIF89a", Encoding.ASCII.GetString(File.ReadAllBytes(outputPath), 0, 6));
            }
            using var decoded = BgInfoRasterImage.Load(outputPath);
            var pixels = decoded.ToRgbaImage();
            Assert.Equal(width, pixels.Width);
            Assert.Equal(height, pixels.Height);
            Assert.True(CountBrightWallpaperPixels(pixels, 10, 10, 500, 60) > 20); // Styled label/value.
            Assert.True(CountBrightWallpaperPixels(pixels, 940, 20, 240, 120) > 20); // Standalone chart.
            Assert.True(CountBrightWallpaperPixels(pixels, 40, 90, 260, 110) > 20); // Raised tile with mini chart.
            Assert.True(CountBrightWallpaperPixels(pixels, 500, 140, 200, 80) > 20); // Floating hero badge.
            Assert.True(CountBrightWallpaperPixels(pixels, 40, 400, 500, 170) > 20); // Topology nodes and routes.
            Assert.Equal(0, wallpaperService.Calls);
            Assert.Equal(0, wallpaperService.LogonCalls);
            Assert.Equal(0, wallpaperService.AllUsersCalls);
            Assert.Equal(0, wallpaperService.SlideshowCalls);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static int CountBrightWallpaperPixels(RgbaImage image, int left, int top, int width, int height)
    {
        var count = 0;
        for (var y = top; y < top + height; y++)
        {
            for (var x = left; x < left + width; x++)
            {
                var offset = (y * image.Width + x) * 4;
                if (image.Pixels[offset] + image.Pixels[offset + 1] + image.Pixels[offset + 2] > 180) count++;
            }
        }
        return count;
    }
}
