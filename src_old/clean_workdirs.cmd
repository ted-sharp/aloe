@echo off

cd /d %~dp0

REM カレントディレクトリ以下の .vs, bin, obj, publish, logs, tmp フォルダを再帰的に削除
for /d /r %%d in (.vs bin obj publish, logs, tmp) do (
    if exist "%%d" (
        echo Deleting directory: "%%d"
        rd /s /q "%%d"
    )
)

REM カレントディレクトリ以下の *.nettrace ファイルを削除
for /r %%f in (*.nettrace) do (
    echo Deleting file: "%%f"
    del /q "%%f"
)

echo Completed.
pause
