BeforeAll {
    $modulePath = $Env:POWERBGINFO_TEST_MODULE
    if ([string]::IsNullOrWhiteSpace($modulePath)) {
        $modulePath = Join-Path $PSScriptRoot '../PowerBGInfo.psd1'
    }
    Import-Module -Name $modulePath -Force -ErrorAction Stop

    function Get-TestPngSize {
        param([string] $Path)
        # Read the artifact header without depending on types isolated in the packaged module's ALC.
        $bytes = [IO.File]::ReadAllBytes($Path)
        $bytes.Length | Should -BeGreaterOrEqual 24
        [BitConverter]::ToString($bytes, 0, 8) | Should -Be '89-50-4E-47-0D-0A-1A-0A'
        [pscustomobject]@{
            Width = ($bytes[16] -shl 24) -bor ($bytes[17] -shl 16) -bor ($bytes[18] -shl 8) -bor $bytes[19]
            Height = ($bytes[20] -shl 24) -bor ($bytes[21] -shl 16) -bor ($bytes[22] -shl 8) -bor $bytes[23]
        }
    }
}

Describe 'PowerShell filesystem locations' {
    BeforeAll {
        $fixtureDirectory = Join-Path $TestDrive 'fixtures'
        $sourceImage = New-BGInfo -Target File -ConfigurationDirectory $fixtureDirectory -OutputFileName 'source.png' {
            New-BGInfoBackground -Width 64 -Height 40 -Color '#123456'
        }
        $decoyImage = New-BGInfo -Target File -ConfigurationDirectory $fixtureDirectory -OutputFileName 'decoy.png' {
            New-BGInfoBackground -Width 96 -Height 48 -Color '#654321'
        }
    }

    BeforeEach {
        $originalLocation = Get-Location
        $originalProcessDirectory = [Environment]::CurrentDirectory
        $caseDirectory = Join-Path $TestDrive ([Guid]::NewGuid().ToString('N'))
        $powerShellDirectory = Join-Path $caseDirectory 'powershell'
        $processDirectory = Join-Path $caseDirectory 'process'
        $null = [IO.Directory]::CreateDirectory($powerShellDirectory)
        $null = [IO.Directory]::CreateDirectory($processDirectory)
        Copy-Item -LiteralPath $sourceImage -Destination (Join-Path $powerShellDirectory 'base.png')
        Copy-Item -LiteralPath $decoyImage -Destination (Join-Path $processDirectory 'base.png')
        Set-Location -LiteralPath $powerShellDirectory
        [Environment]::CurrentDirectory = $processDirectory
    }

    AfterEach {
        Set-Location -LiteralPath $originalLocation.Path
        [Environment]::CurrentDirectory = $originalProcessDirectory
    }

    It 'renders an inline image with output path <OutputName> relative to the configuration directory' -TestCases @(
        @{ OutputName = 'nested/direct.png'; ExpectedOutput = 'inline/nested/direct.png' }
        @{ OutputName = '../sibling/direct.png'; ExpectedOutput = 'sibling/direct.png' }
    ) {
        param($OutputName, $ExpectedOutput)
        $result = New-BGInfo -FilePath './base.png' -ConfigurationDirectory './inline' -OutputFileName $OutputName -Target File { }

        [IO.Path]::GetFullPath($result) | Should -Be (Join-Path $powerShellDirectory $ExpectedOutput)
        $image = Get-TestPngSize $result
        $image.Width | Should -Be 64
        $image.Height | Should -Be 40
        [Environment]::CurrentDirectory | Should -Be $processDirectory
    }

    It 'exports, loads and renders a configuration authored from the PowerShell location' {
        $config = New-BGInfoConfiguration -FilePath './base.png' -ConfigurationDirectory './configured' -OutputFileName 'configured.png' -Target File
        $exported = Export-BGInfoConfiguration -InputObject $config -Path './configuration/config.json' -PassThru
        $result = Invoke-BGInfo -Path './configuration/config.json' -NoApply

        $exported | Should -Be (Join-Path $powerShellDirectory 'configuration/config.json')
        $result | Should -Be (Join-Path $powerShellDirectory 'configured/configured.png')
        $image = Get-TestPngSize $result
        $image.Width | Should -Be 64
        $image.Height | Should -Be 40
    }

    It 'keeps JSON paths relative to the JSON file while resolving command overrides from PowerShell' {
        $jsonDirectory = Join-Path $powerShellDirectory 'json'
        $null = [IO.Directory]::CreateDirectory($jsonDirectory)
        @{
            FilePath = '../base.png'
            ConfigurationDirectory = 'images'
            OutputFileName = 'nested/json.png'
            Target = 'File'
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $jsonDirectory 'config.json') -Encoding UTF8

        $original = Invoke-BGInfo -Path './json/config.json' -NoApply
        $overridden = Invoke-BGInfo -Path './json/config.json' -ConfigurationDirectory './override' -OutputFileName 'nested/override.png' -NoApply

        $original | Should -Be (Join-Path $jsonDirectory 'images/nested/json.png')
        $overridden | Should -Be (Join-Path $powerShellDirectory 'override/nested/override.png')
        foreach ($path in @($original, $overridden)) {
            $image = Get-TestPngSize $path
            $image.Width | Should -Be 64
            $image.Height | Should -Be 40
        }
    }

    It 'resolves filesystem drive paths for base images, output directories and explicit output paths' {
        $driveName = 'BGInfo' + [Guid]::NewGuid().ToString('N')
        $null = New-PSDrive -Name $driveName -PSProvider FileSystem -Root $powerShellDirectory
        try {
            $result = New-BGInfo -FilePath "${driveName}:base.png" -ConfigurationDirectory "${driveName}:output" `
                -OutputFileName "${driveName}:qualified/result.png" -Target File { }

            $result | Should -Be (Join-Path $powerShellDirectory 'qualified/result.png')
            $image = Get-TestPngSize $result
            $image.Width | Should -Be 64
            $image.Height | Should -Be 40
        } finally {
            Remove-PSDrive -Name $driveName
        }
    }

    It 'rejects non-filesystem <Command> <Parameter> paths before writing output' -TestCases @(
        @{ Command = 'New-BGInfo'; Parameter = 'FilePath' }
        @{ Command = 'New-BGInfo'; Parameter = 'ConfigurationDirectory' }
        @{ Command = 'New-BGInfo'; Parameter = 'OutputFileName' }
        @{ Command = 'New-BGInfo'; Parameter = 'JsonPath' }
        @{ Command = 'New-BGInfoConfiguration'; Parameter = 'FilePath' }
        @{ Command = 'New-BGInfoConfiguration'; Parameter = 'ConfigurationDirectory' }
        @{ Command = 'New-BGInfoConfiguration'; Parameter = 'OutputFileName' }
        @{ Command = 'Invoke-BGInfo'; Parameter = 'Path' }
        @{ Command = 'Invoke-BGInfo'; Parameter = 'ConfigurationDirectory' }
        @{ Command = 'Invoke-BGInfo'; Parameter = 'OutputFileName' }
    ) {
        param($Command, $Parameter)

        $outputDirectory = Join-Path $powerShellDirectory 'not-created'
        $arguments = @{
            ConfigurationDirectory = $outputDirectory
            OutputFileName = 'result.png'
            ErrorAction = 'Stop'
        }
        if ($Command -eq 'Invoke-BGInfo') {
            $configurationPath = Join-Path $powerShellDirectory 'valid.json'
            New-BGInfoConfiguration -Target File -Background (New-BGInfoBackground -Width 64 -Height 40) |
                Export-BGInfoConfiguration -Path $configurationPath
            $arguments.Path = $configurationPath
            $arguments.NoApply = $true
        } else {
            $arguments.Target = 'File'
            if ($Command -eq 'New-BGInfo') {
                $arguments.BGInfoContent = { }
                $arguments.JsonPath = './not-created.json'
                $arguments.ExportOnly = $true
            }
        }
        $arguments[$Parameter] = 'Env:PATH'

        { & $Command @arguments } | Should -Throw '*FileSystem path*'
        Test-Path -LiteralPath $outputDirectory | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $powerShellDirectory 'not-created.json') | Should -BeFalse
    }
}
