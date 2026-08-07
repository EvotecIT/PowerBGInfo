---
title: "Export and deploy a PowerBGInfo configuration"
description: "Separate PowerBGInfo layout authoring from scheduled or RMM execution."
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

Keep the JSON beside the deployment policy, test with file output first, and run the final user or system target from the appropriate security context.
