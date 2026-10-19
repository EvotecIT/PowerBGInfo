using System.Diagnostics;

namespace PowerBGInfo.Tests;

public sealed class SystemInfoQueryExecutorTests {
    [Fact]
    public void StalledSetupOrDisposalReturnsUnknownWithoutAccumulatingWorkersAndRecovers() {
        var executor = new SystemInfoQueryExecutor();
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        using var exited = new ManualResetEventSlim();
        var calls = 0;
        var timer = Stopwatch.StartNew();
        try {
            Assert.Null(executor.Run(() => {
                Interlocked.Increment(ref calls);
                entered.Set();
                try { release.Wait(); } finally { exited.Set(); }
                return "late facts";
            }, TimeSpan.FromMilliseconds(100)));
            Assert.True(entered.Wait(TimeSpan.FromSeconds(2)));
            Assert.Null(executor.Run(() => { Interlocked.Increment(ref calls); return "new facts"; }, TimeSpan.FromMilliseconds(100)));
            Assert.Equal(1, Volatile.Read(ref calls));
            Assert.True(timer.Elapsed < TimeSpan.FromSeconds(2), "A stalled provider must not hold its caller.");
        } finally {
            release.Set();
            Assert.True(exited.Wait(TimeSpan.FromSeconds(2)));
        }
        Assert.Equal("recovered", executor.Run(() => "recovered", TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public void ProviderFailureLeavesUnknownAndReleasesCapacity() {
        var executor = new SystemInfoQueryExecutor();
        Assert.Null(executor.Run<string>(() => throw new InvalidOperationException("provider unavailable"), TimeSpan.FromSeconds(2)));
        Assert.Equal("facts", executor.Run(() => "facts", TimeSpan.FromSeconds(2)));
    }
}
