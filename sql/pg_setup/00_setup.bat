CHCP 932

cd /d %~dp0

@ECHO OFF

ECHO PostgreSQL
ECHO DBおよびテーブルの作成を行います。
ECHO;
ECHO %APPDATA%\postgresql\pgpass.conf
ECHO localhost:5432:mydatabase:myuser:mypassword
ECHO 上記ファイルを設置するとパスワード入力を回避できます。
ECHO;

psql -V
ECHO;

SET dbhost=127.0.0.1
SET dbname=aloedb

ECHO データベース %dbname% とテーブルを作成します。

ECHO 既存データベースがある場合は、名前を変更します。
ECHO リネームしたデータベースを削除する場合は下記SQLを利用します。
ECHO SELECT 'DROP DATABASE ' || datname || ';' as command FROM pg_database WHERE datname LIKE 'aloedb%';

PAUSE

ECHO renamming database ...
SET now=%time: =0%
SET yyyymmddhhmmss=%date:/=%%now:~0,2%%now:~3,2%%now:~6,2%
SET sql="ALTER DATABASE %dbname% RENAME TO %dbname%_%yyyymmddhhmmss%;"
psql -h %dbhost% -d postgres -U postgres -w -c %sql%
ECHO;
ECHO;

ECHO データベースおよびテーブルを作成します。
PAUSE

ECHO creating database ...
SET sql2="CREATE DATABASE %dbname%;"
psql -h %dbhost% -d postgres -U postgres -w -c %sql2%

CALL 01_00_create_tables.bat

CALL 02_00_create_indexes.bat

CALL 03_00_create_triggers.bat

ECHO 初回実行時は拡張を有効にしてください。
ECHO postgresql.conf の書き換えを行い ext_create_extensions.sql の中身を手動で実行してください。
PAUSE
