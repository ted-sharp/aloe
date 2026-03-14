# PostalCode — 郵便番号インポートハンドラ

日本郵便が公開している郵便番号データ（UTF-8 CSV）を PostgreSQL へ一括インポートするライブラリ。

`IImportHandler` として `HandlerKey = "postal-code"` で登録されており、`csv-importer` CLI から呼び出す。

## 元データ（日本郵便）

| 種別 | ファイル名 | URL |
|------|-----------|-----|
| 全件 | `utf_ken_all.zip` | https://www.post.japanpost.jp/zipcode/dl/utf/zip/utf_ken_all.zip |
| 差分（追加）| `utf_add_{yymm}.zip` | 同ページ（月次差分） |
| 差分（削除）| `utf_del_{yymm}.zip` | 同ページ（月次差分） |

- 文字コード: UTF-8
- 形式: CSV、ヘッダー行なし
- 列数: 15列
- 更新頻度: 月次
- レコード数: 約 124,000 件（全件）
- ファイルサイズ: ZIP 約 2 MB、展開後 CSV 約 14 MB
- 差分ファイル: 月次で数十〜数百件規模（変更量は月により異なる）

## CSVカラム仕様（元データの列順）

| # | 列名 | 説明 |
|---|------|------|
| 1 | `jis_code` | 全国地方公共団体コード（5桁） |
| 2 | `old_postal_code` | 旧郵便番号（3または5桁） |
| 3 | `postal_code` | 郵便番号（7桁、ハイフンなし） |
| 4 | `prefecture_kana` | 都道府県名カナ |
| 5 | `city_kana` | 市区町村名カナ |
| 6 | `town_kana` | 町域名カナ |
| 7 | `prefecture` | 都道府県名 |
| 8 | `city` | 市区町村名 |
| 9 | `town` | 町域名 |
| 10 | `multi_zip` | 一町域が二以上の郵便番号に該当（`0`/`1`） |
| 11 | `koaza_split` | 小字毎に番地が起番されている町域（`0`/`1`） |
| 12 | `has_chome` | 丁目を有する町域（`0`/`1`） |
| 13 | `multi_town` | 一郵便番号で二以上の町域を表す（`0`/`1`） |
| 14 | `update_reason` | 更新の表示（`0`:変更なし、`1`:変更あり、`2`:廃止） |
| 15 | `change_reason` | 変更理由（`0`〜`6`） |

## データベーススキーマ

### `ext.postal_codes`（本番テーブル）

| カラム | 型 | 制約 | 説明 |
|--------|----|------|------|
| `id` | `BIGSERIAL` | PRIMARY KEY | 自動採番ID |
| `jis_code` | `TEXT` | NOT NULL | 全国地方公共団体コード（5桁） |
| `old_postal_code` | `TEXT` | | 旧郵便番号 |
| `postal_code` | `TEXT` | NOT NULL | 郵便番号（7桁、ハイフンなし） |
| `prefecture_kana` | `TEXT` | | 都道府県名カナ |
| `city_kana` | `TEXT` | | 市区町村名カナ |
| `town_kana` | `TEXT` | | 町域名カナ |
| `prefecture` | `TEXT` | | 都道府県名 |
| `city` | `TEXT` | | 市区町村名 |
| `town` | `TEXT` | | 町域名 |
| `multi_zip` | `BOOLEAN` | NOT NULL DEFAULT FALSE | 一町域が二以上の郵便番号に該当 |
| `koaza_split` | `BOOLEAN` | NOT NULL DEFAULT FALSE | 小字毎に番地が起番されている町域 |
| `has_chome` | `BOOLEAN` | NOT NULL DEFAULT FALSE | 丁目を有する町域 |
| `multi_town` | `BOOLEAN` | NOT NULL DEFAULT FALSE | 一郵便番号で二以上の町域を表す |
| `update_reason` | `SMALLINT` | NOT NULL DEFAULT 0 | 更新の表示 |
| `change_reason` | `SMALLINT` | NOT NULL DEFAULT 0 | 変更理由 |

- ユニーク制約: `(jis_code, postal_code, town_kana)`
- インデックス: `postal_code`、`jis_code`

### `ext.postal_codes_staged`（ステージングテーブル）

COPY FROM STDIN の受け口。型変換を避けるため全列 `TEXT`。カラム構成は本番テーブルの `id` を除く15列と同じ。

## インポートフロー

```
① HTTP（日本郵便）→ ZIP ダウンロード
② ZIP 展開 → CSV ストリーム
③ COPY FROM STDIN → ext.postal_codes_staged
④ SQL マージ → ext.postal_codes（本番）
⑤ ステージング TRUNCATE
```

`MergeToProduction.sql` はセクションマーカーで分割されており、モードに応じて該当セクションのみ実行される。

| セクション | モード | 動作 |
|-----------|--------|------|
| `[FULL]` | 全件 | `TRUNCATE` + `INSERT SELECT` |
| `[DELTA_ADD]` | 差分（追加）| `MERGE`（upsert） |
| `[DELTA_DEL]` | 差分（削除）| `DELETE` |

## 使い方

```bash
# 全件インポート
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" postal-code --full

# 差分インポート（例: 2025年1月）
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" postal-code --yymm 2501
```

## 関連ファイル

| ファイル | 説明 |
|----------|------|
| `PostalCodeImportHandler.cs` | ハンドラー本体 |
| `Sql/CreateTables.sql` | テーブル定義（`ext.postal_codes`、`ext.postal_codes_staged`） |
| `Sql/MergeToProduction.sql` | マージSQL（`[FULL]`/`[DELTA_ADD]`/`[DELTA_DEL]` セクション） |
