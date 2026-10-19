namespace PowerBGInfo.Tests;

public class SessionUserTests {
    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(-1)]
    public void UnresolvedNonzeroSessionDoesNotReportTheConsoleUser(int sessionId) {
        var wmiRead = false;
        var user = SessionUser.Resolve(() => null, () => {
            wmiRead = true;
            return @"CONTOSO\console";
        }, sessionId);

        Assert.Equal(SystemInfoProvider.Unknown, user);
        Assert.False(wmiRead);
    }

    [Fact]
    public void ResolvedRemoteSessionStillWinsWithoutReadingTheConsole() {
        Assert.Equal(@"CONTOSO\remote", SessionUser.Resolve(
            () => @"CONTOSO\remote", () => throw new InvalidOperationException("Must not query console"), 8));
    }

    [Theory]
    [InlineData(@"CONTOSO\remote")]
    [InlineData("local-user")]
    public void UsesDesktopManagerAccountBeforeWmi(string accountName) {
        var wmiRead = false;

        var user = SessionUser.Resolve(() => accountName, () => {
            wmiRead = true;
            return @"CONTOSO\console";
        });

        Assert.Equal(accountName, user);
        Assert.False(wmiRead);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t")]
    public void FallsBackToTrimmedWmiWhenDesktopManagerHasNoUser(string? accountName) {
        var reads = new List<string>();

        var user = SessionUser.Resolve(() => {
            reads.Add("session");
            return accountName;
        }, () => {
            reads.Add("wmi");
            return " \tCONTOSO\\wmi \r\n";
        });

        Assert.Equal(@"CONTOSO\wmi", user);
        Assert.Equal(new[] { "session", "wmi" }, reads);
    }

    [Fact]
    public void FallsBackToTrimmedWmiWhenDesktopManagerThrows() {
        var user = SessionUser.Resolve(
            () => throw new InvalidOperationException("Session lookup unavailable"),
            () => @" CONTOSO\wmi ");

        Assert.Equal(@"CONTOSO\wmi", user);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n")]
    public void ReturnsUnknownWhenNeitherSourceHasAUser(string? wmiUser) {
        var user = SessionUser.Resolve(() => null, () => wmiUser);

        Assert.Equal(SystemInfoProvider.Unknown, user);
    }

    [Fact]
    public void ReturnsUnknownWhenBothSourcesThrow() {
        var user = SessionUser.Resolve(
            () => throw new InvalidOperationException("Session lookup unavailable"),
            () => throw new InvalidOperationException("WMI unavailable"));

        Assert.Equal(SystemInfoProvider.Unknown, user);
    }

    [Fact]
    public void ReturnsUnknownWhenWmiFallbackIsNotProvided() {
        var user = SessionUser.Resolve(() => null, null);

        Assert.Equal(SystemInfoProvider.Unknown, user);
    }
}
