Import-Module PSPublishModule -Force -ErrorAction Stop

$Path = "$PSScriptRoot\.."

Get-ProjectVersion -Path "$Path" -ExcludeFolders @("Artefacts") | Format-Table
Set-ProjectVersion -Path "$Path" -NewVersion "3.5.0" -WhatIf:$false -Verbose -ExcludeFolders @("Artefacts") | Format-Table
