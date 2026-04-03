using System;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace PowerBGInfo.PowerShell;

/// <summary>Exports a BGInfo configuration to JSON.</summary>
/// <para>Writes a JSON file compatible with Invoke-BGInfo and the CLI.</para>
[Cmdlet(VerbsData.Export, "BGInfoConfiguration")]
[OutputType(typeof(string))]
public sealed class CmdletExportBGInfoConfiguration : PSCmdlet {
    /// <para>Configuration object to export.</para>
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public BgInfoConfiguration InputObject { get; set; } = null!;

    /// <para>Output path for the JSON configuration file.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public string Path { get; set; } = string.Empty;

    /// <para>Overwrite the output file if it exists.</para>
    [Parameter]
    public SwitchParameter Force { get; set; }

    /// <para>Return the output path.</para>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    /// <summary>Writes the JSON configuration file.</summary>
    protected override void ProcessRecord() {
        if (InputObject == null) {
            ThrowTerminatingError(new ErrorRecord(
                new ArgumentNullException(nameof(InputObject)),
                "BGInfoConfigurationMissing",
                ErrorCategory.InvalidArgument,
                null));
            return;
        }

        var fullPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(Path);
        if (string.IsNullOrWhiteSpace(fullPath)) {
            ThrowTerminatingError(new ErrorRecord(
                new ArgumentException($"Unable to resolve path {Path}."),
                "BGInfoConfigurationPathInvalid",
                ErrorCategory.InvalidArgument,
                Path));
            return;
        }

        if (File.Exists(fullPath) && !Force.IsPresent) {
            ThrowTerminatingError(new ErrorRecord(
                new IOException($"File already exists: {fullPath}"),
                "BGInfoConfigurationPathExists",
                ErrorCategory.ResourceExists,
                fullPath));
            return;
        }

        var directory = System.IO.Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        BgInfoConfigurationJson.Save(InputObject, fullPath);

        if (PassThru.IsPresent) {
            WriteObject(fullPath);
        }
    }
}
