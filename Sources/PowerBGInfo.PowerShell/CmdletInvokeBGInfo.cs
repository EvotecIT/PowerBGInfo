using System.Management.Automation;

namespace PowerBGInfo.PowerShell;

/// <summary>Runs BGInfo from a JSON configuration file.</summary>
[Cmdlet(VerbsLifecycle.Invoke, "BGInfo")]
[OutputType(typeof(string))]
public sealed class CmdletInvokeBGInfo : PSCmdlet {
    /// <para>Filesystem path to the JSON configuration file, relative to the current PowerShell location.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public string Path { get; set; } = string.Empty;

    /// <para>Override output file name. Relative output paths resolve against ConfigurationDirectory; qualified filesystem paths use PowerShell path resolution.</para>
    [Parameter]
    public string OutputFileName { get; set; } = string.Empty;

    /// <para>Override configuration output directory, relative to the current PowerShell location.</para>
    [Parameter]
    public string ConfigurationDirectory { get; set; } = string.Empty;

    /// <para>Override monitor index.</para>
    [Parameter]
    public int MonitorIndex { get; set; }

    /// <para>Override output target.</para>
    [Parameter]
    public BgInfoTarget Target { get; set; }

    /// <para>Generate the image without applying it to the wallpaper.</para>
    [Parameter]
    public SwitchParameter NoApply { get; set; }

    /// <summary>Loads the configuration and runs BGInfo.</summary>
    protected override void EndProcessing() {
        var config = BgInfoConfigurationJson.Load(PowerShellFileSystemPath.Resolve(SessionState, Path, nameof(Path)));

        if (MyInvocation.BoundParameters.ContainsKey(nameof(OutputFileName))) {
            config.OutputFileName = PowerShellFileSystemPath.ResolveOutputFileName(SessionState, OutputFileName);
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(ConfigurationDirectory))) {
            config.ConfigurationDirectory = PowerShellFileSystemPath.Resolve(SessionState, ConfigurationDirectory, nameof(ConfigurationDirectory));
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(MonitorIndex))) {
            config.MonitorIndex = MonitorIndex;
        }
        if (MyInvocation.BoundParameters.ContainsKey(nameof(Target))) {
            config.Target = Target;
        }
        if (NoApply.IsPresent) {
            config.Target = BgInfoTarget.File;
        }

        var path = BgInfoRunner.Run(config, WriteWarning);
        WriteObject(path);
    }
}
