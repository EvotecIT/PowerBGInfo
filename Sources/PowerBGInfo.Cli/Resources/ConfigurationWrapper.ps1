param(
    [Parameter(Mandatory = $true)]
    [string] $ScriptPath,
    [Parameter(Mandatory = $true)]
    [string] $OutputPath,
    [string] $ModulePath,
    [switch] $LifetimeWorker
)

$ErrorActionPreference = 'Stop'

# Authorize user code only after the CLI owns this invocation's lifetime scope.
# On POSIX this wrapper stays alive after its worker exits to reserve the group
# identity; unlike a PID lookup, that anchor cannot refer to an unrelated group.
if (-not $LifetimeWorker) {
    $lifetimeDirectory = $env:POWERBGINFO_CONFIGURATION_LIFETIME
    if ([string]::IsNullOrWhiteSpace($lifetimeDirectory)) {
        throw 'The configuration process lifetime was not initialized.'
    }
    $isWindowsHost = [System.IO.Path]::DirectorySeparatorChar -eq '\'
    if (-not $isWindowsHost) {
        Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
public static class PowerBGInfoConfigurationGroup {
    [DllImport("libc", SetLastError = true)] public static extern int setsid();
    [DllImport("libc", SetLastError = true)] public static extern int kill(int id, int signal);
    public static void WatchParent(int group) {
        Task.Run(() => { Console.ReadLine(); kill(-group, 9); });
    }
}
'@
        if ([PowerBGInfoConfigurationGroup]::setsid() -ne $PID) {
            throw 'Unable to establish the configuration process group.'
        }
    }

    $readyPath = [System.IO.Path]::Combine($lifetimeDirectory, 'ready')
    [System.IO.File]::WriteAllText($readyPath + '.tmp', $PID.ToString())
    [System.IO.File]::Move($readyPath + '.tmp', $readyPath)
    if ([Console]::ReadLine() -ne 'run') {
        exit 1
    }

    if (-not $isWindowsHost) {
        [PowerBGInfoConfigurationGroup]::WatchParent($PID)
        $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
        $startInfo.FileName = [System.Diagnostics.Process]::GetCurrentProcess().MainModule.FileName
        $startInfo.UseShellExecute = $false
        $startInfo.RedirectStandardInput = $true
        $startInfo.WorkingDirectory = [System.IO.Path]::GetDirectoryName($ScriptPath)
        # ArgumentList belongs to the POSIX PowerShell 7 boundary. Windows
        # PowerShell 5.1 runs the script directly inside its assigned job.
        foreach ($argument in @('-NoLogo', '-NoProfile', '-NonInteractive', '-File', $PSCommandPath,
                '-ScriptPath', $ScriptPath, '-OutputPath', $OutputPath, '-LifetimeWorker')) {
            $startInfo.ArgumentList.Add($argument)
        }
        if ($ModulePath) {
            $startInfo.ArgumentList.Add('-ModulePath')
            $startInfo.ArgumentList.Add($ModulePath)
        }
        $worker = [System.Diagnostics.Process]::Start($startInfo)
        try {
            $worker.StandardInput.Close()
            $worker.WaitForExit()
            $exitPath = [System.IO.Path]::Combine($lifetimeDirectory, 'exit')
            [System.IO.File]::WriteAllText($exitPath + '.tmp', $worker.ExitCode.ToString())
            [System.IO.File]::Move($exitPath + '.tmp', $exitPath)
            # Only the owning CLI or parent-EOF watcher ends the anchor's life.
            [System.Threading.Thread]::Sleep([System.Threading.Timeout]::Infinite)
        } finally {
            try {
                $worker.Dispose()
            } finally {
                # setsid above verified that this live anchor owns group $PID.
                # Preserve descendant cleanup if reporting the exit marker fails.
                [PowerBGInfoConfigurationGroup]::kill(-$PID, 9) | Out-Null
            }
        }
        return
    }
}

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
