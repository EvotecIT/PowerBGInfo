Import-Module PSPublishModule -Force -ErrorAction Stop

Build-Module -ModuleName 'PowerBGInfo' {
    # Usual defaults as per standard module
    $Manifest = [ordered] @{
        # Minimum version of the Windows PowerShell engine required by this module
        PowerShellVersion      = '5.1'
        # prevent using over CORE/PS 7
        CompatiblePSEditions   = @('Desktop', 'Core')
        # ID used to uniquely identify this module
        GUID                   = '91b9c52d-6a39-4a65-a276-409b9390ee04'
        # Version number of this module.
        ModuleVersion          = '1.0.X'
        # Author of this module
        Author                 = 'Przemyslaw Klys'
        # Company or vendor of this module
        CompanyName            = 'Evotec'
        # Copyright statement for this module
        Copyright              = "(c) 2011 - $((Get-Date).Year) Przemyslaw Klys @ Evotec. All rights reserved."
        # Description of the functionality provided by this module
        Description            = 'PowerBGInfo is a module that allows you to create background images with information about your environment.'
        # Tags applied to this module. These help with module discovery in online galleries.
        Tags                   = @('windows', 'image', 'monitor', 'bginfo', 'charts', 'topology', 'diagrams')
        # A URL to the main website for this project.
        ProjectUri             = 'https://github.com/EvotecIT/PowerBGInfo'
        IconUri                = 'https://evotec.xyz/wp-content/uploads/2022/12/PowerBGInfo.png'
        LicenseUri             = 'https://github.com/EvotecIT/PowerBGInfo/blob/master/License'
        DotNetFrameworkVersion = '4.7.2'
    }
    New-ConfigurationManifest @Manifest

    #New-ConfigurationModule -Type RequiredModule -Name 'PSSharedGoods' -Guid Auto -Version Latest
    #New-ConfigurationModule -Type RequiredModule -Name 'DesktopManager' -Guid Auto -Version '2.0.1'
    #New-ConfigurationModule -Type ApprovedModule -Name 'PSSharedGoods', 'PSWriteColor', 'Connectimo', 'PSUnifi', 'PSWebToolbox', 'PSMyPassword', 'PSPublishModule'
    New-ConfigurationModuleSkip -IgnoreModuleName @('NetTCPIP', 'Microsoft.PowerShell.Utility', 'Microsoft.PowerShell.Management', 'CimCmdlets')

    $ConfigurationFormat = [ordered] @{
        RemoveComments                              = $false
        PlaceOpenBraceEnable                        = $true
        PlaceOpenBraceOnSameLine                    = $true
        PlaceOpenBraceNewLineAfter                  = $true
        PlaceOpenBraceIgnoreOneLineBlock            = $true
        PlaceCloseBraceEnable                       = $true
        PlaceCloseBraceNewLineAfter                 = $false
        PlaceCloseBraceIgnoreOneLineBlock           = $true
        PlaceCloseBraceNoEmptyLineBefore            = $false
        UseConsistentIndentationEnable              = $true
        UseConsistentIndentationKind                = 'space'
        UseConsistentIndentationPipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
        UseConsistentIndentationIndentationSize     = 4
        UseConsistentWhitespaceEnable               = $true
        UseConsistentWhitespaceCheckInnerBrace      = $true
        UseConsistentWhitespaceCheckOpenBrace       = $true
        UseConsistentWhitespaceCheckOpenParen       = $true
        UseConsistentWhitespaceCheckOperator        = $true
        UseConsistentWhitespaceCheckPipe            = $true
        UseConsistentWhitespaceCheckSeparator       = $true
        AlignAssignmentStatementEnable              = $true
        AlignAssignmentStatementCheckHashtable      = $true
        UseCorrectCasingEnable                      = $true
    }

    # format PSD1 and PSM1 files when merging into a single file
    # enable formatting is not required as Configuration is provided
    New-ConfigurationFormat -ApplyTo 'OnMergePSM1', 'OnMergePSD1' -Sort None @ConfigurationFormat
    # format PSD1 and PSM1 files within the module
    # enable formatting is required to make sure that formatting is applied (with default settings)
    New-ConfigurationFormat -ApplyTo 'DefaultPSD1', 'DefaultPSM1' -EnableFormatting -Sort None
    # when creating PSD1 use special style without comments and with only required parameters
    New-ConfigurationFormat -ApplyTo 'DefaultPSD1', 'OnMergePSD1' -PSD1Style 'Minimal'
    # configuration for documentation, at the same time it enables documentation processing
    New-ConfigurationDocumentation -Enable:$false -StartClean -UpdateWhenNew -PathReadme 'Docs\Readme.md' -Path 'Docs'

    #New-ConfigurationImportModule -ImportSelf

    $signModule = $Env:COMPUTERNAME -eq 'EVOMAGIC'

    $newConfigurationBuildSplat = @{
        Enable                            = $true
        SignModule                        = $signModule
        MergeModuleOnBuild                = $true
        MergeFunctionsFromApprovedModules = $true
        ResolveBinaryConflicts            = $true
        ResolveBinaryConflictsName        = 'PowerBGInfo.PowerShell'
        NETProjectName                    = 'PowerBGInfo.PowerShell'
        NETProjectPath                    = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\Sources\PowerBGInfo.PowerShell\PowerBGInfo.PowerShell.csproj')).Path
        NETConfiguration                  = 'Release'
        NETFramework                      = 'net8.0-windows', 'net472'
        NETHandleAssemblyWithSameName     = $true
        NETAssemblyLoadContext            = $true
        NETAssemblyTypeAcceleratorMode    = 'AllowList'
        NETAssemblyTypeAccelerators       = @(
            'PowerBGInfo.BgInfoConfiguration'
            'PowerBGInfo.BgInfoConfigurationJson'
            'PowerBGInfo.BgInfoEntry'
            'PowerBGInfo.BgInfoEntryType'
            'PowerBGInfo.BgInfoVariable'
            'PowerBGInfo.BgInfoVariableProvider'
            'PowerBGInfo.BgInfoChart'
            'PowerBGInfo.BgInfoChartKind'
            'PowerBGInfo.BgInfoChartLegendPosition'
            'PowerBGInfo.BgInfoChartMetric'
            'PowerBGInfo.BgInfoChartPictorialSymbol'
            'PowerBGInfo.BgInfoChartLayoutMode'
            'PowerBGInfo.BgInfoChartStackDirection'
            'PowerBGInfo.BgInfoTarget'
            'PowerBGInfo.BgInfoTextPosition'
            'PowerBGInfo.BgInfoTopology'
            'PowerBGInfo.BgInfoVisualCanvas'
            'PowerBGInfo.BgInfoVisualCanvasTemplate'
            'PowerBGInfo.BgInfoVisualCanvasSide'
            'PowerBGInfo.BgInfoVisualCanvasTile'
            'PowerBGInfo.BgInfoVisualCanvasTileSurfaceStyle'
            'PowerBGInfo.BgInfoVisualCanvasTileIconKind'
            'PowerBGInfo.BgInfoVisualCanvasTileMiniChartKind'
            'PowerBGInfo.BgInfoVisualCanvasFeature'
        )
        #NETMergeLibraryDebugging          = $true
        DotSourceLibraries                = $true
        DotSourceClasses                  = $true
        DeleteTargetModuleBeforeBuild     = $true
        NETBinaryModuleDocumentation      = $true
        RefreshPSD1Only                   = if ([string]::IsNullOrWhiteSpace($Env:RefreshPSD1Only)) { $true } else { [bool]::Parse($Env:RefreshPSD1Only) }
    }

    if ($signModule) {
        $newConfigurationBuildSplat.CertificateThumbprint = '483292C9E317AA13B07BB7A96AE9D1A5ED9E7703'
    }

    New-ConfigurationBuild @newConfigurationBuildSplat

    New-ConfigurationArtefact -Type Unpacked -Enable -Path "$PSScriptRoot\..\Artefacts\Unpacked" -ModulesPath "$PSScriptRoot\..\Artefacts\Unpacked\Modules" -RequiredModulesPath "$PSScriptRoot\..\Artefacts\Unpacked\Modules" -AddRequiredModules
    New-ConfigurationArtefact -Type Packed -Enable -Path "$PSScriptRoot\..\Artefacts\Packed" -ArtefactName '<ModuleName>.v<ModuleVersion>.zip'

    # options for publishing to github/psgallery
    #New-ConfigurationPublish -Type PowerShellGallery -FilePath 'C:\Support\Important\PowerShellGalleryAPI.txt' -Enabled
    #New-ConfigurationPublish -Type GitHub -FilePath 'C:\Support\Important\GitHubAPI.txt' -UserName 'EvotecIT' -Enabled -GenerateReleaseNotes
}
