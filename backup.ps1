<#
.Synopsis
    A simple backup tool.
.Description
    A simple backup tool.
    Pulls files set in `include.psd1` and creates a backup zip archive.

    The backup file will be saved as `backup[DTG].zip` where [DTG] is the date-time group at the time of backup.
    
    The include data file structure is hashtable array.
    Each hashtable in the array must be structured with two keys: BaseDirectory and Paths.
    BaseDirectory is a string.
    Paths is a string array.
    
    The signature of the imported object is:
    
        Include : Hashtable [] =
            [|
                { BaseDirectory : string
                  Paths : string [] } 

                ...
            |]
#> 

Push-Location $PSScriptRoot

$ErrorActionPreference = 'Stop' 
$IncludePath = Join-Path $PSScriptRoot include.psd1 
$DayAge = { ((Get-Date) - $this.CreationTime).Days }

Write-Output "Using: $IncludePath" 
Write-Output 'Clean:'
Get-ChildItem -Filter *.zip |
    Add-Member -MemberType ScriptProperty -Name DayAge -Value $DayAge -PassThru |
    Where-Object DayAge -gt 60 -OutVariable ToClean |
    Remove-Item

$ToClean | Write-Output 


filter Get-AllPaths {
    $joinIncludePath = {
        $p, $shift = $args

        Join-Path -Path $p -ChildPath $_
    } 

    foreach ($include in $_.Include) {
        $include.Files.ForEach($joinIncludePath, $include.BaseDirectory)
    }
}


$Paths =
    Import-PowerShellDataFile $IncludePath | 
    Get-AllPaths
$Destination = "backup$( Get-Date -Format yyyyMMdd ).zip"
 
Write-Output 'Backup:' 

$Paths | Write-Output 

Write-Output "Destination: $Destination" 
Compress-Archive -Path $Paths -DestinationPath $Destination -Update 
Pop-Location
