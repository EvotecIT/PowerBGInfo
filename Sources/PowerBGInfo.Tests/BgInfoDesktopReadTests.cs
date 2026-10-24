using ChartForgeX.Primitives;
using DesktopManager;
using System.Reflection;

namespace PowerBGInfo.Tests;

public sealed class BgInfoDesktopReadTests {
    [Theory]
    [InlineData(DesktopOperationStatus.Unsupported)]
    [InlineData(DesktopOperationStatus.Unavailable)]
    [InlineData(DesktopOperationStatus.Failed)]
    public void UnsuccessfulDesktopReadPreservesTypedStatusAndNativeDiagnostic(DesktopOperationStatus status) {
        var state = CreateWallpaperReadState(status, "The desktop retained a hidden image; the selected mode cannot be read faithfully.");

        var error = Assert.Throws<BgInfoWallpaperReadException>(() => WallpaperService.RequireSuccessfulRead(state));

        Assert.Equal(status, error.Status);
        Assert.Equal(state.Message, error.Diagnostic);
        Assert.Contains(state.Message, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("/fixture/wallpaper.png")]
    public void SuccessfulDesktopReadRetainsTheExactSolidOrImageState(string? path) {
        var state = CreateWallpaperReadState(DesktopOperationStatus.Success, "Native wallpaper state.", path);

        Assert.Same(state, WallpaperService.RequireSuccessfulRead(state));
    }

    [Fact]
    public void WallpaperTargetResolutionRetainsKdeScopeWithoutPhysicalDisplayGeometry() {
        var target = new DesktopWallpaperTarget();
        typeof(DesktopWallpaperTarget).GetProperty(nameof(DesktopWallpaperTarget.Index))!.SetValue(target, 2);
        typeof(DesktopWallpaperTarget).GetProperty(nameof(DesktopWallpaperTarget.DisplayId))!.SetValue(target, "screen:4");
        typeof(DesktopWallpaperTarget).GetProperty(nameof(DesktopWallpaperTarget.ScopeId))!.SetValue(target, "activity-one");

        Assert.Null(target.Display);
        Assert.Same(target, WallpaperService.ResolveWallpaperTarget(2, new[] { target }));
        Assert.Throws<ArgumentOutOfRangeException>(() => WallpaperService.ResolveWallpaperTarget(0, new[] { target }));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TargetRecheckRetainsAnUnchangedScopeWithKnownOrUnknownGeometry(bool knownGeometry) {
        WallpaperService.RequireUnchangedWallpaperTarget(
            CreateWallpaperTarget("screen:4", "activity-one", knownGeometry ? CreateWallpaperDisplay() : null),
            CreateWallpaperTarget("screen:4", "activity-one", knownGeometry ? CreateWallpaperDisplay() : null));
    }

    [Theory]
    [InlineData("screen:5", "activity-one")]
    [InlineData("screen:4", "activity-two")]
    public void TargetRecheckRejectsAChangedDisplayOrActivity(string displayId, string scopeId) {
        var selected = CreateWallpaperTarget("screen:4", "activity-one");
        var current = CreateWallpaperTarget(displayId, scopeId);

        Assert.Throws<InvalidOperationException>(() => WallpaperService.RequireUnchangedWallpaperTarget(selected, current));
    }

    [Theory]
    [InlineData(nameof(DesktopDisplayInfo.Width), 1600)]
    [InlineData(nameof(DesktopDisplayInfo.Height), 1920)]
    [InlineData(nameof(DesktopDisplayInfo.Left), 1920)]
    [InlineData(nameof(DesktopDisplayInfo.Scale), 2d)]
    public void TargetRecheckRejectsChangedQualifiedDisplayGeometry(string property, object value) {
        var selected = CreateWallpaperTarget("display-one", null, CreateWallpaperDisplay());
        var changedDisplay = CreateWallpaperDisplay();
        typeof(DesktopDisplayInfo).GetProperty(property)!.SetValue(changedDisplay, value);
        var current = CreateWallpaperTarget("display-one", null, changedDisplay);

        Assert.Throws<InvalidOperationException>(() => WallpaperService.RequireUnchangedWallpaperTarget(selected, current));
    }

    [Fact]
    public void TargetRecheckDoesNotTreatUnknownGeometryAsTheEarlierQualifiedGeometry() {
        var selected = CreateWallpaperTarget("display-one", null, CreateWallpaperDisplay());
        var current = CreateWallpaperTarget("display-one", null);

        Assert.Throws<InvalidOperationException>(() => WallpaperService.RequireUnchangedWallpaperTarget(selected, current));
        Assert.Throws<InvalidOperationException>(() => WallpaperService.RequireUnchangedWallpaperTarget(current, selected));
    }

    [Fact]
    public void CurrentWallpaperReadFailurePreservesTheOriginalDiagnostic() {
        var failure = new InvalidOperationException("The native wallpaper source could not be read.");
        var config = new BgInfoConfiguration();

        var actual = Assert.Throws<InvalidOperationException>(() =>
            BgInfoGenerator.ResolveBaseImagePath(config, _ => throw failure));

        Assert.Same(failure, actual);
    }

    [Fact]
    public void UnsupportedCurrentWallpaperDoesNotBecomeAPlainBackground() {
        var directory = Path.Combine(Path.GetTempPath(), "bginfo-read-failure-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try {
            var source = Path.Combine(directory, "native-source.heic");
            File.WriteAllBytes(source, new byte[] { 0, 0, 0, 24, 102, 116, 121, 112, 104, 101, 105, 99 });
            var config = new BgInfoConfiguration {
                ConfigurationDirectory = directory,
                OutputFileName = "output.png",
                Target = BgInfoTarget.File,
                UseScreenCoordinates = false,
                BackgroundColor = ChartColor.Black
            };
            var wallpaper = new FakeWallpaperService();
            var generator = new BgInfoGenerator(new ImageService(), wallpaper) { CurrentWallpaper = _ => source };

            var error = Assert.Throws<NotSupportedException>(() => generator.Generate(config));

            Assert.Contains("unsupported", error.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(Path.Combine(directory, "output.png")));
            Assert.Equal(0, wallpaper.Calls);
        } finally {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static DesktopWallpaperState CreateWallpaperReadState(DesktopOperationStatus status, string message, string? path = null) =>
        (DesktopWallpaperState)typeof(DesktopWallpaperState)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single()
            .Invoke(new object?[] { status, message, path, null, null, null });

    private static DesktopWallpaperTarget CreateWallpaperTarget(string displayId, string? scopeId, DesktopDisplayInfo? display = null) {
        var target = new DesktopWallpaperTarget();
        typeof(DesktopWallpaperTarget).GetProperty(nameof(DesktopWallpaperTarget.DisplayId))!.SetValue(target, displayId);
        typeof(DesktopWallpaperTarget).GetProperty(nameof(DesktopWallpaperTarget.ScopeId))!.SetValue(target, scopeId);
        typeof(DesktopWallpaperTarget).GetProperty(nameof(DesktopWallpaperTarget.Display))!.SetValue(target, display);
        return target;
    }

    private static DesktopDisplayInfo CreateWallpaperDisplay() {
        var display = new DesktopDisplayInfo();
        typeof(DesktopDisplayInfo).GetProperty(nameof(DesktopDisplayInfo.Id))!.SetValue(display, "display-one");
        typeof(DesktopDisplayInfo).GetProperty(nameof(DesktopDisplayInfo.Width))!.SetValue(display, 1920);
        typeof(DesktopDisplayInfo).GetProperty(nameof(DesktopDisplayInfo.Height))!.SetValue(display, 1080);
        typeof(DesktopDisplayInfo).GetProperty(nameof(DesktopDisplayInfo.Scale))!.SetValue(display, 1d);
        return display;
    }
}
