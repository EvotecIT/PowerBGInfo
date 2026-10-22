using ChartForgeX.Primitives;

namespace PowerBGInfo.Tests;

public sealed class BgInfoFileIdentityTests {
    [Theory]
    [InlineData(@"c:\Images\photo.png", @"C:\Images\photo.png")]
    [InlineData(@"\\?\c:\Images\photo.png", @"C:\Images\photo.png")]
    [InlineData(@"\\.\c:\Images\photo.png", @"C:\Images\photo.png")]
    [InlineData(@"\\host\Share\Images\photo.png", @"\\HOST\SHARE\Images\photo.png")]
    [InlineData(@"\\?\UNC\Host\share\Images\photo.png", @"\\HOST\SHARE\Images\photo.png")]
    [InlineData(@"\\.\UNC\Host\share\Images\photo.png", @"\\HOST\SHARE\Images\photo.png")]
    public void WindowsRootAliasesHaveOneComparisonIdentity(string path, string expected) {
        Assert.Equal(expected, BgInfoPaths.NormalizeWindowsIdentity(path));
    }

    [Fact]
    public void WindowsRootNormalizationRetainsCaseSensitiveDirectoryAndFileNames() {
        Assert.NotEqual(BgInfoPaths.NormalizeWindowsIdentity(@"\\host\share\Images\photo.png"),
            BgInfoPaths.NormalizeWindowsIdentity(@"\\HOST\SHARE\images\photo.png"));
        Assert.NotEqual(BgInfoPaths.NormalizeWindowsIdentity(@"\\?\c:\Images\Photo.png"),
            BgInfoPaths.NormalizeWindowsIdentity(@"C:\Images\photo.png"));
    }

    [Theory]
    [InlineData("x", 220)]
    [InlineData("é", 120)]
    public void LongValidNamesRenderAndPreserveTheirOriginal(string letter, int count) {
        using var folder = new FixtureFolder();
        var source = folder.CreateImage("base.png");
        var name = string.Concat(Enumerable.Repeat(letter, count)) + ".png";
        var output = Path.Combine(folder.Path, name);
        var originalPixels = Pixels(source);
        using (var image = BgInfoRasterImage.Load(source)) image.Save(output);
        Assert.Equal(originalPixels, Pixels(output));

        var config = new BgInfoConfiguration {
            FilePath = output, ConfigurationDirectory = folder.Path, OutputFileName = name,
            Target = BgInfoTarget.File, Color = ChartColors.White, FontSize = 18
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Label, Name = "Preserve the original" });
        var generator = new BgInfoGenerator(new ImageService(), new FakeWallpaperService());
        var rendered = Pixels(generator.Generate(config));
        Assert.NotEqual(originalPixels, rendered);
        Assert.True(BgInfoOutputRegistry.TryGetSource(output, out var backup));
        Assert.NotNull(backup);
        Assert.Equal(originalPixels, Pixels(backup!));
        Assert.Equal(rendered, Pixels(generator.Generate(config)));
        Assert.Empty(Directory.GetFiles(folder.Path).Except(new[] {
            source, output, backup!, Path.Combine(folder.Path, BgInfoOutputRegistry.FileName),
            Path.Combine(folder.Path, BgInfoOutputRegistry.FileName + ".lock")
        }));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SymbolicAliasesPreserveTheOriginalAcrossRenderingRuns(bool backgroundImage, bool linkedTargetParent) {
        if (OperatingSystem.IsWindows()) return; // Windows junction acceptance is a native qualification lane.
        using var folder = new FixtureFolder();
        var images = Path.Combine(folder.Path, "images");
        Directory.CreateDirectory(images);
        var original = folder.CreateImage(Path.Combine("images", "photo.png"));
        var pixels = Pixels(original);
        var aliasDirectory = Path.Combine(folder.Path, "alias");
        var target = images;
        if (linkedTargetParent) {
            var parentAlias = Path.Combine(folder.Path, "parent");
            Directory.CreateSymbolicLink(parentAlias, folder.Path);
            target = Path.Combine(parentAlias, "images");
        }
        Directory.CreateSymbolicLink(aliasDirectory, target);
        var output = Path.Combine(aliasDirectory, "photo.png");
        Assert.True(BgInfoPaths.AreSame(original, output));
        Assert.Equal(BgInfoPaths.Comparer.GetHashCode(original), BgInfoPaths.Comparer.GetHashCode(output));

        string Render() {
            var config = new BgInfoConfiguration {
                FilePath = backgroundImage ? string.Empty : original,
                Background = backgroundImage ? new BgInfoBackground { Width = 320, Height = 180, ImagePath = original } : null,
                ConfigurationDirectory = aliasDirectory, OutputFileName = "photo.png", Target = BgInfoTarget.File,
                Color = ChartColors.White, FontSize = 18
            };
            config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Label, Name = "Safe original" });
            return new BgInfoGenerator(new ImageService(), new FakeWallpaperService()).Generate(config);
        }
        var first = Render();
        var rendered = Pixels(first);
        var backup = Assert.Single(Directory.GetFiles(images, "*.PowerBGInfo-original-*.png"));
        Assert.Equal(pixels, Pixels(backup));
        Assert.Equal(rendered, Pixels(Render()));
        Assert.True(BgInfoOutputRegistry.TryGetSource(first, out var source));
        Assert.True(BgInfoPaths.AreSame(backup, source!));
    }

    [Fact]
    public void ReplacingAHardLinkedOutputKeepsItsSourceImageUnchanged() {
        if (OperatingSystem.IsWindows()) return;
        using var folder = new FixtureFolder();
        var original = folder.CreateImage("photo.png");
        var pixels = Pixels(original);
        var output = Path.Combine(folder.Path, "output.png");
        Assert.Equal(0, CreateHardLink(original, output));
        var config = new BgInfoConfiguration {
            FilePath = original, ConfigurationDirectory = folder.Path, OutputFileName = "output.png",
            Target = BgInfoTarget.File, Color = ChartColors.White, FontSize = 18
        };
        config.Entries.Add(new BgInfoEntry { Type = BgInfoEntryType.Label, Name = "Independent output" });
        var generator = new BgInfoGenerator(new ImageService(), new FakeWallpaperService());
        var first = generator.Generate(config);
        var rendered = Pixels(first);
        Assert.Equal(pixels, Pixels(original));
        Assert.NotEqual(pixels, rendered);
        Assert.Equal(rendered, Pixels(generator.Generate(config)));
        Assert.Equal(pixels, Pixels(original));
        Assert.Empty(Directory.GetFiles(folder.Path).Except(new[] {
            original, output, Path.Combine(folder.Path, BgInfoOutputRegistry.FileName),
            Path.Combine(folder.Path, BgInfoOutputRegistry.FileName + ".lock")
        }));
    }

    [System.Runtime.InteropServices.DllImport("libc", EntryPoint = "link", SetLastError = true)]
    private static extern int CreateHardLink(string existingPath, string newPath);

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
    public void ReadOnlyVolumeFilteringDoesNotRequireListingEveryParentDirectory() {
        if (OperatingSystem.IsWindows()) return;
        using var folder = new FixtureFolder();
        var parent = Path.Combine(folder.Path, "searchable-only");
        var child = Path.Combine(parent, "volume");
        Directory.CreateDirectory(child);
        var originalMode = File.GetUnixFileMode(parent);
        try {
            File.SetUnixFileMode(parent, UnixFileMode.UserExecute);
            Assert.Equal(child, BgInfoVariableResolver.NormalizeVolumeIdentity(child));
        } finally {
            File.SetUnixFileMode(parent, originalMode);
        }
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
