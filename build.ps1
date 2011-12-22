param([parameter(Mandatory=$true,HelpMessage="Enter the Build Configuration for the Package [Debug | Release]")]$buildConfig)

## Add MSBuild Dir to Path
$NETPath = Convert-Path -Path $env:windir'\Microsoft.NET\Framework\v4.0.30319\'
$env:Path = $env:Path + ";" + $NETPath

## Commands
$BuildOGDI = "MSBuild.exe ogdi.sln /t:Rebuild /p:Configuration=$buildConfig"

Invoke-Expression -Command $BuildOGDI