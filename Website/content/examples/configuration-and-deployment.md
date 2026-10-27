---
title: "Export and deploy a PowerBGInfo configuration"
description: "Export a reviewed PowerBGInfo layout to JSON and run it from a scheduled task or RMM tool, so deployments don't carry a long inline script."
layout: docs
---

Export a reviewed layout to JSON when deployment should not carry a long inline script.

```powershell
$configPath = 'C:\ProgramData\PowerBGInfo\workstation.json'

New-BGInfo {
    New-BGInfoValue -BuiltinValue HostName -Name 'Machine'
    New-BGInfoValue -BuiltinValue FullUserName -Name 'User'
    New-BGInfoValue -BuiltinValue OSName -Name 'Operating system'
    New-BGInfoValue -Name 'Support' -Value 'helpdesk@contoso.com'
} -MonitorIndex 0 `
    -Target File `
    -ConfigurationDirectory 'C:\ProgramData\PowerBGInfo' `
    -JsonPath $configPath `
    -ExportOnly

Invoke-BGInfo -Path $configPath
```

Keep the JSON beside the deployment policy, test with file output first, and run the final user or system target from the appropriate security context. [Run in production](/projects/powerbginfo/docs/deployment/) explains which account each target needs and links to scheduling guidance.
