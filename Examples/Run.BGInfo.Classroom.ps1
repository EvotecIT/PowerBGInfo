<#
    Lab or classroom wallpaper. The seat name is readable from the back of
    the room, students see when the machine resets, and the instructor sees
    lab progress at a glance.

    The data block returns plain values; the presentation block only reads
    them. Replace the samples with your lab schedule, timetable or LMS data.
#>
param(
    [ValidateSet('Light', 'Dark')]
    [string] $Theme = 'Light',
    [int] $Width = 1920,
    [int] $Height = 1080,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot 'Output')
)

Import-Module (Join-Path $PSScriptRoot '..\PowerBGInfo.psd1') -Force

#region Data: your logic goes here
function Get-ClassroomData {
    [pscustomobject] @{
        Course       = 'LAB B  ·  INTRODUCTION TO POWERSHELL'
        Seat         = 'LAB-B-14'
        Instructor   = 'Anna Novak'
        Wifi         = 'CAMPUS-LAB, sign in with your ID'
        MinutesLeft  = 35
        SessionTotal = 90
        Completed    = 3
        Exercises    = 5
        # Students who finished each exercise, from the LMS gradebook.
        ClassDone    = @(26, 24, 19, 9, 2)
    }
}

$fallback = [pscustomobject] @{
    Course = 'LAB'; Seat = $env:COMPUTERNAME; Instructor = 'unknown'
    Wifi = 'unknown'; MinutesLeft = 0; SessionTotal = 90; Completed = 0
    Exercises = 5; ClassDone = @(0)
}
$source = @{
    Name           = 'Classroom data'
    TimeoutSeconds = 10
    ScriptBlock    = ${function:Get-ClassroomData}
    Default        = $fallback
}
$lab = Invoke-BGInfoDataSource @source
#endregion

#region Presentation: reads only $lab
$scale = [Math]::Round($Height / 1080, 2)
$second = (New-BGInfoTheme Ember -Mode $Theme).SecondaryAccent
$look = New-BGInfoTheme Ember -Mode $Theme -Scale $scale
$top = [int](120 * $scale)

New-BGInfo -Theme $look -TextPosition TopCenter -SpaceY $top -Target File {
    New-BGInfoBackground -Width $Width -Height $Height

    New-BGInfoLabel -Name $lab.Course -Accent -FontSize ([int](18 * $scale)) `
        -FontWeight 700
    New-BGInfoLabel -Name $lab.Seat -FontSize ([int](150 * $scale)) `
        -FontWeight 700
    New-BGInfoLabel -Name 'This PC resets every night at 02:00.' `
        -FontSize ([int](30 * $scale)) -FontWeight 600
    New-BGInfoLabel -Name 'Save your work to OneDrive before you leave.' `
        -Muted -FontSize ([int](24 * $scale))
    New-BGInfoLabel -Name ' ' -FontSize ([int](10 * $scale))
    New-BGInfoValue -Name 'Instructor' -Value $lab.Instructor
    New-BGInfoValue -Name 'Wi-Fi' -Value $lab.Wifi
    New-BGInfoValue -Name 'Need a hand?' -Value 'Raise your hand or call 4357'

    # Three fixed-width panels sit centered in the bottom band; the gauge
    # turns amber in the last 15 minutes.
    $band = @{
        Region    = 'Bottom'
        Height    = [int](250 * $scale)
        NoHistory = $true
    }
    New-BGInfoChart @band -Id 'time' -Width ([int](360 * $scale)) `
        -Title 'Session time left' -Kind Gauge -Values $lab.MinutesLeft `
        -Maximum $lab.SessionTotal -ValueSuffix ' min' -ShowLatestValue:$false `
        -WarningThreshold 15 -CriticalThreshold 5 -ThresholdDirection Below
    New-BGInfoChart @band -Id 'exercises' -Width ([int](420 * $scale)) `
        -Title 'Your exercises' -Kind Pictorial -Values $lab.Completed `
        -Labels 'Completed' -Palette $second -PictorialSymbol Star `
        -PictorialColumns $lab.Exercises -Maximum $lab.Exercises `
        -ShowDataLabels -ShowLatestValue:$false
    New-BGInfoChart @band -Id 'class' -Width ([int](360 * $scale)) `
        -Title "Class · exercises 1–$($lab.Exercises)" -Kind Bar `
        -Values $lab.ClassDone -LineColor $second -ShowGrid -GridLineCount 3 `
        -ShowLatestValue:$false
} -ConfigurationDirectory $OutputDirectory `
    -OutputFileName "classroom-$($Theme.ToLower()).png"
#endregion
