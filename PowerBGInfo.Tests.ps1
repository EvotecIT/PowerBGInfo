$ModuleName = (Get-ChildItem $PSScriptRoot\*.psd1).BaseName
$PrimaryModule = Get-ChildItem -Path $PSScriptRoot -Filter '*.psd1' -Recurse -ErrorAction SilentlyContinue -Depth 1
if (-not $PrimaryModule) {
    throw "Path $PSScriptRoot doesn't contain PSD1 files. Failing tests."
}
if ($PrimaryModule.Count -ne 1) {
    throw 'More than one PSD1 files detected. Failing tests.'
}
$PSDInformation = Import-PowerShellDataFile -Path $PrimaryModule.FullName
$RequiredModules = @(
    'Pester'
    'PSWriteColor'
    'PowerBgInfo'
    if ($PSDInformation.RequiredModules) {
        $PSDInformation.RequiredModules
    }
)
foreach ($Module in $RequiredModules) {
    if ($Module -eq 'PowerBgInfo') {
        continue
    }
    if ($Module -eq 'Pester') {
        $Pester5 = Get-Module -ListAvailable -Name Pester | Where-Object { $_.Version.Major -eq 5 }
        if (-not $Pester5) {
            Write-Warning "$ModuleName - Installing Pester 5 from PSGallery"
            Install-Module -Name Pester -MinimumVersion 5.0.0 -MaximumVersion 5.99.99 -Force -SkipPublisherCheck -AllowClobber
        }
        continue
    }
    if ($Module -is [System.Collections.IDictionary]) {
        $Exists = Get-Module -ListAvailable -Name $Module.ModuleName
        if (-not $Exists) {
            Write-Warning "$ModuleName - Downloading $($Module.ModuleName) from PSGallery"
            Install-Module -Name $Module.ModuleName -Force -SkipPublisherCheck
        }
    } else {
        $Exists = Get-Module -ListAvailable $Module -ErrorAction SilentlyContinue
        if (-not $Exists) {
            Install-Module -Name $Module -Force -SkipPublisherCheck
        }
    }
}

Import-Module Pester -MinimumVersion 5.0.0 -MaximumVersion 5.99.99 -Force

Write-Color 'ModuleName: ', $ModuleName, ' Version: ', $PSDInformation.ModuleVersion -Color Yellow, Green, Yellow, Green -LinesBefore 2
Write-Color 'PowerShell Version: ', $PSVersionTable.PSVersion -Color Yellow, Green
Write-Color 'PowerShell Edition: ', $PSVersionTable.PSEdition -Color Yellow, Green
Write-Color 'Required modules: ' -Color Yellow
foreach ($Module in $PSDInformation.RequiredModules) {
    if ($Module -is [System.Collections.IDictionary]) {
        Write-Color '   [>] ', $Module.ModuleName, ' Version: ', $Module.ModuleVersion -Color Yellow, Green, Yellow, Green
    } else {
        Write-Color '   [>] ', $Module -Color Yellow, Green
    }
}
Write-Color

Import-Module $PSScriptRoot\*.psd1 -Force
$result = Invoke-Pester -Path @(
    "$PSScriptRoot\Tests"
    "$PSScriptRoot\Sources\PowerBGInfo.Tests\Cmdlet.Tests.ps1"
    "$PSScriptRoot\Sources\PowerBGInfo.Tests\Features.Tests.ps1"
) -Verbose -PassThru

$failedCount = if ($result.PSObject.Properties.Name -contains 'FailedCount') { [int]$result.FailedCount } else { 0 }
$failedBlocksCount = if ($result.PSObject.Properties.Name -contains 'FailedBlocksCount') { [int]$result.FailedBlocksCount } else { 0 }
$failedContainersCount = if ($result.PSObject.Properties.Name -contains 'FailedContainersCount') { [int]$result.FailedContainersCount } else { 0 }
$failedResult = $result.PSObject.Properties.Name -contains 'Result' -and $result.Result -ne 'Passed'
if ($failedResult -or $failedCount -gt 0 -or $failedBlocksCount -gt 0 -or $failedContainersCount -gt 0) {
    throw "PowerBGInfo tests failed: $failedCount tests, $failedBlocksCount blocks, $failedContainersCount containers."
}
