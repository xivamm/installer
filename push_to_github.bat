@echo off
title Push TechInstaller to GitHub
cd /d "%~dp0"
echo =======================================================
echo   Pushing TechInstaller to GitHub (xivamm/installer)
echo =======================================================
echo.
git push -u origin main
echo.
if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] Pushed to GitHub successfully!
) else (
    echo [NOTICE] If prompted by GitHub, please sign in via your browser.
)
echo.
pause
