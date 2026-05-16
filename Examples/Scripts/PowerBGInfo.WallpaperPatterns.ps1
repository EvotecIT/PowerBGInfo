$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = (Resolve-Path (Join-Path -Path $scriptRoot -ChildPath '..\..')).Path
$solutionPath = Join-Path -Path $repositoryRoot -ChildPath 'Sources\PowerBGInfo.sln'
$modulePath = Join-Path -Path $repositoryRoot -ChildPath 'PowerBGInfo.psd1'
$examplesPath = (Resolve-Path (Join-Path -Path $scriptRoot -ChildPath '..')).Path
$sampleImage = Join-Path -Path $examplesPath -ChildPath 'Samples\TapC-Evotec-2560x1080.jpg'
$outputDirectory = Join-Path -Path $examplesPath -ChildPath 'Output'

# Build the current checkout so the pattern gallery uses the latest local cmdlets.
& dotnet build $solutionPath --configuration Debug --nologo -m:1 -nr:false | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for $solutionPath"
}

Import-Module -Name $modulePath -Force

$white = 'White'
$muted = '#E6D2DCE8'
$panel = '#AC0A101C'
$cyan = '#2DD4BF'
$blue = '#60A5FA'
$green = '#34D399'
$orange = '#FB923C'
$red = '#F87171'
$purple = '#A78BFA'
$yellow = '#FACC15'

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoValue -Name 'Hybrid Identity Lab' -Value 'ADFS to Entra ID migration' -Color LemonChiffon -ValueColor $white -FontSize 25 -ValueFontSize 17 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Domain' -Value 'LAB.CONTOSO.COM' -Color $muted -ValueColor $white
    New-BGInfoValue -Name 'Role' -Value 'Federation bridge and test clients' -Color $muted -ValueColor $white
    New-BGInfoValue -Name 'Reset' -Value 'Snapshot returns Friday 18:00 UTC' -Color $muted -ValueColor $white
    New-BGInfoValue -Name 'Owner' -Value 'Identity Engineering' -Color $muted -ValueColor $white

    New-BGInfoChart -Id 'lab-phase' -Title 'Exercise progress' -Kind ProgressBar -Values 65 -Labels 'Completed' -Width 410 -Height 130 -Anchor BottomRight -OffsetX 32 -OffsetY 32 -LineColor $cyan -TextColor $white -BackgroundColor $panel -Maximum 100 -NoProgressHandles -ShowDataLabels -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'lab-services' -Title 'Lab services' -Kind Pictorial -Values 5,1 -Labels 'Ready','Check' -Width 410 -Height 145 -Anchor BottomRight -OffsetX 32 -OffsetY 178 -Palette $green,$orange -TextColor $white -BackgroundColor $panel -PictorialSymbol Circle -PictorialColumns 6 -ShowDataLabels -Maximum 6 -ShowLatestValue:$false -NoHistory
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.Pattern.Lab.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color $muted `
    -ValueColor $white `
    -ValueWrapWidth 430 `
    -TextPosition TopLeft `
    -SpaceX 42 `
    -SpaceY 42

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoValue -Name 'Privileged Admin Workstation' -Value 'Production tenant access' -Color LemonChiffon -ValueColor $white -FontSize 25 -ValueFontSize 17 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Tenant' -Value 'Contoso Production' -Color $muted -ValueColor $white
    New-BGInfoValue -Name 'Session' -Value 'Privileged tools only' -Color $yellow -ValueColor $white
    New-BGInfoValue -Name 'Support' -Value 'SOC bridge +48 22 000 0000' -Color $muted -ValueColor $white
    New-BGInfoValue -Name 'Warning' -Value 'Do not browse, download, or test untrusted files here.' -Color $red -ValueColor $white

    New-BGInfoChart -Id 'admin-access-target' -Title 'Access posture' -Kind Bullet -Values 92 -Target 95 -RangeEnds 70,85 -Width 440 -Height 150 -Anchor BottomRight -OffsetX 32 -OffsetY 32 -LineColor $orange -TextColor $white -BackgroundColor $panel -Maximum 100 -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'admin-tool-state' -Title 'Core tools' -Kind Pictorial -Values 4,1 -Labels 'Ready','Attention' -Width 440 -Height 145 -Anchor BottomRight -OffsetX 32 -OffsetY 194 -Palette $green,$red -TextColor $white -BackgroundColor $panel -PictorialSymbol Square -PictorialColumns 5 -ShowDataLabels -Maximum 5 -ShowLatestValue:$false -NoHistory
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.Pattern.AdminWorkstation.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color $muted `
    -ValueColor $white `
    -ValueWrapWidth 430 `
    -TextPosition TopLeft `
    -SpaceX 42 `
    -SpaceY 42

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoValue -Name 'Build Agent 07' -Value 'Windows package validation' -Color LemonChiffon -ValueColor $white -FontSize 25 -ValueFontSize 17 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Pool' -Value 'Release-Windows' -Color $muted -ValueColor $white
    New-BGInfoValue -Name 'Last build' -Value 'PowerBGInfo #4821 passed' -Color $green -ValueColor $white
    New-BGInfoValue -Name 'Workspace' -Value 'Clean after every successful run' -Color $muted -ValueColor $white

    New-BGInfoChart -Id 'build-queue' -Title 'Queue depth' -Kind Area -Values 7,5,4,6,3,2,4,1,2,1 -Width 390 -Height 145 -Anchor BottomLeft -OffsetX 32 -OffsetY 32 -LineColor $cyan -FillColor $cyan -TextColor $white -BackgroundColor $panel -ShowGrid -GridLineCount 3 -NoHistory
    New-BGInfoChart -Id 'build-pass-target' -Title 'Test pass target' -Kind Bullet -Values 98 -Target 97 -RangeEnds 85,94 -Width 430 -Height 145 -Anchor BottomRight -OffsetX 32 -OffsetY 32 -LineColor $green -TextColor $white -BackgroundColor $panel -Maximum 100 -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'build-disk' -Title 'Workspace disk' -Kind Donut -Values 61,39 -Labels 'Used','Free' -ValueSuffix '%' -Width 330 -Height 220 -Anchor BottomRight -OffsetX 32 -OffsetY 190 -Palette $orange,$green -TextColor $white -BackgroundColor $panel -ShowLegend -ShowPointLegend -LegendPosition Right -ShowDataLabels -Maximum 100 -DonutCenterValue '61%' -DonutCenterLabel 'Used' -ShowLatestValue:$false -NoHistory
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.Pattern.BuildAgent.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color $muted `
    -ValueColor $white `
    -ValueWrapWidth 430 `
    -TextPosition TopLeft `
    -SpaceX 42 `
    -SpaceY 42

New-BGInfo -MonitorIndex 0 -Target File {
    New-BGInfoValue -Name 'Training Station 12' -Value 'GPO troubleshooting exercise' -Color LemonChiffon -ValueColor $white -FontSize 25 -ValueFontSize 17 -FontFamilyName 'Calibri'
    New-BGInfoValue -Name 'Phase' -Value 'Investigate policy precedence' -Color $muted -ValueColor $white
    New-BGInfoValue -Name 'Help' -Value 'https://lab.contoso.com/help' -Color $cyan -ValueColor $white
    New-BGInfoValue -Name 'Hint' -Value 'Start with gpresult and the linked OU path.' -Color $muted -ValueColor $white
    New-BGInfoValue -Name 'Reset' -Value 'Ask instructor before reverting snapshot.' -Color $orange -ValueColor $white

    New-BGInfoChart -Id 'training-steps' -Title 'Exercise steps' -Kind ProgressBar -Values 40 -Labels 'Complete' -Width 440 -Height 130 -Anchor BottomRight -OffsetX 32 -OffsetY 32 -LineColor $purple -TextColor $white -BackgroundColor $panel -Maximum 100 -NoProgressHandles -ShowDataLabels -ShowLatestValue:$false -NoHistory
    New-BGInfoChart -Id 'training-checkpoints' -Title 'Checkpoints' -Kind Pictorial -Values 2,3 -Labels 'Done','Open' -Width 440 -Height 145 -Anchor BottomRight -OffsetX 32 -OffsetY 178 -Palette $green,$blue -TextColor $white -BackgroundColor $panel -PictorialSymbol Diamond -PictorialColumns 5 -ShowDataLabels -Maximum 5 -ShowLatestValue:$false -NoHistory
} -FilePath $sampleImage `
    -ConfigurationDirectory $outputDirectory `
    -OutputFileName 'PowerBGInfo.Pattern.TrainingKiosk.jpg' `
    -WallpaperFit Fill `
    -BackgroundColor Black `
    -Color $muted `
    -ValueColor $white `
    -ValueWrapWidth 430 `
    -TextPosition TopLeft `
    -SpaceX 42 `
    -SpaceY 42
