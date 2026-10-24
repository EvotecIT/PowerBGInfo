using System.Diagnostics;
using System.Globalization;
using PowerBGInfo.Cli;

namespace PowerBGInfo.Tests;

public sealed class PowerShellConfigurationProcessTests {
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConfigurationHandoffUsesAPrivateInvocationDirectoryAndRemovesItsOwnedFiles(bool fails) {
        using var fixture = new ScriptFixture();
        var receipt = Path.Combine(fixture.Directory, "temporary-paths.txt");
        File.WriteAllText(fixture.Script,
            "[System.IO.File]::WriteAllLines(" + Literal(receipt) + ", [string[]] @(" +
            "[System.IO.Path]::GetDirectoryName($MyInvocation.ScriptName), $env:POWERBGINFO_CONFIGURATION_LIFETIME))\n" +
            "if ([System.IO.Path]::DirectorySeparatorChar -ne '\\') {\n" +
            "  $mode = [System.IO.File]::GetUnixFileMode([System.IO.Path]::GetDirectoryName($MyInvocation.ScriptName))\n" +
            "  if (($mode -band 63) -ne 0) { throw 'Configuration handoff is accessible to another user.' }\n" +
            "} else {\n" +
            "  $acl = Get-Acl -LiteralPath ([System.IO.Path]::GetDirectoryName($MyInvocation.ScriptName))\n" +
            "  if (-not $acl.AreAccessRulesProtected) { throw 'Configuration handoff inherits shared access.' }\n" +
            "}\n" + (fails ? "throw 'intentional fixture failure'\n" : Literal(fixture.Json) + "\n"));

        if (fails) {
            var error = Assert.Throws<InvalidOperationException>(() => PowerShellConfigurationLoader.Load(fixture.Script, null, null, TimeSpan.FromSeconds(30)));
            Assert.Contains("intentional fixture failure", error.Message);
        } else {
            Assert.NotNull(PowerShellConfigurationLoader.Load(fixture.Script, null, null, TimeSpan.FromSeconds(30)).Configuration);
        }

        var paths = File.ReadAllLines(receipt);
        Assert.Equal(2, paths.Length);
        Assert.False(Directory.Exists(paths[0]), "The invocation left its wrapper/output directory behind.");
        Assert.False(Directory.Exists(paths[1]), "The invocation left its lifetime-marker directory behind.");
    }

    [Fact]
    public void ConfigurationCleanupPreservesFilesCreatedByTheAuthorScript() {
        using var fixture = new ScriptFixture();
        var receipt = Path.Combine(fixture.Directory, "temporary-path.txt");
        File.WriteAllText(fixture.Script,
            "[System.IO.File]::WriteAllText(" + Literal(receipt) + ", $env:POWERBGINFO_CONFIGURATION_LIFETIME)\n" +
            "[System.IO.File]::WriteAllText([System.IO.Path]::Combine($env:POWERBGINFO_CONFIGURATION_LIFETIME, 'author-data.txt'), 'keep')\n" +
            Literal(fixture.Json) + "\n");

        string? lifetimeDirectory = null;
        try {
            Assert.NotNull(PowerShellConfigurationLoader.Load(fixture.Script, null, null, TimeSpan.FromSeconds(30)).Configuration);
            lifetimeDirectory = File.ReadAllText(receipt);
            Assert.Equal("keep", File.ReadAllText(Path.Combine(lifetimeDirectory, "author-data.txt")));
            Assert.Equal(new[] { "author-data.txt" }, Directory.GetFiles(lifetimeDirectory).Select(Path.GetFileName));
            Assert.False(File.Exists(Path.Combine(Path.GetDirectoryName(lifetimeDirectory)!, "wrapper.ps1")));
            Assert.False(File.Exists(Path.Combine(Path.GetDirectoryName(lifetimeDirectory)!, "configuration.json")));
        } finally {
            if (lifetimeDirectory == null && File.Exists(receipt)) lifetimeDirectory = File.ReadAllText(receipt);
            if (lifetimeDirectory != null && Directory.Exists(lifetimeDirectory)) {
                File.Delete(Path.Combine(lifetimeDirectory, "author-data.txt"));
                Directory.Delete(lifetimeDirectory);
                var invocationDirectory = Path.GetDirectoryName(lifetimeDirectory)!;
                if (Path.GetFileName(invocationDirectory).StartsWith("PowerBGInfo-", StringComparison.Ordinal)) Directory.Delete(invocationDirectory);
            }
        }
    }

    [Fact]
    public void NoisyStandardErrorDoesNotBlockConfigurationLoading() {
        using var fixture = new ScriptFixture();
        File.WriteAllText(fixture.Script, "[Console]::Error.WriteLine(('diagnostic ' * 20000))\n'" + fixture.Json.Replace("'", "''") + "'\n");

        var loaded = PowerShellConfigurationLoader.Load(fixture.Script, null, null, TimeSpan.FromSeconds(30));

        Assert.NotNull(loaded.Configuration);
        Assert.Equal(fixture.Directory, loaded.ScriptDirectory);
    }

    [Fact]
    public void ScriptExecutionHasAnOuterDeadline() {
        using var fixture = new ScriptFixture();
        File.WriteAllText(fixture.Script, "Start-Sleep -Seconds 30\n");

        var failure = Assert.Throws<TimeoutException>(() =>
            PowerShellConfigurationLoader.Load(fixture.Script, null, null, TimeSpan.FromSeconds(1)));

        Assert.Contains("deadline", failure.Message);
    }

    public static IEnumerable<object?[]> NativeChildCases() {
        foreach (var outcome in new[] { "success", "failure", "timeout" }) {
            yield return new object?[] { null, outcome };
            if (OperatingSystem.IsWindows()) {
                // This exercises the actual 5.1 host as well as the default pwsh host.
                yield return new object?[] {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe"), outcome
                };
            }
        }
    }

    [Theory]
    [MemberData(nameof(NativeChildCases))]
    public async Task NativeChildrenHoldingInheritedPipesEndWithTheConfigurationInvocation(string? host, string outcome) {
        using var fixture = new ScriptFixture();
        fixture.WriteNativeChildScript(outcome);
        var elapsed = Stopwatch.StartNew();
        // Includes two cold PowerShell hosts and runtime compilation of the
        // process-group supervisor. This is a lifetime contract, not a startup benchmark.
        var invocation = Task.Run(() => PowerShellConfigurationLoader.Load(fixture.Script, null, host, TimeSpan.FromSeconds(20)));
        try {
            // Keep a regression from hanging the test host on StreamReader.Dispose.
            // Cleanup below uses only the recorded native child's PID and start time.
            Assert.Same(invocation, await Task.WhenAny(invocation, Task.Delay(TimeSpan.FromSeconds(30))));
            if (outcome == "timeout") {
                var failure = await Assert.ThrowsAsync<TimeoutException>(async () => await invocation);
                Assert.Contains("deadline", failure.Message);
            } else if (outcome == "failure") {
                var failure = await Assert.ThrowsAsync<InvalidOperationException>(async () => await invocation);
                // A script-local exit returns to the wrapper, which may itself
                // reject the missing configuration with its own nonzero status.
                Assert.Contains("PowerShell script execution failed with exit code", failure.Message);
                Assert.Contains("intentional fixture failure", failure.Message);
            } else {
                Assert.NotNull((await invocation).Configuration);
            }

            Assert.True(File.Exists(fixture.ChildIdentity), "The real configuration script did not start its native child.");
            Assert.True(SpinWait.SpinUntil(fixture.ChildHasExited, TimeSpan.FromSeconds(5)),
                "The configuration invocation left its owned native child running.");
            Assert.True(elapsed.Elapsed < TimeSpan.FromSeconds(30), "Native child cleanup exceeded the bounded invocation deadline.");
        } finally {
            fixture.StopChild();
            try { await invocation.WaitAsync(TimeSpan.FromSeconds(5)); } catch (Exception) { }
        }
    }

    private sealed class ScriptFixture : IDisposable {
        internal string Directory { get; } = Path.Combine(Path.GetTempPath(), "bginfo-script-" + Guid.NewGuid().ToString("N"));
        internal string Script => Path.Combine(Directory, "author.ps1");
        internal string Json => Path.Combine(Directory, "config.json");
        internal string ChildIdentity => Path.Combine(Directory, "native-child.identity");

        internal ScriptFixture() {
            System.IO.Directory.CreateDirectory(Directory);
            File.WriteAllText(Json, "{}");
        }

        internal void WriteNativeChildScript(string outcome) {
            var executable = OperatingSystem.IsWindows() ? Path.Combine(Environment.SystemDirectory, "ping.exe") : "/bin/sleep";
            var arguments = OperatingSystem.IsWindows() ? "-n 60 127.0.0.1" : "60";
            var ending = outcome == "timeout" ? "Start-Sleep -Seconds 60\n"
                : outcome == "failure" ? "[Console]::Error.WriteLine('intentional fixture failure')\nexit 7\n"
                : Literal(Json) + "\n";
            File.WriteAllText(Script,
                "$start = [System.Diagnostics.ProcessStartInfo]::new()\n" +
                "$start.FileName = " + Literal(executable) + "\n" +
                "$start.Arguments = " + Literal(arguments) + "\n" +
                "$start.UseShellExecute = $false\n" +
                "$child = [System.Diagnostics.Process]::Start($start)\n" +
                "[System.IO.File]::WriteAllText(" + Literal(ChildIdentity) +
                ", $child.Id.ToString() + '|' + $child.StartTime.ToUniversalTime().Ticks.ToString())\n" + ending);
        }

        internal bool ChildHasExited() {
            using var child = FindOwnedChild();
            return child == null || child.HasExited;
        }

        internal void StopChild() {
            using var child = FindOwnedChild();
            if (child == null || child.HasExited) return;
            try {
                child.Kill(entireProcessTree: true);
                child.WaitForExit(5000);
            } catch (InvalidOperationException) { }
        }

        private Process? FindOwnedChild() {
            if (!File.Exists(ChildIdentity)) return null;
            var identity = File.ReadAllText(ChildIdentity).Split('|');
            if (identity.Length != 2 || !int.TryParse(identity[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ||
                !long.TryParse(identity[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var started)) {
                throw new InvalidOperationException("The native fixture did not record a complete process identity.");
            }
            Process? child = null;
            try {
                child = Process.GetProcessById(id);
                if (child.HasExited || child.StartTime.ToUniversalTime().Ticks != started) {
                    child.Dispose();
                    return null;
                }
                return child;
            } catch (ArgumentException) {
                child?.Dispose();
                return null;
            } catch (InvalidOperationException) {
                child?.Dispose();
                return null;
            }
        }

        private static string Literal(string value) => "'" + value.Replace("'", "''") + "'";

        public void Dispose() {
            StopChild();
            System.IO.Directory.Delete(Directory, recursive: true);
        }
    }

    private static string Literal(string value) => "'" + value.Replace("'", "''") + "'";
}
