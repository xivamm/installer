@echo off
title TechInstaller
cd /d "%~dp0"

if exist "%~dp0TechInstaller.exe" (
    start "" "%~dp0TechInstaller.exe"
    exit
) else (
    echo [ERROR] TechInstaller.exe not found in %~dp0
    pause
    exit
)
