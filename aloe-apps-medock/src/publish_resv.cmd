@echo off

cd /d %~dp0

REM publish フォルダを完全に削除（注意：中身が全て消えます）
rmdir /s /q publish

REM 発行します

dotnet publish .\Aloe\Medock\Reservation\AloeMedockResvServer\AloeMedockResvServer.csproj -c Release -r win-x64 -o .\publish\AloeMedockResvServer
dotnet publish .\Aloe\Medock\Reservation\AloeMedockResvServerMonitor\AloeMedockResvServerMonitor.csproj -c Release -r win-x64 -o .\publish\AloeMedockResvServerMonitor
dotnet publish .\Aloe\Medock\Reservation\AloeMedockResvApp\AloeMedockResvApp.csproj -c Release -r win-x64 -o .\publish\AloeMedockResvApp

echo Completed.
pause
