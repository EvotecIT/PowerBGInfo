using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Lists the monitors PowerBGInfo renders for.</summary>
/// <para>Each object has the Index that New-BGInfo -MonitorIndex takes, the device name, the bounds in desktop coordinates, the backend display identity, whether it is the primary monitor, and the display scale when the backend reports one. Loop over the output to render one image per monitor.</para>
/// <para>The cmdlet only reads the display configuration, needs no elevation and works in Constrained Language Mode. It returns nothing when physical display geometry is unavailable, for example on a headless host or the current Linux adapters. Supply explicit background dimensions in those cases.</para>
/// <example>
/// <code>
/// foreach ($monitor in Get-BGInfoMonitor) {
///     New-BGInfo -MonitorIndex $monitor.Index -Target File `
///         -ConfigurationDirectory ([IO.Path]::GetTempPath()) `
///         -OutputFileName "wallpaper-$($monitor.Index).png" {
///         New-BGInfoBackground -Width $monitor.Width -Height $monitor.Height
///         New-BGInfoValue -Name 'Size' -Value "$($monitor.Width) x $($monitor.Height)"
///     }
/// }
/// </code>
/// </example>
[Cmdlet(VerbsCommon.Get, "BGInfoMonitor")]
[OutputType(typeof(BgInfoMonitor))]
public sealed class CmdletGetBGInfoMonitor : PSCmdlet {
    /// <para>Return only the primary monitor.</para>
    [Parameter]
    public SwitchParameter Primary { get; set; }

    /// <summary>Emits the monitors.</summary>
    protected override void EndProcessing() {
        foreach (var monitor in BgInfoMonitorProvider.GetMonitors()) {
            if (Primary.IsPresent && !monitor.Primary) continue;
            WriteObject(monitor);
        }
    }
}
