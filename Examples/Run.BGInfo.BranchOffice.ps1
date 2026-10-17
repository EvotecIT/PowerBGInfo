<#
    Branch office PC. The question for the person at this desk: where am I
    connected, is the link to head office up, and how do I get help?

    The data block returns plain values; the presentation block only reads
    them. Comments name the command or source that supplies each value.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Light',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output')
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
function Get-BranchOfficeData {
    [pscustomobject] @{
        # (Get-ADComputer $env:COMPUTERNAME -Properties Location).Location,
        # or a subnet-to-site table: (Get-NetIPAddress).IPAddress
        Site     = 'Warsaw · Mokotów'
        SiteCode = 'WAW-02'
        Computer = 'WAW02-PC-014'
        Printer  = 'WAW02-PRN-01, 2nd floor'
        Contact  = 'Office manager, ext. 2210'
        Wifi     = 'CONTOSO-STAFF'
        # Test-NetConnection to each device, or your SD-WAN controller's
        # API. Everything here hangs off the office switch.
        Switch   = @{ Label = 'Switch'; Detail = '1 GbE' }
        Links    = @(
            [pscustomobject] @{ Id = 'pc'; Label = 'This PC'
                Detail = '10.62.14.37'; Kind = 'Endpoint'; Status = 'Healthy' }
            [pscustomobject] @{ Id = 'prn'; Label = 'Printer'
                Detail = '2nd floor'; Kind = 'Endpoint'; Status = 'Healthy' }
            [pscustomobject] @{ Id = 'edge'; Label = 'SD-WAN'
                Detail = 'MPLS · 18 ms'; Kind = 'Gateway'; Status = 'Healthy' }
            [pscustomobject] @{ Id = 'lte'; Label = 'LTE'
                Detail = 'standby'; Kind = 'Gateway'
                Status = 'Disabled' }
        )
        Backup   = 'LTE backup ready'
        # The branch calendar, or opening hours from your intranet
        Hours    = @(
            [pscustomobject] @{
                Day = 'Mon–Fri'; Office = '08:00–18:00'; IT = '07:00–19:00' }
            [pscustomobject] @{
                Day = 'Saturday'; Office = '09:00–13:00'; IT = 'on call' }
            [pscustomobject] @{
                Day = 'Sunday'; Office = 'closed'; IT = 'on call' }
        )
        HelpUrl  = 'https://help.contoso.com/new?site=WAW-02'
    }
}

$fallback = [pscustomobject] @{
    Site = 'Contoso'; SiteCode = 'unknown'; Computer = $env:COMPUTERNAME
    Printer = 'unknown'; Contact = 'Service desk, ext. 4357'
    Wifi = 'unknown'
    Switch = @{ Label = 'Switch'; Detail = '' }; Links = @(); Backup = ''
    Hours = @()
    HelpUrl = 'https://help.contoso.com/new'
}
$source = @{
    Name           = 'Branch office data'
    TimeoutSeconds = 15
    ScriptBlock    = ${function:Get-BranchOfficeData}
    Default        = $fallback
}
$branch = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $branch
$scale = [Math]::Round($Height / 1080, 2)
$dark = $Theme -eq 'Dark'
# Aurora with a green accent and dots: calm, and clearly not a server.
$accent = if ($dark) { '#4ADE80' } else { '#15803D' }
$dots = if ($dark) { '#FFFFFF10' } else { '#14532D14' }
$look = New-BGInfoTheme Aurora -Mode $Theme -Scale $scale -Accent $accent
$down = @($branch.Links | Where-Object Status -EQ 'Critical')
# The diagram is small for its panel; a higher scale draws it larger.
$diagramScale = 2

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height -Pattern Dots `
        -PatternColor $dots -PatternSpacing ([int](28 * $scale))
    # The connection diagram fills the space under the text.
    New-BGInfoLayout -MaximumPanelHeight ([int](640 * $scale))

    New-BGInfoLabel -Name "CONTOSO BRANCH  ·  $($branch.SiteCode)" -Accent `
        -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $branch.Site -FontSize ([int](72 * $scale)) `
        -FontWeight 600
    if ($branch.Links.Count -eq 0) {
        New-BGInfoLabel -Name '●  Connectivity unavailable' -Status Info `
            -FontWeight 600
    } elseif ($down.Count -eq 0) {
        $text = "Connected to head office · $($branch.Backup)"
        New-BGInfoLabel -Name "●  $text" -Status Ok -FontWeight 600
    } else {
        $text = "$($down[0].Label) is not reachable, call ext. 4357"
        New-BGInfoLabel -Name "●  $text" -Status Critical -FontWeight 600
    }
    New-BGInfoValue -Name 'This PC' -Value $branch.Computer
    New-BGInfoValue -Name 'Printer' -Value $branch.Printer
    New-BGInfoValue -Name 'Wi-Fi' -Value $branch.Wifi
    New-BGInfoValue -Name 'Local help' -Value $branch.Contact

    # Right column: a ticket from the phone, and when someone is here.
    New-BGInfoQRCode -Region Right -Url $branch.HelpUrl `
        -Title 'Scan for IT help' -Caption 'help.contoso.com' `
        -Note 'opens a ticket' -Height ([int](330 * $scale))
    if ($branch.Hours.Count -gt 0) {
        $branch.Hours | New-BGInfoTable -Region Right -Title 'Opening hours' `
            -Property Day, Office, @{ Name = 'IT desk'
                Expression = { $_.IT } }
    }

    # Center: everything this desk depends on, around the office switch.
    if ($branch.Links.Count -gt 0) {
        New-BGInfoTopology -Region Center -Height 0 `
            -Title 'Your connection' -Subtitle 'checked 2 min ago' `
            -Layout HubAndSpoke -NodeDisplayMode Card -Scale $diagramScale `
            -TopologyDefinition {
            New-BGInfoTopologyNode -Id 'sw' -Label $branch.Switch.Label `
                -Subtitle $branch.Switch.Detail -Kind Network -Status Healthy
            foreach ($device in $branch.Links) {
                New-BGInfoTopologyNode -Id $device.Id -Label $device.Label `
                    -Subtitle $device.Detail -Kind $device.Kind `
                    -Status $device.Status
            }
            foreach ($device in $branch.Links) {
                New-BGInfoTopologyEdge -SourceNodeId 'sw' -TargetNodeId $device.Id `
                    -Kind Connectivity -Status $device.Status
            }
        }
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "branch-office-$($Theme.ToLower()).png"
#endregion
