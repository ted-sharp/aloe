
DROP INDEX IF EXISTS users_IX1;

DROP INDEX IF EXISTS organizations_IX1;
DROP INDEX IF EXISTS organizations_IX2;
DROP INDEX IF EXISTS organizations_IX3;

DROP INDEX IF EXISTS patients_IX1;
DROP INDEX IF EXISTS patients_IX2;
DROP INDEX IF EXISTS patients_IX3;
DROP INDEX IF EXISTS patients_IX4;

DROP INDEX IF EXISTS reservation_equipment_slots_IX1;
DROP INDEX IF EXISTS reservation_equipment_bookings_IX1;
DROP INDEX IF EXISTS reservation_equipment_bookings_IX2;
DROP INDEX IF EXISTS reservation_equipment_bookings_IX3;
DROP INDEX IF EXISTS reservation_equipment_bookings_IX4;
DROP INDEX IF EXISTS reservation_equipment_bookings_IX_is_held;

DROP INDEX IF EXISTS reservation_daily_slots_IX1;
DROP INDEX IF EXISTS reservation_daily_bookings_IX1;
DROP INDEX IF EXISTS reservation_daily_bookings_IX2;
DROP INDEX IF EXISTS reservation_daily_bookings_IX3;
DROP INDEX IF EXISTS reservation_daily_bookings_IX4;
DROP INDEX IF EXISTS reservation_daily_bookings_IX_is_held;





CREATE INDEX users_IX1
  ON users(login_name);





CREATE INDEX organizations_IX1
  ON organizations(parent_org_id);

CREATE INDEX organizations_IX2
  ON organizations(org_name_katakana_normalized);

CREATE INDEX organizations_IX3
  ON organizations(org_name);





CREATE INDEX patients_IX1
  ON patients(pt_full_name);

CREATE INDEX patients_IX2
  ON patients(pt_full_name_katakana_normalized);

CREATE INDEX patients_IX3
  ON patients(pt_given_name);

CREATE INDEX patients_IX4
  ON patients(birth_date);





CREATE INDEX reservation_equipment_slots_IX1
  ON reservation_equipment_slots(start_date, end_date, equip_id);

CREATE INDEX reservation_equipment_bookings_IX1
  ON reservation_equipment_bookings(bkg_date, equip_id);

CREATE INDEX reservation_equipment_bookings_IX2
  ON reservation_equipment_bookings(org_id);

CREATE INDEX reservation_equipment_bookings_IX3
  ON reservation_equipment_bookings(pt_id);

CREATE INDEX reservation_equipment_bookings_IX4
  ON reservation_equipment_bookings(order_id, sub_order_id);

CREATE INDEX reservation_equipment_bookings_IX_is_held
  ON reservation_equipment_bookings(bkg_user_id)
  WHERE is_held = TRUE;





CREATE INDEX reservation_daily_slots_IX1
  ON reservation_daily_slots(start_date, end_date, floor_id);

CREATE INDEX reservation_daily_bookings_IX1
  ON reservation_daily_bookings(bkg_date, floor_id);

CREATE INDEX reservation_daily_bookings_IX2
  ON reservation_daily_bookings(org_id);

CREATE INDEX reservation_daily_bookings_IX3
  ON reservation_daily_bookings(pt_id);

CREATE INDEX reservation_daily_bookings_IX4
  ON reservation_daily_bookings(order_id, sub_order_id);

CREATE INDEX reservation_daily_bookings_IX_is_held
  ON reservation_daily_bookings(bkg_user_id)
  WHERE is_held = TRUE;





