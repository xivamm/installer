@echo off
title TECH INSTALLER - Windows Post-Install & Tech Toolbox
cd /d "%~dp0"

:: Check for Administrator elevation
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [INFO] Requesting Administrator privileges...
    powershell -Command "Start-Process cmd.exe -ArgumentList '/c \"\"%~f0\"\"' -Verb RunAs"
    exit /b
)

if exist "%~dp0TechInstaller.exe" (
    start "" "%~dp0TechInstaller.exe"
    exit /b
) else (
    echo [ERROR] TechInstaller.exe not found in %~dp0
    pause
)
