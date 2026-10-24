using System.Runtime.InteropServices;

namespace PowerBGInfo.Tests;

public sealed class PortableSystemInfoProviderTests {
    [Fact]
    public void RealPortableSnapshotContainsIdentityAndHonestSecurityAvailability() {
        IBgInfoSystemInfoProvider provider = new PortableSystemInfoProvider();
        var before = DateTime.Now;
        var snapshot = provider.GetSnapshot();

        Assert.Equal(Environment.MachineName, snapshot.ComputerName);
        Assert.Equal(Environment.UserName, snapshot.UserName);
        Assert.False(string.IsNullOrWhiteSpace(snapshot.OSName));
        Assert.NotEqual(SystemInfoProvider.Unknown, snapshot.OSName);
        Assert.InRange(snapshot.CollectedAt, before, DateTime.Now);
        Assert.Equal(BgInfoFactState.Available, snapshot.FactStates[nameof(snapshot.ComputerName)]);
        Assert.Equal(SystemInfoProvider.Unknown, snapshot.DefenderStatus);
        Assert.Equal(SystemInfoProvider.Unknown, snapshot.BitLockerStatus);
        Assert.Null(snapshot.LastPatchDate);
        Assert.Equal(BgInfoFactState.Unsupported, snapshot.FactStates[nameof(snapshot.DefenderStatus)]);
        Assert.Equal(BgInfoFactState.Unsupported, snapshot.FactStates[nameof(snapshot.BitLockerStatus)]);
        Assert.Equal(BgInfoFactState.Unsupported, snapshot.FactStates[nameof(snapshot.LastPatchDate)]);
        Assert.All(snapshot.Disks, disk => {
            Assert.True(Path.IsPathRooted(disk.Drive));
            Assert.InRange(disk.UsedPercent, 0, 100);
        });
    }

    [Fact]
    public void SupportedUnixMetricsReadTheRealMachine() {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return;
        var provider = new PortableSystemInfoProvider();
        Assert.True(provider.TryGetNumericValue(BgInfoChartMetric.UptimeHours, null, out var hours));
        Assert.True(hours >= 0);
        Assert.True(provider.TryGetNumericValue(BgInfoChartMetric.MemoryPercent, null, out var memory));
        Assert.InRange(memory, 0, 100);
        Assert.True(provider.TryGetNumericValue(BgInfoChartMetric.CpuPercent, null, out var cpu));
        Assert.InRange(cpu, 0, 100);
        Assert.True(provider.TryGetNumericValue(BgInfoChartMetric.DiskFreeGb, Path.GetTempPath(), out var free));
        Assert.True(free >= 0);
    }

    [Fact]
    public void MissingRequestedFilesystemDoesNotReturnAnotherDisksMeasurement() {
        var provider = new PortableSystemInfoProvider();
        var nonexistent = Path.Combine(Path.GetTempPath(), "powerbginfo-missing-" + Guid.NewGuid().ToString("N"));
        Assert.False(provider.TryGetNumericValue(BgInfoChartMetric.DiskFreePercent, nonexistent, out _));
        Assert.False(provider.TryGetNumericValue(BgInfoChartMetric.None, null, out _));
        Assert.Equal(string.Empty, provider.GetValue("unknown-token"));
    }

    [Fact]
    public void LogicalCpuTokenUsesMachineFactsBeforeTheRuntimeProcessLimit() {
        Assert.Equal(PortableMachineFacts.ReadLogicalCores().ToString(System.Globalization.CultureInfo.CurrentCulture),
            new PortableSystemInfoProvider().GetValue("CpuLogicalCores"));
    }

    [Fact]
    public void UnavailableAndUnsupportedSnapshotFactsRemainDistinct() {
        var snapshot = new BgInfoSystemInfo { ComputerName = "host", DefenderStatus = SystemInfoProvider.Unknown };
        var states = BgInfoFactStates.FromSnapshot(snapshot, windowsFacts: true);
        Assert.Equal(BgInfoFactState.Available, states[nameof(snapshot.ComputerName)]);
        Assert.Equal(BgInfoFactState.Unavailable, states[nameof(snapshot.DefenderStatus)]);
        Assert.Equal(BgInfoFactState.Unsupported,
            BgInfoFactStates.FromSnapshot(snapshot, windowsFacts: false)[nameof(snapshot.DefenderStatus)]);
    }
}

public sealed class LinuxSystemFactsParserTests {
    [Theory]
    [InlineData("PRETTY_NAME=Fedora", "Fedora")]
    [InlineData("PRETTY_NAME=\"Example Linux 1\"", "Example Linux 1")]
    [InlineData("PRETTY_NAME='A $HOSTNAME `whoami`'", "A $HOSTNAME `whoami`")]
    public void OsReleaseRemainsLiteralData(string text, string expected) =>
        Assert.Equal(expected, PortableMachineFacts.ParseLinuxRelease(text, "PRETTY_NAME"));

    [Fact]
    public void LinuxMemoryCountersKeepUnitsAndRejectMalformedValues() {
        const string text = "MemTotal: 16000 kB\nMemAvailable: 4000 kB\n";
        Assert.Equal(16000UL * 1024, PortableMachineFacts.ParseLinuxMemoryValue(text, "MemTotal"));
        Assert.Equal(4000UL * 1024, PortableMachineFacts.ParseLinuxMemoryValue(text, "MemAvailable"));
        Assert.Null(PortableMachineFacts.ParseLinuxMemoryValue("MemTotal: 123 MB", "MemTotal"));
        Assert.Null(PortableMachineFacts.ParseLinuxMemoryValue("MemTotal: 18446744073709551615 kB", "MemTotal"));
        Assert.Null(PortableMachineFacts.ParseLinuxMemoryValue(null, "MemTotal"));
    }

    [Fact]
    public void CpuAccountingDoesNotCountGuestTimeTwice() {
        var counters = PortableMachineFacts.ParseLinuxCpuCounters("cpu  10 20 30 40 50 60 70 80 900 1000\ncpu0  1 2 3 4\n");
        Assert.NotNull(counters);
        Assert.Equal(360UL, counters.GetValueOrDefault().Total);
        Assert.Equal(90UL, counters.GetValueOrDefault().Idle);
        Assert.Null(PortableMachineFacts.ParseLinuxCpuCounters("cpu 1 2 broken 4"));
    }

    [Fact]
    public void LinuxLogicalCpusComeFromProcessorRecords() {
        Assert.Equal(4, PortableMachineFacts.ParseLinuxLogicalCores("processor : 0\ncore id : 0\n\nprocessor : 1\nprocessor : 2\nprocessor : 3\n"));
        Assert.Null(PortableMachineFacts.ParseLinuxLogicalCores("Processor : ARM processor description\n"));
        Assert.Null(PortableMachineFacts.ParseLinuxLogicalCores(null));
    }
}
