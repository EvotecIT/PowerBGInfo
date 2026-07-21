$modulePath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.PowerShell\bin\Debug\net8.0-windows\PowerBGInfo.PowerShell.dll'
if (-not (Test-Path -LiteralPath $modulePath)) {
    $modulePath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.PowerShell\bin\Release\net8.0-windows\PowerBGInfo.PowerShell.dll'
}
Import-Module -Name $modulePath -Force

BeforeAll {
function Get-ImagePixelHash {
    param(
        [string] $Path
    )

    Add-Type -AssemblyName System.Drawing
    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        $bytes = New-Object byte[] ($bitmap.Width * $bitmap.Height * 4)
        $index = 0
        for ($y = 0; $y -lt $bitmap.Height; $y++) {
            for ($x = 0; $x -lt $bitmap.Width; $x++) {
                $pixel = $bitmap.GetPixel($x, $y)
                $bytes[$index++] = $pixel.A
                $bytes[$index++] = $pixel.R
                $bytes[$index++] = $pixel.G
                $bytes[$index++] = $pixel.B
            }
        }

        return ([System.BitConverter]::ToString(([System.Security.Cryptography.SHA256]::HashData($bytes)))).Replace('-', '')
    } finally {
        $bitmap.Dispose()
    }
}
}

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

    It 'resolves WMI-backed builtin values in the PowerShell host' {
        $entry = New-BGInfoValue -BuiltinValue 'OSName'
        $entry.Name | Should -Be 'OSName'
        $entry.Value | Should -Not -BeNullOrEmpty
        $entry.BuiltinValue | Should -Be 'OSName'
    }

    It 'creates template entries for foreach variables' {
        $entry = New-BGInfoValue -ForEach 'Volumes' -Name 'Drive {{DriveLetter}}' -Value '{{SizeRemaining}}'
        $entry.ForEach | Should -Be 'Volumes'
        $entry.Name | Should -Be 'Drive {{DriveLetter}}'
        $entry.Value | Should -Be '{{SizeRemaining}}'
        $entry.BuiltinValue | Should -BeNullOrEmpty
    }
}

Describe 'New-BGInfoLabel cmdlet' {
    It 'accepts bare RGB hex color strings' {
        $entry = New-BGInfoLabel -Name 'Test' -Color 'ffffff'

        $entry.Color.A | Should -Be 255
        $entry.Color.R | Should -Be 255
        $entry.Color.G | Should -Be 255
        $entry.Color.B | Should -Be 255
    }

    It 'accepts ChartForgeX color tokens' {
        $entry = New-BGInfoLabel -Name 'Test' -Color 'Emerald400'

        $entry.Color.A | Should -Be 255
        $entry.Color.R | Should -Be 52
        $entry.Color.G | Should -Be 211
        $entry.Color.B | Should -Be 153
    }

    It 'accepts ChartForgeX named colors' {
        $entry = New-BGInfoLabel -Name 'Test' -Color 'White'

        $entry.Color.A | Should -Be 255
        $entry.Color.R | Should -Be 255
        $entry.Color.G | Should -Be 255
        $entry.Color.B | Should -Be 255
    }

    It 'rejects unknown color names' {
        { New-BGInfoLabel -Name 'Test' -Color 'definitely-not-a-color' -ErrorAction Stop } | Should -Throw
    }

    It 'rejects System.Drawing.Color values' {
        { New-BGInfoLabel -Name 'Test' -Color ([System.Drawing.Color]::White) -ErrorAction Stop } | Should -Throw
    }

    It 'rejects packed integer colors' {
        { New-BGInfoLabel -Name 'Test' -Color 0xFFFFFFFF -ErrorAction Stop } | Should -Throw
    }
}

Describe 'New-BGInfoVariable cmdlet' {
    It 'creates provider-backed variables' {
        $variable = New-BGInfoVariable -Name Volumes -Provider Volumes
        $variable.Name | Should -Be 'Volumes'
        $variable.Provider.ToString() | Should -Be 'Volumes'
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

    It 'supports JsonPath export' {
        $command = Get-Command New-BGInfo
        $command.Parameters.Keys | Should -Contain 'JsonPath'
        $command.Parameters.Keys | Should -Contain 'ExportOnly'
        $command.Parameters.Keys | Should -Contain 'PassThru'
    }

    It 'supports chart stack options' {
        $command = Get-Command New-BGInfo
        $command.Parameters.Keys | Should -Contain 'ChartLayout'
        $command.Parameters.Keys | Should -Contain 'ChartStackAlignToTextBlock'
        $command.Parameters.Keys | Should -Contain 'ChartStackOutsideTextBlock'
    }

    It 'supports disabling wallpaper slideshow preservation' {
        $command = Get-Command New-BGInfo
        $command.Parameters.Keys | Should -Contain 'DisableWallpaperSlideshow'
    }

    It 'supports recipe variables through inline content' {
        $command = Get-Command New-BGInfoVariable
        $command.Parameters.Keys | Should -Contain 'Provider'
    }

    It 'exports visual canvas helper cmdlets' {
        Get-Command New-BGInfoVisualCanvas | Should -Not -BeNullOrEmpty
        Get-Command New-BGInfoVisualCanvasTile | Should -Not -BeNullOrEmpty
        Get-Command New-BGInfoVisualCanvasFeature | Should -Not -BeNullOrEmpty
        Get-Command New-BGInfoImage | Should -Not -BeNullOrEmpty
    }

    It 'supports visual canvas theme parameters' {
        $command = Get-Command New-BGInfoVisualCanvas
        $command.Parameters.Keys | Should -Contain 'TitleColor'
        $command.Parameters.Keys | Should -Contain 'TileValueColor'
        $command.Parameters.Keys | Should -Contain 'HeroBadgeTextColor'
        $command.Parameters.Keys | Should -Contain 'NoHeroBadge'
        $command.Parameters.Keys | Should -Contain 'HeroBadgeText'
        $command.Parameters.Keys | Should -Contain 'HeroBadgeImagePath'
        $command.Parameters.Keys | Should -Contain 'HeroBadgeImageFit'
        $command.Parameters.Keys | Should -Contain 'LayoutPreset'
        $command.Parameters.Keys | Should -Contain 'TileWidth'
        $command.Parameters.Keys | Should -Contain 'TileHeight'
        $command.Parameters.Keys | Should -Contain 'TileGap'
        $command.Parameters.Keys | Should -Contain 'LeftTileWidth'
        $command.Parameters.Keys | Should -Contain 'RightTileWidth'
        $command.Parameters.Keys | Should -Contain 'TileTextFitPolicy'
        (Get-Command New-BGInfoVisualCanvasTile).Parameters.Keys | Should -Contain 'MiniChartKind'
        (Get-Command New-BGInfoVisualCanvasTile).Parameters.Keys | Should -Contain 'Width'
        (Get-Command New-BGInfoVisualCanvasTile).Parameters.Keys | Should -Contain 'Height'
        (Get-Command New-BGInfoVisualCanvasTile).Parameters.Keys | Should -Contain 'TextFitPolicy'
    }
}

Describe 'New-BGInfoVisualCanvas cmdlets' {
    It 'creates a visual canvas model' {
        $tile = New-BGInfoVisualCanvasTile -Side Left -Icon PC -Label HOSTNAME -Value '{{HostName}}' -Detail '{{OSName}}' -Width 460 -Height 144 -Progress 0.25 -SurfaceStyle Raised -IconKind Computer -MiniChartKind Sparkline -TextFitPolicy SingleLineEllipsis -MiniChartValues 18,26,22 -MiniChartMaximum 100
        $feature = New-BGInfoVisualCanvasFeature -Icon PS -Label 'LIGHTWEIGHT'

        $path = Join-Path -Path $TestDrive -ChildPath 'logo.png'
        Set-Content -LiteralPath $path -Value 'not-a-real-rendered-image'

        $visual = New-BGInfoVisualCanvas -Title PowerBGInfo -Subtitle 'Desktop insights' -Width 1200 -Height 630 -TitleColor White -TileValueColor '#F8FAFC' -HeroBadgeTextColor AliceBlue -HeroBadgeText EV -HeroBadgeImagePath $path -HeroBadgeImageFit Cover -HeroBadgeImagePadding 14 -HeroBadgeImageOpacity 0.82 -FeatureAnchor BottomRight -FeatureWidth 610 -FeatureOffsetX 165 -FeatureOffsetY 120 -LayoutPreset WideRails -TileWidth 420 -TileHeight 132 -TileGap 24 -LeftTileWidth 430 -RightTileWidth 460 -LeftTileOffsetX 8 -LeftTileOffsetY 10 -RightTileOffsetX 12 -RightTileOffsetY 14 -TileTextFitPolicy WrapThenShrink -Tile $tile -Feature $feature

        $visual.GetType().FullName | Should -Be 'PowerBGInfo.BgInfoVisualCanvas'
        $visual.Title | Should -Be 'PowerBGInfo'
        $visual.LayoutPreset.ToString() | Should -Be 'WideRails'
        $visual.TitleColor | Should -Not -BeNullOrEmpty
        $visual.TileValueColor | Should -Not -BeNullOrEmpty
        $visual.HeroBadgeTextColor | Should -Not -BeNullOrEmpty
        $visual.HeroBadgeVisible | Should -BeTrue
        $visual.HeroBadgeText | Should -Be 'EV'
        $visual.HeroBadgeImagePath | Should -Be (Resolve-Path -LiteralPath $path).Path
        $visual.HeroBadgeImageFit.ToString() | Should -Be 'Cover'
        $visual.HeroBadgeImagePadding | Should -Be 14
        $visual.HeroBadgeImageOpacity | Should -Be 0.82
        $visual.FeatureAnchor.ToString() | Should -Be 'BottomRight'
        $visual.FeatureWidth | Should -Be 610
        $visual.FeatureOffsetX | Should -Be 165
        $visual.FeatureOffsetY | Should -Be 120
        $visual.TileWidth | Should -Be 420
        $visual.TileHeight | Should -Be 132
        $visual.TileGap | Should -Be 24
        $visual.LeftTileWidth | Should -Be 430
        $visual.RightTileWidth | Should -Be 460
        $visual.LeftTileOffsetX | Should -Be 8
        $visual.LeftTileOffsetY | Should -Be 10
        $visual.RightTileOffsetX | Should -Be 12
        $visual.RightTileOffsetY | Should -Be 14
        $visual.TileTextFitPolicy.ToString() | Should -Be 'WrapThenShrink'
        $visual.Tiles.Count | Should -Be 1
        $visual.Tiles[0].Value | Should -Be '{{HostName}}'
        $visual.Tiles[0].Width | Should -Be 460
        $visual.Tiles[0].Height | Should -Be 144
        $visual.Tiles[0].SurfaceStyle.ToString() | Should -Be 'Raised'
        $visual.Tiles[0].IconKind.ToString() | Should -Be 'Computer'
        $visual.Tiles[0].MiniChartKind.ToString() | Should -Be 'Sparkline'
        $visual.Tiles[0].TextFitPolicy.ToString() | Should -Be 'SingleLineEllipsis'
        $visual.Tiles[0].MiniChartValues.Count | Should -Be 3
        $visual.Tiles[0].MiniChartMaximum | Should -Be 100
        $visual.Features.Count | Should -Be 1
    }

    It 'keeps template feature placement when only the strip size changes' {
        $visual = New-BGInfoVisualCanvas -Title PowerBGInfo -FeatureWidth 610 -FeatureHeight 52

        $visual.FeatureAnchor | Should -BeNullOrEmpty
        $visual.FeatureWidth | Should -Be 610
        $visual.FeatureHeight | Should -Be 52
    }

    It 'can disable the hero badge' {
        $visual = New-BGInfoVisualCanvas -Title PowerBGInfo -NoHeroBadge

        $visual.HeroBadgeVisible | Should -BeFalse
    }
}

Describe 'New-BGInfoImage cmdlet' {
    It 'creates an image overlay model' {
        $path = Join-Path -Path $TestDrive -ChildPath 'logo.png'
        Set-Content -LiteralPath $path -Value 'not-a-real-rendered-image'

        $image = New-BGInfoImage -Path $path -Width 180 -Anchor BottomRight -OffsetX 72 -OffsetY 54 -Opacity 0.85 -Fit Contain

        $image.GetType().FullName | Should -Be 'PowerBGInfo.BgInfoImage'
        $image.Path | Should -Be (Resolve-Path -LiteralPath $path).Path
        $image.Width | Should -Be 180
        $image.Anchor.ToString() | Should -Be 'BottomRight'
        $image.OffsetX | Should -Be 72
        $image.OffsetY | Should -Be 54
        $image.Opacity | Should -Be 0.85
        $image.Fit.ToString() | Should -Be 'Contain'
    }

    It 'rejects non-finite opacity' {
        $path = Join-Path -Path $TestDrive -ChildPath 'logo.png'
        Set-Content -LiteralPath $path -Value 'not-a-real-rendered-image'

        { New-BGInfoImage -Path $path -Opacity ([double]::NaN) } | Should -Throw
        { New-BGInfoImage -Path $path -Opacity ([double]::PositiveInfinity) } | Should -Throw
    }

    It 'honors single-axis absolute positions' {
        $path = Join-Path -Path $TestDrive -ChildPath 'logo.png'
        Set-Content -LiteralPath $path -Value 'not-a-real-rendered-image'

        $xOnly = New-BGInfoImage -Path $path -PositionX 123
        $yOnly = New-BGInfoImage -Path $path -PositionY 456

        $xOnly.PositionX | Should -Be 123
        $xOnly.PositionY | Should -BeNullOrEmpty
        $yOnly.PositionX | Should -BeNullOrEmpty
        $yOnly.PositionY | Should -Be 456
    }
}

Describe 'Export-BGInfoConfiguration cmdlet' {
    It 'writes json configuration file' {
        $config = New-BGInfoConfiguration -Target File
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
        $config1 = New-BGInfoConfiguration -Target File
        $config2 = New-BGInfoConfiguration -Target File
        $path = Join-Path -Path $TestDrive -ChildPath 'pipeline.json'

        $result = @((@($config1, $config2) | Export-BGInfoConfiguration -Path $path -Force -PassThru))

        $result.Count | Should -Be 2
        $result | Should -Be @($path, $path)
    }
}

Describe 'New-BGInfoConfiguration cmdlet' {
    It 'creates configuration with overrides' {
        $config = New-BGInfoConfiguration -Target File -MonitorIndex 1 -SpaceX 5 -SpaceY 7 -ValueWrapWidth 240 -ChartLayout Stack -ChartStackAlignToTextBlock -ChartStackOutsideTextBlock -DisableWallpaperSlideshow
        $config.Target.ToString() | Should -Be 'File'
        $config.MonitorIndex | Should -Be 1
        $config.SpaceX | Should -Be 5
        $config.SpaceY | Should -Be 7
        $config.ValueWrapWidth | Should -Be 240
        $config.ChartLayout.ToString() | Should -Be 'Stack'
        $config.ChartStackAlignToTextBlock | Should -BeTrue
        $config.ChartStackOutsideTextBlock | Should -BeTrue
        $config.PreserveWallpaperSlideshow | Should -BeFalse
    }

    It 'accepts visual canvas overlays directly' {
        $visual = New-BGInfoVisualCanvas -Title PowerBGInfo

        $config = New-BGInfoConfiguration -Target File -VisualCanvas $visual

        $config.VisualCanvases.Count | Should -Be 1
        $config.VisualCanvases[0].Title | Should -Be 'PowerBGInfo'
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

Describe 'New-BGInfo json export' {
    It 'exports json from inline authoring without rendering' {
        $path = Join-Path -Path $TestDrive -ChildPath 'inline.json'

        $result = New-BGInfo {
            New-BGInfoValue -BuiltinValue HostName
            New-BGInfoChart -Id 'cpu' -Title 'CPU' -Metric CpuPercent -ValueSuffix '%' -Kind Sparkline
        } -ConfigurationDirectory $TestDrive -Target Both -JsonPath $path -ExportOnly

        $result | Should -Be $path
        Test-Path -LiteralPath $path | Should -BeTrue
        $json = Get-Content -LiteralPath $path -Raw
        $json | Should -Match '"BuiltinValue"\s*:\s*"HostName"'
        $json | Should -Match '"Metric"\s*:\s*"CpuPercent"'
        $json | Should -Match '"Target"\s*:\s*"Both"'
    }

    It 'exports visual canvases from inline authoring' {
        $path = Join-Path -Path $TestDrive -ChildPath 'visual-canvas.json'
        $tile = New-BGInfoVisualCanvasTile -Side Left -Icon PC -Label HOSTNAME -Value '{{HostName}}'

        New-BGInfo {
            New-BGInfoVisualCanvas -Title PowerBGInfo -Subtitle 'Desktop insights' -Tile $tile
        } -ConfigurationDirectory $TestDrive -Target File -JsonPath $path -ExportOnly | Out-Null

        $json = Get-Content -LiteralPath $path -Raw
        $json | Should -Match '"VisualCanvases"'
        $json | Should -Match '"Value"\s*:\s*"\{\{HostName\}\}"'
    }
}

Describe 'New-BGInfo passthru' {
    It 'returns configuration objects for script-backed runners' {
        $config = New-BGInfo {
            New-BGInfoValue -BuiltinValue HostName
        } -ConfigurationDirectory $TestDrive -Target File -PassThru

        $config.GetType().FullName | Should -Be 'PowerBGInfo.BgInfoConfiguration'
        $config.Target.ToString() | Should -Be 'File'
        $config.Entries.Count | Should -Be 1
        $config.Entries[0].BuiltinValue | Should -Be 'HostName'
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

        Get-ImagePixelHash -Path $psResult | Should -Be (Get-ImagePixelHash -Path $cliResult)
    }

    It 'renders from json exported by inline New-BGInfo authoring' {
        $sampleImage = Join-Path -Path $PSScriptRoot -ChildPath '..\..\Examples\Samples\TapC-Evotec-2560x1080.jpg'
        $sampleImage = (Resolve-Path -Path $sampleImage).Path
        $outputDir = Join-Path -Path $TestDrive -ChildPath 'inline-cli'
        $configPath = Join-Path -Path $TestDrive -ChildPath 'inline-cli.json'
        $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe'
        if (-not (Test-Path -LiteralPath $cliPath)) {
            $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Release\net8.0-windows\PowerBGInfo.Cli.exe'
        }

        New-BGInfo {
            New-BGInfoValue -BuiltinValue HostName
        } -FilePath $sampleImage -ConfigurationDirectory $outputDir -Target File -OutputFileName 'inline.png' -JsonPath $configPath -ExportOnly | Out-Null

        & $cliPath --config $configPath --no-apply | Out-Null
        Test-Path -LiteralPath (Join-Path -Path $outputDir -ChildPath 'inline.png') | Should -BeTrue
    }

    It 'renders provider-backed foreach entries through the CLI' {
        $sampleImage = Join-Path -Path $PSScriptRoot -ChildPath '..\..\Examples\Samples\TapC-Evotec-2560x1080.jpg'
        $sampleImage = (Resolve-Path -Path $sampleImage).Path
        $outputDir = Join-Path -Path $TestDrive -ChildPath 'volume-cli'
        $configPath = Join-Path -Path $TestDrive -ChildPath 'volume-cli.json'
        $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe'
        if (-not (Test-Path -LiteralPath $cliPath)) {
            $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Release\net8.0-windows\PowerBGInfo.Cli.exe'
        }

        New-BGInfo {
            New-BGInfoVariable -Name Volumes -Provider Volumes
            New-BGInfoValue -ForEach Volumes -Name 'Drive {{DriveLetter}}' -Value '{{SizeRemaining}}'
        } -FilePath $sampleImage -ConfigurationDirectory $outputDir -Target File -OutputFileName 'volumes.png' -JsonPath $configPath -ExportOnly | Out-Null

        $json = Get-Content -LiteralPath $configPath -Raw
        $json | Should -Match '"Provider"\s*:\s*"Volumes"'
        $json | Should -Match '"ForEach"\s*:\s*"Volumes"'

        & $cliPath --config $configPath --no-apply | Out-Null
        Test-Path -LiteralPath (Join-Path -Path $outputDir -ChildPath 'volumes.png') | Should -BeTrue
    }

    It 'renders from a PowerShell script that returns configuration objects' {
        $sampleImage = Join-Path -Path $PSScriptRoot -ChildPath '..\..\Examples\Samples\TapC-Evotec-2560x1080.jpg'
        $sampleImage = (Resolve-Path -Path $sampleImage).Path
        $outputDir = Join-Path -Path $TestDrive -ChildPath 'script-cli'
        $scriptPath = Join-Path -Path $TestDrive -ChildPath 'bginfo-script.ps1'
        $moduleManifestPath = (Resolve-Path -Path (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1')).Path
        $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe'
        if (-not (Test-Path -LiteralPath $cliPath)) {
            $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Release\net8.0-windows\PowerBGInfo.Cli.exe'
        }

        @"
Import-Module '$($moduleManifestPath.Replace("'", "''"))' -Force
New-BGInfo {
    New-BGInfoValue -BuiltinValue HostName
} -FilePath '$($sampleImage.Replace("'", "''"))' -ConfigurationDirectory '$($outputDir.Replace("'", "''"))' -Target File -OutputFileName 'script-cli.png' -PassThru
"@ | Set-Content -LiteralPath $scriptPath -Encoding UTF8

        & $cliPath --script $scriptPath --no-apply | Out-Null
        Test-Path -LiteralPath (Join-Path -Path $outputDir -ChildPath 'script-cli.png') | Should -BeTrue
    }

    It 'exports json from a PowerShell script without rendering' {
        $scriptPath = Join-Path -Path $TestDrive -ChildPath 'bginfo-export-script.ps1'
        $moduleManifestPath = (Resolve-Path -Path (Join-Path -Path $PSScriptRoot -ChildPath '..\..\PowerBGInfo.psd1')).Path
        $exportPath = Join-Path -Path $TestDrive -ChildPath 'script-export.json'
        $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe'
        if (-not (Test-Path -LiteralPath $cliPath)) {
            $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Release\net8.0-windows\PowerBGInfo.Cli.exe'
        }

        @"
Import-Module '$($moduleManifestPath.Replace("'", "''"))' -Force
New-BGInfo {
    New-BGInfoValue -BuiltinValue HostName
} -ConfigurationDirectory '$($TestDrive.Replace("'", "''"))' -Target File -PassThru
"@ | Set-Content -LiteralPath $scriptPath -Encoding UTF8

        $result = & $cliPath --script $scriptPath --export-json $exportPath --export-only

        $result | Should -Be $exportPath
        Test-Path -LiteralPath $exportPath | Should -BeTrue
        (Get-Content -LiteralPath $exportPath -Raw) | Should -Match '"BuiltinValue"\s*:\s*"HostName"'
    }

    It 'renders from a PowerShell script using --module without importing the manifest' {
        $sampleImage = Join-Path -Path $PSScriptRoot -ChildPath '..\..\Examples\Samples\TapC-Evotec-2560x1080.jpg'
        $sampleImage = (Resolve-Path -Path $sampleImage).Path
        $outputDir = Join-Path -Path $TestDrive -ChildPath 'script-cli-module'
        $scriptPath = Join-Path -Path $TestDrive -ChildPath 'bginfo-script-module.ps1'
        $modulePath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.PowerShell\bin\Debug\net8.0-windows\PowerBGInfo.PowerShell.dll'
        if (-not (Test-Path -LiteralPath $modulePath)) {
            $modulePath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.PowerShell\bin\Release\net8.0-windows\PowerBGInfo.PowerShell.dll'
        }
        $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Debug\net8.0-windows\PowerBGInfo.Cli.exe'
        if (-not (Test-Path -LiteralPath $cliPath)) {
            $cliPath = Join-Path -Path $PSScriptRoot -ChildPath '..\PowerBGInfo.Cli\bin\Release\net8.0-windows\PowerBGInfo.Cli.exe'
        }

        @"
New-BGInfo {
    New-BGInfoValue -BuiltinValue HostName
} -FilePath '$($sampleImage.Replace("'", "''"))' -ConfigurationDirectory '$($outputDir.Replace("'", "''"))' -Target File -OutputFileName 'script-cli-module.png' -PassThru
"@ | Set-Content -LiteralPath $scriptPath -Encoding UTF8

        & $cliPath --script $scriptPath --module $modulePath --no-apply | Out-Null
        Test-Path -LiteralPath (Join-Path -Path $outputDir -ChildPath 'script-cli-module.png') | Should -BeTrue
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
