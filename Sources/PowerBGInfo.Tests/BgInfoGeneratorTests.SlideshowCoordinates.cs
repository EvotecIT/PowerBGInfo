using ChartForgeX.Primitives;
using DesktopManager;

namespace PowerBGInfo.Tests;

public partial class BgInfoGeneratorTests {
    [Fact]
    public void SlideshowScreenCoordinatesMatchTheSingleWallpaperRender() {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-slide-coordinates-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var sourcePath = Path.Combine(directory, "source.png");
            using (var image = new BgInfoRasterImage()) {
                image.Create(sourcePath, 640, 360, ChartColors.Navy);
                image.Save(sourcePath);
            }
            var config = new BgInfoConfiguration {
                FilePath = sourcePath, ConfigurationDirectory = directory, OutputFileName = "single.png",
                Target = BgInfoTarget.Wallpaper, UseScreenCoordinates = true,
                WallpaperFit = DesktopWallpaperPosition.Fill, ForceWallpaperRefresh = false,
                SpaceX = 32, SpaceY = 40, FontSize = 28, Color = ChartColors.White
            };
            config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Label, Name = "Same desktop size and padding" });
            var wallpaper = new FakeWallpaperService();
            var generator = new BgInfoGenerator(new ImageService(), wallpaper);
            var single = generator.Generate(config);
            config.FilePath = string.Empty;
            config.OutputFileName = "slides.png";
            wallpaper.Slideshow = new DesktopWallpaperSlideshow {
                ImagePaths = new[] { sourcePath }, State = DesktopSlideshowState.Enabled | DesktopSlideshowState.Slideshow
            };
            var slide = generator.Generate(config);
            using var expected = BgInfoRasterImage.Load(single);
            using var actual = BgInfoRasterImage.Load(slide);
            Assert.Equal(expected.Width, actual.Width);
            Assert.Equal(expected.Height, actual.Height);
            Assert.Equal(expected.ToRgbaImage().Pixels, actual.ToRgbaImage().Pixels);
            Assert.Equal(1, wallpaper.SlideshowCalls);
        } finally {
            Directory.Delete(directory, recursive: true);
        }
    }
}
