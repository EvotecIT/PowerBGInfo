# Running PowerBGInfo in production

This guide explains what PowerBGInfo does when it runs, which deployment
route fits which environment, and the one script the other guides build on.

Guides in this folder:

| Guide | Covers |
| --- | --- |
| [Production.md](Production.md) | How the module behaves, choosing a route, the reference script |
| [TaskScheduler.md](TaskScheduler.md) | Logon, unlock and timed refresh with `Register-ScheduledTask` |
| [GroupPolicy.md](GroupPolicy.md) | Logon scripts, Group Policy Preferences tasks, wallpaper and lock screen policies |
| [Intune.md](Intune.md) | Win32 app packaging, platform scripts, Remediations, Personalization CSP |
| [ConfigurationManager.md](ConfigurationManager.md) | Applications, packages and Run Scripts |
| [RemoteDesktop.md](RemoteDesktop.md) | Remote Desktop Services, Azure Virtual Desktop, Windows 365 |
| [Security.md](Security.md) | Rights, signing, App Control, logging |
| [Troubleshooting.md](Troubleshooting.md) | Frequently asked questions and where to look |

These guides describe the module as of the release that includes themes,
regions, widgets, `Get-BGInfoSystemInfo` and `Invoke-BGInfoDataSource`.
Version 2.0.3 on the PowerShell Gallery does not have those cmdlets.

## What one run does

PowerBGInfo renders an image, writes it to disk and, depending on
`-Target`, applies it. Then it exits. It has no service, no refresh timer
and no cmdlet that creates a scheduled task. To keep the values current,
run it again from Task Scheduler, Group Policy, Intune, Configuration
Manager or your RMM tool.

| `-Target` | What it does | Needs elevation |
| --- | --- | --- |
| `Wallpaper` (default) | Sets the desktop wallpaper of the account that runs the command | No |
| `File` | Writes the image only | No |
| `LogonScreen` | Sets the lock screen image (see below) | Yes |
| `Both` | `Wallpaper` and `LogonScreen` | Yes |
| `Wallpaper` with `-AllUsers` | Also writes the wallpaper path into every local profile | Yes |

The details below come from the module source and from the DesktopManager
4.4.0 package that the module uses to talk to Windows.

### Setting the wallpaper

- The wallpaper is set per monitor through the Windows shell
  `IDesktopWallpaper` interface: `SetWallpaper` with the monitor's device
  path. `-MonitorIndex` (default `0`) picks the monitor. One run renders and
  applies one image for one monitor.
- `-WallpaperFit` (default `Center`) calls `IDesktopWallpaper::SetPosition`,
  which has no monitor argument: the fit applies to all monitors.
- When a monitor has no device path, as on some Remote Desktop session hosts,
  or the interface fails, DesktopManager falls back to
  `SystemParametersInfo(SPI_SETDESKWALLPAPER)`, which sets one wallpaper for
  the whole desktop.
- By default PowerBGInfo first applies a temporary copy named
  `<name>_refresh_<guid>.<ext>` and then the final file, and deletes the copy.
  This works around Windows reusing a cached wallpaper after sign-in.
  `-DisableWallpaperRefresh` turns it off.
- If a Windows wallpaper slideshow is running and you pass neither `-FilePath`
  nor `New-BGInfoBackground`, PowerBGInfo renders every slideshow image and
  restarts the slideshow with the rendered copies.
  `-DisableWallpaperSlideshow` renders one static image instead.
- Wallpaper and lock screen targets write PNG, JPEG or BMP. Other extensions
  in `-OutputFileName` become `.png`. `-Target File` also writes GIF, PPM and
  TIFF.

### The image size and the base image

- Without `New-BGInfoBackground`, the base image is `-FilePath`, or, when
  that is empty, the wallpaper currently set on that monitor. If neither
  exists, PowerBGInfo paints the desktop background colour (or
  `-BackgroundColor`) at the monitor size.
- `New-BGInfoBackground` with `-Width 0 -Height 0` (the default) renders at
  the size of the monitor that `-MonitorIndex` points to. When the module
  cannot read the monitors, for example in a session without a desktop, it
  uses 1920 x 1080.
- **Always give production runs a fixed base**: `New-BGInfoBackground`
  (optionally with `-ImagePath` for a corporate image) or `-FilePath`.
  Without one, the second run uses the first run's output as its base image
  and draws the text a second time on top of it. The same happens with a
  running slideshow, because after the first run the slideshow points at the
  rendered images.

### Files it writes

| Path | Written by | Purpose |
| --- | --- | --- |
| `-ConfigurationDirectory` | `New-BGInfo` | Created if missing. Required parameter. |
| `<ConfigurationDirectory>\<OutputFileName>` | `New-BGInfo` | The image. Without `-OutputFileName`: `<wallpaper name>_PowerBgInfo.<ext>` or `PowerBgInfo.png`. |
| `<ConfigurationDirectory>\Charts\<Id>.txt` | `New-BGInfoChart` | Chart history between runs, unless `-NoHistory`. Up to `-MaxPoints` (60) samples. |
| `<ConfigurationDirectory>\<name>_PowerBgInfo_001.<ext>` ... | Slideshow mode | One image per slideshow source. |
| `-JsonPath` | `New-BGInfo` | Optional configuration export. |
| `%APPDATA%\DesktopManager\wallpaper-history.json` | DesktopManager | A list of applied wallpaper paths. The `DESKTOPMANAGER_HISTORY_PATH` environment variable moves it. |

### All users

`-AllUsers` needs elevation. It writes `Wallpaper`, `WallpaperStyle` and
`TileWallpaper` under `Control Panel\Desktop` in every profile listed under
`HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList` that has an
`NTUSER.DAT`, loading hives that are not loaded, and in
`C:\Users\Default\NTUSER.DAT` unless you add `-ExcludeDefaultUserProfile`.
Then it applies the wallpaper to the account that runs the command.

It stages **one** image for everyone. It does not render per-user values, and
it does not notify other signed-in sessions; they pick the value up the next
time Windows reads it, normally at sign-in. Put the image somewhere every
user can read, such as `C:\ProgramData\PowerBGInfo`.

### Lock screen

`-Target LogonScreen` and `-Target Both` stop with "Operation requires
administrative privileges" unless the process is elevated. What happens next
depends on the PowerShell host, because the module loads a different
DesktopManager build in each:

- **Windows PowerShell 5.1**: it calls the WinRT
  `Windows.System.UserProfile.LockScreen.SetImageFileAsync` API. That API sets
  the lock screen of the account that runs the command. In an elevated
  session started by another administrator account, that is the
  administrator's lock screen, not the user's.
- **PowerShell 7**: that WinRT API is not reachable from the build the module
  uses, so it goes straight to the fallback.
- **Fallback** (also used when the API fails): it writes the image path to
  `HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization\LockScreenImage`.
  That is the registry value of the Group Policy setting "Force a specific
  default lock screen and logon image", which Microsoft documents as applying
  only to Enterprise, Education and Server editions.

The PowerShell 7 path was read from the source; it was not run on a test
machine while writing this guide. For a managed fleet, the documented route
is more predictable: render the image with `-Target File` and deliver it with
the policy or the Personalization CSP. See [GroupPolicy.md](GroupPolicy.md)
and [Intune.md](Intune.md).

### Who "the user" is

When the script runs as SYSTEM, `UserName` and `FullUserName` describe
SYSTEM, and `-Target Wallpaper` changes SYSTEM's own wallpaper, which nobody
sees. The `LoggedOnUser` token and `Get-BGInfoSystemInfo.LoggedOnUser` read
`Win32_ComputerSystem.UserName`, which reports the console user. Per-user
wallpapers have to run in the user's own session.

## Choosing a route

| Environment | Recommended route | Runs as | Guide |
| --- | --- | --- | --- |
| Domain-joined PCs | Group Policy Preferences scheduled task: logon and unlock | Each signed-in user | [GroupPolicy.md](GroupPolicy.md) |
| Intune-managed PCs | Win32 app that installs the module and registers the task | Installer: SYSTEM. Task: each user | [Intune.md](Intune.md) |
| Configuration Manager | Application or package that installs the module and registers the task | Installer: SYSTEM. Task: each user | [ConfigurationManager.md](ConfigurationManager.md) |
| Standalone or a few machines | `Register-ScheduledTask` | Each signed-in user | [TaskScheduler.md](TaskScheduler.md) |
| RDS session hosts, AVD multi-session | Per-session task with a per-user output folder | Each session's user | [RemoteDesktop.md](RemoteDesktop.md) |
| AVD personal desktops, Windows 365 | Same as a physical PC | Each signed-in user | [RemoteDesktop.md](RemoteDesktop.md) |
| Shared or kiosk PCs | Logon task; machine image with `-AllUsers` for the default profile | User, or SYSTEM for staging | [TaskScheduler.md](TaskScheduler.md) |
| Lock screen, Enterprise or Education | SYSTEM task renders `-Target File`; policy or CSP shows it | SYSTEM | [GroupPolicy.md](GroupPolicy.md), [Intune.md](Intune.md) |
| Servers with Server Core | Not applicable: no desktop to show a wallpaper | | [Troubleshooting.md](Troubleshooting.md) |

Whatever the route, the same three pieces repeat:

1. Install the module for all users (below).
2. Put the wallpaper script where standard users cannot change it, for
   example `C:\Program Files\PowerBGInfo\Set-BGInfoWallpaper.ps1`.
3. Run it in each user's session at sign-in and unlock, optionally on a
   timer.

## Getting the module onto machines

PowerBGInfo supports Windows PowerShell 5.1 and PowerShell 7 (the manifest
lists both editions and ships .NET Framework 4.7.2 and .NET 8 binaries).
Pick one host per route and use the module path of that host:

| Host | All-users module path |
| --- | --- |
| Windows PowerShell 5.1 (`powershell.exe`) | `$env:ProgramFiles\WindowsPowerShell\Modules` |
| PowerShell 7 (`pwsh.exe`) | `$env:ProgramFiles\PowerShell\Modules` |

PowerShell 7 also searches the Windows PowerShell all-users path, so a copy
under `WindowsPowerShell\Modules` serves both hosts. Most deployment agents
start Windows PowerShell: Group Policy logon scripts on the PowerShell
Scripts tab, Intune platform scripts and Remediations, and Configuration
Manager Run Scripts. In a 32-bit PowerShell process `$env:ProgramFiles` is
`C:\Program Files (x86)`, so a module installed by a 64-bit process is not
found. Run deployment scripts in 64-bit PowerShell.

Online, from an elevated session, pinned to the version you tested:

```powershell
$version = '2.1.0'   # replace with the version you tested
Install-Module -Name PowerBGInfo -RequiredVersion $version `
    -Scope AllUsers -Repository PSGallery
```

Offline, save once on a connected machine and copy the folder:

```powershell
$version = '2.1.0'   # replace with the version you tested
Save-Module -Name PowerBGInfo -RequiredVersion $version `
    -Path 'C:\Staging\Modules' -Repository PSGallery

# On the target, elevated. Keep the <version> folder level.
$target = Join-Path $env:ProgramFiles 'WindowsPowerShell\Modules'
Copy-Item -Path 'C:\Staging\Modules\PowerBGInfo' -Destination $target `
    -Recurse -Force
```

`Save-Module` creates `PowerBGInfo\<version>\`. Keeping that layout lets
several versions sit side by side; the script can then load one exactly:

```powershell
Import-Module -Name PowerBGInfo -RequiredVersion '2.1.0'
```

Check what a machine has:

```powershell
Get-Module -Name PowerBGInfo -ListAvailable |
    Select-Object Name, Version, ModuleBase
```

## The reference script

Every guide runs this script. It follows the data-first pattern of
`Examples/Run.BGInfo.Template.ps1`: one region collects data, one region
only draws it. It renders one image per monitor, writes to a per-user
folder, keeps a transcript of the last run and exits with `1` on failure so
the scheduler records it.

Save it as `C:\Program Files\PowerBGInfo\Set-BGInfoWallpaper.ps1`. Put an
optional corporate image next to it as `Corporate.jpg`.

```powershell
#Requires -Version 5.1
<#
    Set-BGInfoWallpaper.ps1
    Renders one wallpaper per monitor for the signed-in user and applies
    it. Run it in the user's session. -Target File only writes the images,
    which is how to test it.
#>
param(
    [ValidateSet('Wallpaper', 'File')]
    [string] $Target = 'Wallpaper',
    [string] $BaseImage = (Join-Path $PSScriptRoot 'Corporate.jpg'),
    [string] $OutputDirectory = (
        Join-Path $env:LOCALAPPDATA 'PowerBGInfo')
)

$ErrorActionPreference = 'Stop'
$exitCode = 0
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$log = Join-Path $OutputDirectory 'last-run.log'
Start-Transcript -Path $log | Out-Null

try {
    Import-Module -Name PowerBGInfo

    #region 1. Data: your logic goes here
    # Read-only; each WMI probe inside has its own five-second limit.
    $facts = Get-BGInfoSystemInfo

    function Get-MyWallpaperData {
        param($ComputerName)
        # your logic: CMDB, Active Directory, a REST API...
        # (Invoke-RestMethod -Uri $uri -TimeoutSec 5).owner
        [pscustomobject] @{ Owner = 'Finance IT'; Support = 'ext. 4357' }
    }
    $fallback = [pscustomobject] @{
        Owner = 'Owner unavailable'; Support = 'ext. 4357'
    }
    $dataSource = @{
        Name           = 'Wallpaper data'
        TimeoutSeconds = 10
        Default        = $fallback
        ScriptBlock    = ${function:Get-MyWallpaperData}
        ArgumentList   = $env:COMPUTERNAME
    }
    $data = Invoke-BGInfoDataSource @dataSource
    #endregion

    #region 2. Presentation: reads only $facts and $data
    $monitors = @(
        try { [DesktopManager.Monitors]::new().GetMonitors() } catch { }
    )
    if ($monitors.Count -eq 0) {
        # No monitor reported: render once at the 1920 x 1080 fallback.
        $monitors = @([pscustomobject] @{
            Index = 0; PositionTop = 0; PositionBottom = 1080
        })
    }
    $background = @{}
    if (Test-Path -LiteralPath $BaseImage) {
        $background.ImagePath = $BaseImage
    }

    foreach ($monitor in $monitors) {
        $height = $monitor.PositionBottom - $monitor.PositionTop
        $scale = [Math]::Max(1, [Math]::Round($height / 1080, 2))
        $theme = New-BGInfoTheme -Preset Graphite -Mode Dark -Scale $scale
        $render = @{
            Theme                  = $theme
            Target                 = $Target
            MonitorIndex           = $monitor.Index
            WallpaperFit           = 'Fill'
            ConfigurationDirectory = $OutputDirectory
            OutputFileName         = "wallpaper-$($monitor.Index).png"
        }
        New-BGInfo @render {
            New-BGInfoBackground @background
            New-BGInfoLabel -Name $data.Owner -Accent -FontWeight 700
            New-BGInfoLabel -Name $facts.ComputerName `
                -FontSize ([int](48 * $scale)) -FontWeight 600
            New-BGInfoValue -Name 'User' -Value $facts.UserName
            New-BGInfoValue -Name 'OS' -Value $facts.OSName
            New-BGInfoValue -Name 'IPv4' -Value $facts.IPv4Address
            New-BGInfoValue -Name 'Uptime' -Value $facts.UptimeText
            New-BGInfoValue -Name 'Support' -Value $data.Support
            $facts.Disks | New-BGInfoTable -Region Right -Title 'Disks' `
                -Property Drive, FreeGB, UsedPercent
        }
    }
    #endregion
} catch {
    Write-Warning "PowerBGInfo failed: $($_.Exception.Message)"
    $exitCode = 1
} finally {
    Stop-Transcript | Out-Null
}
exit $exitCode
```

Notes on the script:

- `[DesktopManager.Monitors]` is the monitor list from the DesktopManager
  assembly that PowerBGInfo loads. Its `Index` values are the same indexes
  `-MonitorIndex` uses. PowerBGInfo has no monitor cmdlet of its own.
- Each monitor gets its own `-OutputFileName`. Two monitors that share one
  file both show whichever image was written last.
- Charts keep history per `-Id`. If you add `New-BGInfoChart` to a
  per-monitor loop, give each monitor its own `-Id` or use `-NoHistory`;
  otherwise every run appends one sample per monitor to the same history.
- `Invoke-BGInfoDataSource` runs the function in its own runspace, so it
  cannot see the script's variables. Pass what it needs with
  `-ArgumentList`. When the function throws, returns nothing or runs out of
  time, the cmdlet writes a warning and returns `-Default`; the wallpaper
  still renders.
- The transcript overwrites `last-run.log` on every run, so it does not
  grow. It lists the path of every image written.
- Mind the file size on session hosts and in roaming profiles. On a test
  machine a 3840 x 2160 image over a photographic `Corporate.jpg` was
  about 18.5 MB as PNG and 4 MB as JPEG; over the plain theme backdrop it
  was about 0.5 MB as PNG. With a photo, consider
  `wallpaper-$($monitor.Index).jpg`.

Test it without touching the desktop, in the account it will run as:

```powershell
$script = 'C:\Program Files\PowerBGInfo\Set-BGInfoWallpaper.ps1'
& $script -Target File -OutputDirectory "$env:TEMP\PowerBGInfo-test"
Get-ChildItem "$env:TEMP\PowerBGInfo-test"
```

## Keeping sign-in fast

- Prefer a scheduled task. It runs as its own process after sign-in and
  does not hold up the desktop. Group Policy logon scripts also run
  asynchronously unless "Run logon scripts synchronously" is enabled, but
  Windows delays them by five minutes by default (see
  [GroupPolicy.md](GroupPolicy.md)).
- Keep `Invoke-BGInfoDataSource -TimeoutSeconds` small (5 to 15 seconds).
  A timed-out source is stopped in the background and the call returns at
  the limit with the fallback.
- `Get-BGInfoSystemInfo` limits each of its WMI queries (updates, Defender,
  BitLocker, disks, console user) to five seconds. If even that is too slow
  for you, collect it in your own function inside `Invoke-BGInfoDataSource`.
- Give the task an execution time limit (see
  [TaskScheduler.md](TaskScheduler.md)) so a stuck run cannot pile up.

## Further reading

- [IDesktopWallpaper interface](https://learn.microsoft.com/windows/win32/api/shobjidl_core/nn-shobjidl_core-idesktopwallpaper)
- [about_PSModulePath](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_psmodulepath)
- [Install-Module](https://learn.microsoft.com/powershell/module/powershellget/install-module)
- [Save-Module](https://learn.microsoft.com/powershell/module/powershellget/save-module)
- [ADMX_ControlPanelDisplay: Force a specific default lock screen and logon image](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-controlpaneldisplay)
- [Win32_ComputerSystem class](https://learn.microsoft.com/windows/win32/cimwin32prov/win32-computersystem)
