[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $BaselineDirectory,

    [Parameter(Mandatory)]
    [string] $CandidateDirectory,

    [Parameter(Mandatory)]
    [string] $OutputDirectory,

    [switch] $Recursive,
    [switch] $AllowMissing,
    [double] $MeanThreshold = 1.25,
    [double] $RmseThreshold = 3.0,
    [int] $MaxChannelThreshold = 48,
    [double] $ChangedPixelPercentThreshold = 2.0,
    [int] $DiffScale = 4,
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$projectPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.QualityGate\PowerBGInfo.QualityGate.csproj'

$arguments = @(
    'run',
    '--project', $projectPath,
    '-c', $Configuration,
    '--',
    '--baseline', (Resolve-Path -LiteralPath $BaselineDirectory).Path,
    '--candidate', (Resolve-Path -LiteralPath $CandidateDirectory).Path,
    '--output', $OutputDirectory,
    '--mean-threshold', $MeanThreshold.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    '--rmse-threshold', $RmseThreshold.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    '--max-channel-threshold', $MaxChannelThreshold.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    '--changed-pixel-percent-threshold', $ChangedPixelPercentThreshold.ToString([System.Globalization.CultureInfo]::InvariantCulture),
    '--diff-scale', $DiffScale.ToString([System.Globalization.CultureInfo]::InvariantCulture)
)

if ($Recursive.IsPresent) {
    $arguments += '--recursive'
}

if ($AllowMissing.IsPresent) {
    $arguments += '--allow-missing'
}

& dotnet @arguments
exit $LASTEXITCODE
