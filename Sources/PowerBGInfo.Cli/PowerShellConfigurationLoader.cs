using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PowerBGInfo.Cli;

internal static class PowerShellConfigurationLoader {
    internal sealed class LoadResult {
        public BgInfoConfiguration Configuration { get; set; } = null!;
        public string ScriptDirectory { get; set; } = string.Empty;
    }

    public static LoadResult Load(string scriptPath, string? modulePath, string? powerShellPath) {
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
            RunPowerShell(wrapperPath, fullScriptPath, tempJsonPath, fullModulePath, powerShellPath);

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

    private static void RunPowerShell(string wrapperPath, string scriptPath, string outputPath, string? modulePath, string? powerShellPath) {
        var candidates = GetPowerShellCandidates(powerShellPath);
        Exception? lastError = null;

        foreach (var candidate in candidates) {
            try {
                Execute(candidate, wrapperPath, scriptPath, outputPath, modulePath);
                return;
            } catch (Win32Exception ex) {
                lastError = ex;
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

    private static void Execute(string powerShellPath, string wrapperPath, string scriptPath, string outputPath, string? modulePath) {
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

        process.Start();
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0) {
            var message = new StringBuilder();
            message.AppendLine($"PowerShell script execution failed with exit code {process.ExitCode}.");

            if (!string.IsNullOrWhiteSpace(standardError)) {
                message.AppendLine(standardError.Trim());
            } else if (!string.IsNullOrWhiteSpace(standardOutput)) {
                message.AppendLine(standardOutput.Trim());
            }

            throw new InvalidOperationException(message.ToString().Trim());
        }
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
        return """
param(
    [Parameter(Mandatory = $true)]
    [string] $ScriptPath,
    [Parameter(Mandatory = $true)]
    [string] $OutputPath,
    [string] $ModulePath
)

$ErrorActionPreference = 'Stop'

if ($ModulePath) {
    Import-Module -Name $ModulePath -Force
} elseif (-not (Get-Module -Name PowerBGInfo)) {
    $scriptImportsPowerBGInfo = $false
    try {
        $scriptText = Get-Content -LiteralPath $ScriptPath -Raw -ErrorAction Stop
        $scriptImportsPowerBGInfo = $scriptText -match '(?im)^\s*Import-Module\b[^\r\n]*(PowerBGInfo|PowerBGInfo\.psd1|PowerBGInfo\.psm1|PowerBGInfo\.PowerShell\.dll)\b'
    } catch {
    }

    if (-not $scriptImportsPowerBGInfo) {
        try {
            Import-Module -Name PowerBGInfo -Force -ErrorAction Stop
        } catch {
        }
    }
}

$scriptDirectory = [System.IO.Path]::GetDirectoryName($ScriptPath)
if ([string]::IsNullOrWhiteSpace($scriptDirectory)) {
    $scriptDirectory = (Get-Location).Path
}
Push-Location
Set-Location -LiteralPath $scriptDirectory
try {
    $results = & $ScriptPath
} finally {
    Pop-Location
}

$config = @($results | Where-Object {
    $candidate = $_.PSObject.BaseObject
    $candidate -ne $null -and $candidate.GetType().FullName -eq 'PowerBGInfo.BgInfoConfiguration'
} | Select-Object -Last 1)
if ($config.Count -gt 0) {
    $configuration = $config[0].PSObject.BaseObject
    $configurationType = $configuration.GetType()
    $jsonType = $configurationType.Assembly.GetType('PowerBGInfo.BgInfoConfigurationJson', $true)
    $saveMethod = $jsonType.GetMethod('Save', [type[]] @($configurationType, [string]))
    if ($null -eq $saveMethod) {
        throw "Unable to find PowerBGInfo.BgInfoConfigurationJson.Save."
    }
    $saveMethod.Invoke($null, @($configuration, $OutputPath)) | Out-Null
    Write-Output $OutputPath
    return
}

$jsonPath = @(
    $results |
        Where-Object { $_ -is [string] } |
        ForEach-Object {
            try {
                $candidate = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($_)
                if ((Test-Path -LiteralPath $candidate) -and ([System.IO.Path]::GetExtension($candidate) -ieq '.json')) {
                    $candidate
                }
            } catch {
            }
        } |
        Select-Object -Last 1
)

if ($jsonPath.Count -gt 0) {
    Copy-Item -LiteralPath $jsonPath[0] -Destination $OutputPath -Force
    Write-Output $OutputPath
    return
}

throw "Script must return a [PowerBGInfo.BgInfoConfiguration] object (for example via New-BGInfo -PassThru) or a path to a JSON configuration file."
""";
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
