-- 外部キー制約を考慮して、CASCADEオプションを使用してTRUNCATE
-- CASCADEにより、参照しているテーブルも自動的にTRUNCATEされます

-- 最下層: 複数のテーブルを参照しているテーブル
TRUNCATE TABLE appointment_resource_assignments CASCADE;
TRUNCATE TABLE appointment_resource_group_members CASCADE;
TRUNCATE TABLE plan_condition_members CASCADE;
TRUNCATE TABLE plan_options CASCADE;
TRUNCATE TABLE plan_resource_requirements CASCADE;
TRUNCATE TABLE role_permissions CASCADE;
TRUNCATE TABLE user_preferences CASCADE;
TRUNCATE TABLE facility_user_roles CASCADE;
TRUNCATE TABLE facility_policies CASCADE;
TRUNCATE TABLE organization_members CASCADE;

-- 中間層: 1つのテーブルを参照しているテーブル
TRUNCATE TABLE appointment_slot_overrides CASCADE;
TRUNCATE TABLE appointment_slots CASCADE;
TRUNCATE TABLE appointment_stats CASCADE;
TRUNCATE TABLE facility_addresses CASCADE;
TRUNCATE TABLE facility_business_hours CASCADE;
TRUNCATE TABLE facility_user_permissions_cache CASCADE;
TRUNCATE TABLE organization_addresses CASCADE;
TRUNCATE TABLE organization_insurances CASCADE;
TRUNCATE TABLE patient_addresses CASCADE;
TRUNCATE TABLE patient_insurance_cards CASCADE;

-- 上位層: 他のテーブルを参照しているが、さらに参照されているテーブル
TRUNCATE TABLE appointments CASCADE;
TRUNCATE TABLE appointment_resources CASCADE;
TRUNCATE TABLE appointment_resource_groups CASCADE;
TRUNCATE TABLE facility_users CASCADE;
TRUNCATE TABLE organizations CASCADE;
TRUNCATE TABLE patients CASCADE;
TRUNCATE TABLE plans CASCADE;
TRUNCATE TABLE plan_conditions CASCADE;

-- さらに上位: 基本的なエンティティテーブル
TRUNCATE TABLE facilities CASCADE;
TRUNCATE TABLE floors CASCADE;
TRUNCATE TABLE permissions CASCADE;

-- 最上位: 他のテーブルに依存していない、または最小限の依存のみ
TRUNCATE TABLE users CASCADE;
TRUNCATE TABLE policies CASCADE;
TRUNCATE TABLE roles CASCADE;
TRUNCATE TABLE preferences CASCADE;
TRUNCATE TABLE holidays CASCADE;
TRUNCATE TABLE insurance_providers CASCADE;
TRUNCATE TABLE tenants CASCADE;
TRUNCATE TABLE features CASCADE;
TRUNCATE TABLE operations CASCADE;
TRUNCATE TABLE user_sessions CASCADE;
TRUNCATE TABLE user_tokens CASCADE;

-- 監査ログテーブル
TRUNCATE TABLE usr_sessions CASCADE;
--TRUNCATE TABLE change_logs CASCADE;
--TRUNCATE TABLE cache_updates CASCADE;
--TRUNCATE TABLE slow_operation_logs CASCADE;
