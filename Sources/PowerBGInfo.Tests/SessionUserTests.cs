namespace PowerBGInfo.Tests;

public class SessionUserTests
{
    private static readonly Dictionary<int, string?> Sessions = new() {
        [1] = @"CONTOSO\console",
        [3] = @"CONTOSO\remote",
        [4] = null
    };

    [Fact]
    public void UsesTheUserOfTheProcessSessionOnASessionHost()
    {
        var user = SessionUser.Resolve(3, Query, () => 1, () => @"CONTOSO\console");

        Assert.Equal(@"CONTOSO\remote", user);
    }

    [Fact]
    public void UsesTheActiveConsoleSessionFromSessionZero()
    {
        var user = SessionUser.Resolve(0, Query, () => 1, () => "ignored");

        Assert.Equal(@"CONTOSO\console", user);
    }

    [Fact]
    public void FallsBackToWmiWhenTheSessionApiHasNoUser()
    {
        var user = SessionUser.Resolve(4, Query, () => -1, () => @" CONTOSO\wmi ");

        Assert.Equal(@"CONTOSO\wmi", user);
    }

    [Fact]
    public void ReturnsUnknownWhenNothingAnswers()
    {
        var user = SessionUser.Resolve(0, _ => throw new InvalidOperationException("no api"), () => throw new InvalidOperationException("no console"), () => throw new InvalidOperationException("no wmi"));

        Assert.Equal(SystemInfoProvider.Unknown, user);
    }

    [Fact]
    public void ReadsTheSignedInUserOfThisTestSession()
    {
        var session = SessionUser.GetProcessSessionId();
        if (session <= 0)
        {
            return; // Session 0 (a service account on a build agent) has no signed-in user to compare with.
        }

        var expected = $@"{Environment.UserDomainName}\{Environment.UserName}";
        Assert.Equal(expected, SessionUser.QuerySessionUser(session), StringComparer.OrdinalIgnoreCase);
        Assert.Equal(expected, SystemInfoProvider.GetSnapshot().LoggedOnUser, StringComparer.OrdinalIgnoreCase);
    }

    private static string? Query(int session) => Sessions.TryGetValue(session, out var user) ? user : null;
}
