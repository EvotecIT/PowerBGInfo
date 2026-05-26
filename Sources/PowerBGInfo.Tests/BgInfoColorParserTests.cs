namespace PowerBGInfo.Tests;

public class BgInfoColorParserTests {
    [Fact]
    public void TryParseAcceptsBareRgbHex() {
        Assert.True(BgInfoColorParser.TryParse("ffffff", out var color));

        Assert.Equal(255, color.A);
        Assert.Equal(255, color.R);
        Assert.Equal(255, color.G);
        Assert.Equal(255, color.B);
    }

    [Fact]
    public void TryParseUsesChartForgeXRgbaHexOrder() {
        Assert.True(BgInfoColorParser.TryParse("#FFFFFF80", out var color));

        Assert.Equal(128, color.A);
        Assert.Equal(255, color.R);
        Assert.Equal(255, color.G);
        Assert.Equal(255, color.B);
    }

    [Fact]
    public void TryParseAcceptsChartForgeXColorTokens() {
        Assert.True(BgInfoColorParser.TryParse("Emerald400", out var color));

        Assert.Equal(255, color.A);
        Assert.Equal(52, color.R);
        Assert.Equal(211, color.G);
        Assert.Equal(153, color.B);
    }

    [Fact]
    public void TryParseAcceptsChartForgeXNamedColors() {
        Assert.True(BgInfoColorParser.TryParse("White", out var color));

        Assert.Equal(255, color.A);
        Assert.Equal(255, color.R);
        Assert.Equal(255, color.G);
        Assert.Equal(255, color.B);
    }

    [Fact]
    public void TryParseAcceptsShortChartForgeXHex() {
        Assert.True(BgInfoColorParser.TryParse("#0F08", out var color));

        Assert.Equal(136, color.A);
        Assert.Equal(0, color.R);
        Assert.Equal(255, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void TryParseRejectsUnknownColorNames() {
        Assert.False(BgInfoColorParser.TryParse("definitely-not-a-color", out _));
    }
}
