-- Project Name : aloe_reservation_grid
-- Date/Time    : 2025/04/15 11:47:37
-- Author       : user
-- RDBMS Type   : PostgreSQL
-- Application  : A5:SQL Mk-2

/*
  << 注意！！ >>
  BackupToTempTable, RestoreFromTempTable疑似命令が付加されています。
  これにより、drop table, create table 後もデータが残ります。
  この機能は一時的に $$TableName のような一時テーブルを作成します。
  この機能は A5:SQL Mk-2でのみ有効であることに注意してください。
*/

-- sk.ssmix_sources
-- * BackupToTempTable
DROP TABLE if exists sk.ssmix_sources CASCADE;

-- * RestoreFromTempTable
CREATE TABLE sk.ssmix_sources (
  source_id UUID DEFAULT uid_generate_v4() NOT NULL
  , pt_id integer DEFAULT 0 NOT NULL
  , source_file TEXT DEFAULT '' NOT NULL
  , section_type TEXT DEFAULT '' NOT NULL
  , created_at timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
) ;

CREATE UNIQUE INDEX ssmix_sources_PKI
  ON sk.ssmix_sources(source_id);

ALTER TABLE sk.ssmix_sources
  ADD CONSTRAINT ssmix_sources_PKC PRIMARY KEY (source_id);

-- sk.medical_embeddings
-- * BackupToTempTable
DROP TABLE if exists sk.medical_embeddings CASCADE;

-- * RestoreFromTempTable
CREATE TABLE sk.medical_embeddings (
  embedding_id UUID DEFAULT uid_generate_v4() NOT NULL
  , pt_id integer DEFAULT 0 NOT NULL
  , source_id UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , source_type TEXT DEFAULT 'ssmix' NOT NULL
  , content TEXT DEFAULT '' NOT NULL
  , embedding VECTOR(1536) NOT NULL
  , created_at timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
) ;

CREATE UNIQUE INDEX medical_embeddings_PKI
  ON sk.medical_embeddings(embedding_id);

ALTER TABLE sk.medical_embeddings
  ADD CONSTRAINT medical_embeddings_PKC PRIMARY KEY (embedding_id);

COMMENT ON TABLE sk.ssmix_sources IS 'sk.ssmix_sources';
COMMENT ON COLUMN sk.ssmix_sources.source_id IS 'source_id';
COMMENT ON COLUMN sk.ssmix_sources.pt_id IS 'pt_id';
COMMENT ON COLUMN sk.ssmix_sources.source_file IS 'source_file';
COMMENT ON COLUMN sk.ssmix_sources.section_type IS 'section_type';
COMMENT ON COLUMN sk.ssmix_sources.created_at IS 'created_at';

COMMENT ON TABLE sk.medical_embeddings IS 'sk.medical_embeddings';
COMMENT ON COLUMN sk.medical_embeddings.embedding_id IS 'embedding_id';
COMMENT ON COLUMN sk.medical_embeddings.pt_id IS 'pt_id';
COMMENT ON COLUMN sk.medical_embeddings.source_id IS 'source_id';
COMMENT ON COLUMN sk.medical_embeddings.source_type IS 'source_type';
COMMENT ON COLUMN sk.medical_embeddings.content IS 'content';
COMMENT ON COLUMN sk.medical_embeddings.embedding IS 'embedding';
COMMENT ON COLUMN sk.medical_embeddings.created_at IS 'created_at';

