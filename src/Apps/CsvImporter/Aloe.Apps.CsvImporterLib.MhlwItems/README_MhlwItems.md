# MhlwItems — 厚労省XML特定健診項目インポートハンドラ

厚生労働省が定める XML 形式の特定健診項目一覧（XLSX）を PostgreSQL へ一括インポートするライブラリ。

`IImportHandler` として `HandlerKey = "mhlw-items"` で登録されており、`csv-importer` CLI から呼び出す。

## 元データ（厚生労働省 XML特定健診項目一覧）

| 種別 | 形式 | 備考 |
|------|------|------|
| 全件 | XLSX | 1シート目の C3 セルを起点として読み込み |

- 形式: Excel（XLSX）
- 読み取りシート: 1シート目
- 開始セル: C3（3行目、C列から）
- 入力: `--source` でローカル XLSX ファイルパスを指定（必須）
- HTTP 自動取得: 非対応（Full モードのみ）
- 更新頻度: 随時（厚生労働省の改定による）

## Excelカラム仕様（C列以降）

| オフセット | 列 | カラム名 | 説明 |
|-----------|-----|---------|------|
| 0 | C | `code` | 項目コード |
| 1 | D | `name` | 項目名称 |
| 2 | E | `label` | ラベル |

`code` が空の行はスキップされる。

## データベーススキーマ

### `ext.raw_mhlw_xml_tokutei_kenshin_items`（ステージングテーブル）

| カラム | 型 | 説明 |
|--------|----|------|
| `code` | `TEXT` | 項目コード |
| `name` | `TEXT` | 項目名称 |
| `label` | `TEXT` | ラベル |

本番テーブルへの明示的なマージ処理はなく、ステージングテーブルがそのまま参照用テーブルとして機能する。
インポート実行時に `TRUNCATE` してから全件挿入する（洗い替え）。

## インポートフロー

```
① --source で XLSX ファイルを指定
② ext.raw_mhlw_xml_tokutei_kenshin_items を TRUNCATE
③ ClosedXML で1シート目の C3 セルから読み込み
④ COPY BINARY → ext.raw_mhlw_xml_tokutei_kenshin_items (code, name, label)
```

| モード | 動作 |
|--------|------|
| Full のみ | `TRUNCATE` + 全件 `COPY` |

Delta モードは非対応（呼び出すと `NotSupportedException`）。

## 使い方

```bash
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" \
  mhlw-items --source /path/to/mhlw_items.xlsx
```

## 関連ファイル

| ファイル | 説明 |
|----------|------|
| `MhlwItemsImportHandler.cs` | ハンドラー本体 |
| `Extensions/ServiceCollectionExtensions.cs` | DI 登録（`AddMhlwItemsImport()`） |
