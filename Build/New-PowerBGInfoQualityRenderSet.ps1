[CmdletBinding()]
param(
    [string[]] $ConfigurationPath,

    [Parameter(Mandatory)]
    [string] $OutputDirectory,

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [switch] $NoBuild
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$solutionPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.sln'
$cliPath = Join-Path -Path $repositoryRoot -ChildPath "Sources\PowerBGInfo.Cli\bin\$Configuration\net8.0-windows\PowerBGInfo.Cli.exe"

if (-not $ConfigurationPath -or $ConfigurationPath.Count -eq 0) {
    $ConfigurationPath = Get-ChildItem -LiteralPath (Join-Path -Path $repositoryRoot -ChildPath 'Examples\Configuration') -Filter '*.json' |
        Sort-Object Name |
        ForEach-Object { $_.FullName }
}

if (-not $NoBuild.IsPresent) {
    dotnet build $solutionPath -c $Configuration | Out-Null
}

if (-not (Test-Path -LiteralPath $cliPath)) {
    throw "PowerBGInfo CLI was not found at $cliPath. Build the solution first or omit -NoBuild."
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$rendered = foreach ($path in $ConfigurationPath) {
    $resolvedPath = (Resolve-Path -LiteralPath $path).Path
    $json = Get-Content -LiteralPath $resolvedPath -Raw | ConvertFrom-Json
    $configuredName = if ($json.OutputFileName) { [string] $json.OutputFileName } else { [System.IO.Path]::GetFileNameWithoutExtension($resolvedPath) + '.png' }
    $extension = [System.IO.Path]::GetExtension($configuredName)
    if ([string]::IsNullOrWhiteSpace($extension)) {
        $extension = '.png'
    }

    $outputName = [System.IO.Path]::GetFileNameWithoutExtension($resolvedPath) + $extension
    & $cliPath --config $resolvedPath --directory $OutputDirectory --output $outputName --no-apply | Out-Null
    $outputPath = Join-Path -Path $OutputDirectory -ChildPath $outputName
    if (-not (Test-Path -LiteralPath $outputPath)) {
        throw "Expected render output at $outputPath"
    }

    [pscustomobject]@{
        Configuration = $resolvedPath
        Output = (Resolve-Path -LiteralPath $outputPath).Path
    }
}

$manifestPath = Join-Path -Path $OutputDirectory -ChildPath 'render-set.json'
$rendered | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
$rendered
Write-Host "Render set manifest: $manifestPath"
