@echo off
setlocal

cd "%~dp0"

For %%a in (
"XwaMission3DViewer\bin\Release\net45\*.dll"
"XwaMission3DViewer\bin\Release\net45\*.exe"
"XwaMission3DViewer\bin\Release\net45\*.config"
) do (
xcopy /s /d "%%~a" dist\
)
