@echo off
setlocal enabledelayedexpansion
title TechInstaller - 1-Click Push to GitHub
cd /d "%~dp0"

echo ============================================================
echo   TechInstaller - 1-Click GitHub Sync ^& Push
echo   Repository: https://github.com/xivamm/installer
echo ============================================================
echo.

echo [1/3] Checking git status...
git status --short
echo.

echo [2/3] Staging and committing changes...
git add .
git commit -m "Update TechInstaller: add portable driver booster download URL, sync assets, and update tools"
echo.

echo [3/3] Pushing to GitHub (origin main)...
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
