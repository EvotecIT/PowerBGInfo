function Get-MyWallpaperData {
    param([string] $ComputerName)
    $uri = "https://cmdb.contoso.com/api/devices/$ComputerName"
    $device = Invoke-RestMethod -Uri $uri -UseDefaultCredentials -TimeoutSec 5

    [pscustomobject] @{
        Owner = $device.owner
        Team = $device.costCenter
        Tickets = $device.openTickets
    }
}

# The function runs in its own runspace: pass in what it needs.
$source = @{
    Name = 'CMDB'
    TimeoutSeconds = 10
    ScriptBlock = ${function:Get-MyWallpaperData}
    ArgumentList = $env:COMPUTERNAME
    Default = $fallback
}
$data = Invoke-BGInfoDataSource @source
