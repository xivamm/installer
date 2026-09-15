# =====================================================================
# TechInstaller - Standalone Build Script
# Uses Windows built-in C# compiler (no Visual Studio or SDK required)
# =====================================================================

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
if (-not $root) { $root = Get-Location }

$srcDir = Join-Path $root "src"
$outDir = Join-Path $root "output"
$manifest = Join-Path $srcDir "app.manifest"
$outFile = Join-Path $outDir "TechInstaller.exe"

# Locate C# Compiler
$cscPaths = @(
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

$csc = $null
foreach ($path in $cscPaths) {
    if (Test-Path $path) {
        $csc = $path
        break
    }
}

if (-not $csc) {
    Write-Error "Could not find csc.exe in standard .NET Framework directories."
    exit 1
}

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  Building TechInstaller with native csc.exe     " -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Compiler : $csc" -ForegroundColor Gray
Write-Host "Source   : $srcDir" -ForegroundColor Gray
Write-Host "Output   : $outFile" -ForegroundColor Gray

if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

# Collect source files
$csFiles = Get-ChildItem -Path $srcDir -Filter "*.cs" | ForEach-Object { $_.FullName }
Write-Host "Compiling $($csFiles.Count) source files..." -ForegroundColor Yellow

$references = @(
    "System.dll",
    "System.Core.dll",
    "System.Drawing.dll",
    "System.Windows.Forms.dll",
    "System.Web.Extensions.dll",
    "System.IO.Compression.dll",
    "System.IO.Compression.FileSystem.dll"
)

$refArgs = $references | ForEach-Object { "/r:$_" }

$iconFile = Join-Path $srcDir "app.ico"

$compilerArgs = @(
    "/target:winexe",
    "/optimize+",
    "/platform:anycpu",
    "/win32manifest:`"$manifest`"",
    "/win32icon:`"$iconFile`"",
    "/out:`"$outFile`""
) + $refArgs + $csFiles

# Run compilation
$process = Start-Process -FilePath $csc -ArgumentList $compilerArgs -NoNewWindow -Wait -PassThru

if ($process.ExitCode -eq 0 -and (Test-Path $outFile)) {
    $sizeKB = [math]::Round(((Get-Item $outFile).Length / 1KB), 1)
    Write-Host "`n[SUCCESS] Build completed successfully!" -ForegroundColor Green
    Write-Host "Executable generated: $outFile ($sizeKB KB)" -ForegroundColor Green

    # Copy app.ico to output
    if (Test-Path $iconFile) {
        Copy-Item -Path $iconFile -Destination (Join-Path $outDir "app.ico") -Force
    }

    # Ensure scripts directory exists and copy custom.ps1
    $scriptsOut = Join-Path $outDir "scripts"
    if (-not (Test-Path $scriptsOut)) {
        New-Item -ItemType Directory -Path $scriptsOut -Force | Out-Null
    }
    $customPs1Src = Join-Path $root "scripts\custom.ps1"
    if (Test-Path $customPs1Src) {
        Copy-Item -Path $customPs1Src -Destination (Join-Path $scriptsOut "custom.ps1") -Force
    }

    # Copy cloud_apps.json and apps.json if in config
    $cloudConfig = Join-Path $root "config\cloud_apps.json"
    if (Test-Path $cloudConfig) {
        Copy-Item -Path $cloudConfig -Destination (Join-Path $outDir "cloud_apps.json") -Force
    }

    # Create default cache directory in output folder for USB copying convenience
    $cacheOut = Join-Path $outDir "cache"
    if (-not (Test-Path $cacheOut)) {
        New-Item -ItemType Directory -Path $cacheOut -Force | Out-Null
    }
} else {
    Write-Host "`n[ERROR] Build failed with exit code $($process.ExitCode)!" -ForegroundColor Red
    exit 1
}
