-- 郵便番号テーブル（本番・ステージング）
-- 初回実行時に自動作成される
-- 出典: 日本郵便 郵便番号データ utf_ken_all.zip

CREATE SCHEMA IF NOT EXISTS ext;

-- 本番テーブル
CREATE TABLE IF NOT EXISTS ext.postal_codes (
    id              BIGSERIAL   PRIMARY KEY,
    jis_code        TEXT        NOT NULL,   -- 全国地方公共団体コード (5桁)
    old_zip_code    TEXT,                   -- 旧郵便番号 (3または5桁)
    zip_code        TEXT        NOT NULL,   -- 郵便番号 (7桁, ハイフンなし)
    prefecture_kana TEXT,                   -- 都道府県名カナ
    city_kana       TEXT,                   -- 市区町村名カナ
    town_kana       TEXT,                   -- 町域名カナ
    prefecture      TEXT,                   -- 都道府県名
    city            TEXT,                   -- 市区町村名
    town            TEXT,                   -- 町域名
    multi_zip       BOOLEAN     NOT NULL DEFAULT FALSE, -- 一町域が二以上の郵便番号に該当
    koaza_split     BOOLEAN     NOT NULL DEFAULT FALSE, -- 小字毎に番地が起番されている町域
    has_chome       BOOLEAN     NOT NULL DEFAULT FALSE, -- 丁目を有する町域
    multi_town      BOOLEAN     NOT NULL DEFAULT FALSE, -- 一郵便番号で二以上の町域を表す
    update_reason   SMALLINT    NOT NULL DEFAULT 0,     -- 更新の表示 (0:変更なし, 1:変更あり, 2:廃止)
    change_reason   SMALLINT    NOT NULL DEFAULT 0,     -- 変更理由 (0-6)
    UNIQUE (jis_code, zip_code, town_kana)
);

CREATE INDEX IF NOT EXISTS idx_postal_codes_zip ON ext.postal_codes (zip_code);
CREATE INDEX IF NOT EXISTS idx_postal_codes_jis ON ext.postal_codes (jis_code);

-- ステージングテーブル: COPY FROM STDIN の受け口 (全列 TEXT で型変換を避ける)
CREATE TABLE IF NOT EXISTS ext.postal_codes_staged (
    jis_code        TEXT,
    old_zip_code    TEXT,
    zip_code        TEXT,
    prefecture_kana TEXT,
    city_kana       TEXT,
    town_kana       TEXT,
    prefecture      TEXT,
    city            TEXT,
    town            TEXT,
    multi_zip       TEXT,
    koaza_split     TEXT,
    has_chome       TEXT,
    multi_town      TEXT,
    update_reason   TEXT,
    change_reason   TEXT
);
