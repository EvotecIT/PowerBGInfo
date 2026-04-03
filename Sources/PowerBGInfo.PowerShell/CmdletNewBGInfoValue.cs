using System.Drawing;
using System.Management.Automation;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;
/// <summary>Creates a BGInfo value entry.</summary>

[Cmdlet(VerbsCommon.New, "BGInfoValue", DefaultParameterSetName = "Values")]
[OutputType(typeof(BgInfoEntry))]
public class CmdletNewBGInfoValue : PSCmdlet {
    /// <para>Label text to render.</para>
    [Parameter(ParameterSetName = "Values", Mandatory = true)]
    [Parameter(ParameterSetName = "Builtin")]
    public string Name { get; set; } = string.Empty;

    /// <para>Explicit value to render.</para>
    [Parameter(ParameterSetName = "Values", Mandatory = true)]
    public string Value { get; set; } = string.Empty;

    /// <para>Built-in token to resolve to a value.</para>
    [Parameter(ParameterSetName = "Builtin", Mandatory = true)]
    [ValidateSet(
        "UserName",
        "HostName",
        "FullUserName",
        "CpuName",
        "CpuMaxClockSpeed",
        "CpuCores",
        "CpuLogicalCores",
        "RAMSize",
        "RAMSpeed",
        "RAMPartNumber",
        "BiosVersion",
        "BiosManufacturer",
        "BiosReleaseDate",
        "OSName",
        "OSVersion",
        "OSArchitecture",
        "OSBuild",
        "OSInstallDate",
        "OSLastBootUpTime",
        "UserDNSDomain",
        "FQDN",
        "IPv4Address",
        "IPv6Address"
    )]
    public string BuiltinValue { get; set; } = string.Empty;

    /// <para>Label color override.</para>
    [Parameter]
    public Color Color { get; set; }

    /// <para>Label font size override.</para>
    [Parameter]
    public float FontSize { get; set; }

    /// <para>Label font family override.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = string.Empty;

    /// <para>Value color override.</para>
    [Parameter]
    public Color ValueColor { get; set; }

    /// <para>Value font size override.</para>
    [Parameter]
    public float ValueFontSize { get; set; }

    /// <para>Value font family override.</para>
    [Parameter]
    public string ValueFontFamilyName { get; set; } = string.Empty;

    /// <summary>Emits a BGInfo value entry.</summary>
    protected override void EndProcessing() {
        string finalValue = string.IsNullOrEmpty(BuiltinValue) ? Value : SystemInfoProvider.GetValue(BuiltinValue);
        var entry = new BgInfoEntry
        {
            Type = BgInfoEntryType.Value,
            Name = string.IsNullOrEmpty(Name) ? BuiltinValue : Name,
            Value = finalValue,
            Color = IsParameterBound(nameof(Color)) ? Color : null,
            FontSize = IsParameterBound(nameof(FontSize)) ? FontSize : null,
            FontFamilyName = IsParameterBound(nameof(FontFamilyName)) ? FontFamilyName : null,
            ValueColor = IsParameterBound(nameof(ValueColor)) ? ValueColor : null,
            ValueFontSize = IsParameterBound(nameof(ValueFontSize)) ? ValueFontSize : null,
            ValueFontFamilyName = IsParameterBound(nameof(ValueFontFamilyName)) ? ValueFontFamilyName : null
        };
        WriteObject(entry);
    }

    private bool IsParameterBound(string name)
    {
        return MyInvocation.BoundParameters.ContainsKey(name);
    }
}
