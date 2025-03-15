@echo off

REM カレントディレクトリ以下の .vs, bin, obj フォルダを再帰的に削除

for /d /r %%d in (.vs bin obj) do (
    if exist "%%d" (
        echo Deleting directory: "%%d"
        rd /s /q "%%d"
    )
)

echo Completed.
pause
