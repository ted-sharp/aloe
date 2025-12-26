-- Project Name : aloe
-- Date/Time    : 2025/12/26 22:36:10
-- Author       : ted
-- RDBMS Type   : PostgreSQL
-- Application  : A5:SQL Mk-2

/*
  << 注意！！ >>
  BackupToTempTable, RestoreFromTempTable疑似命令が付加されています。
  これにより、drop table, create table 後もデータが残ります。
  この機能は一時的に $$TableName のような一時テーブルを作成します。
  この機能は A5:SQL Mk-2でのみ有効であることに注意してください。
*/

-- appointment_resource_assignments
-- * BackupToTempTable
DROP TABLE if exists "appointment_resource_assignments" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "appointment_resource_assignments" (
  "appt_res_assign_id" UUID DEFAULT uuidv7() NOT NULL
  , "appt_id" UUID NOT NULL
  , "appt_res_id" UUID NOT NULL
  , "appt_start_time" time
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "appointment_resource_assignments_IX1"
  ON "appointment_resource_assignments"("appt_id");

CREATE INDEX "appointment_resource_assignments_IX2"
  ON "appointment_resource_assignments"("appt_res_id");

CREATE UNIQUE INDEX "appointment_resource_assignments_PKI"
  ON "appointment_resource_assignments"("appt_res_assign_id");

ALTER TABLE "appointment_resource_assignments"
  ADD CONSTRAINT "appointment_resource_assignments_PKC" PRIMARY KEY ("appt_res_assign_id");

-- appointment_resource_group_members
-- * BackupToTempTable
DROP TABLE if exists "appointment_resource_group_members" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "appointment_resource_group_members" (
  "appt_res_group_member_id" UUID DEFAULT uuidv7() NOT NULL
  , "appt_res_id" UUID NOT NULL
  , "appt_res_group_id" UUID NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "appointment_resource_group_members_IX1"
  ON "appointment_resource_group_members"("appt_res_id");

CREATE INDEX "appointment_resource_group_members_IX2"
  ON "appointment_resource_group_members"("appt_res_group_id");

CREATE UNIQUE INDEX "appointment_resource_group_members_PKI"
  ON "appointment_resource_group_members"("appt_res_group_member_id");

ALTER TABLE "appointment_resource_group_members"
  ADD CONSTRAINT "appointment_resource_group_members_PKC" PRIMARY KEY ("appt_res_group_member_id");

-- appointment_resource_groups
-- * BackupToTempTable
DROP TABLE if exists "appointment_resource_groups" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "appointment_resource_groups" (
  "appt_res_group_id" UUID DEFAULT uuidv7() NOT NULL
  , "facility_id" UUID NOT NULL
  , "res_group_code" character varying(20) DEFAULT '' NOT NULL
  , "res_group_name" character varying(100) DEFAULT '' NOT NULL
  , "res_group_desc" character varying(1000) DEFAULT '' NOT NULL
  , "res_group_seq" integer DEFAULT 0 NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "appointment_resource_groups_IX1"
  ON "appointment_resource_groups"("facility_id");

CREATE UNIQUE INDEX "appointment_resource_groups_PKI"
  ON "appointment_resource_groups"("appt_res_group_id");

ALTER TABLE "appointment_resource_groups"
  ADD CONSTRAINT "appointment_resource_groups_PKC" PRIMARY KEY ("appt_res_group_id");

-- appointment_slot_overrides
-- * BackupToTempTable
DROP TABLE if exists "appointment_slot_overrides" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "appointment_slot_overrides" (
  "appt_slot_override_id" UUID DEFAULT uuidv7() NOT NULL
  , "appt_date" date DEFAULT CURRENT_DATE NOT NULL
  , "appt_res_id" UUID NOT NULL
  , "appt_slots" JSONB DEFAULT '{}' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "appointment_slot_overrides_IX1"
  ON "appointment_slot_overrides"("appt_date","appt_res_id") WHERE is_deleted = FALSE;

CREATE UNIQUE INDEX "appointment_slot_overrides_PKI"
  ON "appointment_slot_overrides"("appt_slot_override_id");

ALTER TABLE "appointment_slot_overrides"
  ADD CONSTRAINT "appointment_slot_overrides_PKC" PRIMARY KEY ("appt_slot_override_id");

-- appointment_slots
-- * BackupToTempTable
DROP TABLE if exists "appointment_slots" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "appointment_slots" (
  "appt_slot_id" UUID DEFAULT uuidv7() NOT NULL
  , "appt_res_id" UUID NOT NULL
  , "appt_slots" JSONB DEFAULT '{}' NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "active_from" date DEFAULT CURRENT_DATE NOT NULL
  , "active_to" date DEFAULT '9999-12-31' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "appointment_slots_IX1"
  ON "appointment_slots"("appt_res_id");

CREATE UNIQUE INDEX "appointment_slots_PKI"
  ON "appointment_slots"("appt_slot_id");

ALTER TABLE "appointment_slots"
  ADD CONSTRAINT "appointment_slots_PKC" PRIMARY KEY ("appt_slot_id");

-- appointment_stat_slots
-- * BackupToTempTable
DROP TABLE if exists "appointment_stat_slots" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "appointment_stat_slots" (
  "appt_stat_slot_id" UUID DEFAULT uuidv7() NOT NULL
  , "appt_stat_id" UUID NOT NULL
  , "appt_date" date NOT NULL
  , "appt_res_id" UUID NOT NULL
  , "slot_start" integer DEFAULT 0 NOT NULL
  , "slot_end" integer DEFAULT 0 NOT NULL
  , "slot_cap" integer DEFAULT 0 NOT NULL
  , "slot_count" integer DEFAULT 0 NOT NULL
  , "slot_available" integer NOT NULL GENERATED ALWAYS AS (slot_cap - slot_count) STORED
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "appointment_stat_slots_IX1"
  ON "appointment_stat_slots"("appt_stat_id","appt_date","appt_res_id");

CREATE INDEX "appointment_stat_slots_IX2"
  ON "appointment_stat_slots"("appt_date","appt_res_id","updated_at");

CREATE UNIQUE INDEX "appointment_stat_slots_PKI"
  ON "appointment_stat_slots"("appt_stat_slot_id");

ALTER TABLE "appointment_stat_slots"
  ADD CONSTRAINT "appointment_stat_slots_PKC" PRIMARY KEY ("appt_stat_slot_id");

-- appointment_stats
-- * BackupToTempTable
DROP TABLE if exists "appointment_stats" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "appointment_stats" (
  "appt_stat_id" UUID DEFAULT uuidv7() NOT NULL
  , "appt_date" date NOT NULL
  , "appt_res_id" UUID NOT NULL
  , "appt_cap" integer DEFAULT 0 NOT NULL
  , "appt_count" integer DEFAULT 0 NOT NULL
  , "appt_available" integer NOT NULL GENERATED ALWAYS AS (appt_cap - appt_count) STORED
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "appointment_stats_IX1"
  ON "appointment_stats"("appt_date","appt_res_id") WHERE is_deleted = FALSE;

CREATE INDEX "appointment_stats_IX2"
  ON "appointment_stats"("appt_date","appt_res_id","updated_at");

CREATE UNIQUE INDEX "appointment_stats_PKI"
  ON "appointment_stats"("appt_stat_id");

ALTER TABLE "appointment_stats"
  ADD CONSTRAINT "appointment_stats_PKC" PRIMARY KEY ("appt_stat_id");

-- appointments
-- * BackupToTempTable
DROP TABLE if exists "appointments" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "appointments" (
  "appt_id" UUID DEFAULT uuidv7() NOT NULL
  , "floor_id" UUID NOT NULL
  , "org_id" UUID NOT NULL
  , "pt_id" UUID NOT NULL
  , "appt_date" date
  , "appt_start_time" time
  , "appt_duration_min" integer
  , "appt_status_code" integer DEFAULT 0 NOT NULL
  , "appt_memo" character varying(1000) DEFAULT '' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "appointments_IX1"
  ON "appointments"("floor_id","appt_date");

CREATE INDEX "appointments_IX2"
  ON "appointments"("org_id");

CREATE INDEX "appointments_IX3"
  ON "appointments"("pt_id");

CREATE UNIQUE INDEX "appointments_PKI"
  ON "appointments"("appt_id");

ALTER TABLE "appointments"
  ADD CONSTRAINT "appointments_PKC" PRIMARY KEY ("appt_id");

-- facility_addresses
-- * BackupToTempTable
DROP TABLE if exists "facility_addresses" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "facility_addresses" (
  "facility_adr_id" UUID DEFAULT uuidv7() NOT NULL
  , "facility_id" UUID NOT NULL
  , "adr_type_code" integer DEFAULT 0 NOT NULL
  , "postal_code" character varying(7) DEFAULT '' NOT NULL
  , "adr1" character varying(100) DEFAULT '' NOT NULL
  , "adr2" character varying(100) DEFAULT '' NOT NULL
  , "adr3" character varying(100) DEFAULT '' NOT NULL
  , "attention_name" character varying(100) DEFAULT '' NOT NULL
  , "tel" character varying(20) DEFAULT '' NOT NULL
  , "tel2" character varying(20) DEFAULT '' NOT NULL
  , "fax" character varying(20) DEFAULT '' NOT NULL
  , "email" character varying(255) DEFAULT '' NOT NULL
  , "adr_memo" character varying(1000) DEFAULT '' NOT NULL
  , "adr_seq" integer DEFAULT 0 NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "facility_addresses_IX1"
  ON "facility_addresses"("facility_id");

CREATE INDEX "facility_addresses_IX2"
  ON "facility_addresses"("tel") WHERE tel <> '';

CREATE INDEX "facility_addresses_IX3"
  ON "facility_addresses"("tel2") WHERE tel2 <> '';

CREATE INDEX "facility_addresses_IX4"
  ON "facility_addresses"("email") WHERE email <> '';

CREATE UNIQUE INDEX "facility_addresses_PKI"
  ON "facility_addresses"("facility_adr_id");

ALTER TABLE "facility_addresses"
  ADD CONSTRAINT "facility_addresses_PKC" PRIMARY KEY ("facility_adr_id");

-- facility_business_hours
-- * BackupToTempTable
DROP TABLE if exists "facility_business_hours" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "facility_business_hours" (
  "facility_business_hours_id" UUID DEFAULT uuidv7() NOT NULL
  , "facility_id" UUID NOT NULL
  , "business_hours" JSONB DEFAULT '{}' NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "active_from" date DEFAULT CURRENT_DATE NOT NULL
  , "active_to" date DEFAULT '9999-12-31' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "facility_business_hours_IX1"
  ON "facility_business_hours"("facility_id");

CREATE UNIQUE INDEX "facility_business_hours_PKI"
  ON "facility_business_hours"("facility_business_hours_id");

ALTER TABLE "facility_business_hours"
  ADD CONSTRAINT "facility_business_hours_PKC" PRIMARY KEY ("facility_business_hours_id");

-- facility_policies
-- * BackupToTempTable
DROP TABLE if exists "facility_policies" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "facility_policies" (
  "facility_policy_id" UUID DEFAULT uuidv7() NOT NULL
  , "facility_id" UUID NOT NULL
  , "policy_code" character varying(100) NOT NULL
  , "policy_value" character varying(10) DEFAULT '' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "facility_policies_IX1"
  ON "facility_policies"("facility_id","policy_code") WHERE is_deleted = FALSE;

CREATE UNIQUE INDEX "facility_policies_PKI"
  ON "facility_policies"("facility_policy_id");

ALTER TABLE "facility_policies"
  ADD CONSTRAINT "facility_policies_PKC" PRIMARY KEY ("facility_policy_id");

-- facility_user_permissions_cache
-- * BackupToTempTable
DROP TABLE if exists "facility_user_permissions_cache" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "facility_user_permissions_cache" (
  "facility_user_id" UUID NOT NULL
  , "permission_codes" character varying(21)[] DEFAULT '{}' NOT NULL
  , "expires_at" timestamp
) ;

CREATE UNIQUE INDEX "facility_user_permissions_cache_PKI"
  ON "facility_user_permissions_cache"("facility_user_id");

ALTER TABLE "facility_user_permissions_cache"
  ADD CONSTRAINT "facility_user_permissions_cache_PKC" PRIMARY KEY ("facility_user_id");

-- facility_user_roles
-- * BackupToTempTable
DROP TABLE if exists "facility_user_roles" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "facility_user_roles" (
  "user_role_id" UUID DEFAULT uuidv7() NOT NULL
  , "facility_user_id" UUID NOT NULL
  , "role_code" character varying(10) NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "facility_user_roles_IX1"
  ON "facility_user_roles"("facility_user_id","role_code") WHERE is_deleted = FALSE;

CREATE INDEX "facility_user_roles_IX2"
  ON "facility_user_roles"("role_code");

CREATE UNIQUE INDEX "facility_user_roles_PKI"
  ON "facility_user_roles"("user_role_id");

ALTER TABLE "facility_user_roles"
  ADD CONSTRAINT "facility_user_roles_PKC" PRIMARY KEY ("user_role_id");

-- facility_users
-- * BackupToTempTable
DROP TABLE if exists "facility_users" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "facility_users" (
  "facility_user_id" UUID DEFAULT uuidv7() NOT NULL
  , "facility_id" UUID NOT NULL
  , "user_id" UUID NOT NULL
  , "facility_user_seq" integer DEFAULT 0 NOT NULL
  , "is_facility_admin" BOOLEAN DEFAULT FALSE NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "facility_users_IX1"
  ON "facility_users"("facility_id","user_id") WHERE is_deleted = FALSE;

CREATE INDEX "facility_users_IX2"
  ON "facility_users"("user_id");

CREATE UNIQUE INDEX "facility_users_PKI"
  ON "facility_users"("facility_user_id");

ALTER TABLE "facility_users"
  ADD CONSTRAINT "facility_users_PKC" PRIMARY KEY ("facility_user_id");

-- holidays
-- * BackupToTempTable
DROP TABLE if exists "holidays" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "holidays" (
  "holiday_date" date DEFAULT CURRENT_DATE NOT NULL
  , "holiday_name" character varying(100) DEFAULT '' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "holidays_PKI"
  ON "holidays"("holiday_date");

ALTER TABLE "holidays"
  ADD CONSTRAINT "holidays_PKC" PRIMARY KEY ("holiday_date");

-- insurance_providers
-- * BackupToTempTable
DROP TABLE if exists "insurance_providers" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "insurance_providers" (
  "insurer_id" UUID NOT NULL
  , "insurer_type_code" integer DEFAULT 0 NOT NULL
  , "insurer_code" character varying(20) DEFAULT '' NOT NULL
  , "insurer_name" character varying(100) DEFAULT '' NOT NULL
  , "insurer_short_name" character varying(100) DEFAULT '' NOT NULL
  , "insurer_desc" character varying(1000) DEFAULT '' NOT NULL
  , "insurer_seq" integer DEFAULT 0 NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "insurance_providers_PKI"
  ON "insurance_providers"("insurer_id");

ALTER TABLE "insurance_providers"
  ADD CONSTRAINT "insurance_providers_PKC" PRIMARY KEY ("insurer_id");

-- organization_addresses
-- * BackupToTempTable
DROP TABLE if exists "organization_addresses" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "organization_addresses" (
  "org_adr_id" UUID DEFAULT uuidv7() NOT NULL
  , "org_id" UUID NOT NULL
  , "adr_type_code" integer DEFAULT 0 NOT NULL
  , "postal_code" character varying(7) DEFAULT '' NOT NULL
  , "adr1" character varying(100) DEFAULT '' NOT NULL
  , "adr2" character varying(100) DEFAULT '' NOT NULL
  , "adr3" character varying(100) DEFAULT '' NOT NULL
  , "attention_name" character varying(100) DEFAULT '' NOT NULL
  , "tel" character varying(20) DEFAULT '' NOT NULL
  , "tel2" character varying(20) DEFAULT '' NOT NULL
  , "fax" character varying(20) DEFAULT '' NOT NULL
  , "email" character varying(255) DEFAULT '' NOT NULL
  , "adr_memo" character varying(1000) DEFAULT '' NOT NULL
  , "adr_seq" integer DEFAULT 0 NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "organization_addresses_IX1"
  ON "organization_addresses"("org_id");

CREATE INDEX "organization_addresses_IX2"
  ON "organization_addresses"("tel") WHERE tel <> '';

CREATE INDEX "organization_addresses_IX3"
  ON "organization_addresses"("tel2") WHERE tel2 <> '';

CREATE INDEX "organization_addresses_IX4"
  ON "organization_addresses"("email") WHERE email <> '';

CREATE UNIQUE INDEX "organization_addresses_PKI"
  ON "organization_addresses"("org_adr_id");

ALTER TABLE "organization_addresses"
  ADD CONSTRAINT "organization_addresses_PKC" PRIMARY KEY ("org_adr_id");

-- organization_insurances
-- * BackupToTempTable
DROP TABLE if exists "organization_insurances" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "organization_insurances" (
  "org_insurance_id" UUID DEFAULT uuidv7() NOT NULL
  , "org_id" UUID NOT NULL
  , "is_primary" BOOLEAN DEFAULT FALSE NOT NULL
  , "insurer_id" UUID
  , "insurer_type_code" integer DEFAULT 0 NOT NULL
  , "insurer_code" character varying(20) DEFAULT '' NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "deactivated_on" date
  , "org_insurance_memo" character varying(1000) DEFAULT '' NOT NULL
  , "org_insurance_seq" integer DEFAULT 0 NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "organization_insurances_IX1"
  ON "organization_insurances"("org_id");

CREATE INDEX "organization_insurances_IX2"
  ON "organization_insurances"("insurer_id");

CREATE UNIQUE INDEX "organization_insurances_PKI"
  ON "organization_insurances"("org_insurance_id");

ALTER TABLE "organization_insurances"
  ADD CONSTRAINT "organization_insurances_PKC" PRIMARY KEY ("org_insurance_id");

-- organization_members
-- * BackupToTempTable
DROP TABLE if exists "organization_members" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "organization_members" (
  "org_member_id" UUID NOT NULL
  , "org_id" UUID NOT NULL
  , "pt_id" UUID NOT NULL
  , "personal_code" character varying(100) DEFAULT '' NOT NULL
  , "department" character varying(100) DEFAULT '' NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "deactivated_on" date
  , "org_member_memo" character varying(1000) DEFAULT '' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "organization_members_IX1"
  ON "organization_members"("org_id","pt_id");

CREATE UNIQUE INDEX "organization_members_PKI"
  ON "organization_members"("org_member_id");

ALTER TABLE "organization_members"
  ADD CONSTRAINT "organization_members_PKC" PRIMARY KEY ("org_member_id");

-- organizations
-- * BackupToTempTable
DROP TABLE if exists "organizations" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "organizations" (
  "org_id" UUID DEFAULT uuidv7() NOT NULL
  , "facility_id" UUID NOT NULL
  , "parent_org_id" UUID
  , "org_code" character varying(13) DEFAULT '' NOT NULL
  , "org_name" character varying(100) DEFAULT '' NOT NULL
  , "org_name_katakana" character varying(100) DEFAULT '' NOT NULL
  , "org_name_katakana_compat" character varying(100) DEFAULT '' NOT NULL
  , "org_name_display" character varying(100) DEFAULT '' NOT NULL
  , "org_name_print" character varying(100) DEFAULT '' NOT NULL
  , "org_memo" character varying(1000) DEFAULT '' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "organizations_IX1"
  ON "organizations"("facility_id","org_code") WHERE is_deleted = FALSE;

CREATE INDEX "organizations_IX2"
  ON "organizations"("parent_org_id") WHERE parent_org_id IS NOT NULL;

CREATE UNIQUE INDEX "organizations_PKI"
  ON "organizations"("org_id");

ALTER TABLE "organizations"
  ADD CONSTRAINT "organizations_PKC" PRIMARY KEY ("org_id");

-- patient_addresses
-- * BackupToTempTable
DROP TABLE if exists "patient_addresses" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "patient_addresses" (
  "pt_adr_id" UUID DEFAULT uuidv7() NOT NULL
  , "pt_id" UUID NOT NULL
  , "adr_type_code" integer DEFAULT 0 NOT NULL
  , "postal_code" character varying(7) DEFAULT '' NOT NULL
  , "adr1" character varying(100) DEFAULT '' NOT NULL
  , "adr2" character varying(100) DEFAULT '' NOT NULL
  , "adr3" character varying(100) DEFAULT '' NOT NULL
  , "attention_name" character varying(100) DEFAULT '' NOT NULL
  , "tel" character varying(20) DEFAULT '' NOT NULL
  , "tel2" character varying(20) DEFAULT '' NOT NULL
  , "fax" character varying(20) DEFAULT '' NOT NULL
  , "email" character varying(255) DEFAULT '' NOT NULL
  , "adr_memo" character varying(1000) DEFAULT '' NOT NULL
  , "adr_seq" integer DEFAULT 0 NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "patient_addresses_IX1"
  ON "patient_addresses"("pt_id");

CREATE INDEX "patient_addresses_IX2"
  ON "patient_addresses"("tel") WHERE tel <> '';

CREATE INDEX "patient_addresses_IX3"
  ON "patient_addresses"("tel2") WHERE tel2 <> '';

CREATE INDEX "patient_addresses_IX4"
  ON "patient_addresses"("email") WHERE email <> '';

CREATE UNIQUE INDEX "patient_addresses_PKI"
  ON "patient_addresses"("pt_adr_id");

ALTER TABLE "patient_addresses"
  ADD CONSTRAINT "patient_addresses_PKC" PRIMARY KEY ("pt_adr_id");

-- patient_insurance_cards
-- * BackupToTempTable
DROP TABLE if exists "patient_insurance_cards" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "patient_insurance_cards" (
  "pt_insur_card_id" UUID DEFAULT uuidv7() NOT NULL
  , "pt_id" UUID NOT NULL
  , "is_primary" BOOLEAN DEFAULT FALSE NOT NULL
  , "insurer_id" UUID
  , "insurer_type_code" integer DEFAULT 0 NOT NULL
  , "insurer_code" character varying(20) DEFAULT '' NOT NULL
  , "insurer_name" character varying(100) DEFAULT '' NOT NULL
  , "insured_code" character varying(20) DEFAULT '' NOT NULL
  , "insured_code_symbol" character varying(100) DEFAULT '' NOT NULL
  , "insured_code_number" character varying(20) DEFAULT '' NOT NULL
  , "insured_code_branch_number" character varying(20) DEFAULT '' NOT NULL
  , "insured_person_name" character varying(100) DEFAULT '' NOT NULL
  , "self_family_relationship_code" integer DEFAULT 0 NOT NULL
  , "assistance_code" integer DEFAULT 0 NOT NULL
  , "continuation_code" integer DEFAULT 0 NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "deactivated_on" date
  , "pt_insure_card_memo" character varying(1000) DEFAULT '' NOT NULL
  , "pt_insure_card_seq" integer DEFAULT 0 NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "patient_insurance_cards_IX1"
  ON "patient_insurance_cards"("pt_id");

CREATE INDEX "patient_insurance_cards_IX2"
  ON "patient_insurance_cards"("insurer_id");

CREATE UNIQUE INDEX "patient_insurance_cards_PKI"
  ON "patient_insurance_cards"("pt_insur_card_id");

ALTER TABLE "patient_insurance_cards"
  ADD CONSTRAINT "patient_insurance_cards_PKC" PRIMARY KEY ("pt_insur_card_id");

-- patients
-- * BackupToTempTable
DROP TABLE if exists "patients" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "patients" (
  "pt_id" UUID DEFAULT uuidv7() NOT NULL
  , "canonical_pt_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "facility_id" UUID NOT NULL
  , "primary_org_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "pt_code" character varying(100) DEFAULT '' NOT NULL
  , "karte_code" character varying(100)
  , "pt_name" character varying(100) DEFAULT '' NOT NULL
  , "pt_name_compat" character varying(100) DEFAULT '' NOT NULL
  , "pt_name_katakana" character varying(100) DEFAULT '' NOT NULL
  , "pt_name_katakana_compat" character varying(100) DEFAULT '' NOT NULL
  , "pt_maiden_name" character varying(100) DEFAULT '' NOT NULL
  , "pt_alias_name" character varying(100) DEFAULT '' NOT NULL
  , "birth_date" date DEFAULT CURRENT_DATE NOT NULL
  , "sex_code" integer DEFAULT 0 NOT NULL
  , "pt_memo" character varying(1000) DEFAULT '' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "patients_IX1"
  ON "patients"("facility_id","pt_code") WHERE is_deleted = FALSE;

CREATE UNIQUE INDEX "patients_IX2"
  ON "patients"("facility_id","karte_code") WHERE is_deleted = FALSE;

CREATE INDEX "patients_IX3"
  ON "patients"("canonical_pt_id");

CREATE INDEX "patients_IX4"
  ON "patients"("primary_org_id");

CREATE INDEX "patients_IX5"
  ON "patients"("birth_date");

CREATE UNIQUE INDEX "patients_PKI"
  ON "patients"("pt_id");

ALTER TABLE "patients"
  ADD CONSTRAINT "patients_PKC" PRIMARY KEY ("pt_id");

-- plan_condition_members
-- * BackupToTempTable
DROP TABLE if exists "plan_condition_members" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "plan_condition_members" (
  "plan_cond_member_id" UUID DEFAULT uuidv7() NOT NULL
  , "plan_id" UUID NOT NULL
  , "plan_cond_id" UUID NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "plan_condition_members_IX1"
  ON "plan_condition_members"("plan_id");

CREATE INDEX "plan_condition_members_IX2"
  ON "plan_condition_members"("plan_cond_id");

CREATE UNIQUE INDEX "plan_condition_members_PKI"
  ON "plan_condition_members"("plan_cond_member_id");

ALTER TABLE "plan_condition_members"
  ADD CONSTRAINT "plan_condition_members_PKC" PRIMARY KEY ("plan_cond_member_id");

-- plan_conditions
-- * BackupToTempTable
DROP TABLE if exists "plan_conditions" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "plan_conditions" (
  "plan_cond_id" UUID DEFAULT uuidv7() NOT NULL
  , "condition_name" character varying(100) DEFAULT '' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "plan_conditions_PKI"
  ON "plan_conditions"("plan_cond_id");

ALTER TABLE "plan_conditions"
  ADD CONSTRAINT "plan_conditions_PKC" PRIMARY KEY ("plan_cond_id");

-- plan_options
-- * BackupToTempTable
DROP TABLE if exists "plan_options" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "plan_options" (
  "plan_option_id" UUID DEFAULT uuidv7() NOT NULL
  , "plan_id" UUID NOT NULL
  , "option_plan_id" UUID NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "plan_options_IX1"
  ON "plan_options"("plan_id");

CREATE UNIQUE INDEX "plan_options_PKI"
  ON "plan_options"("plan_option_id");

ALTER TABLE "plan_options"
  ADD CONSTRAINT "plan_options_PKC" PRIMARY KEY ("plan_option_id");

-- plan_resource_requirements
-- * BackupToTempTable
DROP TABLE if exists "plan_resource_requirements" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "plan_resource_requirements" (
  "plan_res_req_id" UUID DEFAULT uuidv7() NOT NULL
  , "plan_id" UUID NOT NULL
  , "appt_res_id" UUID NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "plan_resource_requirements_IX1"
  ON "plan_resource_requirements"("plan_id");

CREATE INDEX "plan_resource_requirements_IX2"
  ON "plan_resource_requirements"("appt_res_id");

CREATE UNIQUE INDEX "plan_resource_requirements_PKI"
  ON "plan_resource_requirements"("plan_res_req_id");

ALTER TABLE "plan_resource_requirements"
  ADD CONSTRAINT "plan_resource_requirements_PKC" PRIMARY KEY ("plan_res_req_id");

-- plans
-- * BackupToTempTable
DROP TABLE if exists "plans" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "plans" (
  "plan_id" UUID DEFAULT uuidv7() NOT NULL
  , "facility_id" UUID NOT NULL
  , "plan_code" character varying(20) DEFAULT '' NOT NULL
  , "plan_name" character varying(100) DEFAULT '' NOT NULL
  , "plan_short_name" character varying(100) DEFAULT '' NOT NULL
  , "plan_abbr_name" character varying(100) DEFAULT '' NOT NULL
  , "plan_desc" character varying(1000) DEFAULT '' NOT NULL
  , "plan_kind_code" integer DEFAULT 0 NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "active_from" date DEFAULT CURRENT_DATE NOT NULL
  , "active_to" date DEFAULT '9999-12-31' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "plans_PKI"
  ON "plans"("plan_id");

ALTER TABLE "plans"
  ADD CONSTRAINT "plans_PKC" PRIMARY KEY ("plan_id");

-- policies
-- * BackupToTempTable
DROP TABLE if exists "policies" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "policies" (
  "policy_code" character varying(100) DEFAULT '' NOT NULL
  , "policy_name" character varying(100) DEFAULT '' NOT NULL
  , "policy_desc" character varying(1000) DEFAULT '' NOT NULL
  , "data_type" character varying(10) DEFAULT '' NOT NULL
  , "policy_value" character varying(10) DEFAULT '' NOT NULL
  , "policy_seq" integer DEFAULT 0 NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "policies_PKI"
  ON "policies"("policy_code");

ALTER TABLE "policies"
  ADD CONSTRAINT "policies_PKC" PRIMARY KEY ("policy_code");

-- role_permissions
-- * BackupToTempTable
DROP TABLE if exists "role_permissions" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "role_permissions" (
  "role_permission_code" character varying(32) DEFAULT '' NOT NULL
  , "role_code" character varying(10) NOT NULL
  , "permission_code" character varying(21) NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "role_permissions_IX1"
  ON "role_permissions"("role_code","permission_code") WHERE is_deleted = FALSE;

CREATE UNIQUE INDEX "role_permissions_PKI"
  ON "role_permissions"("role_permission_code");

ALTER TABLE "role_permissions"
  ADD CONSTRAINT "role_permissions_PKC" PRIMARY KEY ("role_permission_code");

-- roles
-- * BackupToTempTable
DROP TABLE if exists "roles" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "roles" (
  "role_code" character varying(10) DEFAULT '' NOT NULL
  , "role_name" character varying(100) DEFAULT '' NOT NULL
  , "role_desc" character varying(1000) DEFAULT '' NOT NULL
  , "role_seq" integer DEFAULT 0 NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "roles_PKI"
  ON "roles"("role_code");

ALTER TABLE "roles"
  ADD CONSTRAINT "roles_PKC" PRIMARY KEY ("role_code");

-- user_preferences
-- * BackupToTempTable
DROP TABLE if exists "user_preferences" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "user_preferences" (
  "user_preference_id" UUID DEFAULT uuidv7() NOT NULL
  , "user_id" UUID NOT NULL
  , "preference_code" character varying(100) NOT NULL
  , "preference_value" character varying(10) DEFAULT '' NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "user_preferences_IX1"
  ON "user_preferences"("user_id","preference_code") WHERE is_deleted = FALSE;

CREATE UNIQUE INDEX "user_preferences_PKI"
  ON "user_preferences"("user_preference_id");

ALTER TABLE "user_preferences"
  ADD CONSTRAINT "user_preferences_PKC" PRIMARY KEY ("user_preference_id");

-- user_sessions
-- * BackupToTempTable
DROP TABLE if exists "user_sessions" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "user_sessions" (
  "session_id" UUID DEFAULT uuidv7() NOT NULL
  , "user_id" UUID NOT NULL
  , "user_display_name" character varying(100) DEFAULT '' NOT NULL
  , "security_stamp" UUID NOT NULL
  , "issued_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "expires_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "revoked_at" timestamp
  , "ip_address" character varying(256) DEFAULT '' NOT NULL
  , "user_agent" character varying(512) DEFAULT '' NOT NULL
  , "app_name" character varying(100) DEFAULT '' NOT NULL
) ;

CREATE INDEX "user_sessions_IX1"
  ON "user_sessions"("user_id");

CREATE INDEX "user_sessions_IX2"
  ON "user_sessions"("security_stamp");

CREATE UNIQUE INDEX "user_sessions_PKI"
  ON "user_sessions"("session_id");

ALTER TABLE "user_sessions"
  ADD CONSTRAINT "user_sessions_PKC" PRIMARY KEY ("session_id");

-- user_tokens
-- * BackupToTempTable
DROP TABLE if exists "user_tokens" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "user_tokens" (
  "token_id" UUID DEFAULT uuidv7() NOT NULL
  , "user_id" UUID NOT NULL
  , "token_provider" integer DEFAULT 0 NOT NULL
  , "token_name" character varying(64) DEFAULT '' NOT NULL
  , "token_value" TEXT DEFAULT '' NOT NULL
) ;

CREATE UNIQUE INDEX "user_tokens_IX1"
  ON "user_tokens"("user_id","token_provider","token_name");

CREATE UNIQUE INDEX "user_tokens_PKI"
  ON "user_tokens"("token_id");

ALTER TABLE "user_tokens"
  ADD CONSTRAINT "user_tokens_PKC" PRIMARY KEY ("token_id");

-- users
-- * BackupToTempTable
DROP TABLE if exists "users" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "users" (
  "user_id" UUID DEFAULT uuidv7() NOT NULL
  , "user_code" character varying(100) NOT NULL
  , "user_display_name" character varying(100) DEFAULT '' NOT NULL
  , "password_hash" character varying(255) NOT NULL
  , "password_salt" character varying(64) NOT NULL
  , "expires_on" date DEFAULT '9999-12-31' NOT NULL
  , "access_success_total_count" integer DEFAULT 0 NOT NULL
  , "access_failed_total_count" integer DEFAULT 0 NOT NULL
  , "access_failed_count" integer DEFAULT 0 NOT NULL
  , "locked_end_at" timestamp DEFAULT '1970-01-01' NOT NULL
  , "last_login_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "last_logout_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "security_stamp" UUID DEFAULT uuidv7() NOT NULL
  , "two_factor_enabled" BOOLEAN DEFAULT FALSE NOT NULL
  , "mfa_method" integer DEFAULT 0 NOT NULL
  , "email" character varying(254) DEFAULT '' NOT NULL
  , "email_confirmed" BOOLEAN DEFAULT FALSE NOT NULL
  , "sms" character varying(20) DEFAULT '' NOT NULL
  , "sms_confirmed" BOOLEAN DEFAULT FALSE NOT NULL
  , "is_system_admin" BOOLEAN DEFAULT FALSE NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "users_IX1"
  ON "users"("user_code") WHERE is_deleted = FALSE;

CREATE UNIQUE INDEX "users_PKI"
  ON "users"("user_id");

ALTER TABLE "users"
  ADD CONSTRAINT "users_PKC" PRIMARY KEY ("user_id");

-- appointment_resources
-- * BackupToTempTable
DROP TABLE if exists "appointment_resources" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "appointment_resources" (
  "appt_res_id" UUID DEFAULT uuidv7() NOT NULL
  , "floor_id" UUID NOT NULL
  , "appt_res_type_code" integer DEFAULT 0 NOT NULL
  , "appt_res_name" character varying(100) DEFAULT '' NOT NULL
  , "appt_res_desc" character varying(1000) DEFAULT '' NOT NULL
  , "appt_res_seq" integer DEFAULT 0 NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "appointment_resources_IX1"
  ON "appointment_resources"("floor_id") WHERE is_deleted = FALSE AND appt_res_type_code = 1;

CREATE UNIQUE INDEX "appointment_resources_PKI"
  ON "appointment_resources"("appt_res_id");

ALTER TABLE "appointment_resources"
  ADD CONSTRAINT "appointment_resources_PKC" PRIMARY KEY ("appt_res_id");

-- floors
-- * BackupToTempTable
DROP TABLE if exists "floors" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "floors" (
  "floor_id" UUID DEFAULT uuidv7() NOT NULL
  , "facility_id" UUID NOT NULL
  , "floor_code" character varying(10) DEFAULT '' NOT NULL
  , "floor_name" character varying(100) DEFAULT '' NOT NULL
  , "floor_desc" character varying(1000) DEFAULT '' NOT NULL
  , "floor_seq" integer DEFAULT 0 NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "floors_IX1"
  ON "floors"("facility_id","floor_code") WHERE is_deleted = FALSE;

CREATE UNIQUE INDEX "floors_PKI"
  ON "floors"("floor_id");

ALTER TABLE "floors"
  ADD CONSTRAINT "floors_PKC" PRIMARY KEY ("floor_id");

-- permissions
-- * BackupToTempTable
DROP TABLE if exists "permissions" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "permissions" (
  "permission_code" character varying(21) DEFAULT '' NOT NULL
  , "feature_code" character varying(10) NOT NULL
  , "operation_code" character varying(10) NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "permissions_IX1"
  ON "permissions"("feature_code","operation_code") WHERE is_deleted = FALSE;

CREATE UNIQUE INDEX "permissions_PKI"
  ON "permissions"("permission_code");

ALTER TABLE "permissions"
  ADD CONSTRAINT "permissions_PKC" PRIMARY KEY ("permission_code");

-- preferences
-- * BackupToTempTable
DROP TABLE if exists "preferences" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "preferences" (
  "preference_code" character varying(100) DEFAULT '' NOT NULL
  , "preference_name" character varying(100) DEFAULT '' NOT NULL
  , "preference_desc" character varying(1000) DEFAULT '' NOT NULL
  , "data_type" character varying(10) DEFAULT '' NOT NULL
  , "preference_value" character varying(10) DEFAULT '' NOT NULL
  , "preference_seq" integer DEFAULT 0 NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "preferences_PKI"
  ON "preferences"("preference_code");

ALTER TABLE "preferences"
  ADD CONSTRAINT "preferences_PKC" PRIMARY KEY ("preference_code");

-- facilities
-- * BackupToTempTable
DROP TABLE if exists "facilities" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "facilities" (
  "facility_id" UUID DEFAULT uuidv7() NOT NULL
  , "tenant_id" UUID NOT NULL
  , "medical_institution_code" character varying(10) DEFAULT '' NOT NULL
  , "facility_name" character varying(100) DEFAULT '' NOT NULL
  , "facility_name_display" character varying(100) DEFAULT '' NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "active_from" date DEFAULT CURRENT_DATE NOT NULL
  , "active_to" date
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE INDEX "facilities_IX1"
  ON "facilities"("tenant_id");

CREATE UNIQUE INDEX "facilities_PKI"
  ON "facilities"("facility_id");

ALTER TABLE "facilities"
  ADD CONSTRAINT "facilities_PKC" PRIMARY KEY ("facility_id");

-- features
-- * BackupToTempTable
DROP TABLE if exists "features" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "features" (
  "feature_code" character varying(10) DEFAULT '' NOT NULL
  , "feature_type_code" integer DEFAULT 0 NOT NULL
  , "feature_name" character varying(100) DEFAULT '' NOT NULL
  , "feature_desc" character varying(1000) DEFAULT '' NOT NULL
  , "feature_seq" integer DEFAULT 0 NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "features_PKI"
  ON "features"("feature_code");

ALTER TABLE "features"
  ADD CONSTRAINT "features_PKC" PRIMARY KEY ("feature_code");

-- operations
-- * BackupToTempTable
DROP TABLE if exists "operations" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "operations" (
  "operation_code" character varying(10) DEFAULT '' NOT NULL
  , "operation_name" character varying(100) DEFAULT '' NOT NULL
  , "operation_desc" character varying(1000) DEFAULT '' NOT NULL
  , "operation_seq" integer DEFAULT 0 NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "operations_PKI"
  ON "operations"("operation_code");

ALTER TABLE "operations"
  ADD CONSTRAINT "operations_PKC" PRIMARY KEY ("operation_code");

-- tenants
-- * BackupToTempTable
DROP TABLE if exists "tenants" CASCADE;

-- * RestoreFromTempTable
CREATE TABLE "tenants" (
  "tenant_id" UUID DEFAULT uuidv7() NOT NULL
  , "tenant_name" character varying(100) DEFAULT '' NOT NULL
  , "is_active" BOOLEAN DEFAULT FALSE NOT NULL
  , "active_from" date DEFAULT CURRENT_DATE NOT NULL
  , "active_to" date
  , "is_deleted" BOOLEAN DEFAULT FALSE NOT NULL
  , "created_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "created_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "created_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_at" timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL
  , "updated_user_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
  , "updated_session_id" UUID DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL
) ;

CREATE UNIQUE INDEX "tenants_PKI"
  ON "tenants"("tenant_id");

ALTER TABLE "tenants"
  ADD CONSTRAINT "tenants_PKC" PRIMARY KEY ("tenant_id");

ALTER TABLE "appointment_resource_assignments"
  ADD CONSTRAINT "appointment_resource_assignments_FK1" FOREIGN KEY ("appt_id") REFERENCES "appointments"("appt_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointment_resource_assignments"
  ADD CONSTRAINT "appointment_resource_assignments_FK2" FOREIGN KEY ("appt_res_id") REFERENCES "appointment_resources"("appt_res_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointment_resource_group_members"
  ADD CONSTRAINT "appointment_resource_group_members_FK1" FOREIGN KEY ("appt_res_group_id") REFERENCES "appointment_resource_groups"("appt_res_group_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointment_resource_group_members"
  ADD CONSTRAINT "appointment_resource_group_members_FK2" FOREIGN KEY ("appt_res_id") REFERENCES "appointment_resources"("appt_res_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointment_resource_groups"
  ADD CONSTRAINT "appointment_resource_groups_FK1" FOREIGN KEY ("facility_id") REFERENCES "facilities"("facility_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointment_resources"
  ADD CONSTRAINT "appointment_resources_FK1" FOREIGN KEY ("floor_id") REFERENCES "floors"("floor_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointment_slot_overrides"
  ADD CONSTRAINT "appointment_slot_overrides_FK1" FOREIGN KEY ("appt_res_id") REFERENCES "appointment_resources"("appt_res_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointment_slots"
  ADD CONSTRAINT "appointment_slots_FK1" FOREIGN KEY ("appt_res_id") REFERENCES "appointment_resources"("appt_res_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointment_stat_slots"
  ADD CONSTRAINT "appointment_stat_slots_FK1" FOREIGN KEY ("appt_stat_id") REFERENCES "appointment_stats"("appt_stat_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointment_stats"
  ADD CONSTRAINT "appointment_stats_FK1" FOREIGN KEY ("appt_res_id") REFERENCES "appointment_resources"("appt_res_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointments"
  ADD CONSTRAINT "appointments_FK1" FOREIGN KEY ("org_id") REFERENCES "organizations"("org_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointments"
  ADD CONSTRAINT "appointments_FK2" FOREIGN KEY ("pt_id") REFERENCES "patients"("pt_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "appointments"
  ADD CONSTRAINT "appointments_FK3" FOREIGN KEY ("floor_id") REFERENCES "floors"("floor_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "facilities"
  ADD CONSTRAINT "facilities_FK1" FOREIGN KEY ("tenant_id") REFERENCES "tenants"("tenant_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "facility_addresses"
  ADD CONSTRAINT "facility_addresses_FK1" FOREIGN KEY ("facility_id") REFERENCES "facilities"("facility_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "facility_business_hours"
  ADD CONSTRAINT "facility_business_hours_FK1" FOREIGN KEY ("facility_id") REFERENCES "facilities"("facility_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "facility_policies"
  ADD CONSTRAINT "facility_policies_FK1" FOREIGN KEY ("facility_id") REFERENCES "facilities"("facility_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "facility_policies"
  ADD CONSTRAINT "facility_policies_FK2" FOREIGN KEY ("policy_code") REFERENCES "policies"("policy_code")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "facility_user_permissions_cache"
  ADD CONSTRAINT "facility_user_permissions_cache_FK1" FOREIGN KEY ("facility_user_id") REFERENCES "facility_users"("facility_user_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "facility_user_roles"
  ADD CONSTRAINT "facility_user_roles_FK1" FOREIGN KEY ("role_code") REFERENCES "roles"("role_code")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "facility_user_roles"
  ADD CONSTRAINT "facility_user_roles_FK2" FOREIGN KEY ("facility_user_id") REFERENCES "facility_users"("facility_user_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "facility_users"
  ADD CONSTRAINT "facility_users_FK1" FOREIGN KEY ("user_id") REFERENCES "users"("user_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "floors"
  ADD CONSTRAINT "floors_FK1" FOREIGN KEY ("facility_id") REFERENCES "facilities"("facility_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "organization_addresses"
  ADD CONSTRAINT "organization_addresses_FK1" FOREIGN KEY ("org_id") REFERENCES "organizations"("org_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "organization_insurances"
  ADD CONSTRAINT "organization_insurances_FK1" FOREIGN KEY ("org_id") REFERENCES "organizations"("org_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "organization_members"
  ADD CONSTRAINT "organization_members_FK1" FOREIGN KEY ("org_id") REFERENCES "organizations"("org_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "organization_members"
  ADD CONSTRAINT "organization_members_FK2" FOREIGN KEY ("pt_id") REFERENCES "patients"("pt_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "organizations"
  ADD CONSTRAINT "organizations_FK1" FOREIGN KEY ("facility_id") REFERENCES "facilities"("facility_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "patient_addresses"
  ADD CONSTRAINT "patient_addresses_FK1" FOREIGN KEY ("pt_id") REFERENCES "patients"("pt_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "patient_insurance_cards"
  ADD CONSTRAINT "patient_insurance_cards_FK1" FOREIGN KEY ("pt_id") REFERENCES "patients"("pt_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "patients"
  ADD CONSTRAINT "patients_FK1" FOREIGN KEY ("facility_id") REFERENCES "facilities"("facility_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "permissions"
  ADD CONSTRAINT "permissions_FK1" FOREIGN KEY ("feature_code") REFERENCES "features"("feature_code")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "permissions"
  ADD CONSTRAINT "permissions_FK2" FOREIGN KEY ("operation_code") REFERENCES "operations"("operation_code")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "plan_condition_members"
  ADD CONSTRAINT "plan_condition_members_FK1" FOREIGN KEY ("plan_id") REFERENCES "plans"("plan_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "plan_condition_members"
  ADD CONSTRAINT "plan_condition_members_FK2" FOREIGN KEY ("plan_cond_id") REFERENCES "plan_conditions"("plan_cond_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "plan_options"
  ADD CONSTRAINT "plan_options_FK1" FOREIGN KEY ("plan_id") REFERENCES "plans"("plan_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "plan_options"
  ADD CONSTRAINT "plan_options_FK2" FOREIGN KEY ("option_plan_id") REFERENCES "plans"("plan_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "plan_resource_requirements"
  ADD CONSTRAINT "plan_resource_requirements_FK1" FOREIGN KEY ("plan_id") REFERENCES "plans"("plan_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "plan_resource_requirements"
  ADD CONSTRAINT "plan_resource_requirements_FK2" FOREIGN KEY ("appt_res_id") REFERENCES "appointment_resources"("appt_res_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "role_permissions"
  ADD CONSTRAINT "role_permissions_FK1" FOREIGN KEY ("role_code") REFERENCES "roles"("role_code")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "role_permissions"
  ADD CONSTRAINT "role_permissions_FK2" FOREIGN KEY ("permission_code") REFERENCES "permissions"("permission_code")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "user_preferences"
  ADD CONSTRAINT "user_preferences_FK1" FOREIGN KEY ("preference_code") REFERENCES "preferences"("preference_code")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "user_preferences"
  ADD CONSTRAINT "user_preferences_FK2" FOREIGN KEY ("user_id") REFERENCES "users"("user_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "user_sessions"
  ADD CONSTRAINT "user_sessions_FK1" FOREIGN KEY ("user_id") REFERENCES "users"("user_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

ALTER TABLE "user_tokens"
  ADD CONSTRAINT "user_tokens_FK1" FOREIGN KEY ("user_id") REFERENCES "users"("user_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;

COMMENT ON TABLE "appointment_resource_assignments" IS 'appointment_resource_assignments';
COMMENT ON COLUMN "appointment_resource_assignments"."appt_res_assign_id" IS 'appt_res_assign_id';
COMMENT ON COLUMN "appointment_resource_assignments"."appt_id" IS 'appt_id';
COMMENT ON COLUMN "appointment_resource_assignments"."appt_res_id" IS 'appt_res_id';
COMMENT ON COLUMN "appointment_resource_assignments"."appt_start_time" IS 'appt_start_time';
COMMENT ON COLUMN "appointment_resource_assignments"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "appointment_resource_assignments"."created_at" IS 'created_at';
COMMENT ON COLUMN "appointment_resource_assignments"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "appointment_resource_assignments"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "appointment_resource_assignments"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "appointment_resource_assignments"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "appointment_resource_assignments"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "appointment_resource_group_members" IS 'appointment_resource_group_members';
COMMENT ON COLUMN "appointment_resource_group_members"."appt_res_group_member_id" IS 'appt_res_group_member_id';
COMMENT ON COLUMN "appointment_resource_group_members"."appt_res_id" IS 'appt_res_id';
COMMENT ON COLUMN "appointment_resource_group_members"."appt_res_group_id" IS 'appt_res_group_id';
COMMENT ON COLUMN "appointment_resource_group_members"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "appointment_resource_group_members"."created_at" IS 'created_at';
COMMENT ON COLUMN "appointment_resource_group_members"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "appointment_resource_group_members"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "appointment_resource_group_members"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "appointment_resource_group_members"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "appointment_resource_group_members"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "appointment_resource_groups" IS 'appointment_resource_groups';
COMMENT ON COLUMN "appointment_resource_groups"."appt_res_group_id" IS 'appt_res_group_id';
COMMENT ON COLUMN "appointment_resource_groups"."facility_id" IS 'facility_id';
COMMENT ON COLUMN "appointment_resource_groups"."res_group_code" IS 'res_group_code';
COMMENT ON COLUMN "appointment_resource_groups"."res_group_name" IS 'res_group_name';
COMMENT ON COLUMN "appointment_resource_groups"."res_group_desc" IS 'res_group_desc';
COMMENT ON COLUMN "appointment_resource_groups"."res_group_seq" IS 'res_group_seq';
COMMENT ON COLUMN "appointment_resource_groups"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "appointment_resource_groups"."created_at" IS 'created_at';
COMMENT ON COLUMN "appointment_resource_groups"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "appointment_resource_groups"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "appointment_resource_groups"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "appointment_resource_groups"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "appointment_resource_groups"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "appointment_slot_overrides" IS 'appointment_slot_overrides';
COMMENT ON COLUMN "appointment_slot_overrides"."appt_slot_override_id" IS 'appt_slot_override_id';
COMMENT ON COLUMN "appointment_slot_overrides"."appt_date" IS 'appt_date';
COMMENT ON COLUMN "appointment_slot_overrides"."appt_res_id" IS 'appt_res_id';
COMMENT ON COLUMN "appointment_slot_overrides"."appt_slots" IS 'appt_slots';
COMMENT ON COLUMN "appointment_slot_overrides"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "appointment_slot_overrides"."created_at" IS 'created_at';
COMMENT ON COLUMN "appointment_slot_overrides"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "appointment_slot_overrides"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "appointment_slot_overrides"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "appointment_slot_overrides"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "appointment_slot_overrides"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "appointment_slots" IS 'appointment_slots';
COMMENT ON COLUMN "appointment_slots"."appt_slot_id" IS 'appt_slot_id';
COMMENT ON COLUMN "appointment_slots"."appt_res_id" IS 'appt_res_id';
COMMENT ON COLUMN "appointment_slots"."appt_slots" IS 'appt_slots';
COMMENT ON COLUMN "appointment_slots"."is_active" IS 'is_active';
COMMENT ON COLUMN "appointment_slots"."active_from" IS 'active_from:期限内の古い方から順に適用していく';
COMMENT ON COLUMN "appointment_slots"."active_to" IS 'active_to:排他';
COMMENT ON COLUMN "appointment_slots"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "appointment_slots"."created_at" IS 'created_at';
COMMENT ON COLUMN "appointment_slots"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "appointment_slots"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "appointment_slots"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "appointment_slots"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "appointment_slots"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "appointment_stat_slots" IS 'appointment_stat_slots';
COMMENT ON COLUMN "appointment_stat_slots"."appt_stat_slot_id" IS 'appt_stat_slot_id';
COMMENT ON COLUMN "appointment_stat_slots"."appt_stat_id" IS 'appt_stat_id';
COMMENT ON COLUMN "appointment_stat_slots"."appt_date" IS 'appt_date';
COMMENT ON COLUMN "appointment_stat_slots"."appt_res_id" IS 'appt_res_id';
COMMENT ON COLUMN "appointment_stat_slots"."slot_start" IS 'slot_start';
COMMENT ON COLUMN "appointment_stat_slots"."slot_end" IS 'slot_end';
COMMENT ON COLUMN "appointment_stat_slots"."slot_cap" IS 'slot_cap';
COMMENT ON COLUMN "appointment_stat_slots"."slot_count" IS 'slot_count';
COMMENT ON COLUMN "appointment_stat_slots"."slot_available" IS 'slot_available';
COMMENT ON COLUMN "appointment_stat_slots"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "appointment_stat_slots"."created_at" IS 'created_at';
COMMENT ON COLUMN "appointment_stat_slots"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "appointment_stat_slots"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "appointment_stat_slots"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "appointment_stat_slots"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "appointment_stat_slots"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "appointment_stats" IS 'appointment_stats';
COMMENT ON COLUMN "appointment_stats"."appt_stat_id" IS 'appt_stat_id';
COMMENT ON COLUMN "appointment_stats"."appt_date" IS 'appt_date';
COMMENT ON COLUMN "appointment_stats"."appt_res_id" IS 'appt_res_id';
COMMENT ON COLUMN "appointment_stats"."appt_cap" IS 'appt_cap';
COMMENT ON COLUMN "appointment_stats"."appt_count" IS 'appt_count';
COMMENT ON COLUMN "appointment_stats"."appt_available" IS 'appt_available';
COMMENT ON COLUMN "appointment_stats"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "appointment_stats"."created_at" IS 'created_at';
COMMENT ON COLUMN "appointment_stats"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "appointment_stats"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "appointment_stats"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "appointment_stats"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "appointment_stats"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "appointments" IS 'appointments';
COMMENT ON COLUMN "appointments"."appt_id" IS 'appt_id';
COMMENT ON COLUMN "appointments"."floor_id" IS 'floor_id';
COMMENT ON COLUMN "appointments"."org_id" IS 'org_id';
COMMENT ON COLUMN "appointments"."pt_id" IS 'pt_id';
COMMENT ON COLUMN "appointments"."appt_date" IS 'appt_date:未定がある';
COMMENT ON COLUMN "appointments"."appt_start_time" IS 'appt_start_time';
COMMENT ON COLUMN "appointments"."appt_duration_min" IS 'appt_duration_min';
COMMENT ON COLUMN "appointments"."appt_status_code" IS 'appt_status_code:仮押、予約、来院済み、検査完了、キャンセル、無断キャンセル';
COMMENT ON COLUMN "appointments"."appt_memo" IS 'appt_memo';
COMMENT ON COLUMN "appointments"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "appointments"."created_at" IS 'created_at';
COMMENT ON COLUMN "appointments"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "appointments"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "appointments"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "appointments"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "appointments"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "facility_addresses" IS 'facility_addresses';
COMMENT ON COLUMN "facility_addresses"."facility_adr_id" IS 'facility_adr_id';
COMMENT ON COLUMN "facility_addresses"."facility_id" IS 'facility_id';
COMMENT ON COLUMN "facility_addresses"."adr_type_code" IS 'adr_type_code';
COMMENT ON COLUMN "facility_addresses"."postal_code" IS 'postal_code';
COMMENT ON COLUMN "facility_addresses"."adr1" IS 'adr1';
COMMENT ON COLUMN "facility_addresses"."adr2" IS 'adr2';
COMMENT ON COLUMN "facility_addresses"."adr3" IS 'adr3';
COMMENT ON COLUMN "facility_addresses"."attention_name" IS 'attention_name';
COMMENT ON COLUMN "facility_addresses"."tel" IS 'tel';
COMMENT ON COLUMN "facility_addresses"."tel2" IS 'tel2';
COMMENT ON COLUMN "facility_addresses"."fax" IS 'fax';
COMMENT ON COLUMN "facility_addresses"."email" IS 'email';
COMMENT ON COLUMN "facility_addresses"."adr_memo" IS 'adr_memo';
COMMENT ON COLUMN "facility_addresses"."adr_seq" IS 'adr_seq';
COMMENT ON COLUMN "facility_addresses"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "facility_addresses"."created_at" IS 'created_at';
COMMENT ON COLUMN "facility_addresses"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "facility_addresses"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "facility_addresses"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "facility_addresses"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "facility_addresses"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "facility_business_hours" IS 'facility_business_hours';
COMMENT ON COLUMN "facility_business_hours"."facility_business_hours_id" IS 'facility_business_hours_id';
COMMENT ON COLUMN "facility_business_hours"."facility_id" IS 'facility_id';
COMMENT ON COLUMN "facility_business_hours"."business_hours" IS 'business_hours';
COMMENT ON COLUMN "facility_business_hours"."is_active" IS 'is_active';
COMMENT ON COLUMN "facility_business_hours"."active_from" IS 'active_from:期限内の古い方から順に適用していく';
COMMENT ON COLUMN "facility_business_hours"."active_to" IS 'active_to:排他';
COMMENT ON COLUMN "facility_business_hours"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "facility_business_hours"."created_at" IS 'created_at';
COMMENT ON COLUMN "facility_business_hours"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "facility_business_hours"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "facility_business_hours"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "facility_business_hours"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "facility_business_hours"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "facility_policies" IS 'facility_policies';
COMMENT ON COLUMN "facility_policies"."facility_policy_id" IS 'facility_policy_id';
COMMENT ON COLUMN "facility_policies"."facility_id" IS 'facility_id';
COMMENT ON COLUMN "facility_policies"."policy_code" IS 'policy_code';
COMMENT ON COLUMN "facility_policies"."policy_value" IS 'policy_value';
COMMENT ON COLUMN "facility_policies"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "facility_policies"."created_at" IS 'created_at';
COMMENT ON COLUMN "facility_policies"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "facility_policies"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "facility_policies"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "facility_policies"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "facility_policies"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "facility_user_permissions_cache" IS 'facility_user_permissions_cache:いつ消失してもよい、なければ作成';
COMMENT ON COLUMN "facility_user_permissions_cache"."facility_user_id" IS 'facility_user_id';
COMMENT ON COLUMN "facility_user_permissions_cache"."permission_codes" IS 'permission_codes';
COMMENT ON COLUMN "facility_user_permissions_cache"."expires_at" IS 'expires_at';

COMMENT ON TABLE "facility_user_roles" IS 'facility_user_roles';
COMMENT ON COLUMN "facility_user_roles"."user_role_id" IS 'user_role_id';
COMMENT ON COLUMN "facility_user_roles"."facility_user_id" IS 'facility_user_id';
COMMENT ON COLUMN "facility_user_roles"."role_code" IS 'role_code';
COMMENT ON COLUMN "facility_user_roles"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "facility_user_roles"."created_at" IS 'created_at';
COMMENT ON COLUMN "facility_user_roles"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "facility_user_roles"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "facility_user_roles"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "facility_user_roles"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "facility_user_roles"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "facility_users" IS 'facility_users';
COMMENT ON COLUMN "facility_users"."facility_user_id" IS 'facility_user_id';
COMMENT ON COLUMN "facility_users"."facility_id" IS 'facility_id';
COMMENT ON COLUMN "facility_users"."user_id" IS 'user_id';
COMMENT ON COLUMN "facility_users"."facility_user_seq" IS 'facility_user_seq';
COMMENT ON COLUMN "facility_users"."is_facility_admin" IS 'is_facility_admin';
COMMENT ON COLUMN "facility_users"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "facility_users"."created_at" IS 'created_at';
COMMENT ON COLUMN "facility_users"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "facility_users"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "facility_users"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "facility_users"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "facility_users"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "holidays" IS 'holidays';
COMMENT ON COLUMN "holidays"."holiday_date" IS 'holiday_date';
COMMENT ON COLUMN "holidays"."holiday_name" IS 'holiday_name';
COMMENT ON COLUMN "holidays"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "holidays"."created_at" IS 'created_at';
COMMENT ON COLUMN "holidays"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "holidays"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "holidays"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "holidays"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "holidays"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "insurance_providers" IS 'insurance_providers:テーブル名を insurers としないのは、保険者と被保険者の区別を明確にするために provider という表現を優先したためです。';
COMMENT ON COLUMN "insurance_providers"."insurer_id" IS 'insurer_id';
COMMENT ON COLUMN "insurance_providers"."insurer_type_code" IS 'insurer_type_code:0=None, 1=協会けんぽ, 2=代行機関, 3=健康保険組合, 4=国保, 5=その他';
COMMENT ON COLUMN "insurance_providers"."insurer_code" IS 'insurer_code:可能であれば保険者番号';
COMMENT ON COLUMN "insurance_providers"."insurer_name" IS 'insurer_name';
COMMENT ON COLUMN "insurance_providers"."insurer_short_name" IS 'insurer_short_name';
COMMENT ON COLUMN "insurance_providers"."insurer_desc" IS 'insurer_desc';
COMMENT ON COLUMN "insurance_providers"."insurer_seq" IS 'insurer_seq';
COMMENT ON COLUMN "insurance_providers"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "insurance_providers"."created_at" IS 'created_at';
COMMENT ON COLUMN "insurance_providers"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "insurance_providers"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "insurance_providers"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "insurance_providers"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "insurance_providers"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "organization_addresses" IS 'organization_addresses';
COMMENT ON COLUMN "organization_addresses"."org_adr_id" IS 'org_adr_id';
COMMENT ON COLUMN "organization_addresses"."org_id" IS 'org_id';
COMMENT ON COLUMN "organization_addresses"."adr_type_code" IS 'adr_type_code';
COMMENT ON COLUMN "organization_addresses"."postal_code" IS 'postal_code';
COMMENT ON COLUMN "organization_addresses"."adr1" IS 'adr1';
COMMENT ON COLUMN "organization_addresses"."adr2" IS 'adr2';
COMMENT ON COLUMN "organization_addresses"."adr3" IS 'adr3';
COMMENT ON COLUMN "organization_addresses"."attention_name" IS 'attention_name';
COMMENT ON COLUMN "organization_addresses"."tel" IS 'tel';
COMMENT ON COLUMN "organization_addresses"."tel2" IS 'tel2';
COMMENT ON COLUMN "organization_addresses"."fax" IS 'fax';
COMMENT ON COLUMN "organization_addresses"."email" IS 'email';
COMMENT ON COLUMN "organization_addresses"."adr_memo" IS 'adr_memo';
COMMENT ON COLUMN "organization_addresses"."adr_seq" IS 'adr_seq';
COMMENT ON COLUMN "organization_addresses"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "organization_addresses"."created_at" IS 'created_at';
COMMENT ON COLUMN "organization_addresses"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "organization_addresses"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "organization_addresses"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "organization_addresses"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "organization_addresses"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "organization_insurances" IS 'organization_insurances';
COMMENT ON COLUMN "organization_insurances"."org_insurance_id" IS 'org_insurance_id';
COMMENT ON COLUMN "organization_insurances"."org_id" IS 'org_id';
COMMENT ON COLUMN "organization_insurances"."is_primary" IS 'is_primary:主保険';
COMMENT ON COLUMN "organization_insurances"."insurer_id" IS 'insurer_id:あれば';
COMMENT ON COLUMN "organization_insurances"."insurer_type_code" IS 'insurer_type_code:0=None, 1=協会けんぽ, 2=代行機関, 3=健康保険組合, 4=国保, 5=その他';
COMMENT ON COLUMN "organization_insurances"."insurer_code" IS 'insurer_code';
COMMENT ON COLUMN "organization_insurances"."is_active" IS 'is_active';
COMMENT ON COLUMN "organization_insurances"."deactivated_on" IS 'deactivated_on:無効日';
COMMENT ON COLUMN "organization_insurances"."org_insurance_memo" IS 'org_insurance_memo';
COMMENT ON COLUMN "organization_insurances"."org_insurance_seq" IS 'org_insurance_seq';
COMMENT ON COLUMN "organization_insurances"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "organization_insurances"."created_at" IS 'created_at';
COMMENT ON COLUMN "organization_insurances"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "organization_insurances"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "organization_insurances"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "organization_insurances"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "organization_insurances"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "organization_members" IS 'organization_members';
COMMENT ON COLUMN "organization_members"."org_member_id" IS 'org_member_id';
COMMENT ON COLUMN "organization_members"."org_id" IS 'org_id:複数回同じ会社もありえる';
COMMENT ON COLUMN "organization_members"."pt_id" IS 'pt_id';
COMMENT ON COLUMN "organization_members"."personal_code" IS 'personal_code:社員番号、学生番号';
COMMENT ON COLUMN "organization_members"."department" IS 'department';
COMMENT ON COLUMN "organization_members"."is_active" IS 'is_active';
COMMENT ON COLUMN "organization_members"."deactivated_on" IS 'deactivated_on';
COMMENT ON COLUMN "organization_members"."org_member_memo" IS 'org_member_memo';
COMMENT ON COLUMN "organization_members"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "organization_members"."created_at" IS 'created_at';
COMMENT ON COLUMN "organization_members"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "organization_members"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "organization_members"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "organization_members"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "organization_members"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "organizations" IS 'organizations';
COMMENT ON COLUMN "organizations"."org_id" IS 'org_id';
COMMENT ON COLUMN "organizations"."facility_id" IS 'facility_id';
COMMENT ON COLUMN "organizations"."parent_org_id" IS 'parent_org_id';
COMMENT ON COLUMN "organizations"."org_code" IS 'org_code:病院採番や法人番号';
COMMENT ON COLUMN "organizations"."org_name" IS 'org_name';
COMMENT ON COLUMN "organizations"."org_name_katakana" IS 'org_name_katakana';
COMMENT ON COLUMN "organizations"."org_name_katakana_compat" IS 'org_name_katakana_compat';
COMMENT ON COLUMN "organizations"."org_name_display" IS 'org_name_display';
COMMENT ON COLUMN "organizations"."org_name_print" IS 'org_name_print';
COMMENT ON COLUMN "organizations"."org_memo" IS 'org_memo';
COMMENT ON COLUMN "organizations"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "organizations"."created_at" IS 'created_at';
COMMENT ON COLUMN "organizations"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "organizations"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "organizations"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "organizations"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "organizations"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "patient_addresses" IS 'patient_addresses';
COMMENT ON COLUMN "patient_addresses"."pt_adr_id" IS 'pt_adr_id';
COMMENT ON COLUMN "patient_addresses"."pt_id" IS 'pt_id';
COMMENT ON COLUMN "patient_addresses"."adr_type_code" IS 'adr_type_code';
COMMENT ON COLUMN "patient_addresses"."postal_code" IS 'postal_code';
COMMENT ON COLUMN "patient_addresses"."adr1" IS 'adr1';
COMMENT ON COLUMN "patient_addresses"."adr2" IS 'adr2';
COMMENT ON COLUMN "patient_addresses"."adr3" IS 'adr3';
COMMENT ON COLUMN "patient_addresses"."attention_name" IS 'attention_name';
COMMENT ON COLUMN "patient_addresses"."tel" IS 'tel';
COMMENT ON COLUMN "patient_addresses"."tel2" IS 'tel2';
COMMENT ON COLUMN "patient_addresses"."fax" IS 'fax';
COMMENT ON COLUMN "patient_addresses"."email" IS 'email';
COMMENT ON COLUMN "patient_addresses"."adr_memo" IS 'adr_memo';
COMMENT ON COLUMN "patient_addresses"."adr_seq" IS 'adr_seq';
COMMENT ON COLUMN "patient_addresses"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "patient_addresses"."created_at" IS 'created_at';
COMMENT ON COLUMN "patient_addresses"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "patient_addresses"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "patient_addresses"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "patient_addresses"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "patient_addresses"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "patient_insurance_cards" IS 'patient_insurance_cards';
COMMENT ON COLUMN "patient_insurance_cards"."pt_insur_card_id" IS 'pt_insur_card_id';
COMMENT ON COLUMN "patient_insurance_cards"."pt_id" IS 'pt_id';
COMMENT ON COLUMN "patient_insurance_cards"."is_primary" IS 'is_primary:主保険';
COMMENT ON COLUMN "patient_insurance_cards"."insurer_id" IS 'insurer_id:あれば';
COMMENT ON COLUMN "patient_insurance_cards"."insurer_type_code" IS 'insurer_type_code:0=None, 1=協会けんぽ, 2=代行機関, 3=健康保険組合, 4=国保, 5=その他';
COMMENT ON COLUMN "patient_insurance_cards"."insurer_code" IS 'insurer_code';
COMMENT ON COLUMN "patient_insurance_cards"."insurer_name" IS 'insurer_name:保険者名';
COMMENT ON COLUMN "patient_insurance_cards"."insured_code" IS 'insured_code:記号、番号、枝番';
COMMENT ON COLUMN "patient_insurance_cards"."insured_code_symbol" IS 'insured_code_symbol:記号';
COMMENT ON COLUMN "patient_insurance_cards"."insured_code_number" IS 'insured_code_number:番号';
COMMENT ON COLUMN "patient_insurance_cards"."insured_code_branch_number" IS 'insured_code_branch_number:枝番';
COMMENT ON COLUMN "patient_insurance_cards"."insured_person_name" IS 'insured_person_name:被保険者名';
COMMENT ON COLUMN "patient_insurance_cards"."self_family_relationship_code" IS 'self_family_relationship_code:本人家族区分(1=Self[本人], 2=Dependents[家族])';
COMMENT ON COLUMN "patient_insurance_cards"."assistance_code" IS 'assistance_code:補助区分(0=国保0割,A船員1割)';
COMMENT ON COLUMN "patient_insurance_cards"."continuation_code" IS 'continuation_code:任意継続(0=None, 1=ExtendedCare[継続療養], 2=VoluntaryContinuation[任意継続])';
COMMENT ON COLUMN "patient_insurance_cards"."is_active" IS 'is_active';
COMMENT ON COLUMN "patient_insurance_cards"."deactivated_on" IS 'deactivated_on:無効日';
COMMENT ON COLUMN "patient_insurance_cards"."pt_insure_card_memo" IS 'pt_insure_card_memo';
COMMENT ON COLUMN "patient_insurance_cards"."pt_insure_card_seq" IS 'pt_insure_card_seq';
COMMENT ON COLUMN "patient_insurance_cards"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "patient_insurance_cards"."created_at" IS 'created_at';
COMMENT ON COLUMN "patient_insurance_cards"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "patient_insurance_cards"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "patient_insurance_cards"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "patient_insurance_cards"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "patient_insurance_cards"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "patients" IS 'patients';
COMMENT ON COLUMN "patients"."pt_id" IS 'pt_id';
COMMENT ON COLUMN "patients"."canonical_pt_id" IS 'canonical_pt_id:名寄せ検索用';
COMMENT ON COLUMN "patients"."facility_id" IS 'facility_id';
COMMENT ON COLUMN "patients"."primary_org_id" IS 'primary_org_id';
COMMENT ON COLUMN "patients"."pt_code" IS 'pt_code';
COMMENT ON COLUMN "patients"."karte_code" IS 'karte_code';
COMMENT ON COLUMN "patients"."pt_name" IS 'pt_name';
COMMENT ON COLUMN "patients"."pt_name_compat" IS 'pt_name_compat:JIS縮退で第二水準までにしたもの';
COMMENT ON COLUMN "patients"."pt_name_katakana" IS 'pt_name_katakana';
COMMENT ON COLUMN "patients"."pt_name_katakana_compat" IS 'pt_name_katakana_compat:トリガで更新、全角化、区切り文字の統一、unaccent';
COMMENT ON COLUMN "patients"."pt_maiden_name" IS 'pt_maiden_name:結婚後の名寄せなどで使用';
COMMENT ON COLUMN "patients"."pt_alias_name" IS 'pt_alias_name:通名、有名人隠匿用';
COMMENT ON COLUMN "patients"."birth_date" IS 'birth_date';
COMMENT ON COLUMN "patients"."sex_code" IS 'sex_code:0: None, 1: Man, 2: Woman, 9: Unknown';
COMMENT ON COLUMN "patients"."pt_memo" IS 'pt_memo';
COMMENT ON COLUMN "patients"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "patients"."created_at" IS 'created_at';
COMMENT ON COLUMN "patients"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "patients"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "patients"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "patients"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "patients"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "plan_condition_members" IS 'plan_condition_members';
COMMENT ON COLUMN "plan_condition_members"."plan_cond_member_id" IS 'plan_cond_member_id';
COMMENT ON COLUMN "plan_condition_members"."plan_id" IS 'plan_id';
COMMENT ON COLUMN "plan_condition_members"."plan_cond_id" IS 'plan_cond_id';
COMMENT ON COLUMN "plan_condition_members"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "plan_condition_members"."created_at" IS 'created_at';
COMMENT ON COLUMN "plan_condition_members"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "plan_condition_members"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "plan_condition_members"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "plan_condition_members"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "plan_condition_members"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "plan_conditions" IS 'plan_conditions';
COMMENT ON COLUMN "plan_conditions"."plan_cond_id" IS 'plan_cond_id';
COMMENT ON COLUMN "plan_conditions"."condition_name" IS 'condition_name';
COMMENT ON COLUMN "plan_conditions"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "plan_conditions"."created_at" IS 'created_at';
COMMENT ON COLUMN "plan_conditions"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "plan_conditions"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "plan_conditions"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "plan_conditions"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "plan_conditions"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "plan_options" IS 'plan_options';
COMMENT ON COLUMN "plan_options"."plan_option_id" IS 'plan_option_id:論理削除の履歴を保持するために必要';
COMMENT ON COLUMN "plan_options"."plan_id" IS 'plan_id';
COMMENT ON COLUMN "plan_options"."option_plan_id" IS 'option_plan_id';
COMMENT ON COLUMN "plan_options"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "plan_options"."created_at" IS 'created_at';
COMMENT ON COLUMN "plan_options"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "plan_options"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "plan_options"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "plan_options"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "plan_options"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "plan_resource_requirements" IS 'plan_resource_requirements';
COMMENT ON COLUMN "plan_resource_requirements"."plan_res_req_id" IS 'plan_res_req_id';
COMMENT ON COLUMN "plan_resource_requirements"."plan_id" IS 'plan_id';
COMMENT ON COLUMN "plan_resource_requirements"."appt_res_id" IS 'appt_res_id';
COMMENT ON COLUMN "plan_resource_requirements"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "plan_resource_requirements"."created_at" IS 'created_at';
COMMENT ON COLUMN "plan_resource_requirements"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "plan_resource_requirements"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "plan_resource_requirements"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "plan_resource_requirements"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "plan_resource_requirements"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "plans" IS 'plans';
COMMENT ON COLUMN "plans"."plan_id" IS 'plan_id';
COMMENT ON COLUMN "plans"."facility_id" IS 'facility_id';
COMMENT ON COLUMN "plans"."plan_code" IS 'plan_code';
COMMENT ON COLUMN "plans"."plan_name" IS 'plan_name';
COMMENT ON COLUMN "plans"."plan_short_name" IS 'plan_short_name';
COMMENT ON COLUMN "plans"."plan_abbr_name" IS 'plan_abbr_name';
COMMENT ON COLUMN "plans"."plan_desc" IS 'plan_desc';
COMMENT ON COLUMN "plans"."plan_kind_code" IS 'plan_kind_code';
COMMENT ON COLUMN "plans"."is_active" IS 'is_active';
COMMENT ON COLUMN "plans"."active_from" IS 'active_from';
COMMENT ON COLUMN "plans"."active_to" IS 'active_to';
COMMENT ON COLUMN "plans"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "plans"."created_at" IS 'created_at';
COMMENT ON COLUMN "plans"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "plans"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "plans"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "plans"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "plans"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "policies" IS 'policies';
COMMENT ON COLUMN "policies"."policy_code" IS 'policy_code';
COMMENT ON COLUMN "policies"."policy_name" IS 'policy_name';
COMMENT ON COLUMN "policies"."policy_desc" IS 'policy_desc';
COMMENT ON COLUMN "policies"."data_type" IS 'data_type';
COMMENT ON COLUMN "policies"."policy_value" IS 'policy_value';
COMMENT ON COLUMN "policies"."policy_seq" IS 'policy_seq';
COMMENT ON COLUMN "policies"."is_active" IS 'is_active';
COMMENT ON COLUMN "policies"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "policies"."created_at" IS 'created_at';
COMMENT ON COLUMN "policies"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "policies"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "policies"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "policies"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "policies"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "role_permissions" IS 'role_permissions';
COMMENT ON COLUMN "role_permissions"."role_permission_code" IS 'role_permission_code';
COMMENT ON COLUMN "role_permissions"."role_code" IS 'role_code';
COMMENT ON COLUMN "role_permissions"."permission_code" IS 'permission_code';
COMMENT ON COLUMN "role_permissions"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "role_permissions"."created_at" IS 'created_at';
COMMENT ON COLUMN "role_permissions"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "role_permissions"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "role_permissions"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "role_permissions"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "role_permissions"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "roles" IS 'roles';
COMMENT ON COLUMN "roles"."role_code" IS 'role_code';
COMMENT ON COLUMN "roles"."role_name" IS 'role_name';
COMMENT ON COLUMN "roles"."role_desc" IS 'role_desc';
COMMENT ON COLUMN "roles"."role_seq" IS 'role_seq';
COMMENT ON COLUMN "roles"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "roles"."created_at" IS 'created_at';
COMMENT ON COLUMN "roles"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "roles"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "roles"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "roles"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "roles"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "user_preferences" IS 'user_preferences';
COMMENT ON COLUMN "user_preferences"."user_preference_id" IS 'user_preference_id:論理削除の履歴を保持するために必要';
COMMENT ON COLUMN "user_preferences"."user_id" IS 'user_id';
COMMENT ON COLUMN "user_preferences"."preference_code" IS 'preference_code';
COMMENT ON COLUMN "user_preferences"."preference_value" IS 'preference_value';
COMMENT ON COLUMN "user_preferences"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "user_preferences"."created_at" IS 'created_at';
COMMENT ON COLUMN "user_preferences"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "user_preferences"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "user_preferences"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "user_preferences"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "user_preferences"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "user_sessions" IS 'user_sessions';
COMMENT ON COLUMN "user_sessions"."session_id" IS 'session_id';
COMMENT ON COLUMN "user_sessions"."user_id" IS 'user_id';
COMMENT ON COLUMN "user_sessions"."user_display_name" IS 'user_display_name';
COMMENT ON COLUMN "user_sessions"."security_stamp" IS 'security_stamp';
COMMENT ON COLUMN "user_sessions"."issued_at" IS 'issued_at';
COMMENT ON COLUMN "user_sessions"."expires_at" IS 'expires_at';
COMMENT ON COLUMN "user_sessions"."revoked_at" IS 'revoked_at';
COMMENT ON COLUMN "user_sessions"."ip_address" IS 'ip_address';
COMMENT ON COLUMN "user_sessions"."user_agent" IS 'user_agent';
COMMENT ON COLUMN "user_sessions"."app_name" IS 'app_name:バージョン含む';

COMMENT ON TABLE "user_tokens" IS 'user_tokens';
COMMENT ON COLUMN "user_tokens"."token_id" IS 'token_id';
COMMENT ON COLUMN "user_tokens"."user_id" IS 'user_id';
COMMENT ON COLUMN "user_tokens"."token_provider" IS 'token_provider:0: None, 1: App, 2: Totp, 3: WebAuthn, 4: Email, 5: Sms, 99: Others';
COMMENT ON COLUMN "user_tokens"."token_name" IS 'token_name';
COMMENT ON COLUMN "user_tokens"."token_value" IS 'token_value';

COMMENT ON TABLE "users" IS 'users';
COMMENT ON COLUMN "users"."user_id" IS 'user_id';
COMMENT ON COLUMN "users"."user_code" IS 'user_code:ログイン用ID';
COMMENT ON COLUMN "users"."user_display_name" IS 'user_display_name';
COMMENT ON COLUMN "users"."password_hash" IS 'password_hash';
COMMENT ON COLUMN "users"."password_salt" IS 'password_salt';
COMMENT ON COLUMN "users"."expires_on" IS 'expires_on';
COMMENT ON COLUMN "users"."access_success_total_count" IS 'access_success_total_count';
COMMENT ON COLUMN "users"."access_failed_total_count" IS 'access_failed_total_count';
COMMENT ON COLUMN "users"."access_failed_count" IS 'access_failed_count:ログイン成功でリセットします';
COMMENT ON COLUMN "users"."locked_end_at" IS 'locked_end_at:ロックされたら更新します、ロックの前回値としてずっと保持します';
COMMENT ON COLUMN "users"."last_login_at" IS 'last_login_at';
COMMENT ON COLUMN "users"."last_logout_at" IS 'last_logout_at:last_login_atより前を探すため初期値は同じとします。';
COMMENT ON COLUMN "users"."security_stamp" IS 'security_stamp';
COMMENT ON COLUMN "users"."two_factor_enabled" IS 'two_factor_enabled';
COMMENT ON COLUMN "users"."mfa_method" IS 'mfa_method:0: None, 1: App, 2: Totp, 3: WebAuthn, 4: Email, 5: Sms, 99: Others';
COMMENT ON COLUMN "users"."email" IS 'email:メールアドレスも併用';
COMMENT ON COLUMN "users"."email_confirmed" IS 'email_confirmed';
COMMENT ON COLUMN "users"."sms" IS 'sms';
COMMENT ON COLUMN "users"."sms_confirmed" IS 'sms_confirmed';
COMMENT ON COLUMN "users"."is_system_admin" IS 'is_system_admin';
COMMENT ON COLUMN "users"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "users"."created_at" IS 'created_at';
COMMENT ON COLUMN "users"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "users"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "users"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "users"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "users"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "appointment_resources" IS 'appointment_resources';
COMMENT ON COLUMN "appointment_resources"."appt_res_id" IS 'appt_res_id';
COMMENT ON COLUMN "appointment_resources"."floor_id" IS 'floor_id';
COMMENT ON COLUMN "appointment_resources"."appt_res_type_code" IS 'appt_res_type_code:0: None, 1: Main, 2: Equipment, 3: Environment, 99: Others';
COMMENT ON COLUMN "appointment_resources"."appt_res_name" IS 'appt_res_name';
COMMENT ON COLUMN "appointment_resources"."appt_res_desc" IS 'appt_res_desc';
COMMENT ON COLUMN "appointment_resources"."appt_res_seq" IS 'appt_res_seq';
COMMENT ON COLUMN "appointment_resources"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "appointment_resources"."created_at" IS 'created_at';
COMMENT ON COLUMN "appointment_resources"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "appointment_resources"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "appointment_resources"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "appointment_resources"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "appointment_resources"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "floors" IS 'floors';
COMMENT ON COLUMN "floors"."floor_id" IS 'floor_id';
COMMENT ON COLUMN "floors"."facility_id" IS 'facility_id';
COMMENT ON COLUMN "floors"."floor_code" IS 'floor_code';
COMMENT ON COLUMN "floors"."floor_name" IS 'floor_name';
COMMENT ON COLUMN "floors"."floor_desc" IS 'floor_desc';
COMMENT ON COLUMN "floors"."floor_seq" IS 'floor_seq';
COMMENT ON COLUMN "floors"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "floors"."created_at" IS 'created_at';
COMMENT ON COLUMN "floors"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "floors"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "floors"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "floors"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "floors"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "permissions" IS 'permissions';
COMMENT ON COLUMN "permissions"."permission_code" IS 'permission_code';
COMMENT ON COLUMN "permissions"."feature_code" IS 'feature_code';
COMMENT ON COLUMN "permissions"."operation_code" IS 'operation_code';
COMMENT ON COLUMN "permissions"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "permissions"."created_at" IS 'created_at';
COMMENT ON COLUMN "permissions"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "permissions"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "permissions"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "permissions"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "permissions"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "preferences" IS 'preferences';
COMMENT ON COLUMN "preferences"."preference_code" IS 'preference_code';
COMMENT ON COLUMN "preferences"."preference_name" IS 'preference_name';
COMMENT ON COLUMN "preferences"."preference_desc" IS 'preference_desc';
COMMENT ON COLUMN "preferences"."data_type" IS 'data_type';
COMMENT ON COLUMN "preferences"."preference_value" IS 'preference_value';
COMMENT ON COLUMN "preferences"."preference_seq" IS 'preference_seq';
COMMENT ON COLUMN "preferences"."is_active" IS 'is_active';
COMMENT ON COLUMN "preferences"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "preferences"."created_at" IS 'created_at';
COMMENT ON COLUMN "preferences"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "preferences"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "preferences"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "preferences"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "preferences"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "facilities" IS 'facilities';
COMMENT ON COLUMN "facilities"."facility_id" IS 'facility_id';
COMMENT ON COLUMN "facilities"."tenant_id" IS 'tenant_id';
COMMENT ON COLUMN "facilities"."medical_institution_code" IS 'medical_institution_code:医療機関コード';
COMMENT ON COLUMN "facilities"."facility_name" IS 'facility_name';
COMMENT ON COLUMN "facilities"."facility_name_display" IS 'facility_name_display';
COMMENT ON COLUMN "facilities"."is_active" IS 'is_active';
COMMENT ON COLUMN "facilities"."active_from" IS 'active_from';
COMMENT ON COLUMN "facilities"."active_to" IS 'active_to';
COMMENT ON COLUMN "facilities"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "facilities"."created_at" IS 'created_at';
COMMENT ON COLUMN "facilities"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "facilities"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "facilities"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "facilities"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "facilities"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "features" IS 'features';
COMMENT ON COLUMN "features"."feature_code" IS 'feature_code';
COMMENT ON COLUMN "features"."feature_type_code" IS 'feature_type_code:0: Undefined, 1: Screen, 2: UseCase, 3: DataSet, 4: Report, 5: FileStorage, 6: ExternalSystem, 99: Others';
COMMENT ON COLUMN "features"."feature_name" IS 'feature_name';
COMMENT ON COLUMN "features"."feature_desc" IS 'feature_desc';
COMMENT ON COLUMN "features"."feature_seq" IS 'feature_seq';
COMMENT ON COLUMN "features"."is_active" IS 'is_active';
COMMENT ON COLUMN "features"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "features"."created_at" IS 'created_at';
COMMENT ON COLUMN "features"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "features"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "features"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "features"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "features"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "operations" IS 'operations';
COMMENT ON COLUMN "operations"."operation_code" IS 'operation_code';
COMMENT ON COLUMN "operations"."operation_name" IS 'operation_name';
COMMENT ON COLUMN "operations"."operation_desc" IS 'operation_desc';
COMMENT ON COLUMN "operations"."operation_seq" IS 'operation_seq';
COMMENT ON COLUMN "operations"."is_active" IS 'is_active';
COMMENT ON COLUMN "operations"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "operations"."created_at" IS 'created_at';
COMMENT ON COLUMN "operations"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "operations"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "operations"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "operations"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "operations"."updated_session_id" IS 'updated_session_id';

COMMENT ON TABLE "tenants" IS 'tenants';
COMMENT ON COLUMN "tenants"."tenant_id" IS 'tenant_id';
COMMENT ON COLUMN "tenants"."tenant_name" IS 'tenant_name';
COMMENT ON COLUMN "tenants"."is_active" IS 'is_active';
COMMENT ON COLUMN "tenants"."active_from" IS 'active_from';
COMMENT ON COLUMN "tenants"."active_to" IS 'active_to';
COMMENT ON COLUMN "tenants"."is_deleted" IS 'is_deleted';
COMMENT ON COLUMN "tenants"."created_at" IS 'created_at';
COMMENT ON COLUMN "tenants"."created_user_id" IS 'created_user_id';
COMMENT ON COLUMN "tenants"."created_session_id" IS 'created_session_id';
COMMENT ON COLUMN "tenants"."updated_at" IS 'updated_at';
COMMENT ON COLUMN "tenants"."updated_user_id" IS 'updated_user_id';
COMMENT ON COLUMN "tenants"."updated_session_id" IS 'updated_session_id';

