@{
    AliasesToExport        = @()
    Author                 = 'Przemyslaw Klys'
    CmdletsToExport        = @()
    CompanyName            = 'Evotec'
    CompatiblePSEditions   = @('Desktop', 'Core')
    Copyright              = '(c) 2011 - 2022 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description            = 'PowerBGInfo is a module that allows you to create background images with information about your environment.'
    DotNetFrameworkVersion = '4.7.2'
    FunctionsToExport      = @('New-BGInfo', 'New-BGInfoLabel', 'New-BGInfoValue')
    GUID                   = '91b9c52d-6a39-4a65-a276-409b9390ee04'
    ModuleVersion          = '0.0.3'
    PowerShellVersion      = '5.1'
    PrivateData            = @{
        PSData = @{
            Tags       = @('windows', 'image', 'monitor', 'bginfo')
            LicenseUri = 'https://github.com/EvotecIT/PowerBGInfo/blob/master/License'
            ProjectUri = 'https://github.com/EvotecIT/PowerBGInfo'
            IconUri    = 'https://evotec.xyz/wp-content/uploads/2022/12/PowerBGInfo.png'
        }
    }
    RequiredModules        = @(@{
            ModuleVersion = '0.0.3'
            ModuleName    = 'DesktopManager'
            Guid          = '56f85fa6-c622-4204-8e97-3d99e3e06e75'
        }, @{
            ModuleVersion = '0.0.4'
            ModuleName    = 'ImagePlayground'
            Guid          = 'ff5469f2-c542-4318-909e-fd054d16821f'
        }, @{
            ModuleVersion = '0.0.254'
            ModuleName    = 'PSSharedGoods'
            Guid          = 'ee272aa8-baaa-4edf-9f45-b6d6f7d844fe'
        })
    RootModule             = 'PowerBGInfo.psm1'
}