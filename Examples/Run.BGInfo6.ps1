$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$cliProject = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.Cli\PowerBGInfo.Cli.csproj'
$cliPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe'
$scriptPath = Join-Path -Path $repositoryRoot -ChildPath 'Examples\Scripts\PowerBGInfo.Cli.Sample.ps1'
$jsonPath = Join-Path -Path $repositoryRoot -ChildPath 'Examples\Configuration\PowerBGInfo.Script.Generated.json'
$outputPath = Join-Path -Path $repositoryRoot -ChildPath 'Examples\Output\PowerBGInfo.Script.Sample.png'

dotnet build $cliProject -c Debug | Out-Null

& $cliPath --script $scriptPath --export-json $jsonPath --no-apply

if (-not (Test-Path -LiteralPath $jsonPath)) {
    throw "Expected generated JSON at $jsonPath"
}

if (-not (Test-Path -LiteralPath $outputPath)) {
    throw "Expected generated image at $outputPath"
}

Write-Host "Generated $outputPath"
Write-Host "Exported $jsonPath"
