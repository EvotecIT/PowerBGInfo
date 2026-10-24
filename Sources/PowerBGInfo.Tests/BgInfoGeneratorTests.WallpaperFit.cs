using System.Reflection;
using ChartForgeX.Composition;
using ChartForgeX.Raster;
using DesktopManager;

namespace PowerBGInfo.Tests;

public partial class BgInfoGeneratorTests {
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScreenCoordinateFillCropsTheSourceWhileStretchRetainsBothEdges(bool portrait) {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-wallpaper-fit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var width = portrait ? 8 : 24;
            var height = portrait ? 24 : 8;
            var pixels = new byte[width * height * 4];
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    // Three equal bands: centered Fill must retain only green; Stretch retains all three.
                    var band = (portrait ? y : x) / 8;
                    var offset = (y * width + x) * 4;
                    pixels[offset + band] = 255;
                    pixels[offset + 3] = 255;
                }
            }
            var sourcePath = Path.Combine(directory, "source.png");
            File.WriteAllBytes(sourcePath, ImageComposition.FromImage(new RgbaImage(width, height, pixels)).ToPng());
            var config = new BgInfoConfiguration {
                FilePath = sourcePath,
                ConfigurationDirectory = directory,
                OutputFileName = "fill.png",
                Target = BgInfoTarget.Wallpaper,
                UseScreenCoordinates = true,
                WallpaperFit = DesktopWallpaperPosition.Fill,
                ForceWallpaperRefresh = false
            };
            var wallpaper = new FakeWallpaperService();
            var generator = new BgInfoGenerator(new ImageService(), wallpaper);
            // The existing production boundary accepts display discovery separately. Exercise its real
            // decode/composition/save workflow with fixed display geometry and no desktop mutation.
            var generate = typeof(BgInfoGenerator).GetMethod("GenerateImage", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var monitors = new BgInfoMonitor[] { new() { Index = 0, Primary = true, Width = 8, Height = 8 } };
            string Render() => (string)generate.Invoke(generator, new object[] {
                config,
                new Func<int, string?>(_ => null),
                new Func<IReadOnlyList<BgInfoMonitor>?>(() => monitors),
                false,
                true
            })!;

            using var fill = BgInfoRasterImage.Load(Render());
            config.WallpaperFit = DesktopWallpaperPosition.Stretch;
            config.OutputFileName = "stretch.png";
            using var stretch = BgInfoRasterImage.Load(Render());
            Assert.Equal(8, fill.Width);
            Assert.Equal(8, fill.Height);
            Assert.Equal(8, stretch.Width);
            Assert.Equal(8, stretch.Height);
            var fillPixels = fill.ToRgbaImage().Pixels;
            for (var offset = 0; offset < fillPixels.Length; offset += 4) {
                Assert.Equal(new byte[] { 0, 255, 0, 255 }, fillPixels.Skip(offset).Take(4).ToArray());
            }
            var stretchPixels = stretch.ToRgbaImage().Pixels;
            Assert.Equal(new byte[] { 255, 0, 0, 255 }, stretchPixels.Take(4).ToArray());
            Assert.Equal(new byte[] { 0, 0, 255, 255 }, stretchPixels.Skip((portrait ? 7 * 8 : 7) * 4).Take(4).ToArray());
            Assert.Equal(0, wallpaper.Calls);
            // Optional test-only retention lets the same verified artifacts be inspected visually.
            var proofDirectory = Environment.GetEnvironmentVariable("POWERBGINFO_FIT_PROOF_DIRECTORY");
            if (!string.IsNullOrWhiteSpace(proofDirectory)) {
                Directory.CreateDirectory(proofDirectory);
                foreach (var file in new[] { "source.png", "fill.png", "stretch.png" }) {
                    File.Copy(Path.Combine(directory, file), Path.Combine(proofDirectory, (portrait ? "portrait-" : "wide-") + file), true);
                }
            }
        } finally {
            Directory.Delete(directory, recursive: true);
        }
    }
}
