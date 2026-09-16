# =====================================================================
# TechInstaller - 1-Click Bootstrap Launcher v2.0
# Run on any Windows PC via PowerShell:
# irm tinyurl.com/techinst | iex
# or: irm tinyurl.com/xivam1 | iex
# =====================================================================

$ErrorActionPreference = "Stop"

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls

# Target directory determination
$currentDrive = (Get-Location).Drive
$targetDir = "C:\TechInstaller"
if ($currentDrive -and $currentDrive.DriveType -eq "Removable") {
    $targetDir = Join-Path (Get-Location).Path "TechInstaller"
}

if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
}

$scriptsDir = Join-Path $targetDir "scripts"
if (-not (Test-Path $scriptsDir)) {
    New-Item -ItemType Directory -Path $scriptsDir -Force | Out-Null
}

# Clear and display Cyber/Technician ASCII Art Banner
Write-Host ""
Write-Host "  +==================================================================+" -ForegroundColor Cyan
Write-Host "  |   " -NoNewline -ForegroundColor Cyan
Write-Host '______           __    ____           __        ____          ' -NoNewline -ForegroundColor Yellow
Write-Host " |" -ForegroundColor Cyan

Write-Host "  |  " -NoNewline -ForegroundColor Cyan
Write-Host '/_  __/__  _____ / /_  /  _/___  _____/ /_____ _/ / /__  _____ ' -NoNewline -ForegroundColor Yellow
Write-Host " |" -ForegroundColor Cyan

Write-Host "  |   " -NoNewline -ForegroundColor Cyan
Write-Host '/ / / _ \/ ___// __ \ / // __ \/ ___/ __/ __ `/ / / _ \/ ___/ ' -NoNewline -ForegroundColor Yellow
Write-Host " |" -ForegroundColor Cyan

Write-Host "  |  " -NoNewline -ForegroundColor Cyan
Write-Host '/ / /  __/ /__ / / / // // / / (__  ) /_/ /_/ / / /  __/ /     ' -NoNewline -ForegroundColor Yellow
Write-Host " |" -ForegroundColor Cyan

Write-Host "  | " -NoNewline -ForegroundColor Cyan
Write-Host '/_/  \___/\___//_/ /_/___/_/ /_/____/\__/\__,_/_/_/\___/_/      ' -NoNewline -ForegroundColor Yellow
Write-Host " |" -ForegroundColor Cyan

Write-Host "  |                                                                  |" -ForegroundColor Cyan
Write-Host "  |   " -NoNewline -ForegroundColor Cyan
Write-Host "[>] ULTIMATE PC TECHNICIAN DEPLOYMENT TOOLKIT" -NoNewline -ForegroundColor White
Write-Host "                  |" -ForegroundColor Cyan

Write-Host "  |   " -NoNewline -ForegroundColor Cyan
Write-Host "[*] 1-Click Bootstrap | XIVAM Edition v2.0" -NoNewline -ForegroundColor Green
Write-Host "                     |" -ForegroundColor Cyan
Write-Host "  +==================================================================+" -ForegroundColor Cyan
Write-Host ""

$arch = if ([System.Environment]::Is64BitOperatingSystem) { "64-bit" } else { "32-bit" }
Write-Host "  [i] System Environment:" -ForegroundColor DarkGray
Write-Host "      * Target Directory : " -NoNewline -ForegroundColor DarkGray
Write-Host "$targetDir" -ForegroundColor White
Write-Host "      * Host Platform    : " -NoNewline -ForegroundColor DarkGray
Write-Host "$([System.Environment]::OSVersion.VersionString) ($arch)" -ForegroundColor White
Write-Host "      * Cloud Endpoint   : " -NoNewline -ForegroundColor DarkGray
Write-Host "github.com/xivamm/installer" -ForegroundColor White
Write-Host ""

Write-Host "  --------------------------------------------------------------------" -ForegroundColor DarkCyan
Write-Host "   >> Synchronizing Application Packages & Dependencies" -ForegroundColor Yellow
Write-Host "  --------------------------------------------------------------------" -ForegroundColor DarkCyan
Write-Host ""

function Render-ProgressBar {
    param([int]$Pct, [int]$Width = 14)
    if ($Pct -lt 0) { $Pct = 0 }
    if ($Pct -gt 100) { $Pct = 100 }
    $filled = [int](($Pct / 100.0) * $Width)
    $empty = $Width - $filled
    return "[" + ("=" * $filled) + ("-" * $empty) + "]"
}

function Download-ComponentWithLoader {
    param(
        [string]$Url,
        [string]$Dest,
        [string]$Title,
        [int]$Step,
        [int]$Total
    )

    $spinner = @('|', '/', '-', '\')
    $spinIdx = 0

    $req = [System.Net.HttpWebRequest]::Create($Url)
    $req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
    $req.Timeout = 25000

    try {
        $resp = $req.GetResponse()
        $totalBytes = $resp.ContentLength
        $stream = $resp.GetResponseStream()
        $fs = [System.IO.File]::Create($Dest)

        $buffer = New-Object byte[] 16384
        $totalRead = 0
        $lastUpdate = [DateTime]::MinValue

        while (($read = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
            $fs.Write($buffer, 0, $read)
            $totalRead += $read

            $now = [DateTime]::Now
            if (($now - $lastUpdate).TotalMilliseconds -ge 50) {
                $lastUpdate = $now
                $frame = $spinner[$spinIdx % $spinner.Count]
                $spinIdx++

                $pctVal = 0
                $bar = "[--------------]"
                if ($totalBytes -gt 0) {
                    $pctVal = [int](($totalRead / $totalBytes) * 100)
                    $bar = Render-ProgressBar -Pct $pctVal -Width 14
                }
                $size = "$([math]::Round($totalRead / 1KB, 0)) KB"
                Write-Host -NoNewline "`r   $frame [$Step/$Total] Fetching $Title $bar $pctVal% ($size)    " -ForegroundColor Cyan
            }
        }

        $fs.Close()
        $stream.Close()
        $resp.Close()

        $finalSize = ""
        $len = (Get-Item $Dest).Length
        if ($len -gt 1MB) {
            $finalSize = "$([math]::Round($len / 1MB, 2)) MB"
        } else {
            $finalSize = "$([math]::Round($len / 1KB, 1)) KB"
        }

        Write-Host "`r   [OK] [$Step/$Total] $Title - Verified ($finalSize)                              " -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "`r   [FAIL] [$Step/$Total] $Title - Error: $($_.Exception.Message)                 " -ForegroundColor Red
        return $false
    }
}

$baseUrl = "https://raw.githubusercontent.com/xivamm/installer/main"

$manifest = @(
    @{ Title = "TechInstaller.exe"; Url = "$baseUrl/output/TechInstaller.exe"; Dest = (Join-Path $targetDir "TechInstaller.exe") },
    @{ Title = "apps.json"; Url = "$baseUrl/output/apps.json"; Dest = (Join-Path $targetDir "apps.json") },
    @{ Title = "cloud_apps.json"; Url = "$baseUrl/output/cloud_apps.json"; Dest = (Join-Path $targetDir "cloud_apps.json") },
    @{ Title = "scripts/custom.ps1"; Url = "$baseUrl/scripts/custom.ps1"; Dest = (Join-Path $scriptsDir "custom.ps1") }
)

$allSucceeded = $true
for ($i = 0; $i -lt $manifest.Count; $i++) {
    $item = $manifest[$i]
    $ok = Download-ComponentWithLoader -Url $item.Url -Dest $item.Dest -Title $item.Title -Step ($i + 1) -Total $manifest.Count
    if (-not $ok) {
        $allSucceeded = $false
    }
}

Write-Host ""
Write-Host "  --------------------------------------------------------------------" -ForegroundColor DarkCyan

$exe = Join-Path $targetDir "TechInstaller.exe"
if (Test-Path $exe) {
    Write-Host "  [+] All core deployment components verified." -ForegroundColor Green
    Write-Host ""
    Write-Host "  ====================================================================" -ForegroundColor Cyan
    Write-Host "   [>>] Launching TechInstaller with Administrator Privileges...      " -ForegroundColor Yellow
    Write-Host "  ====================================================================" -ForegroundColor Cyan
    Write-Host ""

    $pulse = @('[ *   ]', '[  *  ]', '[   * ]', '[  *  ]', '[ OK! ]')
    foreach ($p in $pulse) {
        Write-Host -NoNewline "`r   >> Initializing GUI Process $p " -ForegroundColor Cyan
        Start-Sleep -Milliseconds 100
    }
    Write-Host "`r   [DONE] TechInstaller GUI initialized and running!               " -ForegroundColor Green
    Write-Host ""

    Start-Process -FilePath $exe -Verb RunAs
} else {
    Write-Host "  [!] ERROR: TechInstaller.exe could not be found at: $exe" -ForegroundColor Red
}
