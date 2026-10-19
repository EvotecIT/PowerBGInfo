---
title: "PowerBGInfo troubleshooting"
description: "Answers to the usual PowerBGInfo questions: reverting, black, stretched or blurry wallpapers, placement, readability, BGInfo migration, Server Core, SYSTEM, the lock screen and multiple monitors."
layout: docs
---

Start with the transcript of the last run
(`%LOCALAPPDATA%\PowerBGInfo\last-run.log` with the reference script) and
the task result (`Get-ScheduledTaskInfo`). The table of log locations is in
[Security and operations](../security/#logging-and-troubleshooting).

Then render to a file as the same account, to separate rendering from
applying:

```powershell
$script = 'C:\Program Files\PowerBGInfo\Set-BGInfoWallpaper.ps1'
& $script -Target File -OutputDirectory "$env:TEMP\PowerBGInfo-test"
```

If the file looks right, the problem is in applying it; if it does not,
the problem is in the data or the layout.

## Why does the wallpaper revert?

Something else owns the wallpaper, or the task ran as a different account.

- **A policy.** The Desktop Wallpaper Group Policy, the Personalization CSP
  (`DesktopImageUrl`) from Intune, or a theme pushed by another tool sets
  the wallpaper again at sign-in. Check with `gpresult /h report.html` and
  the device's Intune configuration. Use one owner; see
  [Group Policy](../group-policy/#the-desktop-wallpaper-policy).
- **The task ran as SYSTEM.** It changed SYSTEM's wallpaper. Run per-user
  wallpapers with the `BUILTIN\Users` group principal.
- **Windows reused a cached wallpaper after sign-in** (reported on Windows
  11 24H2, issue #24). PowerBGInfo applies a temporary copy first and then
  the final file to avoid this. Do not use `-DisableWallpaperRefresh`
  unless you have a reason.
- **A Windows slideshow or another background setting.** If the user picks
  a different background type in Settings, Windows replaces the image until
  the next run.

## Why is the wallpaper black?

- **No base image and no wallpaper file.** Without `-FilePath` or
  `New-BGInfoBackground`, PowerBGInfo uses the current wallpaper. If there
  is none, it paints the desktop background colour, or black when it cannot
  read the colour (for example under SYSTEM). Give the script a fixed base.
- **A policy points at a missing file.** Microsoft documents that the
  Desktop Wallpaper policy shows no wallpaper when its file is not
  available at sign-in.
- **Remote Desktop.** **Enforce Removal of Remote Desktop Wallpaper** or
  the client's Experience settings hide the wallpaper in remote sessions.
- **The user cannot read the file.** An image in another user's profile,
  or on a share the user cannot reach, cannot be shown.

## Why is it stretched, cropped or too small?

- `-WallpaperFit` defaults to `Center`. When the image is not the size of
  the monitor, Windows centres it at its own size. Render at the monitor
  size with `New-BGInfoBackground` (width and height `0`) and use
  `-WallpaperFit Fill`, as the reference script does.
- A `-FilePath` image keeps its own size; text positions are relative to
  that image, not to the screen. Use `New-BGInfoBackground -ImagePath`
  instead: it fits the image into a canvas of the monitor's size
  (`-ImageFit Cover` by default).
- The fit applies to all monitors at once (`IDesktopWallpaper::SetPosition`
  has no monitor argument).
- The resolution changed after the last run (docking, a projector, a remote
  session resized). The next unlock or timer run fixes it; see
  [Task Scheduler](../task-scheduler/#resolution-and-display-changes).

## Why is the text blurry on high-DPI screens?

The image is smaller than the screen and Windows scales it up. Check the
size of the rendered file against the native resolution:

```powershell
Add-Type -AssemblyName System.Drawing
$file = Join-Path $env:LOCALAPPDATA 'PowerBGInfo\wallpaper-0.png'
$image = [System.Drawing.Image]::FromFile($file)
"$($image.Width) x $($image.Height)"
$image.Dispose()
Get-BGInfoMonitor | Select-Object Index, Width, Height, Scale
```

- PowerBGInfo reads physical monitor geometry and effective DPI through
  DesktopManager's `GetMonitorSnapshots()`. Both Windows PowerShell 5.1 and
  PowerShell 7 use this path, independent of the host's DPI awareness.
  `Get-BGInfoMonitor` reports physical `Width` and `Height`, with `Scale`
  such as 1.5 for 150 %; an empty scale means DPI was unavailable, not that
  the monitor is at 100 %. Re-query after a display change or remote
  reconnect. If no monitors can be read, rendering falls back to
  1920 x 1080; give `New-BGInfoBackground -Width -Height` explicit dimensions
  when rendering for a different screen.
- Small fonts on a 4K screen look thin rather than blurry. Scale the theme
  with the screen, as the reference script does (`-Scale` of height divided
  by 1080).
- A small corporate image scaled up to 4K stays soft. Use a source image at
  least as large as the biggest screen.

## How do I make tile text bigger or smaller?

Set `-TextScale` on one tile or `-TileTextScale` on its canvas. The
`New-BGInfoTheme -Scale` value is the default. Tiles without explicit width
or height grow with the text; fixed-size tiles keep their dimensions and may
shrink text or trim detail when the requested size does not fit. The layout
reports those fit adjustments.

## How do I migrate from BGInfo?

There is no importer for `.bgi` files; rebuild the layout in PowerShell.
Common BGInfo fields map to `New-BGInfoValue -BuiltinValue`:

| BGInfo field | PowerBGInfo |
| --- | --- |
| Host Name | `HostName` |
| User Name | `UserName`, `FullUserName` |
| Logon Domain | `UserDNSDomain`, `Domain` |
| OS Version | `OSName`, `OSVersion`, `OSBuild` |
| Boot Time | `OSLastBootUpTime`, `Uptime` |
| IP Address | `IPv4Address`, `IPv6Address` |
| CPU | `CpuName`, `CpuCores`, `CpuLogicalCores` |
| Memory | `RAMSize` |
| Free Space | `DiskSummary`, or `Get-BGInfoSystemInfo` disks in a table |
| Custom fields (WMI, registry, environment) | Your own data function and `New-BGInfoValue -Name -Value` |

Behaviour differences:

- BGInfo's `/timer:0` applies without a dialog; PowerBGInfo never shows a
  dialog.
- BGInfo's `/all` changes the wallpaper for every signed-in user from one
  run. PowerBGInfo changes the wallpaper of the account that runs it;
  `-AllUsers` writes the path into every profile for their next sign-in.
  For signed-in users, run the per-user task.
- BGInfo's logon desktop option is `-Target LogonScreen`, or better the
  lock screen policy (see [Group Policy](../group-policy/)).
- BGInfo's "Copy existing settings" background is PowerBGInfo's default
  when you give no base image: it draws on the current wallpaper, and on
  later runs on the original it recorded, not on its own output. A fixed
  base is still the predictable choice (see
  [Run in production](../deployment/#the-image-size-and-the-base-image)).
  When `-FilePath` and `-OutputFileName` point to the same image, PowerBGInfo
  keeps an adjacent copy of the original as the base for later runs.

## Does it work on Server Core?

No wallpaper can be shown: Server Core has no desktop shell. Microsoft
notes that the shell APIs PowerShell uses for zone checks are missing
there too. You can still render files with `-Target File` and look at
them elsewhere. On servers with the Desktop Experience it works like a
client.

## Why does it not work under SYSTEM?

It does run, but SYSTEM's wallpaper is SYSTEM's own. Under SYSTEM:

- `-Target Wallpaper` changes a desktop nobody sees.
- `UserName` and `FullUserName` are SYSTEM. `LoggedOnUser` shows the user
  of the active console session through DesktopManager's `GetSessionUser()`.
  PowerBGInfo keeps `Win32_ComputerSystem.UserName` as a last resort, then
  `Unknown` if neither source supplies a user. This does not identify a
  remote user whose session the task never entered.
- The monitor size may not be readable; the fallback is 1920 x 1080.

Use SYSTEM for `-Target File` (machine images), `-AllUsers` and managed
machine-policy deployment. Run per-user wallpaper and lock screen attempts
in the intended user's session. A lock screen attempt under SYSTEM targets
SYSTEM's account first; only a subsequent policy fallback is device-wide.

## Why does the lock screen not change on Windows Pro?

- The documented ways to force a lock screen image, the Group Policy
  setting "Force a specific default lock screen and logon image" and the
  Personalization CSP, apply only to Enterprise and Education (the policy
  also to Server). Pro ignores them, apart from the Shared PC cases listed
  in [Microsoft Intune](../intune/#the-personalization-csp-enforced-images).
- Both Windows PowerShell 5.1 and PowerShell 7 first attempt the running
  account's WinRT lock screen API and wait for its result. That per-user
  attempt needs no elevation and is separate from the policy's edition
  limits. Policy restrictions, Windows configuration or an unavailable user
  profile can still prevent a change.
- If WinRT fails, PowerBGInfo allows the `PersonalizationPolicy` fallback
  to write that same machine-policy value. Only the fallback needs elevation.
  PowerBGInfo exposes a failed result and warns when a successful policy
  write targets an unsupported or unreadable edition. The write does not
  establish that Windows applied the image. Check the error or edition
  warning; switching hosts does not choose a different route. See
  [Run in production](../deployment/#lock-screen).
- Check which account runs the command. An elevated session under a separate
  admin account targets the admin's per-user lock screen, not the user's.

## Why do all my monitors show the same image?

- One run renders one monitor (`-MonitorIndex`, default `0`). Loop over
  `Get-BGInfoMonitor`, as the reference script does.
- Each monitor needs its own `-OutputFileName`. If two monitors point at
  the same file, both show whatever was written last.
- On some Remote Desktop session hosts the monitors have no device path and
  the wallpaper is set for the whole session; see
  [Remote Desktop](../remote-desktop/#what-is-different-in-a-remote-session).
- `-WallpaperFit Span` stretches one image across all monitors.

## Why is the information outside the visible area?

- Check that `-MonitorIndex` points at the monitor you expect;
  `Get-BGInfoMonitor` lists the indexes with their sizes.
- A `-FilePath` image of another size, combined with `-WallpaperFit`, moves
  or crops what you placed. Render at the monitor size with
  `New-BGInfoBackground` and use `-WallpaperFit Fill`.
- `-PositionX`, `-PositionY` and `-TextPosition` place the text on the
  image; `-UseScreenCoordinates` makes them follow the monitor's coordinate
  space instead. Layout regions (`-Region Left`, `Right`, `Top`, `Bottom`,
  `Center`) keep panels inside the screen and clear of the taskbar on any
  size.
- Author the layout with `-Target File` at the sizes you deploy to before
  applying it.

## Why is the text hard to read?

Use a contrasting colour, or a theme (`New-BGInfo -Theme`), which gives
text, panels and widgets matching colours. Over a photo, add a panel, or
`New-BGInfoBackground -ImagePath` with a translucent gradient as a scrim.
Place text over a calm part of the image. Judge the result at normal
desktop scaling, not zoomed into the image file.

## How often should it refresh?

At sign-in, at unlock and, if the values change during the day, every 30
to 60 minutes. Machine identity and support details need no timer. Charts
keep up to `-MaxPoints` (60) samples, so a 30-minute timer shows about 30
hours of history. On multi-session hosts skip the timer.

## Other questions

| Symptom | Cause and fix |
| --- | --- |
| The text is drawn twice, or each run adds another layer | The base image was an earlier output that PowerBGInfo could not recognise, for example a copy Windows or a policy made of it. PowerBGInfo records its outputs in `PowerBgInfo.outputs.txt` and draws on the original; give a fixed base with `New-BGInfoBackground` or `-FilePath` to rule it out. |
| `Supply values for the following parameters: ConfigurationDirectory` | `-ConfigurationDirectory` is required (issue #9). |
| `Operation requires administrative privileges.` | `-AllUsers` without elevation. Per-user lock screen attempts do not require elevation. |
| A lock screen failure after the WinRT attempt | Check the reported error, running account, profile and policy. The machine-policy fallback needs elevation; a denied write is a failure. |
| A lock screen policy edition warning | The result used `PersonalizationPolicy` on an unsupported or unreadable edition. A policy write does not prove a visible change; switching PowerShell hosts does not remove the edition limit. |
| `The term 'New-BGInfo' is not recognized` from Intune or Configuration Manager | A 32-bit PowerShell; use the 64-bit host (see [Microsoft Intune](../intune/#which-host-runs-what)). |
| `Error HRESULT E_FAIL has been returned from a call to a COM component` | Reported with a closed laptop lid or duplicated displays (issue #18). Re-run after the display layout settles; the unlock trigger does this. |
| A warning that an image `is an image PowerBGInfo wrote earlier` | The base image has the name of a PowerBGInfo output but no record, so it was skipped. Give a fixed base. |
| A warning that a data source `did not finish within <n> s` | `Invoke-BGInfoDataSource` hit its time limit and used `-Default`. Fix or speed up the source; the wallpaper still rendered. |
| Errors only in the VS Code PowerShell extension on Windows PowerShell 5.1 | A known host issue; use the regular console or PowerShell 7. |
| The task never starts on a laptop | Default task conditions do not start on battery. See [Task Scheduler](../task-scheduler/#step-1-register-the-per-user-task). |
| A Group Policy logon script runs five minutes late | The logon script delay; see [Group Policy](../group-policy/#step-2-option-b-a-user-logon-script). |

## Further reading

- [BgInfo (Sysinternals)](https://learn.microsoft.com/sysinternals/downloads/bginfo)
- [IDesktopWallpaper interface](https://learn.microsoft.com/windows/win32/api/shobjidl_core/nn-shobjidl_core-idesktopwallpaper)
- [ADMX_Desktop: Desktop Wallpaper](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-desktop)
- [ADMX_ControlPanelDisplay: lock screen policies](https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-controlpaneldisplay)
- [Personalization CSP](https://learn.microsoft.com/windows/client-management/mdm/personalization-csp)
- [about_Execution_Policies: Server Core](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_execution_policies)
