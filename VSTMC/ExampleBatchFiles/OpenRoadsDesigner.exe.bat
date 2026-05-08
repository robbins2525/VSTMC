@echo off
setlocal

set "SDKROOT=%~fs2"
if not "%SDKROOT:~-1%"=="\" set "SDKROOT=%SDKROOT%\"

set "SrcRoot=%SDKROOT%"

set "MS=%~fs1"
set "MSMD=%SDKROOT%"
set "MSMDE=%SDKROOT%"

pushd "%SDKROOT%"
call "%SDKROOT%OpenRoadsDesignerDeveloperShell.bat" "%~fs1" "%SDKROOT%"
popd

pushd "%~4"
bmake %~5
popd

endlocal
