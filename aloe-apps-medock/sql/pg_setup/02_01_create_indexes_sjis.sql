
-- TODO: DROP ÇçÏÇÈ


DROP INDEX IF EXISTS users_IX1;
DROP INDEX IF EXISTS user_preferences_IX1;
DROP INDEX IF EXISTS user_roles_IX1;
DROP INDEX IF EXISTS role_permissions_IX1;

DROP INDEX IF EXISTS organizations_IX1;
DROP INDEX IF EXISTS organizations_IX2;
DROP INDEX IF EXISTS organizations_IX3;
DROP INDEX IF EXISTS organizations_IX4;
DROP INDEX IF EXISTS organization_members_IX1;

DROP INDEX IF EXISTS patients_IX1;
DROP INDEX IF EXISTS patients_IX2;
DROP INDEX IF EXISTS patients_IX3;
DROP INDEX IF EXISTS patients_IX4;
DROP INDEX IF EXISTS patients_IX5;
DROP INDEX IF EXISTS patients_IX6;
DROP INDEX IF EXISTS patients_IX7;

DROP INDEX IF EXISTS patient_duplicates_IX1;
DROP INDEX IF EXISTS patient_duplicates_IX2;

DROP INDEX IF EXISTS customer_locations_IX1;
DROP INDEX IF EXISTS customer_locations_IX2;
DROP INDEX IF EXISTS customer_locations_IX3;
DROP INDEX IF EXISTS customer_locations_IX4;
DROP INDEX IF EXISTS customer_marks_IX1;
DROP INDEX IF EXISTS customer_marks_IX2;
DROP INDEX IF EXISTS customer_notes_IX1;
DROP INDEX IF EXISTS customer_notes_IX2;
DROP INDEX IF EXISTS customer_files_IX1;
DROP INDEX IF EXISTS customer_files_IX2;

DROP INDEX IF EXISTS patient_insurance_cards_IX1;
DROP INDEX IF EXISTS patient_insurance_cards_IX2;


DROP INDEX IF EXISTS organization_locks_IX1;
DROP INDEX IF EXISTS patient_locks_IX1;
DROP INDEX IF EXISTS organization_logs_IX1;
DROP INDEX IF EXISTS patient_logs_IX1;
DROP INDEX IF EXISTS user_logs_IX1;


DROP INDEX IF EXISTS reservation_floors_IX1;
DROP INDEX IF EXISTS reservation_rooms_IX1;
DROP INDEX IF EXISTS reservation_room_details_IX1;
DROP INDEX IF EXISTS reservation_room_details_IX2;

DROP INDEX IF EXISTS reservation_daily_slots_IX1;
DROP INDEX IF EXISTS reservation_daily_caps_IX1;
DROP INDEX IF EXISTS reservation_daily_slot_caps_IX1;
DROP INDEX IF EXISTS reservation_daily_notes_IX1;

DROP INDEX IF EXISTS reservation_daily_bookings_IX1;
DROP INDEX IF EXISTS reservation_daily_bookings_IX2;
DROP INDEX IF EXISTS reservation_daily_bookings_IX3;
DROP INDEX IF EXISTS reservation_daily_bookings_IX4;
DROP INDEX IF EXISTS reservation_daily_bookings_IX_is_tentative;


DROP INDEX IF EXISTS reservation_equipment_slots_IX1;
DROP INDEX IF EXISTS reservation_equipment_bookings_IX1;
DROP INDEX IF EXISTS reservation_equipment_bookings_IX2;
DROP INDEX IF EXISTS reservation_equipment_bookings_IX3;
DROP INDEX IF EXISTS reservation_equipment_bookings_IX4;
DROP INDEX IF EXISTS reservation_equipment_bookings_IX_is_tentative;





CREATE INDEX users_IX1
  ON users(login_name);

CREATE INDEX user_preferences_IX1
  ON user_preferences(user_id, pref_code);

CREATE INDEX user_roles_IX1
  ON user_roles(user_id, role_id);

CREATE INDEX role_permissions_IX1
  ON role_permissions(role_id, perm_code);





CREATE INDEX organizations_IX1
  ON organizations(parent_org_id);

CREATE INDEX organizations_IX2
  ON organizations(org_number);

CREATE INDEX organizations_IX3
  ON organizations(org_name);

CREATE INDEX organizations_IX4
  ON organizations(org_name_katakana_normalized);

CREATE INDEX organization_members_IX1
  ON organization_members(org_id, pt_id);






CREATE INDEX patients_IX1
  ON patients(current_org_id);

CREATE INDEX patients_IX2
  ON patients(pt_number);

CREATE INDEX patients_IX3
  ON patients(karte_number);

CREATE INDEX patients_IX4
  ON patients(pt_full_name);

CREATE INDEX patients_IX5
  ON patients(pt_full_name_katakana_normalized);

CREATE INDEX patients_IX6
  ON patients(pt_given_name);

CREATE INDEX patients_IX7
  ON patients(birth_date);




CREATE INDEX patient_duplicates_IX1
  ON patient_duplicates(current_pt_id);

CREATE INDEX patient_duplicates_IX2
  ON patient_duplicates(relative_pt_id);






CREATE INDEX customer_locations_IX1
  ON customer_locations(org_id, seq)
  WHERE org_id <> 0;

CREATE INDEX customer_locations_IX2
  ON customer_locations(pt_id, seq)
  WHERE pt_id <> 0;

CREATE INDEX customer_locations_IX3
  ON customer_locations(tel)
  WHERE tel <> '';

CREATE INDEX customer_locations_IX4
  ON customer_locations(tel2)
  WHERE tel2 <> '';

CREATE INDEX customer_marks_IX1
  ON customer_marks(org_id)
  WHERE org_id <> 0;

CREATE INDEX customer_marks_IX2
  ON customer_marks(pt_id)
  WHERE pt_id <> 0;

CREATE INDEX customer_notes_IX1
  ON customer_notes(org_id)
  WHERE org_id <> 0;

CREATE INDEX customer_notes_IX2
  ON customer_notes(pt_id)
  WHERE pt_id <> 0;

CREATE INDEX customer_files_IX1
  ON customer_files(org_id)
  WHERE org_id <> 0;

CREATE INDEX customer_files_IX2
  ON customer_files(pt_id)
  WHERE pt_id <> 0;





CREATE INDEX patient_insurance_cards_IX1
  ON patient_insurance_cards(pt_id);

CREATE INDEX patient_insurance_cards_IX2
  ON patient_insurance_cards(insur_prov_id);





CREATE INDEX organization_locks_IX1
  ON organization_locks(org_id);

CREATE INDEX patient_locks_IX1
  ON patient_locks(pt_id);

CREATE INDEX organization_logs_IX1
  ON organization_logs(org_id);

CREATE INDEX patient_logs_IX1
  ON patient_logs(pt_id);

CREATE INDEX user_logs_IX1
  ON user_logs(user_id);







CREATE INDEX reservation_floors_IX1
  ON reservation_floors(floor_code, seq);

CREATE INDEX reservation_rooms_IX1
  ON reservation_rooms(floor_id, seq);

CREATE INDEX reservation_room_details_IX1
  ON reservation_room_details(room_id);

CREATE INDEX reservation_room_details_IX2
  ON reservation_room_details(exam_id);






CREATE INDEX reservation_daily_slots_IX1
  ON reservation_daily_slots(start_date, end_date, floor_id);

CREATE INDEX reservation_daily_caps_IX1
  ON reservation_daily_caps(start_date, end_date, floor_id);

CREATE INDEX reservation_daily_slot_caps_IX1
  ON reservation_daily_slot_caps(start_date, end_date, floor_id);

CREATE INDEX reservation_daily_notes_IX1
  ON reservation_daily_notes(bkg_date, floor_id);





CREATE INDEX reservation_daily_bookings_IX1
  ON reservation_daily_bookings(bkg_date, floor_id);

CREATE INDEX reservation_daily_bookings_IX2
  ON reservation_daily_bookings(org_id)
  WHERE org_id <> 0;

CREATE INDEX reservation_daily_bookings_IX3
  ON reservation_daily_bookings(pt_id)
  WHERE pt_id <> 0;

CREATE INDEX reservation_daily_bookings_IX4
  ON reservation_daily_bookings(rec_id);

CREATE INDEX reservation_daily_bookings_IX_is_tentative
  ON reservation_daily_bookings(resv_daily_bkg_id)
  WHERE is_tentative = TRUE;








CREATE INDEX reservation_equipment_slots_IX1
  ON reservation_equipment_slots(start_date, end_date, equip_id);

CREATE INDEX reservation_equipment_bookings_IX1
  ON reservation_equipment_bookings(bkg_date, equip_id);

CREATE INDEX reservation_equipment_bookings_IX2
  ON reservation_equipment_bookings(org_id);

CREATE INDEX reservation_equipment_bookings_IX3
  ON reservation_equipment_bookings(pt_id);

CREATE INDEX reservation_equipment_bookings_IX4
  ON reservation_equipment_bookings(rec_id);

CREATE INDEX reservation_equipment_bookings_IX_is_tentative
  ON reservation_equipment_bookings(resv_equip_bkg_id)
  WHERE is_tentative = TRUE;



