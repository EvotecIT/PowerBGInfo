function Get-MyWallpaperData {
    $os = Get-CimInstance -ClassName Win32_OperatingSystem
    $disks = Get-CimInstance -ClassName Win32_LogicalDisk `
        -Filter 'DriveType = 3'
    $uptime = (Get-Date) - $os.LastBootUpTime
    $free = $os.FreePhysicalMemory / $os.TotalVisibleMemorySize

    $volumes = [ordered] @{}
    foreach ($disk in $disks) {
        $used = 1 - $disk.FreeSpace / $disk.Size
        $volumes[$disk.DeviceID] = [int] (100 * $used)
    }
    [pscustomobject] @{
        Windows = $os.Caption
        Uptime = '{0} days {1} h' -f $uptime.Days, $uptime.Hours
        Memory = [int] (100 * (1 - $free))
        Volumes = $volumes
    }
}
