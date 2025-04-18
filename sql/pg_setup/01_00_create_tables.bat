@ECHO OFF
ECHO creating tables ...
SET dbhost=127.0.0.1
SET dbname=aloedb
psql -h %dbhost% -d %dbname% -U postgres -w -f 01_01_create_schema_sjis.sql
psql -h %dbhost% -d %dbname% -U postgres -w -f 01_02_create_audit_tables_sjis.sql
psql -h %dbhost% -d %dbname% -U postgres -w -f 01_03_create_reservation_tables_sjis.sql
psql -h %dbhost% -d %dbname% -U postgres -w -f 01_04_create_ext_tables_sjis.sql
psql -h %dbhost% -d %dbname% -U postgres -w -f 01_05_create_sk_tables_sjis.sql
ECHO;
