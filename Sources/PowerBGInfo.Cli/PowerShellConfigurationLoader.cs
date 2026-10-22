using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBGInfo.Cli;

internal static class PowerShellConfigurationLoader {
    internal sealed class LoadResult {
        public BgInfoConfiguration Configuration { get; set; } = null!;
        public string ScriptDirectory { get; set; } = string.Empty;
    }

    public static LoadResult Load(string scriptPath, string? modulePath, string? powerShellPath, TimeSpan? timeout = null) {
        if (string.IsNullOrWhiteSpace(scriptPath)) {
            throw new ArgumentException("Script path is required.", nameof(scriptPath));
        }

        var fullScriptPath = Path.GetFullPath(scriptPath);
        if (!File.Exists(fullScriptPath)) {
            throw new FileNotFoundException("PowerShell script was not found.", fullScriptPath);
        }

        string? fullModulePath = null;
        if (!string.IsNullOrWhiteSpace(modulePath)) {
            fullModulePath = Path.GetFullPath(modulePath);
            if (!File.Exists(fullModulePath)) {
                throw new FileNotFoundException("PowerBGInfo module was not found.", fullModulePath);
            }
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "PowerBGInfo");
        Directory.CreateDirectory(tempDirectory);

        var tempJsonPath = Path.Combine(tempDirectory, $"{Path.GetFileNameWithoutExtension(fullScriptPath)}.{Guid.NewGuid():N}.json");
        var wrapperPath = Path.Combine(tempDirectory, $"powerbginfo-script-wrapper.{Guid.NewGuid():N}.ps1");

        try {
            File.WriteAllText(wrapperPath, BuildWrapperScript(), Encoding.UTF8);
            RunPowerShell(wrapperPath, fullScriptPath, tempJsonPath, fullModulePath, powerShellPath, timeout ?? TimeSpan.FromMinutes(2));

            if (!File.Exists(tempJsonPath)) {
                throw new InvalidOperationException($"The PowerShell script did not produce a BGInfo configuration at '{tempJsonPath}'.");
            }

            return new LoadResult {
                Configuration = BgInfoConfigurationJson.Load(tempJsonPath, Path.GetDirectoryName(fullScriptPath)),
                ScriptDirectory = Path.GetDirectoryName(fullScriptPath) ?? Environment.CurrentDirectory
            };
        } finally {
            TryDeleteFile(wrapperPath);
            TryDeleteFile(tempJsonPath);
        }
    }

    private static void RunPowerShell(string wrapperPath, string scriptPath, string outputPath, string? modulePath, string? powerShellPath, TimeSpan timeout) {
        if (timeout <= TimeSpan.Zero || timeout.TotalMilliseconds > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(timeout));
        var candidates = GetPowerShellCandidates(powerShellPath);
        Exception? lastError = null;

        foreach (var candidate in candidates) {
            try {
                Execute(candidate, wrapperPath, scriptPath, outputPath, modulePath, timeout);
                return;
            } catch (PowerShellHostStartException ex) {
                lastError = ex.InnerException;
            } catch (InvalidOperationException) {
                throw;
            }
        }

        throw new InvalidOperationException("Unable to execute the PowerShell-backed configuration script.", lastError);
    }

    private static IEnumerable<string> GetPowerShellCandidates(string? preferredPath) {
        if (!string.IsNullOrWhiteSpace(preferredPath)) {
            yield return preferredPath!;
            yield break;
        }

        yield return "pwsh";
        yield return "powershell";
    }

    private static void Execute(string powerShellPath, string wrapperPath, string scriptPath, string outputPath, string? modulePath, TimeSpan timeout) {
        using var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = powerShellPath,
                Arguments = BuildArguments(wrapperPath, scriptPath, outputPath, modulePath),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? Environment.CurrentDirectory
            }
        };

        using var lifetime = new ConfigurationScriptLifetime(process.StartInfo);
        var elapsed = Stopwatch.StartNew();
        try {
            process.Start();
        } catch (Win32Exception error) {
            // Only host launch failure permits another candidate. A later native
            // cleanup failure must never execute the author's script a second time.
            throw new PowerShellHostStartException(error);
        }
        try {
            lifetime.Attach(process);
            var output = Task.Run(() => DrainOutput(process.StandardOutput));
            var errors = Task.Run(() => DrainOutput(process.StandardError));
            int exitCode;
            try {
                exitCode = lifetime.WaitForCompletion(elapsed, timeout);
            } finally {
                // Descendants may retain both pipes even after the script exits.
                // End their owned scope before waiting for or disposing readers.
                lifetime.Terminate();
            }
            if (!process.WaitForExit(ConfigurationScriptLifetime.RemainingMilliseconds(elapsed, timeout)) ||
                !Task.WaitAll(new Task[] { output, errors }, ConfigurationScriptLifetime.RemainingMilliseconds(elapsed, timeout))) {
                throw ConfigurationScriptLifetime.Deadline(timeout);
            }
            var standardOutput = output.GetAwaiter().GetResult();
            var standardError = errors.GetAwaiter().GetResult();

            if (exitCode != 0) {
                var message = new StringBuilder();
                message.AppendLine($"PowerShell script execution failed with exit code {exitCode}.");

                if (!string.IsNullOrWhiteSpace(standardError)) {
                    message.AppendLine(standardError.Trim());
                } else if (!string.IsNullOrWhiteSpace(standardOutput)) {
                    message.AppendLine(standardOutput.Trim());
                }

                throw new InvalidOperationException(message.ToString().Trim());
            }
        } finally {
            lifetime.Terminate();
            // Allow kernel termination to close inherited pipe handles before
            // Process.Dispose closes the readers used by the drain tasks.
            process.WaitForExit(5000);
        }
    }

    // Continue draining after the capture limit so a noisy child cannot fill either pipe.
    private static string DrainOutput(StreamReader reader) {
        const int maximumCharacters = 65536;
        var captured = new StringBuilder();
        var buffer = new char[4096];
        try {
            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) != 0) {
                var retain = Math.Min(read, maximumCharacters - captured.Length);
                if (retain > 0) captured.Append(buffer, 0, retain);
            }
        } catch (ObjectDisposedException) { }
        catch (IOException) { }
        return captured.ToString();
    }

    private static string BuildArguments(string wrapperPath, string scriptPath, string outputPath, string? modulePath) {
        var arguments = new List<string> {
            "-NoLogo",
            "-NoProfile",
            "-NonInteractive",
            "-File",
            wrapperPath,
            "-ScriptPath",
            scriptPath,
            "-OutputPath",
            outputPath
        };

        if (!string.IsNullOrWhiteSpace(modulePath)) {
            arguments.Add("-ModulePath");
            arguments.Add(modulePath!);
        }

        return string.Join(" ", arguments.Select(QuoteArgument));
    }

    private static string QuoteArgument(string value) {
        if (value.Length == 0) {
            return "\"\"";
        }

        bool needsQuotes = value.Any(character => char.IsWhiteSpace(character) || character == '"');
        if (!needsQuotes) {
            return value;
        }

        var builder = new StringBuilder();
        builder.Append('"');

        int backslashCount = 0;
        foreach (var character in value) {
            if (character == '\\') {
                backslashCount++;
                continue;
            }

            if (character == '"') {
                builder.Append('\\', backslashCount * 2 + 1);
                builder.Append('"');
                backslashCount = 0;
                continue;
            }

            if (backslashCount > 0) {
                builder.Append('\\', backslashCount);
                backslashCount = 0;
            }

            builder.Append(character);
        }

        if (backslashCount > 0) {
            builder.Append('\\', backslashCount * 2);
        }

        builder.Append('"');
        return builder.ToString();
    }

    private static string BuildWrapperScript() {
        using var resource = typeof(PowerShellConfigurationLoader).Assembly.GetManifestResourceStream("PowerBGInfo.Cli.ConfigurationWrapper.ps1")
            ?? throw new InvalidOperationException("The configuration-script resource is missing.");
        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }

    private sealed class PowerShellHostStartException : Exception {
        internal PowerShellHostStartException(Win32Exception error) : base(error.Message, error) { }
    }

    private static void TryDeleteFile(string path) {
        try {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        } catch {
        }
    }
}
