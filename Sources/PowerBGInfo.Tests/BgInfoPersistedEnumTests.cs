namespace PowerBGInfo.Tests;

public sealed class BgInfoPersistedEnumTests {
    [Theory]
    [InlineData("{\"Widgets\":[{\"Kind\":\"Metric\",\"Value\":\"1\",\"ThemeMode\":\"Ligth\"}]}")]
    [InlineData("{\"Topologies\":[{\"Theme\":\"Ligth\"}]}")]
    [InlineData("{\"Target\":\"not-a-target\"}")]
    [InlineData("{\"Target\":\"1\"}")]
    [InlineData("{\"Target\":\"Wallpaper, Future\"}")]
    [InlineData("{\"Target\":\"Wallpaper,,File\"}")]
    [InlineData("{\"Charts\":[{\"Kind\":\"not-a-chart\"}]}")]
    [InlineData("{\"Topologies\":[{\"Direction\":\"south\"}]}")]
    [InlineData("{\"VisualCanvases\":[{\"Template\":\"not-a-template\"}]}")]
    [InlineData("{\"Images\":[{\"Fit\":\"not-a-fit\"}]}")]
    [InlineData("{\"Entries\":[{\"Type\":\"not-an-entry\",\"Value\":\"42\"}]}")]
    public void UnknownPersistedEnumNamesFailInsteadOfSilentlySelectingDefaults(string json) {
        var path = Path.Combine(Path.GetTempPath(), "bginfo-enum-" + Path.GetRandomFileName() + ".json");
        File.WriteAllText(path, json);
        try { Assert.Throws<InvalidDataException>(() => BgInfoConfigurationJson.Load(path)); }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(null, ChartForgeX.Themes.VisualThemeMode.Dark)]
    [InlineData(" ", ChartForgeX.Themes.VisualThemeMode.Dark)]
    [InlineData("light", ChartForgeX.Themes.VisualThemeMode.Light)]
    public void MissingWidgetModeKeepsTheDefaultAndNamedModesRemainCaseInsensitive(string? mode, ChartForgeX.Themes.VisualThemeMode expected) {
        var path = Path.Combine(Path.GetTempPath(), "bginfo-mode-" + Path.GetRandomFileName() + ".json");
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new { Widgets = new[] { new { Kind = "Metric", Value = "1", ThemeMode = mode } } }));
        try { Assert.Equal(expected, Assert.Single(BgInfoConfigurationJson.Load(path).Widgets).ThemeMode); }
        finally { File.Delete(path); }
    }

    [Fact]
    public void CombinedTargetsRoundTripThroughPublicSaveAndLoad() {
        var path = Path.Combine(Path.GetTempPath(), "bginfo-flags-" + Path.GetRandomFileName() + ".json");
        try {
            var expected = BgInfoTarget.Wallpaper | BgInfoTarget.File;
            BgInfoConfigurationJson.Save(new BgInfoConfiguration { Target = expected }, path);
            Assert.Equal(expected, BgInfoConfigurationJson.Load(path).Target);
        } finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("Wallpaper, LogonScreen", BgInfoTarget.Both)]
    [InlineData("wallpaper, file", BgInfoTarget.Wallpaper | BgInfoTarget.File)]
    [InlineData("Both", BgInfoTarget.Both)]
    public void DefinedTargetNamesCanBeCombined(string names, BgInfoTarget expected) {
        var path = Path.Combine(Path.GetTempPath(), "bginfo-flags-" + Path.GetRandomFileName() + ".json");
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new { Target = names }));
        try { Assert.Equal(expected, BgInfoConfigurationJson.Load(path).Target); }
        finally { File.Delete(path); }
    }
}
