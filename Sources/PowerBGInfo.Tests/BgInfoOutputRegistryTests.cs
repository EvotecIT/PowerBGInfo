namespace PowerBGInfo.Tests;

public sealed class BgInfoOutputRegistryTests {
    [Fact]
    public void NativeFileNamesRoundTripWithoutCreatingAnotherProvenanceRecord() {
        using var folder = new RegistryFolder();
        var cases = new List<(string, string)> { ("#wallpaper.png", "original.png") };
        if (!OperatingSystem.IsWindows()) { // Pipe and newline are valid Unix filename characters.
            cases.Add(("wall|paper.png", "original|photo.png"));
            cases.Add(("wall\npaper.png", "original\nphoto.png"));
            cases.Add(("ordinary.png", "original.png\nforged.png|/unrelated-original.png"));
        }

        foreach (var item in cases) {
            BgInfoOutputRegistry.Record(Path.Combine(folder.Path, item.Item1), Path.Combine(folder.Path, item.Item2));
        }

        foreach (var item in cases) {
            Assert.True(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, item.Item1), out var source));
            Assert.Equal(Path.Combine(folder.Path, item.Item2), source);
        }
        Assert.False(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "forged.png"), out _));
    }

    [Fact]
    public void RecordingNewOutputMigratesAnUnambiguousLegacyRecord() {
        using var folder = new RegistryFolder();
        var original = Path.Combine(folder.Path, "original.png");
        File.WriteAllText(folder.Registry, "# Images written by PowerBGInfo and the image each was drawn on (empty: a generated or plain background).\nold.png|" + original + "\n");

        BgInfoOutputRegistry.Record(Path.Combine(folder.Path, "new.png"), null);

        Assert.True(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "old.png"), out var source));
        Assert.Equal(original, source);
        Assert.True(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "new.png"), out source));
        Assert.Null(source);
        Assert.StartsWith("# PowerBGInfo output registry v2", File.ReadAllLines(folder.Registry)[0]);
    }

    [Fact]
    public void AmbiguousLegacyRowsCannotSupplyAnOriginalImage() {
        using var folder = new RegistryFolder();
        File.WriteAllText(folder.Registry, "wallpaper.png|" + Path.Combine(folder.Path, "original|photo.png") + "\n");

        Assert.False(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "wallpaper.png"), out _));
    }

    [Fact]
    public void MalformedLegacySnapshotDoesNotTrustATruncatedNewlineSource() {
        using var folder = new RegistryFolder();
        File.WriteAllText(folder.Registry, "wallpaper.png|" + Path.Combine(folder.Path, "original") + "\nphoto.png\n");

        Assert.False(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "wallpaper.png"), out _));
    }

    [Fact]
    public void UnknownRegistryVersionCannotBeReadAsAnUnescapedLegacyRecord() {
        using var folder = new RegistryFolder();
        File.WriteAllText(folder.Registry, "# PowerBGInfo output registry v999\nwallpaper.png|" + Path.Combine(folder.Path, "original.png") + "\n");

        Assert.False(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "wallpaper.png"), out _));
    }

    private sealed class RegistryFolder : IDisposable {
        internal string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bginfo-registry-" + Guid.NewGuid().ToString("N"));
        internal string Registry => System.IO.Path.Combine(Path, BgInfoOutputRegistry.FileName);
        internal RegistryFolder() => Directory.CreateDirectory(Path);
        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
