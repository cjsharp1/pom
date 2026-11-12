#Requires -Version 5.1
<#
.SYNOPSIS
    Builds Pom as a standalone single-file .exe and copies it to dist\Pom.exe.
.DESCRIPTION
    Produces a self-contained executable with no .NET runtime dependency.
    Double-click dist\Pom.exe to run, or pin/shortcut it anywhere.
.PARAMETER Desktop
    Also place a shortcut to the built exe on your Desktop.
#>
param(
    [switch]$Desktop
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$dist = Join-Path $root 'dist'

Write-Host 'Publishing Pom (self-contained, single file)...' -ForegroundColor Cyan

dotnet publish (Join-Path $root 'src\Pom.csproj') `
    -c Release `
    -o (Join-Path $root 'publish') `
    --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

$built = Join-Path $root 'publish\Pom.exe'
if (-not (Test-Path $built)) { throw "Expected output not found: $built" }

New-Item -ItemType Directory -Force -Path $dist | Out-Null
$target = Join-Path $dist 'Pom.exe'
Copy-Item $built $target -Force

$sizeMb = [math]::Round((Get-Item $target).Length / 1MB, 1)
Write-Host ''
Write-Host "Built: $target ($sizeMb MB)" -ForegroundColor Green
Write-Host 'Standalone - no .NET install needed. Double-click to run.' -ForegroundColor Green

if ($Desktop) {
    $lnk = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Pom.lnk'
    $shell = New-Object -ComObject WScript.Shell
    $sc = $shell.CreateShortcut($lnk)
    $sc.TargetPath = $target
    $sc.WorkingDirectory = $dist
    $sc.Description = 'Pom - Pomodoro timer'
    $sc.Save()
    Write-Host "Desktop shortcut created: $lnk" -ForegroundColor Green
}
