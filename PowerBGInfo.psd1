@{
    AliasesToExport        = @()
    Author                 = 'Przemyslaw Klys'
    CmdletsToExport        = @('Export-BGInfoConfiguration', 'Invoke-BGInfo', 'New-BGInfo', 'New-BGInfoChart', 'New-BGInfoConfiguration', 'New-BGInfoImage', 'New-BGInfoLabel', 'New-BGInfoTopology', 'New-BGInfoTopologyEdge', 'New-BGInfoTopologyGroup', 'New-BGInfoTopologyNode', 'New-BGInfoValue', 'New-BGInfoVariable', 'New-BGInfoVisualCanvas', 'New-BGInfoVisualCanvasFeature', 'New-BGInfoVisualCanvasTile')
    CompanyName            = 'Evotec'
    CompatiblePSEditions   = @('Desktop', 'Core')
    Copyright              = '(c) 2011 - 2026 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description            = 'PowerBGInfo is a module that allows you to create background images with information about your environment.'
    DotNetFrameworkVersion = '4.7.2'
    FunctionsToExport      = @()
    GUID                   = '91b9c52d-6a39-4a65-a276-409b9390ee04'
    ModuleVersion          = '2.0.2'
    PowerShellVersion      = '5.1'
    PrivateData            = @{
        PSData = @{
            IconUri                    = 'https://evotec.xyz/wp-content/uploads/2022/12/PowerBGInfo.png'
            LicenseUri                 = 'https://github.com/EvotecIT/PowerBGInfo/blob/master/License'
            ProjectUri                 = 'https://github.com/EvotecIT/PowerBGInfo'
            Tags                       = @('windows', 'image', 'monitor', 'bginfo', 'charts', 'topology', 'diagrams')
            RequireLicenseAcceptance   = $false
            ExternalModuleDependencies = @()
        }
    }
    RootModule             = 'PowerBGInfo.psm1'
    RequiredModules        = @()
    ScriptsToProcess       = @()
}
