@echo off
if "%taglite_view%" == "" (
  echo "%%taglite_view%% not defined"
) else (
  if exist "%taglite_view%\NUL" (
    cd %taglite_view%
    del * /s /f /q
  )
)
