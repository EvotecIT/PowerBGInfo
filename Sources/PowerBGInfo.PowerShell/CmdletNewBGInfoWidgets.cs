using System.Collections;
using System.Globalization;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a KPI panel: label, value, trend, status and optional history.</summary>
/// <para>Pass -WarningThreshold and -CriticalThreshold with a numeric -Value and the status follows the value.</para>
/// <example>
/// <code>
/// New-BGInfoMetric -Region Right -Label 'CPU' -Value 38 -Unit '%' -Symbol 'CPU' `
///     -History 52, 48, 44, 41, 38 -WarningThreshold 70 -CriticalThreshold 90 -Caption '5 minute trend'
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoMetric")]
[OutputType(typeof(BgInfoMetricWidget))]
public sealed class CmdletNewBGInfoMetric : BgInfoWidgetCmdlet {
    /// <para>Metric label, for example CPU.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public string Label { get; set; } = string.Empty;

    /// <para>Value to show. Numbers are also compared with the thresholds.</para>
    [Parameter(Mandatory = true, Position = 1)]
    [AllowEmptyString]
    public object? Value { get; set; }

    /// <para>.NET format for numeric values, for example N0 or 0.0.</para>
    [Parameter]
    public string Format { get; set; } = string.Empty;

    /// <para>Unit shown after the value.</para>
    [Parameter]
    public string Unit { get; set; } = string.Empty;

    /// <para>Short change text, for example +4 %.</para>
    [Parameter]
    public string Trend { get; set; } = string.Empty;

    /// <para>Supporting text under the value.</para>
    [Parameter]
    public string Caption { get; set; } = string.Empty;

    /// <para>Short badge text, for example CPU, RAM or SSD.</para>
    [Parameter]
    public string Symbol { get; set; } = string.Empty;

    /// <para>Explicit status: Ok, Warning, Critical or Info.</para>
    [Parameter]
    public BgInfoStatus Status { get; set; }

    /// <para>Recent values drawn beside the metric.</para>
    [Parameter]
    public double[] History { get; set; } = Array.Empty<double>();

    /// <para>Sparkline or Bars.</para>
    [Parameter]
    public BgInfoMetricHistoryKind HistoryKind { get; set; }

    /// <para>History scale maximum, for example 100 for percentages.</para>
    [Parameter]
    public double HistoryMaximum { get; set; }

    /// <para>Value at which the metric turns the warning status.</para>
    [Parameter]
    public double WarningThreshold { get; set; }

    /// <para>Value at which the metric turns the critical status.</para>
    [Parameter]
    public double CriticalThreshold { get; set; }

    /// <para>Above: higher values are worse. Below: lower values are worse.</para>
    [Parameter]
    public BgInfoThresholdDirection ThresholdDirection { get; set; }

    /// <summary>Emits the metric widget.</summary>
    protected override void EndProcessing() {
        var widget = Configure(new BgInfoMetricWidget());
        // Metric values are already large; only the theme scale applies by default.
        if (!IsBound(nameof(Scale))) widget.Scale = Math.Max(1d, Math.Min(4d, BgInfoThemeScope.Current?.Scale ?? 1d));
        var raw = BgInfoValueReader.Unwrap(Value);
        var number = BgInfoValueReader.Number(raw);
        widget.Label = Label;
        widget.NumericValue = number;
        widget.Value = number.HasValue && !string.IsNullOrWhiteSpace(Format)
            ? number.Value.ToString(Format, CultureInfo.CurrentCulture)
            : raw is double or float or decimal ? Convert.ToDouble(raw, CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.CurrentCulture) : BgInfoValueReader.Text(raw);
        widget.Unit = Unit;
        widget.Trend = Trend;
        widget.Caption = Caption;
        widget.Symbol = Symbol;
        widget.Status = Status;
        widget.History = History ?? Array.Empty<double>();
        widget.HistoryKind = HistoryKind;
        if (IsBound(nameof(HistoryMaximum))) widget.HistoryMaximum = HistoryMaximum;
        widget.Threshold = BgInfoThemeDefaults.Threshold(this, WarningThreshold, CriticalThreshold, ThresholdDirection);
        if (widget.Threshold != null && !number.HasValue) {
            WriteWarning($"Metric '{Label}' has thresholds but its value is not a number; the status is not evaluated.");
        }
        Emit(widget);
    }
}

/// <summary>Creates a table panel from objects or explicit rows.</summary>
/// <para>Pipe objects in and pick columns with -Property, or pass -Column and -Row. A -StatusColumn adds a colored dot for values such as OK, Warning or Failed.</para>
/// <example>
/// <code>
/// Get-Process | Sort-Object CPU -Descending | Select-Object -First 5 |
///     New-BGInfoTable -Region Right -Title 'Top processes' -Property Name, Id, @{ Name = 'CPU s'; Expression = { [math]::Round($_.CPU) } }
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoTable", DefaultParameterSetName = "Objects")]
[OutputType(typeof(BgInfoTableWidget))]
public sealed class CmdletNewBGInfoTable : BgInfoWidgetCmdlet {
    private readonly List<object> _items = new();

    /// <para>Objects to show, one row each.</para>
    [Parameter(ValueFromPipeline = true, ParameterSetName = "Objects")]
    public object? InputObject { get; set; }

    /// <para>Columns to read from the objects: property names or calculated properties (@{ Name = ...; Expression = { ... } }). Defaults to the properties of the first object.</para>
    [Parameter(Position = 0, ParameterSetName = "Objects")]
    public object[] Property { get; set; } = Array.Empty<object>();

    /// <para>Column headers for explicit rows.</para>
    [Parameter(Mandatory = true, ParameterSetName = "Rows")]
    public string[] Column { get; set; } = Array.Empty<string>();

    /// <para>Explicit rows: arrays of values, or hashtables keyed by column.</para>
    [Parameter(Mandatory = true, ParameterSetName = "Rows")]
    public object[] Row { get; set; } = Array.Empty<object>();

    /// <para>Column whose values (OK, Warning, Failed...) get a status dot.</para>
    [Parameter]
    public string StatusColumn { get; set; } = string.Empty;

    /// <para>Use compact rows.</para>
    [Parameter]
    public SwitchParameter Dense { get; set; }

    /// <para>Hide the column header row.</para>
    [Parameter]
    public SwitchParameter NoHeader { get; set; }

    /// <para>Do not shade every other row.</para>
    [Parameter]
    public SwitchParameter NoStriping { get; set; }

    /// <summary>Collects piped objects.</summary>
    protected override void ProcessRecord() {
        if (ParameterSetName == "Objects" && InputObject != null) _items.Add(InputObject);
    }

    /// <summary>Emits the table widget.</summary>
    protected override void EndProcessing() {
        var widget = Configure(new BgInfoTableWidget());
        widget.Dense = Dense.IsPresent;
        widget.ShowHeader = !NoHeader.IsPresent;
        widget.RowStriping = !NoStriping.IsPresent;
        if (!string.IsNullOrWhiteSpace(StatusColumn)) widget.StatusColumn = StatusColumn;

        if (ParameterSetName == "Rows") {
            widget.Columns.AddRange(Column);
            foreach (var row in Row) widget.Rows.Add(ReadRow(row, Column));
        } else {
            var columns = ResolveColumns();
            widget.Columns.AddRange(columns.Select(column => column.Name));
            foreach (var item in _items) widget.Rows.Add(columns.Select(column => FormatCell(column.Read(item))).ToArray());
        }

        Emit(widget);
    }

    private List<(string Name, Func<object, object?> Read)> ResolveColumns() {
        var columns = new List<(string Name, Func<object, object?> Read)>();
        var selection = Property != null && Property.Length > 0
            ? Property
            : _items.Count == 0 ? Array.Empty<object>() : PSObject.AsPSObject(_items[0]).Properties.Select(p => (object)p.Name).ToArray();
        foreach (var entry in selection) {
            var value = BgInfoValueReader.Unwrap(entry);
            if (value is IDictionary calculated) {
                var name = BgInfoValueReader.Text(BgInfoValueReader.GetProperty(calculated, "Name", "Label", "N", "L"));
                var expression = BgInfoValueReader.GetProperty(calculated, "Expression", "E");
                if (expression is ScriptBlock script) {
                    columns.Add((name, item => {
                        var result = script.InvokeWithContext(null, new List<PSVariable> { new("_", item), new("PSItem", item) });
                        return result.Count == 0 ? null : BgInfoValueReader.Unwrap(result[result.Count - 1]);
                    }));
                } else {
                    var source = BgInfoValueReader.Text(expression);
                    columns.Add((name.Length > 0 ? name : source, item => BgInfoValueReader.GetProperty(item, source)));
                }
            } else {
                var name = BgInfoValueReader.Text(value);
                columns.Add((name, item => BgInfoValueReader.GetProperty(item, name)));
            }
        }
        return columns;
    }

    private static string[] ReadRow(object? row, string[] columns) {
        var value = BgInfoValueReader.Unwrap(row);
        if (value is IDictionary) return columns.Select(column => FormatCell(BgInfoValueReader.GetProperty(value, column))).ToArray();
        if (value is IEnumerable enumerable && value is not string) return enumerable.Cast<object?>().Select(FormatCell).ToArray();
        return new[] { FormatCell(value) };
    }

    internal static string FormatCell(object? value) {
        value = BgInfoValueReader.Unwrap(value);
        return value switch {
            null => string.Empty,
            DateTime date => date.ToString("g", CultureInfo.CurrentCulture),
            double number => number.ToString("0.##", CultureInfo.CurrentCulture),
            float number => number.ToString("0.##", CultureInfo.CurrentCulture),
            decimal number => number.ToString("0.##", CultureInfo.CurrentCulture),
            TimeSpan span => SystemInfoProvider.FormatUptime(span),
            _ => BgInfoValueReader.Text(value)
        };
    }
}

/// <summary>Creates a list panel: bullets, numbers, a checklist or status rows.</summary>
/// <para>Items can be strings, or hashtables and objects with Text (or Name), Value, Status and Checked properties.</para>
/// <example>
/// <code>
/// New-BGInfoList -Region Left -Title 'Before you leave' -Item @(
///     @{ Text = 'Save to OneDrive'; Checked = $true }
///     @{ Text = 'Sign out'; Checked = $false }
/// )
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoList")]
[OutputType(typeof(BgInfoListWidget))]
public sealed class CmdletNewBGInfoList : BgInfoWidgetCmdlet {
    private readonly List<object> _items = new();

    /// <para>List items.</para>
    [Parameter(Position = 0, ValueFromPipeline = true)]
    public object[] Item { get; set; } = Array.Empty<object>();

    /// <para>Marker: Auto, Bullet, Number, Check, Status or None. Auto uses Check for checklist items, Status for status items and Bullet otherwise.</para>
    [Parameter]
    public BgInfoListMarker Marker { get; set; }

    /// <para>Use compact rows.</para>
    [Parameter]
    public SwitchParameter Dense { get; set; }

    /// <summary>Collects items.</summary>
    protected override void ProcessRecord() {
        foreach (var item in Item ?? Array.Empty<object>()) if (item != null) _items.Add(item);
    }

    /// <summary>Emits the list widget.</summary>
    protected override void EndProcessing() {
        var widget = Configure(new BgInfoListWidget());
        widget.Dense = Dense.IsPresent;
        widget.Marker = Marker;
        foreach (var raw in _items) {
            var value = BgInfoValueReader.Unwrap(raw);
            if (value is string text) {
                widget.Items.Add(new BgInfoListItem { Text = text });
                continue;
            }
            var checkedValue = BgInfoValueReader.GetProperty(value, "Checked", "Done", "Completed");
            widget.Items.Add(new BgInfoListItem {
                Text = BgInfoValueReader.Text(BgInfoValueReader.GetProperty(value, "Text", "Name", "Label", "Title")),
                Value = BgInfoValueReader.GetProperty(value, "Value") is { } itemValue ? CmdletNewBGInfoTable.FormatCell(itemValue) : null,
                Status = BgInfoValueReader.Status(BgInfoValueReader.GetProperty(value, "Status", "State")),
                Checked = ParseChecked(checkedValue)
            });
        }

        Emit(widget);
    }

    private static bool? ParseChecked(object? value) {
        if (value == null) return null;
        value = BgInfoValueReader.Unwrap(value);
        if (value is string text && bool.TryParse(text, out var parsed)) return parsed;
        return LanguagePrimitives.IsTrue(value);
    }
}
