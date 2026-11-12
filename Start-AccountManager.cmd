@echo off
set "ROOT=%~dp0"
powershell.exe -ExecutionPolicy Bypass -File "%ROOT%scripts\Run.ps1"
if errorlevel 1 pause
