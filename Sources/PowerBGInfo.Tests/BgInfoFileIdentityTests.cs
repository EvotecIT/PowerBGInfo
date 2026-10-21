using ChartForgeX.Primitives;

namespace PowerBGInfo.Tests;

public sealed class BgInfoFileIdentityTests {
    [Fact]
    public void PlannedOutputSetRemainsValidWhenADifferentlyCasedFileIsCreated() {
        using var folder = new FixtureFolder();
        var planned = Path.Combine(folder.Path, "photo.png");
        var actual = Path.Combine(folder.Path, "Photo.png");
        var hash = BgInfoPaths.Comparer.GetHashCode(planned);
        var outputs = new HashSet<string>(BgInfoPaths.Comparer) { planned };
        folder.CreateImage("Photo.png");

        var aliases = File.Exists(planned);
        Assert.Equal(hash, BgInfoPaths.Comparer.GetHashCode(planned));
        Assert.Equal(aliases, outputs.Contains(actual));
        Assert.Equal(!aliases, outputs.Add(actual));
        Assert.Equal(aliases ? 1 : 2, outputs.Count);
    }

    [Fact]
    public void ExistingFilesUseTheVolumesActualCaseRules() {
        using var folder = new FixtureFolder();
        var lower = folder.CreateImage("photo.png");
        var upper = Path.Combine(folder.Path, "Photo.png");
        var aliases = File.Exists(upper);
        if (!aliases) folder.CreateImage("Photo.png", ChartColors.DarkGreen);

        Assert.Equal(aliases, BgInfoPaths.AreSame(lower, upper));
        var knownOutputs = new HashSet<string>(BgInfoPaths.Comparer) { lower, upper };
        Assert.Equal(aliases ? 1 : 2, knownOutputs.Count);
        Assert.Equal(aliases, string.Equals(BgInfoPaths.GetIdentity(lower), BgInfoPaths.GetIdentity(upper), StringComparison.Ordinal));
        var generator = new BgInfoGenerator(new ImageService(), new FakeWallpaperService());
        var resolved = generator.ResolveOriginalImage(upper, null, "The slideshow image", chosenByUser: false,
            knownOutputPaths: new HashSet<string>(BgInfoPaths.Comparer) { lower });
        if (aliases) Assert.Null(resolved);
        else Assert.Equal(upper, resolved);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CaseAliasedOutputPreservesTheOriginalAndSurvivesANewGenerator(bool backgroundImage) {
        using var folder = new FixtureFolder();
        var original = folder.CreateImage("photo.png");
        var output = Path.Combine(folder.Path, "Photo.png");
        var aliases = File.Exists(output);
        var originalPixels = Pixels(original);

        string Render() {
            var config = new BgInfoConfiguration {
                FilePath = backgroundImage ? string.Empty : original,
                Background = backgroundImage ? new BgInfoBackground { Width = 320, Height = 180, ImagePath = original } : null,
                ConfigurationDirectory = folder.Path,
                OutputFileName = "Photo.png",
                Target = BgInfoTarget.File,
                Color = ChartColors.White,
                FontSize = 18
            };
            config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Label, Name = "Host identity" });
            return new BgInfoGenerator(new ImageService(), new FakeWallpaperService()).Generate(config);
        }

        var first = Render();
        var firstPixels = Pixels(first);
        Assert.NotEqual(originalPixels, firstPixels);
        Assert.True(BgInfoOutputRegistry.TryGetSource(first, out var recordedSource));
        Assert.NotNull(recordedSource);
        Assert.False(BgInfoPaths.AreSame(recordedSource!, first));
        Assert.Equal(originalPixels, Pixels(recordedSource!));
        Assert.Equal(aliases ? 1 : 0, Directory.GetFiles(folder.Path, "*.PowerBGInfo-original-*.png").Length);
        if (!aliases) Assert.Equal(originalPixels, Pixels(original));

        var second = Render();
        Assert.Equal(firstPixels, Pixels(second));
        Assert.True(BgInfoOutputRegistry.TryGetSource(second, out var restartedSource));
        Assert.Equal(recordedSource, restartedSource);
        Assert.Single(File.ReadAllLines(Path.Combine(folder.Path, BgInfoOutputRegistry.FileName)),
            line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#", StringComparison.Ordinal));
    }

    [Fact]
    public void RegistryAliasesShareOneMappingOnlyWhenTheFilesystemDoes() {
        using var folder = new FixtureFolder();
        var lower = folder.CreateImage("screen.png");
        var upper = Path.Combine(folder.Path, "Screen.png");
        var aliases = File.Exists(upper);
        if (!aliases) folder.CreateImage("Screen.png", ChartColors.DarkGreen);
        var originalOne = folder.CreateImage("original-one.png");
        var originalTwo = folder.CreateImage("original-two.png");
        BgInfoOutputRegistry.Record(lower, originalOne);
        BgInfoOutputRegistry.Record(upper, originalTwo);

        Assert.True(BgInfoOutputRegistry.TryGetSource(lower, out var lowerSource));
        Assert.True(BgInfoOutputRegistry.TryGetSource(upper, out var upperSource));
        Assert.True(BgInfoPaths.AreSame(aliases ? originalTwo : originalOne, lowerSource!));
        Assert.True(BgInfoPaths.AreSame(originalTwo, upperSource!));
        Assert.Equal(aliases ? 1 : 2, File.ReadAllLines(Path.Combine(folder.Path, BgInfoOutputRegistry.FileName))
            .Count(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#", StringComparison.Ordinal)));
    }

    [Fact]
    public void UnixVolumeBindingsNormalizeExistingDirectoryAliasesWithoutMergingNewNames() {
        if (OperatingSystem.IsWindows()) return;
        using var folder = new FixtureFolder();
        var parent = Directory.GetParent(folder.Path)!.FullName;
        var alias = Path.Combine(parent, Path.GetFileName(folder.Path).ToUpperInvariant());
        var expectedSame = Directory.Exists(alias);
        Assert.Equal(expectedSame, string.Equals(BgInfoVariableResolver.NormalizeVolumeIdentity(folder.Path),
            BgInfoVariableResolver.NormalizeVolumeIdentity(alias), StringComparison.Ordinal));
        Assert.False(BgInfoPaths.AreSame(Path.Combine(folder.Path, "not-created.png"), Path.Combine(folder.Path, "NOT-CREATED.png")));
    }

    [Fact]
    public void RecordedSourceCaseAliasIsNotUsedAsItsOwnOriginal() {
        using var folder = new FixtureFolder();
        var lower = folder.CreateImage("screen.png");
        var upper = Path.Combine(folder.Path, "Screen.png");
        var aliases = File.Exists(upper);
        if (!aliases) folder.CreateImage("Screen.png", ChartColors.DarkGreen);
        BgInfoOutputRegistry.Record(lower, upper);
        var resolved = new BgInfoGenerator(new ImageService(), new FakeWallpaperService())
            .ResolveOriginalImage(lower, null, "The current wallpaper", chosenByUser: false);
        if (aliases) Assert.Null(resolved);
        else Assert.True(BgInfoPaths.AreSame(upper, resolved!));
    }

    private static byte[] Pixels(string path) {
        using var image = BgInfoRasterImage.Load(path);
        return image.ToRgbaImage().Pixels;
    }

    private sealed class FixtureFolder : IDisposable {
        internal FixtureFolder() {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "bginfo-identity-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        internal string CreateImage(string name, ChartColor? color = null) {
            var path = System.IO.Path.Combine(Path, name);
            using var image = new BgInfoRasterImage();
            image.Create(path, 320, 180, color ?? ChartColors.Navy);
            image.Save(path);
            return path;
        }

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
