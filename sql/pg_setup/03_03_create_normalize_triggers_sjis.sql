
-- トリガー関数の作成
CREATE OR REPLACE FUNCTION fn_update_normalized_katakana()
RETURNS TRIGGER AS $$
BEGIN
  -- INSERT の場合は、無条件で正規化を行う
  IF (TG_OP = 'INSERT') THEN
    NEW.pt_full_name_katakana_normalized := fn_normalize_katakana(NEW.pt_full_name_katakana);
  
  -- UPDATE の場合は、変更があった場合のみ正規化を行う
  ELSIF (TG_OP = 'UPDATE' AND NEW.pt_full_name_katakana IS DISTINCT FROM OLD.pt_full_name_katakana) THEN
    NEW.pt_full_name_katakana_normalized := fn_normalize_katakana(NEW.pt_full_name_katakana);
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 正規化処理用の補助関数
CREATE OR REPLACE FUNCTION fn_normalize_katakana(input TEXT)
RETURNS TEXT AS $$
DECLARE
  normalized_text TEXT;
BEGIN
  normalized_text := input;

  -- ひらがなをカタカナに変換
  normalized_text := translate(normalized_text, 
                               'ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんヴ??',
                               'ァアイィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲンヴヵヶ');
                               
  -- 半角カタカナを全角に変換
  normalized_text := translate(normalized_text, 'ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜｦﾝ', 'アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン');
  
  -- 半角スペースを全角スペースに統一
  normalized_text := replace(normalized_text, ' ', '　');

  -- アクセント記号の除去
  normalized_text := unaccent(normalized_text);

  RETURN normalized_text;
END;
$$ LANGUAGE plpgsql;

-- トリガーの作成
DROP TRIGGER IF EXISTS patents_TRG_before_normalize ON patients;
CREATE TRIGGER patents_TRG_before_normalize
BEFORE INSERT OR UPDATE ON patients
FOR EACH ROW
EXECUTE FUNCTION fn_update_normalized_katakana();
