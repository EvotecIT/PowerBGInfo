using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace PowerBGInfo.Cli;

/// <summary>
/// Owns this configuration invocation's descendants until output draining finishes.
/// The wrapper waits for authorization before running user code. Windows uses a job;
/// POSIX keeps the wrapper alive as its process-group anchor, including after its
/// script worker exits, so cleanup never signals an identity that could be reused.
/// </summary>
internal sealed class ConfigurationScriptLifetime : IDisposable {
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "PowerBGInfo", "lifetime-" + Guid.NewGuid().ToString("N"));
    private readonly bool _windows = Path.DirectorySeparatorChar == '\\';
    private SafeFileHandle? _job;
    private Process? _process;
    private bool _authorized;
    private bool _jobAssigned;
    private bool _terminated;

    internal ConfigurationScriptLifetime(ProcessStartInfo startInfo) {
        Directory.CreateDirectory(_directory);
        startInfo.RedirectStandardInput = true;
        startInfo.EnvironmentVariables["POWERBGINFO_CONFIGURATION_LIFETIME"] = _directory;
        if (!_windows) return;

        _job = CreateJobObject(IntPtr.Zero, null);
        if (_job.IsInvalid) {
            var error = NativeError("CreateJobObject");
            Dispose();
            throw error;
        }
        var limits = new JobLimits { Basic = new BasicJobLimits { LimitFlags = 0x2000 } }; // KILL_ON_JOB_CLOSE
        if (!SetInformationJobObject(_job, 9, ref limits, (uint)Marshal.SizeOf<JobLimits>())) {
            var error = NativeError("SetInformationJobObject");
            Dispose();
            throw error;
        }
    }

    internal void Attach(Process process) {
        _process = process;
        // User code cannot run until WaitForCompletion sends the authorization line.
        if (_job != null) {
            if (!AssignProcessToJobObject(_job, process.Handle)) throw NativeError("AssignProcessToJobObject");
            _jobAssigned = true;
        }
    }

    internal int WaitForCompletion(Stopwatch elapsed, TimeSpan timeout) {
        var process = _process ?? throw new InvalidOperationException("The configuration process has not started.");
        while (true) {
            var readyPath = Path.Combine(_directory, "ready");
            if (!_authorized && File.Exists(readyPath)) {
                if (!int.TryParse(File.ReadAllText(readyPath), out var owner) || owner != process.Id) {
                    throw new InvalidOperationException("The configuration process returned an invalid lifetime identity.");
                }
                // Readiness already proves ownership. Record it before writing:
                // a failed flush must still terminate a worker that received run.
                _authorized = true;
                process.StandardInput.WriteLine("run");
                process.StandardInput.Flush();
            }

            if (!_windows) {
                var resultPath = Path.Combine(_directory, "exit");
                if (File.Exists(resultPath)) {
                    if (!int.TryParse(File.ReadAllText(resultPath), out var exitCode)) {
                        throw new InvalidOperationException("The configuration process returned an invalid exit code.");
                    }
                    return exitCode;
                }
            }
            if (process.WaitForExit(0)) return process.ExitCode;
            var remaining = RemainingMilliseconds(elapsed, timeout);
            if (remaining == 0) throw Deadline(timeout);
            Thread.Sleep(Math.Min(remaining, 20));
        }
    }

    /// <summary>Stops the owned scope before closing streams that descendants may still hold open.</summary>
    internal void Terminate() {
        if (_terminated || _process == null) return;
        if (_job != null) {
            if (!TerminateJobObject(_job, 1)) throw NativeError("TerminateJobObject");
        } else if (_authorized && !_process.HasExited) {
            // The supervisor deliberately waits for us after recording the worker's
            // exit. Its live PID reserves this group's identity until this signal.
            if (Kill(-_process.Id, 9) != 0 && Marshal.GetLastWin32Error() != 3) {
                throw NativeError("kill(configuration process group)");
            }
        } else if (!_process.HasExited) {
            // Before authorization the wrapper cannot have started the script worker.
            KillUnstartedWrapper();
        }
        // A failed Windows job assignment must still stop its unstarted wrapper.
        if (_job != null && !_jobAssigned && !_process.HasExited) KillUnstartedWrapper();
        _terminated = true;
    }

    private void KillUnstartedWrapper() {
        try { _process!.Kill(); }
        catch (InvalidOperationException) when (_process!.HasExited) { /* It exited between the check and kill. */ }
    }

    internal static int RemainingMilliseconds(Stopwatch elapsed, TimeSpan timeout)
        => Math.Max(0, (int)Math.Ceiling((timeout - elapsed.Elapsed).TotalMilliseconds));

    internal static TimeoutException Deadline(TimeSpan timeout)
        => new TimeoutException($"The PowerShell configuration script exceeded its {timeout.TotalSeconds:G} second deadline.");

    public void Dispose() {
        try {
            Terminate();
        } finally {
            _job?.Dispose();
            try { Directory.Delete(_directory, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    private static Win32Exception NativeError(string operation) => new Win32Exception(Marshal.GetLastWin32Error(), operation + " failed.");

    [StructLayout(LayoutKind.Sequential)]
    private struct BasicJobLimits {
        internal long PerProcessTime, PerJobTime;
        internal uint LimitFlags;
        internal UIntPtr MinimumWorkingSet, MaximumWorkingSet;
        internal uint ActiveProcessLimit;
        internal UIntPtr Affinity;
        internal uint PriorityClass, SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JobLimits {
        internal BasicJobLimits Basic;
        internal ulong ReadOperations, WriteOperations, OtherOperations, ReadBytes, WriteBytes, OtherBytes;
        internal UIntPtr ProcessMemoryLimit, JobMemoryLimit, PeakProcessMemory, PeakJobMemory;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateJobObjectW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateJobObject(IntPtr attributes, string? name);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetInformationJobObject(SafeFileHandle job, uint informationClass, ref JobLimits limits, uint length);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AssignProcessToJobObject(SafeFileHandle job, IntPtr process);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TerminateJobObject(SafeFileHandle job, uint exitCode);
    [DllImport("libc", EntryPoint = "kill", SetLastError = true)]
    private static extern int Kill(int processGroup, int signal);
}
