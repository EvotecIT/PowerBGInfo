using Xunit;

namespace PowerBGInfo.Tests;

public class BgInfoVariableResolverTests {
    [Fact]
    public void ExpandEntriesRepeatsTemplateEntryForEachVariableItem() {
        var configuration = new BgInfoConfiguration();
        configuration.Entries.Add(new BgInfoEntry {
            Type = BgInfoEntryType.Value,
            ForEach = "Volumes",
            Name = "Drive {{DriveLetter}}",
            Value = "{{SizeRemaining}} free on {{HostName}}"
        });

        var expanded = BgInfoVariableResolver.ExpandEntries(configuration,
            new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>>(StringComparer.OrdinalIgnoreCase) {
                ["Volumes"] = new List<IReadOnlyDictionary<string, string>> {
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                        ["DriveLetter"] = "C",
                        ["SizeRemaining"] = "100 GB"
                    },
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                        ["DriveLetter"] = "D",
                        ["SizeRemaining"] = "200 GB"
                    }
                }
            });

        Assert.Collection(expanded,
            first => {
                Assert.Equal("Drive C", first.Name);
                Assert.Contains("100 GB", first.Value);
                Assert.DoesNotContain("{{", first.Value);
                Assert.Null(first.ForEach);
            },
            second => {
                Assert.Equal("Drive D", second.Name);
                Assert.Contains("200 GB", second.Value);
                Assert.Null(second.ForEach);
            });
    }

    [Fact]
    public void RenderTemplateFallsBackToBuiltinSystemValues() {
        var value = BgInfoVariableResolver.RenderTemplate("Host {{HostName}}",
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        Assert.StartsWith("Host ", value);
        Assert.NotEqual("Host ", value);
    }
}
