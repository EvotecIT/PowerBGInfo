namespace PowerBGInfo.Tests;

public class BgInfoMonitorTests
{
    [Fact]
    public void ProviderUsesCanonicalWallpaperIndicesAndDoesNotInventUnknownGeometry()
    {
        var selected = BgInfoDesktopDefaultsTests.QualifiedTarget(targetIndex: 0, physicalIndex: 2);
        var unknown = BgInfoDesktopDefaultsTests.QualifiedTarget(targetIndex: 1, hasGeometry: false);
        var other = BgInfoDesktopDefaultsTests.QualifiedTarget(targetIndex: 2, physicalIndex: 4);

        var monitors = BgInfoMonitorProvider.FromWallpaperTargets(new[] { other, unknown, selected });

        Assert.Equal(new[] { 0, 2 }, monitors.Select(monitor => monitor.Index));
        Assert.Equal(2, selected.Display!.Index);
        Assert.Equal(160, monitors[0].Width);
        Assert.Equal(120, monitors[0].Height);
        Assert.Equal(selected.Display.Id, monitors[0].Id);
    }

    [Fact]
    public void CreateTurnsDesktopBoundsIntoSize()
    {
        var monitor = BgInfoMonitorProvider.Create(1, @"\\.\DISPLAY2", primary: false, left: -2560, top: 0, right: 0, bottom: 1440, scale: 1.254);

        Assert.Equal(1, monitor.Index);
        Assert.Equal(@"\\.\DISPLAY2", monitor.DeviceName);
        Assert.False(monitor.Primary);
        Assert.Equal(-2560, monitor.Left);
        Assert.Equal(0, monitor.Top);
        Assert.Equal(2560, monitor.Width);
        Assert.Equal(1440, monitor.Height);
        Assert.Equal(1.25, monitor.Scale);
    }

    [Fact]
    public void CreateKeepsMissingValuesEmpty()
    {
        var monitor = BgInfoMonitorProvider.Create(0, null, primary: true, left: 0, top: 0, right: 0, bottom: 0, scale: 0);

        Assert.Equal(string.Empty, monitor.DeviceName);
        Assert.Equal(0, monitor.Width);
        Assert.Equal(0, monitor.Height);
        Assert.Null(monitor.Scale);
    }

    [Fact]
    public void GetMonitorsNeverThrowsAndUsesUniqueIndexes()
    {
        var monitors = BgInfoMonitorProvider.GetMonitors();

        Assert.Equal(monitors.Length, monitors.Select(monitor => monitor.Index).Distinct().Count());
        Assert.All(monitors, monitor => Assert.True(monitor.Width > 0 && monitor.Height > 0));
    }

    [Theory]
    [InlineData(8, 2560, 1440)]
    [InlineData(3, 3840, 2160)]
    [InlineData(99, 3840, 2160)]
    public void RendererUsesRequestedSnapshotIndexThenPrimary(int index, int width, int height)
    {
        var monitors = new[] {
            BgInfoMonitorProvider.Create(8, "secondary", false, -2560, 0, 0, 1440, 1.5),
            BgInfoMonitorProvider.Create(3, "primary", true, 0, 0, 3840, 2160, 2)
        };

        Assert.Equal((width, height), BgInfoGenerator.ResolveMonitorSize(monitors, index));
    }

    [Fact]
    public void RendererUsesFirstSnapshotWithoutPrimaryAndFallsBackWithoutGeometry()
    {
        var monitor = BgInfoMonitorProvider.Create(8, "", false, -2560, 0, 0, 1440, null);
        Assert.Equal((2560, 1440), BgInfoGenerator.ResolveMonitorSize(new[] { monitor }, 99));
        Assert.Equal((1920, 1080), BgInfoGenerator.ResolveMonitorSize(Array.Empty<BgInfoMonitor>(), 0));
    }
}
