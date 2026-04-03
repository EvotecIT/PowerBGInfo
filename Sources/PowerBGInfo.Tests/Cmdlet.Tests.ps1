$modulePath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.PowerShell\bin\Debug\net8.0-windows\PowerBGInfo.PowerShell.dll'
if (-not (Test-Path -LiteralPath $modulePath)) {
    $modulePath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.PowerShell\bin\Release\net8.0-windows\PowerBGInfo.PowerShell.dll'
}
Import-Module -Name $modulePath -Force

Describe 'New-BGInfoValue cmdlet' {
    It 'creates entry' {
        $entry = New-BGInfoValue -Name 'Test' -Value 'X'
        $entry.Name | Should -Be 'Test'
        $entry.Value | Should -Be 'X'
    }

    It 'resolves builtin values' {
        $entry = New-BGInfoValue -BuiltinValue 'HostName'
        $entry.Name | Should -Be 'HostName'
        $entry.Value | Should -Not -BeNullOrEmpty
        $entry.BuiltinValue | Should -Be 'HostName'
    }
}

Describe 'New-BGInfo cmdlet parameters' {
    It 'supports UseScreenCoordinates' {
        $command = Get-Command New-BGInfo
        $command.Parameters.Keys | Should -Contain 'UseScreenCoordinates'
    }

    It 'supports ValueWrapWidth' {
        $command = Get-Command New-BGInfo
        $command.Parameters.Keys | Should -Contain 'ValueWrapWidth'
    }
}

Describe 'Export-BGInfoConfiguration cmdlet' {
    It 'writes json configuration file' {
        $config = [PowerBGInfo.BgInfoConfiguration]::new()
        $path = Join-Path -Path $TestDrive -ChildPath 'bginfo.json'
        Export-BGInfoConfiguration -InputObject $config -Path $path -Force -PassThru | Should -Be $path
        Test-Path -LiteralPath $path | Should -BeTrue
        (Get-Content -LiteralPath $path -Raw).Length | Should -BeGreaterThan 0
    }

    It 'preserves builtin values in exported json' {
        $config = New-BGInfoConfiguration -Target File
        $config.Entries.Add((New-BGInfoValue -BuiltinValue HostName))
        $path = Join-Path -Path $TestDrive -ChildPath 'builtin.json'

        Export-BGInfoConfiguration -InputObject $config -Path $path -Force | Out-Null

        $json = Get-Content -LiteralPath $path -Raw
        $json | Should -Match '"BuiltinValue"\s*:\s*"HostName"'
        $json | Should -Not -Match '"Value"\s*:\s*"'
    }

    It 'supports multiple pipeline inputs' {
        $config1 = [PowerBGInfo.BgInfoConfiguration]::new()
        $config2 = [PowerBGInfo.BgInfoConfiguration]::new()
        $path = Join-Path -Path $TestDrive -ChildPath 'pipeline.json'

        $result = @((@($config1, $config2) | Export-BGInfoConfiguration -Path $path -Force -PassThru))

        $result.Count | Should -Be 2
        $result | Should -Be @($path, $path)
    }
}

Describe 'New-BGInfoConfiguration cmdlet' {
    It 'creates configuration with overrides' {
        $config = New-BGInfoConfiguration -Target File -MonitorIndex 1 -SpaceX 5 -SpaceY 7 -ValueWrapWidth 240 -ChartLayout Stack -ChartStackAlignToTextBlock -ChartStackOutsideTextBlock
        $config.Target | Should -Be ([PowerBGInfo.BgInfoTarget]::File)
        $config.MonitorIndex | Should -Be 1
        $config.SpaceX | Should -Be 5
        $config.SpaceY | Should -Be 7
        $config.ValueWrapWidth | Should -Be 240
        $config.ChartLayout | Should -Be ([PowerBGInfo.BgInfoChartLayoutMode]::Stack)
        $config.ChartStackAlignToTextBlock | Should -BeTrue
        $config.ChartStackOutsideTextBlock | Should -BeTrue
    }
}

Describe 'Invoke-BGInfo cmdlet' {
    It 'generates output from json without applying wallpaper' {
        $sampleImage = Join-Path -Path $PSScriptRoot -ChildPath '..\..\Examples\Samples\TapC-Evotec-2560x1080.jpg'
        $sampleImage = (Resolve-Path -Path $sampleImage).Path
        $outputDir = Join-Path -Path $TestDrive -ChildPath 'bginfo'
        $configPath = Join-Path -Path $TestDrive -ChildPath 'bginfo.json'
        $config = [ordered]@{
            ConfigurationDirectory = $outputDir
            FilePath               = $sampleImage
            Target                 = 'File'
            Entries                = @(
                [ordered]@{
                    Type         = 'Value'
                    Name         = 'Host'
                    BuiltinValue = 'HostName'
                }
            )
        }
        $config | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $configPath -Encoding UTF8

        $result = Invoke-BGInfo -Path $configPath -NoApply
        Test-Path -LiteralPath $result | Should -BeTrue
    }
}

Describe 'CLI interoperability' {
    It 'renders the same image from PowerShell-exported json' {
        $sampleImage = Join-Path -Path $PSScriptRoot -ChildPath '..\..\Examples\Samples\TapC-Evotec-2560x1080.jpg'
        $sampleImage = (Resolve-Path -Path $sampleImage).Path
        $outputDir = Join-Path -Path $TestDrive -ChildPath 'interop'
        $configPath = Join-Path -Path $TestDrive -ChildPath 'interop.json'
        $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe'
        if (-not (Test-Path -LiteralPath $cliPath)) {
            $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Release\net8.0-windows\PowerBGInfo.Cli.exe'
        }

        $config = New-BGInfoConfiguration -Target File
        $config.FilePath = $sampleImage
        $config.ConfigurationDirectory = $outputDir
        $config.OutputFileName = 'powershell.png'
        $config.Entries.Add((New-BGInfoValue -BuiltinValue HostName))
        Export-BGInfoConfiguration -InputObject $config -Path $configPath -Force | Out-Null

        $psResult = Invoke-BGInfo -Path $configPath -NoApply
        & $cliPath --config $configPath --output cli.png --no-apply | Out-Null
        $cliResult = Join-Path -Path $outputDir -ChildPath 'cli.png'

        (Get-FileHash -LiteralPath $psResult -Algorithm SHA256).Hash | Should -Be ((Get-FileHash -LiteralPath $cliResult -Algorithm SHA256).Hash)
    }
}

Describe 'New-BGInfo legacy output' {
    It 'accepts legacy PSCustomObject entries' {
        $samplePath = Join-Path -Path $PSScriptRoot -ChildPath '..\..\Examples\Samples\TapC-Evotec-2560x1080.jpg'
        $outputDir = Join-Path -Path $TestDrive -ChildPath 'bginfo'
        $legacyEntry = [PSCustomObject]@{
            Type           = 'Values'
            Name           = 'Legacy'
            Value          = 'OK'
            Color          = 'Red'
            FontSize       = 12
            FontFamilyName = 'Calibri'
        }

        $result = New-BGInfo -BGInfoContent { $legacyEntry } -FilePath $samplePath -ConfigurationDirectory $outputDir -Target File
        Test-Path -LiteralPath $result | Should -BeTrue
    }
}
