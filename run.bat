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

:: Ensure output directory and executable exist
if exist "%~dp0output\TechInstaller.exe" (
    cd /d "%~dp0output"
    start "" "%~dp0output\TechInstaller.exe"
    exit /b
)

if exist "%~dp0TechInstaller.exe" (
    start "" "%~dp0TechInstaller.exe"
    exit /b
)

:: If executable not found, build it now
echo [INFO] TechInstaller.exe not found. Compiling with build.ps1...
powershell -ExecutionPolicy Bypass -File "%~dp0build.ps1"

if exist "%~dp0output\TechInstaller.exe" (
    cd /d "%~dp0output"
    start "" "%~dp0output\TechInstaller.exe"
    exit /b
) else (
    echo.
    echo [ERROR] Failed to find or compile TechInstaller.exe.
    pause
)
