
CREATE OR REPLACE FUNCTION fn_log_changes()
RETURNS TRIGGER AS $$
DECLARE
  pk_column_name TEXT := TG_ARGV[0];  -- トリガーの引数から主キーのカラム名を取得
  pk_value TEXT;
BEGIN
  -- 主キーの値を取得する（NEW または OLD のどちらかを使用）
  EXECUTE format('SELECT ($1).%I', pk_column_name)
  INTO pk_value
  USING COALESCE(NEW, OLD);

  -- change_logs テーブルへ挿入
  INSERT INTO change_logs (
    table_name,
    record_id,
    changed_action,
    changed_user_id,
    changed_user_name,
    changed_at,
    changed_session_id,
    sql_statement,
    old_values,
    new_values
  ) VALUES (
    TG_TABLE_NAME,
    pk_value,  -- 主キーの値
    TG_OP,  -- 'INSERT' または 'UPDATE'
    NEW.changed_user_id,
    NEW.changed_user_name,
    NOW(),
    NEW.changed_session_id,
    current_query(),
    to_jsonb(OLD),
    to_jsonb(NEW)
  );

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;





-- すべてのテーブルにトリガを設定します。(sessions, change_logs は除く)
-- 必要であれば個別に設定してください。
DO $$
DECLARE
  rec RECORD;
  trigger_name TEXT;
  pk_column_name TEXT;
BEGIN
  -- 必要なカラムを全て持つテーブルを取得
  FOR rec IN
    SELECT table_name
    FROM information_schema.columns
    WHERE column_name IN ('changed_user_id', 'changed_user_name', 'changed_at', 'changed_session_id')
    GROUP BY table_name
    HAVING COUNT(column_name) = 4
  LOOP
    -- テーブルの主キー列を取得
    SELECT kc.column_name INTO pk_column_name
    FROM information_schema.table_constraints tc
    JOIN information_schema.key_column_usage kc
      ON tc.constraint_name = kc.constraint_name
      AND tc.table_schema = kc.table_schema
      AND tc.table_name = rec.table_name
    WHERE tc.constraint_type = 'PRIMARY KEY'
      AND tc.table_schema = 'public'
    LIMIT 1;

    -- 主キーが見つからない場合はスキップ
    IF pk_column_name IS NULL THEN
      CONTINUE;
    END IF;

    -- トリガー名を生成
    trigger_name := rec.table_name || '_TRG_after_log';

    -- 既存のトリガーがある場合は削除
    EXECUTE format($sql$
      DROP TRIGGER IF EXISTS %I ON %I
    $sql$, trigger_name, rec.table_name);

    -- トリガーを作成
    EXECUTE format($sql$
      CREATE TRIGGER %I
      AFTER INSERT OR UPDATE ON %I
      FOR EACH ROW
      EXECUTE FUNCTION fn_log_changes(%L)
    $sql$, trigger_name, rec.table_name, pk_column_name);
  END LOOP;
EXCEPTION
  WHEN OTHERS THEN
    RAISE NOTICE 'Error occurred: %', SQLERRM;
END $$;





