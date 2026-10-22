<#
    Jump host. The question for the administrator who just connected: what
    are the rules on this machine? It says who is signed in and how they
    were elevated, that the session is recorded, which tools belong here,
    what never to do here, and who else is connected right now.

    The data block returns plain values; the presentation block only reads
    them. Comments name the command or source that supplies each value.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output')
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
function Get-JumpHostRules {
    [pscustomobject] @{
        Available = $true
        # $env:COMPUTERNAME and $env:USERDNSDOMAIN
        Name      = 'JMP-T0-01'
        Domain    = 'corp.contoso.com'
        Tier      = 'Tier 0'
        # whoami, or [Security.Principal.WindowsIdentity]::GetCurrent().Name
        User      = 'CONTOSO\adm-t0.avery'
        # whoami /groups lists the admin group and the
        # "Mandatory Label\High Mandatory Level" of an elevated token. The end
        # time comes from your PIM or PAM tool, for example
        # Get-MgRoleManagementDirectoryRoleAssignmentScheduleInstance.
        Elevation = 'Domain Admins until 11:40'
        # Get-MgAuditLogSignIn -Filter "userPrincipalName eq '...'" -Top 1,
        # its authenticationDetails
        Mfa       = 'FIDO2 key at 09:38'
        # Your session recording product, or the policy text it enforces
        Recording = 'Recorded and reviewed, kept 90 days'
        # Your access policy, published where the admins can scan it
        Policy    = 'https://intranet.contoso.com/policy/admin-access'
        # The tools installed for this tier, from your build definition, or
        # Get-Package / Get-WindowsCapability -Online |
        #     Where-Object State -EQ Installed
        Allowed   = @(
            @{ Text = 'Active Directory tools'; Value = 'RSAT'; Status = 'Ok' }
            @{ Text = 'PowerShell 7'; Value = 'signed scripts'; Status = 'Ok' }
            @{ Text = 'Group Policy Management'; Value = 'GPMC'; Status = 'Ok' }
            @{ Text = 'Remote Desktop'; Value = 'tier 0 only'; Status = 'Ok' }
        )
        # Your admin policy, in the words people remember
        Never     = @(
            @{ Text = 'No web browsing'; Value = 'proxy blocks it'
                Status = 'Critical' }
            @{ Text = 'No email or chat'; Value = 'not installed'
                Status = 'Critical' }
            @{ Text = 'No file copy to the internet'; Value = 'DLP enforced'
                Status = 'Critical' }
            @{ Text = 'No personal or shared accounts'; Value = 'named admins'
                Status = 'Critical' }
        )
        # query session (qwinsta), or Get-RDUserSession on a session host
        Sessions  = @(
            [pscustomobject] @{
                User = 'adm-t0.avery (you)'; From = 'PAW-T0-03'
                Since = '09:38'; State = 'Online'
            }
            [pscustomobject] @{
                User = 'adm-t0.morgan'; From = 'PAW-T0-07'
                Since = '08:12'; State = 'Online'
            }
            [pscustomobject] @{
                User = 'adm-t0.quinn'; From = 'PAW-T0-01'
                Since = 'yesterday 17:05'; State = 'Disconnected'
            }
        )
        # Get-LocalGroupMember -Group Administrators | Measure-Object
        LocalAdmins = '2 groups, no users'
        # Your baseline scan time; each line below from its own check:
        BaselineAt = 'today 06:00'
        Baseline  = @(
            # Get-MpComputerStatus: RealTimeProtectionEnabled,
            # AntivirusSignatureAge
            @{ Text = 'Defender real-time'; Value = 'on, 2 h old'
                Status = 'Ok' }
            # (Get-CimInstance -Namespace root\Microsoft\Windows\DeviceGuard
            #   -ClassName Win32_DeviceGuard).SecurityServicesRunning
            @{ Text = 'Credential Guard'; Value = 'running'; Status = 'Ok' }
            # Get-MgDeviceManagementManagedDevice: ComplianceState
            @{ Text = 'Intune compliance'; Value = 'compliant'; Status = 'Ok' }
            # Get-HotFix | Sort-Object InstalledOn | Select-Object -Last 1
            @{ Text = 'Security updates'; Value = '1 pending'
                Status = 'Warning' }
        )
    }
}

$fallback = [pscustomobject] @{
    Available = $false; Name = $env:COMPUTERNAME; Domain = 'unknown'; Tier = ''
    User = 'unknown'; Elevation = 'unknown'; Mfa = 'unknown'
    Recording = 'Assume this session is recorded'; Policy = ''
    Allowed = @(); Never = @(); Sessions = @(); LocalAdmins = 'unknown'
    BaselineAt = ''; Baseline = @()
}
$source = @{
    Name           = 'Jump host rules'
    TimeoutSeconds = 15
    ScriptBlock    = ${function:Get-JumpHostRules}
    Default        = $fallback
}
$jump = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $jump
$scale = [Math]::Round($Height / 1080, 2)
# Violet marks the privileged tier; a deeper shade keeps contrast in light.
$accent = if ($Theme -eq 'Dark') { '#A78BFA' } else { '#7C3AED' }
$look = New-BGInfoTheme Midnight -Mode $Theme -Scale $scale -Accent $accent
$glow = if ($Theme -eq 'Dark') { '#7C3AED33' } else { '#C4B5FD66' }

New-BGInfo -Theme $look -Target File {
    New-BGInfoLayout -ColumnWidth ([int](470 * $scale))
    New-BGInfoBackground -Width $Width -Height $Height -GradientAngle 160 `
        -Glow @(
            New-BGInfoBackgroundGlow -X 0.0 -Y 0.0 -Radius 0.5 -Color $glow
            New-BGInfoBackgroundGlow -X 1.0 -Y 0.55 -Radius 0.35 -Color $glow
        )

    $heading = @('JUMP HOST', $jump.Tier.ToUpper(), $jump.Name,
        $jump.Domain.ToUpper()) | Where-Object { $_ }
    New-BGInfoLabel -Name ($heading -join '  ·  ') -Accent `
        -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name 'Privileged access only' `
        -FontSize ([int](80 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name "●  $($jump.Recording)" -Status Critical `
        -FontWeight 600
    New-BGInfoValue -Name 'Signed in' -Value $jump.User
    New-BGInfoValue -Name 'Elevation' -Value $jump.Elevation
    New-BGInfoValue -Name 'MFA' -Value $jump.Mfa
    New-BGInfoValue -Name 'Local admins' -Value $jump.LocalAdmins

    # The rules nobody gets to bend, as large as the policy deserves.
    if (@($jump.Never).Count -gt 0) {
        New-BGInfoLabel -Name ' ' -FontSize ([int](14 * $scale))
        New-BGInfoLabel -Name 'NEVER ON THIS HOST' -Accent `
            -FontSize ([int](15 * $scale)) -FontWeight 700
        foreach ($rule in $jump.Never) {
            New-BGInfoLabel -Name "✕  $($rule.Text)" -Status Critical `
                -FontSize ([int](30 * $scale)) -FontWeight 600
        }
    }

    # Right column: the policy to read, then the tools that belong here.
    if ($jump.Policy) {
        $link = [uri] $jump.Policy
        New-BGInfoQRCode -Region Right -Text $jump.Policy `
            -Title 'Access policy' `
            -Caption "$($link.Host)$($link.AbsolutePath)" `
            -Height ([int](360 * $scale))
    }
    if (@($jump.Allowed).Count -gt 0) {
        New-BGInfoList -Region Right -Title 'Allowed here' -Item $jump.Allowed
    }

    # Bottom band: who else is connected, and the last baseline check.
    $band = @{ Region = 'Bottom'; Height = [int](250 * $scale) }
    if (@($jump.Sessions).Count -gt 0) {
        $jump.Sessions | New-BGInfoTable @band -Title 'Admin sessions' `
            -Note "$(@($jump.Sessions).Count) on this host" -StatusColumn State
    }
    if (@($jump.Baseline).Count -gt 0) {
        New-BGInfoList @band -Title 'Security baseline' -Note $jump.BaselineAt `
            -Item $jump.Baseline -Width ([int](560 * $scale))
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "jump-host-$($Theme.ToLower()).png"
#endregion
