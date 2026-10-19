using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PowerBGInfo;

/// <summary>
/// Isolates blocking WMI setup and cleanup from the caller's deadline. A stalled provider retains
/// at most one background worker; subsequent probes fail closed until it releases its resources.
/// </summary>
internal sealed class SystemInfoQueryExecutor {
    private readonly SemaphoreSlim _capacity = new(1, 1);
    private readonly object _gate = new();
    private bool _stalled;

    internal T? Run<T>(Func<T> operation, TimeSpan timeout) where T : class {
        var timer = Stopwatch.StartNew();
        lock (_gate) if (_stalled) return null;
        if (!_capacity.Wait(timeout)) return null;
        var completion = new TaskCompletionSource<T?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var worker = new Thread(() => {
            T? result = null;
            try {
                result = operation();
            } catch {
                // Unavailable namespaces, permissions and provider failures leave facts unknown.
            } finally {
                lock (_gate) {
                    _stalled = false;
                    _capacity.Release();
                    completion.SetResult(result);
                }
            }
        }) { IsBackground = true, Name = "PowerBGInfo WMI query" };
        try {
            worker.Start();
        } catch {
            _capacity.Release();
            return null;
        }
        var remaining = timeout - timer.Elapsed;
        if (remaining > TimeSpan.Zero && completion.Task.Wait(remaining)) return completion.Task.Result;
        lock (_gate) {
            if (!completion.Task.IsCompleted) _stalled = true;
        }
        return null;
    }
}
