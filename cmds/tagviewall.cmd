@echo off
cd %~dp0
cd ..
dotnet run -- all . %*
cd %taglite_view%