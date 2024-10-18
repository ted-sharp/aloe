@ECHO OFF
ECHO creating indexes ...
SET dbhost=127.0.0.1
SET dbname=aloedb
psql -h %dbhost% -d %dbname% -U postgres -w -f 02_01_create_indexes_sjis.sql
ECHO;
