using System;
using System.IO;

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
            string? scriptPath = null;
            string? outputFileName = null;
            string? configurationDirectory = null;
            string? targetOverride = null;
            string? exportJsonPath = null;
            string? powerShellPath = null;
            string? modulePath = null;
            int? monitorIndex = null;
            bool noApply = false;
            bool exportOnly = false;
            string? scriptDirectory = null;

            for (int i = 0; i < args.Length; i++) {
                var arg = args[i];
                switch (arg.ToLowerInvariant()) {
                    case "--config":
                    case "-c":
                        configPath = GetValue(args, ref i, "config");
                        break;
                    case "--script":
                    case "-s":
                        scriptPath = GetValue(args, ref i, "script");
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
                    case "--export-json":
                        exportJsonPath = GetValue(args, ref i, "export-json");
                        break;
                    case "--export-only":
                        exportOnly = true;
                        break;
                    case "--pwsh":
                        powerShellPath = GetValue(args, ref i, "pwsh");
                        break;
                    case "--module":
                        modulePath = GetValue(args, ref i, "module");
                        break;
                    case "--no-apply":
                        noApply = true;
                        break;
                    default:
                        return Fail($"Unknown argument: {arg}");
                }
            }

            if (string.IsNullOrWhiteSpace(configPath) == string.IsNullOrWhiteSpace(scriptPath)) {
                return Fail("Specify exactly one input source: --config <file> or --script <file>.");
            }

            if (exportOnly && string.IsNullOrWhiteSpace(exportJsonPath)) {
                return Fail("Missing --export-json argument for --export-only.");
            }

            BgInfoConfiguration config;
            if (!string.IsNullOrWhiteSpace(scriptPath)) {
                var loadResult = PowerShellConfigurationLoader.Load(scriptPath!, modulePath, powerShellPath);
                config = loadResult.Configuration;
                scriptDirectory = loadResult.ScriptDirectory;
            } else {
                config = BgInfoConfigurationJson.Load(configPath!);
            }

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

            if (!string.IsNullOrWhiteSpace(exportJsonPath)) {
                var fullExportJsonPath = Path.GetFullPath(exportJsonPath!);
                var exportDirectory = Path.GetDirectoryName(fullExportJsonPath);
                if (!string.IsNullOrWhiteSpace(exportDirectory)) {
                    Directory.CreateDirectory(exportDirectory);
                }

                BgInfoConfigurationJson.Save(config, fullExportJsonPath);
                if (exportOnly) {
                    Console.WriteLine(fullExportJsonPath);
                    return 0;
                }
            }

            if (!string.IsNullOrWhiteSpace(scriptDirectory)) {
                ResolveScriptRelativePaths(config, scriptDirectory!);
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
        Console.WriteLine("  PowerBGInfo.Cli --script <file> [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -c, --config <file>       Path to JSON configuration file.");
        Console.WriteLine("  -s, --script <file>       Path to a PowerShell authoring script.");
        Console.WriteLine("  -o, --output <file>       Override output file name.");
        Console.WriteLine("  -d, --directory <dir>     Override configuration output directory.");
        Console.WriteLine("  -m, --monitor <index>     Override monitor index.");
        Console.WriteLine("      --target <target>     Override target (Wallpaper, File, LogonScreen, Both).");
        Console.WriteLine("      --export-json <file>  Save the loaded configuration to JSON.");
        Console.WriteLine("      --export-only         Save JSON and skip rendering. Requires --export-json.");
        Console.WriteLine("      --pwsh <path>         Override the PowerShell executable used for --script.");
        Console.WriteLine("      --module <path>       Import the PowerBGInfo module before running --script.");
        Console.WriteLine("      --no-apply            Generate the image without applying wallpaper.");
    }

    private static void ResolveScriptRelativePaths(BgInfoConfiguration configuration, string scriptDirectory) {
        if (!string.IsNullOrWhiteSpace(configuration.FilePath) && !Path.IsPathRooted(configuration.FilePath)) {
            configuration.FilePath = Path.GetFullPath(Path.Combine(scriptDirectory, configuration.FilePath));
        }

        if (!string.IsNullOrWhiteSpace(configuration.ConfigurationDirectory) && !Path.IsPathRooted(configuration.ConfigurationDirectory)) {
            configuration.ConfigurationDirectory = Path.GetFullPath(Path.Combine(scriptDirectory, configuration.ConfigurationDirectory));
        }
    }
}
