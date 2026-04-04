$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = (Resolve-Path (Join-Path -Path $scriptRoot -ChildPath '..')).Path
$solutionPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.sln'
$modulePath = Join-Path -Path $repositoryRoot -ChildPath 'PowerBGInfo.psd1'
$cliPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe'
$configPath = Join-Path -Path $scriptRoot -ChildPath 'Configuration\PowerBGInfo.Volumes.json'

& dotnet build $solutionPath -c Debug --nologo | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for $solutionPath"
}

Import-Module -Name $modulePath -Force
if (-not (Test-Path -LiteralPath $cliPath)) {
    throw "Unable to find CLI executable at $cliPath"
}

New-BGInfo {
    New-BGInfoValue -BuiltinValue HostName -Color Red -FontSize 20 -FontFamilyName 'Calibri'
    New-BGInfoLabel -Name 'Volumes' -Color LemonChiffon -FontSize 16 -FontFamilyName 'Calibri'
    New-BGInfoVariable -Name Volumes -Provider Volumes
    New-BGInfoValue -ForEach Volumes -Name 'Drive {{DriveLetter}}' -Value '{{SizeRemaining}} free ({{FreePercent}}%)'
} -FilePath '..\Samples\TapC-Evotec-2560x1080.jpg' `
    -ConfigurationDirectory '..\Output' `
    -OutputFileName 'PowerBGInfo.Volumes.Sample.png' `
    -Target File `
    -TextPosition MiddleCenter `
    -SpaceBetweenColumns 50 `
    -JsonPath $configPath `
    -ExportOnly | Out-Null

& $cliPath --config $configPath --no-apply
