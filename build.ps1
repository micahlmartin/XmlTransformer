param (
	[System.Collections.Hashtable]$properties = @{}
)
$rootPath = Split-Path $MyInvocation.MyCommand.Definition -Parent

Import-Module $rootPath\tools\psake\psake.psm1

Invoke-psake $rootPath\default.ps1 -properties $properties

Remove-Module psake