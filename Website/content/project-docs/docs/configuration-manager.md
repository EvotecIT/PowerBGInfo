---
title: "Deploy PowerBGInfo with Configuration Manager"
description: "Install PowerBGInfo and register its refresh task with a Configuration Manager application, a package and program, or Run Scripts."
layout: docs
---

Configuration Manager installs the module and the script and registers the
scheduled task. It does not run the wallpaper itself at every sign-in;
the task from [Task Scheduler](../task-scheduler/) does that.

## When to use this route

- Devices managed by Configuration Manager, including co-managed devices
  where the apps workload stays with Configuration Manager.

## Prerequisites

- The source folder from [Microsoft Intune](../intune/#step-1-build-the-win32-app), step 1: `Install.ps1`,
  `Uninstall.ps1`, `Register-BGInfoTask.ps1`, the saved module and the
  scripts. The same files work for both tools.
- A content source share that the site server's computer account can read.

## Option A (recommended): an application

1. **Software Library > Application Management > Applications > Create
   Application**, **Manually specify the application information**.
2. Add a **Script Installer** deployment type with the folder as content.
3. **Installation program** (one line):

   ```text
   %windir%\System32\WindowsPowerShell\v1.0\powershell.exe
   -NoProfile -ExecutionPolicy Bypass -File .\Install.ps1
   ```

   **Uninstall program**: the same with `.\Uninstall.ps1`.
4. **Detection method**: the folder
   `C:\Program Files\WindowsPowerShell\Modules\PowerBGInfo\<version>`, the
   version folder you packaged, exists
   and the file `C:\Program Files\PowerBGInfo\Set-BGInfoWallpaper.ps1`
   exists. Clear **This file or folder is associated with a 32-bit
   application on 64-bit systems**.
5. **User experience**: **Install for system**, **Whether or not a user is
   logged on**.
6. Deploy as **Required** to a device collection.

Leave **Run installation and uninstall program as 32-bit process on
64-bit clients** unchecked and use the `System32` command above to start
64-bit Windows PowerShell. If the deployment type must run as a 32-bit
process, use `%windir%\Sysnative\WindowsPowerShell\v1.0\powershell.exe`
instead; `Sysnative` is available only to a 32-bit process on 64-bit Windows.
Check `AppEnforce.log` on a pilot device to confirm which PowerShell started.

## Option B: a package and program

Use this if you still deploy with packages. Create a **Standard Program**:

- **Command line**:
  `%windir%\Sysnative\WindowsPowerShell\v1.0\powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Install.ps1`.
- **Program can run**: **Whether or not a user is logged on**.
- **Run mode**: **Run with administrative rights**.

Standard Program commands run as 32-bit processes on 64-bit clients, so
use `Sysnative` to launch 64-bit Windows PowerShell.

## Run Scripts

**Software Library > Scripts** runs an approved PowerShell script on a
collection, as the computer account (SYSTEM), with a one-hour timeout and
output truncated to 4 KB. A different administrator must approve each
script unless you turn that off in the hierarchy settings.

Run Scripts cannot set a user's wallpaper, because it does not run in the
user's session. Use it to run `Register-BGInfoTask.ps1` or
`Set-BGInfoMachineImage.ps1` on demand, for example after changing the
script on a share.

## Verify

On a client, in `C:\Windows\CCM\Logs`:

| Log | What it shows |
| --- | --- |
| `AppEnforce.log` | The installation command line and its exit code |
| `AppDiscovery.log` | Detection results |
| `Scripts.log` | Run Scripts executions |

Then run the checks from [Task Scheduler](../task-scheduler/).

## Pitfalls

- **Detection checks the wrong folder.** A detection rule for a 32-bit
  application looks under `Program Files (x86)`.
- **The deployment succeeds but nothing changes for users.** The
  application only registers the task; the wallpaper appears at the next
  sign-in or unlock.

## Further reading

- [Create applications](https://learn.microsoft.com/intune/configmgr/apps/deploy-use/create-applications)
- [Create and run scripts](https://learn.microsoft.com/intune/configmgr/apps/deploy-use/create-deploy-scripts)
- [Packages and programs](https://learn.microsoft.com/intune/configmgr/apps/deploy-use/packages-and-programs)
- [File System Redirector](https://learn.microsoft.com/windows/win32/winprog64/file-system-redirector)
