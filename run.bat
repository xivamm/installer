@echo off
title TechInstaller
cd /d "%~dp0"

if exist "%~dp0output\TechInstaller.exe" (
    start "" "%~dp0output\TechInstaller.exe"
    exit
)

if exist "%~dp0TechInstaller.exe" (
    start "" "%~dp0TechInstaller.exe"
    exit
)

echo [INFO] TechInstaller.exe not found. Building now...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1"

if exist "%~dp0output\TechInstaller.exe" (
    start "" "%~dp0output\TechInstaller.exe"
    exit
) else (
    echo [ERROR] Build failed or TechInstaller.exe was not created.
    pause
    exit
)
