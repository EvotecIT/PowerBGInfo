using System.Collections;
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
/// <para>A wallpaper must never fail or hang at logon because one source is slow or down. The script block runs in its own runspace; when it throws, returns nothing or exceeds -TimeoutSeconds, the cmdlet writes a warning and returns -Default instead.</para>
/// <para>Variables of the calling script are not visible inside the script block. Pass what it needs with -ArgumentList and read it with param().</para>
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
        var shell = System.Management.Automation.PowerShell.Create();
        shell.AddScript(ScriptBlock.ToString(), useLocalScope: true);
        foreach (var argument in ArgumentList ?? Array.Empty<object>()) shell.AddArgument(argument);

        var output = new PSDataCollection<PSObject>();
        IAsyncResult handle;
        try {
            handle = shell.BeginInvoke<PSObject, PSObject>(null, output);
        } catch (Exception exception) {
            shell.Dispose();
            Fallback($"could not start: {exception.Message}");
            return;
        }

        if (!handle.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(TimeoutSeconds))) {
            // Stop in the background and release the runspace when it ends; a stuck source never blocks the wallpaper.
            try { shell.BeginStop(result => { try { shell.Dispose(); } catch (Exception) { } }, null); } catch (Exception) { }
            Fallback($"did not finish within {TimeoutSeconds} s");
            return;
        }

        using var completed = shell;
        try {
            shell.EndInvoke(handle);
        } catch (Exception exception) {
            Fallback(exception is RuntimeException runtime ? runtime.Message : exception.Message);
            return;
        }

        if (shell.HadErrors && output.Count == 0) {
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

    private void Fallback(string reason) {
        WriteWarning($"{Name} {reason}; using the default value.");
        WriteObject(Default);
    }
}
