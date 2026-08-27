using System;
using Color = ChartForgeX.Primitives.ChartColor;
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
    [Parameter(ParameterSetName = "Template", Mandatory = true)]
    public string Name { get; set; } = string.Empty;

    /// <para>Explicit value to render.</para>
    [Parameter(ParameterSetName = "Values", Mandatory = true)]
    [Parameter(ParameterSetName = "Template", Mandatory = true)]
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
    public object? Color { get; set; }

    /// <para>Label font size override.</para>
    [Parameter]
    public float FontSize { get; set; }

    /// <para>Label font family override.</para>
    [Parameter]
    public string FontFamilyName { get; set; } = string.Empty;

    /// <para>Render the label with a bold font weight.</para>
    [Parameter]
    public SwitchParameter Bold { get; set; }

    /// <para>Underline the label.</para>
    [Parameter]
    public SwitchParameter Underline { get; set; }

    /// <para>Value color override.</para>
    [Parameter]
    public object? ValueColor { get; set; }

    /// <para>Value font size override.</para>
    [Parameter]
    public float ValueFontSize { get; set; }

    /// <para>Value font family override.</para>
    [Parameter]
    public string ValueFontFamilyName { get; set; } = string.Empty;

    /// <para>Render the value with a bold font weight.</para>
    [Parameter]
    public SwitchParameter ValueBold { get; set; }

    /// <para>Underline the value.</para>
    [Parameter]
    public SwitchParameter ValueUnderline { get; set; }

    /// <para>Variable name used to expand this entry multiple times.</para>
    [Parameter(ParameterSetName = "Template", Mandatory = true)]
    public string ForEach { get; set; } = string.Empty;

    /// <summary>Emits a BGInfo value entry.</summary>
    protected override void EndProcessing() {
        string finalValue = string.IsNullOrEmpty(BuiltinValue) ? Value : SystemInfoProvider.GetValue(BuiltinValue);
        bool isTemplate = string.Equals(ParameterSetName, "Template", StringComparison.OrdinalIgnoreCase);
        var entry = new BgInfoEntry
        {
            Type = BgInfoEntryType.Value,
            Name = string.IsNullOrEmpty(Name) ? BuiltinValue : Name,
            Value = isTemplate ? Value : finalValue,
            BuiltinValue = isTemplate ? null : string.IsNullOrEmpty(BuiltinValue) ? null : BuiltinValue,
            ForEach = isTemplate ? ForEach : null,
            Color = IsParameterBound(nameof(Color)) ? PowerShellColorConverter.ConvertRequired(Color, nameof(Color)) : null,
            FontSize = IsParameterBound(nameof(FontSize)) ? FontSize : null,
            FontFamilyName = IsParameterBound(nameof(FontFamilyName)) ? FontFamilyName : null,
            Bold = IsParameterBound(nameof(Bold)) ? Bold.IsPresent : null,
            Underline = IsParameterBound(nameof(Underline)) ? Underline.IsPresent : null,
            ValueColor = IsParameterBound(nameof(ValueColor)) ? PowerShellColorConverter.ConvertRequired(ValueColor, nameof(ValueColor)) : null,
            ValueFontSize = IsParameterBound(nameof(ValueFontSize)) ? ValueFontSize : null,
            ValueFontFamilyName = IsParameterBound(nameof(ValueFontFamilyName)) ? ValueFontFamilyName : null,
            ValueBold = IsParameterBound(nameof(ValueBold)) ? ValueBold.IsPresent : null,
            ValueUnderline = IsParameterBound(nameof(ValueUnderline)) ? ValueUnderline.IsPresent : null
        };
        WriteObject(entry);
    }

    private bool IsParameterBound(string name)
    {
        return MyInvocation.BoundParameters.ContainsKey(name);
    }
}
