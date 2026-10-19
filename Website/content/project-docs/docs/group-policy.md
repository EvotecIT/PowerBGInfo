---
title: "Deploy PowerBGInfo with Group Policy"
description: "Install PowerBGInfo with a startup script, refresh it with a Group Policy Preferences scheduled task or a logon script, and set the execution policy, wallpaper and lock screen policies."
layout: docs
---

Group Policy can install the module, register the refresh task, set the
PowerShell execution policy and, on Enterprise and Education editions,
force a lock screen image that PowerBGInfo renders.

## When to use this route

- Active Directory domain-joined PCs and session hosts.
- When the same GPO should also carry the execution policy and the lock
  screen policy.

## Prerequisites

- Group Policy Management Console and rights to edit and link a GPO.
- A share that computer accounts can read (for example under `SYSVOL` or
  `NETLOGON`) holding a saved copy of the module (`Save-Module`, see
  [Run in production](../deployment/)) and `Set-BGInfoWallpaper.ps1`.
- For the lock screen policy: Windows Enterprise, Education or Server.

## Step 1: install the module and the script

Use your software distribution tool, or a computer startup script
(**Computer Configuration > Policies > Windows Settings > Scripts
(Startup/Shutdown) > Startup**, PowerShell Scripts tab). Startup scripts run
as the computer account, before anyone signs in.

```powershell
# Startup script: installs the staged PowerBGInfo and the wallpaper script.
$share = '\\contoso.com\NETLOGON\PowerBGInfo'
# The share holds one tested version, saved with Save-Module.
$version = (Get-ChildItem "$share\Module\PowerBGInfo" -Directory |
    Select-Object -First 1).Name
$programFiles = if ($env:ProgramW6432) { $env:ProgramW6432 } else {
    $env:ProgramFiles
}

$moduleTarget = Join-Path $programFiles `
    "WindowsPowerShell\Modules\PowerBGInfo\$version"
$moduleSource = Join-Path $share "Module\PowerBGInfo\$version"
$moduleFiles = @(Get-ChildItem -LiteralPath $moduleSource -File -Recurse -ErrorAction Stop)
if (-not $moduleFiles.Count) { throw "The module source is empty: $moduleSource" }

$copyModule = -not (Test-Path -LiteralPath (Join-Path $moduleTarget 'PowerBGInfo.psd1') -PathType Leaf)
foreach ($moduleFile in $moduleFiles) {
    $relativePath = $moduleFile.FullName.Substring($moduleSource.Length).TrimStart('\', '/')
    $targetFile = Join-Path $moduleTarget $relativePath
    if (-not (Test-Path -LiteralPath $targetFile -PathType Leaf)) {
        $copyModule = $true
        break
    }
    $sourceHash = (Get-FileHash -LiteralPath $moduleFile.FullName -Algorithm SHA256).Hash
    $targetHash = (Get-FileHash -LiteralPath $targetFile -Algorithm SHA256).Hash
    if ($sourceHash -ne $targetHash) {
        $copyModule = $true
        break
    }
}

if ($copyModule) {
    New-Item -ItemType Directory -Path $moduleTarget -Force | Out-Null
    Copy-Item -Path (Join-Path $moduleSource '*') -Destination $moduleTarget `
        -Recurse -Force -ErrorAction Stop
    foreach ($moduleFile in $moduleFiles) {
        $relativePath = $moduleFile.FullName.Substring($moduleSource.Length).TrimStart('\', '/')
        $targetFile = Join-Path $moduleTarget $relativePath
        if (-not (Test-Path -LiteralPath $targetFile -PathType Leaf)) {
            throw "The module copy is incomplete: $targetFile"
        }
        $sourceHash = (Get-FileHash -LiteralPath $moduleFile.FullName -Algorithm SHA256).Hash
        $targetHash = (Get-FileHash -LiteralPath $targetFile -Algorithm SHA256).Hash
        if ($sourceHash -ne $targetHash) {
            throw "The module copy does not match its source: $targetFile"
        }
    }
}

$scriptTarget = Join-Path $programFiles 'PowerBGInfo'
New-Item -ItemType Directory -Path $scriptTarget -Force | Out-Null
Copy-Item -Path "$share\Scripts\*" -Destination $scriptTarget -Force
```

`$env:ProgramW6432` always points at the 64-bit `Program Files`, so the
script writes to the right place whichever PowerShell runs it.

## Step 2, option A (recommended): a Preferences scheduled task

A Group Policy Preferences scheduled task gives you the logon and unlock
triggers from [Task Scheduler](../task-scheduler/) without a registration
script.

1. Edit the GPO linked to the computers. Go to **Computer Configuration >
   Preferences > Control Panel Settings > Scheduled Tasks**.
2. **New > Scheduled Task (At least Windows 7)**.
3. **General** tab:
   - Action: **Replace** (see Common tab below).
   - Name: `PowerBGInfo wallpaper`.
   - When running the task, use: **BUILTIN\Users** (Change User or Group).
   - **Run only when user is logged on**. Do not tick **Run with highest
     privileges**.
4. **Triggers** tab:
   - New: **At log on**, **Any user**, **Delay task for: 30 seconds**.
   - New: **On workstation unlock**, **Any user**.
   - Optional: **Daily**, **Repeat task every: 30 minutes**,
     **for a duration of: Indefinitely**.
5. **Actions** tab: **Start a program**.
   - Program: `%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe`
   - Arguments, entered on one line with spaces between the parts:

     ```text
     -NoProfile -NonInteractive -WindowStyle Hidden
     -ExecutionPolicy Bypass
     -File "C:\Program Files\PowerBGInfo\Set-BGInfoWallpaper.ps1"
     ```
6. **Conditions** tab: clear **Start the task only if the computer is on AC
   power**.
7. **Settings** tab: **If the task is already running: Do not start a new
   instance**; **Stop the task if it runs longer than: 1 hour**.
8. **Common** tab: tick **Remove this item when it is no longer applied**.
   Microsoft documents that this switches the item's action to **Replace**
   and deletes the task when the GPO no longer applies.

The task editor in the Group Policy Management Editor is the Task Scheduler
editor, so the options match [Task Scheduler](../task-scheduler/).

An **Immediate Task (At least Windows 7)** runs once when Group Policy
refreshes and is then removed. That suits a one-off machine render as
SYSTEM, not a recurring per-user wallpaper.

## Step 2, option B: a user logon script

Simpler, but it runs only at sign-in and never on unlock.

1. **User Configuration > Policies > Windows Settings > Scripts
   (Logon/Logoff) > Logon > PowerShell Scripts** tab.
2. Add `Set-BGInfoWallpaper.ps1` (copy it into the GPO folder, or use a
   UNC path users can read).

Things to know:

- The PowerShell Scripts tab runs Windows PowerShell. For PowerShell 7, add
  `pwsh.exe` on the **Scripts** tab with `-File <path>` as parameters.
- Windows waits **five minutes** after sign-in before it runs logon
  scripts, unless **Computer Configuration > Administrative Templates >
  System > Group Policy > Configure Logon Script Delay** is set (`0`
  disables the delay).
- Logon scripts run alongside Explorer unless **Run logon scripts
  synchronously** is enabled. Leave it off.
- Microsoft notes that a PowerShell logon script can start before the
  desktop shell is ready and fail its execution policy zone check with
  "AuthorizationManager check failed". The `Bypass` and `AllSigned`
  policies do not do that check.

## Execution policy

**Computer Configuration > Administrative Templates > Windows Components >
Windows PowerShell > Turn on Script Execution** sets the execution policy
for Windows PowerShell. PowerShell 7 has its own setting under
**Administrative Templates > PowerShell Core** once you install its
templates (`InstallPSCorePolicyDefinitions.ps1` in `$PSHOME`); each
PowerShell 7 setting can defer to the Windows PowerShell one.

| Policy option | Execution policy |
| --- | --- |
| Allow only signed scripts | `AllSigned` |
| Allow local scripts and remote signed scripts | `RemoteSigned` |
| Allow all scripts | `Unrestricted` |
| Disabled | `Restricted` |

A policy set here overrides every other scope, including
`-ExecutionPolicy Bypass` on the task's command line. With **Allow only
signed scripts**, sign `Set-BGInfoWallpaper.ps1` and your other scripts.
PowerBGInfo itself is signed by Evotec; add its publisher certificate to
**Computer Configuration > Policies > Windows Settings > Security Settings >
Public Key Policies > Trusted Publishers** in the same GPO, so tasks never
stop at the untrusted-publisher prompt (see
[Security and operations](../security/#script-signing-and-allsigned)).

## The Desktop Wallpaper policy

**User Configuration > Administrative Templates > Desktop > Desktop >
Desktop Wallpaper** sets one wallpaper file for the user and stops them
changing it. Microsoft documents that:

- The file can be a local path or a UNC path, and a `.bmp` or `.jpg`.
- If the file is not available when the user signs in, no wallpaper is
  shown.
- The setting does not apply to Remote Desktop server sessions.

PowerBGInfo does not check for this policy. Do not combine it with the
per-user task: the two fight over the same setting, and the policy is
applied again at the latest at the next sign-in. If you need an enforced
wallpaper, let PowerBGInfo produce the file and let the policy show it:

1. Run `Set-BGInfoMachineImage.ps1` from
   [Task Scheduler](../task-scheduler/) as SYSTEM with
   `-OutputFileName Wallpaper.jpg`.
2. Point the policy at `C:\ProgramData\PowerBGInfo\Wallpaper.jpg`.

The image then carries machine facts only (no user name), and Windows
shows changes the next time it applies the wallpaper, normally at sign-in.

## The lock screen policy

**Computer Configuration > Administrative Templates > Control Panel >
Personalization > Force a specific default lock screen and logon image**
takes the path of an image, local or UNC, and uses it for the lock screen
and the sign-in screen. Microsoft documents that it **only applies to
Enterprise, Education and Server editions**. Pair it with **Prevent
changing lock screen and logon image** so users keep that image.

1. Register the SYSTEM task from [Task Scheduler](../task-scheduler/)
   (startup and daily) to render `C:\ProgramData\PowerBGInfo\LockScreen.jpg`
   with `-Target File`.
2. Set the policy to `C:\ProgramData\PowerBGInfo\LockScreen.jpg`.

PowerBGInfo's own `-Target LogonScreen` writes the same registry value
(`HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization\LockScreenImage`)
only when the per-user lock screen API fails, in both PowerShell 7 and
Windows PowerShell 5.1 (see
[Run in production](../deployment/#lock-screen)). Do not use both: the GPO
owns that value and overwrites it.

An archived Microsoft support blog (2018) describes how the sign-in screen
caches this image: it reloads the file only when its last-modified time is
newer than the cached copy. PowerBGInfo rewrites the file on every run, so
a newer timestamp is expected; restoring an older copy of the file would
not show.

## Verify

```powershell
gpresult /r /scope computer
Get-ScheduledTask -TaskName 'PowerBGInfo wallpaper' |
    Select-Object TaskPath, State
Get-ExecutionPolicy -List
```

Then sign in as a test user and check
`%LOCALAPPDATA%\PowerBGInfo\last-run.log`.

## Pitfalls

- **The share is not readable by computer accounts.** Startup scripts run
  as the computer; grant `Domain Computers` read access.
- **The logon script appears to do nothing.** It is waiting out the
  five-minute logon script delay.
- **Two wallpaper owners.** The Desktop Wallpaper policy, a Personalization
  CSP from Intune or a theme from another tool all override PowerBGInfo.
- **The lock screen policy on Pro.** It is ignored; see
  [Troubleshooting](../troubleshooting/).

## Further reading

- [Group Policy Preferences](https://learn.microsoft.com/windows-server/identity/ad-ds/manage/group-policy/group-policy-preferences)
- [Group Policy Preferences common options (archived)](https://learn.microsoft.com/previous-versions/windows/it-pro/windows-server-2012-r2-and-2012/dn581922(v=ws.11))
- [Scheduled Tasks extension (archived)](https://learn.microsoft.com/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/cc770904(v=ws.10))
- [ADMX_Scripts policies](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-scripts)
- [ADMX_GroupPolicy: Configure Logon Script Delay](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-grouppolicy)
- [about_Execution_Policies](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_execution_policies)
- [about_Group_Policy_Settings](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_group_policy_settings)
- [ADMX_Desktop: Desktop Wallpaper](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-desktop)
- [ADMX_ControlPanelDisplay: lock screen policies](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-controlpaneldisplay)
- [Changing the image used in the lock screen policy (archived blog)](https://learn.microsoft.com/archive/blogs/jasone/changing-the-image-used-in-the-force-a-specific-default-lock-screen-and-logon-image-policy)
