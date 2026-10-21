namespace PowerBGInfo.Tests;

public sealed class WallpaperDisplayMappingTests {
    [Fact]
    public void NamedDisplayRequestsPreserveTheirIdentity() {
        var displays = new Dictionary<int, string> { [0] = "display-a", [2] = "display-c" };

        Assert.Equal("display-c", WallpaperService.ResolveDisplayId(2, true, displays));
        Assert.Throws<ArgumentOutOfRangeException>(() => WallpaperService.ResolveDisplayId(1, true, displays));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DesktopWithoutDisplayGeometryAcceptsOnlyItsDefaultIndex(bool supportsPerDisplay) {
        var displays = new Dictionary<int, string>();

        Assert.Null(WallpaperService.ResolveDisplayId(0, supportsPerDisplay, displays));
        Assert.Throws<PlatformNotSupportedException>(() => WallpaperService.ResolveDisplayId(1, supportsPerDisplay, displays));
        Assert.Throws<PlatformNotSupportedException>(() => WallpaperService.ResolveDisplayId(-1, supportsPerDisplay, displays));
    }
}
