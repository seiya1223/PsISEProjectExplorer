if ($PSVersionTable.PSVersion.Major -lt 3) {
	Write-Warning "PsISEProjectExplorer requires Powershell 3.0 or above"
	return
}

$currentDir = Split-Path -parent $MyInvocation.MyCommand.Path

$docDir = [Environment]::getfolderpath("mydocuments")
$moduleDir = "$docDir\WindowsPowerShell\Modules\PsISEProjectExplorer"
$profileFile = "$docDir\WindowsPowerShell\Microsoft.PowerShellISE_profile.ps1"

$copyToModules = Read-Host 'Install PsISEProjectExplorer to your Modules directory [y/n]?'
if ($copyToModules -ieq 'y') {
	if (!(Test-Path $moduleDir)) {
		Write-Host "Creating directory '$moduleDir'..." -NoNewline
		New-Item -Path $moduleDir -ItemType Directory | Out-Null
        Write-Host "OK"
	}
	Write-Host "Unblocking PSISEProjectExplorer files..." -NoNewLine
	Get-ChildItem (Join-Path $currentDir "PsISEProjectExplorer") | Unblock-File
	Write-Host "OK"

	Write-Host "Copying PSISEProjectExplorer files to '$moduleDir'..." -NoNewline
	Copy-Item -Path (Join-Path $currentDir "PsISEProjectExplorer\*") -Destination $moduleDir -Recurse -Force
    Write-Host "OK"
} else {
	return
}

Write-Host ""

$installToProfile = Read-Host 'Install PsISEProjectExplorer to ISE Profile (will start when ISE starts) [y/n]?'

if ($installToProfile -ieq 'y') {
	if (!(Test-Path $profileFile)) {
		Write-Host "Creating file '$profileFile'..." -NoNewline
		New-Item -Path $profileFile -ItemType file | Out-Null
        Write-Host "OK"
		$contents = ""
	} else {
		Write-Host "Reading file '$profileFile'..." -NoNewLine
		$contents = Get-Content -Path $profileFile | Out-String
        Write-Host "OK"
	}

	$importModule = "Import-Module PsISEProjectExplorer"

	if ($contents -inotmatch $importModule) {
		Write-Host "Adding '$importModule'..." -NoNewLine
		Add-Content -Path $profileFile -Value $importModule | Out-Null
        Write-Host "OK"
	} else {
		Write-Host "Import command for PSIseProjectExplorer already exists in profile file."
	}
}
