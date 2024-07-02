@echo off
cd %taglite_view%
if %errorlevel% neq 0 exit /b %errorlevel%
del * /s /f /q