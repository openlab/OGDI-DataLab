param([parameter(Mandatory=$true,HelpMessage="Enter the Build Configuration for the Package [Debug | Release]")]$buildConfig,[parameter(Mandatory=$true,HelpMessage="Enter the Azure Configuration for the Package (leave blank for default)")]$azureConfig)

$azurePath = Convert-Path -Path $env:programfiles'\Windows Azure SDK\v1.6\bin'
$env:Path = $env:Path + ";" + $azurePath
$defaultConfigurationFileName = "ServiceConfiguration.cscfg"
$configurationFileNameFormat = "ServiceConfiguration.{0}.cscfg"

if($azureConfig)
{ 
    $ServiceConfig = $configurationFileNameFormat -f $azureConfig
    $InteractiveSdkConfig = $configurationFileNameFormat -f $azureConfig
}
else
{ 
    $ServiceConfig = $defaultConfigurationFileName 
    $InteractiveSdkConfig = $defaultConfigurationFileName
}

## Commands -- ticks (`) are used to escape characters in Powershell
$PackService = 'cspack DataService\Services\ServiceDefinition.csdef /role:Services_WebRole`;DataService\Services_WebRole /sites:Services_WebRole`;Web`;DataService\Services_WebRole /out:Deployment\Service\OGDI_Service.cspkg'
$PackInteractiveSdk = 'cspack InteractiveSdk\InteractiveSdk.Mvc\ServiceDefinition.csdef /role:InteractiveSdk.WorkerRole`;InteractiveSdk\InteractiveSdk.WorkerRole\bin\'+$buildconfig+'`;InteractiveSdk.WorkerRole.dll /role:InteractiveSdk.WebRole`;"InteractiveSdk\Interactive Sdk.Mvc_WebRole" /sites:InteractiveSdk.WebRole`;Web`;"InteractiveSdk\Interactive Sdk.Mvc_WebRole" /out:Deployment\InteractiveSdk\OGDI_InteractiveSDK.cspkg'

## Create Deployment Directory
New-Item -Name Deployment\Service -ItemType Directory -ErrorAction SilentlyContinue
New-Item -Name Deployment\InteractiveSdk -ItemType Directory -ErrorAction SilentlyContinue

## Move Configuration Files to Deployment Directory
Copy-Item -Path "DataService\Services\$ServiceConfig" -Destination "Deployment\Service\$ServiceConfig"
Copy-Item -Path "InteractiveSdk\InteractiveSdk.Mvc\$InteractiveSdkConfig" -Destination "Deployment\InteractiveSdk\$InteractiveSdkConfig"

## Create Cloud Service Package
Invoke-Expression -Command $PackService
Invoke-Expression -Command $PackInteractiveSdk