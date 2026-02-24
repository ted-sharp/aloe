@echo off
setlocal enabledelayedexpansion

echo Removing junction links for all aloe-utils projects...
echo.

cd /d %~dp0

rem Remove junction links for all aloe-utils projects
set "PROJECTS=aloe-utils-commandline aloe-utils-configuration-default aloe-utils-configuration-json aloe-utils-drawing aloe-utils-drawing-wpf aloe-utils-json aloe-utils-logging-dump aloe-utils-safeio aloe-utils-text aloe-utils-wafu-date aloe-utils-wafu-jiscompat aloe-utils-wafu-kansuji aloe-utils-wafu-romaji aloe-utils-win32-sccommand aloe-utils-async"

for %%p in (%PROJECTS%) do (
    if exist "%%p" (
        rem Check if it's a junction
        dir "%%p" | findstr /C:"<JUNCTION>" >nul
        if !ERRORLEVEL! EQU 0 (
            rmdir "%%p"
            if !ERRORLEVEL! EQU 0 (
                echo [REMOVED] %%p
            ) else (
                echo [FAILED] %%p
            )
        ) else (
            echo [SKIP] %%p ^(not a junction^)
        )
    ) else (
        echo [NOT FOUND] %%p
    )
)

echo.
echo Junction removal completed.
pause