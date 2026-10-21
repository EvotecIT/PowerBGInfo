using DesktopManager;

namespace PowerBGInfo.PowerShell;

/// <summary>Reports the current desktop backend and the wallpaper operations it supports.</summary>
/// <para>Reads capabilities without changing the desktop. File rendering works independently of desktop availability. Linux backends may support wallpaper changes while requiring explicit image dimensions.</para>
/// <example><code>Get-BGInfoCapability</code><para>Shows the selected Windows, macOS, GNOME, KDE, or unsupported backend and its limits.</para></example>
[Cmdlet(VerbsCommon.Get, "BGInfoCapability")]
[OutputType(typeof(DesktopCapabilities))]
public sealed class CmdletGetBGInfoCapability : PSCmdlet {
    /// <summary>Emits the shared desktop backend's capability report.</summary>
    protected override void EndProcessing() => WriteObject(new PortableDesktopService().GetCapabilities());
}
