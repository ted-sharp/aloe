
CREATE OR REPLACE FUNCTION fn_update_cache_updates()
RETURNS TRIGGER AS $$
BEGIN
  INSERT INTO cache_updates (table_name, latest_updated_at, latest_updated_user_id, latest_updated_session_id)
  VALUES (TG_TABLE_NAME, NOW(), NEW.updated_user_id, NEW.updated_session_id)
  ON CONFLICT (table_name)
  DO UPDATE SET
    latest_updated_at = NEW.updated_at,
    latest_updated_user_id = NEW.updated_user_id,
    latest_updated_session_id = NEW.updated_session_id;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;



DROP TRIGGER IF EXISTS reservation_equipments_TRG_after_cache ON reservation_equipments;
CREATE TRIGGER reservation_equipments_TRG
AFTER INSERT OR UPDATE ON reservation_equipments
FOR EACH ROW EXECUTE FUNCTION fn_update_cache_updates();



DROP TRIGGER IF EXISTS reservation_equipment_slots_TRG_after_cache ON reservation_equipment_slots;
CREATE TRIGGER reservation_equipment_slots_TRG
AFTER INSERT OR UPDATE ON reservation_equipment_slots
FOR EACH ROW EXECUTE FUNCTION fn_update_cache_updates();



DROP TRIGGER IF EXISTS reservation_floors_TRG_after_cache ON reservation_floors;
CREATE TRIGGER reservation_floors_TRG
AFTER INSERT OR UPDATE ON reservation_floors
FOR EACH ROW EXECUTE FUNCTION fn_update_cache_updates();



DROP TRIGGER IF EXISTS reservation_rooms_TRG_after_cache ON reservation_rooms;
CREATE TRIGGER reservation_rooms_TRG
AFTER INSERT OR UPDATE ON reservation_rooms
FOR EACH ROW EXECUTE FUNCTION fn_update_cache_updates();



DROP TRIGGER IF EXISTS reservation_daily_slots_TRG_after_cache ON reservation_daily_slots;
CREATE TRIGGER reservation_daily_slots_TRG
AFTER INSERT OR UPDATE ON reservation_daily_slots
FOR EACH ROW EXECUTE FUNCTION fn_update_cache_updates();



