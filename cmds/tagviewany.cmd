@echo off
cd %~dp0
cd ..
dotnet run -- any . %*
cd %taglite_view%