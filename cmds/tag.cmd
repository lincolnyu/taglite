@echo off
cd /d "%~dp0.."
dotnet run -- tag . %*