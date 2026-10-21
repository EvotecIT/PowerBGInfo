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
