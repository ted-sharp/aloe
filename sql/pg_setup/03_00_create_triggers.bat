@ECHO OFF
ECHO creating triggers ...
SET dbhost=127.0.0.1
SET dbname=aloedb
psql -h %dbhost% -d %dbname% -U postgres -w -f 03_01_create_log_triggers_sjis.sql
psql -h %dbhost% -d %dbname% -U postgres -w -f 03_02_create_cache_triggers_sjis.sql
psql -h %dbhost% -d %dbname% -U postgres -w -f 03_03_create_normalize_triggers_sjis.sql
ECHO;
