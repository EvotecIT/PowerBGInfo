---
title: "Run PowerBGInfo in production"
description: "What one PowerBGInfo run does, which deployment route fits which environment, and the reference script the deployment guides build on."
layout: docs
---

This guide explains what PowerBGInfo does when it runs, which deployment
route fits which environment, and the one script the other guides build on.

| Guide | Covers |
| --- | --- |
| [Run in production](../deployment/) (this page) | How the module behaves, choosing a route, installing it, the reference script |
| [Task Scheduler](../task-scheduler/) | Logon, unlock and timed refresh with `Register-ScheduledTask` |
| [Group Policy](../group-policy/) | Logon scripts, Group Policy Preferences tasks, wallpaper and lock screen policies |
| [Microsoft Intune](../intune/) | Win32 app packaging, platform scripts, Remediations, Personalization CSP |
| [Configuration Manager](../configuration-manager/) | Applications, packages and Run Scripts |
| [Remote Desktop and Azure Virtual Desktop](../remote-desktop/) | Remote Desktop Services, Azure Virtual Desktop, Windows 365 |
| [Security and operations](../security/) | Rights, signing, App Control, logging |
| [Troubleshooting](../troubleshooting/) | Frequently asked questions and where to look |

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
| `LogonScreen` | Attempts the running account's lock screen, with a machine-policy fallback if that fails (see below) | Only for the policy fallback |
| `Both` | `Wallpaper` and `LogonScreen` | Only for the lock screen policy fallback |
| `Wallpaper` with `-AllUsers` | Also writes the wallpaper path into every local profile | Yes |

Author and test every layout with `-Target File` first: it writes an
image you can open without changing anything on the machine. Switch to
the target you need only when the file looks right at the size of the
screens you deploy to.

The details below come from the module source and from the DesktopManager
library that ships inside the module and talks to Windows.

### Setting the wallpaper

- The wallpaper is set per monitor through the Windows shell
  `IDesktopWallpaper` interface: `SetWallpaper` with the monitor's device
  path. `-MonitorIndex` (default `0`) picks the monitor. One run renders and
  applies one image for one monitor; `Get-BGInfoMonitor` lists the indexes.
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
  restarts the slideshow with the rendered copies, keeping its options and
  interval. `-DisableWallpaperSlideshow` renders one static image instead.
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
- Monitor sizes come from DesktopManager's `GetMonitorSnapshots()` in
  physical pixels. Its per-monitor DPI and scale query is independent of
  the PowerShell host's DPI awareness; it temporarily changes only the
  calling thread's context and restores it afterwards. Missing DPI leaves
  `Scale` empty without discarding the monitor's geometry.
  Snapshot queries require Windows 10 version 1607 / Windows Server 2016 or
  later; unavailable monitor information uses the rendering fallback above.
- PowerBGInfo never draws on its own earlier output. It records every image
  it writes, with the base it was drawn on, in `PowerBgInfo.outputs.txt` next
  to the image. When the current wallpaper or a slideshow image is one of
  those outputs, the run draws on the recorded original again, so repeated
  runs do not stack the text and default file names do not grow. An image
  that has the name of a PowerBGInfo output but no record is not used: the
  run paints the background colour and writes a warning.
- `-FilePath` and `New-BGInfoBackground -ImagePath` are used as given, even
  when PowerBGInfo rendered them, unless they are the file the run is about
  to overwrite; then the recorded original is used.
- Detection relies on the path Windows reports for the wallpaper. Something
  that copies the image elsewhere first, such as a theme or a policy that
  caches the file, hides the record. For production runs, still give a fixed
  base: `New-BGInfoBackground` (optionally with `-ImagePath` for a corporate
  image) or `-FilePath`. The result is then the same on every machine.

### Files it writes

| Path | Written by | Purpose |
| --- | --- | --- |
| `-ConfigurationDirectory` | `New-BGInfo` | Created if missing. Required parameter. |
| `<ConfigurationDirectory>\<OutputFileName>` | `New-BGInfo` | The image. Without `-OutputFileName`: `<wallpaper name>_PowerBgInfo.<ext>` or `PowerBgInfo.png`. |
| `PowerBgInfo.outputs.txt` next to the image | `New-BGInfo` | One line per image: its file name and the base it was drawn on. Read on the next run; deleting it only disables the detection above. |
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

Both Windows PowerShell 5.1 and PowerShell 7 first attempt the running
account's WinRT lock screen API. PowerBGInfo uses DesktopManager's
`SetLogonWallpaperWithResult()` and inspects the completed operation's
result; it does not infer API availability from the host or a managed-type
probe.

| Attempt | Mechanism | Whose lock screen | Requirements |
| --- | --- | --- | --- |
| First, in either host | WinRT `Windows.System.UserProfile.LockScreen.SetImageStreamAsync` | The account that runs the command | No elevation; Windows configuration, policy and the account's profile must allow it |
| If WinRT fails | The policy value `HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization\LockScreenImage` (`PersonalizationPolicy`) | The device | Elevation; Enterprise, Education or Server, including relevant IoT Enterprise editions |

- An elevated session started by another administrator account still
  targets that administrator's per-user lock screen. It does not select the
  user whose desktop is visible. A SYSTEM task is not a substitute for a
  per-user lock screen task.
- PowerBGInfo opts into the policy fallback. Only that fallback needs
  elevation; a denied write is reported as a failure. When the result shows
  `PersonalizationPolicy`, PowerBGInfo checks the edition and warns if the
  policy is unsupported or the edition cannot be read. Pro and Home do not
  gain policy support merely because an administrator writes the value.
- A failed result is exposed to the caller. A completed WinRT action or a
  successful policy write still does not prove that Windows visibly changed
  the lock screen; policy and Windows configuration can prevent it.

For a managed fleet, the documented route is more predictable: render the
image with `-Target File` and deliver it with the policy or the
Personalization CSP. See [Group Policy](../group-policy/) and
[Microsoft Intune](../intune/).

### Who "the user" is

When the script runs as SYSTEM, `UserName` and `FullUserName` describe
SYSTEM, and `-Target Wallpaper` changes SYSTEM's own wallpaper, which nobody
sees. Per-user wallpapers have to run in the user's own session.

The `LoggedOnUser` token and `Get-BGInfoSystemInfo.LoggedOnUser` return the
user signed in to the Windows session the script runs in, as
`DOMAIN\user` (or just the user name if no domain is available).
PowerBGInfo delegates the session lookup to DesktopManager's read-only
`DesktopSessionService.GetSessionUser()`, which uses
`WTSQuerySessionInformation` and needs no elevation for the current session.
A task in the user's session, including a remote session on a session host,
gets that user. A SYSTEM task runs in session 0, which has no user; there it
reports the user of the active console session. If session information is unavailable,
PowerBGInfo retains `Win32_ComputerSystem.UserName` as its last-resort console
lookup, and returns `Unknown` when neither source supplies a user. That
fallback cannot identify the intended remote user for a SYSTEM task.

## Choosing a route

| Environment | Recommended route | Runs as | Guide |
| --- | --- | --- | --- |
| Domain-joined PCs | Group Policy Preferences scheduled task: logon and unlock | Each signed-in user | [Group Policy](../group-policy/) |
| Intune-managed PCs | Win32 app that installs the module and registers the task | Installer: SYSTEM. Task: each user | [Microsoft Intune](../intune/) |
| Configuration Manager | Application or package that installs the module and registers the task | Installer: SYSTEM. Task: each user | [Configuration Manager](../configuration-manager/) |
| Standalone or a few machines | `Register-ScheduledTask` | Each signed-in user | [Task Scheduler](../task-scheduler/) |
| RDS session hosts, AVD multi-session | Per-session task with a per-user output folder | Each session's user | [Remote Desktop](../remote-desktop/) |
| AVD personal desktops, Windows 365 | Same as a physical PC | Each signed-in user | [Remote Desktop](../remote-desktop/) |
| Shared or kiosk PCs | Logon task; machine image with `-AllUsers` for the default profile | User, or SYSTEM for staging | [Task Scheduler](../task-scheduler/) |
| Lock screen, Enterprise or Education | SYSTEM task renders `-Target File`; policy or CSP shows it | SYSTEM | [Group Policy](../group-policy/), [Microsoft Intune](../intune/) |
| Servers with Server Core | Not applicable: no desktop to show a wallpaper | | [Troubleshooting](../troubleshooting/) |

Whatever the route, the same three pieces repeat:

1. Install the module for all users (below).
2. Put the wallpaper script where standard users cannot change it, for
   example `C:\Program Files\PowerBGInfo\Set-BGInfoWallpaper.ps1`.
3. Run it in each user's session at sign-in and unlock, optionally on a
   timer.

## Getting the module onto machines

PowerBGInfo supports Windows PowerShell 5.1 and PowerShell 7 (the manifest
lists both editions and ships .NET Framework 4.7.2 and .NET 8 binaries).
The rendering and desktop libraries are inside the module; nothing else
needs installing. Pick one host per route and use the module path of that
host:

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

Every release is Authenticode-signed by Evotec. Where the execution policy
is `AllSigned`, trust the publisher once and sign your own scripts; see
[Security and operations](../security/#script-signing-and-allsigned).

Online, from an elevated session, pinned to the version you tested:

```powershell
$version = '<version you tested>'
Install-Module -Name PowerBGInfo -RequiredVersion $version `
    -Scope AllUsers -Repository PSGallery
```

Offline, save once on a connected machine and copy the folder:

```powershell
$version = '<version you tested>'
Save-Module -Name PowerBGInfo -RequiredVersion $version `
    -Path 'C:\Staging\Modules' -Repository PSGallery

# On the target, elevated. Keep the <version> folder level.
$target = Join-Path $env:ProgramFiles 'WindowsPowerShell\Modules'
Copy-Item -Path 'C:\Staging\Modules\PowerBGInfo' -Destination $target `
    -Recurse -Force
```

`Save-Module` creates `PowerBGInfo\<version>\`. Keeping that layout lets
several versions sit side by side; the script can then load one exactly
with `Import-Module -Name PowerBGInfo -RequiredVersion <version>`.

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
the scheduler records it. It uses no .NET types, so it also runs in
Constrained Language Mode once the module itself is allowed (see
[Security and operations](../security/#constrained-language-mode-applocker-and-app-control)).

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
        @{ Owner = 'Finance IT'; Support = 'ext. 4357' }
    }
    $fallback = @{ Owner = 'Owner unavailable'; Support = 'ext. 4357' }
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
    $monitors = @(Get-BGInfoMonitor)
    if ($monitors.Count -eq 0) {
        # No monitor reported: render once at the 1920 x 1080 fallback.
        $monitors = @(@{ Index = 0; Width = 1920; Height = 1080 })
    }
    $background = @{}
    if (Test-Path -LiteralPath $BaseImage) {
        $background.ImagePath = $BaseImage
    }

    $sessionId = (Get-Process -Id $PID).SessionId
    foreach ($monitor in $monitors) {
        # 1 at 1080 lines, 1.33 at 1440, 2 at 2160.
        $scale = [int]($monitor.Height / 10.8) / 100
        if ($scale -lt 1) { $scale = 1 }
        $theme = New-BGInfoTheme -Preset Graphite -Mode Dark -Scale $scale
        $render = @{
            Theme                  = $theme
            Target                 = $Target
            MonitorIndex           = $monitor.Index
            WallpaperFit           = 'Fill'
            ConfigurationDirectory = $OutputDirectory
            OutputFileName         = "wallpaper-$sessionId-$($monitor.Index).png"
        }
        New-BGInfo @render {
            New-BGInfoBackground @background
            New-BGInfoLabel -Name $data.Owner -Accent -FontWeight 700
            New-BGInfoLabel -Name $facts.ComputerName `
                -FontSize ([int](48 * $scale)) -FontWeight 600
            New-BGInfoValue -Name 'User' -Value $facts.LoggedOnUser
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

- `Get-BGInfoMonitor` returns each monitor's `Index` (the value
  `-MonitorIndex` takes), `DeviceName`, `Primary`, bounds (`Left`, `Top`,
  `Width`, `Height`) in physical pixels and the Windows display `Scale`
  from DesktopManager's monitor snapshots. `Scale` is empty when DPI cannot
  be read. Re-query after docking or reconnecting; indexes are not permanent
  display identities. It returns nothing when no monitor can be read; the
  script then renders once at the fallback size.
- Each monitor gets its own `-OutputFileName`. Two monitors that share one
  file both show whichever image was written last.
- Charts keep history per `-Id`. If you add `New-BGInfoChart` to a
  per-monitor loop, give each monitor its own `-Id` or use `-NoHistory`;
  otherwise every run appends one sample per monitor to the same history.
- `Invoke-BGInfoDataSource` runs the function in its own runspace, so it
  cannot see the script's variables. Pass what it needs with
  `-ArgumentList`. When the function throws, returns nothing or runs out of
  time, the cmdlet writes a warning and returns `-Default`; the wallpaper
  still renders. The data is a hashtable rather than `[pscustomobject]`,
  which Constrained Language Mode does not allow.
- The transcript overwrites `last-run.log` on every run, so it does not
  grow. It lists the path of every image written and every warning.
- Mind the file size on session hosts and in roaming profiles. On a test
  machine a 3840 x 2160 image over a photographic `Corporate.jpg` was
  about 18.5 MB as PNG and 4 MB as JPEG; over the plain theme backdrop it
  was about 0.5 MB as PNG. With a photo, consider
  `wallpaper-$sessionId-$($monitor.Index).jpg`.

Test it without touching the desktop, in the account it will run as:

```powershell
$script = 'C:\Program Files\PowerBGInfo\Set-BGInfoWallpaper.ps1'
& $script -Target File -OutputDirectory "$env:TEMP\PowerBGInfo-test"
Get-ChildItem "$env:TEMP\PowerBGInfo-test"
```

## Keeping the layout in JSON

When the layout should be reviewed and versioned apart from the deployment
script, export it once and render the file from the task. `-ExportOnly`
writes the JSON without rendering:

```powershell
$folder = 'C:\ProgramData\PowerBGInfo'
$json = Join-Path $folder 'workstation.json'
New-BGInfo -Target File -ConfigurationDirectory $folder `
    -JsonPath $json -ExportOnly {
    New-BGInfoValue -BuiltinValue HostName -Name 'Machine'
    New-BGInfoValue -BuiltinValue LoggedOnUser -Name 'User'
    New-BGInfoValue -Name 'Support' -Value 'helpdesk@contoso.com'
}

# In the task, in the user's session:
Invoke-BGInfo -Path $json -Target Wallpaper `
    -ConfigurationDirectory (Join-Path $env:LOCALAPPDATA 'PowerBGInfo')
```

The JSON keeps built-in value tokens, chart metrics, themes, regions,
widgets, layout and target settings, so values are read again on every
run. It does not keep PowerShell code: a `foreach` or a data function runs
once at export, and its results are stored as fixed values. For data that
changes, keep the reference script.

## Keeping sign-in fast

- Prefer a scheduled task. It runs as its own process after sign-in and
  does not hold up the desktop. Group Policy logon scripts also run
  asynchronously unless "Run logon scripts synchronously" is enabled, but
  Windows delays them by five minutes by default (see
  [Group Policy](../group-policy/)).
- Keep `Invoke-BGInfoDataSource -TimeoutSeconds` small (5 to 15 seconds).
  A timed-out source is stopped in the background and the call returns at
  the limit with the fallback.
- `Get-BGInfoSystemInfo` limits each of its WMI queries (updates, Defender,
  BitLocker, disks, console user) to five seconds. If even that is too slow
  for you, collect it in your own function inside `Invoke-BGInfoDataSource`.
- Give the task an execution time limit (see
  [Task Scheduler](../task-scheduler/)) so a stuck run cannot pile up.

## Further reading

- [IDesktopWallpaper interface](https://learn.microsoft.com/windows/win32/api/shobjidl_core/nn-shobjidl_core-idesktopwallpaper)
- [WTSQuerySessionInformation](https://learn.microsoft.com/windows/win32/api/wtsapi32/nf-wtsapi32-wtsquerysessioninformationw)
- [about_PSModulePath](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_psmodulepath)
- [Install-Module](https://learn.microsoft.com/powershell/module/powershellget/install-module)
- [Save-Module](https://learn.microsoft.com/powershell/module/powershellget/save-module)
- [ADMX_ControlPanelDisplay: Force a specific default lock screen and logon image](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-controlpaneldisplay)
- [Win32_ComputerSystem class](https://learn.microsoft.com/windows/win32/cimwin32prov/win32-computersystem)
