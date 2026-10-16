---
title: "Install PowerBGInfo"
description: "Install PowerBGInfo from the PowerShell Gallery for one user or for all users."
layout: docs
---

Install PowerBGInfo before trying the curated desktop background examples.

## PowerShell Gallery

For yourself:

```powershell
Install-Module PowerBGInfo -Scope CurrentUser
```

Import the module and inspect the commands in the installed release:

```powershell
Import-Module PowerBGInfo
Get-Command -Module PowerBGInfo
```

PowerBGInfo supports Windows PowerShell 5.1 and PowerShell 7 on Windows. The package includes the rendering and desktop libraries it needs; separate installs of ImagePlayground or DesktopManager are not required. When there is an update, run the same `Install-Module` command again.

Every release is Authenticode-signed by Evotec. For machines that run it unattended, install it for all users, pinned to the version you tested, and read how to trust the publisher where the execution policy is `AllSigned`:

- [Getting the module onto machines](../deployment/#getting-the-module-onto-machines)
- [Script signing and AllSigned](../security/#script-signing-and-allsigned)

## Next steps

- Review the [project overview](../overview/)
- Read [Run in production](../deployment/)
- Browse the [PowerShell API reference](/projects/powerbginfo/api/)
- Browse the [curated examples](/projects/powerbginfo/examples/)
