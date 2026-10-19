using System.Collections;
using System.Globalization;
using PowerBGInfo;

namespace PowerBGInfo.PowerShell;

/// <summary>Creates a calendar heatmap panel: one cell per day, darker for larger values.</summary>
/// <para>Pipe objects with Date and Value properties, pipe plain dates (each counts once), or pass -Data @{ date = value }.</para>
/// <example>
/// <code>
/// Get-WinEvent -FilterHashtable @{ LogName = 'Security'; Id = 4624; StartTime = (Get-Date).AddDays(-90) } -MaxEvents 5000 |
///     ForEach-Object TimeCreated | New-BGInfoCalendarHeatmap -Region Bottom -Title 'Sign-ins, last 90 days'
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoCalendarHeatmap", DefaultParameterSetName = "Objects")]
[OutputType(typeof(BgInfoCalendarHeatmapWidget))]
public sealed class CmdletNewBGInfoCalendarHeatmap : BgInfoWidgetCmdlet {
    private readonly List<BgInfoDatedValue> _items = new();

    /// <para>Objects with a date and a value, or plain dates.</para>
    [Parameter(ValueFromPipeline = true, ParameterSetName = "Objects")]
    public object? InputObject { get; set; }

    /// <para>Dated values as a hashtable: keys are dates, values are numbers.</para>
    [Parameter(Mandatory = true, ParameterSetName = "Data")]
    public IDictionary Data { get; set; } = new Hashtable();

    /// <para>Property that holds the date.</para>
    [Parameter(ParameterSetName = "Objects")]
    public string DateProperty { get; set; } = "Date";

    /// <para>Property that holds the value. Objects without it count once.</para>
    [Parameter(ParameterSetName = "Objects")]
    public string ValueProperty { get; set; } = "Value";

    /// <para>High-intensity cell color. Unset uses the accent.</para>
    [Parameter]
    public object? Color { get; set; }

    /// <para>First day of the week rows.</para>
    [Parameter]
    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

    /// <para>Show the less-to-more color key.</para>
    [Parameter]
    public SwitchParameter ShowLegend { get; set; }

    /// <summary>Collects piped values.</summary>
    protected override void ProcessRecord() {
        if (ParameterSetName != "Objects" || InputObject == null) return;
        var value = BgInfoValueReader.Unwrap(InputObject);
        if (value is DateTime day) {
            _items.Add(new BgInfoDatedValue { Date = day, Value = 1 });
            return;
        }
        var date = ToDate(BgInfoValueReader.GetProperty(value, DateProperty, "Day", "TimeCreated", "Timestamp"));
        if (!date.HasValue) {
            WriteWarning($"Skipped an item without a '{DateProperty}' date.");
            return;
        }
        var hasValue = BgInfoValueReader.TryGetProperty(value, out var rawValue, ValueProperty, "Count");
        var numericValue = hasValue ? BgInfoValueReader.Number(rawValue) : 1;
        if (!numericValue.HasValue) {
            WriteWarning($"Skipped an item with an invalid '{ValueProperty}' value.");
            return;
        }
        _items.Add(new BgInfoDatedValue { Date = date.Value, Value = numericValue.Value });
    }

    /// <summary>Emits the calendar widget.</summary>
    protected override void EndProcessing() {
        if (ParameterSetName == "Data") {
            foreach (DictionaryEntry entry in Data) {
                var date = ToDate(entry.Key);
                var number = BgInfoValueReader.Number(entry.Value);
                if (date.HasValue && number.HasValue) _items.Add(new BgInfoDatedValue { Date = date.Value, Value = number.Value });
                else WriteWarning($"Skipped '{entry.Key}': expected a date and numeric value.");
            }
        }
        var widget = Configure(new BgInfoCalendarHeatmapWidget());
        widget.Items.AddRange(_items);
        widget.Color = PowerShellColorConverter.ConvertOptional(Color, nameof(Color));
        widget.FirstDayOfWeek = FirstDayOfWeek;
        widget.ShowLegend = ShowLegend.IsPresent;
        Emit(widget);
    }

    internal static DateTime? ToDate(object? value) {
        value = BgInfoValueReader.Unwrap(value);
        switch (value) {
            case DateTime date: return date;
            case DateTimeOffset offset: return offset.LocalDateTime;
            case string text:
                string[] isoFormats = { "yyyy-MM-dd", "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK" };
                if (DateTime.TryParseExact(text, isoFormats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var iso)) return iso;
                if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var local)) return local;
                if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var invariant)) return invariant;
                return null;
            default:
                return value != null && LanguagePrimitives.TryConvertTo<DateTime>(value, out var converted) ? converted : null;
        }
    }
}

/// <summary>Creates one range for New-BGInfoTimeline.</summary>
/// <example>
/// <code>
/// New-BGInfoTimelineItem -Lane 'SQL-PROD-01' -Label 'Patching' -Start '2026-10-18 02:00' -Duration '02:00' -Category Maintenance
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoTimelineItem")]
[OutputType(typeof(BgInfoTimelineItem))]
public sealed class CmdletNewBGInfoTimelineItem : PSCmdlet {
    /// <para>Lane name, for example a server, a team or On call.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public string Lane { get; set; } = string.Empty;

    /// <para>Text drawn on the bar.</para>
    [Parameter(Position = 1)]
    public string Label { get; set; } = string.Empty;

    /// <para>Range start.</para>
    [Parameter(Mandatory = true)]
    public DateTime Start { get; set; }

    /// <para>Range end, later than Start at the chart's 100-microsecond resolution. Without -End or -Duration the range stays open until now.</para>
    [Parameter]
    public DateTime End { get; set; }

    /// <para>Range length, instead of -End.</para>
    [Parameter]
    public TimeSpan Duration { get; set; }

    /// <para>Legend category, for example Maintenance, Change freeze or On call.</para>
    [Parameter]
    public string Category { get; set; } = "Scheduled";

    /// <para>Status color for the category: Ok, Warning, Critical or Info.</para>
    [Parameter]
    public BgInfoStatus Status { get; set; }

    /// <para>Explicit category color.</para>
    [Parameter]
    public object? Color { get; set; }

    /// <summary>Emits the item.</summary>
    protected override void EndProcessing() {
        var bound = MyInvocation.BoundParameters;
        if (bound.ContainsKey(nameof(End)) && bound.ContainsKey(nameof(Duration))) {
            ThrowTerminatingError(new ErrorRecord(new ArgumentException("Use -End or -Duration, not both."), "BGInfoTimelineEndAndDuration", ErrorCategory.InvalidArgument, Duration));
            return;
        }
        DateTime? end = bound.ContainsKey(nameof(End)) ? End : bound.ContainsKey(nameof(Duration)) ? Start + Duration : null;
        var item = new BgInfoTimelineItem {
            Lane = Lane,
            Label = Label,
            Start = Start,
            End = end,
            Category = string.IsNullOrWhiteSpace(Category) ? "Scheduled" : Category,
            Status = Status,
            Color = PowerShellColorConverter.ConvertOptional(Color, nameof(Color))
        };
        if (item.ValidationError() is { } error) {
            ThrowTerminatingError(new ErrorRecord(new ArgumentException(error), "BGInfoTimelineNegativeRange", ErrorCategory.InvalidArgument, end));
            return;
        }
        WriteObject(item);
    }
}

/// <summary>Creates a timeline panel: ranges on named lanes, with a Now marker.</summary>
/// <para>Use it for maintenance windows, change freezes, backups or the on-call rota. Items come from New-BGInfoTimelineItem, or hashtables and objects with Lane, Label, Start, End and Category.</para>
/// <example>
/// <code>
/// New-BGInfoTimeline -Region Bottom -Title 'This week' -Now (Get-Date) -Item @(
///     New-BGInfoTimelineItem 'On call' 'A. Novak' -Start '2026-10-12' -End '2026-10-15' -Category 'On call'
///     New-BGInfoTimelineItem 'SQL-PROD-01' 'Patching' -Start '2026-10-18 02:00' -Duration '02:00' -Category Maintenance -Status Warning
/// )
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoTimeline")]
[OutputType(typeof(BgInfoTimelineWidget))]
public sealed class CmdletNewBGInfoTimeline : BgInfoWidgetCmdlet {
    private readonly List<BgInfoTimelineItem> _items = new();

    /// <para>Timeline items. Records without a lane or with an invalid range are skipped with a warning.</para>
    [Parameter(Position = 0, ValueFromPipeline = true)]
    public object[] Item { get; set; } = Array.Empty<object>();

    /// <para>Instant of the Now marker; open items end there.</para>
    [Parameter]
    public DateTime Now { get; set; }

    /// <para>Hide the category legend.</para>
    [Parameter]
    public SwitchParameter NoLegend { get; set; }

    /// <summary>Collects items.</summary>
    protected override void ProcessRecord() {
        foreach (var raw in Item ?? Array.Empty<object>()) {
            var value = BgInfoValueReader.Unwrap(raw);
            if (value is BgInfoTimelineItem item) {
                AddItem(item);
                continue;
            }
            var start = CmdletNewBGInfoCalendarHeatmap.ToDate(BgInfoValueReader.GetProperty(value, "Start", "StartTime", "From"));
            if (!start.HasValue) {
                WriteWarning("Skipped a timeline item without a Start.");
                continue;
            }
            var rawEnd = BgInfoValueReader.GetProperty(value, "End", "EndTime", "To");
            var end = CmdletNewBGInfoCalendarHeatmap.ToDate(rawEnd);
            if (rawEnd != null && !string.IsNullOrWhiteSpace(BgInfoValueReader.Text(rawEnd)) && !end.HasValue) {
                WriteWarning("Skipped a timeline item with an invalid end date.");
                continue;
            }
            AddItem(new BgInfoTimelineItem {
                Lane = BgInfoValueReader.Text(BgInfoValueReader.GetProperty(value, "Lane", "Name", "Server", "Team")),
                Label = BgInfoValueReader.Text(BgInfoValueReader.GetProperty(value, "Label", "Title", "Text")),
                Start = start.Value,
                End = end,
                Category = BgInfoValueReader.Text(BgInfoValueReader.GetProperty(value, "Category", "Kind", "Type")) is { Length: > 0 } category ? category : "Scheduled",
                Status = BgInfoValueReader.Status(BgInfoValueReader.GetProperty(value, "Status", "State")),
                Color = PowerShellColorConverter.ConvertOptional(BgInfoValueReader.GetProperty(value, "Color"), "Color")
            });
        }
    }

    private void AddItem(BgInfoTimelineItem item) {
        if (item.ValidationError() is { } error) {
            WriteWarning("Skipped a timeline item: " + error);
            return;
        }
        _items.Add(item);
    }

    /// <summary>Emits the timeline widget.</summary>
    protected override void EndProcessing() {
        var widget = Configure(new BgInfoTimelineWidget());
        widget.Items.AddRange(_items);
        if (IsBound(nameof(Now))) widget.Now = Now;
        widget.ShowLegend = !NoLegend.IsPresent;
        Emit(widget);
    }
}

/// <summary>Creates a treemap panel: sizes as nested rectangles, for example disk usage by folder or VM.</summary>
/// <para>Pipe objects with Name, Value and an optional Group, or pass -Data @{ name = size }.</para>
/// <example>
/// <code>
/// New-BGInfoTreemap -Region Right -Title 'D: by folder' -ValueSuffix ' GB' -Data ([ordered]@{ Databases = 412; Logs = 96; Backups = 230 })
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoTreemap", DefaultParameterSetName = "Objects")]
[OutputType(typeof(BgInfoTreemapWidget))]
public sealed class CmdletNewBGInfoTreemap : BgInfoWidgetCmdlet {
    private readonly List<BgInfoTreemapItem> _items = new();

    /// <para>Objects with a name, a size and an optional group.</para>
    [Parameter(ValueFromPipeline = true, ParameterSetName = "Objects")]
    public object? InputObject { get; set; }

    /// <para>Sizes as a hashtable: keys are names, values are numbers.</para>
    [Parameter(Mandatory = true, ParameterSetName = "Data")]
    public IDictionary Data { get; set; } = new Hashtable();

    /// <para>Property that holds the name.</para>
    [Parameter(ParameterSetName = "Objects")]
    public string NameProperty { get; set; } = "Name";

    /// <para>Property that holds the size.</para>
    [Parameter(ParameterSetName = "Objects")]
    public string ValueProperty { get; set; } = "Value";

    /// <para>Property that holds the optional group.</para>
    [Parameter(ParameterSetName = "Objects")]
    public string GroupProperty { get; set; } = "Group";

    /// <para>Suffix after values, for example ' GB'.</para>
    [Parameter]
    public string ValueSuffix { get; set; } = string.Empty;

    /// <para>Hide values inside the rectangles.</para>
    [Parameter]
    public SwitchParameter NoValues { get; set; }

    /// <summary>Collects piped items.</summary>
    protected override void ProcessRecord() {
        if (ParameterSetName != "Objects" || InputObject == null) return;
        var value = BgInfoValueReader.Unwrap(InputObject);
        var size = BgInfoValueReader.Number(BgInfoValueReader.GetProperty(value, ValueProperty, "Size", "Length", "Count"));
        if (!size.HasValue) {
            WriteWarning($"Skipped an item without a numeric '{ValueProperty}'.");
            return;
        }
        var group = BgInfoValueReader.Text(BgInfoValueReader.GetProperty(value, GroupProperty));
        _items.Add(new BgInfoTreemapItem { Name = BgInfoValueReader.Text(BgInfoValueReader.GetProperty(value, NameProperty, "Label")), Value = size.Value, Group = group.Length > 0 ? group : null });
    }

    /// <summary>Emits the treemap widget.</summary>
    protected override void EndProcessing() {
        if (ParameterSetName == "Data") {
            foreach (DictionaryEntry entry in Data) {
                var number = BgInfoValueReader.Number(entry.Value);
                if (number.HasValue) _items.Add(new BgInfoTreemapItem { Name = BgInfoValueReader.Text(entry.Key), Value = number.Value });
                else WriteWarning($"Skipped '{entry.Key}': expected a numeric value.");
            }
        }
        var widget = Configure(new BgInfoTreemapWidget());
        widget.Items.AddRange(_items);
        widget.ValueSuffix = ValueSuffix ?? string.Empty;
        widget.ShowValues = !NoValues.IsPresent;
        Emit(widget);
    }
}

/// <summary>Creates a diagram panel from Mermaid text, for example an escalation path or a network flow. Pipeline lines are joined before rendering.</summary>
/// <para>Flowcharts and other topology-backed families take the panel theme. Rendering uses ChartForgeX.Mermaid; no browser or JavaScript is involved.</para>
/// <example>
/// <code>
/// New-BGInfoMermaid -Region Right -Title 'Escalation' -Definition @'
/// flowchart TD
///     A[Service desk] --> B{Resolved in 30 min?}
///     B -- No --> C[On-call engineer]
///     C --> D[Major incident]
/// '@
/// Get-Content .\escalation.mmd | New-BGInfoMermaid -Region Right -Title 'Escalation'
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoMermaid")]
[OutputType(typeof(BgInfoMermaidWidget))]
public sealed class CmdletNewBGInfoMermaid : BgInfoWidgetCmdlet {
    private readonly System.Text.StringBuilder _definition = new();

    /// <para>Mermaid source.</para>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    [AllowEmptyString]
    public string Definition { get; set; } = string.Empty;

    /// <summary>Collects pipeline lines into one Mermaid definition.</summary>
    protected override void ProcessRecord() {
        if (_definition.Length > 0) _definition.AppendLine();
        _definition.Append(Definition);
    }

    /// <summary>Checks the source and emits the widget.</summary>
    protected override void EndProcessing() {
        var definition = _definition.ToString();
        var result = ChartForgeX.Mermaid.MermaidRenderer.Render(definition);
        if (result.HasErrors || result.Artifact == null) {
            var error = result.Diagnostics.FirstOrDefault(d => d.Severity == ChartForgeX.Diagnostics.VisualDiagnosticSeverity.Error) ?? result.Diagnostics.FirstOrDefault();
            var message = error == null ? "The Mermaid diagram family is not supported." : $"Mermaid line {error.Span.Line}: {error.Message}";
            ThrowTerminatingError(new ErrorRecord(new ArgumentException(message, nameof(Definition)), "BGInfoMermaidInvalid", ErrorCategory.InvalidData, definition));
            return;
        }
        foreach (var warning in result.Diagnostics.Where(d => d.Severity == ChartForgeX.Diagnostics.VisualDiagnosticSeverity.Warning)) {
            WriteVerbose($"Mermaid line {warning.Span.Line}: {warning.Message}");
        }
        var widget = Configure(new BgInfoMermaidWidget());
        widget.Definition = definition;
        Emit(widget);
    }
}

/// <summary>Creates a QR code panel, for example a link to the help-desk portal.</summary>
/// <para>The code is encoded with CodeGlyphX and drawn with whole-pixel modules on a light field so phone cameras read it reliably.</para>
/// <example>
/// <code>
/// New-BGInfoQRCode -Url 'https://help.contoso.com/new?asset=CT-04217' -Title 'Scan for help' -Caption 'help.contoso.com'
/// </code>
/// </example>
[Cmdlet(VerbsCommon.New, "BGInfoQRCode")]
[OutputType(typeof(BgInfoQrCodeWidget))]
public sealed class CmdletNewBGInfoQRCode : BgInfoWidgetCmdlet {
    /// <para>Text to encode, usually a URL.</para>
    [Parameter(Mandatory = true, Position = 0)]
    [Alias("Url")]
    public string Text { get; set; } = string.Empty;

    /// <para>Text under the code, for example the short URL.</para>
    [Parameter]
    public string Caption { get; set; } = string.Empty;

    /// <para>Module color. Keep it dark on a light field.</para>
    [Parameter]
    public object? ModuleColor { get; set; }

    /// <para>Field and quiet-zone color.</para>
    [Parameter]
    public object? FieldColor { get; set; }

    /// <summary>Checks that the text fits a QR code and emits the widget.</summary>
    protected override void EndProcessing() {
        try {
            CodeGlyphX.QrCodeEncoder.EncodeText(Text, CodeGlyphX.QrErrorCorrectionLevel.M, 1, 40, null);
        } catch (Exception exception) when (exception is ArgumentException || exception is InvalidOperationException) {
            ThrowTerminatingError(new ErrorRecord(exception, "BGInfoQrCodeInvalid", ErrorCategory.InvalidData, Text));
            return;
        }
        var widget = Configure(new BgInfoQrCodeWidget());
        widget.Text = Text;
        widget.Caption = Caption ?? string.Empty;
        widget.ModuleColor = PowerShellColorConverter.ConvertOptional(ModuleColor, nameof(ModuleColor));
        widget.FieldColor = PowerShellColorConverter.ConvertOptional(FieldColor, nameof(FieldColor));
        Emit(widget);
    }
}
