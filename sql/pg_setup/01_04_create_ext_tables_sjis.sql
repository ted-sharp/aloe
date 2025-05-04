-- Project Name : aloe_reservation_grid
-- Date/Time    : 2025/04/28 13:21:15
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

-- ext.hot13_codes
-- * BackupToTempTable
DROP TABLE if exists ext.hot13_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.hot13_codes (
  hot13_code TEXT DEFAULT '' NOT NULL
  , hot9_code TEXT DEFAULT '' NOT NULL
  , hot7_code TEXT DEFAULT '' NOT NULL
  , yakka_code TEXT DEFAULT '' NOT NULL
  , yj_code TEXT DEFAULT '' NOT NULL
  , receipt_code TEXT DEFAULT '' NOT NULL
  , official_name TEXT DEFAULT '' NOT NULL
  , product_name TEXT DEFAULT '' NOT NULL
  , receipt_drug_name TEXT DEFAULT '' NOT NULL
  , medication_type TEXT DEFAULT '' NOT NULL
  , pharmaceutical_company TEXT DEFAULT '' NOT NULL
  , pharmaceutical_distributor TEXT DEFAULT '' NOT NULL
) ;

-- ext.raw_hot13_codes
-- * BackupToTempTable
DROP TABLE if exists ext.raw_hot13_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_hot13_codes (
  hot13_code TEXT NOT NULL
  , hot7_code TEXT
  , distributor_code TEXT
  , package_code TEXT
  , logistics_code TEXT
  , gtin13_code TEXT
  , yakka_code TEXT
  , yj_code TEXT
  , receipt_code TEXT
  , receipt_code_old TEXT
  , official_name TEXT
  , product_name TEXT
  , receipt_drug_name TEXT
  , strength_per_unit TEXT
  , package_form TEXT
  , item_quantity_per_package TEXT
  , item_quantity_unit TEXT
  , total_quantity_per_product TEXT
  , total_quantity_unit TEXT
  , medication_type TEXT
  , pharmaceutical_company TEXT
  , pharmaceutical_distributor TEXT
  , record_type TEXT
  , update_date TEXT
) ;

-- ext.icd10_index_terms
-- * BackupToTempTable
DROP TABLE if exists ext.icd10_index_terms CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.icd10_index_terms (
  index_term TEXT DEFAULT '' NOT NULL
  , linked_code TEXT DEFAULT '' NOT NULL
  , linked_table_code TEXT DEFAULT '' NOT NULL
  , linked_name_variant_code TEXT DEFAULT '' NOT NULL
  , synonym_type_code TEXT DEFAULT '' NOT NULL
  , character_variant_type_code TEXT DEFAULT '' NOT NULL
) ;

-- ext.raw_icd10_index_terms
-- * BackupToTempTable
DROP TABLE if exists ext.raw_icd10_index_terms CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_icd10_index_terms (
  index_term TEXT DEFAULT '' NOT NULL
  , linked_code TEXT DEFAULT '' NOT NULL
  , linked_table_code TEXT DEFAULT '' NOT NULL
  , linked_name_variant_code TEXT DEFAULT '' NOT NULL
  , synonym_type_code TEXT DEFAULT '' NOT NULL
  , character_variant_type_code TEXT DEFAULT '' NOT NULL
  , icd10_2013_entry_type_code TEXT DEFAULT '' NOT NULL
  , reserved1 TEXT DEFAULT '' NOT NULL
  , reserved2 TEXT DEFAULT '' NOT NULL
) ;

-- ext.icd10_modifier_codes
-- * BackupToTempTable
DROP TABLE if exists ext.icd10_modifier_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.icd10_modifier_codes (
  modifier_code TEXT DEFAULT '' NOT NULL
  , modifier_name TEXT DEFAULT '' NOT NULL
  , modifier_name_kana TEXT DEFAULT '' NOT NULL
  , modifier_position_code TEXT DEFAULT '' NOT NULL
  , modifier_classification_code TEXT DEFAULT '' NOT NULL
  , modifier_mutex_group_code TEXT DEFAULT '' NOT NULL
  , modifier_description TEXT DEFAULT '' NOT NULL
) ;

-- ext.raw_icd10_modifier_codes
-- * BackupToTempTable
DROP TABLE if exists ext.raw_icd10_modifier_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_icd10_modifier_codes (
  change_flag TEXT DEFAULT '' NOT NULL
  , modifier_id TEXT DEFAULT '' NOT NULL
  , modifier_name TEXT DEFAULT '' NOT NULL
  , modifier_name_kana TEXT DEFAULT '' NOT NULL
  , modifier_code TEXT DEFAULT '' NOT NULL
  , modifier_position_code TEXT DEFAULT '' NOT NULL
  , modifier_classification_code TEXT DEFAULT '' NOT NULL
  , modifier_mutex_group_code TEXT DEFAULT '' NOT NULL
  , receipt_modifier_code TEXT DEFAULT '' NOT NULL
  , modifier_description TEXT DEFAULT '' NOT NULL
) ;

-- ext.icd10_diagnosis_codes
-- * BackupToTempTable
DROP TABLE if exists ext.icd10_diagnosis_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.icd10_diagnosis_codes (
  diagnosis_code TEXT DEFAULT '' NOT NULL
  , diagnosis_name TEXT DEFAULT '' NOT NULL
  , diagnosis_name_kana TEXT DEFAULT '' NOT NULL
  , diagnosis_frequency_level TEXT DEFAULT '' NOT NULL
  , diagnosis_domain_code TEXT DEFAULT '' NOT NULL
  , modifier_recommendation_code TEXT DEFAULT '' NOT NULL
) ;

-- ext.raw_icd10_diagnosis_codes
-- * BackupToTempTable
DROP TABLE if exists ext.raw_icd10_diagnosis_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_icd10_diagnosis_codes (
  change_flag TEXT DEFAULT '' NOT NULL
  , diagnosis_id TEXT DEFAULT '' NOT NULL
  , diagnosis_name TEXT DEFAULT '' NOT NULL
  , diagnosis_name_kana TEXT DEFAULT '' NOT NULL
  , diagnosis_frequency_level TEXT DEFAULT '' NOT NULL
  , diagnosis_code TEXT DEFAULT '' NOT NULL
  , diagnosis_code_2013 TEXT DEFAULT '' NOT NULL
  , diagnosis_subcode_2013 TEXT DEFAULT '' NOT NULL
  , reserved1 TEXT DEFAULT '' NOT NULL
  , reserved2 TEXT DEFAULT '' NOT NULL
  , receipt_diagnosis_code TEXT DEFAULT '' NOT NULL
  , receipt_diagnosis_abbreviated_name TEXT DEFAULT '' NOT NULL
  , diagnosis_domain_code TEXT DEFAULT '' NOT NULL
  , revision_number TEXT DEFAULT '' NOT NULL
  , updated_date TEXT DEFAULT '' NOT NULL
  , migrated_diagnosis_id TEXT DEFAULT '' NOT NULL
  , modifier_recommendation_code TEXT DEFAULT '' NOT NULL
  , insurance_claim_exclusion_code TEXT DEFAULT '' NOT NULL
  , reserved3 TEXT DEFAULT '' NOT NULL
  , reserved4 TEXT DEFAULT '' NOT NULL
) ;

-- ext.facility_codes
-- * BackupToTempTable
DROP TABLE if exists ext.facility_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.facility_codes (
  facility_code TEXT DEFAULT '' NOT NULL
  , facility_name TEXT DEFAULT '' NOT NULL
  , zip_code TEXT DEFAULT '' NOT NULL
  , address TEXT DEFAULT '' NOT NULL
  , phone TEXT DEFAULT '' NOT NULL
  , website TEXT DEFAULT '' NOT NULL
) ;

-- ext.raw_special_health_facility_codes
-- * BackupToTempTable
DROP TABLE if exists ext.raw_special_health_facility_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_special_health_facility_codes (
  facility_code TEXT DEFAULT '' NOT NULL
  , facility_type TEXT DEFAULT '' NOT NULL
  , facility_name TEXT DEFAULT '' NOT NULL
  , zip_code TEXT DEFAULT '' NOT NULL
  , phone TEXT DEFAULT '' NOT NULL
  , address TEXT DEFAULT '' NOT NULL
  , website TEXT DEFAULT '' NOT NULL
  , owner TEXT DEFAULT '' NOT NULL
) ;

-- ext.raw_fhir_observation_codes
-- * BackupToTempTable
DROP TABLE if exists ext.raw_fhir_observation_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_fhir_observation_codes (
  concept_code TEXT DEFAULT '' NOT NULL
  , jurisdiction_coding_system TEXT DEFAULT '' NOT NULL
  , concept_display TEXT DEFAULT '' NOT NULL
) ;

-- ext.raw_mhlw_xml_tokutei_kenshin_items
-- * BackupToTempTable
DROP TABLE if exists ext.raw_mhlw_xml_tokutei_kenshin_items CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_mhlw_xml_tokutei_kenshin_items (
  category_code TEXT DEFAULT '' NOT NULL
  , category_name TEXT
  , sort_no TEXT
  , jlac10_code TEXT
  , item_name TEXT
  , item_data_type TEXT
  , xml_data_type TEXT
  , xml_data_length TEXT
  , xml_data_format TEXT
  , item_data_unit TEXT
  , xml_data_unit TEXT
  , xml_analyte_code TEXT
  , xml_analyte_name TEXT
  , xml_methodology_code TEXT
  , xml_methodology_name TEXT
  , result_code_oid TEXT
  , item_code_oid TEXT
  , xml_remarks TEXT
  , remarks TEXT
) ;

-- ext.jlac10_codes
-- * BackupToTempTable
DROP TABLE if exists ext.jlac10_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.jlac10_codes (
  jlac10_code TEXT DEFAULT '' NOT NULL
  , analyte_code TEXT
  , analyte_name TEXT
  , identification_code TEXT
  , identification_name TEXT
  , specimen_code TEXT
  , specimen_name TEXT
  , methodology_code TEXT
  , methodology_name TEXT
  , result_identifying_general_code TEXT
  , result_identifying_general_name TEXT
  , result_identifying_specific_code TEXT
  , result_identifying_specific_name TEXT
) ;

-- ext.raw_jlac10_codes
-- * BackupToTempTable
DROP TABLE if exists ext.raw_jlac10_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_jlac10_codes (
  jlac10_code_17 TEXT DEFAULT '' NOT NULL
  , analyte_flag TEXT
  , analyte_code TEXT
  , analyte_name TEXT
  , identification_flag TEXT
  , identification_code TEXT
  , identification_name TEXT
  , specimen_flag TEXT
  , specimen_code TEXT
  , specimen_name TEXT
  , methodology_flag TEXT
  , methodology_code TEXT
  , methodology_name TEXT
  , result_identifying_general_flag TEXT
  , result_identifying_general_code TEXT
  , result_identifying_general_name TEXT
  , result_identifying_specific_flag TEXT
  , result_identifying_specific_name TEXT
  , result_identifying_specific_code TEXT
) ;

-- ext.jis_compat_maps
-- * BackupToTempTable
DROP TABLE if exists ext.jis_compat_maps CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.jis_compat_maps (
  source_text TEXT DEFAULT '' NOT NULL
  , mapped_text TEXT DEFAULT '' NOT NULL
) ;

-- ext.houjin_numbers
-- * BackupToTempTable
DROP TABLE if exists ext.houjin_numbers CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.houjin_numbers (
  corporate_number TEXT DEFAULT '' NOT NULL
  , name TEXT DEFAULT '' NOT NULL
  , zip_code TEXT
  , prefecture_name TEXT
  , city_name TEXT
  , street_number TEXT
) ;

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
  , name_image_id TEXT
  , kind TEXT DEFAULT '' NOT NULL
  , prefecture_name TEXT
  , city_name TEXT
  , street_number TEXT
  , address_image_id TEXT
  , prefecture_code TEXT
  , city_code TEXT
  , post_code TEXT
  , address_outside TEXT
  , address_outside_image_id TEXT
  , close_date TEXT
  , close_cause TEXT
  , successor_corporate_number TEXT
  , change_cause TEXT
  , assignment_date TEXT
  , latest TEXT
  , en_name TEXT
  , en_prefecture_name TEXT
  , en_city_name TEXT
  , en_address_outside TEXT
  , furigana TEXT
  , hihyoji TEXT DEFAULT '' NOT NULL
) ;

-- ext.zip_codes
-- * BackupToTempTable
DROP TABLE if exists ext.zip_codes CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.zip_codes (
  zip_code TEXT DEFAULT '' NOT NULL
  , prefecture_katakana TEXT DEFAULT '' NOT NULL
  , city_katakana TEXT DEFAULT '' NOT NULL
  , town_katakana TEXT DEFAULT '' NOT NULL
  , prefecture TEXT DEFAULT '' NOT NULL
  , city TEXT DEFAULT '' NOT NULL
  , town TEXT DEFAULT '' NOT NULL
) ;

-- ext.raw_jis_compat_maps
-- * BackupToTempTable
DROP TABLE if exists ext.raw_jis_compat_maps CASCADE;

-- * RestoreFromTempTable
CREATE TABLE ext.raw_jis_compat_maps (
  source_menkuten_code TEXT NOT NULL
  , source_unicode TEXT DEFAULT '' NOT NULL
  , source_text TEXT DEFAULT '' NOT NULL
  , source_jis_kubun TEXT DEFAULT '' NOT NULL
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

COMMENT ON TABLE ext.hot13_codes IS 'ext.hot13_codes';
COMMENT ON COLUMN ext.hot13_codes.hot13_code IS 'hot13_code';
COMMENT ON COLUMN ext.hot13_codes.hot9_code IS 'hot9_code';
COMMENT ON COLUMN ext.hot13_codes.hot7_code IS 'hot7_code';
COMMENT ON COLUMN ext.hot13_codes.yakka_code IS 'yakka_code';
COMMENT ON COLUMN ext.hot13_codes.yj_code IS 'yj_code';
COMMENT ON COLUMN ext.hot13_codes.receipt_code IS 'receipt_code';
COMMENT ON COLUMN ext.hot13_codes.official_name IS 'official_name';
COMMENT ON COLUMN ext.hot13_codes.product_name IS 'product_name';
COMMENT ON COLUMN ext.hot13_codes.receipt_drug_name IS 'receipt_drug_name';
COMMENT ON COLUMN ext.hot13_codes.medication_type IS 'medication_type';
COMMENT ON COLUMN ext.hot13_codes.pharmaceutical_company IS 'pharmaceutical_company';
COMMENT ON COLUMN ext.hot13_codes.pharmaceutical_distributor IS 'pharmaceutical_distributor';

COMMENT ON TABLE ext.raw_hot13_codes IS 'ext.raw_hot13_codes';
COMMENT ON COLUMN ext.raw_hot13_codes.hot13_code IS 'HOT13(JAN粒度)';
COMMENT ON COLUMN ext.raw_hot13_codes.hot7_code IS 'HOT7(処方粒度)';
COMMENT ON COLUMN ext.raw_hot13_codes.distributor_code IS '会社用';
COMMENT ON COLUMN ext.raw_hot13_codes.package_code IS '調剤用';
COMMENT ON COLUMN ext.raw_hot13_codes.logistics_code IS '物流用';
COMMENT ON COLUMN ext.raw_hot13_codes.gtin13_code IS 'JANコード(GTIN-13)';
COMMENT ON COLUMN ext.raw_hot13_codes.yakka_code IS '薬価基準収載医薬品コード';
COMMENT ON COLUMN ext.raw_hot13_codes.yj_code IS '個別医薬品コード';
COMMENT ON COLUMN ext.raw_hot13_codes.receipt_code IS 'レセ用コード1';
COMMENT ON COLUMN ext.raw_hot13_codes.receipt_code_old IS 'レセ用コード2(過去互換用)';
COMMENT ON COLUMN ext.raw_hot13_codes.official_name IS '告示名称';
COMMENT ON COLUMN ext.raw_hot13_codes.product_name IS '販売名';
COMMENT ON COLUMN ext.raw_hot13_codes.receipt_drug_name IS 'レセ用医薬品名';
COMMENT ON COLUMN ext.raw_hot13_codes.strength_per_unit IS '規格単位';
COMMENT ON COLUMN ext.raw_hot13_codes.package_form IS '包装形態';
COMMENT ON COLUMN ext.raw_hot13_codes.item_quantity_per_package IS '個数/包装(小数対応)';
COMMENT ON COLUMN ext.raw_hot13_codes.item_quantity_unit IS '包装単位';
COMMENT ON COLUMN ext.raw_hot13_codes.total_quantity_per_product IS '個数/製品(小数対応)';
COMMENT ON COLUMN ext.raw_hot13_codes.total_quantity_unit IS '総量単位';
COMMENT ON COLUMN ext.raw_hot13_codes.medication_type IS '医薬品区分';
COMMENT ON COLUMN ext.raw_hot13_codes.pharmaceutical_company IS '製造会社';
COMMENT ON COLUMN ext.raw_hot13_codes.pharmaceutical_distributor IS '販売会社';
COMMENT ON COLUMN ext.raw_hot13_codes.record_type IS 'レコード区分';
COMMENT ON COLUMN ext.raw_hot13_codes.update_date IS '更新日';

COMMENT ON TABLE ext.icd10_index_terms IS 'ext.icd10_index_terms';
COMMENT ON COLUMN ext.icd10_index_terms.index_term IS 'index_term';
COMMENT ON COLUMN ext.icd10_index_terms.linked_code IS 'linked_code';
COMMENT ON COLUMN ext.icd10_index_terms.linked_table_code IS 'linked_table_code:1: 病名マスター, 2: 修飾語マスター';
COMMENT ON COLUMN ext.icd10_index_terms.linked_name_variant_code IS 'linked_name_variant_code:1: 漢字, 2: カナ';
COMMENT ON COLUMN ext.icd10_index_terms.synonym_type_code IS 'synonym_type_code';
COMMENT ON COLUMN ext.icd10_index_terms.character_variant_type_code IS 'character_variant_type_code';

COMMENT ON TABLE ext.raw_icd10_index_terms IS 'ext.raw_icd10_index_terms';
COMMENT ON COLUMN ext.raw_icd10_index_terms.index_term IS '索引用語';
COMMENT ON COLUMN ext.raw_icd10_index_terms.linked_code IS '対応用語コード';
COMMENT ON COLUMN ext.raw_icd10_index_terms.linked_table_code IS '病名修飾語区分:1: 病名マスター, 2: 修飾語マスター';
COMMENT ON COLUMN ext.raw_icd10_index_terms.linked_name_variant_code IS 'カナ漢字区分:1: 漢字, 2: カナ';
COMMENT ON COLUMN ext.raw_icd10_index_terms.synonym_type_code IS '同義語区分';
COMMENT ON COLUMN ext.raw_icd10_index_terms.character_variant_type_code IS '異字体区分';
COMMENT ON COLUMN ext.raw_icd10_index_terms.icd10_2013_entry_type_code IS '第一版採用表記区分';
COMMENT ON COLUMN ext.raw_icd10_index_terms.reserved1 IS '言語区分(予定)';
COMMENT ON COLUMN ext.raw_icd10_index_terms.reserved2 IS '省略区分(予定)';

COMMENT ON TABLE ext.icd10_modifier_codes IS 'ext.icd10_modifier_codes';
COMMENT ON COLUMN ext.icd10_modifier_codes.modifier_code IS 'modifier_code';
COMMENT ON COLUMN ext.icd10_modifier_codes.modifier_name IS 'modifier_name';
COMMENT ON COLUMN ext.icd10_modifier_codes.modifier_name_kana IS 'modifier_name_kana';
COMMENT ON COLUMN ext.icd10_modifier_codes.modifier_position_code IS 'modifier_position_code';
COMMENT ON COLUMN ext.icd10_modifier_codes.modifier_classification_code IS 'modifier_classification_code';
COMMENT ON COLUMN ext.icd10_modifier_codes.modifier_mutex_group_code IS 'modifier_mutex_group_code';
COMMENT ON COLUMN ext.icd10_modifier_codes.modifier_description IS 'modifier_description';

COMMENT ON TABLE ext.raw_icd10_modifier_codes IS 'ext.raw_icd10_modifier_codes';
COMMENT ON COLUMN ext.raw_icd10_modifier_codes.change_flag IS '変更区分';
COMMENT ON COLUMN ext.raw_icd10_modifier_codes.modifier_id IS '修飾語管理番号';
COMMENT ON COLUMN ext.raw_icd10_modifier_codes.modifier_name IS '修飾語表記';
COMMENT ON COLUMN ext.raw_icd10_modifier_codes.modifier_name_kana IS '修飾語表記カナ';
COMMENT ON COLUMN ext.raw_icd10_modifier_codes.modifier_code IS '修飾語コード';
COMMENT ON COLUMN ext.raw_icd10_modifier_codes.modifier_position_code IS '接続位置区分';
COMMENT ON COLUMN ext.raw_icd10_modifier_codes.modifier_classification_code IS '修飾語区分';
COMMENT ON COLUMN ext.raw_icd10_modifier_codes.modifier_mutex_group_code IS '排他グループコード';
COMMENT ON COLUMN ext.raw_icd10_modifier_codes.receipt_modifier_code IS 'レセ電算修飾語コード';
COMMENT ON COLUMN ext.raw_icd10_modifier_codes.modifier_description IS '修飾語説明用ラベル';

COMMENT ON TABLE ext.icd10_diagnosis_codes IS 'ext.icd10_diagnosis_codes';
COMMENT ON COLUMN ext.icd10_diagnosis_codes.diagnosis_code IS 'diagnosis_code';
COMMENT ON COLUMN ext.icd10_diagnosis_codes.diagnosis_name IS 'diagnosis_name';
COMMENT ON COLUMN ext.icd10_diagnosis_codes.diagnosis_name_kana IS 'diagnosis_name_kana';
COMMENT ON COLUMN ext.icd10_diagnosis_codes.diagnosis_frequency_level IS 'diagnosis_frequency_level';
COMMENT ON COLUMN ext.icd10_diagnosis_codes.diagnosis_domain_code IS 'diagnosis_domain_code';
COMMENT ON COLUMN ext.icd10_diagnosis_codes.modifier_recommendation_code IS 'modifier_recommendation_code';

COMMENT ON TABLE ext.raw_icd10_diagnosis_codes IS 'ext.raw_icd10_diagnosis_codes';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.change_flag IS '変更区分';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.diagnosis_id IS '病名管理番号';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.diagnosis_name IS '病名表記';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.diagnosis_name_kana IS '病名表記カナ';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.diagnosis_frequency_level IS '採択区分';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.diagnosis_code IS 'ICD10標準病名コード';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.diagnosis_code_2013 IS 'ICD10-2013コード';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.diagnosis_subcode_2013 IS '複数分類コード';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.reserved1 IS '予備1';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.reserved2 IS '予備2';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.receipt_diagnosis_code IS 'レセ電算傷病名コード';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.receipt_diagnosis_abbreviated_name IS 'レセ傷病名省略名称';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.diagnosis_domain_code IS '使用分野';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.revision_number IS '変更履歴番号';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.updated_date IS '更新日付';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.migrated_diagnosis_id IS '移行先病名管理番号';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.modifier_recommendation_code IS '単独使用禁止区分';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.insurance_claim_exclusion_code IS '保険請求外区分';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.reserved3 IS '予備3';
COMMENT ON COLUMN ext.raw_icd10_diagnosis_codes.reserved4 IS '予備4';

COMMENT ON TABLE ext.facility_codes IS 'ext.facility_codes';
COMMENT ON COLUMN ext.facility_codes.facility_code IS 'facility_code';
COMMENT ON COLUMN ext.facility_codes.facility_name IS 'facility_name';
COMMENT ON COLUMN ext.facility_codes.zip_code IS 'zip_code';
COMMENT ON COLUMN ext.facility_codes.address IS 'address';
COMMENT ON COLUMN ext.facility_codes.phone IS 'phone';
COMMENT ON COLUMN ext.facility_codes.website IS 'website';

COMMENT ON TABLE ext.raw_special_health_facility_codes IS 'ext.raw_special_health_facility_codes';
COMMENT ON COLUMN ext.raw_special_health_facility_codes.facility_code IS '医療機関コード';
COMMENT ON COLUMN ext.raw_special_health_facility_codes.facility_type IS '医療機関種別';
COMMENT ON COLUMN ext.raw_special_health_facility_codes.facility_name IS '医療機関名';
COMMENT ON COLUMN ext.raw_special_health_facility_codes.zip_code IS '郵便番号';
COMMENT ON COLUMN ext.raw_special_health_facility_codes.phone IS '電話番号';
COMMENT ON COLUMN ext.raw_special_health_facility_codes.address IS '所在地';
COMMENT ON COLUMN ext.raw_special_health_facility_codes.website IS 'ホームページ';
COMMENT ON COLUMN ext.raw_special_health_facility_codes.owner IS '経営主体';

COMMENT ON TABLE ext.raw_fhir_observation_codes IS 'ext.raw_fhir_observation_codes';
COMMENT ON COLUMN ext.raw_fhir_observation_codes.concept_code IS 'JLAC10コード';
COMMENT ON COLUMN ext.raw_fhir_observation_codes.jurisdiction_coding_system IS 'システム';
COMMENT ON COLUMN ext.raw_fhir_observation_codes.concept_display IS '表示名';

COMMENT ON TABLE ext.raw_mhlw_xml_tokutei_kenshin_items IS 'ext.raw_mhlw_xml_tokutei_kenshin_items';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.category_code IS '区分番号';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.category_name IS '区分名称';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.sort_no IS '順番合';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.jlac10_code IS '項目コード(17桁)';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.item_name IS '項目名';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.item_data_type IS 'データタイプ';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.xml_data_type IS 'XMLデータ型';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.xml_data_length IS '最大バイト長';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.xml_data_format IS '数値フォーマット';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.item_data_unit IS '表示用単位';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.xml_data_unit IS 'XML用単位';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.xml_analyte_code IS '同一性項目コード';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.xml_analyte_name IS '同一性項目名称';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.xml_methodology_code IS 'XML検査方法コード';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.xml_methodology_name IS 'XML検査方法';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.result_code_oid IS '結果コードOID';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.item_code_oid IS '項目コードOID';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.xml_remarks IS 'XML向け備考';
COMMENT ON COLUMN ext.raw_mhlw_xml_tokutei_kenshin_items.remarks IS '備考';

COMMENT ON TABLE ext.jlac10_codes IS 'ext.jlac10_codes';
COMMENT ON COLUMN ext.jlac10_codes.jlac10_code IS 'jlac10_code';
COMMENT ON COLUMN ext.jlac10_codes.analyte_code IS 'analyte_code';
COMMENT ON COLUMN ext.jlac10_codes.analyte_name IS 'analyte_name';
COMMENT ON COLUMN ext.jlac10_codes.identification_code IS 'identification_code';
COMMENT ON COLUMN ext.jlac10_codes.identification_name IS 'identification_name';
COMMENT ON COLUMN ext.jlac10_codes.specimen_code IS 'specimen_code';
COMMENT ON COLUMN ext.jlac10_codes.specimen_name IS 'specimen_name';
COMMENT ON COLUMN ext.jlac10_codes.methodology_code IS 'methodology_code';
COMMENT ON COLUMN ext.jlac10_codes.methodology_name IS 'methodology_name';
COMMENT ON COLUMN ext.jlac10_codes.result_identifying_general_code IS 'result_identifying_general_code';
COMMENT ON COLUMN ext.jlac10_codes.result_identifying_general_name IS 'result_identifying_general_name';
COMMENT ON COLUMN ext.jlac10_codes.result_identifying_specific_code IS 'result_identifying_specific_code';
COMMENT ON COLUMN ext.jlac10_codes.result_identifying_specific_name IS 'result_identifying_specific_name';

COMMENT ON TABLE ext.raw_jlac10_codes IS 'ext.raw_jlac10_codes';
COMMENT ON COLUMN ext.raw_jlac10_codes.jlac10_code_17 IS 'JLAC10コード(17桁)';
COMMENT ON COLUMN ext.raw_jlac10_codes.analyte_flag IS '分析物_拡張フラグ';
COMMENT ON COLUMN ext.raw_jlac10_codes.analyte_code IS '分析物_コード';
COMMENT ON COLUMN ext.raw_jlac10_codes.analyte_name IS '分析物_名称';
COMMENT ON COLUMN ext.raw_jlac10_codes.identification_flag IS '識別_拡張フラグ';
COMMENT ON COLUMN ext.raw_jlac10_codes.identification_code IS '識別_コード';
COMMENT ON COLUMN ext.raw_jlac10_codes.identification_name IS '識別_名称';
COMMENT ON COLUMN ext.raw_jlac10_codes.specimen_flag IS '材料_拡張フラグ';
COMMENT ON COLUMN ext.raw_jlac10_codes.specimen_code IS '材料_コード';
COMMENT ON COLUMN ext.raw_jlac10_codes.specimen_name IS '材料_名称';
COMMENT ON COLUMN ext.raw_jlac10_codes.methodology_flag IS '測定法_拡張フラグ';
COMMENT ON COLUMN ext.raw_jlac10_codes.methodology_code IS '測定法_コード';
COMMENT ON COLUMN ext.raw_jlac10_codes.methodology_name IS '測定法_名称';
COMMENT ON COLUMN ext.raw_jlac10_codes.result_identifying_general_flag IS '結果識別(共通)_拡張フラグ';
COMMENT ON COLUMN ext.raw_jlac10_codes.result_identifying_general_code IS '結果識別(共通)_コード';
COMMENT ON COLUMN ext.raw_jlac10_codes.result_identifying_general_name IS '結果識別(共通)_名称';
COMMENT ON COLUMN ext.raw_jlac10_codes.result_identifying_specific_flag IS '結果識別(固有)_拡張フラグ';
COMMENT ON COLUMN ext.raw_jlac10_codes.result_identifying_specific_name IS '結果識別(固有)_名称';
COMMENT ON COLUMN ext.raw_jlac10_codes.result_identifying_specific_code IS '結果識別(固有)_コード';

COMMENT ON TABLE ext.jis_compat_maps IS 'ext.jis_compat_maps';
COMMENT ON COLUMN ext.jis_compat_maps.source_text IS '3. source_text';
COMMENT ON COLUMN ext.jis_compat_maps.mapped_text IS '7. mapped_text';

COMMENT ON TABLE ext.houjin_numbers IS 'ext.houjin_numbers';
COMMENT ON COLUMN ext.houjin_numbers.corporate_number IS '8. corporate_number';
COMMENT ON COLUMN ext.houjin_numbers.name IS '13. name';
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
COMMENT ON COLUMN ext.zip_codes.zip_code IS 'zip_code';
COMMENT ON COLUMN ext.zip_codes.prefecture_katakana IS 'prefecture_katakana';
COMMENT ON COLUMN ext.zip_codes.city_katakana IS 'city_katakana';
COMMENT ON COLUMN ext.zip_codes.town_katakana IS 'town_katakana';
COMMENT ON COLUMN ext.zip_codes.prefecture IS 'prefecture';
COMMENT ON COLUMN ext.zip_codes.city IS 'city';
COMMENT ON COLUMN ext.zip_codes.town IS 'town';

COMMENT ON TABLE ext.raw_jis_compat_maps IS 'ext.raw_jis_compat_maps';
COMMENT ON COLUMN ext.raw_jis_compat_maps.source_menkuten_code IS '1. 面区点コード';
COMMENT ON COLUMN ext.raw_jis_compat_maps.source_unicode IS '2. Unicode';
COMMENT ON COLUMN ext.raw_jis_compat_maps.source_text IS '3. 字形';
COMMENT ON COLUMN ext.raw_jis_compat_maps.source_jis_kubun IS '4. JIS区分';
COMMENT ON COLUMN ext.raw_jis_compat_maps.mapped_menkuten_code IS '5. 面区点コード';
COMMENT ON COLUMN ext.raw_jis_compat_maps.mapped_unicode IS '6. Unicode';
COMMENT ON COLUMN ext.raw_jis_compat_maps.mapped_text IS '7. 字形';
COMMENT ON COLUMN ext.raw_jis_compat_maps.multi_menkuten_code_1 IS '8. 面区点コード①';
COMMENT ON COLUMN ext.raw_jis_compat_maps.multi_menkuten_code_2 IS '9. 面区点コード②';
COMMENT ON COLUMN ext.raw_jis_compat_maps.multi_menkuten_code_3 IS '10. 面区点コード③';
COMMENT ON COLUMN ext.raw_jis_compat_maps.multi_menkuten_code_4 IS '11. 面区点コード④';
COMMENT ON COLUMN ext.raw_jis_compat_maps.multi_unicode_1 IS '12. Unicode①';
COMMENT ON COLUMN ext.raw_jis_compat_maps.multi_unicode_2 IS '13. Unicode②';
COMMENT ON COLUMN ext.raw_jis_compat_maps.multi_unicode_3 IS '14. Unicode③';
COMMENT ON COLUMN ext.raw_jis_compat_maps.multi_unicode_4 IS '15. Unicode④';
COMMENT ON COLUMN ext.raw_jis_compat_maps.multi_text IS '16. 字形';
COMMENT ON COLUMN ext.raw_jis_compat_maps.remarks IS '17. 備考';

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

