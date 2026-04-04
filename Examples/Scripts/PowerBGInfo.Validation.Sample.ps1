New-BGInfo {
    New-BGInfoValue -BuiltinValue HostName -Name 'Host'
    New-BGInfoValue -BuiltinValue FullUserName -Name 'User'
    New-BGInfoLabel -Name 'System' -Color LemonChiffon -FontSize 14 -FontFamilyName 'Calibri'
    New-BGInfoValue -BuiltinValue OSName -Name 'OS'
    New-BGInfoValue -BuiltinValue OSBuild -Name 'Build'
    New-BGInfoValue -BuiltinValue CpuLogicalCores -Name 'Logical Cores'
} -FilePath '..\Samples\TapN-Evotec-1600x900.jpg' `
    -ConfigurationDirectory '..\Output' `
    -OutputFileName 'PowerBGInfo.Validation.Sample.png' `
    -BackgroundColor Black `
    -Target File `
    -PassThru
