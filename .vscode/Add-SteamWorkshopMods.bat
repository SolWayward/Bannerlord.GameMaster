@echo off
REM Batch wrapper for Add-SteamWorkshopMods.ps1
REM This allows easy execution by double-clicking or running from command prompt

REM Change to the script's directory
cd /d "%~dp0"

echo.
echo ========================================
echo Steam Workshop Mod Scanner for BLGM
echo ========================================
echo.
echo Running PowerShell script...
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0Add-SteamWorkshopMods.ps1"

echo.
echo ========================================
echo.
echo Press any key to exit...
pause >nul
