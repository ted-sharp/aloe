# JisCompatMap — JIS互換マップインポートハンドラ

JIS互換文字マッピング表（XLSX）を PostgreSQL へ一括インポートするライブラリ。
IBM拡張漢字の13エントリも固定値として追加する。

`IImportHandler` として `HandlerKey = "jis-compat-map"` で登録されており、`csv-importer` CLI から呼び出す。

## 元データ（JIS互換マップ）

| 種別 | 形式 | 備考 |
|------|------|------|
| 全件 | XLSX | 1シート目の A3 セルを起点として読み込み |

- 形式: Excel（XLSX）
- 読み取りシート: 1シート目
- 開始セル: A3（3行目、A列から）
- 列数: 17列
- 入力: `--source` でローカル XLSX ファイルパスを指定（必須）
- HTTP 自動取得: 非対応（Full モードのみ）

## Excelカラム仕様（A列以降、17列）

| # | 列 | カラム名（raw テーブル） | 説明 |
|---|-----|------------------------|------|
| 1 | A | `source_menkuten_code` | 変換元 面区点コード |
| 2 | B | `source_unicode` | 変換元 Unicode |
| 3 | C | `source_text` | 変換元 文字 |
| 4 | D | `source_jis_kubun` | 変換元 JIS区分 |
| 5 | E | `mapped_menkuten_code` | 変換先 面区点コード |
| 6 | F | `mapped_unicode` | 変換先 Unicode |
| 7 | G | `mapped_text` | 変換先 文字 |
| 8 | H | `multi_menkuten_code_1` | 複数変換先 面区点コード1 |
| 9 | I | `multi_menkuten_code_2` | 複数変換先 面区点コード2 |
| 10 | J | `multi_menkuten_code_3` | 複数変換先 面区点コード3 |
| 11 | K | `multi_menkuten_code_4` | 複数変換先 面区点コード4 |
| 12 | L | `multi_unicode_1` | 複数変換先 Unicode1 |
| 13 | M | `multi_unicode_2` | 複数変換先 Unicode2 |
| 14 | N | `multi_unicode_3` | 複数変換先 Unicode3 |
| 15 | O | `multi_unicode_4` | 複数変換先 Unicode4 |
| 16 | P | `multi_text` | 複数変換先 文字 |
| 17 | Q | `remarks` | 備考 |

A列（`source_menkuten_code`）が空の行はスキップされる。

## データベーススキーマ

### `ext.raw_jis_compat_maps`（ステージングテーブル）

上記17カラムをすべて `TEXT` として保持する。

### `ext.jis_compat_maps`（本番テーブル）

| カラム | 型 | 説明 |
|--------|----|------|
| `source_text` | `TEXT` | 変換元文字 |
| `mapped_text` | `TEXT` | 変換先文字 |

- フィルター条件: `mapped_text <> source_text AND mapped_text <> ''`（変換前後が同一または空のものは除外）
- ユニーク制約: `(source_text, mapped_text)`（IBM拡張漢字の `ON CONFLICT DO NOTHING` に対応）

### IBM拡張漢字（固定追加エントリ）

XLSX に含まれない IBM 拡張漢字の変換マッピング13件を `ON CONFLICT DO NOTHING` で追加する。

| 変換元 | 変換先 |
|--------|--------|
| 髙 | 高 |
| 閒 | 聞 |
| 晴 | 晴 |
| 益 | 益 |
| 礼 | 礼 |
| 靖 | 靖 |
| 精 | 精 |
| 羽 | 羽 |
| 逸 | 逸 |
| 飯 | 飯 |
| 飼 | 飼 |
| 館 | 館 |
| 鶴 | 鶴 |

## インポートフロー

```
① --source で XLSX ファイルを指定
② ext.raw_jis_compat_maps を TRUNCATE
③ ClosedXML で1シート目の A3 セルから17列読み込み
④ COPY BINARY → ext.raw_jis_compat_maps（17列）
⑤ TRUNCATE + フィルター付き INSERT SELECT → ext.jis_compat_maps（本番）
⑥ ステージング TRUNCATE
⑦ IBM拡張漢字13エントリを INSERT（ON CONFLICT DO NOTHING）
```

| モード | 動作 |
|--------|------|
| Full のみ | `TRUNCATE` + フィルター付き `INSERT SELECT` + IBM拡張漢字 `INSERT` |

Delta モードは非対応（呼び出すと `NotSupportedException`）。

## 使い方

```bash
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" \
  jis-compat-map --source /path/to/jis_compat_map.xlsx
```

## 関連ファイル

| ファイル | 説明 |
|----------|------|
| `JisCompatMapImportHandler.cs` | ハンドラー本体（IBM拡張漢字の固定INSERTを含む） |
| `Extensions/ServiceCollectionExtensions.cs` | DI 登録（`AddJisCompatMapImport()`） |
