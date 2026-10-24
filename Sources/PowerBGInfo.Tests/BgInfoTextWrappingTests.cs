using ChartForgeX.Primitives;
using ChartForgeX.Raster;
using ChartForgeX.Typography;

namespace PowerBGInfo.Tests;

public sealed class BgInfoTextWrappingTests {
    [Theory]
    [InlineData(0)]
    [InlineData(70)]
    public void ExplicitEmptyAndWhitespaceParagraphsKeepTheirLineSlots(double width) {
        using var image = new BgInfoRasterImage();
        var style = TextStyle.Create(12, ChartColor.White);
        var lines = BgInfoGenerator.WrapTextLines(image, "One\r\n\r  \nTwo\n", width, style);

        Assert.Equal(5, lines.Count);
        Assert.Equal("One", lines[0]);
        Assert.Equal(string.Empty, lines[1]);
        Assert.Equal(width > 0 ? string.Empty : "  ", lines[2]);
        Assert.Equal("Two", lines[3]);
        Assert.Equal(string.Empty, lines[4]);
    }

    [Fact]
    public void NarrowLinesRetainWholeSurrogateCombiningAndJoinedEmojiClusters() {
        using var image = new BgInfoRasterImage();
        var style = TextStyle.Create(24, ChartColor.White);
        const string accented = "A\u0301";
        const string rocket = "\U0001F680";
        const string developer = "\U0001F469\u200D\U0001F4BB";
        var lines = BgInfoGenerator.WrapTextLines(image, accented + rocket + developer, 0.01, style);

        Assert.Equal(new[] { accented, rocket, developer }, lines);
        Assert.Equal(accented + rocket + developer, string.Concat(lines));
    }

    [Fact]
    public void WrappingKeepsRawTextForTheStyledPainterWithoutChangingStyle() {
        using var image = new BgInfoRasterImage();
        var style = TextStyle.Create(14, ChartColor.White);
        style.TextCase = TextCaseTransform.ToggleCase;
        var lines = BgInfoGenerator.WrapTextLines(image, "Service\tSTATE", 1000, style);

        Assert.Equal(new[] { "Service STATE" }, lines);
        Assert.Equal(TextCaseTransform.ToggleCase, style.TextCase);
    }

    [Fact]
    public void EntryPipelineAppliesToggleCaseOnceAcrossWrappedLines() {
        var directory = Path.Combine(Path.GetTempPath(), "powerbginfo-wrapped-case-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var generator = new BgInfoGenerator(new ImageService(), new WallpaperService());
            byte[] Render(string name, string text, TextCaseTransform textCase) {
                var configuration = new BgInfoConfiguration {
                    Target = BgInfoTarget.File,
                    ConfigurationDirectory = directory,
                    OutputFileName = name,
                    Background = new BgInfoBackground { Width = 320, Height = 180, Color = ChartColor.Black },
                    TextPosition = BgInfoTextPosition.TopLeft,
                    ValueWrapWidth = 90,
                    ValueFontSize = 20,
                    ValueColor = ChartColor.White,
                    ValueTextCase = textCase
                };
                configuration.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Value, Name = "State", Value = text });
                return RasterImageDecoder.Read(generator.Generate(configuration)).Pixels;
            }

            var actual = Render("styled.png", "Service STATE", TextCaseTransform.ToggleCase);
            var expected = Render("prepared.png", "sERVICE state", TextCaseTransform.None);
            Assert.Equal(expected, actual);
            Assert.False(actual.SequenceEqual(Render("original.png", "Service STATE", TextCaseTransform.None)));
        } finally {
            Directory.Delete(directory, recursive: true);
        }
    }
}
