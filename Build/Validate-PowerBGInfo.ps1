[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',
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

    Add-Type -AssemblyName System.Drawing
    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        $bytes = New-Object byte[] ($bitmap.Width * $bitmap.Height * 4)
        $index = 0
        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                $pixel = $bitmap.GetPixel($x, $y)
                $bytes[$index++] = $pixel.A
                $bytes[$index++] = $pixel.R
                $bytes[$index++] = $pixel.G
                $bytes[$index++] = $pixel.B
            }
        }

        return ([System.BitConverter]::ToString(([System.Security.Cryptography.SHA256]::HashData($bytes)))).Replace('-', '')
    } finally {
        $bitmap.Dispose()
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

$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$solutionPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.sln'
$cliProjectPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.Cli\PowerBGInfo.Cli.csproj'
$modulePath = Join-Path -Path $repositoryRoot -ChildPath "Sources\PowerBGInfo.PowerShell\bin\$Configuration\net8.0-windows\PowerBGInfo.PowerShell.dll"
$cliPath = Join-Path -Path $repositoryRoot -ChildPath "Sources\PowerBGInfo.Cli\bin\$Configuration\net8.0-windows\PowerBGInfo.Cli.exe"
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
$hadDevelopmentConfiguration = Test-Path -LiteralPath Env:PowerBGInfoDevelopmentConfiguration
$previousDevelopmentConfiguration = $Env:PowerBGInfoDevelopmentConfiguration

try {
    $Env:PowerBGInfoDevelopmentConfiguration = $Configuration
    New-Item -ItemType Directory -Path $renderDirectory -Force | Out-Null

    dotnet build $solutionPath -c $Configuration | Out-Null
    Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'Solution build'
    dotnet test $solutionPath -c $Configuration --no-build | Out-Null
    Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'Solution test run'

    $pesterResult = Invoke-Pester -Path (Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.Tests\Cmdlet.Tests.ps1') -Output Detailed -PassThru
    if ($pesterResult.FailedCount -gt 0) {
        throw "$($pesterResult.FailedCount) PowerBGInfo Pester test(s) failed."
    }

    Assert-PathExists -Path $modulePath -Description 'PowerBGInfo.PowerShell module'
    Assert-PathExists -Path $cliPath -Description 'PowerBGInfo CLI'
    Assert-PathExists -Path $validationScriptPath -Description 'validation script'

    & $cliPath --script $validationScriptPath --module $modulePath --directory $renderDirectory --output (Split-Path -Path $scriptOutputPath -Leaf) --export-json $validationJsonPath --no-apply | Out-Null
    Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'Script-backed CLI validation'

    Assert-PathExists -Path $scriptOutputPath -Description 'script-backed CLI output'
    Assert-PathExists -Path $validationJsonPath -Description 'exported validation json'

    & $cliPath --config $validationJsonPath --directory $renderDirectory --output (Split-Path -Path $jsonOutputPath -Leaf) --no-apply | Out-Null
    Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'JSON-backed CLI validation'
    Assert-PathExists -Path $jsonOutputPath -Description 'json-backed CLI output'

    @"
Import-Module -Name '$($modulePath.Replace("'", "''"))' -Force
Invoke-BGInfo -Path '$($validationJsonPath.Replace("'", "''"))' -ConfigurationDirectory '$($renderDirectory.Replace("'", "''"))' -OutputFileName '$((Split-Path -Path $powerShellOutputPath -Leaf).Replace("'", "''"))' -NoApply | Out-Null
"@ | Set-Content -LiteralPath $powerShellRunnerPath -Encoding UTF8

    pwsh -NoLogo -NoProfile -File $powerShellRunnerPath | Out-Null
    Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'PowerShell module validation'
    Assert-PathExists -Path $powerShellOutputPath -Description 'Invoke-BGInfo output'

    Assert-ImageEquivalent -ReferencePath $scriptOutputPath -CandidatePath $jsonOutputPath -Description 'CLI script vs CLI json'
    Assert-ImageEquivalent -ReferencePath $jsonOutputPath -CandidatePath $powerShellOutputPath -Description 'CLI json vs PowerShell json'

    if (-not $SkipPublishSingleFile.IsPresent) {
        dotnet publish $cliProjectPath -c $Configuration -f net8.0-windows -r win-x64 --self-contained false -p:PublishSingleFile=true -o $singleFileDirectory | Out-Null
        Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'Single-file CLI publish'
        $singleFileCliPath = Join-Path -Path $singleFileDirectory -ChildPath 'PowerBGInfo.Cli.exe'
        Assert-PathExists -Path $singleFileCliPath -Description 'single-file CLI'
        & $singleFileCliPath --config $validationJsonPath --directory $renderDirectory --output (Split-Path -Path $singleFileOutputPath -Leaf) --no-apply | Out-Null
        Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'Single-file CLI validation'
        Assert-PathExists -Path $singleFileOutputPath -Description 'single-file CLI output'
        Assert-ImageEquivalent -ReferencePath $jsonOutputPath -CandidatePath $singleFileOutputPath -Description 'single-file CLI vs normal CLI'
    }

    if (-not $SkipPublishAot.IsPresent) {
        dotnet publish $cliProjectPath -c $Configuration -f net8.0-windows -r win-x64 -p:PublishAot=true -o $aotDirectory | Out-Null
        Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'NativeAOT CLI publish'
        $aotCliPath = Join-Path -Path $aotDirectory -ChildPath 'PowerBGInfo.Cli.exe'
        Assert-PathExists -Path $aotCliPath -Description 'NativeAOT CLI'
        & $aotCliPath --config $validationJsonPath --directory $renderDirectory --output (Split-Path -Path $aotOutputPath -Leaf) --no-apply | Out-Null
        Assert-NativeCommandSucceeded -ExitCode $LASTEXITCODE -Description 'NativeAOT CLI validation'
        Assert-PathExists -Path $aotOutputPath -Description 'NativeAOT CLI output'
        if ((Get-ImagePixelHash -Path $jsonOutputPath) -ne (Get-ImagePixelHash -Path $aotOutputPath)) {
            Write-Warning 'NativeAOT CLI output differs from the normal CLI output on this machine. NativeAOT remains a smoke-tested file-generation path rather than a strict parity path.'
        }
    }

    Write-Host "Validated PowerBGInfo with configuration $Configuration"
    Write-Host "Script-backed CLI : $scriptOutputPath"
    Write-Host "JSON-backed CLI   : $jsonOutputPath"
    Write-Host "PowerShell JSON   : $powerShellOutputPath"
    if (-not $SkipPublishSingleFile.IsPresent) {
        Write-Host "Single-file CLI   : $singleFileOutputPath"
    }
    if (-not $SkipPublishAot.IsPresent) {
        Write-Host "NativeAOT CLI     : $aotOutputPath"
    }

    if ($KeepArtifacts.IsPresent) {
        Write-Host "Artifacts kept at : $validationRoot"
    }
} finally {
    if ($hadDevelopmentConfiguration) {
        $Env:PowerBGInfoDevelopmentConfiguration = $previousDevelopmentConfiguration
    } else {
        Remove-Item -LiteralPath Env:PowerBGInfoDevelopmentConfiguration -ErrorAction SilentlyContinue
    }
    if (-not $KeepArtifacts.IsPresent -and (Test-Path -LiteralPath $validationRoot)) {
        Remove-Item -LiteralPath $validationRoot -Recurse -Force
    }
}
