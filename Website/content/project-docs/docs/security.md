---
title: "PowerBGInfo security and operations"
description: "Rights, file locations, Authenticode signing and AllSigned, Constrained Language Mode under AppLocker and App Control, and logging for PowerBGInfo."
layout: docs
---

What PowerBGInfo needs, what it writes, and how to run it where scripts
are signed, constrained or audited.

## Least privilege

| Action | Needs admin | Notes |
| --- | --- | --- |
| `-Target Wallpaper` for yourself | No | Runs in the user's session |
| `-Target File` | No | Only write access to `-ConfigurationDirectory` |
| `-AllUsers` | Yes | Writes every profile's registry hive and the default profile |
| `-Target LogonScreen`, `-Target Both` | Yes | DesktopManager checks elevation and stops otherwise |
| `Get-BGInfoSystemInfo`, `Get-BGInfoMonitor` | No | BitLocker state usually reads `Unknown` without elevation |
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
- **Per-user output**: `%LOCALAPPDATA%\PowerBGInfo`, including
  `PowerBgInfo.outputs.txt`, the record of which image each output was drawn
  on.
- **Machine images** for policies: `C:\ProgramData\PowerBGInfo`, written by
  SYSTEM. By default standard users can create files in `ProgramData`
  folders; they cannot change files SYSTEM created. Check the folder with
  `icacls` or `Get-Acl` on your build if a policy reads files from it.

## Script signing and AllSigned

Every PowerBGInfo release, like every Evotec module release, is
Authenticode-signed by Evotec at release time, through the module's build
scripts. The signer is **Evotec Services sp. z o.o.** The signature covers
the module's PowerShell files (`PowerBGInfo.psd1`, `PowerBGInfo.psm1` and
the libraries script) and its own assemblies (PowerBGInfo, DesktopManager,
ChartForgeX and the rest of the Evotec libraries). Microsoft assemblies in
the package keep Microsoft's signatures. You do not need to re-sign the
module, and re-signing would replace Evotec's signature with yours.

With the `AllSigned` execution policy (or **Allow only signed scripts** in
Group Policy), every script PowerShell loads must be signed by a publisher
the machine trusts. The first time it meets a publisher that is not in the
**Trusted Publishers** store, PowerShell asks whether to run the file. A
scheduled task cannot answer, so the run fails with "AuthorizationManager
check failed". Trust the publisher on the machine before the first run:

1. Check the signatures of the installed module:

   ```powershell
   $module = Get-Module -Name PowerBGInfo -ListAvailable |
       Sort-Object Version -Descending | Select-Object -First 1
   Get-ChildItem -Path $module.ModuleBase -Recurse `
       -Include *.ps1, *.psm1, *.psd1, *.dll |
       Get-AuthenticodeSignature |
       Select-Object Status, Path, @{
           Name = 'Signer'; Expression = { $_.SignerCertificate.Subject }
       }
   ```

   Every file should be `Valid`. Do not trust a module whose files report
   `HashMismatch` or `NotSigned`: get a clean copy instead.
2. Export the publisher certificate from a signed module file, on a
   machine where you checked it:

   ```powershell
   $file = Join-Path $module.ModuleBase 'PowerBGInfo.psm1'
   $signature = Get-AuthenticodeSignature -FilePath $file
   $signature.SignerCertificate |
       Export-Certificate -FilePath 'C:\Staging\Evotec.cer'
   ```
3. Add it to the machine's Trusted Publishers store: with Group Policy
   (**Computer Configuration > Policies > Windows Settings > Security
   Settings > Public Key Policies > Trusted Publishers**), or in your
   installer, elevated:

   ```powershell
   Import-Certificate -FilePath 'C:\Staging\Evotec.cer' `
       -CertStoreLocation 'Cert:\LocalMachine\TrustedPublisher'
   ```

   The certificate chains to a DigiCert root that Windows already trusts;
   if your machines restrict root certificates, add that root too.

Your own scripts need signing too. In an `AllSigned` environment
`Set-BGInfoWallpaper.ps1`, `Register-BGInfoTask.ps1`, `Install.ps1`, the
Intune detection and remediation scripts, and anything else you deploy must
be signed with a code-signing certificate whose publisher the machines
trust. A signature covers the exact file content, so sign again after every
edit:

```powershell
$cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert |
    Select-Object -First 1
$files = Get-ChildItem 'C:\Packages\PowerBGInfo' -Recurse -Filter *.ps1
foreach ($file in $files) {
    Set-AuthenticodeSignature -FilePath $file.FullName -Certificate $cert `
        -TimestampServer 'http://timestamp.digicert.com' `
        -HashAlgorithm SHA256
}
```

The timestamp server is an example; use the one your certificate
authority documents. A timestamp keeps the signature valid after the
certificate expires.

With `RemoteSigned`, only files that carry the downloaded-from-the-internet
mark need a signature. The module is signed either way; scripts copied from
a browser download or some file shares need signing or `Unblock-File`.

`-ExecutionPolicy Bypass` on the command line does not override an
execution policy set by Group Policy. Intune checks signatures only when
**Enforce script signature check** is on; for Remediations the script must
then be UTF-8 without a byte order mark.

## Constrained Language Mode, AppLocker and App Control

When AppLocker or App Control for Business (WDAC) enforces script rules,
PowerShell runs untrusted scripts in `ConstrainedLanguage` mode and trusted
ones in `FullLanguage`. In `ConstrainedLanguage` mode `Add-Type` can load
signed assemblies only, methods can be called on a short list of core types
only, and `[pscustomobject]` and most .NET types are blocked.

- **The module needs `FullLanguage`.** `PowerBGInfo.psm1` loads its .NET
  assemblies with `Add-Type -Path` and imports the binary module. Allow it
  with a publisher (signer) rule for Evotec Services sp. z o.o., which
  covers the module's script files and its Evotec assemblies.
- **Your wallpaper script does not.** The reference script in
  [Run in production](../deployment/#the-reference-script) uses only
  cmdlets, hashtables and arithmetic. `Get-BGInfoMonitor` replaces the
  DesktopManager types that earlier scripts called, which Constrained
  Language Mode blocks. Such a script can stay unsigned under App Control
  and run constrained, or be signed and trusted like the module.

Check the mode a script actually gets:

```powershell
$ExecutionContext.SessionState.LanguageMode
```

Verified for this guide in a session switched to `ConstrainedLanguage`
after the module was loaded: `Get-BGInfoMonitor` and the reference script's
constructs work, `[DesktopManager.Monitors]::new()` and `[Math]` calls do
not. Not verified: a real App Control policy, and the language mode of the
runspace that `Invoke-BGInfoDataSource` creates for your data function on
a locked-down machine. Test a data source under your policy before rollout.

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

PowerBGInfo reports problems it can work around as warnings, which appear
in the transcript:

- `<Name> did not finish within <n> s; using the default value.` from
  `Invoke-BGInfoDataSource` when it falls back.
- `... is an image PowerBGInfo wrote earlier, and the image it was drawn on
  is not recorded.` when a base image was skipped to avoid drawing the
  text twice.
- `The Windows lock screen API is not available in this PowerShell host,
  ...` when `-Target LogonScreen` used the policy value on an edition that
  ignores it.

## Further reading

- [about_Execution_Policies](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_execution_policies)
- [about_Signing](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_signing)
- [Get-AuthenticodeSignature](https://learn.microsoft.com/powershell/module/microsoft.powershell.security/get-authenticodesignature)
- [Set-AuthenticodeSignature](https://learn.microsoft.com/powershell/module/microsoft.powershell.security/set-authenticodesignature)
- [Import-Certificate](https://learn.microsoft.com/powershell/module/pki/import-certificate)
- [about_Language_Modes](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_language_modes)
- [Use App Control to secure PowerShell](https://learn.microsoft.com/powershell/scripting/security/app-control/application-control)
- [about_Group_Policy_Settings](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_group_policy_settings)
- [Remediations: script requirements](https://learn.microsoft.com/intune/device-management/tools/deploy-remediations)
