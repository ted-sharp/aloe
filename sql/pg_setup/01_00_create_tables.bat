@ECHO OFF
ECHO creating tables ...
SET dbhost=127.0.0.1
SET dbname=aloedb
REM psql -h %dbhost% -d %dbname% -U postgres -w -f 01_01_create_schema_sjis.sql
REM psql -h %dbhost% -d %dbname% -U postgres -w -f 01_02_create_audit_tables_sjis.sql
psql -h %dbhost% -d %dbname% -U postgres -w -f 01_03_create_appt_tables_sjis.sql
ECHO;
