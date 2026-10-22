<#
    Production or test. The question for the administrator who just signed
    in over Remote Desktop: which environment is this, and what am I allowed
    to change here? The whole wallpaper takes the environment's colour, so a
    production server never looks like its test twin.

    The data block returns plain values; the presentation block only reads
    them. Comments name the command or source that supplies each value.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Dark',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output'),
    # Render the same layout for another environment to compare them.
    [ValidateSet('Production', 'Acceptance', 'Test', 'Development')]
    [string] $Environment = 'Production'
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
function Get-EnvironmentInfo {
    param([string] $Environment)
    $today = Get-Date '2026-10-16 09:40'
    # One machine per environment for the same service. In practice read the
    # environment from a CMDB, a registry value set at build time, or a tag:
    #   (Get-ItemProperty 'HKLM:\SOFTWARE\Contoso\Server').Environment
    #   (Invoke-RestMethod -Headers @{ Metadata = 'true' } -Uri
    #       'http://169.254.169.254/metadata/instance?api-version=2021-02-01'
    #   ).compute.tagsList
    $hosts = [ordered] @{
        Production  = 'PAY-PRD-07'
        Acceptance  = 'PAY-UAT-02'
        Test        = 'PAY-TST-04'
        Development = 'PAY-DEV-01'
    }
    [pscustomobject] @{
        Available   = $true
        Environment = $Environment
        Name        = $hosts[$Environment]
        Service     = 'Payments API'
        Company     = 'Contoso Finance'
        # (Get-CimInstance Win32_OperatingSystem).Caption
        Os          = 'Windows Server 2025 Standard'
        # Get-WebSite, or the role your build pipeline records
        Role        = 'IIS application server, pool PaymentsApi'
        Owner       = 'Payments Platform team'
        Data        = 'Confidential: cardholder data in scope'
        # Your change policy, per environment
        Policy      = switch ($Environment) {
            'Production' { 'Every change needs an approved CAB ticket' }
            'Acceptance' { 'Changes need sign-off from the release manager' }
            'Test' { 'Free to change; reset from image every Monday' }
            'Development' { 'Free to change; nothing here is backed up' }
        }
        Link        = 'https://change.contoso.com/policy/payments'
        # Your change calendar API, for example ServiceNow change_request:
        # one lane per change window, with its approval state.
        Windows     = @(
            [pscustomobject] @{
                Lane = 'Monthly patching'; Category = 'Done'
                Start = $today.Date.AddDays(-1).AddHours(22)
                End = $today.Date.AddDays(-1).AddHours(25)
            }
            [pscustomobject] @{
                Lane = 'Release 4.12'; Category = 'Approved'
                Start = $today.Date.AddDays(1).AddHours(21)
                End = $today.Date.AddDays(1).AddHours(23)
            }
            [pscustomobject] @{
                Lane = 'SQL index rebuild'; Category = 'Approved'
                Start = $today.Date.AddDays(2).AddHours(1)
                End = $today.Date.AddDays(2).AddHours(4)
            }
            [pscustomobject] @{
                Lane = 'Firewall rule change'; Category = 'Awaiting approval'
                Start = $today.Date.AddDays(3).AddHours(19)
                End = $today.Date.AddDays(3).AddHours(21)
            }
        )
        Now         = $today
        # The same service in every environment, from your CMDB
        Siblings    = foreach ($entry in $hosts.GetEnumerator()) {
            [pscustomobject] @{
                Environment = $entry.Key
                Host        = $entry.Value
                Version     = switch ($entry.Key) {
                    'Production' { '4.11.2' } 'Acceptance' { '4.12 RC2' }
                    'Test' { '4.12 RC3' } 'Development' { '4.13 dev' }
                }
            }
        }
        # Get-HotFix | Sort-Object InstalledOn | Select-Object -Last 1, plus
        # the last deployment from your release pipeline
        LastChange  = 'Release 4.11.2, 3 days ago, CHG0042117'
    }
}

$fallback = [pscustomobject] @{
    Available = $false; Environment = $Environment; Name = $env:COMPUTERNAME
    Service = 'unknown'; Company = ''; Os = 'unknown'; Role = 'unknown'
    Owner = 'unknown'; Data = 'unknown'
    Policy = 'Environment unknown: treat as production'
    Link = ''; Windows = @(); Now = Get-Date; Siblings = @()
    LastChange = 'unknown'
}
$source = @{
    Name           = 'Environment'
    TimeoutSeconds = 10
    ScriptBlock    = ${function:Get-EnvironmentInfo}
    ArgumentList   = @($Environment)
    Default        = $fallback
}
$envInfo = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $envInfo
$scale = [Math]::Round($Height / 1080, 2)
# One colour per environment drives the accent, the glows and the headline.
$colors = @{
    Production  = @{
        Accent = '#EF4444'; Glow = '#EF444455'; GlowLight = '#FCA5A588'
        Status = 'Critical'
    }
    Acceptance  = @{
        Accent = '#F59E0B'; Glow = '#F59E0B4D'; GlowLight = '#FCD34D88'
        Status = 'Warning'
    }
    Test        = @{
        Accent = '#22C55E'; Glow = '#22C55E44'; GlowLight = '#86EFAC88'
        Status = 'Ok'
    }
    Development = @{
        Accent = '#3B82F6'; Glow = '#3B82F644'; GlowLight = '#93C5FD88'
        Status = 'Info'
    }
}
$color = $colors[$envInfo.Environment]
$glow = if ($Theme -eq 'Dark') { $color.Glow } else { $color.GlowLight }
$look = New-BGInfoTheme Midnight -Mode $Theme -Scale $scale `
    -Accent $color.Accent

New-BGInfo -Theme $look -Target File {
    New-BGInfoLayout -ColumnWidth ([int](460 * $scale))
    New-BGInfoBackground -Width $Width -Height $Height -GradientAngle 150 `
        -Glow @(
            New-BGInfoBackgroundGlow -X 0.05 -Y 0.05 -Radius 0.6 -Color $glow
            New-BGInfoBackgroundGlow -X 0.95 -Y 1.0 -Radius 0.45 -Color $glow
        )

    $heading = "$($envInfo.Company.ToUpper())  ·  " +
        $envInfo.Service.ToUpper()
    New-BGInfoLabel -Name $heading `
        -Accent -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $envInfo.Environment.ToUpper() `
        -Color $color.Accent -FontSize ([int](128 * $scale)) -FontWeight 800
    New-BGInfoLabel -Name $envInfo.Name -FontSize ([int](44 * $scale)) `
        -FontWeight 600
    New-BGInfoLabel -Name "●  $($envInfo.Policy)" -Status $color.Status `
        -FontWeight 600
    New-BGInfoValue -Name 'Role' -Value $envInfo.Role
    New-BGInfoValue -Name 'System' -Value $envInfo.Os
    New-BGInfoValue -Name 'Owner' -Value $envInfo.Owner
    New-BGInfoValue -Name 'Data' -Value $envInfo.Data
    New-BGInfoValue -Name 'Last change' -Value $envInfo.LastChange

    # Right column: where to read the policy, and the same service elsewhere.
    if ($envInfo.Link) {
        $link = [uri] $envInfo.Link
        New-BGInfoQRCode -Region Right -Text $envInfo.Link `
            -Title 'Change policy' `
            -Caption "$($link.Host)$($link.AbsolutePath)" `
            -Height ([int](400 * $scale))
    }
    if (@($envInfo.Siblings).Count -gt 0) {
        # Each environment in its own colour; the one you are on says so.
        $environments = foreach ($sibling in $envInfo.Siblings) {
            $here = if ($sibling.Environment -eq $envInfo.Environment) {
                '  (here)'
            } else { '' }
            @{
                Text   = "$($sibling.Environment)$here"
                Value  = "$($sibling.Host)  ·  $($sibling.Version)"
                Status = $colors[$sibling.Environment].Status
            }
        }
        New-BGInfoList -Region Right -Title 'Same service' -Item $environments
    }

    # Bottom band: this week's change windows, so nobody is surprised.
    if (@($envInfo.Windows).Count -gt 0) {
        $states = @{
            'Done'              = $look.Ok
            'Approved'          = $look.Muted
            'Awaiting approval' = $look.Warning
        }
        $windows = $envInfo.Windows | Select-Object *, @{
            Name = 'Color'; Expression = { "$($states[$_.Category].ToHex())" }
        }
        New-BGInfoTimeline -Region Bottom -Title 'Change windows' `
            -Note 'this week, local time' -Item $windows -Now $envInfo.Now `
            -Height ([int](250 * $scale))
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "environment-marker-$($Theme.ToLower()).png"
#endregion
