function Get-MyWallpaperData {
    # Needs the RSAT ActiveDirectory module and a domain controller in reach.
    Import-Module ActiveDirectory -ErrorAction Stop
    $user = Get-ADUser -Identity $env:USERNAME -Properties Department, Title
    $computer = Get-ADComputer -Identity $env:COMPUTERNAME `
        -Properties Description, ManagedBy

    [pscustomobject] @{
        User = $user.Name
        Team = '{0} · {1}' -f $user.Department, $user.Title
        Role = $computer.Description
        Owner = if ($computer.ManagedBy) {
            (Get-ADObject -Identity $computer.ManagedBy).Name
        } else { 'not set' }
    }
}
