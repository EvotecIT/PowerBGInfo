<#
    Backup server. The question for the administrator who signs in first
    thing in the morning: did last night's backups work, and if not, which
    jobs need attention before tonight's window?

    The data block returns plain values; the presentation block only reads
    them. Comments name the command or source that supplies each value.
    The samples are product-neutral: swap in your backup product's API,
    for example Get-VBRJob and Get-VBRBackupSession (Veeam), or
    Get-WBSummary and Get-WBJob (Windows Server Backup).
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
function Get-BackupStatus {
    $today = Get-Date '2026-10-16 07:50'
    [pscustomobject] @{
        Available = $true
        # $env:COMPUTERNAME and (Get-CimInstance Win32_ComputerSystem).Domain
        Name      = 'BKP-AMS-01'
        Domain    = 'corp.contoso.com'
        # Results of the sessions that ran last night, for example
        #   Get-VBRBackupSession | Where-Object CreationTime -GT $since |
        #       Group-Object { $_.Result }
        # or (Get-WBJob -Previous 1).JobState for Windows Server Backup.
        Succeeded = 47
        Warning   = 1
        Failed    = 1
        # First session start and last session end of the night
        Started   = '22:00'
        Finished  = '04:12'
        Protected = '18.4 TB from 212 machines'
        # (Get-WBSummary).NextBackupTime, or the job schedule:
        # Get-VBRJob | Get-VBRJobScheduleOptions
        NextWindow = '22:00'
        NextDetail = 'tonight · 49 jobs · in 14 h'
        # Your retention policy, or Get-VBRJob | Get-VBRJobOptions
        Retention  = '30 daily, 12 monthly'
        # Failed and warning sessions with their reason, for example
        # (Get-VBRBackupSession -Name <job>).Logger.GetLog().UpdatedRecords
        Attention = @(
            [pscustomobject] @{
                Job = 'SQL-PRD cluster'; Target = 'SQL-PRD-02'
                Reason = 'VSS writer timed out'
                Retries = '3 of 3'; State = 'Failed'
            }
            [pscustomobject] @{
                Job = 'File servers'; Target = 'FS-AMS-03'
                Reason = 'Skipped 12 locked files'
                Retries = '1 of 3'; State = 'Warning'
            }
        )
        # Get-VBRBackupRepository, then free space per repository; for a
        # local repository Get-Volume -DriveLetter R gives Size and
        # SizeRemaining. Percent used.
        Repositories = [ordered] @{
            'Primary · R:'   = 71
            'Archive · S:'   = 86
            'Object storage' = 54
        }
        # Copies off this server. The immutable lock comes from your
        # repository settings, or from the bucket itself:
        #   Get-S3ObjectLockConfiguration -BucketName <bucket> (AWS.Tools.S3)
        # The offsite copy lag from your backup copy job, for example
        #   Get-VBRBackupCopyJob and its last session.
        Copies = @(
            @{ Text = 'Immutable copy'; Value = 'locked 30 days'
                Status = 'Ok' }
            @{ Text = 'Offsite copy'; Value = 'synced 05:40'; Status = 'Ok' }
            @{ Text = 'Tape export'; Value = 'Sat 06:00'; Status = 'Info' }
            @{ Text = 'Restore test'; Value = '9 days ago'; Status = 'Warning' }
        )
        # Percent of jobs that succeeded on each of the last 14 nights,
        # oldest first, from the same session history as below.
        Rate = @(100, 100, 100, 100, 71, 90, 100,
            100, 100, 94, 100, 100, 100, 96)
        # Nights with failed or warning jobs, from the session history:
        #   Get-VBRBackupSession | Where-Object Result -NE 'Success' |
        #       Group-Object { $_.CreationTime.Date }
        # A night without a problem is simply not in the list.
        History = foreach ($day in 1..364) {
            $value = if ($day % 9 -eq 0) { 1 }
            elseif ($day % 23 -eq 5) { 3 } else { 0 }
            # A storage outage ten days ago failed 14 jobs.
            if ($day -eq 10) { $value = 14 }
            if ($day -eq 9) { $value = 5 }
            if ($day -eq 1) { $value = 2 }
            if ($value -gt 0) {
                [pscustomobject] @{
                    Date = $today.Date.AddDays(-$day); Value = $value
                }
            }
        }
        Now = $today
    }
}

$fallback = [pscustomobject] @{
    Available = $false; Name = $env:COMPUTERNAME; Domain = 'unknown'
    Succeeded = 0; Warning = 0; Failed = 0; Started = ''; Finished = ''
    Protected = 'unknown'; NextWindow = 'unknown'; NextDetail = ''
    Retention = 'unknown'; Attention = @()
    Repositories = [ordered] @{}; Copies = @(); Rate = @()
    History = @()
    Now = Get-Date
}
$source = @{
    Name           = 'Backup status'
    TimeoutSeconds = 30
    ScriptBlock    = ${function:Get-BackupStatus}
    Default        = $fallback
}
$backup = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $backup
$scale = [Math]::Round($Height / 1080, 2)
$look = New-BGInfoTheme Graphite -Mode $Theme -Scale $scale -Accent '#2DD4BF'
$total = $backup.Succeeded + $backup.Warning + $backup.Failed
$glow = if ($Theme -eq 'Dark') { '#2DD4BF26' } else { '#5EEAD455' }

New-BGInfo -Theme $look -Target File {
    New-BGInfoLayout -ColumnWidth ([int](470 * $scale))
    New-BGInfoBackground -Width $Width -Height $Height -Glow @(
        New-BGInfoBackgroundGlow -X 0.0 -Y 0.0 -Radius 0.55 -Color $glow
    )

    $heading = "BACKUP SERVER  ·  $($backup.Name)  ·  " +
        $backup.Domain.ToUpper()
    New-BGInfoLabel -Name $heading `
        -Accent -FontSize ([int](15 * $scale)) -FontWeight 700
    if ($total -eq 0) {
        New-BGInfoLabel -Name 'No results' -FontSize ([int](96 * $scale)) `
            -FontWeight 700
        New-BGInfoLabel -Name '●  Backup results unavailable' -Status Info `
            -FontWeight 600
    } else {
        New-BGInfoLabel -Name "$($backup.Succeeded) of $total" `
            -FontSize ([int](112 * $scale)) -FontWeight 700
        New-BGInfoLabel -Name 'jobs succeeded last night' `
            -FontSize ([int](26 * $scale)) -FontWeight 600
        $problems = @()
        if ($backup.Failed -gt 0) { $problems += "$($backup.Failed) failed" }
        if ($backup.Warning -gt 0) {
            $problems += "$($backup.Warning) with warnings"
        }
        if ($backup.Failed -gt 0) {
            $text = "$($problems -join ', '): retry before tonight"
            New-BGInfoLabel -Name "●  $text" -Status Critical -FontWeight 600
        } elseif ($backup.Warning -gt 0) {
            New-BGInfoLabel -Name "●  $($problems -join ', ')" `
                -Status Warning -FontWeight 600
        } else {
            New-BGInfoLabel -Name '●  Every job succeeded' -Status Ok `
                -FontWeight 600
        }
    }
    New-BGInfoValue -Name 'Last night' `
        -Value "$($backup.Started) – $($backup.Finished)"
    New-BGInfoValue -Name 'Protected' -Value $backup.Protected
    New-BGInfoValue -Name 'Retention' -Value $backup.Retention

    # Right column: when the next run starts, where the copies of the data
    # are, and how full each repository is (amber at 80 %, red at 90 %).
    New-BGInfoMetric -Region Right -Label 'Next window' `
        -Value $backup.NextWindow `
        -Caption $backup.NextDetail -Status Info -Height ([int](170 * $scale))
    if (@($backup.Copies).Count -gt 0) {
        New-BGInfoList -Region Right -Title 'Copies' -Note '3-2-1 rule' `
            -Item $backup.Copies
    }
    if ($backup.Repositories.Count -gt 0) {
        New-BGInfoChart -Region Right -Id 'repositories' `
            -Title 'Repository space used' `
            -Kind ProgressBar -Values @($backup.Repositories.Values) `
            -Labels @($backup.Repositories.Keys) -Maximum 100 `
            -ValueSuffix ' %' -NoProgressHandles -ShowLatestValue:$false `
            -NoHistory -ShowDataLabels -WarningThreshold 80 `
            -CriticalThreshold 90 -Height ([int](220 * $scale))
    }

    # Center: only the jobs that need someone, failures first.
    if (@($backup.Attention).Count -gt 0) {
        $backup.Attention | Sort-Object { $_.State -ne 'Failed' } |
            New-BGInfoTable -Region Center -Title 'Needs attention' `
                -Note 'last night' -StatusColumn State
    }

    # Bottom band: a year of nights, so a bad night stands out, and the
    # success rate of the last two weeks (amber below 98 %, red below 90 %).
    $band = @{ Region = 'Bottom'; Height = [int](270 * $scale) }
    if (@($backup.History).Count -gt 0) {
        @($backup.History) | New-BGInfoCalendarHeatmap @band `
            -Title 'Nights with failed or warning jobs' -Note 'last 12 months' `
            -Color $look.Critical -Width ([int](1240 * $scale))
    }
    if (@($backup.Rate).Count -gt 0) {
        New-BGInfoChart @band -Id 'rate' `
            -Title 'Succeeded · last 14 nights' -Kind Bar `
            -Values $backup.Rate -Minimum 0 -Maximum 100 -ValueSuffix ' %' `
            -ShowGrid -GridLineCount 3 -ShowLatestValue:$false -NoHistory `
            -WarningThreshold 98 -CriticalThreshold 90 -ThresholdDirection Below
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "backup-server-$($Theme.ToLower()).png"
#endregion
