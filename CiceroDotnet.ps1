<#
.SYNOPSIS
    Builds and runs the Melissa Cicero Cloud API .NET sample.

.DESCRIPTION
    This script builds CiceroDotnet with dotnet publish, then runs the
    resulting executable, passing along the license and (if supplied) the location fields.

    Overall flow:
      1. Resolve the license (parameter, prompt, or MD_LICENSE environment variable).
      2. Publish CiceroDotnet in Release configuration to
         .\CiceroDotnet\Build.
      3. Run the built executable: one-shot mode if -lat or -long was supplied,
         otherwise interactive mode (the .NET program prompts for each field).

.PARAMETER lat
    Latitude of the location.

.PARAMETER long
    Longitude of the location.

.PARAMETER location
    Address or place to search.

.PARAMETER max
    Maximum number of results to return.

.PARAMETER license
    License string. Resolved in this order:
      1. This parameter.
      2. An interactive prompt, if the parameter was not supplied.
      3. The MD_LICENSE environment variable, if the prompt was left blank.
    Note that the environment variable is the last resort, not the first: running
    without -license always prompts, even when MD_LICENSE is set.

.PARAMETER quiet
    Accepted for parity with other sample scripts; not currently used to suppress output.

.EXAMPLE
    .\CiceroDotnet.ps1 -license "your-license"

.EXAMPLE
    .\CiceroDotnet.ps1 -lat "33.637562" -long "-117.606887" -location "22382 Avenida Empresa, Rancho Santa Margarita, CA" -max "3" -license "your-license"
#>

######################### Parameters ##########################
param(
    $lat = '', 
    $long = '',
    $location = '',
    $max = '',
    $license = '', 
    [switch]$quiet = $false
    )

# Uses the location of the .ps1 file 
$CurrentPath = $PSScriptRoot
Set-Location $CurrentPath
$ProjectPath = "$CurrentPath\CiceroDotnet"
$BuildPath = "$ProjectPath\Build"

If (!(Test-Path $BuildPath)) {
  New-Item -Path $ProjectPath -Name 'Build' -ItemType "directory"
}

########################## Main ############################
Write-Host "`n===================================== Melissa Cicero Cloud API ===================================`n"

# Get license (either from parameters or user input)
if ([string]::IsNullOrEmpty($license) ) {
  $license = Read-Host "Please enter your license string"
}

# Check for License from Environment Variables 
if ([string]::IsNullOrEmpty($license) ) {
  $license = $env:MD_LICENSE 
}

if ([string]::IsNullOrEmpty($license)) {
  Write-Host "`nLicense String is invalid!"
  Exit
}

# Start program
# Build project
Write-Host "`n================================= BUILD PROJECT ================================"

dotnet publish -f="net7.0" -c Release -o $BuildPath CiceroDotnet\CiceroDotnet.csproj

# Run project
# No latitude or longitude supplied -> run interactively; otherwise pass every field
# through for one-shot mode.
if ([string]::IsNullOrEmpty($lat) -and [string]::IsNullOrEmpty($long)) {
  dotnet $BuildPath\CiceroDotnet.dll --license $license 
}
else {
  dotnet $BuildPath\CiceroDotnet.dll --license $license --lat $lat --long $long --location $location --max $max
}
