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

    internal T? Run<T>(Func<T> operation, TimeSpan timeout) where T : class {
        var timer = Stopwatch.StartNew();
        if (!_capacity.Wait(timeout)) return null;
        var completion = new TaskCompletionSource<T?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var worker = new Thread(() => {
            T? result = null;
            try {
                result = operation();
            } catch {
                // Unavailable namespaces, permissions and provider failures leave facts unknown.
            } finally {
                _capacity.Release();
                completion.SetResult(result);
            }
        }) { IsBackground = true, Name = "PowerBGInfo WMI query" };
        try {
            worker.Start();
        } catch {
            _capacity.Release();
            return null;
        }
        var remaining = timeout - timer.Elapsed;
        return remaining > TimeSpan.Zero && completion.Task.Wait(remaining) ? completion.Task.Result : null;
    }
}
