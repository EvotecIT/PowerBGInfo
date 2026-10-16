<#
    Feature: built-in data helpers. Get-BGInfoSystemInfo returns the facts
    support asks for first (uptime, OS build, IP, disks, last update,
    Defender and BitLocker state, signed-in user) as one plain object. Every
    probe is read-only and falls back to 'Unknown' without elevation.

    Run with -Live to read this machine. Without it, a sample object of the
    same shape keeps the render reproducible, which also shows how to swap in
    your own function: return the same property names and the presentation
    code does not change.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot '..\Output\Features'),
    [switch] $Live
)

Import-Module (Join-Path $PSScriptRoot '..\..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
function Get-SampleSystemInfo {
    # Same property names as Get-BGInfoSystemInfo.
    [pscustomobject] @{
        ComputerName         = 'WS-ANALYST-11'
        LoggedOnUser         = 'CONTOSO\anowak'
        Domain               = 'corp.contoso.com'
        OSName               = 'Windows 11 Enterprise'
        OSBuild              = '26100'
        UptimeText           = '3 days 4 h'
        IPv4Address          = '10.20.4.37'
        LastPatchId          = 'KB5044284'
        LastPatchDate        = [datetime] '2026-10-14'
        DefenderStatus       = 'Protected'
        DefenderSignatureAge = [timespan]::FromHours(2)
        BitLockerStatus      = 'On'
        Disks                = @(
            [pscustomobject] @{ Drive = 'C:'; Label = 'System'; SizeGB = 476.3; FreeGB = 212.4; UsedPercent = 55.4 }
            [pscustomobject] @{ Drive = 'D:'; Label = 'Data'; SizeGB = 931.5; FreeGB = 98.2; UsedPercent = 89.5 }
        )
    }
}

# Your own function plugs in here; the time box keeps a slow probe from blocking logon.
$source = if ($Live) { { Get-BGInfoSystemInfo } } else { ${function:Get-SampleSystemInfo} }
$facts = Invoke-BGInfoDataSource -Name 'System facts' -TimeoutSeconds 20 -ScriptBlock $source -Default (Get-SampleSystemInfo)
# Values that a helper could not read come back as 'Unknown'; show them as such.
$patch = if ($facts.LastPatchDate) { "$($facts.LastPatchId) · $($facts.LastPatchDate.ToString('d MMM yyyy'))" } else { $facts.LastPatchId }
#endregion

#region Presentation: reads only $facts
$scale = [Math]::Round($Height / 1080, 2)
New-BGInfo -Theme (New-BGInfoTheme Corporate -Mode $Theme -Scale $scale) -Target File {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLabel -Name 'SYSTEM FACTS' -Accent -FontWeight 700
    New-BGInfoLabel -Name $facts.ComputerName -FontSize ([int](56 * $scale)) -FontWeight 600
    New-BGInfoValue -Name 'Signed in' -Value $facts.LoggedOnUser
    New-BGInfoValue -Name 'Windows' -Value "$($facts.OSName) · build $($facts.OSBuild)"
    New-BGInfoValue -Name 'Up for' -Value $facts.UptimeText
    New-BGInfoValue -Name 'IP address' -Value $facts.IPv4Address
    New-BGInfoValue -Name 'Last update' -Value $patch

    $protection = @(
        @{ Text = 'Microsoft Defender'; Value = $facts.DefenderStatus; Status = if ($facts.DefenderStatus -eq 'Protected') { 'Ok' } elseif ($facts.DefenderStatus -eq 'Unknown') { 'Info' } else { 'Critical' } }
        @{ Text = 'BitLocker (system drive)'; Value = $facts.BitLockerStatus; Status = switch ($facts.BitLockerStatus) { 'On' { 'Ok' } 'Off' { 'Critical' } default { 'Info' } } }
    )
    New-BGInfoList -Region Right -Title 'Protection' -Item $protection
    $facts.Disks | New-BGInfoTable -Region Right -Title 'Disks' -Property @(
        'Drive', 'Label'
        @{ Name = 'Free'; Expression = { '{0:N0} GB' -f $_.FreeGB } }
        @{ Name = 'Used'; Expression = { '{0:N0} %' -f $_.UsedPercent } }
    )
} -ConfigurationDirectory $OutputDirectory -OutputFileName "system-info-$($Theme.ToLower())-$Width.png"
#endregion
