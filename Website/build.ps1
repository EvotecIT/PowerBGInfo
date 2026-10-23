#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build and serve the powerbginfo.com website.

.PARAMETER Serve
    Build the full pipeline and start the development server.

.PARAMETER Fast
    Run the pipeline with `--fast` (recommended for local iteration).
    When `-Serve` is used, fast mode is enabled by default unless `-NoFast` is set.

.PARAMETER Dev
    Run the pipeline with `--dev` (implies fast mode and enables pipeline mode 'dev').
    When `-Serve` is used, dev mode is enabled by default unless `-NoDev` is set.

.PARAMETER NoFast
    Disable fast mode when `-Serve` is used.

.PARAMETER NoDev
    Disable dev mode when `-Serve` is used.

.PARAMETER Only
    Run only the specified pipeline tasks (comma/semicolon separated), for example: build,verify.

.PARAMETER Skip
    Skip the specified pipeline tasks (comma/semicolon separated), for example: optimize,doctor.

.PARAMETER Watch
    Run the pipeline in watch mode (rebuild on file changes).
    When combined with `-Serve`, starts the static server and keeps rebuilding in the foreground.

.PARAMETER Port
    Server port (default: 8080).

.PARAMETER PowerForgeRoot
    Root folder of PSPublishModule (overrides $env:POWERFORGE_ROOT).

.PARAMETER CI
    Force pipeline CI mode locally (equivalent to setting CI=true). This overrides the implicit fast and dev modes from -Serve.

.EXAMPLE
    ./build.ps1
    ./build.ps1 -CI
    ./build.ps1 -Serve
    ./build.ps1 -Serve -Port 3000
#>

param(
    [switch]$Serve,
    [switch]$Watch,
    [switch]$Dev,
    [switch]$Fast,
    [switch]$NoDev,
    [switch]$NoFast,
    [int]$Port = 8080,
    [string[]]$Only = @(),
    [string[]]$Skip = @(),
    [switch]$CI,
    [switch]$SkipBuildTool,
    [string]$PowerForgeRoot = $env:POWERFORGE_ROOT
)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot

function Get-PowerForgeRootCandidates {
    param(
        [string]$RequestedRoot
    )

    $candidates = @()
    if (-not [string]::IsNullOrWhiteSpace($RequestedRoot)) {
        $candidates += $RequestedRoot
    }
    if (-not [string]::IsNullOrWhiteSpace($env:POWERFORGE_ROOT)) {
        $candidates += $env:POWERFORGE_ROOT
    }

    # Walk up and probe for a sibling PSPublishModule folder (works for common mono-root layouts).
    $here = (Resolve-Path -LiteralPath $PSScriptRoot).Path
    $cursor = $here
    for ($i = 0; $i -lt 8; $i++) {
        $parent = Split-Path -Parent $cursor
        if ([string]::IsNullOrWhiteSpace($parent) -or $parent -eq $cursor) { break }
        $candidates += (Join-Path $parent 'PSPublishModule')
        $cursor = $parent
    }

    $evotecRoot = $env:EVOTEC_GITHUB_ROOT
    if ([string]::IsNullOrWhiteSpace($evotecRoot)) {
        $evotecRoot = if ($IsWindows) {
            'C:\Support\GitHub'
        } else {
            Join-Path $HOME 'Documents/GitHub'
        }
    }
    $candidates += (Join-Path $evotecRoot 'PSPublishModule')

    return $candidates | Select-Object -Unique
}

function Resolve-PowerForgeRoot {
    param(
        [string]$RequestedRoot
    )

    foreach ($candidate in (Get-PowerForgeRootCandidates -RequestedRoot $RequestedRoot)) {
        if ([string]::IsNullOrWhiteSpace($candidate)) { continue }
        try {
            $root = (Resolve-Path -LiteralPath $candidate -ErrorAction Stop).Path
        } catch {
            continue
        }

        $cliProject = Join-Path $root (Join-Path 'PowerForge.Web.Cli' 'PowerForge.Web.Cli.csproj')
        if (Test-Path -LiteralPath $cliProject -PathType Leaf) {
            return $root
        }
    }

    return $null
}

function Get-PowerForgeCliArtifacts {
    param(
        [Parameter(Mandatory)]
        [string]$CliRoot,
        [Parameter(Mandatory)]
        [string[]]$TargetFrameworks
    )

    $artifacts = [ordered]@{
        ReleaseExe = $null
        ReleaseAppHost = $null
        ReleaseDll = $null
        DebugExe = $null
        DebugAppHost = $null
        DebugDll = $null
    }
    foreach ($tfm in $TargetFrameworks) {
        foreach ($configuration in @('Release', 'Debug')) {
            $frameworkPath = Join-Path (Join-Path (Join-Path $CliRoot 'bin') $configuration) $tfm
            $candidateTypes = @(
                @{ Suffix = '.exe'; Property = "${configuration}Exe" },
                @{ Suffix = ''; Property = "${configuration}AppHost" },
                @{ Suffix = '.dll'; Property = "${configuration}Dll" }
            )
            foreach ($candidateType in $candidateTypes) {
                if ($artifacts[$candidateType.Property]) { continue }
                $candidate = Join-Path $frameworkPath ("PowerForge.Web.Cli" + $candidateType.Suffix)
                if (Test-Path -LiteralPath $candidate -PathType Leaf) {
                    $artifacts[$candidateType.Property] = $candidate
                }
            }
        }
    }

    return [pscustomobject]$artifacts
}

# Resolve PowerForge.Web.Cli executable (prefer local fresh build)
$PowerForge = $null
$PowerForgeArgs = @()
$PowerForgeRoot = Resolve-PowerForgeRoot -RequestedRoot $PowerForgeRoot
if (-not [string]::IsNullOrWhiteSpace($PowerForgeRoot)) {
    $PowerForgeCliProject = Join-Path $PowerForgeRoot (Join-Path 'PowerForge.Web.Cli' 'PowerForge.Web.Cli.csproj')
    $cliRoot = Join-Path $PowerForgeRoot 'PowerForge.Web.Cli'

    $tfms = @('net10.0', 'net8.0')
    $cliArtifacts = Get-PowerForgeCliArtifacts -CliRoot $cliRoot -TargetFrameworks $tfms
    $PowerForgeReleaseExe = $cliArtifacts.ReleaseExe
    $PowerForgeReleaseAppHost = $cliArtifacts.ReleaseAppHost
    $PowerForgeReleaseDll = $cliArtifacts.ReleaseDll
    $PowerForgeDebugExe = $cliArtifacts.DebugExe
    $PowerForgeDebugAppHost = $cliArtifacts.DebugAppHost
    $PowerForgeDebugDll = $cliArtifacts.DebugDll
}

if (-not $SkipBuildTool -and $PowerForgeCliProject -and (Test-Path $PowerForgeCliProject)) {
    Write-Host "Building PowerForge.Web.Cli..." -ForegroundColor Cyan
    dotnet build $PowerForgeCliProject -c Release | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "PowerForge.Web.Cli build failed (exit code $LASTEXITCODE)" }
    $cliArtifacts = Get-PowerForgeCliArtifacts -CliRoot $cliRoot -TargetFrameworks $tfms
    $PowerForgeReleaseExe = $cliArtifacts.ReleaseExe
    $PowerForgeReleaseAppHost = $cliArtifacts.ReleaseAppHost
    $PowerForgeReleaseDll = $cliArtifacts.ReleaseDll
    $PowerForgeDebugExe = $cliArtifacts.DebugExe
    $PowerForgeDebugAppHost = $cliArtifacts.DebugAppHost
    $PowerForgeDebugDll = $cliArtifacts.DebugDll
}

if ($PowerForgeReleaseExe -and (Test-Path $PowerForgeReleaseExe)) {
    $PowerForge = $PowerForgeReleaseExe
} elseif ($PowerForgeReleaseAppHost -and (Test-Path $PowerForgeReleaseAppHost)) {
    $PowerForge = $PowerForgeReleaseAppHost
} elseif ($PowerForgeReleaseDll -and (Test-Path $PowerForgeReleaseDll)) {
    $PowerForge = 'dotnet'
    $PowerForgeArgs = @($PowerForgeReleaseDll)
} elseif ($PowerForgeDebugExe -and (Test-Path $PowerForgeDebugExe)) {
    $PowerForge = $PowerForgeDebugExe
} elseif ($PowerForgeDebugAppHost -and (Test-Path $PowerForgeDebugAppHost)) {
    $PowerForge = $PowerForgeDebugAppHost
} elseif ($PowerForgeDebugDll -and (Test-Path $PowerForgeDebugDll)) {
    $PowerForge = 'dotnet'
    $PowerForgeArgs = @($PowerForgeDebugDll)
} else {
    $PowerForge = Get-Command powerforge-web -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
}

if (-not $PowerForge) {
    Write-Error 'PowerForge.Web.Cli not found. Install the global tool (powerforge-web), set POWERFORGE_ROOT (path to PSPublishModule), or pass -PowerForgeRoot.'
    exit 1
}

Write-Host "Using: $PowerForge $($PowerForgeArgs -join ' ')" -ForegroundColor DarkGray

function Assert-SiteOutput {
    param(
        [Parameter(Mandatory)]
        [string]$SiteRoot
    )

    $notFoundPage = Join-Path $SiteRoot '404.html'
    if (-not (Test-Path -LiteralPath $notFoundPage -PathType Leaf)) {
        throw "Build validation failed: expected '$notFoundPage' in the generated site."
    }
}

function Test-PipelineTaskDependsOn {
    param(
        [Parameter(Mandatory)]
        [string]$TaskId,
        [Parameter(Mandatory)]
        [string]$RequiredTaskId,
        [Parameter(Mandatory)]
        [hashtable]$Tasks,
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.HashSet[string]]$Visited
    )

    if ($TaskId -eq $RequiredTaskId) { return $true }
    if (-not $Visited.Add($TaskId) -or -not $Tasks.ContainsKey($TaskId)) { return $false }
    foreach ($dependency in @($Tasks[$TaskId].dependsOn)) {
        if ([string]::IsNullOrWhiteSpace([string]$dependency)) { continue }
        if (Test-PipelineTaskDependsOn -TaskId ([string]$dependency) -RequiredTaskId $RequiredTaskId -Tasks $Tasks -Visited $Visited) {
            return $true
        }
    }
    return $false
}

function Test-SiteOutputRequired {
    param(
        [string[]]$SelectedTask = @(),
        [bool]$StartServer = $false,
        [Parameter(Mandatory)]
        [string]$PipelinePath
    )

    if ($StartServer -or -not $SelectedTask -or $SelectedTask.Count -eq 0) { return $true }

    $pipeline = Get-Content -LiteralPath $pipelinePath -Raw | ConvertFrom-Json
    $tasks = @{}
    if ($pipeline.extends) {
        $basePath = Join-Path (Split-Path -Parent $pipelinePath) ([string]$pipeline.extends)
        $basePipeline = Get-Content -LiteralPath $basePath -Raw | ConvertFrom-Json
        foreach ($task in $basePipeline.steps) { $tasks[[string]$task.id] = $task }
    }
    foreach ($task in $pipeline.steps) { $tasks[[string]$task.id] = $task }

    $selectedTaskIds = @(
        foreach ($selection in $SelectedTask) {
            foreach ($taskId in ($selection -split '[;,]')) {
                if (-not [string]::IsNullOrWhiteSpace($taskId)) { $taskId.Trim() }
            }
        }
    )
    foreach ($taskId in $selectedTaskIds) {
        $visited = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        if (Test-PipelineTaskDependsOn -TaskId $taskId -RequiredTaskId 'build-site' -Tasks $tasks -Visited $visited) { return $true }
    }
    return $false
}

try {
    $IsCI = $CI -or
            ($env:CI -and $env:CI.ToString().ToLowerInvariant() -eq 'true') -or
            ($env:GITHUB_ACTIONS -and $env:GITHUB_ACTIONS.ToString().ToLowerInvariant() -eq 'true') -or
            ($env:TF_BUILD -and $env:TF_BUILD.ToString().ToLowerInvariant() -eq 'true')
    $UseDev = ($Dev -or ($Serve -and -not $NoDev -and -not $IsCI))
    $UseFast = ($Fast -or ($Serve -and -not $NoFast -and -not $IsCI))

    $pipelineArgsBase = @('pipeline', '--config', 'pipeline.json', '--profile')
    if ($UseDev) {
        $pipelineArgsBase += '--dev'
    } elseif ($UseFast) {
        $pipelineArgsBase += '--fast'
    }
    if ($IsCI -and -not $UseDev) {
        # CI contract: run any steps gated behind modes:["ci"] (strict verify, baselines, budgets).
        $pipelineArgsBase += @('--mode', 'ci')
    }
    if ($Only -and $Only.Count -gt 0) {
        $pipelineArgsBase += @('--only', ($Only -join ','))
    }
    if ($Skip -and $Skip.Count -gt 0) {
        $pipelineArgsBase += @('--skip', ($Skip -join ','))
    }

    $pipelineArgs = @($pipelineArgsBase)
    if ($Watch) {
        $pipelineArgs += '--watch'
    }

    $modeLabel = $UseDev ? 'dev' : ($UseFast ? 'fast' : ($IsCI ? 'ci' : 'default'))
    Write-Host "Pipeline mode: $modeLabel" -ForegroundColor DarkGray

    if ($Serve) {
        Write-Host 'Building website...' -ForegroundColor Cyan
        if ($Watch) {
            # Ensure _site exists before we start the server.
            & $PowerForge @PowerForgeArgs @pipelineArgsBase
        } else {
            & $PowerForge @PowerForgeArgs @pipelineArgs
        }
        if ($LASTEXITCODE -ne 0) { throw "Build failed (exit code $LASTEXITCODE)" }
        if (Test-SiteOutputRequired -SelectedTask $Only -StartServer $Serve.IsPresent -PipelinePath (Join-Path $PSScriptRoot 'pipeline.json')) {
            Assert-SiteOutput -SiteRoot (Join-Path $PSScriptRoot '_site')
        }
        Write-Host "Starting dev server on http://localhost:$Port ..." -ForegroundColor Cyan

        $serveArgs = @($PowerForgeArgs + @('serve', '--path', '_site', '--port', $Port))
        $serveProcess = Start-Process -FilePath $PowerForge -ArgumentList $serveArgs -NoNewWindow -PassThru
        try {
            if ($Watch) {
                Write-Host 'Watching for changes...' -ForegroundColor Cyan
                & $PowerForge @PowerForgeArgs @pipelineArgs
            } else {
                Wait-Process -Id $serveProcess.Id
            }
        } finally {
            if ($serveProcess -and -not $serveProcess.HasExited) {
                Stop-Process -Id $serveProcess.Id -Force -ErrorAction SilentlyContinue
            }
        }
    } else {
        Write-Host 'Building website...' -ForegroundColor Cyan
        & $PowerForge @PowerForgeArgs @pipelineArgs
        if ($LASTEXITCODE -ne 0) { throw "Build failed (exit code $LASTEXITCODE)" }
        if (Test-SiteOutputRequired -SelectedTask $Only -StartServer $Serve.IsPresent -PipelinePath (Join-Path $PSScriptRoot 'pipeline.json')) {
            Assert-SiteOutput -SiteRoot (Join-Path $PSScriptRoot '_site')
        }
        Write-Host 'Build complete -> _site/' -ForegroundColor Green
    }
} finally {
    Pop-Location
}
