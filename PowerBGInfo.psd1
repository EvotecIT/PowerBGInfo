@{
    AliasesToExport        = @()
    Author                 = 'Przemyslaw Klys'
    CmdletsToExport        = @('Export-BGInfoConfiguration', 'Get-BGInfoMonitor', 'Get-BGInfoSystemInfo', 'Invoke-BGInfo', 'Invoke-BGInfoDataSource', 'New-BGInfo', 'New-BGInfoBackground', 'New-BGInfoBackgroundGlow', 'New-BGInfoCalendarHeatmap', 'New-BGInfoChart', 'New-BGInfoConfiguration', 'New-BGInfoImage', 'New-BGInfoLabel', 'New-BGInfoLayout', 'New-BGInfoList', 'New-BGInfoMermaid', 'New-BGInfoMetric', 'New-BGInfoQRCode', 'New-BGInfoTable', 'New-BGInfoTheme', 'New-BGInfoTimeline', 'New-BGInfoTimelineItem', 'New-BGInfoTopology', 'New-BGInfoTopologyEdge', 'New-BGInfoTopologyGroup', 'New-BGInfoTopologyNode', 'New-BGInfoTreemap', 'New-BGInfoValue', 'New-BGInfoVariable', 'New-BGInfoVisualCanvas', 'New-BGInfoVisualCanvasFeature', 'New-BGInfoVisualCanvasTile')
    CompanyName            = 'Evotec'
    CompatiblePSEditions   = @('Desktop', 'Core')
    Copyright              = '(c) 2011 - 2026 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description            = 'PowerBGInfo is a module that allows you to create background images with information about your environment.'
    DotNetFrameworkVersion = '4.7.2'
    FunctionsToExport      = @()
    GUID                   = '91b9c52d-6a39-4a65-a276-409b9390ee04'
    ModuleVersion          = '2.0.6'
    PowerShellVersion      = '5.1'
    PrivateData            = @{
        PSData = @{
            # Publish this cutover metadata only after powerbginfo.com is live.
            IconUri                    = 'https://powerbginfo.com/apple-touch-icon.png'
            LicenseUri                 = 'https://github.com/EvotecIT/PowerBGInfo/blob/master/License'
            ProjectUri                 = 'https://powerbginfo.com'
            Tags                       = @('windows', 'image', 'monitor', 'bginfo', 'charts', 'topology', 'diagrams')
            RequireLicenseAcceptance   = $false
            ExternalModuleDependencies = @()
        }
    }
    RootModule             = 'PowerBGInfo.psm1'
    RequiredModules        = @()
}
