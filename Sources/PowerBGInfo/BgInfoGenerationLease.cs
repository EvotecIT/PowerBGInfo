using System.Security.Principal;
using System.Threading;

namespace PowerBGInfo;

/// <summary>Owns one synchronous generation transaction for the effective Windows user.</summary>
internal sealed class BgInfoGenerationLease : IDisposable {
    private Mutex? _mutex;

    private BgInfoGenerationLease(Mutex mutex) => _mutex = mutex;

    internal static string MutexName(string userSid) => @"Global\Evotec.PowerBGInfo.Generation." + userSid;

    internal static BgInfoGenerationLease Acquire() {
        using var identity = WindowsIdentity.GetCurrent();
        var sid = identity.User?.Value ?? throw new InvalidOperationException("Cannot determine the Windows user for image generation.");
        var mutex = new Mutex(false, MutexName(sid));
        try {
            try {
                if (!mutex.WaitOne(TimeSpan.FromMinutes(5))) {
                    throw new TimeoutException("Another PowerBGInfo run for this user has not finished within five minutes.");
                }
            } catch (AbandonedMutexException) {
                // WaitOne grants ownership when the previous rendering process exited.
            }
            return new BgInfoGenerationLease(mutex);
        } catch {
            mutex.Dispose();
            throw;
        }
    }

    public void Dispose() {
        var mutex = _mutex;
        _mutex = null;
        if (mutex == null) return;
        try { mutex.ReleaseMutex(); }
        finally { mutex.Dispose(); }
    }
}
