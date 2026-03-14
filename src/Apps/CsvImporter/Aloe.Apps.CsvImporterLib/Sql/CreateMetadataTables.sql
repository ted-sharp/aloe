-- インポート実行メタデータテーブル
-- 初回実行時に自動作成される

CREATE SCHEMA IF NOT EXISTS ext;

CREATE TABLE IF NOT EXISTS ext.ingestion_runs (
    run_id        BIGSERIAL    PRIMARY KEY,
    source_system TEXT         NOT NULL,
    mode          TEXT         NOT NULL,
    status        TEXT         NOT NULL DEFAULT 'running',
    rows_loaded   BIGINT,
    started_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    finished_at   TIMESTAMPTZ,
    errors        JSONB
);

CREATE TABLE IF NOT EXISTS ext.ingestion_files (
    file_id       BIGSERIAL    PRIMARY KEY,
    run_id        BIGINT       NOT NULL REFERENCES ext.ingestion_runs (run_id),
    uri           TEXT         NOT NULL,
    hash_sha256   TEXT,
    downloaded_at TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);
