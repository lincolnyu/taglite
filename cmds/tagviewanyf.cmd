@echo off
cd /d "%~dp0.."
dotnet run -- anyf . %*
cd /d %taglite_view%