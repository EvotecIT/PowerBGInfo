---
title: "Install PowerBGInfo"
description: "Install PowerBGInfo from the package source used by this project."
layout: docs
---

Install PowerBGInfo before trying the curated desktop background examples.

## PowerShell Gallery

```powershell
Install-Module PowerBGInfo -Scope CurrentUser
```

Import the module and inspect the exact commands in the installed release:

```powershell
Import-Module PowerBGInfo
Get-Command -Module PowerBGInfo
```

PowerBGInfo supports Windows PowerShell 5.1 and PowerShell 7+ on Windows. Current 2.x packages include the required rendering and desktop-management assemblies; separate installs of ImagePlayground or DesktopManager are not required.

## Next steps

- Review the [project overview](../overview/)
- Read [deployment and refresh guidance](../deployment/)
- Browse the [PowerShell API reference](/projects/powerbginfo/api/)
- Browse the [curated examples](/projects/powerbginfo/examples/)
