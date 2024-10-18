@ECHO OFF
ECHO creating tables ...
SET dbhost=127.0.0.1
SET dbname=aloedb
psql -h %dbhost% -d %dbname% -U postgres -w -f 01_01_create_audit_tables_sjis.sql
psql -h %dbhost% -d %dbname% -U postgres -w -f 01_02_create_reservation_tables_sjis.sql
ECHO;
