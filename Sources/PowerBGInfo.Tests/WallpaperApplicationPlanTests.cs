namespace PowerBGInfo.Tests;

public sealed class WallpaperApplicationPlanTests {
    [Fact]
    public void WindowsAllUserPolicyCanRunWithoutAnInteractiveDesktop() {
        var config = new BgInfoConfiguration { Target = BgInfoTarget.Wallpaper, ApplyToAllUsers = true };
        Assert.False(WallpaperService.ValidateTarget(config, isWindows: true, canSetWallpaper: false, "Session 0"));
        Assert.True(WallpaperService.ValidateTarget(config, isWindows: true, canSetWallpaper: true, string.Empty));
    }

    [Fact]
    public void CurrentUserWallpaperStillRequiresAnInteractiveDesktop() {
        var config = new BgInfoConfiguration { Target = BgInfoTarget.Wallpaper };
        Assert.Throws<PlatformNotSupportedException>(() => WallpaperService.ValidateTarget(config, true, false, "Session 0"));
    }

    [Fact]
    public void NonWindowsCannotApplyAllUserPolicy() {
        var config = new BgInfoConfiguration { Target = BgInfoTarget.Wallpaper, ApplyToAllUsers = true };
        Assert.Throws<PlatformNotSupportedException>(() => WallpaperService.ValidateTarget(config, false, true, string.Empty));
    }
}
