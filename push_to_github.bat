@echo off
setlocal enabledelayedexpansion
title TechInstaller - 1-Click Push to GitHub
cd /d "%~dp0"

echo ============================================================
echo   TechInstaller - 1-Click GitHub Sync ^& Push
echo   Repository: https://github.com/xivamm/installer
echo ============================================================
echo.

echo [1/4] Compiling latest binaries and packages...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1"
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Build failed. Aborting push.
    pause
    exit /b %ERRORLEVEL%
)
echo.

echo [2/4] Checking pending git changes...
git status --short
echo.

echo [3/4] Staging and committing changes...
git add .
git commit -m "Update TechInstaller: Cyber ASCII bootstrap with live animated loaders & synced portable packages"
echo.

echo [4/4] Pushing to GitHub (origin main)...
git push -u origin main
echo.

if %ERRORLEVEL% EQU 0 (
    echo ============================================================
    echo   [SUCCESS] Pushed to GitHub successfully!
    echo.
    echo   1-Click Run commands ready:
    echo     irm tinyurl.com/techinst ^| iex
    echo     irm tinyurl.com/xivam1 ^| iex
    echo ============================================================
) else (
    echo ============================================================
    echo   [NOTICE] Push encountered an issue or requires sign-in.
    echo   If GitHub prompted you in a browser window, please sign in.
    echo ============================================================
)

echo.
pause
