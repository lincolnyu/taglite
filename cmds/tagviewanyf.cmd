@echo off
cd %~dp0
cd ..
dotnet run -- anyf . %*
cd %taglite_view%