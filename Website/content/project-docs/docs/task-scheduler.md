---
title: "Run PowerBGInfo with Task Scheduler"
description: "Refresh the PowerBGInfo wallpaper at sign-in, at unlock and on a timer with a scheduled task for every user, and render machine images as SYSTEM."
layout: docs
---

Task Scheduler is the most direct way to refresh a wallpaper: it can start
the script at sign-in, at unlock and on a timer, in the user's own session.
The Group Policy, Intune and Configuration Manager guides deliver the same
task.

## When to use this route

- Standalone machines, labs, servers you sign in to, or any fleet where
  another tool can run the registration script once.
- Whenever the wallpaper should refresh while the user is signed in.

## Prerequisites

- The module installed for all users and `Set-BGInfoWallpaper.ps1` from
  [Run in production](../deployment/) in `C:\Program Files\PowerBGInfo`.
- An elevated session to register a task for all users. The task itself
  runs without elevation.
- Windows PowerShell 5.1 or PowerShell 7. The examples use
  `powershell.exe`; replace the path with
  `C:\Program Files\PowerShell\7\pwsh.exe` to use PowerShell 7.

## Who the task runs as

| Principal | Runs | Can do | Cannot do |
| --- | --- | --- | --- |
| Group `BUILTIN\Users` (SID `S-1-5-32-545`), limited | In the session of the member who triggered it, only while they are signed in | `-Target Wallpaper` for that user, `-Target File` into the user's folders, per-user `-Target LogonScreen` when the lock-screen API is available | `-AllUsers` and the machine-policy lock-screen fallback (both need elevation) |
| `SYSTEM` | Without a user, in session 0 | `-Target File` anywhere, `-AllUsers`, `-Target LogonScreen` | A user's live wallpaper |

A group principal is the scripted form of "Run only when user is logged on"
for every user: Task Scheduler runs the task with the interactive token of
the group member, so it can change that member's desktop. A SYSTEM task
changes SYSTEM's own wallpaper, which nobody sees.

Two things are not documented by Microsoft and were not tested for this
guide: which sessions a group task with a **time** trigger runs in when
several members are signed in (a multi-session host, fast user switching),
and whether `-AllUsers` run as SYSTEM finishes cleanly. With `-AllUsers`,
the profile registry writes happen first; the following step applies the
image to SYSTEM's own desktop and may fail in session 0. Test both on your
build before relying on them.

## Step 1: register the per-user task

Save this as `Register-BGInfoTask.ps1` and run it once, elevated. It
registers one task for all users with three triggers: sign-in (after 30
seconds, so Explorer is up), unlock, and every 30 minutes. `-Force`
replaces an existing task, so it is safe to run again. The Intune and
Configuration Manager installers in the other guides call this file.

```powershell
# Register-BGInfoTask.ps1: run elevated, in 64-bit PowerShell.
$taskName = 'PowerBGInfo wallpaper'
$script = 'C:\Program Files\PowerBGInfo\Set-BGInfoWallpaper.ps1'
$shell = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe"
$arguments = '-NoProfile -NonInteractive -WindowStyle Hidden ' +
    "-ExecutionPolicy Bypass -File `"$script`""
$action = New-ScheduledTaskAction -Execute $shell -Argument $arguments

$logon = New-ScheduledTaskTrigger -AtLogOn
$logon.Delay = 'PT30S'

$stateClass = Get-CimClass -ClassName MSFT_TaskSessionStateChangeTrigger `
    -Namespace 'Root/Microsoft/Windows/TaskScheduler'
$unlock = New-CimInstance -CimClass $stateClass -ClientOnly -Property @{
    StateChange = 8   # TASK_SESSION_UNLOCK
    Enabled     = $true
}

$timer = New-ScheduledTaskTrigger -Once -At (Get-Date).Date `
    -RepetitionInterval (New-TimeSpan -Minutes 30) `
    -RepetitionDuration (New-TimeSpan -Days 3650)

$principal = New-ScheduledTaskPrincipal -GroupId 'S-1-5-32-545' `
    -RunLevel Limited

$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries -StartWhenAvailable `
    -MultipleInstances Parallel `
    -ExecutionTimeLimit (New-TimeSpan -Minutes 5)

Register-ScheduledTask -TaskName $taskName -TaskPath '\PowerBGInfo\' `
    -Action $action -Trigger $logon, $unlock, $timer `
    -Principal $principal -Settings $settings -Force `
    -Description 'Renders the PowerBGInfo wallpaper for the signed-in user.'
```

What the pieces do:

- `Parallel` preserves a second user's trigger while another session is
  rendering. Each user writes to their own output directory; a group task
  using `IgnoreNew` would discard that second user's run.
- `-AtLogOn` without `-User` fires for any user who signs in.
- The unlock trigger is a `SessionStateChangeTrigger` with
  `StateChange = 8` (`TASK_SESSION_UNLOCK`). `New-ScheduledTaskTrigger` has
  no parameter for it, so the trigger is created from its CIM class. Other
  values: `1` console connect, `3` remote connect (useful on
  [Remote Desktop](../remote-desktop/) hosts), `7` lock.
- The trigger repeats every 30 minutes for ten years. Re-register it before
  the duration expires. Older ScheduledTasks modules require a finite
  `-RepetitionDuration` when `-RepetitionInterval` is supplied.
- `New-ScheduledTaskSettingsSet` defaults to *not* starting on battery and
  stopping when the laptop unplugs, and to a 72-hour execution time limit.
  The switches above change both.
- `-ExecutionPolicy Bypass` on the command line does not override an
  execution policy set by Group Policy. In an `AllSigned` environment, sign
  your scripts and trust the module's publisher (see
  [Security and operations](../security/#script-signing-and-allsigned)).
- `-WindowStyle Hidden` hides the console once PowerShell starts; a window
  can still flash briefly.

To use PowerShell 7, change `$shell`:

```powershell
$shell = "$env:ProgramFiles\PowerShell\7\pwsh.exe"
```

## Step 2 (optional): a machine-level task as SYSTEM

Use this for images that are not per-user: a lock screen image that a
policy points at (see [Group Policy](../group-policy/) and
[Microsoft Intune](../intune/)), or a staged default wallpaper. Save this as
`C:\Program Files\PowerBGInfo\Set-BGInfoMachineImage.ps1`:

```powershell
#Requires -Version 5.1
# Renders a machine-wide image as a file. Runs as SYSTEM: show machine
# facts only. UserName would be SYSTEM.
param(
    [string] $OutputDirectory = (
        Join-Path $env:ProgramData 'PowerBGInfo'),
    [string] $OutputFileName = 'LockScreen.jpg',
    [int] $Width = 1920,
    [int] $Height = 1080
)

$ErrorActionPreference = 'Stop'
Import-Module -Name PowerBGInfo
$facts = Get-BGInfoSystemInfo
$scale = [int]($Height / 10.8) / 100

New-BGInfo -Theme (New-BGInfoTheme -Preset Corporate -Scale $scale) `
    -Target File -ConfigurationDirectory $OutputDirectory `
    -OutputFileName $OutputFileName {
    New-BGInfoBackground -Width $Width -Height $Height
    New-BGInfoLabel -Name $facts.ComputerName -FontWeight 700
    New-BGInfoValue -Name 'Support' -Value 'ext. 4357'
}
```

A SYSTEM process has no monitor of its own, so the size is passed in.
Choose the most common resolution and aspect ratio of the screens you
deploy to; Windows resizes the lock screen image to each screen, so keep
text away from the edges.

Register it, elevated:

```powershell
$script = 'C:\Program Files\PowerBGInfo\Set-BGInfoMachineImage.ps1'
$shell = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe"
$arguments = '-NoProfile -NonInteractive -ExecutionPolicy Bypass ' +
    "-File `"$script`""
$action = New-ScheduledTaskAction -Execute $shell -Argument $arguments
$triggers = @(
    New-ScheduledTaskTrigger -AtStartup
    New-ScheduledTaskTrigger -Daily -At '06:00'
)
$principal = New-ScheduledTaskPrincipal -UserId 'SYSTEM' `
    -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries -StartWhenAvailable `
    -ExecutionTimeLimit (New-TimeSpan -Minutes 5)

Register-ScheduledTask -TaskName 'PowerBGInfo machine image' `
    -TaskPath '\PowerBGInfo\' -Action $action -Trigger $triggers `
    -Principal $principal -Settings $settings -Force
```

## Resolution and display changes

Task Scheduler has no trigger for a display or resolution change, and
Microsoft does not document an event log entry for one that a custom event
trigger could watch. There is no clean way to re-render the moment a
laptop docks or a monitor is added. Practical options:

- The unlock trigger covers the common case of docking a locked laptop.
- The console connect (`1`) and remote connect (`3`) state changes cover
  switching users and reconnecting a remote session.
- The repeating trigger catches everything else within its interval.
- Rendering with `New-BGInfoBackground` at the monitor size and
  `-WallpaperFit Fill` keeps a stale image filling the screen until the next
  run, instead of leaving bars or tiles.

## Verify

```powershell
$task = Get-ScheduledTask -TaskPath '\PowerBGInfo\' `
    -TaskName 'PowerBGInfo wallpaper'
$task.Triggers | Select-Object CimClassName, Delay, StateChange
Start-ScheduledTask -InputObject $task
Start-Sleep -Seconds 20
Get-ScheduledTaskInfo -InputObject $task |
    Select-Object LastRunTime, LastTaskResult, NextRunTime
```

`LastTaskResult` is the script's exit code: `0` for success, `1` when the
reference script caught an error. Then, as the user, read the transcript:

```powershell
Get-Content (Join-Path $env:LOCALAPPDATA 'PowerBGInfo\last-run.log')
```

The task history in Task Scheduler (Event Viewer:
`Microsoft-Windows-TaskScheduler/Operational`) records each start and the
result. On many installations task history is turned off until you choose
**Enable All Tasks History** in Task Scheduler.

Remove the tasks:

```powershell
Get-ScheduledTask -TaskPath '\PowerBGInfo\' |
    Unregister-ScheduledTask -Confirm:$false
```

## Pitfalls

- **Running the per-user task as SYSTEM.** It succeeds and changes nothing
  visible. Use the group principal.
- **A shared output folder.** Several users writing
  `C:\ProgramData\PowerBGInfo\wallpaper-0.png` overwrite each other, and a
  file created by one user usually cannot be replaced by another. The
  reference script writes to `%LOCALAPPDATA%\PowerBGInfo`.
- **A 32-bit PowerShell.** `C:\Windows\SysWOW64\WindowsPowerShell` does not
  see modules in `C:\Program Files\WindowsPowerShell\Modules`. Use the
  `System32` path from a 64-bit registration.
- **Too frequent a timer.** Each run renders every monitor and reads WMI.
  30 to 60 minutes is enough for most values; data that changes only at
  sign-in needs no timer at all.
- **The task does nothing on battery.** See the settings above.

## Further reading

- [New-ScheduledTaskTrigger](https://learn.microsoft.com/powershell/module/scheduledtasks/new-scheduledtasktrigger)
- [New-ScheduledTaskPrincipal](https://learn.microsoft.com/powershell/module/scheduledtasks/new-scheduledtaskprincipal)
- [Register-ScheduledTask](https://learn.microsoft.com/powershell/module/scheduledtasks/register-scheduledtask)
- [New-ScheduledTaskSettingsSet](https://learn.microsoft.com/powershell/module/scheduledtasks/new-scheduledtasksettingsset)
- [SessionStateChangeTrigger.StateChange](https://learn.microsoft.com/windows/win32/taskschd/sessionstatechangetrigger-statechange)
- [Principal.LogonType](https://learn.microsoft.com/windows/win32/taskschd/principal-logontype)
- [RepetitionPattern.Duration](https://learn.microsoft.com/windows/win32/taskschd/repetitionpattern-duration)
- [about_Execution_Policies](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_execution_policies)
