<#
    Linux server. The question for whoever opens the console or the
    rendered status image: what is this box? Its name, distribution and
    kernel, how long it has been up and how loaded it is, how full its
    mounts are, which systemd units failed, what it listens on, when it was
    last updated and who owns it.

    PowerBGInfo renders headless on Linux under PowerShell 7 (preview), so
    -Target File writes the image for a console splash, a KVM or BMC
    screen, or a status page. The data block returns plain values; the
    presentation block only reads them. Comments name the pwsh-friendly
    command or file that supplies each value.
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
function Get-LinuxServerInfo {
    [pscustomobject] @{
        Available = $true
        # [System.Net.Dns]::GetHostName() and hostname --domain
        Name      = 'web-lnx-04'
        Domain    = 'corp.contoso.com'
        # $os = Get-Content /etc/os-release | ConvertFrom-StringData
        # $os.PRETTY_NAME.Trim('"')
        Distro    = 'Ubuntu 24.04.1 LTS'
        # uname -r and uname -m
        Kernel    = '6.8.0-45-generic · x86_64'
        # Get-Uptime (PowerShell 7 reads /proc/uptime)
        Uptime    = '41 days, 6 h'
        # ip -json -4 addr show eth0 | ConvertFrom-Json
        Address   = '10.20.4.14 · eth0'
        # A file your provisioning writes, for example /etc/contoso/owner
        Owner     = 'Web Platform team'
        # (Get-Content /proc/loadavg) -split ' ' gives the 1, 5 and 15
        # minute averages; [Environment]::ProcessorCount the cores.
        Load      = @(1.42, 1.18, 0.97)
        Cores     = 8
        # The 1-minute average sampled every 15 minutes into a small file,
        # or sar -q from sysstat for the last six hours
        LoadHistory = @(0.8, 0.9, 1.1, 0.9, 1.0, 1.4, 2.6, 3.1, 2.2, 1.6, 1.3,
            1.2, 1.1, 1.0, 0.9, 1.1, 1.3, 1.2, 1.5, 1.6, 1.4, 1.3, 1.5, 1.42)
        # df -P --output=target,pcent | Select-Object -Skip 1, or
        # Get-PSDrive -PSProvider FileSystem (Used and Free). Percent used.
        Mounts    = [ordered] @{
            '/'                   = 58
            '/var'                = 83
            '/var/lib/postgresql' = 71
            '/boot'               = 34
        }
        # systemctl list-units --type=service --state=failed --plain --no-legend
        # and systemctl is-active for the services that matter here
        UnitCount = 214
        Units     = @(
            @{ Text = 'backup-sync.service'; Value = 'failed 02:00'
                Status = 'Critical' }
            @{ Text = 'nginx.service'; Value = 'running'; Status = 'Ok' }
            @{ Text = 'postgresql@16-main'; Value = 'running'; Status = 'Ok' }
            @{ Text = 'ssh.service'; Value = 'running'; Status = 'Ok' }
            @{ Text = 'node_exporter.service'; Value = 'running'
                Status = 'Ok' }
            @{ Text = 'certbot.timer'; Value = 'next 03:12'; Status = 'Info' }
        )
        # ss -Htlnp: listening TCP sockets with the owning process
        Listening = @(
            [pscustomobject] @{ Port = 22; Service = 'sshd'
                Bound = '0.0.0.0'; Exposed = 'Internal' }
            [pscustomobject] @{ Port = 443; Service = 'nginx'
                Bound = '0.0.0.0'; Exposed = 'Public' }
            [pscustomobject] @{ Port = 80; Service = 'nginx'
                Bound = '0.0.0.0'; Exposed = 'Redirect' }
            [pscustomobject] @{ Port = 5432; Service = 'postgres'
                Bound = '127.0.0.1'; Exposed = 'Local' }
            [pscustomobject] @{ Port = 9100; Service = 'node_exporter'
                Bound = '10.20.4.14'; Exposed = 'Internal' }
        )
        # Select-String '^Start-Date|^Upgrade' /var/log/apt/history.log,
        # dnf history list on Red Hat family systems, and
        # apt list --upgradable for what is still pending
        Updates   = @(
            @{ Text = 'Pending'; Value = '3 security'; Status = 'Warning' }
            @{ Text = 'Mon 13 Oct · apt upgrade'; Value = '14 packages'
                Status = 'Ok' }
            @{ Text = 'Thu 09 Oct · unattended'; Value = 'openssl'
                Status = 'Ok' }
            @{ Text = 'Thu 02 Oct · unattended'; Value = 'linux-image'
                Status = 'Ok' }
        )
    }
}

$fallback = [pscustomobject] @{
    Available = $false; Name = [System.Net.Dns]::GetHostName(); Domain = ''
    Distro = 'unknown'; Kernel = 'unknown'; Uptime = 'unknown'
    Address = 'unknown'; Owner = 'unknown'; Load = @(); Cores = 0
    LoadHistory = @(); Mounts = [ordered] @{}; UnitCount = 0; Units = @()
    Listening = @(); Updates = @()
}
$source = @{
    Name           = 'Linux server'
    TimeoutSeconds = 15
    ScriptBlock    = ${function:Get-LinuxServerInfo}
    Default        = $fallback
}
$linux = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $linux
$scale = [Math]::Round($Height / 1080, 2)
$look = New-BGInfoTheme Aurora -Mode $Theme -Scale $scale
# The host name in a terminal face, as it appears at the shell prompt.
$mono = 'Cascadia Mono, DejaVu Sans Mono, Menlo, Consolas, monospace'
$failed = @($linux.Units | Where-Object { $_.Status -eq 'Critical' })
$dots = if ($Theme -eq 'Dark') { '#FFFFFF10' } else { '#0F172A12' }

New-BGInfo -Theme $look -Target File {
    New-BGInfoLayout -ColumnWidth ([int](470 * $scale))
    New-BGInfoBackground -Width $Width -Height $Height -Pattern Dots `
        -PatternColor $dots -PatternSpacing ([int](26 * $scale))

    $heading = @('LINUX SERVER', $linux.Distro.ToUpper(),
        $linux.Domain.ToUpper()) | Where-Object { $_ }
    New-BGInfoLabel -Name ($heading -join '  ·  ') -Accent `
        -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $linux.Name -FontFamilyName $mono `
        -FontSize ([int](76 * $scale)) -FontWeight 700
    if (-not $linux.Available) {
        New-BGInfoLabel -Name '●  Server data unavailable' -Status Info `
            -FontWeight 600
    } elseif ($failed.Count -gt 0) {
        $names = ($failed | ForEach-Object { $_.Text }) -join ', '
        $text = "$($failed.Count) failed unit: $names"
        New-BGInfoLabel -Name "●  $text" -Status Critical -FontWeight 600
    } else {
        New-BGInfoLabel -Name '●  All systemd units healthy' -Status Ok `
            -FontWeight 600
    }
    New-BGInfoValue -Name 'Kernel' -Value $linux.Kernel
    New-BGInfoValue -Name 'Uptime' -Value $linux.Uptime
    if (@($linux.Load).Count -eq 3) {
        $load = "{0} · {1} · {2}  (1, 5, 15 min)" -f $linux.Load
        New-BGInfoValue -Name 'Load' -Value $load
    }
    New-BGInfoValue -Name 'Address' -Value $linux.Address
    New-BGInfoValue -Name 'Owner' -Value $linux.Owner

    # Right column: the units that matter, then what the package manager did.
    if (@($linux.Units).Count -gt 0) {
        New-BGInfoList -Region Right -Title 'systemd units' `
            -Note "$($failed.Count) failed of $($linux.UnitCount)" `
            -Item $linux.Units
    }
    if (@($linux.Updates).Count -gt 0) {
        New-BGInfoList -Region Right -Title 'Package updates' `
            -Item $linux.Updates
    }

    # Center: what this box answers on.
    if (@($linux.Listening).Count -gt 0) {
        $linux.Listening | New-BGInfoTable -Region Center -Title 'Listening' `
            -Note 'TCP, ss -tlnp'
    }

    # Bottom band: mounts (amber at 80 %, red at 90 %) and the load over
    # the last six hours (amber at the core count, red at 1.5 times it).
    $band = @{ Region = 'Bottom'; Height = [int](260 * $scale) }
    if ($linux.Mounts.Count -gt 0) {
        New-BGInfoChart @band -Id 'mounts' -Title 'Mounts used' `
            -Kind ProgressBar -Values @($linux.Mounts.Values) `
            -Labels @($linux.Mounts.Keys) -Maximum 100 -ValueSuffix ' %' `
            -NoProgressHandles -ShowLatestValue:$false -NoHistory `
            -ShowDataLabels -WarningThreshold 80 -CriticalThreshold 90 `
            -Width ([int](720 * $scale))
    }
    if (@($linux.LoadHistory).Count -gt 0) {
        $title = "Load average · last 6 h · $($linux.Cores) cores"
        New-BGInfoChart @band -Id 'load' -Title $title `
            -Kind Area -Values $linux.LoadHistory -Minimum 0 -ShowGrid `
            -GridLineCount 3 -ShowLatestValue -ValueFormat '0.00' -NoHistory `
            -WarningThreshold $linux.Cores `
            -CriticalThreshold ($linux.Cores * 1.5)
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "linux-server-$($Theme.ToLower()).png"
#endregion
