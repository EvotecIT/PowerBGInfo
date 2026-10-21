<#
.SYNOPSIS
Builds and validates file-only PowerBGInfo workflows on the current platform.
.DESCRIPTION
Runs managed and PowerShell contracts, compares decoded image pixels across CLI and module paths,
and optionally checks local single-file and NativeAOT output. It does not apply desktop settings or publish packages.
Run with Windows PowerShell 5.1 to exercise net472 module commands, or PowerShell 7 to exercise net8.0.
.PARAMETER Configuration
Build configuration used by the module, CLI, and tests.
.PARAMETER DesktopManagerProjectPath
Optional DesktopManager source project used for isolated integration proof before package publication.
.PARAMETER ChartForgeXProjectPath
Optional ChartForgeX core source project; its sibling rendering projects are resolved by the product project.
.PARAMETER RuntimeIdentifier
Runtime for local executable export checks. Defaults to the current host runtime and must be executable here.
.PARAMETER SkipPublishSingleFile
Skips the local single-file executable check.
.PARAMETER SkipPublishAot
Skips the local NativeAOT executable check.
.PARAMETER KeepArtifacts
Retains generated images and executable exports, together with a storage manifest.
.EXAMPLE
./Build/Validate-PowerBGInfo.ps1 -Configuration Release -SkipPublishSingleFile -SkipPublishAot
.OUTPUTS
Validation status and local artifact paths. A failed contract throws.
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',
    [string] $DesktopManagerProjectPath,
    [string] $ChartForgeXProjectPath,
    [string] $RuntimeIdentifier,
    [switch] $SkipPublishSingleFile,
    [switch] $SkipPublishAot,
    [switch] $KeepArtifacts
)

$ErrorActionPreference = 'Stop'

function Assert-PathExists {
    param(
        [string] $Path,
        [string] $Description
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Expected $Description at $Path"
    }
}

function Assert-NativeCommandSucceeded {
    param(
        [int] $ExitCode,
        [string] $Description
    )

    if ($ExitCode -ne 0) {
        throw "$Description failed with exit code $ExitCode."
    }
}

function Get-ImagePixelHash {
    param(
        [string] $Path
    )

    $image = [ChartForgeX.Raster.RasterImageDecoder]::Read($Path)
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        return ([System.BitConverter]::ToString($sha256.ComputeHash($image.Pixels))).Replace('-', '')
    } finally {
        $sha256.Dispose()
    }
}

function Assert-ImageEquivalent {
    param(
        [string] $ReferencePath,
        [string] $CandidatePath,
        [string] $Description
    )

    $referenceHash = Get-ImagePixelHash -Path $ReferencePath
    $candidateHash = Get-ImagePixelHash -Path $CandidatePath
    if ($referenceHash -ne $candidateHash) {
        throw "$Description mismatch. Reference: $ReferencePath Candidate: $CandidatePath"
    }
}

function Invoke-ValidationCli {
    param(
        [string] $Path,
        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]] $Arguments
    )

    if ([IO.Path]::GetExtension($Path) -eq '.dll') {
        & dotnet $Path @Arguments
    } else {
        & $Path @Arguments
    }
    Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'PowerBGInfo CLI invocation'
}

$isWindowsHost = [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT
$moduleFramework = if ($PSVersionTable.PSEdition -eq 'Desktop') { 'net472' } else { 'net8.0' }
$powerShellName = if ($PSVersionTable.PSEdition -eq 'Desktop') {
    'powershell.exe'
} elseif ($isWindowsHost) {
    'pwsh.exe'
} else {
    'pwsh'
}
$powerShellPath = Join-Path -Path $PSHOME -ChildPath $powerShellName
$cliExecutableName = if ($isWindowsHost) { 'PowerBGInfo.Cli.exe' } else { 'PowerBGInfo.Cli' }
if ([string]::IsNullOrWhiteSpace($RuntimeIdentifier)) {
    $RuntimeIdentifier = if ($PSVersionTable.PSEdition -eq 'Desktop') {
        if ($env:PROCESSOR_ARCHITECTURE -eq 'ARM64' -or $env:PROCESSOR_ARCHITEW6432 -eq 'ARM64') {
            'win-arm64'
        } elseif ([Environment]::Is64BitOperatingSystem) {
            'win-x64'
        } else {
            'win-x86'
        }
    } else {
        [Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
    }
}
$sourceProperties = @(
    if (-not [string]::IsNullOrWhiteSpace($DesktopManagerProjectPath)) {
        "-p:DesktopManagerProjectPath=$((Resolve-Path -LiteralPath $DesktopManagerProjectPath).Path)"
    }
    if (-not [string]::IsNullOrWhiteSpace($ChartForgeXProjectPath)) {
        "-p:ChartForgeXProjectPath=$((Resolve-Path -LiteralPath $ChartForgeXProjectPath).Path)"
    }
    if (-not [string]::IsNullOrWhiteSpace($DesktopManagerProjectPath) -or
        -not [string]::IsNullOrWhiteSpace($ChartForgeXProjectPath)) {
        # Source owners outside the solution must retain its requested configuration.
        '-p:ShouldUnsetParentConfigurationAndPlatform=false'
    }
)
if (-not $isWindowsHost) {
    $sourceProperties += '-p:EnableWindowsTargeting=true'
}
$buildProperties = @($sourceProperties)
if (-not $isWindowsHost) {
    $buildProperties += @('--framework', 'net8.0')
}

$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$solutionPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.sln'
$cliProjectPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.Cli\PowerBGInfo.Cli.csproj'
$modulePath = Join-Path -Path $repositoryRoot -ChildPath "Sources/PowerBGInfo.PowerShell/bin/$Configuration/$moduleFramework/PowerBGInfo.PowerShell.dll"
$cliPath = Join-Path -Path $repositoryRoot -ChildPath "Sources/PowerBGInfo.Cli/bin/$Configuration/net8.0/PowerBGInfo.Cli.dll"
$validationScriptPath = Join-Path -Path $repositoryRoot -ChildPath 'Examples\Scripts\PowerBGInfo.Validation.Sample.ps1'
$validationRoot = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath "PowerBGInfo.Validation.$([guid]::NewGuid().ToString('N'))"
$renderDirectory = Join-Path -Path $validationRoot -ChildPath 'renders'
$validationJsonPath = Join-Path -Path $validationRoot -ChildPath 'validation.json'
$scriptOutputPath = Join-Path -Path $renderDirectory -ChildPath 'script.png'
$jsonOutputPath = Join-Path -Path $renderDirectory -ChildPath 'json.png'
$powerShellOutputPath = Join-Path -Path $renderDirectory -ChildPath 'powershell.png'
$powerShellRunnerPath = Join-Path -Path $validationRoot -ChildPath 'invoke-bginfo.ps1'
$singleFileOutputPath = Join-Path -Path $renderDirectory -ChildPath 'singlefile.png'
$aotOutputPath = Join-Path -Path $renderDirectory -ChildPath 'aot.png'
$singleFileDirectory = Join-Path -Path $validationRoot -ChildPath 'cli-singlefile'
$aotDirectory = Join-Path -Path $validationRoot -ChildPath 'cli-aot'
$previousEnvironment = @{}
foreach ($name in @('PowerBGInfoDevelopmentConfiguration', 'POWERBGINFO_TEST_MODULE', 'POWERBGINFO_TEST_CLI')) {
    $previousEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
}

$validationSucceeded = $false
try {
    $Env:PowerBGInfoDevelopmentConfiguration = $Configuration
    $Env:POWERBGINFO_TEST_MODULE = $modulePath
    $Env:POWERBGINFO_TEST_CLI = $cliPath
    New-Item -ItemType Directory -Path $renderDirectory -Force | Out-Null

    dotnet build $solutionPath -c $Configuration @buildProperties
    Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'Solution build'
    dotnet test $solutionPath -c $Configuration --no-build @buildProperties
    Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'Solution test run'

    $pesterPaths = @('Cmdlet.Tests.ps1', 'Features.Tests.ps1', 'WidgetContracts.Tests.ps1') | ForEach-Object {
        Join-Path -Path $repositoryRoot -ChildPath "Sources/PowerBGInfo.Tests/$_"
    }
    $pesterResult = Invoke-Pester -Path $pesterPaths -Output Detailed -PassThru
    if ($pesterResult.Result -ne 'Passed') {
        throw "PowerBGInfo Pester validation failed with result '$($pesterResult.Result)' (tests: $($pesterResult.FailedCount), blocks: $($pesterResult.FailedBlocksCount), containers: $($pesterResult.FailedContainersCount))."
    }

    Assert-PathExists -Path $modulePath -Description 'PowerBGInfo.PowerShell module'
    Assert-PathExists -Path $cliPath -Description 'PowerBGInfo CLI'
    Assert-PathExists -Path $validationScriptPath -Description 'validation script'
    Import-Module -Name $modulePath -Force -ErrorAction Stop

    Invoke-ValidationCli $cliPath --script $validationScriptPath --module $modulePath --pwsh $powerShellPath --directory $renderDirectory --output (Split-Path -Path $scriptOutputPath -Leaf) --export-json $validationJsonPath --no-apply | Out-Null
    Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'Script-backed CLI validation'

    Assert-PathExists -Path $scriptOutputPath -Description 'script-backed CLI output'
    Assert-PathExists -Path $validationJsonPath -Description 'exported validation json'

    Invoke-ValidationCli $cliPath --config $validationJsonPath --directory $renderDirectory --output (Split-Path -Path $jsonOutputPath -Leaf) --no-apply | Out-Null
    Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'JSON-backed CLI validation'
    Assert-PathExists -Path $jsonOutputPath -Description 'json-backed CLI output'

    @"
Import-Module -Name '$($modulePath.Replace("'", "''"))' -Force
Invoke-BGInfo -Path '$($validationJsonPath.Replace("'", "''"))' -ConfigurationDirectory '$($renderDirectory.Replace("'", "''"))' -OutputFileName '$((Split-Path -Path $powerShellOutputPath -Leaf).Replace("'", "''"))' -NoApply | Out-Null
"@ | Set-Content -LiteralPath $powerShellRunnerPath -Encoding UTF8

    & $powerShellPath -NoLogo -NoProfile -File $powerShellRunnerPath | Out-Null
    Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'PowerShell module validation'
    Assert-PathExists -Path $powerShellOutputPath -Description 'Invoke-BGInfo output'

    Assert-ImageEquivalent -ReferencePath $scriptOutputPath -CandidatePath $jsonOutputPath -Description 'CLI script vs CLI json'
    Assert-ImageEquivalent -ReferencePath $jsonOutputPath -CandidatePath $powerShellOutputPath -Description 'CLI json vs PowerShell json'

    if (-not $SkipPublishSingleFile.IsPresent) {
        dotnet publish $cliProjectPath -c $Configuration -f net8.0 -r $RuntimeIdentifier --self-contained false -p:PublishSingleFile=true @sourceProperties -o $singleFileDirectory
        Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'Single-file CLI publish'
        $singleFileCliPath = Join-Path -Path $singleFileDirectory -ChildPath $cliExecutableName
        Assert-PathExists -Path $singleFileCliPath -Description 'single-file CLI'
        Invoke-ValidationCli $singleFileCliPath --config $validationJsonPath --directory $renderDirectory --output (Split-Path -Path $singleFileOutputPath -Leaf) --no-apply | Out-Null
        Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'Single-file CLI validation'
        Assert-PathExists -Path $singleFileOutputPath -Description 'single-file CLI output'
        Assert-ImageEquivalent -ReferencePath $jsonOutputPath -CandidatePath $singleFileOutputPath -Description 'single-file CLI vs normal CLI'
    }

    if (-not $SkipPublishAot.IsPresent) {
        dotnet publish $cliProjectPath -c $Configuration -f net8.0 -r $RuntimeIdentifier -p:PublishProfile=NativeAot @sourceProperties -o $aotDirectory
        Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'NativeAOT CLI publish'
        $aotCliPath = Join-Path -Path $aotDirectory -ChildPath $cliExecutableName
        Assert-PathExists -Path $aotCliPath -Description 'NativeAOT CLI'
        Invoke-ValidationCli $aotCliPath --config $validationJsonPath --directory $renderDirectory --output (Split-Path -Path $aotOutputPath -Leaf) --no-apply | Out-Null
        Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'NativeAOT CLI validation'
        Assert-PathExists -Path $aotOutputPath -Description 'NativeAOT CLI output'
        if ((Get-ImagePixelHash -Path $jsonOutputPath) -ne (Get-ImagePixelHash -Path $aotOutputPath)) {
            Write-Warning 'NativeAOT CLI output differs from the normal CLI output on this machine. NativeAOT remains a smoke-tested file-generation path rather than a strict parity path.'
        }
    }

    $validationSucceeded = $true
    Write-Host "Validated PowerBGInfo with configuration $Configuration, module $moduleFramework, runtime $RuntimeIdentifier"
    Write-Host "Script-backed CLI : $scriptOutputPath"
    Write-Host "JSON-backed CLI   : $jsonOutputPath"
    Write-Host "PowerShell JSON   : $powerShellOutputPath"
    if (-not $SkipPublishSingleFile.IsPresent) {
        Write-Host "Single-file CLI   : $singleFileOutputPath"
    }
    if (-not $SkipPublishAot.IsPresent) {
        Write-Host "NativeAOT CLI     : $aotOutputPath"
    }


} finally {
    foreach ($name in $previousEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($name, $previousEnvironment[$name], 'Process')
    }
    if ($KeepArtifacts.IsPresent -and (Test-Path -LiteralPath $validationRoot)) {
        $artifactBytes = [long](Get-ChildItem -LiteralPath $validationRoot -File -Recurse | Measure-Object -Property Length -Sum).Sum
        [pscustomobject]@{
            Task = 'PowerBGInfo portable workflow validation'
            Repository = $repositoryRoot
            Configuration = $Configuration
            ModuleFramework = $moduleFramework
            RuntimeIdentifier = $RuntimeIdentifier
            Status = if ($validationSucceeded) { 'Validated; retained by KeepArtifacts' } else { 'Failed; retained for diagnosis' }
            LogicalBytes = $artifactBytes
            Purpose = 'CLI/module pixel parity and optional local executable export evidence'
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $validationRoot 'storage-manifest.json') -Encoding UTF8
        Write-Host "Artifacts kept at : $validationRoot ($artifactBytes bytes)"
    }
    if (-not $KeepArtifacts.IsPresent -and (Test-Path -LiteralPath $validationRoot)) {
        Remove-Item -LiteralPath $validationRoot -Recurse
    }
}
