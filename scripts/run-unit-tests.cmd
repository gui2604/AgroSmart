@echo off
REM Executa testes unitarios sem alterar ExecutionPolicy do PowerShell
cd /d "%~dp0.."
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-unit-tests.ps1"
exit /b %ERRORLEVEL%
