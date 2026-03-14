-- 郵便番号ステージング → 本番テーブルへのマージ SQL
-- PostalCodeImportHandler が AddSection マーカーで分割して使用する

-- [FULL]
-- Full モード: 本番テーブルを全件置換
TRUNCATE ext.postal_codes RESTART IDENTITY;

INSERT INTO ext.postal_codes (
    jis_code, old_zip_code, zip_code,
    prefecture_kana, city_kana, town_kana,
    prefecture, city, town,
    multi_zip, koaza_split, has_chome, multi_town,
    update_reason, change_reason
)
SELECT
    jis_code,
    old_zip_code,
    zip_code,
    prefecture_kana,
    city_kana,
    town_kana,
    prefecture,
    city,
    town,
    multi_zip    = '1',
    koaza_split  = '1',
    has_chome    = '1',
    multi_town   = '1',
    update_reason::SMALLINT,
    change_reason::SMALLINT
FROM ext.postal_codes_staged;

-- [DELTA_ADD]
-- Delta モード: 追加・更新レコードを MERGE (upsert)
MERGE INTO ext.postal_codes AS t
USING (
    SELECT
        jis_code,
        old_zip_code,
        zip_code,
        prefecture_kana,
        city_kana,
        town_kana,
        prefecture,
        city,
        town,
        multi_zip    = '1'          AS multi_zip,
        koaza_split  = '1'          AS koaza_split,
        has_chome    = '1'          AS has_chome,
        multi_town   = '1'          AS multi_town,
        update_reason::SMALLINT     AS update_reason,
        change_reason::SMALLINT     AS change_reason
    FROM ext.postal_codes_staged
) AS s
ON t.jis_code = s.jis_code AND t.zip_code = s.zip_code AND t.town_kana = s.town_kana
WHEN MATCHED THEN
    UPDATE SET
        old_zip_code    = s.old_zip_code,
        prefecture_kana = s.prefecture_kana,
        city_kana       = s.city_kana,
        prefecture      = s.prefecture,
        city            = s.city,
        town            = s.town,
        multi_zip       = s.multi_zip,
        koaza_split     = s.koaza_split,
        has_chome       = s.has_chome,
        multi_town      = s.multi_town,
        update_reason   = s.update_reason,
        change_reason   = s.change_reason
WHEN NOT MATCHED THEN
    INSERT (
        jis_code, old_zip_code, zip_code,
        prefecture_kana, city_kana, town_kana,
        prefecture, city, town,
        multi_zip, koaza_split, has_chome, multi_town,
        update_reason, change_reason
    )
    VALUES (
        s.jis_code, s.old_zip_code, s.zip_code,
        s.prefecture_kana, s.city_kana, s.town_kana,
        s.prefecture, s.city, s.town,
        s.multi_zip, s.koaza_split, s.has_chome, s.multi_town,
        s.update_reason, s.change_reason
    );

-- [DELTA_DEL]
-- Delta モード: 削除レコードを本番から除去
DELETE FROM ext.postal_codes AS t
USING ext.postal_codes_staged AS s
WHERE t.jis_code  = s.jis_code
  AND t.zip_code  = s.zip_code
  AND t.town_kana = s.town_kana;
