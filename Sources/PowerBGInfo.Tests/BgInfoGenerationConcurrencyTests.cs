using ChartForgeX.Primitives;
using DesktopManager;

namespace PowerBGInfo.Tests;

public sealed class BgInfoGenerationConcurrencyTests {
    [Fact]
    public async Task GenerationDoesNotOverwriteAnImageWhileTheSameUsersApplyIsRunning() {
        var folder = Path.Combine(Path.GetTempPath(), "pbg-generation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        using var applying = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        using var secondStarted = new ManualResetEventSlim();
        using var secondApplied = new ManualResetEventSlim();
        var service = new ApplyBarrierService(applying, release, secondApplied);
        var generator = new BgInfoGenerator(new ImageService(), service);
        Task<string>? first = null;
        Task<string>? second = null;
        try {
            first = Task.Factory.StartNew(() => generator.Generate(Configuration(folder, ChartColors.Navy)),
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Assert.True(applying.Wait(TimeSpan.FromSeconds(5)));
            var initial = File.ReadAllBytes(Path.Combine(folder, "wallpaper.png"));
            second = Task.Factory.StartNew(() => {
                secondStarted.Set();
                return generator.Generate(Configuration(folder, ChartColors.DarkRed));
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Assert.True(secondStarted.Wait(TimeSpan.FromSeconds(5)));
            Assert.False(secondApplied.Wait(TimeSpan.FromMilliseconds(250)), "A second render applied while the first apply was still running.");
            Assert.Equal(initial, File.ReadAllBytes(Path.Combine(folder, "wallpaper.png")));
            release.Set();
            var outputs = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(secondApplied.IsSet);
            Assert.False(initial.SequenceEqual(File.ReadAllBytes(outputs[1])));
        } finally {
            release.Set();
            if (first != null && second != null) await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task FailedApplicationReleasesGenerationForTheNextThread() {
        var folder = Path.Combine(Path.GetTempPath(), "pbg-generation-failure-" + Guid.NewGuid().ToString("N"));
        try {
            var service = new CallbackService(() => throw new InvalidOperationException("Controlled apply failure"));
            var generator = new BgInfoGenerator(new ImageService(), service);
            Assert.Throws<InvalidOperationException>(() => generator.Generate(Configuration(folder, ChartColors.Navy)));
            var config = Configuration(folder, ChartColors.DarkRed);
            config.Target = BgInfoTarget.File;
            var next = Task.Factory.StartNew(() => generator.Generate(config), CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
            var output = await next.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(File.Exists(output));
        } finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }

    [Fact]
    public void SameThreadNestedGenerationKeepsTheOuterTransactionUsable() {
        var folder = Path.Combine(Path.GetTempPath(), "pbg-generation-nested-" + Guid.NewGuid().ToString("N"));
        try {
            var inner = Configuration(folder, ChartColors.DarkRed);
            inner.OutputFileName = "nested.png";
            inner.Target = BgInfoTarget.File;
            var service = new CallbackService(() => new BgInfoGenerator(new ImageService(), new FakeWallpaperService()).Generate(inner));
            var output = new BgInfoGenerator(new ImageService(), service).Generate(Configuration(folder, ChartColors.Navy));
            Assert.True(File.Exists(output));
            Assert.True(File.Exists(Path.Combine(folder, "nested.png")));
        } finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }

    [Fact]
    public void DifferentWindowsUsersHaveIndependentCrossSessionMutexNames() {
        var first = BgInfoGenerationLease.MutexName("S-1-5-21-1001");
        var second = BgInfoGenerationLease.MutexName("S-1-5-21-1002");
        Assert.StartsWith(@"Global\", first);
        Assert.NotEqual(first, second);
    }

    private sealed class CallbackService : FakeWallpaperService, IWallpaperService {
        private readonly Action _callback;
        public CallbackService(Action callback) => _callback = callback;
        public new void SetLogonWallpaper(string path) => _callback();
    }

    private static BgInfoConfiguration Configuration(string folder, ChartColor color) => new() {
        ConfigurationDirectory = folder,
        OutputFileName = "wallpaper.png",
        Target = BgInfoTarget.LogonScreen,
        Background = new BgInfoBackground { Width = 120, Height = 80, Color = color }
    };

    private sealed class ApplyBarrierService : IWallpaperService {
        private readonly ManualResetEventSlim _applying;
        private readonly ManualResetEventSlim _release;
        private readonly ManualResetEventSlim _secondApplied;
        private int _calls;
        public ApplyBarrierService(ManualResetEventSlim applying, ManualResetEventSlim release, ManualResetEventSlim secondApplied) {
            _applying = applying; _release = release; _secondApplied = secondApplied;
        }
        public void SetLogonWallpaper(string path) {
            if (Interlocked.Increment(ref _calls) == 1) {
                _applying.Set();
                if (!_release.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Test apply barrier expired.");
            } else _secondApplied.Set();
        }
        public void SetWallpaper(int index, string path, DesktopWallpaperPosition position) => SetLogonWallpaper(path);
        public void SetWallpaperForAllUsers(string path, DesktopWallpaperPosition position, bool includeDefaultUserProfile) => SetLogonWallpaper(path);
        public DesktopWallpaperSlideshow GetWallpaperSlideshow() => new();
        public void StartWallpaperSlideshow(IEnumerable<string> paths, DesktopWallpaperPosition position, DesktopSlideshowOptions options, uint tick) => SetLogonWallpaper(paths.First());
    }
}
