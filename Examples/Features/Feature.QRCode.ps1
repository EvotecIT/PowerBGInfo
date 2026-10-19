<#
    Feature: QR code. A phone-scannable link to the help-desk portal that
    already carries the asset tag, so the ticket starts with the right machine.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Light',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot '..\Output\Features')
)

Import-Module (Join-Path $PSScriptRoot '..\..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
# Live source for the asset tag: (Get-CimInstance Win32_SystemEnclosure).SMBIOSAssetTag
function Get-HelpDeskData {
    [pscustomobject] @{
        AssetTag = 'CT-04217'
        Portal   = 'help.contoso.com'
        Phone    = '555-0100, ext. 4357'
    }
}

$data = Invoke-BGInfoDataSource -Name 'Help-desk data' -TimeoutSeconds 5 -ScriptBlock ${function:Get-HelpDeskData} `
    -Default ([pscustomobject] @{ AssetTag = 'unknown'; Portal = 'help.contoso.com'; Phone = '555-0100' })
#endregion

#region Presentation: reads only $data
$scale = [Math]::Round($Height / 1080, 2)
$link = "https://$($data.Portal)/new?asset=$([uri]::EscapeDataString($data.AssetTag))"
New-BGInfo -Theme (New-BGInfoTheme Orchid -Mode $Theme -Scale $scale) -TextPosition MiddleLeft -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLabel -Name 'IT SERVICE DESK' -Accent -FontWeight 700
    New-BGInfoLabel -Name 'Need help?' -FontSize ([int](80 * $scale)) -FontWeight 600
    New-BGInfoLabel -Name 'Scan the code with your phone to open a ticket.' -FontSize ([int](24 * $scale))
    New-BGInfoValue -Name 'Phone' -Value $data.Phone
    New-BGInfoValue -Name 'Asset tag' -Value $data.AssetTag

    New-BGInfoQRCode -Region Right -Url $link -Title 'Scan for help' -Caption $data.Portal `
        -Width ([int](300 * $scale)) -Height ([int](360 * $scale))
} -ConfigurationDirectory $OutputDirectory -OutputFileName "qrcode-$($Theme.ToLower())-$Width.png"
#endregion
