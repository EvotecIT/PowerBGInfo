using ChartForgeX.Primitives;

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
    public void OnlyWindowsCanMigrateValidatedUnescapedLegacyRecords() {
        using var folder = new RegistryFolder();
        var original = Path.Combine(folder.Path, "original.png");
        File.WriteAllText(folder.Registry, "# Images written by PowerBGInfo and the image each was drawn on (empty: a generated or plain background).\nold.png|" + original + "\n");

        BgInfoOutputRegistry.Record(Path.Combine(folder.Path, "new.png"), null);

        Assert.Equal(OperatingSystem.IsWindows(), BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "old.png"), out var source));
        Assert.Equal(OperatingSystem.IsWindows() ? original : null, source);
        Assert.True(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "new.png"), out source));
        Assert.Null(source);
        Assert.StartsWith("# PowerBGInfo output registry v2", File.ReadAllLines(folder.Registry)[0]);
    }

    [Fact]
    public void UnixLegacyNewlineCannotTrustAnExistingTruncatedSourceOrInventAnotherOutput() {
        if (OperatingSystem.IsWindows()) return;
        using var folder = new RegistryFolder();
        var truncatedSource = folder.CreateImage("original.png", ChartColors.Navy);
        var forgedSource = folder.CreateImage("other.png", ChartColors.DarkGreen);
        var realSource = truncatedSource + "\nforged.png|" + forgedSource;
        Directory.CreateDirectory(Path.GetDirectoryName(realSource)!);
        folder.CreateImage(realSource, ChartColors.DarkRed);
        var output = Path.Combine(folder.Path, "out.png");
        // This is exactly the text the former unescaped writer produced for
        // the real newline-containing path, with two syntactically valid rows.
        File.WriteAllText(folder.Registry, "out.png|" + realSource + "\n");

        Assert.True(File.Exists(realSource));
        Assert.True(File.Exists(truncatedSource));
        Assert.True(File.Exists(forgedSource));
        Assert.False(BgInfoOutputRegistry.TryGetSource(output, out _, out var untrusted));
        Assert.True(untrusted);
        Assert.False(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "forged.png"), out _));

        var newOutput = Path.Combine(folder.Path, "new.png");
        BgInfoOutputRegistry.Record(newOutput, realSource);

        Assert.True(BgInfoOutputRegistry.TryGetSource(newOutput, out var source, out untrusted));
        Assert.Equal(realSource, source);
        Assert.False(untrusted);
        Assert.False(BgInfoOutputRegistry.TryGetSource(output, out _, out untrusted));
        Assert.True(untrusted);
        Assert.False(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "forged.png"), out _, out untrusted));
        Assert.True(untrusted);

        BgInfoOutputRegistry.Record(Path.Combine(folder.Path, "another.png"), truncatedSource);

        Assert.False(BgInfoOutputRegistry.TryGetSource(output, out _, out untrusted));
        Assert.True(untrusted);
        Assert.True(BgInfoOutputRegistry.TryGetSource(newOutput, out source, out untrusted));
        Assert.Equal(realSource, source);
        Assert.False(untrusted);
    }

    [Theory]
    [InlineData("original*.png")]
    [InlineData("original\u0001.png")]
    [InlineData("original.png:stream")]
    public void WindowsLegacyRecordsMustValidateBothKeysAndSources(string name) {
        if (!OperatingSystem.IsWindows()) return;
        using var folder = new RegistryFolder();
        var validSource = Path.Combine(folder.Path, "original.png");
        File.WriteAllText(folder.Registry, "wallpaper.png|" + validSource + "\n" + name + "|" + validSource + "\n");

        Assert.False(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "wallpaper.png"), out _, out var untrusted));
        Assert.True(untrusted);

        File.WriteAllText(folder.Registry, "wallpaper.png|" + Path.Combine(folder.Path, name) + "\n");
        Assert.False(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "wallpaper.png"), out _, out untrusted));
        Assert.True(untrusted);
    }

    [Fact]
    public void AmbiguousLegacyRowsCannotSupplyAnOriginalImage() {
        using var folder = new RegistryFolder();
        File.WriteAllText(folder.Registry, "wallpaper.png|" + Path.Combine(folder.Path, "original|photo.png") + "\n");

        Assert.False(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "wallpaper.png"), out _, out var untrusted));
        Assert.True(untrusted);
    }

    [Fact]
    public void MalformedLegacySnapshotDoesNotTrustATruncatedNewlineSource() {
        using var folder = new RegistryFolder();
        File.WriteAllText(folder.Registry, "wallpaper.png|" + Path.Combine(folder.Path, "original") + "\nphoto.png\n");

        Assert.False(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "wallpaper.png"), out _, out var untrusted));
        Assert.True(untrusted);
    }

    [Fact]
    public void UnknownRegistryVersionCannotBeReadAsAnUnescapedLegacyRecord() {
        using var folder = new RegistryFolder();
        File.WriteAllText(folder.Registry, "# PowerBGInfo output registry v999\nwallpaper.png|" + Path.Combine(folder.Path, "original.png") + "\n");

        Assert.False(BgInfoOutputRegistry.TryGetSource(Path.Combine(folder.Path, "wallpaper.png"), out _, out var untrusted));
        Assert.True(untrusted);
    }

    private sealed class RegistryFolder : IDisposable {
        internal string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bginfo-registry-" + Guid.NewGuid().ToString("N"));
        internal string Registry => System.IO.Path.Combine(Path, BgInfoOutputRegistry.FileName);
        internal RegistryFolder() => Directory.CreateDirectory(Path);
        internal string CreateImage(string name, ChartColor color) {
            var path = System.IO.Path.Combine(Path, name);
            using var image = new BgInfoRasterImage();
            image.Create(path, 8, 8, color);
            image.Save(path);
            return path;
        }
        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
