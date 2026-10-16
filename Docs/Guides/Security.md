# Security and operations

What PowerBGInfo needs, what it writes, and how to run it where scripts
are signed, constrained or audited.

## Least privilege

| Action | Needs admin | Notes |
| --- | --- | --- |
| `-Target Wallpaper` for yourself | No | Runs in the user's session |
| `-Target File` | No | Only write access to `-ConfigurationDirectory` |
| `-AllUsers` | Yes | Writes every profile's registry hive and the default profile |
| `-Target LogonScreen`, `-Target Both` | Yes | The module checks elevation and stops otherwise |
| `Get-BGInfoSystemInfo` | No | BitLocker state usually reads `Unknown` without elevation |
| `Install-Module -Scope AllUsers`, copying to `Program Files` | Yes | Once per machine |
| Registering a task for the `Users` group or SYSTEM | Yes | Once per machine |

Run the per-user wallpaper task as a limited user (`-RunLevel Limited`).
Keep elevation for the installer and for SYSTEM tasks that write
machine-wide files.

## Where things live

- **Scripts**: `C:\Program Files\PowerBGInfo`. Standard users can read and
  run them but not change them. A script that runs as SYSTEM must never sit
  in a folder users can write to.
- **Module**: `C:\Program Files\WindowsPowerShell\Modules` or
  `C:\Program Files\PowerShell\Modules`, writable by administrators only.
- **Per-user output**: `%LOCALAPPDATA%\PowerBGInfo`.
- **Machine images** for policies: `C:\ProgramData\PowerBGInfo`, written by
  SYSTEM. By default standard users can create files in `ProgramData`
  folders; they cannot change files SYSTEM created. Check the folder with
  `icacls` or `Get-Acl` on your build if a policy reads files from it.

## Script signing and AllSigned

With the `AllSigned` execution policy (or **Allow only signed scripts** in
Group Policy) every script and configuration file PowerShell loads must be
signed by a trusted publisher. PowerShell prompts before it runs a script
from a publisher you have not yet trusted, and an unattended task cannot
answer, so put the signing certificate in the machine's **Trusted
Publishers** store (and its root in **Trusted Root Certification
Authorities**). That covers:

- your `Set-BGInfoWallpaper.ps1` and any other script you deploy;
- the module's `PowerBGInfo.psd1` and `PowerBGInfo.psm1`.

Check what you have before you deploy:

```powershell
$module = Get-Module -Name PowerBGInfo -ListAvailable |
    Sort-Object Version -Descending | Select-Object -First 1
Get-ChildItem -Path $module.ModuleBase -Include *.ps1, *.psm1, *.psd1 `
    -Recurse | Get-AuthenticodeSignature |
    Select-Object Status, @{ n = 'Signer'; e = {
        $_.SignerCertificate.Subject } }, Path
```

If the module files are not signed by a publisher you trust, sign them
yourself with your code-signing certificate, and re-sign after every
module update:

```powershell
$cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert |
    Select-Object -First 1
$files = 'C:\Program Files\PowerBGInfo\Set-BGInfoWallpaper.ps1'
Set-AuthenticodeSignature -FilePath $files -Certificate $cert `
    -TimestampServer 'http://timestamp.digicert.com' -HashAlgorithm SHA256
```

The timestamp server is an example; use the one your certificate
authority documents. Intune checks signatures only when **Enforce script
signature check** is on; for Remediations the script must then be UTF-8
without a byte order mark.

## Constrained Language Mode, AppLocker and App Control

When AppLocker or App Control for Business (WDAC) enforces script rules,
PowerShell runs untrusted scripts in `ConstrainedLanguage` mode and trusted
ones in `FullLanguage`. In `ConstrainedLanguage` mode `Add-Type` can load
signed assemblies only, and only a short list of .NET types is allowed.

PowerBGInfo needs `FullLanguage`:

- `PowerBGInfo.psm1` loads its .NET assemblies with `Add-Type -Path` and
  imports the binary module.
- The reference script uses `[Math]` and `[DesktopManager.Monitors]`,
  which are not on the allowed type list.

So the module files, its DLLs and your scripts must be allowed by the
policy, typically through a signer rule for your code-signing certificate
(and for the module publisher if the DLLs are signed). Check the mode a
script actually gets:

```powershell
$ExecutionContext.SessionState.LanguageMode
```

Not verified for this guide: the language mode of the runspace that
`Invoke-BGInfoDataSource` creates for your data function on a locked-down
machine. Test a data source under your policy before rollout.

## What the wallpaper shows

Everything on the wallpaper is visible to anyone who can see the screen,
and on the lock screen to anyone at the device before sign-in.

- Keep the lock screen to machine identity and support details. No user
  names, IP addresses or patch levels before sign-in unless you mean to.
- Do not put secrets, tokens or passwords in scripts. Microsoft gives the
  same advice for Intune scripts. A data source that needs credentials
  should read them from where the running account already has access.

## Logging and troubleshooting

| Source | Where |
| --- | --- |
| Reference script transcript | `%LOCALAPPDATA%\PowerBGInfo\last-run.log` (last run only) |
| Task results | `Get-ScheduledTaskInfo`; Event Viewer `Microsoft-Windows-TaskScheduler/Operational` once task history is enabled |
| Intune | `C:\ProgramData\Microsoft\IntuneManagementExtension\Logs` |
| Configuration Manager | `C:\Windows\CCM\Logs` |
| Group Policy | `gpresult /h report.html` |
| PowerShell | Script block logging (Group Policy: **Turn on PowerShell Script Block Logging**) writes to `Microsoft-Windows-PowerShell/Operational` |
| DesktopManager | `%APPDATA%\DesktopManager\wallpaper-history.json` lists applied paths. Its internal warnings go to .NET tracing, not to a file. |

`Invoke-BGInfoDataSource` writes a warning (`<Name> did not finish within
<n> s; using the default value.`) when it falls back. Warnings appear in
the transcript.

## Further reading

- [about_Execution_Policies](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_execution_policies)
- [about_Signing](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_signing)
- [Set-AuthenticodeSignature](https://learn.microsoft.com/powershell/module/microsoft.powershell.security/set-authenticodesignature)
- [about_Language_Modes](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_language_modes)
- [Use App Control to secure PowerShell](https://learn.microsoft.com/powershell/scripting/security/app-control/application-control)
- [about_Group_Policy_Settings](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_group_policy_settings)
- [Remediations: script requirements](https://learn.microsoft.com/intune/device-management/tools/deploy-remediations)
