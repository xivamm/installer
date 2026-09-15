# =====================================================================
# TechInstaller - 1-Click Bootstrap Launcher
# Run this on any Windows PC with PowerShell:
# irm tinyurl.com/techinst | iex
# (Alternative full URL: irm https://raw.githubusercontent.com/xivamm/installer/main/bootstrap.ps1 | iex)
# =====================================================================

$ErrorActionPreference = "Stop"

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "   ⚡ TechInstaller - 1-Click Bootstrap Launcher     " -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

# Determine destination folder (if running from a USB drive, use current drive, else C:\TechInstaller)
$currentDrive = (Get-Location).Drive
$targetDir = "C:\TechInstaller"
if ($currentDrive -and $currentDrive.DriveType -eq "Removable") {
    $targetDir = Join-Path (Get-Location).Path "TechInstaller"
}

Write-Host "Destination: $targetDir" -ForegroundColor Gray
if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
}

$scriptsDir = Join-Path $targetDir "scripts"
if (-not (Test-Path $scriptsDir)) {
    New-Item -ItemType Directory -Path $scriptsDir -Force | Out-Null
}

$baseUrl = "https://raw.githubusercontent.com/xivamm/installer/main"

$files = @(
    @{ Name = "TechInstaller.exe"; Url = "$baseUrl/output/TechInstaller.exe"; Dest = (Join-Path $targetDir "TechInstaller.exe") },
    @{ Name = "apps.json"; Url = "$baseUrl/output/apps.json"; Dest = (Join-Path $targetDir "apps.json") },
    @{ Name = "cloud_apps.json"; Url = "$baseUrl/output/cloud_apps.json"; Dest = (Join-Path $targetDir "cloud_apps.json") },
    @{ Name = "scripts/custom.ps1"; Url = "$baseUrl/scripts/custom.ps1"; Dest = (Join-Path $scriptsDir "custom.ps1") }
)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls

foreach ($f in $files) {
    Write-Host "Downloading $($f.Name)..." -ForegroundColor Yellow
    try {
        $wc = New-Object Net.WebClient
        $wc.Headers.Add("User-Agent", "Mozilla/5.0")
        $wc.DownloadFile($f.Url, $f.Dest)
        Write-Host "  -> OK" -ForegroundColor Green
    } catch {
        Write-Host "  -> Failed downloading $($f.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

$exe = Join-Path $targetDir "TechInstaller.exe"
if (Test-Path $exe) {
    Write-Host "`n[SUCCESS] Launching TechInstaller as Administrator..." -ForegroundColor Green
    Start-Process -FilePath $exe -Verb RunAs
} else {
    Write-Host "`n[ERROR] TechInstaller.exe not found at $exe" -ForegroundColor Red
}
