<#
    Decommission notice. The question for anyone who signs in to this
    server: can I still use it? The countdown and the retirement date come
    first, then where to go instead, who owns the move and how far the
    migration has got.

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
function Get-DecommissionNotice {
    [pscustomobject] @{
        Available   = $true
        # (Get-Date)
        Now         = Get-Date '2026-10-16 08:30'
        # $env:COMPUTERNAME and (Get-CimInstance Win32_OperatingSystem).Caption
        Name        = 'FS-AMS-02'
        Os          = 'Windows Server 2012 R2 Standard'
        # The decommission change in your CMDB or change record, for example
        # ServiceNow:
        #   Invoke-RestMethod ('https://contoso.service-now.com/api/now/' +
        #       'table/change_request?number=CHG0043310')
        Change      = 'CHG0043310'
        ChangeLink  = 'https://change.contoso.com/CHG0043310'
        ReadOnlyAt  = Get-Date '2026-10-26 18:00'
        RetireAt    = Get-Date '2026-11-03 18:00'
        Owner       = 'File Services team · ext. 4410'
        Replacement = 'FS-AMS-11 · Windows Server 2025'
        NewPath     = '\\corp.contoso.com\files'
        # One row per share: Get-SmbShare here, the DFS link it moved to from
        # Get-DfsnFolderTarget -Path '\\corp.contoso.com\files\<share>',
        # and the copy progress from that share's robocopy log
        Shares      = @(
            [pscustomobject] @{ Share = 'Projects'; Size = '2.4 TB'
                Path = '\\corp.contoso.com\files\Projects'
                Copied = 'Thu 15 Oct'; State = 'Ready' }
            [pscustomobject] @{ Share = 'Finance'; Size = '0.9 TB'
                Path = '\\corp.contoso.com\files\Finance'
                Copied = 'Thu 15 Oct'; State = 'Ready' }
            [pscustomobject] @{ Share = 'HR'; Size = '0.6 TB'
                Path = '\\corp.contoso.com\files\HR'
                Copied = '82 %'; State = 'Pending' }
            [pscustomobject] @{ Share = 'Scans'; Size = '2.3 TB'
                Path = '\\corp.contoso.com\files\Scans'
                Copied = '41 %'; State = 'Pending' }
        )
        # The migration runbook steps from the change record; the copy and
        # DFS states from robocopy logs and Get-DfsnFolderTarget.
        Checklist   = @(
            @{ Text = 'Shares copied'; Value = 'robocopy /MIR'; Status = 'Ok' }
            @{ Text = 'DFS links switched'; Value = '2 of 4'
                Status = 'Warning' }
            @{ Text = 'Users notified'; Value = '14 Oct'; Status = 'Ok' }
            @{ Text = 'Backups kept 90 days'; Value = 'to 1 Feb'
                Status = 'Ok' }
            @{ Text = 'Final shutdown'; Value = 'pending'; Status = 'None' }
        )
        # Bytes still to copy after each nightly delta pass, from the robocopy
        # job summary: robocopy <source> <target> /MIR /L /NJH /BYTES
        LeftTB      = [ordered] @{
            'Thu 8' = 6.2; 'Fri 9' = 5.1; 'Sat 10' = 4.6; 'Sun 11' = 4.3
            'Mon 12' = 3.4; 'Tue 13' = 2.6; 'Wed 14' = 1.9; 'Thu 15' = 1.4
        }
        # Sessions still open against this server, sampled each weekday at
        # 09:00 since users were notified:
        # (Get-SmbSession | Measure-Object).Count
        Sessions    = @(214, 207, 188, 161, 122, 85, 51, 23)
    }
}

$fallback = [pscustomobject] @{
    Available = $false; Now = Get-Date; Name = $env:COMPUTERNAME; Os = 'unknown'
    Change = ''; ChangeLink = ''; ReadOnlyAt = $null; RetireAt = $null
    Owner = 'unknown'; Replacement = 'unknown'; NewPath = 'unknown'
    Shares = @(); Checklist = @(); LeftTB = [ordered] @{}; Sessions = @()
}
$source = @{
    Name           = 'Decommission notice'
    TimeoutSeconds = 20
    ScriptBlock    = ${function:Get-DecommissionNotice}
    Default        = $fallback
}
$notice = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $notice
$scale = [Math]::Round($Height / 1080, 2)
$look = New-BGInfoTheme Ember -Mode $Theme -Scale $scale
$dark = $Theme -eq 'Dark'
$glow = if ($dark) { '#F9731638' } else { '#FDBA7477' }

$headline = 'Retirement date unknown'
$when = 'Ask the server owner before you store anything here.'
if ($null -ne $notice.RetireAt) {
    # Whole days left; the last day counts down in hours.
    $left = $notice.RetireAt - $notice.Now
    $days = [int] [Math]::Floor($left.TotalDays)
    $headline = if ($days -gt 1) { "$days days left" }
        elseif ($days -eq 1) { '1 day left' }
        elseif ($left.TotalHours -gt 0) {
            "$([int] [Math]::Ceiling($left.TotalHours)) hours left"
        }
        else { 'Retired' }
    $when = "$($notice.Name) shuts down on " +
        $notice.RetireAt.ToString('dddd d MMMM yyyy, HH:mm')
}

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height -GradientAngle 160 `
        -Glow @(
            New-BGInfoBackgroundGlow -X 0.0 -Y 0.0 -Radius 0.55 -Color $glow
            New-BGInfoBackgroundGlow -X 1.0 -Y 0.55 -Radius 0.35 -Color $glow
        )
    New-BGInfoLayout -ColumnWidth ([int](440 * $scale))

    New-BGInfoLabel -Name "DECOMMISSION NOTICE  ·  $($notice.Name)" -Accent `
        -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $headline -Accent `
        -FontSize ([int](112 * $scale)) -FontWeight 800
    New-BGInfoLabel -Name $when -FontSize ([int](26 * $scale)) -FontWeight 600
    if ($null -ne $notice.ReadOnlyAt) {
        $text = "Read-only from $($notice.ReadOnlyAt.ToString('ddd d MMM')). " +
            "Save new files to $($notice.NewPath)"
        New-BGInfoLabel -Name "●  $text" -Status Warning -FontWeight 600
    }
    New-BGInfoValue -Name 'Use instead' -Value $notice.Replacement
    New-BGInfoValue -Name 'New path' -Value $notice.NewPath
    New-BGInfoValue -Name 'Owner' -Value $notice.Owner
    New-BGInfoValue -Name 'System' -Value $notice.Os

    # Right column: the change ticket on a phone, then the runbook so far.
    if ($notice.ChangeLink) {
        $link = [uri] $notice.ChangeLink
        New-BGInfoQRCode -Region Right -Text $notice.ChangeLink `
            -Title 'Change ticket' `
            -Caption "$($link.Host)$($link.AbsolutePath)" `
            -Height ([int](340 * $scale))
    }
    if (@($notice.Checklist).Count -gt 0) {
        New-BGInfoList -Region Right -Title 'Migration checklist' `
            -Item $notice.Checklist
    }

    # Center: where each share went, so nobody has to ask.
    if (@($notice.Shares).Count -gt 0) {
        $notice.Shares | New-BGInfoTable -Region Center `
            -Title 'Where your share moved' `
            -Note 'DFS namespace' -Property Share, Size, Path, Copied, State `
            -StatusColumn State -Dense
    }

    # Bottom band: the nightly delta copies shrinking towards zero, and the
    # sessions that still connect here.
    $band = @{ Region = 'Bottom'; Height = [int](240 * $scale) }
    if ($notice.LeftTB.Count -gt 0) {
        $nights = @($notice.LeftTB.Keys)
        $title = 'Data left to move  ·  nightly, ' +
            "$($nights[0]) – $($nights[-1]) Oct"
        New-BGInfoChart @band -Id 'left' -Title $title `
            -Kind Bar -Values @($notice.LeftTB.Values) -Labels $nights `
            -ValueSuffix ' TB' -ShowDataLabels -NoHistory `
            -ShowLatestValue:$false
    }
    if (@($notice.Sessions).Count -gt 0) {
        New-BGInfoChart @band -Id 'sessions' `
            -Title 'Still connected  ·  SMB sessions' `
            -Kind Area -Values $notice.Sessions -ShowLatestValue `
            -ValueSuffix ' now' -ShowGrid -GridLineCount 3 -NoHistory `
            -Width ([int](560 * $scale))
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "decommission-notice-$($Theme.ToLower()).png"
#endregion
