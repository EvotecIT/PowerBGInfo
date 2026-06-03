Describe 'Packaged AssemblyLoadContext isolation' {
    It 'loads the packaged binary module through the PowerBGInfo ALC' {
        $packagedAlcRequired = $Env:PowerBGInfoPackagedAlcRequired -eq 'true'
        $packagedModuleRoot = Join-Path $PSScriptRoot '..\Artefacts\Unpacked\Modules'
        $packagedModule = Join-Path $packagedModuleRoot 'PowerBGInfo'
        $packagedLoader = Join-Path $packagedModule 'Lib\Core\PowerBGInfo.ModuleLoadContext.dll'
        if ($PSVersionTable.PSEdition -ne 'Core') {
            Set-ItResult -Skipped -Because 'AssemblyLoadContext validation requires PowerShell Core'
            return
        }

        if (-not $packagedAlcRequired) {
            Set-ItResult -Skipped -Because 'packaged AssemblyLoadContext validation runs in the packaged ALC job'
            return
        }

        Test-Path -LiteralPath $packagedLoader | Should -BeTrue -Because 'the packaged build must produce the module-scoped ALC loader'

        $moduleRootLiteral = $packagedModuleRoot.Replace("'", "''")
        $script = @"
`$ErrorActionPreference = 'Stop'
`$WarningPreference = 'SilentlyContinue'
`$moduleRoot = '$moduleRootLiteral'
`$env:PSModulePath = `$moduleRoot + [IO.Path]::PathSeparator + `$env:PSModulePath

Import-Module PowerBGInfo -Force

`$outputDir = Join-Path ([IO.Path]::GetTempPath()) ('powerbginfo-alc-' + [Guid]::NewGuid().ToString('N'))
`$result = New-BGInfo -Target File -ConfigurationDirectory `$outputDir -OutputFileName 'alc.png' -BackgroundColor Black {
    New-BGInfoValue -Name 'ALC' -Value 'PowerBGInfo'
}

`$command = Get-Command New-BGInfo -ErrorAction Stop
`$commandAssembly = `$command.ImplementingType.Assembly
`$commandAlc = [System.Runtime.Loader.AssemblyLoadContext]::GetLoadContext(`$commandAssembly)
`$loadedAssemblies = [System.Runtime.Loader.AssemblyLoadContext]::All |
    ForEach-Object {
        `$alc = `$_
        foreach (`$assembly in `$alc.Assemblies) {
            if (`$assembly.GetName().Name -in @('PowerBGInfo.PowerShell', 'PowerBGInfo', 'DesktopManager', 'ImagePlayground.Gdi')) {
                [pscustomobject]@{
                    Assembly = `$assembly.GetName().Name
                    Version = `$assembly.GetName().Version.ToString()
                    ALC = `$alc.Name
                    IsDefault = [object]::ReferenceEquals(`$alc, [System.Runtime.Loader.AssemblyLoadContext]::Default)
                }
            }
        }
    }

[pscustomobject]@{
    OutputExists = Test-Path -LiteralPath `$result
    NewBGInfoAssembly = `$commandAssembly.Location
    NewBGInfoALC = `$commandAlc.Name
    NewBGInfoALCIsDefault = [object]::ReferenceEquals(`$commandAlc, [System.Runtime.Loader.AssemblyLoadContext]::Default)
    LoadedAssemblies = @(`$loadedAssemblies)
} | ConvertTo-Json -Depth 6 -Compress
"@
        $encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($script))
        $output = pwsh -NoProfile -ExecutionPolicy Bypass -EncodedCommand $encoded 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($output -join [Environment]::NewLine)

        $json = $output | Where-Object { $_ -is [string] -and $_.TrimStart().StartsWith('{') } | Select-Object -Last 1
        $json | Should -Not -BeNullOrEmpty -Because ($output -join [Environment]::NewLine)
        $result = $json | ConvertFrom-Json

        $result.OutputExists | Should -BeTrue
        $result.NewBGInfoAssembly | Should -BeLike '*\Artefacts\Unpacked\Modules\PowerBGInfo\Lib\Core\PowerBGInfo.PowerShell.dll'
        $result.NewBGInfoALC | Should -Be 'PowerBGInfo'
        $result.NewBGInfoALCIsDefault | Should -BeFalse

        $loadedAssemblies = @($result.LoadedAssemblies)
        $powerShellAssembly = $loadedAssemblies | Where-Object Assembly -eq 'PowerBGInfo.PowerShell' | Select-Object -First 1
        $coreAssembly = $loadedAssemblies | Where-Object Assembly -eq 'PowerBGInfo' | Select-Object -First 1

        $powerShellAssembly.ALC | Should -Be 'PowerBGInfo'
        $powerShellAssembly.IsDefault | Should -BeFalse
        $coreAssembly.ALC | Should -Be 'PowerBGInfo'
        $coreAssembly.IsDefault | Should -BeFalse
    }
}
