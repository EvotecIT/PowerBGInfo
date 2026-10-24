using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PowerBGInfo.Tests;

public sealed class WindowsDiskMetricTests {
    private const double Gigabyte = 1024d * 1024d * 1024d;

    [WindowsDiskMetricFact]
    public void MissingExplicitPathsDoNotReturnTheirDriveLettersCurrentMetrics() {
        using var folder = new FixtureFolder();
        var missing = Path.Combine(folder.Path, "not-created", "image.png");

        foreach (var metric in new[] { BgInfoChartMetric.DiskFreeGb, BgInfoChartMetric.DiskFreePercent, BgInfoChartMetric.DiskUsedPercent }) {
            Assert.False(TryReadMetric(metric, missing, out _));
        }
    }

    [WindowsDiskMetricFact]
    public void DeliberateDriveLettersAndExistingFilesOrDirectoriesSelectTheSameFilesystem() {
        using var folder = new FixtureFolder();
        var root = Path.GetPathRoot(BgInfoPaths.GetIdentity(folder.Path))!;
        var file = Path.Combine(folder.Path, "existing.txt");
        File.WriteAllText(file, "A disk metric may be bound to an existing file.");
        var expected = new DriveInfo(root).AvailableFreeSpace / Gigabyte;

        foreach (var argument in new[] { root.Substring(0, 1), root.Substring(0, 2), root, folder.Path, file }) {
            Assert.True(TryReadMetric(BgInfoChartMetric.DiskFreeGb, argument, out var free), argument);
            Assert.InRange(free, Math.Max(0, expected - 0.25d), expected + 0.25d);
        }
    }

    [WindowsDiskMetricFact(requiresAnotherVolume: true)]
    public void ExistingJunctionSelectsTheTargetVolumeInsteadOfTheJunctionsParentDrive() {
        using var folder = new FixtureFolder();
        var target = WindowsDiskMetricFixtures.FindDifferentVolume();
        Assert.NotNull(target);
        var junction = Path.Combine(folder.Path, "other-volume");
        try {
            CreateJunction(junction, target.RootDirectory.FullName);
            var expected = target.AvailableFreeSpace / Gigabyte;

            Assert.True(TryReadMetric(BgInfoChartMetric.DiskFreeGb, junction, out var free));

            Assert.InRange(free, Math.Max(0, expected - 0.25d), expected + 0.25d);
        } finally {
            if (Directory.Exists(junction)) Directory.Delete(junction);
        }
    }

    [WindowsDiskMetricFact(requiresUncDirectory: true)]
    public void ExplicitQualifiedUncDirectoryUsesItsOwnAvailableSpace() {
        var path = Environment.GetEnvironmentVariable(WindowsDiskMetricFixtures.UncDirectoryVariable)!;
        Assert.True(path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(@"\\", StringComparison.Ordinal) && !path.StartsWith(@"\\?\", StringComparison.Ordinal) &&
            !path.StartsWith(@"\\.\", StringComparison.Ordinal), "The explicitly supplied fixture must be a UNC directory.");
        Assert.True(Directory.Exists(path), "The explicitly supplied UNC fixture must already exist and be accessible.");
        var directory = path.EndsWith("\\", StringComparison.Ordinal) ? path : path + "\\";
        Assert.True(GetDiskFreeSpaceEx(directory, out var nativeFree, out _, out _), "The UNC fixture must support the native space query.");

        Assert.True(TryReadMetric(BgInfoChartMetric.DiskFreeGb, path, out var free));

        var expected = nativeFree / Gigabyte;
        Assert.InRange(free, Math.Max(0, expected - 0.25d), expected + 0.25d);
    }

    private static bool TryReadMetric(BgInfoChartMetric metric, string path, out double value) {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("This fixture requires native Windows.");
        return new WindowsSystemInfoProvider().TryGetNumericValue(metric, path, out value);
    }

    private static void CreateJunction(string path, string target) {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("This fixture requires native Windows.");
        using var process = Process.Start(new ProcessStartInfo {
            FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
            Arguments = "/d /c mklink /J \"" + path + "\" \"" + target + "\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        })!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(10000)) {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
            Assert.Fail("The owned junction-fixture command did not complete.");
        }
        Assert.True(process.ExitCode == 0, output.GetAwaiter().GetResult() + error.GetAwaiter().GetResult());
    }

    [DllImport("kernel32.dll", EntryPoint = "GetDiskFreeSpaceExW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetDiskFreeSpaceEx(string directory, out ulong available, out ulong total, out ulong free);

    private sealed class FixtureFolder : IDisposable {
        internal FixtureFolder() {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bginfo-windows-disk-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}

/// <summary>Reports genuinely unavailable native fixtures as skips instead of passing unexecuted bodies.</summary>
internal sealed class WindowsDiskMetricFactAttribute : FactAttribute {
    public WindowsDiskMetricFactAttribute(bool requiresAnotherVolume = false, bool requiresUncDirectory = false) {
        if (!OperatingSystem.IsWindows()) {
            Skip = "Native Windows disk-metric validation is unavailable on this platform.";
        } else if (requiresAnotherVolume && WindowsDiskMetricFixtures.FindDifferentVolume() == null) {
            Skip = "A second ready fixed volume with distinct available space is required for junction selection proof.";
        } else if (requiresUncDirectory && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(WindowsDiskMetricFixtures.UncDirectoryVariable))) {
            Skip = "Set " + WindowsDiskMetricFixtures.UncDirectoryVariable + " to an explicitly qualified existing UNC directory.";
        }
    }
}

internal static class WindowsDiskMetricFixtures {
    internal const string UncDirectoryVariable = "POWERBGINFO_TEST_UNC_DIRECTORY";

    internal static DriveInfo? FindDifferentVolume() {
        if (!OperatingSystem.IsWindows()) return null;
        try {
            var currentRoot = Path.GetPathRoot(BgInfoPaths.GetIdentity(Path.GetTempPath()))!;
            var current = new DriveInfo(currentRoot);
            foreach (var drive in DriveInfo.GetDrives()) {
                try {
                    if (drive.DriveType == DriveType.Fixed && drive.IsReady &&
                        !BgInfoPaths.AreSame(drive.RootDirectory.FullName, currentRoot) &&
                        Math.Abs(drive.AvailableFreeSpace - (double)current.AvailableFreeSpace) > 1024d * 1024d * 1024d) return drive;
                } catch (IOException) { /* A disappearing volume is not a fixture. */ }
                catch (UnauthorizedAccessException) { /* An inaccessible volume is not a fixture. */ }
            }
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException) {
            // A valid native temp-volume identity is required before creating a junction fixture.
        }
        return null;
    }
}
