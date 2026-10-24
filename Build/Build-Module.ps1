<#
.SYNOPSIS
Builds or publishes PowerBGInfo through the shared module pipeline.
.PARAMETER ConfigurationGateMode
Selects a local build or publication.
Build is the default and does not publish configured destinations.
.PARAMETER SignModule
Controls module signing. Use false for unsigned local validation.
.PARAMETER SkipInstall
Leaves installed module copies unchanged.
.PARAMETER PowerShellGalleryApiKeyPath
Path to the protected Gallery API-key file, required when publishing.
.PARAMETER GitHubApiKeyPath
Path to the protected GitHub token file, required when publishing.
.EXAMPLE
./Build/Build-Module.ps1 -ConfigurationGateMode Build -SignModule:$false -SkipInstall
.EXAMPLE
./Build/Build-Module.ps1 -ConfigurationGateMode Publish -SignModule:$true -SkipInstall -PowerShellGalleryApiKeyPath '<protected Gallery key file>' -GitHubApiKeyPath '<protected GitHub token file>'
#>
param(
    [ValidateSet('Build', 'Publish')]
    [string] $ConfigurationGateMode = 'Build',
    [bool] $SignModule = $(if ([string]::IsNullOrWhiteSpace($Env:SignModule)) { $true } else { [bool]::Parse($Env:SignModule) }),
    [switch] $SkipInstall,
    [string] $PowerShellGalleryApiKeyPath,
    [string] $GitHubApiKeyPath
)

Import-Module PSPublishModule -MinimumVersion 3.0.170 -Force -ErrorAction Stop

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
        ModuleVersion          = '2.0.X'
        # Author of this module
        Author                 = 'Przemyslaw Klys'
        # Company or vendor of this module
        CompanyName            = 'Evotec'
        # Copyright statement for this module
        Copyright              = "(c) 2011 - $((Get-Date).Year) Przemyslaw Klys @ Evotec. All rights reserved."
        # Description of the functionality provided by this module
        Description            = 'PowerBGInfo is a module that allows you to create background images with information about your environment.'
        # Tags applied to this module. These help with module discovery in online galleries.
        Tags                   = @('windows', 'macos', 'linux', 'image', 'monitor', 'bginfo', 'charts', 'topology', 'diagrams')
        # Website cutover metadata: publish only after powerbginfo.com is live.
        ProjectUri             = 'https://powerbginfo.com'
        IconUri                = 'https://powerbginfo.com/apple-touch-icon.png'
        LicenseUri             = 'https://github.com/EvotecIT/PowerBGInfo/blob/master/License'
        DotNetFrameworkVersion = '4.7.2'
    }
    New-ConfigurationManifest @Manifest

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
    New-ConfigurationDocumentation -Enable -PathReadme 'Docs/Readme.md' -Path 'Docs' -SyncExternalHelpToProjectRoot

    $newConfigurationBuildSplat = @{
        Enable                            = $true
        SignModule                        = $SignModule
        MergeModuleOnBuild                = $true
        MergeFunctionsFromApprovedModules = $true
        ResolveBinaryConflicts            = $true
        ResolveBinaryConflictsName        = 'PowerBGInfo.PowerShell'
        NETProjectName                    = 'PowerBGInfo.PowerShell'
        NETProjectPath                    = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../Sources/PowerBGInfo.PowerShell/PowerBGInfo.PowerShell.csproj')).Path
        NETConfiguration                  = 'Release'
        NETFramework                      = 'net8.0', 'net472'
        NETHandleAssemblyWithSameName     = $true
        NETAssemblyLoadContext            = $true
        NETAssemblyTypeAcceleratorMode    = 'Enums'
        NETAssemblyTypeAcceleratorAssemblies = @('PowerBGInfo', 'DesktopManager', 'ChartForgeX', 'ChartForgeX.Visuals')
        NETAssemblyTypeAccelerators       = @(
            'PowerBGInfo.BgInfoConfiguration'
            'PowerBGInfo.BgInfoChart'
            'PowerBGInfo.BgInfoEntry'
            'PowerBGInfo.BgInfoImage'
            'PowerBGInfo.BgInfoTopology'
            'PowerBGInfo.BgInfoVariable'
            'PowerBGInfo.BgInfoVisualCanvas'
            'PowerBGInfo.BgInfoVisualCanvasTile'
            'PowerBGInfo.BgInfoVisualCanvasFeature'
            'ChartForgeX.Primitives.ChartColor'
            'ChartForgeX.Themes.VisualDesignTokens'
            'ChartForgeX.Themes.VisualTheme'
            'ChartForgeX.Topology.TopologyNode'
            'ChartForgeX.Topology.TopologyEdge'
            'ChartForgeX.Topology.TopologyGroup'
        )
        #NETMergeLibraryDebugging          = $true
        DotSourceLibraries                = $true
        DotSourceClasses                  = $true
        DeleteTargetModuleBeforeBuild     = $true
        NETBinaryModuleDocumentation      = $true
        CertificateThumbprint             = '92e95fb58effa6a4a75e77a33cdd6bfe6dd30f1a'
    }

    New-ConfigurationBuild @newConfigurationBuildSplat

    New-ConfigurationArtefact -Type Unpacked -Enable -Path "$PSScriptRoot/../Artefacts/Unpacked" -ModulesPath "$PSScriptRoot/../Artefacts/Unpacked/Modules" -RequiredModulesPath "$PSScriptRoot/../Artefacts/Unpacked/Modules" -AddRequiredModules
    New-ConfigurationArtefact -Type Packed -Enable -Path "$PSScriptRoot/../Artefacts/Packed" -ArtefactName '<ModuleName>.v<ModuleVersion>.zip'

    New-ConfigurationPublish -Type PowerShellGallery -FilePath $PowerShellGalleryApiKeyPath -Enabled:$false
    New-ConfigurationPublish -Type GitHub -FilePath $GitHubApiKeyPath -UserName 'EvotecIT' -RepositoryName 'PowerBGInfo' -Enabled:$false -GenerateReleaseNotes
    New-ConfigurationGate -Mode $ConfigurationGateMode
} -SkipInstall:$SkipInstall
