---
title: "Deploy PowerBGInfo with Microsoft Intune"
description: "Package PowerBGInfo as a Win32 app that installs the module and registers the refresh task, and use platform scripts, Remediations and the Personalization CSP around it."
layout: docs
---

Intune has no setting that runs a script at every sign-in and unlock, so
the dependable pattern is a Win32 app: it installs the module and the
script, and registers the scheduled task from
[Task Scheduler](../task-scheduler/). Platform scripts, Remediations and
the Personalization CSP cover the cases around it.

## When to use this route

- Microsoft Entra joined or hybrid joined Windows devices enrolled in
  Intune.
- Azure Virtual Desktop multi-session and Windows 365 Cloud PCs managed by
  Intune (see [Remote Desktop](../remote-desktop/)).

## Prerequisites

- Devices that meet the Intune management extension (IME) prerequisites:
  a supported Windows version, not Windows Home and not S mode, joined to
  Microsoft Entra ID and enrolled. Microsoft lists IME version 1.58.103.0
  or later. The IME installs itself when you assign a Win32 app, a platform
  script or a Remediation.
- Win32 apps: Windows Enterprise, Pro or Education.
- Remediations: users licensed for Windows Enterprise E3 or E5 (included
  in Microsoft 365 F3, E3 or E5), Windows Education A3 or A5 (included in
  Microsoft 365 A3 or A5), or Windows Virtual Desktop Access per user.
- Personalization CSP: Windows Enterprise or Education (see below).
- The [Microsoft Win32 Content Prep Tool](https://go.microsoft.com/fwlink/?linkid=2065730)
  (`IntuneWinAppUtil.exe`).

## Which host runs what

| Intune feature | Runs as | PowerShell | Bitness |
| --- | --- | --- | --- |
| Win32 app install command | SYSTEM (install behavior **System**) | `powershell.exe` = Windows PowerShell | 32-bit unless you call `Sysnative` |
| Platform script | User or SYSTEM | Windows PowerShell | 32-bit unless **Run script in 64-bit PowerShell host: Yes** |
| Remediation | User or SYSTEM | Windows PowerShell | 32-bit unless **Run script in 64-bit PowerShell: Yes** |

A 32-bit PowerShell looks for modules under `C:\Program Files (x86)`, so it
does not find a module installed under `C:\Program Files`. Always use the
64-bit host.

## Step 1: build the Win32 app

Lay out a source folder:

```text
C:\Packages\PowerBGInfo\
    Install.ps1
    Uninstall.ps1
    Register-BGInfoTask.ps1            the Task Scheduler guide, step 1
    Modules\PowerBGInfo\<version>\...  Save-Module of the tested version
    Scripts\Set-BGInfoWallpaper.ps1    the reference script
    Scripts\Corporate.jpg              optional base image
```

`Install.ps1`:

```powershell
# Install.ps1: runs as SYSTEM from the Win32 app, in 64-bit PowerShell.
$ErrorActionPreference = 'Stop'
# The one version saved under .\Modules\PowerBGInfo.
$saved = Join-Path $PSScriptRoot 'Modules\PowerBGInfo'
$version = (Get-ChildItem $saved -Directory | Select-Object -First 1).Name

$modules = Join-Path $env:ProgramFiles 'WindowsPowerShell\Modules'
$moduleTarget = Join-Path $modules 'PowerBGInfo'
New-Item -ItemType Directory -Path $moduleTarget -Force | Out-Null
$moduleSource = Join-Path $saved $version
Copy-Item -Path $moduleSource -Destination $moduleTarget -Recurse -Force

$scriptTarget = Join-Path $env:ProgramFiles 'PowerBGInfo'
New-Item -ItemType Directory -Path $scriptTarget -Force | Out-Null
Copy-Item -Path (Join-Path $PSScriptRoot 'Scripts\*') `
    -Destination $scriptTarget -Force

& (Join-Path $PSScriptRoot 'Register-BGInfoTask.ps1')
exit 0
```

`Uninstall.ps1`:

```powershell
# Uninstall.ps1: removes the task, the script folder and this version.
$saved = Join-Path $PSScriptRoot 'Modules\PowerBGInfo'
$version = (Get-ChildItem $saved -Directory | Select-Object -First 1).Name
Get-ScheduledTask -TaskPath '\PowerBGInfo\' -ErrorAction SilentlyContinue |
    Unregister-ScheduledTask -Confirm:$false
$scriptFolder = Join-Path $env:ProgramFiles 'PowerBGInfo'
Remove-Item -LiteralPath $scriptFolder -Recurse -Force `
    -ErrorAction SilentlyContinue
$moduleFolder = Join-Path $env:ProgramFiles `
    "WindowsPowerShell\Modules\PowerBGInfo\$version"
Remove-Item -LiteralPath $moduleFolder -Recurse -Force `
    -ErrorAction SilentlyContinue
exit 0
```

Rendered images and logs stay in each user's
`%LOCALAPPDATA%\PowerBGInfo`; the uninstaller runs as SYSTEM and does not
touch user profiles.

Package it. Keep the prep tool outside the source folder; the tool zips
everything in it.

```powershell
IntuneWinAppUtil.exe -c 'C:\Packages\PowerBGInfo' `
    -s 'C:\Packages\PowerBGInfo\Install.ps1' -o 'C:\Packages\Output' -q
```

## Step 2: add the app in Intune

**Apps > All Apps > Create > Windows app (Win32)**, then select the
`.intunewin` file.

- **Install command** (one line):

  ```text
  %SystemRoot%\Sysnative\WindowsPowerShell\v1.0\powershell.exe
  -NoProfile -ExecutionPolicy Bypass -File .\Install.ps1
  ```

  Microsoft documents that calling `powershell.exe` here starts a 32-bit
  PowerShell, and that the `Sysnative` path starts the 64-bit one.
- **Uninstall command** (one line). Intune does not expand environment
  variables in the uninstall command, so write the path out:

  ```text
  C:\Windows\Sysnative\WindowsPowerShell\v1.0\powershell.exe
  -NoProfile -ExecutionPolicy Bypass -File .\Uninstall.ps1
  ```

- **Install behavior**: **System**.
- **Detection rules**: manually configure, rule type **File**:
  - Path: `C:\Program Files\WindowsPowerShell\Modules\PowerBGInfo`
  - File or folder: the version folder you packaged (the name under
    `Modules\PowerBGInfo`)
  - Detection method: **File or folder exists**
  - Associated with a 32-bit app on 64-bit clients: **No**

  Add a second **File** rule for
  `C:\Program Files\PowerBGInfo\Set-BGInfoWallpaper.ps1`. All rules must
  match for Intune to report the app as installed.
- **Assignments**: **Required** to device groups.

When you ship a new module version or script, create a new app with the
new version folder and a matching detection rule, and use
**Supersedence** to replace the old one. `Install.ps1` picks up whichever
version folder the package holds.

## Alternative: platform scripts

**Devices > Scripts and remediations > Platform scripts > Add > Windows 10
and later.** Microsoft documents that a platform script:

- runs once, and again only when the script or its assignment changes;
  failures are retried three times on the next three IME check-ins;
- runs as the signed-in user with **Run this script using the logged on
  credentials: Yes** (the default), or as SYSTEM with **No**;
- checks the signature by default (**Enforce script signature check:
  Yes**);
- runs in 32-bit PowerShell unless **Run script in 64-bit PowerShell host:
  Yes**;
- times out after 30 minutes and must be smaller than 200 KB.

Because it does not repeat, a platform script cannot keep a wallpaper
current. It fits one-off jobs: running `Register-BGInfoTask.ps1` as
SYSTEM on devices that already have the module, or a single render on a
kiosk. Device-assigned scripts run for every new user who signs in, except
on multi-session editions.

## Alternative: Remediations

Remediations run on a schedule: **Once**, **Hourly** (every *n* hours,
less than 24) or **Daily**. A remediation script runs only when the
detection script exits with `1`. Use them when you cannot register a
scheduled task, and only with the licences listed above.

Detection script, run as the user:

```powershell
# Detect: ask for a render when the last one is missing or over an hour old.
$log = Join-Path $env:LOCALAPPDATA 'PowerBGInfo\last-run.log'
$item = Get-Item -LiteralPath $log -ErrorAction SilentlyContinue
if (-not $item -or $item.LastWriteTime -lt (Get-Date).AddHours(-1)) {
    Write-Output 'Wallpaper is stale'
    exit 1
}
Write-Output 'Wallpaper is current'
exit 0
```

Remediation script:

```powershell
# Remediate: render and apply the wallpaper for the signed-in user.
$script = Join-Path $env:ProgramFiles 'PowerBGInfo\Set-BGInfoWallpaper.ps1'
& $script
exit $LASTEXITCODE
```

Settings:

- **Run this script using the logged-on credentials: Yes**. The wallpaper
  belongs to the user.
- **Run script in 64-bit PowerShell: Yes**, so the module path resolves.
  Microsoft's general recommendation is **No**; it does not apply here.
- **Enforce script signature check**: when **Yes**, the scripts run under
  the device's execution policy and must be UTF-8 without a byte order
  mark. When **No**, they run with `Bypass`.
- Output is capped at 2,048 characters.

Not verified for this guide: whether a Remediation in user context can
change the wallpaper on every Windows build, and what it does when nobody
is signed in. Test it on a pilot group before relying on it.

## The Personalization CSP: enforced images

The Personalization CSP sets a desktop image and a lock screen image for
the device and stops users changing them. In the settings catalog, search
for the **Personalization** category; its desktop and lock screen image
URL settings map to the CSP nodes below.

| Node | Value |
| --- | --- |
| `./Vendor/MSFT/Personalization/DesktopImageUrl` | `http` or `https` URL of a `.jpg`, `.jpeg` or `.png`, or a file URL of a local image |
| `./Vendor/MSFT/Personalization/LockScreenImageUrl` | Same |
| `DesktopImageStatus`, `LockScreenImageStatus` | `1` copied, `2` in progress, `3` failed, `4` unknown file type, `5` unsupported URL scheme, `6` max retry failed |

Microsoft documents that the Personalization CSP is supported on Windows
**Enterprise and Education**, and works on Pro only when `SetEduPolicies`
in the SharedPC CSP is set or the device is in Shared PC mode with the
`BootToCloudPCEnhanced` policy. The per-node tables on the same page list
Pro as well; the edition note is the one that applies.

To show a PowerBGInfo image this way, render it on the device as SYSTEM
with `Set-BGInfoMachineImage.ps1` from
[Task Scheduler](../task-scheduler/) and point the URL at the file, for
example `file:///C:/ProgramData/PowerBGInfo/LockScreen.jpg`. Microsoft does
not show the exact file URL format, and the status values say the CSP
downloads or copies the image; it does not document whether it copies the
file again after PowerBGInfo rewrites it. Treat the CSP as a way to deliver
an image that rarely changes, and test refresh behaviour before you
depend on it. The image carries machine facts only, because it is the same
for every user.

Do not combine the CSP with the per-user task: the CSP wins and users see
its image.

## Settings catalog equivalents of the Group Policy settings

These Administrative Template settings from [Group Policy](../group-policy/)
are ADMX-backed policies that the settings catalog exposes:

| Group Policy setting | Policy CSP | Scope |
| --- | --- | --- |
| Force a specific default lock screen and logon image | `ADMX_ControlPanelDisplay/CPL_Personalization_ForceDefaultLockScreen` | Device |
| Prevent changing lock screen and logon image | `ADMX_ControlPanelDisplay/CPL_Personalization_NoChangingLockScreen` | Device |
| Desktop Wallpaper | `ADMX_Desktop/Wallpaper` | User |
| Prevent changing desktop background | `ADMX_ControlPanelDisplay/CPL_Personalization_NoDesktopBackgroundUI` | User |
| Configure Logon Script Delay | `ADMX_GroupPolicy/LogonScriptDelay` | Device |

The edition limits are the same as with Group Policy: the forced lock
screen image applies only to Enterprise, Education and Server. For the
PowerShell execution policy, search the settings catalog for **Turn on
Script Execution**.

## Verify

On a device:

```powershell
$logs = 'C:\ProgramData\Microsoft\IntuneManagementExtension\Logs'
Get-ChildItem $logs -Filter *.log |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 8 Name, LastWriteTime
Get-ScheduledTask -TaskPath '\PowerBGInfo\'
Get-Module -Name PowerBGInfo -ListAvailable |
    Select-Object Version, ModuleBase
```

| Log | What it shows |
| --- | --- |
| `IntuneManagementExtension.log` | Check-ins and policy processing |
| `AppWorkload.log` | Win32 app download, install and detection |
| `AgentExecutor.log` | Platform script runs |
| `HealthScripts.log` | Remediation runs |

To trigger a check-in, choose **Sync** in Company Portal settings, or
restart the **IntuneManagementExtension** service. Without a trigger the
IME checks in every eight hours.

## Pitfalls

- **32-bit PowerShell**: the most common reason for "PowerBGInfo is not
  recognized". Use `Sysnative` and the 64-bit options.
- **User-context Win32 apps**: they fail if the step needs admin rights,
  and on Azure Virtual Desktop multi-session only system-context apps
  assigned to devices are supported. Install as **System**.
- **Signature check on platform scripts**: it is on by default. Turn it
  off or sign the script.
- **Personalization CSP on Pro**: ignored outside the Shared PC cases
  above.

## Further reading

- [Intune Management Extension](https://learn.microsoft.com/intune/device-management/tools/management-extension-windows)
- [Prepare a Win32 app](https://learn.microsoft.com/intune/app-management/deployment/create-win32-package)
- [Add and assign Win32 apps](https://learn.microsoft.com/intune/app-management/deployment/add-win32)
- [PowerShell scripts on Windows devices](https://learn.microsoft.com/intune/device-management/tools/run-powershell-scripts-windows)
- [Remediations](https://learn.microsoft.com/intune/device-management/tools/deploy-remediations)
- [Personalization CSP](https://learn.microsoft.com/windows/client-management/mdm/personalization-csp)
- [ADMX_ControlPanelDisplay](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-controlpaneldisplay)
- [ADMX_Desktop](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-desktop)
- [Azure Virtual Desktop multi-session with Intune](https://learn.microsoft.com/intune/solutions/azure-virtual-desktop-multi-session)
