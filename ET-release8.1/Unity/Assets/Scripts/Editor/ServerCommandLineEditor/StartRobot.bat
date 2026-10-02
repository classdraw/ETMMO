@echo off
chcp 936 >nul
cd /d "%~dp0"
title ET Robot Process=2
echo.
echo [1] Unity ServerTools start game server in another window first
echo [2] Unity login and enter map first (aa01 on map 10001)
echo [3] Then type: Run 3 10 10001
echo.
App.exe --Process=2 --Console=1 --Develop=__DEVELOP__ --LogLevel=1 --StartConfig=__STARTCONFIG__
echo.
echo App exited.
pause >nul
