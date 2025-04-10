@echo off

cd /d %~dp0

REM カレントディレクトリ以下の .vs, bin, obj, publish フォルダを再帰的に削除

for /d /r %%d in (.vs bin obj publish) do (
    if exist "%%d" (
        echo Deleting directory: "%%d"
        rd /s /q "%%d"
    )
)

echo Completed.
pause
