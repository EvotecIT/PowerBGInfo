using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Lists the monitors PowerBGInfo renders for.</summary>
/// <para>Each object has the Index that New-BGInfo -MonitorIndex takes, the device name, the bounds in desktop coordinates, whether it is the primary monitor, and the display scale when Windows reports one. Loop over the output to render one image per monitor.</para>
/// <para>The cmdlet only reads the display configuration, needs no elevation and works in Constrained Language Mode. It returns nothing when no monitor can be read, for example in a session without a desktop.</para>
/// <example>
/// <code>
/// foreach ($monitor in Get-BGInfoMonitor) {
///     New-BGInfo -MonitorIndex $monitor.Index -Target File `
///         -ConfigurationDirectory C:\Temp\BGInfo `
///         -OutputFileName "wallpaper-$($monitor.Index).png" {
///         New-BGInfoBackground
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
