@echo off
cd /d "%~dp0.."
dotnet run -- all . %*
cd /d %taglite_view%