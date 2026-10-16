namespace PowerBGInfo.Tests;

public class BgInfoMonitorTests
{
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
}
