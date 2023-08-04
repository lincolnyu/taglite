@echo off
cd %~dp0
cd ..
dotnet run -- allf . %*
cd %taglite_view%