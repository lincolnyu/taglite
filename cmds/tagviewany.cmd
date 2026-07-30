@echo off
cd /d "%~dp0.."
dotnet run -- any . %*
cd /d %taglite_view%