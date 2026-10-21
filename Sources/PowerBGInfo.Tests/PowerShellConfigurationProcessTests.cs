using PowerBGInfo.Cli;

namespace PowerBGInfo.Tests;

public sealed class PowerShellConfigurationProcessTests {
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

    private sealed class ScriptFixture : IDisposable {
        internal string Directory { get; } = Path.Combine(Path.GetTempPath(), "bginfo-script-" + Guid.NewGuid().ToString("N"));
        internal string Script => Path.Combine(Directory, "author.ps1");
        internal string Json => Path.Combine(Directory, "config.json");

        internal ScriptFixture() {
            System.IO.Directory.CreateDirectory(Directory);
            File.WriteAllText(Json, "{}");
        }

        public void Dispose() => System.IO.Directory.Delete(Directory, recursive: true);
    }
}
