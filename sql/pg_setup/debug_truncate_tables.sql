-- 外部キー制約を考慮して、CASCADEオプションを使用してTRUNCATE
-- CASCADEにより、参照しているテーブルも自動的にTRUNCATEされます

-- 最下層: 他のテーブルを参照しているテーブル
TRUNCATE TABLE appointment_slots CASCADE;
TRUNCATE TABLE appointment_stats CASCADE;
TRUNCATE TABLE appointments CASCADE;
TRUNCATE TABLE equipment_appointment_stats CASCADE;
TRUNCATE TABLE equipment_appointments CASCADE;
TRUNCATE TABLE equipment_slots CASCADE;
TRUNCATE TABLE facility_addresses CASCADE;
TRUNCATE TABLE facility_business_hours CASCADE;
TRUNCATE TABLE facility_policies CASCADE;
TRUNCATE TABLE facility_user_permissions_cache CASCADE;
TRUNCATE TABLE facility_user_roles CASCADE;
TRUNCATE TABLE organization_addresses CASCADE;
TRUNCATE TABLE organization_insurances CASCADE;
TRUNCATE TABLE organization_members CASCADE;
TRUNCATE TABLE patient_addresses CASCADE;
TRUNCATE TABLE patient_insurance_cards CASCADE;
TRUNCATE TABLE role_permissions CASCADE;
TRUNCATE TABLE user_preferences CASCADE;

-- 中間層: 他のテーブルを参照しているが、さらに参照されているテーブル
TRUNCATE TABLE equipments CASCADE;
TRUNCATE TABLE floors CASCADE;
TRUNCATE TABLE organizations CASCADE;
TRUNCATE TABLE patients CASCADE;
TRUNCATE TABLE facility_users CASCADE;

-- 上位層: 基本的なエンティティテーブル
TRUNCATE TABLE facilities CASCADE;
TRUNCATE TABLE permissions CASCADE;

-- 最上位: 他のテーブルに依存していない、または最小限の依存のみ
TRUNCATE TABLE users CASCADE;
TRUNCATE TABLE policies CASCADE;
TRUNCATE TABLE roles CASCADE;
TRUNCATE TABLE resources CASCADE;
TRUNCATE TABLE operations CASCADE;
TRUNCATE TABLE preferences CASCADE;
TRUNCATE TABLE holidays CASCADE;
TRUNCATE TABLE sessions CASCADE;
TRUNCATE TABLE insurance_providers CASCADE;
TRUNCATE TABLE tenants CASCADE;
