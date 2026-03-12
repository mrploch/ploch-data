$loc = Get-Location
# Ensure posh-git module is installed or updated
if (-not (Get-Module -ListAvailable -Name posh-git)) {
    Write-Output "posh-git module not found. Installing..."
    Install-Module posh-git -Scope CurrentUser -Force
} else {
    Write-Output "posh-git module is already installed. Updating..."
    Update-Module posh-git -Force
}


# Get current script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
# Go up one directory
$parentDir = Split-Path $scriptDir -Parent
$devFolder = Join-Path $parentDir 'mrploch-development'

if (-not (Test-Path $devFolder -PathType Container)) {
    Write-Output "'mrploch-development' folder not found. Cloning repository..."
    Set-Location $parentDir
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        Write-Error "Git is not installed or not in PATH."
        exit 1
    }
    Import-Module posh-git -ErrorAction SilentlyContinue
    git clone https://github.com/mrploch/mrploch-development.git mrploch-development
} else {
    Write-Output "'mrploch-development' folder exists. Pulling latest changes..."
    Set-Location $devFolder
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        Write-Error "Git is not installed or not in PATH."
        exit 1
    }
    Import-Module posh-git -ErrorAction SilentlyContinue
    git pull
}
Set-Location $loc