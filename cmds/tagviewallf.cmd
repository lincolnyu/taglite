@echo off
cd /d "%~dp0.."
dotnet run -- allf . %*
cd /d %taglite_view%