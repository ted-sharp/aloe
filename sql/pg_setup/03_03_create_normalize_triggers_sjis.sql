
-- 正規化拡張機能の作成
CREATE EXTENSION IF NOT EXISTS unaccent;

-- トリガー関数の作成
CREATE OR REPLACE FUNCTION fn_update_normalized_katakana()
RETURNS TRIGGER AS $$
BEGIN
  -- INSERT の場合は、必ず正規化を実行
  IF (TG_OP = 'INSERT') THEN
    NEW.pt_name_katakana_compat := fn_normalize_katakana(NEW.pt_name_katakana);

  -- UPDATE の場合は、変更された場合のみ正規化を実行
  ELSIF (TG_OP = 'UPDATE' AND NEW.pt_name_katakana IS DISTINCT FROM OLD.pt_name_katakana) THEN
    NEW.pt_name_katakana_compat := fn_normalize_katakana(NEW.pt_name_katakana);
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 正規化処理用の関数
CREATE OR REPLACE FUNCTION fn_normalize_katakana(input TEXT)
RETURNS TEXT AS $$
DECLARE
  normalized_text TEXT;
BEGIN
  normalized_text := input;

  -- ひらがなをカタカナに変換
  normalized_text := translate(normalized_text,
                               'ぁあぃいぅうぇえぉおかがきぎくぐけげこごさがしじすずせぜそぞただちぢつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもやゆよらりるれろわをん',
                               'ァアィイゥウェエォオカガキギクグケゲコゴサガシジスズセゼソゾタダチヂツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモヤユヨラリルレロワヲン');

  -- 半角カタカナを全角に変換
  normalized_text := translate(normalized_text, 'ｦｧｨｩｪｫｬｭｮｯｰｱｳｴｵｶｷｸｺｻｼｽｾﾀﾂﾃﾅﾆﾇﾈﾊﾋﾌﾍﾏﾐﾑﾒﾓﾔﾕﾗﾘﾙﾚﾜﾝﾟﾞ', 'ヲァィゥェォャュョッーアウエオカキクコサシスセタツテナニヌネハヒフヘマミムメモヤユラリルレワンプボ');

  -- 半角スペースを全角スペースに置換
  normalized_text := replace(normalized_text, ' ', '　');

  -- アクセント記号の削除
  normalized_text := unaccent(normalized_text);

  RETURN normalized_text;
END;
$$ LANGUAGE plpgsql;

-- トリガーの作成
DROP TRIGGER IF EXISTS patients_TRG_before_normalize ON patients;
CREATE TRIGGER patients_TRG_before_normalize
BEFORE INSERT OR UPDATE ON patients
FOR EACH ROW
EXECUTE FUNCTION fn_update_normalized_katakana();
