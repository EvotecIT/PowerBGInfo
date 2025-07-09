Import-Module "$PSScriptRoot/../PowerBGInfo.PowerShell/bin/Debug/net8.0/PowerBGInfo.PowerShell.dll" -Force

Describe 'New-BGInfoValue cmdlet' {
    It 'creates entry' {
        $entry = New-BGInfoValue -Name Test -Value X
        $entry.Name | Should -Be 'Test'
        $entry.Value | Should -Be 'X'
    }
}

Describe 'New-BGInfo cmdlet parameters' {
    It 'supports UseScreenCoordinates' {
        $command = Get-Command New-BGInfo
        $command.Parameters.Keys | Should -Contain 'UseScreenCoordinates'
    }
}
