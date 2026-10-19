using System.Collections;
using System.Diagnostics;
using System.Threading;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Returns read-only facts about this machine as one plain object.</summary>
/// <para>Uptime, OS and build, IPv4, fixed disks, last installed update, Defender and BitLocker state, and the console user. Every probe is read-only and fails on its own: a missing WMI class, a timeout or a lack of elevation leaves that field Unknown or empty instead of failing the wallpaper.</para>
/// <para>The object is plain data. Replace it with your own function that returns the same property names and the presentation code keeps working.</para>
/// <example>
/// <code>
/// $facts = Get-BGInfoSystemInfo
/// New-BGInfoValue -Name 'Uptime' -Value $facts.UptimeText
/// $facts.Disks | New-BGInfoTable -Title 'Disks' -Property Drive, FreeGB, UsedPercent
/// </code>
/// </example>
[Cmdlet(VerbsCommon.Get, "BGInfoSystemInfo")]
[OutputType(typeof(BgInfoSystemInfo))]
public sealed class CmdletGetBGInfoSystemInfo : PSCmdlet {
    /// <summary>Emits the snapshot.</summary>
    protected override void EndProcessing() => WriteObject(SystemInfoProvider.GetSnapshot());
}

/// <summary>Runs your data-collection code with a time limit and a fallback value.</summary>
/// <para>A wallpaper must never fail or hang at logon because one source is slow or down. The script block runs in its own runspace; when it reports an error, returns nothing or exceeds -TimeoutSeconds, the cmdlet writes a warning and returns -Default instead. Partial output from a failed source is discarded.</para>
/// <para>Variables of the calling script are not visible inside the script block. Pass what it needs with -ArgumentList and read it with param().</para>
/// <para>Run only trusted code: the runspace is not a security sandbox. Cancellation is cooperative. One invocation, including its pending stop and cleanup, is allowed per process; concurrent callers wait within their own time limit and use the fallback if capacity is still occupied.</para>
/// <example>
/// <code>
/// $patch = Invoke-BGInfoDataSource -Name 'Patch level' -TimeoutSeconds 10 -Default 'unknown' -ScriptBlock {
///     (Get-HotFix | Sort-Object InstalledOn -Descending | Select-Object -First 1).HotFixID
/// }
/// $tickets = Invoke-BGInfoDataSource -Name 'Open tickets' -Default 0 -ArgumentList $uri -ScriptBlock {
///     param($uri) (Invoke-RestMethod -Uri $uri -TimeoutSec 5).count
/// }
/// </code>
/// </example>
[Cmdlet(VerbsLifecycle.Invoke, "BGInfoDataSource")]
[OutputType(typeof(object))]
public sealed class CmdletInvokeBGInfoDataSource : PSCmdlet {
    private static readonly SemaphoreSlim InvocationCapacity = new(1, 1);
    private readonly CancellationTokenSource _stop = new();
    private readonly object _stopGate = new();
    private bool _finished;
    /// <para>Your data-collection code.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public ScriptBlock ScriptBlock { get; set; } = null!;

    /// <para>Name used in warnings, for example 'Patch level'.</para>
    [Parameter]
    public string Name { get; set; } = "Data source";

    /// <para>Value returned when the code fails, returns nothing or runs out of time.</para>
    [Parameter]
    [AllowNull]
    public object? Default { get; set; }

    /// <para>Time limit in seconds, from 1 to 600.</para>
    [Parameter]
    [ValidateRange(1, 600)]
    public int TimeoutSeconds { get; set; } = 15;

    /// <para>Arguments passed to the script block's param() block.</para>
    [Parameter]
    public object[] ArgumentList { get; set; } = Array.Empty<object>();

    /// <summary>Runs the code and emits its output or the fallback.</summary>
    protected override void EndProcessing() {
        try { RunWithCapacity(_stop.Token); }
        finally {
            lock (_stopGate) {
                _finished = true;
                _stop.Dispose();
            }
        }
    }

    /// <summary>Interrupts both waits without releasing resources still owned by the provider.</summary>
    protected override void StopProcessing() {
        lock (_stopGate) if (!_finished) _stop.Cancel();
    }

    private void RunWithCapacity(CancellationToken cancellation) {
        var timer = Stopwatch.StartNew();
        var timeout = TimeSpan.FromSeconds(TimeoutSeconds);
        bool acquired;
        try { acquired = InvocationCapacity.Wait(timeout, cancellation); }
        catch (OperationCanceledException) { return; }
        if (!acquired) {
            Fallback("could not start while another data source is still running or cleaning up");
            return;
        }

        System.Management.Automation.PowerShell? shell = null;
        PSDataCollection<PSObject>? output = null;
        var releaseHere = true;
        try {
            Run();
        } finally {
            if (releaseHere) {
                try { output?.Dispose(); shell?.Dispose(); }
                finally { InvocationCapacity.Release(); }
            }
        }

        void Run() {
            if (!CanStart()) return;
            shell = System.Management.Automation.PowerShell.Create();
            shell.AddScript(ScriptBlock.ToString(), useLocalScope: true);
            foreach (var argument in ArgumentList ?? Array.Empty<object>()) shell.AddArgument(argument);

            output = new PSDataCollection<PSObject>();
            IAsyncResult handle;
            if (!CanStart()) return;
            try {
                handle = shell.BeginInvoke<PSObject, PSObject>(null, output);
            } catch (Exception exception) {
                Fallback($"could not start: {exception.Message}");
                return;
            }

            var remaining = timeout - timer.Elapsed;
            if (remaining <= TimeSpan.Zero || WaitHandle.WaitAny(new[] { handle.AsyncWaitHandle, cancellation.WaitHandle }, remaining) != 0) {
                // Retain the single capacity slot until cooperative stop, completion and
                // disposal actually finish. A stuck provider cannot accumulate runspaces.
                var expiredShell = shell;
                var expiredOutput = output;
                releaseHere = false;
                ThreadPool.QueueUserWorkItem(_ => {
                    try {
                        try { expiredShell.Stop(); } catch (Exception) { }
                        try { expiredShell.EndInvoke(handle); } catch (Exception) { }
                    } finally {
                        try { expiredOutput.Dispose(); expiredShell.Dispose(); } catch (Exception) { }
                        finally { InvocationCapacity.Release(); }
                    }
                });
                if (!cancellation.IsCancellationRequested) Fallback($"did not finish within {TimeoutSeconds} s");
                return;
            }

            try {
                shell.EndInvoke(handle);
            } catch (Exception exception) {
                Fallback(exception is RuntimeException runtime ? runtime.Message : exception.Message);
                return;
            }

            if (shell.HadErrors) {
                var error = shell.Streams.Error.Count > 0 ? shell.Streams.Error[0].ToString() : "failed";
                Fallback(error);
                return;
            }
            if (output.Count == 0) {
                Fallback("returned nothing");
                return;
            }
            foreach (var item in output) WriteObject(item);
        }

        bool CanStart() {
            if (cancellation.IsCancellationRequested) return false;
            if (timer.Elapsed < timeout) return true;
            Fallback($"did not start within {TimeoutSeconds} s");
            return false;
        }
    }

    private void Fallback(string reason) {
        WriteWarning($"{Name} {reason}; using the default value.");
        WriteObject(Default);
    }
}
