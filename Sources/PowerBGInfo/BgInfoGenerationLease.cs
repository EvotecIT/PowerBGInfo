using System.Diagnostics;
using System.Threading;

namespace PowerBGInfo;

/// <summary>Owns one synchronous generation transaction for the effective Windows user.</summary>
internal sealed class BgInfoGenerationLease : IDisposable {
    [ThreadStatic] private static FileStream? _currentLock;
    [ThreadStatic] private static int _depth;
    private readonly int _threadId = Thread.CurrentThread.ManagedThreadId;
    private bool _disposed;

    private BgInfoGenerationLease() { }

    internal static string LockPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PowerBGInfo", "generation.lock");

    internal static BgInfoGenerationLease Acquire() {
        if (_depth != 0) {
            _depth++;
            return new BgInfoGenerationLease();
        }
        var path = LockPath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var timeout = TimeSpan.FromMinutes(5);
        var wait = Stopwatch.StartNew();
        while (true) {
            if (wait.Elapsed >= timeout) throw TimedOut();
            try {
                var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                if (wait.Elapsed >= timeout) {
                    stream.Dispose();
                    throw TimedOut();
                }
                _currentLock = stream;
                _depth = 1;
                return new BgInfoGenerationLease();
            } catch (IOException ex) when ((ex.HResult & 0xffff) is 32 or 33) {
                // Sharing/lock violations mean another process owns the transaction.
                // Other I/O failures should remain actionable instead of waiting five minutes.
                var remaining = timeout - wait.Elapsed;
                if (remaining <= TimeSpan.Zero) throw TimedOut();
                Thread.Sleep(remaining < TimeSpan.FromMilliseconds(100) ? remaining : TimeSpan.FromMilliseconds(100));
            }
        }
    }

    private static TimeoutException TimedOut() => new(
        "Another PowerBGInfo run for this user has not finished within five minutes.");

    public void Dispose() {
        if (_disposed) return;
        if (Thread.CurrentThread.ManagedThreadId != _threadId)
            throw new InvalidOperationException("The generation lease must be released on its owning thread.");
        _disposed = true;
        if (--_depth != 0) return;
        var stream = _currentLock;
        _currentLock = null;
        stream?.Dispose();
    }
}
