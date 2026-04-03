using System;

namespace PowerBGInfo.Cli;

internal static class Program {
    public static int Main(string[] args) {
        if (args != null && HasHelpFlag(args)) {
            PrintUsage();
            return 0;
        }

        if (args == null || args.Length == 0) {
            PrintUsage();
            return 1;
        }

        try {
            string? configPath = null;
            string? outputFileName = null;
            string? configurationDirectory = null;
            string? targetOverride = null;
            int? monitorIndex = null;
            bool noApply = false;

            for (int i = 0; i < args.Length; i++) {
                var arg = args[i];
                switch (arg.ToLowerInvariant()) {
                    case "--config":
                    case "-c":
                        configPath = GetValue(args, ref i, "config");
                        break;
                    case "--output":
                    case "-o":
                        outputFileName = GetValue(args, ref i, "output");
                        break;
                    case "--directory":
                    case "-d":
                        configurationDirectory = GetValue(args, ref i, "directory");
                        break;
                    case "--monitor":
                    case "-m":
                        var monitorText = GetValue(args, ref i, "monitor");
                        if (!int.TryParse(monitorText, out var monitorValue)) {
                            return Fail($"Invalid monitor index: {monitorText}");
                        }
                        monitorIndex = monitorValue;
                        break;
                    case "--target":
                        targetOverride = GetValue(args, ref i, "target");
                        break;
                    case "--no-apply":
                        noApply = true;
                        break;
                    default:
                        return Fail($"Unknown argument: {arg}");
                }
            }

            if (string.IsNullOrWhiteSpace(configPath)) {
                return Fail("Missing --config argument.");
            }

            var config = BgInfoConfigurationJson.Load(configPath!);
            if (!string.IsNullOrWhiteSpace(outputFileName)) {
                config.OutputFileName = outputFileName!;
            }
            if (!string.IsNullOrWhiteSpace(configurationDirectory)) {
                config.ConfigurationDirectory = configurationDirectory!;
            }
            if (monitorIndex.HasValue) {
                config.MonitorIndex = monitorIndex.Value;
            }
            if (!string.IsNullOrWhiteSpace(targetOverride)) {
                if (!Enum.TryParse(targetOverride, true, out BgInfoTarget target)) {
                    return Fail($"Unknown target: {targetOverride}");
                }
                config.Target = target;
            }
            if (noApply) {
                config.Target = BgInfoTarget.File;
            }

            var outputPath = BgInfoRunner.Run(config);
            Console.WriteLine(outputPath);
            return 0;
        } catch (Exception ex) {
            return Fail(ex.Message);
        }
    }

    private static bool HasHelpFlag(string[] args) {
        foreach (var arg in args) {
            if (arg.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("/?", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }
        return false;
    }

    private static string GetValue(string[] args, ref int index, string name) {
        if (index + 1 >= args.Length) {
            throw new ArgumentException($"Missing value for --{name}.");
        }
        index++;
        return args[index];
    }

    private static int Fail(string message) {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine();
        PrintUsage();
        return 1;
    }

    private static void PrintUsage() {
        Console.WriteLine("PowerBGInfo.Cli");
        Console.WriteLine("Usage:");
        Console.WriteLine("  PowerBGInfo.Cli --config <file> [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -c, --config <file>       Path to JSON configuration file.");
        Console.WriteLine("  -o, --output <file>       Override output file name.");
        Console.WriteLine("  -d, --directory <dir>     Override configuration output directory.");
        Console.WriteLine("  -m, --monitor <index>     Override monitor index.");
        Console.WriteLine("      --target <target>     Override target (Wallpaper, File, LogonScreen, Both).");
        Console.WriteLine("      --no-apply            Generate the image without applying wallpaper.");
    }
}
