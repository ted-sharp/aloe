@echo off
setlocal enabledelayedexpansion

echo Creating junction links for all aloe-utils projects...
echo.

cd /d %~dp0

rem Create junction links for all aloe-utils projects
mklink /J "aloe-utils-commandline" "..\aloe-utils-commandline"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-commandline
) else (
    echo [FAILED] aloe-utils-commandline
)

mklink /J "aloe-utils-configuration-default" "..\aloe-utils-configuration-default"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-configuration-default
) else (
    echo [FAILED] aloe-utils-configuration-default
)

mklink /J "aloe-utils-configuration-json" "..\aloe-utils-configuration-json"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-configuration-json
) else (
    echo [FAILED] aloe-utils-configuration-json
)

mklink /J "aloe-utils-drawing" "..\aloe-utils-drawing"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-drawing
) else (
    echo [FAILED] aloe-utils-drawing
)

mklink /J "aloe-utils-drawing-wpf" "..\aloe-utils-drawing-wpf"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-drawing-wpf
) else (
    echo [FAILED] aloe-utils-drawing-wpf
)

mklink /J "aloe-utils-json" "..\aloe-utils-json"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-json
) else (
    echo [FAILED] aloe-utils-json
)

mklink /J "aloe-utils-logging-dump" "..\aloe-utils-logging-dump"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-logging-dump
) else (
    echo [FAILED] aloe-utils-logging-dump
)

mklink /J "aloe-utils-safeio" "..\aloe-utils-safeio"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-safeio
) else (
    echo [FAILED] aloe-utils-safeio
)

mklink /J "aloe-utils-text" "..\aloe-utils-text"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-text
) else (
    echo [FAILED] aloe-utils-text
)

mklink /J "aloe-utils-wafu-date" "..\aloe-utils-wafu-date"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-wafu-date
) else (
    echo [FAILED] aloe-utils-wafu-date
)

mklink /J "aloe-utils-wafu-jiscompat" "..\aloe-utils-wafu-jiscompat"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-wafu-jiscompat
) else (
    echo [FAILED] aloe-utils-wafu-jiscompat
)

mklink /J "aloe-utils-wafu-kansuji" "..\aloe-utils-wafu-kansuji"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-wafu-kansuji
) else (
    echo [FAILED] aloe-utils-wafu-kansuji
)

mklink /J "aloe-utils-wafu-romaji" "..\aloe-utils-wafu-romaji"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-wafu-romaji
) else (
    echo [FAILED] aloe-utils-wafu-romaji
)

mklink /J "aloe-utils-win32-sccommand" "..\aloe-utils-win32-sccommand"
if %ERRORLEVEL% EQU 0 (
    echo [OK] aloe-utils-win32-sccommand
) else (
    echo [FAILED] aloe-utils-win32-sccommand
)

rem Check for additional aloe-utils projects
if exist "..\aloe-utils-async" (
    mklink /J "aloe-utils-async" "..\aloe-utils-async"
    if %ERRORLEVEL% EQU 0 (
        echo [OK] aloe-utils-async
    ) else (
        echo [FAILED] aloe-utils-async
    )
)

echo.
echo Junction creation completed.
echo.
echo To remove junctions later, use: remove_junctions.cmd
pause