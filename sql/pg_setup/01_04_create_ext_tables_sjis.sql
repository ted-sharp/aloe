-- Project Name : aloe_reservation_grid
-- Date/Time    : 2025/03/14 13:46:03
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

-- ext.jis_degrade_maps
-- * BackupToTempTable
DROP TABLE if exists ext.jis_degrade_maps CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.jis_degrade_maps (
  source_text TEXT DEFAULT '' NOT NULL
  , mapped_text TEXT DEFAULT '' NOT NULL
) ;

CREATE UNIQUE INDEX jis_degrade_maps_PKI
  ON ext.jis_degrade_maps(source_text);

ALTER TABLE ext.jis_degrade_maps
  ADD CONSTRAINT jis_degrade_maps_PKC PRIMARY KEY (source_text);

-- ext.houjin_numbers
-- * BackupToTempTable
DROP TABLE if exists ext.houjin_numbers CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.houjin_numbers (
  sequence_number TEXT NOT NULL
  , corporate_number TEXT DEFAULT '' NOT NULL
  , name TEXT DEFAULT '' NOT NULL
  , name_katakana TEXT DEFAULT '' NOT NULL
  , zip_code TEXT DEFAULT '' NOT NULL
  , prefecture_name TEXT DEFAULT '' NOT NULL
  , city_name TEXT DEFAULT '' NOT NULL
  , street_number TEXT DEFAULT '' NOT NULL
) ;

CREATE UNIQUE INDEX houjin_numbers_PKI
  ON ext.houjin_numbers(sequence_number);

ALTER TABLE ext.houjin_numbers
  ADD CONSTRAINT houjin_numbers_PKC PRIMARY KEY (sequence_number);

-- ext.raw_houjin_numbers
-- * BackupToTempTable
DROP TABLE if exists ext.raw_houjin_numbers CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_houjin_numbers (
  sequence_number TEXT NOT NULL
  , corporate_number TEXT DEFAULT '' NOT NULL
  , process TEXT DEFAULT '' NOT NULL
  , correct TEXT DEFAULT '' NOT NULL
  , update_date TEXT DEFAULT '' NOT NULL
  , change_date TEXT DEFAULT '' NOT NULL
  , name TEXT DEFAULT '' NOT NULL
  , name_image_id TEXT DEFAULT '' NOT NULL
  , kind TEXT DEFAULT '' NOT NULL
  , prefecture_name TEXT DEFAULT '' NOT NULL
  , city_name TEXT DEFAULT '' NOT NULL
  , street_number TEXT DEFAULT '' NOT NULL
  , address_image_id TEXT DEFAULT '' NOT NULL
  , prefecture_code TEXT DEFAULT '' NOT NULL
  , city_code TEXT DEFAULT '' NOT NULL
  , post_code TEXT DEFAULT '' NOT NULL
  , address_outside TEXT DEFAULT '' NOT NULL
  , address_outside_image_id TEXT DEFAULT '' NOT NULL
  , close_date TEXT DEFAULT '' NOT NULL
  , close_cause TEXT DEFAULT '' NOT NULL
  , successor_corporate_number TEXT DEFAULT '' NOT NULL
  , change_cause TEXT DEFAULT '' NOT NULL
  , assignment_date TEXT DEFAULT '' NOT NULL
  , latest TEXT DEFAULT '' NOT NULL
  , en_name TEXT DEFAULT '' NOT NULL
  , en_prefecture_name TEXT DEFAULT '' NOT NULL
  , en_city_name TEXT DEFAULT '' NOT NULL
  , en_address_outside TEXT DEFAULT '' NOT NULL
  , furigana TEXT DEFAULT '' NOT NULL
  , hihyoji TEXT DEFAULT '' NOT NULL
) ;

CREATE UNIQUE INDEX raw_houjin_numbers_PKI
  ON ext.raw_houjin_numbers(sequence_number);

ALTER TABLE ext.raw_houjin_numbers
  ADD CONSTRAINT raw_houjin_numbers_PKC PRIMARY KEY (sequence_number);

-- ext.zip_codes
-- * BackupToTempTable
DROP TABLE if exists ext.zip_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.zip_codes (
  local_government_code TEXT NOT NULL
  , zip_code TEXT DEFAULT '' NOT NULL
  , prefecture_katakana TEXT DEFAULT '' NOT NULL
  , city_katakana TEXT DEFAULT '' NOT NULL
  , town_katakana TEXT DEFAULT '' NOT NULL
  , prefecture TEXT DEFAULT '' NOT NULL
  , city TEXT DEFAULT '' NOT NULL
  , town TEXT DEFAULT '' NOT NULL
) ;

CREATE UNIQUE INDEX zip_codes_PKI
  ON ext.zip_codes(local_government_code);

ALTER TABLE ext.zip_codes
  ADD CONSTRAINT zip_codes_PKC PRIMARY KEY (local_government_code);

-- ext.raw_jis_degrade_maps
-- * BackupToTempTable
DROP TABLE if exists ext.raw_jis_degrade_maps CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_jis_degrade_maps (
  source_menkuten_code TEXT NOT NULL
  , source_unicode TEXT DEFAULT '' NOT NULL
  , source_text TEXT DEFAULT '' NOT NULL
  , mapped_menkuten_code TEXT DEFAULT '' NOT NULL
  , mapped_unicode TEXT DEFAULT '' NOT NULL
  , mapped_text TEXT DEFAULT '' NOT NULL
  , multi_menkuten_code_1 TEXT DEFAULT '' NOT NULL
  , multi_menkuten_code_2 TEXT DEFAULT '' NOT NULL
  , multi_menkuten_code_3 TEXT DEFAULT '' NOT NULL
  , multi_menkuten_code_4 TEXT DEFAULT '' NOT NULL
  , multi_unicode_1 TEXT DEFAULT '' NOT NULL
  , multi_unicode_2 TEXT DEFAULT '' NOT NULL
  , multi_unicode_3 TEXT DEFAULT '' NOT NULL
  , multi_unicode_4 TEXT DEFAULT '' NOT NULL
  , multi_text TEXT DEFAULT '' NOT NULL
  , remarks TEXT DEFAULT '' NOT NULL
) ;

CREATE UNIQUE INDEX raw_jis_degrade_maps_PKI
  ON ext.raw_jis_degrade_maps(source_menkuten_code);

ALTER TABLE ext.raw_jis_degrade_maps
  ADD CONSTRAINT raw_jis_degrade_maps_PKC PRIMARY KEY (source_menkuten_code);

-- ext.raw_zip_codes
-- * BackupToTempTable
DROP TABLE if exists ext.raw_zip_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_zip_codes (
  local_government_code TEXT NOT NULL
  , old_zip_code5 TEXT DEFAULT '' NOT NULL
  , zip_code7 TEXT DEFAULT '' NOT NULL
  , prefecture_katakana TEXT DEFAULT '' NOT NULL
  , city_katakana TEXT DEFAULT '' NOT NULL
  , town_katakana TEXT DEFAULT '' NOT NULL
  , prefecture TEXT DEFAULT '' NOT NULL
  , city TEXT DEFAULT '' NOT NULL
  , town TEXT DEFAULT '' NOT NULL
  , is_multi_zip BOOLEAN DEFAULT FALSE NOT NULL
  , is_koaza BOOLEAN DEFAULT FALSE NOT NULL
  , is_chome BOOLEAN DEFAULT FALSE NOT NULL
  , is_multi_town BOOLEAN DEFAULT FALSE NOT NULL
  , update_status TEXT DEFAULT '' NOT NULL
  , update_reason TEXT DEFAULT '' NOT NULL
) ;

CREATE UNIQUE INDEX raw_zip_codes_PKI
  ON ext.raw_zip_codes(local_government_code);

ALTER TABLE ext.raw_zip_codes
  ADD CONSTRAINT raw_zip_codes_PKC PRIMARY KEY (local_government_code);

COMMENT ON TABLE ext.jis_degrade_maps IS 'ext.jis_degrade_maps';
COMMENT ON COLUMN ext.jis_degrade_maps.source_text IS '3. source_text';
COMMENT ON COLUMN ext.jis_degrade_maps.mapped_text IS '7. mapped_text';

COMMENT ON TABLE ext.houjin_numbers IS 'ext.houjin_numbers';
COMMENT ON COLUMN ext.houjin_numbers.sequence_number IS '7. sequence_number';
COMMENT ON COLUMN ext.houjin_numbers.corporate_number IS '8. corporate_number';
COMMENT ON COLUMN ext.houjin_numbers.name IS '13. name';
COMMENT ON COLUMN ext.houjin_numbers.name_katakana IS '35. name_katakana';
COMMENT ON COLUMN ext.houjin_numbers.zip_code IS '22. zip_code';
COMMENT ON COLUMN ext.houjin_numbers.prefecture_name IS '16. prefecture_name';
COMMENT ON COLUMN ext.houjin_numbers.city_name IS '17. city_name';
COMMENT ON COLUMN ext.houjin_numbers.street_number IS '18. street_number';

COMMENT ON TABLE ext.raw_houjin_numbers IS 'ext.raw_houjin_numbers';
COMMENT ON COLUMN ext.raw_houjin_numbers.sequence_number IS '7. 一連番号';
COMMENT ON COLUMN ext.raw_houjin_numbers.corporate_number IS '8. 法人番号(13桁)';
COMMENT ON COLUMN ext.raw_houjin_numbers.process IS '9. 処理区分:01: 新規, 11: 名称変更, 12: 住所変更, etc...';
COMMENT ON COLUMN ext.raw_houjin_numbers.correct IS '10. 訂正区分:0: 訂正以外, 1: 訂正';
COMMENT ON COLUMN ext.raw_houjin_numbers.update_date IS '11. 更新年月日';
COMMENT ON COLUMN ext.raw_houjin_numbers.change_date IS '12. 変更年月日';
COMMENT ON COLUMN ext.raw_houjin_numbers.name IS '13. 商号又は名称(150文字):150文字までを格納';
COMMENT ON COLUMN ext.raw_houjin_numbers.name_image_id IS '14. 商号又は名称イメージID:151文字以降は画像で確認';
COMMENT ON COLUMN ext.raw_houjin_numbers.kind IS '15. 法人種別:101: 国の機関, 201: 地方公共団体, 301: 株式会社, etc...';
COMMENT ON COLUMN ext.raw_houjin_numbers.prefecture_name IS '16. 国内所在地(都道府県)';
COMMENT ON COLUMN ext.raw_houjin_numbers.city_name IS '17. 国内所在地(市区町村)';
COMMENT ON COLUMN ext.raw_houjin_numbers.street_number IS '18. 国内所在地(丁目番地等)';
COMMENT ON COLUMN ext.raw_houjin_numbers.address_image_id IS '19. 国内所在地イメージID';
COMMENT ON COLUMN ext.raw_houjin_numbers.prefecture_code IS '20. 都道府県コード:JIS X 0401';
COMMENT ON COLUMN ext.raw_houjin_numbers.city_code IS '21. 市区町村コード:JIS X 0402';
COMMENT ON COLUMN ext.raw_houjin_numbers.post_code IS '22. 郵便番号';
COMMENT ON COLUMN ext.raw_houjin_numbers.address_outside IS '23. 国外所在地';
COMMENT ON COLUMN ext.raw_houjin_numbers.address_outside_image_id IS '24. 国外所在地イメージID';
COMMENT ON COLUMN ext.raw_houjin_numbers.close_date IS '25. 登記記録の閉鎖等年月日';
COMMENT ON COLUMN ext.raw_houjin_numbers.close_cause IS '26. 登記記録の閉鎖等の事由:01: 清算の結了
等
, 11: 合併による
解散等
, 21: 登記官によ
る閉鎖
, 31: その他の清
算の結了等';
COMMENT ON COLUMN ext.raw_houjin_numbers.successor_corporate_number IS '27. 承継先法人番号';
COMMENT ON COLUMN ext.raw_houjin_numbers.change_cause IS '28. 変更事由の詳細';
COMMENT ON COLUMN ext.raw_houjin_numbers.assignment_date IS '29. 法人番号指定年月日';
COMMENT ON COLUMN ext.raw_houjin_numbers.latest IS '30. 最新履歴:0: 過去情報, 1: 最新情報';
COMMENT ON COLUMN ext.raw_houjin_numbers.en_name IS '31. 商号又は名称(en)';
COMMENT ON COLUMN ext.raw_houjin_numbers.en_prefecture_name IS '32. 国内所在地(都道府県)(en)';
COMMENT ON COLUMN ext.raw_houjin_numbers.en_city_name IS '33. 国内所在地(市区町村丁目番地等)(en)';
COMMENT ON COLUMN ext.raw_houjin_numbers.en_address_outside IS '34. 国外所在地(en)';
COMMENT ON COLUMN ext.raw_houjin_numbers.furigana IS '35. フリガナ';
COMMENT ON COLUMN ext.raw_houjin_numbers.hihyoji IS '36. 検索対象除外:0: 検索対象, 1: 除外';

COMMENT ON TABLE ext.zip_codes IS 'ext.zip_codes';
COMMENT ON COLUMN ext.zip_codes.local_government_code IS 'local_government_code';
COMMENT ON COLUMN ext.zip_codes.zip_code IS 'zip_code';
COMMENT ON COLUMN ext.zip_codes.prefecture_katakana IS 'prefecture_katakana';
COMMENT ON COLUMN ext.zip_codes.city_katakana IS 'city_katakana';
COMMENT ON COLUMN ext.zip_codes.town_katakana IS 'town_katakana';
COMMENT ON COLUMN ext.zip_codes.prefecture IS 'prefecture';
COMMENT ON COLUMN ext.zip_codes.city IS 'city';
COMMENT ON COLUMN ext.zip_codes.town IS 'town';

COMMENT ON TABLE ext.raw_jis_degrade_maps IS 'ext.raw_jis_degrade_maps';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.source_menkuten_code IS '1. 面区点コード';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.source_unicode IS '2. Unicode';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.source_text IS '3. 字形';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.mapped_menkuten_code IS '5. 面区点コード';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.mapped_unicode IS '6. Unicode';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.mapped_text IS '7. 字形';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.multi_menkuten_code_1 IS '8. 面区点コード①';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.multi_menkuten_code_2 IS '9. 面区点コード②';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.multi_menkuten_code_3 IS '10. 面区点コード③';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.multi_menkuten_code_4 IS '11. 面区点コード④';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.multi_unicode_1 IS '12. Unicode①';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.multi_unicode_2 IS '13. Unicode②';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.multi_unicode_3 IS '14. Unicode③';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.multi_unicode_4 IS '15. Unicode④';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.multi_text IS '16. 字形';
COMMENT ON COLUMN ext.raw_jis_degrade_maps.remarks IS '17. 備考';

COMMENT ON TABLE ext.raw_zip_codes IS 'ext.raw_zip_codes';
COMMENT ON COLUMN ext.raw_zip_codes.local_government_code IS '全国地方公共団体コード:JIS X0401、X0402';
COMMENT ON COLUMN ext.raw_zip_codes.old_zip_code5 IS '旧郵便番号(5桁)';
COMMENT ON COLUMN ext.raw_zip_codes.zip_code7 IS '郵便番号(7桁)';
COMMENT ON COLUMN ext.raw_zip_codes.prefecture_katakana IS '都道府県名(カナ)';
COMMENT ON COLUMN ext.raw_zip_codes.city_katakana IS '市区町村(カナ)';
COMMENT ON COLUMN ext.raw_zip_codes.town_katakana IS '町域名(カナ)';
COMMENT ON COLUMN ext.raw_zip_codes.prefecture IS '都道府県名';
COMMENT ON COLUMN ext.raw_zip_codes.city IS '市区町村名';
COMMENT ON COLUMN ext.raw_zip_codes.town IS '町域名';
COMMENT ON COLUMN ext.raw_zip_codes.is_multi_zip IS '複数郵便番号フラグ:一町域が二以上の郵便番号で表される場合の表示（"1": 該当、"0": 該当せず）';
COMMENT ON COLUMN ext.raw_zip_codes.is_koaza IS '小字番地フラグ:小字毎に番地が起番されている町域の表示（"1": 該当、"0": 該当せず）';
COMMENT ON COLUMN ext.raw_zip_codes.is_chome IS '丁目フラグ:丁目を有する町域の場合の表示（"1": 該当、"0": 該当せず）';
COMMENT ON COLUMN ext.raw_zip_codes.is_multi_town IS '複数町域フラグ:一つの郵便番号で二以上の町域を表す場合の表示（"1": 該当、"0": 該当せず）';
COMMENT ON COLUMN ext.raw_zip_codes.update_status IS '更新フラグ:更新の表示（"0": 変更なし、"1": 変更あり、"2": 廃止）';
COMMENT ON COLUMN ext.raw_zip_codes.update_reason IS '更新理由:変更理由（"0": 変更なし、"1": 市政等施行、"2": 住居表示の実施、"3": 区画整理、"4": 郵便区調整等、"5": 訂正、"6": 廃止）';

