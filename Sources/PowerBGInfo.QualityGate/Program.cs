using System.Text.Json;
using System.Text.Json.Serialization;
using PowerBGInfo.QualityGate;

if (args.Length == 0 || args.Any(IsHelp)) {
    PrintUsage();
    return args.Length == 0 ? 1 : 0;
}

try {
    var options = Parse(args);
    var report = ImageQualityComparer.Compare(options);
    var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    var jsonPath = Path.Combine(options.OutputDirectory, "quality-report.json");
    var htmlPath = Path.Combine(options.OutputDirectory, "quality-report.html");
    File.WriteAllText(jsonPath, JsonSerializer.Serialize(report, jsonOptions));
    HtmlReportWriter.Write(report, htmlPath);

    Console.WriteLine("Compared: " + report.Compared);
    Console.WriteLine("Passed  : " + report.Passed);
    Console.WriteLine("Failed  : " + report.Failed);
    Console.WriteLine("Missing : " + report.Missing);
    Console.WriteLine("JSON    : " + jsonPath);
    Console.WriteLine("HTML    : " + htmlPath);
    return report.Failed == 0 ? 0 : 2;
} catch (Exception ex) {
    Console.Error.WriteLine(ex.Message);
    Console.Error.WriteLine();
    PrintUsage();
    return 1;
}

static bool IsHelp(string arg) =>
    arg.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
    arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
    arg.Equals("/?", StringComparison.OrdinalIgnoreCase);

static ImageComparisonOptions Parse(string[] args) {
    var options = new ImageComparisonOptions();
    for (var i = 0; i < args.Length; i++) {
        switch (args[i].ToLowerInvariant()) {
            case "--baseline":
                options.BaselineDirectory = Required(args, ref i, "baseline");
                break;
            case "--candidate":
                options.CandidateDirectory = Required(args, ref i, "candidate");
                break;
            case "--output":
                options.OutputDirectory = Required(args, ref i, "output");
                break;
            case "--recursive":
                options.Recursive = true;
                break;
            case "--allow-missing":
                options.FailOnMissing = false;
                break;
            case "--mean-threshold":
                options.MeanThreshold = DoubleValue(args, ref i, "mean-threshold");
                break;
            case "--rmse-threshold":
                options.RmseThreshold = DoubleValue(args, ref i, "rmse-threshold");
                break;
            case "--max-channel-threshold":
                options.MaxChannelThreshold = IntValue(args, ref i, "max-channel-threshold");
                break;
            case "--changed-pixel-percent-threshold":
                options.ChangedPixelPercentThreshold = DoubleValue(args, ref i, "changed-pixel-percent-threshold");
                break;
            case "--diff-scale":
                options.DiffScale = IntValue(args, ref i, "diff-scale");
                break;
            default:
                throw new ArgumentException("Unknown argument: " + args[i]);
        }
    }

    return options;
}

static string Required(string[] args, ref int index, string name) {
    if (index + 1 >= args.Length) throw new ArgumentException("Missing value for --" + name + ".");
    return args[++index];
}

static double DoubleValue(string[] args, ref int index, string name) {
    var text = Required(args, ref index, name);
    if (!double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value)) {
        throw new ArgumentException("Invalid numeric value for --" + name + ": " + text);
    }
    return value;
}

static int IntValue(string[] args, ref int index, string name) {
    var text = Required(args, ref index, name);
    if (!int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var value)) {
        throw new ArgumentException("Invalid integer value for --" + name + ": " + text);
    }
    return value;
}

static void PrintUsage() {
    Console.WriteLine("PowerBGInfo.QualityGate");
    Console.WriteLine("Usage:");
    Console.WriteLine("  PowerBGInfo.QualityGate --baseline <dir> --candidate <dir> --output <dir> [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("      --recursive                              Compare subdirectories.");
    Console.WriteLine("      --allow-missing                          Do not fail when a candidate image is missing.");
    Console.WriteLine("      --mean-threshold <value>                 Default: 1.25");
    Console.WriteLine("      --rmse-threshold <value>                 Default: 3.0");
    Console.WriteLine("      --max-channel-threshold <0-255>          Default: 48");
    Console.WriteLine("      --changed-pixel-percent-threshold <pct>  Default: 2.0");
    Console.WriteLine("      --diff-scale <integer>                   Default: 4");
}
