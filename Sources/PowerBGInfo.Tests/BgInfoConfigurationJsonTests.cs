using System.IO;
using Xunit;

namespace PowerBGInfo.Tests;

public class BgInfoConfigurationJsonTests
{
    [Fact]
    public void SaveAndLoadPreservesBuiltinValueEntries()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "bginfo-json-" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, "config.json");

        var configuration = new BgInfoConfiguration
        {
            ConfigurationDirectory = tempDirectory
        };
        configuration.Entries.Add(new BgInfoEntry
        {
            Type = BgInfoEntryType.Value,
            Name = "Host",
            BuiltinValue = "HostName",
            Value = "stale-value"
        });

        BgInfoConfigurationJson.Save(configuration, path);

        var json = File.ReadAllText(path);
        Assert.Contains("\"BuiltinValue\"", json);
        Assert.DoesNotContain("stale-value", json);

        var roundTripped = BgInfoConfigurationJson.Load(path);
        var entry = Assert.Single(roundTripped.Entries);
        Assert.Equal("HostName", entry.BuiltinValue);
        Assert.Null(entry.Value);
    }
}
