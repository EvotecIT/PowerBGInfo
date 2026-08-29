New-BGInfo {
    New-BGInfoValue -BuiltinValue HostName -Name 'Host' -FontWeight 700 -ValueFontFamilyName 'Consolas' -ValueTextCase ToggleCase
    New-BGInfoValue -BuiltinValue FullUserName -Name 'User' -Italic -ValueUnderlineStyle Dotted
    New-BGInfoLabel -Name 'System' -Color LemonChiffon -FontSize 14 -FontFamilyName 'Calibri' -UnderlineStyle Double -TextCase Uppercase
    New-BGInfoValue -BuiltinValue OSName -Name 'OS' -StrikethroughStyle Dashed -ValueBaseline Subscript
    New-BGInfoValue -BuiltinValue OSBuild -Name 'Build'
    New-BGInfoValue -BuiltinValue CpuLogicalCores -Name 'Logical Cores'
} -FilePath '..\Samples\TapN-Evotec-1600x900.jpg' `
    -ConfigurationDirectory '..\Output' `
    -OutputFileName 'PowerBGInfo.Validation.Sample.png' `
    -BackgroundColor Black `
    -ValueColor White `
    -Target File `
    -PassThru
