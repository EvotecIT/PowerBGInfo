<#
    SQL Server instance. The question for the DBA who signs in to this
    server: are the databases on it safe? That means a recent full backup,
    log backups that keep up, and replicas that hold a current copy. The
    status line names the worst database; the bottom table shows every one.

    The data block returns plain values; the presentation block only reads
    them. Comments name the command or source that supplies each value.
    dbatools (Get-DbaDatabase, Get-DbaAgReplica) returns the same facts if
    you prefer it to plain T-SQL through Invoke-Sqlcmd.
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
function Get-SqlServerSafety {
    [pscustomobject] @{
        Available = $true
        # SELECT @@SERVERNAME, SERVERPROPERTY('ProductVersion'),
        #        SERVERPROPERTY('ProductUpdateLevel'),
        #        SERVERPROPERTY('Edition')
        Name      = 'SQL-FIN-01'
        Instance  = 'SQL-FIN-01\FINANCE'
        Version   = 'SQL Server 2022 CU16'
        Build     = '16.0.4165.4'
        Edition   = 'Enterprise, 16 cores'
        # sys.availability_groups joined to
        # sys.dm_hadr_availability_replica_states (role_desc for the local
        # replica) and the listener from sys.availability_group_listeners
        Group     = 'AG-FINANCE'
        Role      = 'Primary'
        Listener  = 'sqlfin.corp.contoso.com'
        # msdb.dbo.sysjobs and sysjobschedules for your backup jobs, or the
        # schedule in your backup product
        BackupJob = 'Full nightly 02:00, log every 15 min'
        # Percent used per volume that holds database files:
        #   SELECT DISTINCT vs.volume_mount_point, vs.total_bytes,
        #       vs.available_bytes
        #   FROM sys.master_files f
        #   CROSS APPLY sys.dm_os_volume_stats(f.database_id, f.file_id) vs
        # and Get-Volume or Get-PSDrive for the backup target.
        Drives    = [ordered] @{
            'D:  data'     = 71
            'L:  logs'     = 38
            'T:  tempdb'   = 59
            'Backup share' = 84
        }
        # sys.dm_hadr_database_replica_states per replica:
        # synchronization_state, log_send_queue_size and the
        # secondary_lag_seconds column
        Replicas  = @(
            @{ Text = 'SQL-FIN-01  ·  primary'; Value = 'this server'
                Status = 'Ok' }
            @{ Text = 'SQL-FIN-02  ·  synchronous'; Value = 'synchronized'
                Status = 'Ok' }
            @{ Text = 'SQL-DR-01  ·  asynchronous'; Value = '42 s behind'
                Status = 'Warning' }
        )
        # sys.databases (name, state_desc, recovery_model_desc), size from
        # sys.master_files, and the last backups from msdb:
        #   SELECT database_name, type, MAX(backup_finish_date)
        #   FROM msdb.dbo.backupset GROUP BY database_name, type
        # type D is a full backup, L a log backup.
        Databases = @(
            [pscustomobject] @{ Name = 'Finance'; SizeGB = 412; FullHours = 6
                LogMinutes = 12; Recovery = 'Full'; State = 'Online' }
            [pscustomobject] @{ Name = 'Archive'; SizeGB = 520; FullHours = 218
                LogMinutes = 15; Recovery = 'Full'; State = 'Online' }
            [pscustomobject] @{ Name = 'CRM'; SizeGB = 238; FullHours = 6
                LogMinutes = 14; Recovery = 'Full'; State = 'Online' }
            [pscustomobject] @{ Name = 'Payroll'; SizeGB = 186; FullHours = 6
                LogMinutes = 9; Recovery = 'Full'; State = 'Online' }
            [pscustomobject] @{ Name = 'Reporting'; SizeGB = 96; FullHours = 6
                LogMinutes = 104; Recovery = 'Full'; State = 'Online' }
            [pscustomobject] @{ Name = 'StagingETL'; SizeGB = 44; FullHours = 30
                LogMinutes = $null; Recovery = 'Simple'; State = 'Online' }
            [pscustomobject] @{ Name = 'SSISDB'; SizeGB = 12; FullHours = 6
                LogMinutes = 11; Recovery = 'Full'; State = 'Online' }
        )
        # SQLServer:SQL Statistics\Batch Requests/sec from
        # sys.dm_os_performance_counters, sampled every 10 minutes
        Batches   = @(820, 940, 1180, 1460, 1720, 1650, 1540, 1810, 2050, 1930,
            1760, 1880)
    }
}

$fallback = [pscustomobject] @{
    Available = $false; Name = $env:COMPUTERNAME; Instance = 'unknown'
    Version = 'unknown'; Build = ''; Edition = 'unknown'; Group = ''
    Role = 'unknown'; Listener = 'unknown'; BackupJob = 'unknown'
    Drives = [ordered] @{}; Replicas = @(); Databases = @(); Batches = @()
}
$source = @{
    Name           = 'SQL Server safety'
    TimeoutSeconds = 30
    ScriptBlock    = ${function:Get-SqlServerSafety}
    Default        = $fallback
}
$sql = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $sql
$scale = [Math]::Round($Height / 1080, 2)
$look = New-BGInfoTheme Corporate -Mode $Theme -Scale $scale

# One rule for every database: a full backup within 26 hours and a log
# backup within 30 minutes is OK; a week without a full backup is critical.
function Format-Age([double] $Hours) {
    if ($Hours -ge 48) { "$([Math]::Round($Hours / 24)) days" }
    elseif ($Hours -ge 1) { "$([Math]::Round($Hours)) h" }
    else { "$([Math]::Round($Hours * 60)) min" }
}
$rows = foreach ($db in $sql.Databases) {
    $status = if ($db.State -ne 'Online' -or $db.FullHours -gt 168) {
        'Critical'
    }
    elseif ($db.FullHours -gt 26 -or $db.LogMinutes -gt 30) { 'Warning' }
    else { 'OK' }
    $log = if ($null -eq $db.LogMinutes) {
        "$($db.Recovery.ToLower()) recovery"
    } else { Format-Age ($db.LogMinutes / 60) }
    [pscustomobject] @{
        Database    = $db.Name
        Size        = "$($db.SizeGB) GB"
        'Last full' = Format-Age $db.FullHours
        'Last log'  = $log
        Status      = $status
        Order       = @{ Critical = 0; Warning = 1; OK = 2 }[$status]
        Hours       = $db.FullHours
    }
}
$rows = @($rows | Sort-Object Order, Database)
$worst = $rows | Select-Object -First 1

New-BGInfo -Theme $look -Target File {
    New-BGInfoBackground -Width $Width -Height $Height -Pattern Grid `
        -PatternSpacing ([int](48 * $scale))
    New-BGInfoLayout -ColumnWidth ([int](540 * $scale))

    $edition = $sql.Edition.Split(',')[0].ToUpper()
    New-BGInfoLabel -Name "$($sql.Version.ToUpper())  ·  $edition" -Accent `
        -FontSize ([int](15 * $scale)) -FontWeight 700
    New-BGInfoLabel -Name $sql.Name -FontSize ([int](72 * $scale)) `
        -FontWeight 600
    if ($sql.Group) {
        New-BGInfoLabel -Name "$($sql.Role) replica of $($sql.Group)" -Accent `
            -FontSize ([int](26 * $scale)) -FontWeight 600
    }
    if (-not $worst) {
        New-BGInfoLabel -Name '●  Database data unavailable' -Status Info `
            -FontWeight 600
    } elseif ($worst.Status -eq 'OK') {
        $text = "All $($rows.Count) databases backed up on schedule"
        New-BGInfoLabel -Name "●  $text" -Status Ok -FontWeight 600
    } else {
        $issues = @($rows | Where-Object Status -NE 'OK').Count
        $text = "$($worst.Database): last full backup $($worst.'Last full') ago"
        if ($issues -gt 1) { $text += "  ·  $($issues - 1) more to check" }
        New-BGInfoLabel -Name "●  $text" -Status $worst.Status -FontWeight 600
    }
    New-BGInfoValue -Name 'Instance' -Value $sql.Instance
    $tier = $sql.Edition.Split(',')[-1].Trim()
    New-BGInfoValue -Name 'Build' -Value "$($sql.Build), $tier"
    New-BGInfoValue -Name 'Listener' -Value $sql.Listener
    New-BGInfoValue -Name 'Backups' -Value $sql.BackupJob

    # Right column: where the data is, colored by the age of its last full
    # backup, then the replicas that hold a copy of it.
    if ($rows.Count -gt 0) {
        $sql.Databases | New-BGInfoTreemap -Region Right `
            -Title 'Data by database' `
            -Note 'color: last full backup' -ValueProperty SizeGB `
            -ValueSuffix ' GB' -ColorProperty FullHours `
            -WarningThreshold 26 -CriticalThreshold 168 `
            -Height ([int](360 * $scale))
    }
    if (@($sql.Replicas).Count -gt 0) {
        New-BGInfoList -Region Right -Title $sql.Group -Note 'replicas' `
            -Item $sql.Replicas
    }

    # Center, under the text: the workload right now.
    if (@($sql.Batches).Count -gt 0) {
        New-BGInfoChart -Region Center -Id 'batches' `
            -Title 'Batch requests / s  ·  last 2 hours' -Kind Area `
            -Values $sql.Batches -NoHistory -ShowGrid -GridLineCount 2
    }

    # Bottom band: every database, problems first, and the room left for
    # data, logs and backups (amber from 80 %, red from 90 %).
    $band = @{ Region = 'Bottom'; Height = [int](400 * $scale) }
    if ($rows.Count -gt 0) {
        $rows | New-BGInfoTable @band -Title 'Databases' `
            -Note 'from msdb backup history' -Width ([int](1240 * $scale)) `
            -Property Database, Size, 'Last full', 'Last log', Status `
            -StatusColumn Status
    }
    if ($sql.Drives.Count -gt 0) {
        New-BGInfoChart @band -Id 'drives' `
            -Title 'Space for data, logs and backups' `
            -Kind ProgressBar -Values @($sql.Drives.Values) `
            -Labels @($sql.Drives.Keys) -Maximum 100 -ValueSuffix ' %' `
            -NoHistory -NoProgressHandles -ShowLatestValue:$false `
            -ShowDataLabels `
            -WarningThreshold 80 -CriticalThreshold 90
    }
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "sql-server-$($Theme.ToLower()).png"
#endregion
