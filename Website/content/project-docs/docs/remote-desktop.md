---
title: "PowerBGInfo on Remote Desktop, Azure Virtual Desktop and Windows 365"
description: "Run PowerBGInfo in every session on Remote Desktop Services and Azure Virtual Desktop session hosts, and on personal desktops and Windows 365 Cloud PCs."
layout: docs
---

On a session host many users share one Windows installation, each in their
own session. The wallpaper is per user, so PowerBGInfo has to run in every
session.

## When to use this guide

| Platform | Treat it as |
| --- | --- |
| Remote Desktop Session Host (Windows Server) | Multi-session: this guide |
| Azure Virtual Desktop, Windows Enterprise multi-session | Multi-session: this guide |
| Azure Virtual Desktop personal host pools | A physical PC, plus the remote session notes below |
| Windows 365 Cloud PCs | A physical PC running Windows Enterprise, plus the remote session notes below |

## Prerequisites

- The module and `Set-BGInfoWallpaper.ps1` installed on every host (the
  image or the Win32 app from [Microsoft Intune](../intune/)).
- The per-user task from [Task Scheduler](../task-scheduler/), with two
  extra triggers (below).

## Step 1: run in every session

Register the task with the `BUILTIN\Users` group principal and add the
remote connect state change, so a reconnect from a different client, at a
different resolution, re-renders:

```powershell
# Add to Register-BGInfoTask.ps1 on session hosts.
$stateClass = Get-CimClass -ClassName MSFT_TaskSessionStateChangeTrigger `
    -Namespace 'Root/Microsoft/Windows/TaskScheduler'
$reconnect = New-CimInstance -CimClass $stateClass -ClientOnly -Property @{
    StateChange = 3   # TASK_REMOTE_CONNECT
    Enabled     = $true
}
# ...then pass it with the other triggers:
# -Trigger $logon, $unlock, $reconnect
```

On multi-session hosts consider leaving out the repeating timer. Microsoft
does not document which sessions a group task with a time trigger runs in
when many members are signed in, and every run costs CPU on a shared host.
Logon, unlock and reconnect cover what users see.

## Step 2: keep every user's files apart

The reference script writes to `%LOCALAPPDATA%\PowerBGInfo`, so sessions
never share an image, a chart history or a log. Do not point
`-ConfigurationDirectory` at a shared folder on a session host.

With FSLogix profile containers, nearly everything under
`C:\Users\<user>` lives in the container, including
`AppData\Local\PowerBGInfo`, so chart history follows the user from host
to host. Check your FSLogix redirections if you exclude `AppData\Local`.

## What is different in a remote session

- **Monitor size.** PowerBGInfo sizes the image from DesktopManager's
  `GetMonitorSnapshots()`: physical geometry and effective per-monitor DPI,
  independent of the PowerShell host's DPI awareness. A monitor remains in
  the snapshots when its wallpaper device path is missing; unavailable DPI
  leaves `Scale` empty. Some session hosts report a valid wallpaper monitor
  slot without a device path; DesktopManager then sets the wallpaper with
  `SystemParametersInfo` for the whole session (the fix for issue #71).
  In a multi-monitor session that means
  the last image the script writes covers every monitor. If no monitor can
  be read, the image is 1920 x 1080.
- **Dynamic resolution.** The `dynamic resolution` RDP property defaults to
  `1`: the session resolution follows the client window. The image is
  rendered for the size at the time of the run; `-WallpaperFit Fill` keeps
  it covering the screen afterwards.
- **Multiple monitors.** With `use multimon:i:1` (the default) the session
  has one monitor per client display. The reference script renders one
  image per monitor. Re-query after reconnecting or a display change;
  monitor indexes are not permanent display identities. Windows wallpaper
  fit is shared by all monitors even when their images differ.
- **Wallpaper removed by policy.** **Computer Configuration > Administrative
  Templates > Windows Components > Remote Desktop Services > Remote Desktop
  Session Host > Remote Session Environment > Enforce Removal of Remote
  Desktop Wallpaper**, when enabled, means wallpaper never appears in a
  remote session. When disabled, the client decides (the **Experience**
  options in Remote Desktop Connection).
- **The Desktop Wallpaper policy does not apply.** Microsoft documents that
  the user policy does not apply to Remote Desktop server sessions.
- **The user name.** `UserName`, `FullUserName` and
  `Get-BGInfoSystemInfo.UserName` are the session's user, because the task
  runs as that user. `LoggedOnUser` is the user of the session the task
  runs in too: PowerBGInfo delegates to DesktopManager's
  `DesktopSessionService.GetSessionUser()`, which asks the Remote Desktop
  Services session API (`WTSQuerySessionInformation`) for the process's own
  session without elevation. Only a process in session 0, such as a SYSTEM
  task, selects the console session's user instead. PowerBGInfo retains
  `Win32_ComputerSystem.UserName` as its last resort when session information
  is unavailable, then `Unknown` if neither source supplies a user. On a
  session host the console is usually empty or belongs to an administrator;
  these fallbacks cannot select the intended remote user for a SYSTEM task.
- **Slideshows.** PowerBGInfo skips slideshow handling when DesktopManager
  reports the slideshow as disabled by the remote session.

## Azure Virtual Desktop multi-session with Intune

Microsoft documents these limits for Windows Enterprise multi-session:

- Win32 apps must install in the **System** context, be assigned to
  **devices** and use the **Required** or **Uninstall** intent.
- Platform scripts in the user context assigned to users, and in the system
  context assigned to devices, are supported. Scripts assigned to devices do
  not run for every new user on multi-session editions.
- Configuration profiles come from the settings catalog; most templates do
  not apply.

So deploy the Win32 app from [Microsoft Intune](../intune/) to the host pool's
device group; the scheduled task then runs in every session.

## Shared images and -AllUsers

`-AllUsers` writes the wallpaper path into profiles listed under
`ProfileList` and into the default profile. On session hosts with FSLogix
or other profile containers most user profiles are not there between
sessions, and the image would be the same for everyone. Use the per-session
task instead.

## Verify

In a test user's session:

```powershell
Get-Content (Join-Path $env:LOCALAPPDATA 'PowerBGInfo\last-run.log')
Get-ChildItem (Join-Path $env:LOCALAPPDATA 'PowerBGInfo') -Filter *.png
```

Sign in from two clients with different resolutions and check that each
session gets an image of its own size.

## Pitfalls

- **One output folder for all sessions**: users overwrite each other's
  images, and files created by one user usually cannot be replaced by
  another.
- **A SYSTEM task on the host**: it changes the wallpaper of session 0,
  which nobody sees.
- **Logon storms**: keep `-TimeoutSeconds` in the data sources short and
  avoid slow network sources at sign-in on busy hosts.

## Further reading

- [Supported RDP properties](https://learn.microsoft.com/azure/virtual-desktop/rdp-properties)
- [ADMX_TerminalServer: Enforce Removal of Remote Desktop Wallpaper](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-terminalserver)
- [ADMX_Desktop: Desktop Wallpaper](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-desktop)
- [Azure Virtual Desktop multi-session with Intune](https://learn.microsoft.com/intune/solutions/azure-virtual-desktop-multi-session)
- [FSLogix container types](https://learn.microsoft.com/fslogix/concepts-container-types)
- [Windows 365 device images](https://learn.microsoft.com/windows-365/enterprise/device-images)
- [Win32_ComputerSystem class](https://learn.microsoft.com/windows/win32/cimwin32prov/win32-computersystem)
