# JfagyCodes — J-FAGY アレルゲンコードインポートハンドラ

FHIR CodeSystem リソース形式の JSON ファイルを PostgreSQL へ一括インポートするライブラリ。

`IImportHandler` として `HandlerKey = "jfagy-codes"` で登録されており、`csv-importer` CLI から呼び出す。

## 元データ（FHIR CodeSystem JSON）

| 種別 | 形式 | 備考 |
|------|------|------|
| CodeSystem リソース | JSON | 複数ファイル指定可（`--source` を繰り返す） |

- 形式: FHIR R4 / R5 準拠の CodeSystem リソース JSON
- 入力: `--source <json>` を1つ以上指定（`Arity.OneOrMore`）
- HTTP 自動取得: 非対応（Full モードのみ）
- 更新頻度: 随時（提供元による）

### JSONフォーマット

```json
{
  "url": "http://example.com/fhir/CodeSystem/xxx",
  "concept": [
    {
      "code": "001",
      "display": "コード説明",
      "property": [
        { "code": "parent", "valueCode": "PARENT_CODE" }
      ],
      "designation": [
        { "value": "別名" }
      ],
      "concept": [
        { "code": "001-1", "display": "子コード説明" }
      ]
    }
  ]
}
```

`concept` は再帰的にネストされる。各レベルで `property[code=="parent"].valueCode` から親コードを、`designation[0].value` から別名を抽出する。

| フィールド | 対応カラム | 説明 |
|-----------|-----------|------|
| `url` | `coding_system` | CodeSystem の URL |
| `concept[].code` | `code` | コード値 |
| `concept[].display` | `display` | 表示名 |
| `concept[].property[code=="parent"].valueCode` | `parent_code` | 親コード |
| `concept[].designation[0].value` | `designation` | 別名 |

`concept` 配列が存在しないファイルはスキップされる。`code` が空のエントリもスキップ。

## データベーススキーマ

### `ext.raw_jfagy_allergen_codes`（ステージングテーブル）

| カラム | 型 | NULL | 説明 |
|--------|----|------|------|
| `coding_system` | `TEXT` | YES | CodeSystem URL（JSON の `url` フィールド） |
| `code` | `TEXT` | NO | コード値 |
| `display` | `TEXT` | YES | 表示名 |
| `parent_code` | `TEXT` | YES | 親コード |
| `designation` | `TEXT` | YES | 別名 |

本番テーブルへの明示的なマージ処理はなく、ステージングテーブルがそのまま参照用テーブルとして機能する。
インポート実行時に `TRUNCATE` してから全件挿入する（洗い替え）。

## インポートフロー

```
① --source で JSON ファイルを1つ以上指定
② ext.raw_jfagy_allergen_codes を TRUNCATE
③ 各 JSON ファイルをパース → concept 配列を再帰的に走査
④ COPY BINARY → ext.raw_jfagy_allergen_codes (coding_system, code, display, parent_code, designation)
```

| モード | 動作 |
|--------|------|
| Full のみ | `TRUNCATE` + 全件 `COPY` |

Delta モードは非対応（呼び出すと `NotSupportedException`）。

## 使い方

```bash
# 単一ファイル
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" \
  jfagy-codes --source /path/to/codesystem1.json

# 複数ファイル（--source を繰り返す）
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" \
  jfagy-codes --source /path/to/codesystem1.json --source /path/to/codesystem2.json
```

## 関連ファイル

| ファイル | 説明 |
|----------|------|
| `JfagyCodesImportHandler.cs` | ハンドラー本体 |
| `Extensions/ServiceCollectionExtensions.cs` | DI 登録（`AddJfagyCodesImport()`） |
