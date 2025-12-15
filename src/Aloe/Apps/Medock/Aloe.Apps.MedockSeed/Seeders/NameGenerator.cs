namespace Aloe.Apps.MedockSeed.Seeders;

/// <summary>
/// 患者名・団体名のランダム生成を行うヘルパークラス
/// </summary>
internal static class NameGenerator
{
    // 一般的な日本の姓（漢字とカタカナ）
    private static readonly List<(string Kanji, string Kana)> FamilyNames =
    [
        // よくある姓
        ("佐藤", "サトウ"),
        ("鈴木", "スズキ"),
        ("高橋", "タカハシ"),
        ("田中", "タナカ"),
        ("山田", "ヤマダ"),
        ("伊藤", "イトウ"),
        ("中村", "ナカムラ"),
        ("小林", "コバヤシ"),
        ("加藤", "カトウ"),
        ("吉田", "ヨシダ"),
        ("松本", "マツモト"),
        ("井上", "イノウエ"),
        ("木村", "キムラ"),
        ("林", "ハヤシ"),
        ("斎藤", "サイトウ"),
        ("清水", "シミズ"),
        ("山口", "ヤマグチ"),
        ("森", "モリ"),
        ("池田", "イケダ"),
        ("橋本", "ハシモト"),
        ("前田", "マエダ"),
        ("藤原", "フジワラ"),
        ("岡田", "オカダ"),
        ("長谷川", "ハセガワ"),
        ("村上", "ムラカミ"),
        ("近藤", "コンドウ"),
        ("坂本", "サカモト"),
        ("遠藤", "エンドウ"),
        ("青木", "アオキ"),
        ("藤井", "フジイ"),
        ("石井", "イシイ"),
        ("後藤", "ゴトウ"),
        ("小川", "オガワ"),
        ("渡辺", "ワタナベ"),
        ("原田", "ハラダ"),
        ("中島", "ナカジマ"),
        ("太田", "オオタ"),
        ("平野", "ヒラノ"),
        ("福田", "フクダ"),
        ("西村", "ニシムラ"),
        ("藤田", "フジタ"),
        ("三浦", "ミウラ"),
        ("松田", "マツダ"),
        ("岡本", "オカモト"),
        ("中川", "ナカガワ"),
        ("竹内", "タケウチ"),
        ("金子", "カネコ"),
        ("和田", "ワダ"),
        ("中山", "ナカヤマ"),
        ("石田", "イシダ"),
        ("上田", "ウエダ"),
        ("森田", "モリタ"),
        ("小野", "オノ"),
        ("田村", "タムラ"),
        ("藤村", "フジムラ"),
        ("新井", "アライ"),
        ("千葉", "チバ"),
        ("久保", "クボ"),
        ("松井", "マツイ"),
        ("野口", "ノグチ"),
        ("上野", "ウエノ"),
        ("横山", "ヨコヤマ"),
        ("内田", "ウチダ"),
        ("酒井", "サカイ"),
        ("武田", "タケダ"),
        ("中野", "ナカノ"),
        ("宮崎", "ミヤザキ"),
        ("増田", "マスダ"),
        ("大島", "オオシマ"),
        ("宮本", "ミヤモト"),
        ("高木", "タカギ"),
        ("工藤", "クドウ"),
        ("谷口", "タニグチ"),
        ("村田", "ムラタ"),
        ("細川", "ホソカワ"),
        ("古川", "フルカワ"),
        ("荒木", "アラキ"),
        ("浅野", "アサノ"),
        ("北村", "キタムラ"),
        ("星野", "ホシノ"),
        ("今井", "イマイ"),
        ("関", "セキ"),
        ("中西", "ナカニシ"),
        ("丸山", "マルヤマ"),
        ("河野", "カワノ"),
        ("平田", "ヒラタ"),
        ("菅原", "スガワラ"),
        ("大野", "オオノ"),
        ("菊地", "キクチ"),
        ("須藤", "スドウ"),
        ("岩崎", "イワサキ"),
        ("大橋", "オオハシ"),
        ("高田", "タカタ"),
        ("野村", "ノムラ"),
        ("松尾", "マツオ"),
        ("小島", "コジマ"),
        ("五十嵐", "イガラシ"),
        ("吉川", "ヨシカワ"),
        ("多田", "タダ"),
        ("安藤", "アンドウ"),
        ("川上", "カワカミ"),
        ("松岡", "マツオカ"),
        ("大西", "オオニシ"),
        ("山本", "ヤマモト"),
    ];

    // 一般的な日本の名（漢字とカタカナ）
    private static readonly List<(string Kanji, string Kana)> GivenNames =
    [
        // 男性名
        ("太郎", "タロウ"),
        ("健太", "ケンタ"),
        ("翔太", "ショウタ"),
        ("大輔", "ダイスケ"),
        ("直樹", "ナオキ"),
        ("一郎", "イチロウ"),
        ("次郎", "ジロウ"),
        ("三郎", "サブロウ"),
        ("健一", "ケンイチ"),
        ("雄一", "ユウイチ"),
        ("誠", "マコト"),
        ("和彦", "カズヒコ"),
        ("正雄", "マサオ"),
        ("敏夫", "トシオ"),
        ("一郎", "イチロウ"),
        ("博", "ヒロシ"),
        ("清", "キヨシ"),
        ("義明", "ヨシアキ"),
        ("真一", "シンイチ"),
        ("拓也", "タクヤ"),
        ("悠太", "ユウタ"),
        ("隼人", "ハヤト"),
        ("蓮", "レン"),
        ("湊", "ミナト"),
        ("陽翔", "ハルト"),
        ("樹", "イツキ"),
        ("大和", "ヤマト"),
        ("陸", "リク"),
        ("颯太", "フウタ"),
        ("蒼", "ソウ"),
        // 女性名
        ("美咲", "ミサキ"),
        ("花子", "ハナコ"),
        ("由美", "ユミ"),
        ("麻衣", "マイ"),
        ("恵子", "ケイコ"),
        ("静香", "シズカ"),
        ("久美子", "クミコ"),
        ("真理", "マリ"),
        ("幸子", "サチコ"),
        ("節子", "セツコ"),
        ("陽菜", "ハルナ"),
        ("結衣", "ユイ"),
        ("美月", "ミツキ"),
        ("莉子", "リコ"),
        ("優奈", "ユウナ"),
        ("愛美", "アイミ"),
        ("さくら", "サクラ"),
        ("楓", "カエデ"),
        ("葵", "アオイ"),
        ("美羽", "ミウ"),
        ("心愛", "ココロ"),
        ("優花", "ユウカ"),
        ("咲良", "サクラ"),
        ("美咲", "ミサキ"),
        ("優衣", "ユイ"),
        ("愛", "アイ"),
        ("結菜", "ユイナ"),
        ("美優", "ミユ"),
        ("奏", "カナデ"),
    ];

    // 団体名の接頭辞
    private static readonly string[] CompanyPrefixes =
    [
        "株式会社",
        "有限会社",
        "合同会社",
        "合資会社",
        "合名会社",
    ];

    // 団体名の基本名称とカタカナのペア
    private static readonly List<(string Name, string Katakana)> CompanyBaseNames =
    [
        ("アロエ商事", "アロエショウジ"),
        ("アロエ工業", "アロエコウギョウ"),
        ("アロエ建設", "アロエケンセツ"),
        ("アロエサービス", "アロエサービス"),
        ("アロエテクノ", "アロエテクノ"),
        ("アロエ物流", "アロエブツリュウ"),
        ("アロエ電機", "アロエデンキ"),
        ("アロエ自動車", "アロエジドウシャ"),
        ("アロエ食品", "アロエショクヒン"),
        ("アロエ化学", "アロエカガク"),
        ("アロエ製薬", "アロエセイヤク"),
        ("アロエ機械", "アロエキカイ"),
        ("アロエ通信", "アロエツウシン"),
        ("アロエ運輸", "アロエウンユ"),
        ("アロエ不動産", "アロエフドウサン"),
        ("アロエ保険", "アロエホケン"),
        ("アロエ銀行", "アロエギンコウ"),
        ("アロエ証券", "アロエショウケン"),
        ("アロエ貿易", "アロエボウエキ"),
        ("アロエ出版", "アロエシュッパン"),
        ("アロエ印刷", "アロエインサツ"),
        ("アロエ広告", "アロエコウコク"),
        ("アロエコンサルティング", "アロエコンサルティング"),
        ("アロエシステム", "アロエシステム"),
        ("アロエソフトウェア", "アロエソフトウェア"),
        ("アロエエンジニアリング", "アロエエンジニアリング"),
        ("アロエプランニング", "アロエプランニング"),
        ("アロエデザイン", "アロエデザイン"),
        ("アロエマーケティング", "アロエマーケティング"),
        ("アロエセールス", "アロエセールス"),
    ];

    // 接頭辞のカタカナ
    private static readonly Dictionary<string, string> CompanyPrefixKatakana = new()
    {
        { "株式会社", "カブシキガイシャ" },
        { "有限会社", "ユウゲンガイシャ" },
        { "合同会社", "ゴウドウガイシャ" },
        { "合資会社", "ゴウシガイシャ" },
        { "合名会社", "ゴウメイガイシャ" },
    };

    // 接尾辞のカタカナ
    private static readonly Dictionary<string, string> CompanySuffixKatakana = new()
    {
        { "", "" },
        { "本社", "ホンシャ" },
        { "支店", "シテン" },
        { "東京支店", "トウキョウシテン" },
        { "大阪支店", "オオサカシテン" },
        { "名古屋支店", "ナゴヤシテン" },
    };

    // 団体名の接尾辞（必要に応じて）
    private static readonly string[] CompanySuffixes =
    [
        "",
        "本社",
        "支店",
        "東京支店",
        "大阪支店",
        "名古屋支店",
    ];

    // 患者メモの候補
    private static readonly string[] PatientMemoOptions =
    [
        "一般健診",
        "特定健診",
        "人間ドック",
        "定期健診",
    ];

    // 団体メモの候補
    private static readonly string[] OrganizationMemoOptions =
    [
        "健診契約企業",
        "定期健診契約",
        "企業健診",
        "健康診断契約",
    ];

    /// <summary>
    /// ランダムな患者名を生成します
    /// </summary>
    /// <param name="random">ランダムジェネレーター</param>
    /// <returns>(姓名（スペースあり）, 姓名（スペースなし）, カタカナ（スペースあり）, カタカナ（スペースなし）)</returns>
    public static (string Name, string NameCompat, string Katakana, string KatakanaCompat) GeneratePatientName(Random random)
    {
        var family = FamilyNames[random.Next(FamilyNames.Count)];
        var given = GivenNames[random.Next(GivenNames.Count)];

        var name = $"{family.Kanji} {given.Kanji}";
        var nameCompat = $"{family.Kanji}{given.Kanji}";
        var katakana = $"{family.Kana} {given.Kana}";
        var katakanaCompat = $"{family.Kana}{given.Kana}";

        return (name, nameCompat, katakana, katakanaCompat);
    }

    /// <summary>
    /// ランダムな団体名を生成します
    /// </summary>
    /// <param name="random">ランダムジェネレーター</param>
    /// <returns>(団体名, カタカナ, カタカナ互換, 表示名, 印刷名)</returns>
    public static (string Name, string NameKatakana, string NameKatakanaCompat, string NameDisplay, string NamePrint) GenerateOrganizationName(Random random)
    {
        var prefix = CompanyPrefixes[random.Next(CompanyPrefixes.Length)];
        var baseNameData = CompanyBaseNames[random.Next(CompanyBaseNames.Count)];
        var suffix = CompanySuffixes[random.Next(CompanySuffixes.Length)];

        var name = suffix == "" ? $"{prefix}{baseNameData.Name}" : $"{prefix}{baseNameData.Name}{suffix}";
        var nameDisplay = name;
        var namePrint = name;

        // カタカナを組み立て
        var prefixKatakana = CompanyPrefixKatakana[prefix];
        var suffixKatakana = CompanySuffixKatakana[suffix];
        var nameKatakana = suffixKatakana == "" 
            ? $"{prefixKatakana}{baseNameData.Katakana}" 
            : $"{prefixKatakana}{baseNameData.Katakana}{suffixKatakana}";
        var nameKatakanaCompat = nameKatakana.Replace(" ", "").Replace("　", "");

        return (name, nameKatakana, nameKatakanaCompat, nameDisplay, namePrint);
    }

    /// <summary>
    /// 患者メモをランダムに取得します
    /// </summary>
    public static string GetRandomPatientMemo(Random random)
    {
        return PatientMemoOptions[random.Next(PatientMemoOptions.Length)];
    }

    /// <summary>
    /// 団体メモをランダムに取得します
    /// </summary>
    public static string GetRandomOrganizationMemo(Random random)
    {
        return OrganizationMemoOptions[random.Next(OrganizationMemoOptions.Length)];
    }

}

