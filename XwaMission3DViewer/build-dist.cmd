@echo off
setlocal

cd "%~dp0"

For %%a in (
"XwaMission3DViewer\bin\Release\net48\*.dll"
"XwaMission3DViewer\bin\Release\net48\*.exe"
"XwaMission3DViewer\bin\Release\net48\*.config"
) do (
xcopy /s /d "%%~a" dist\
)
