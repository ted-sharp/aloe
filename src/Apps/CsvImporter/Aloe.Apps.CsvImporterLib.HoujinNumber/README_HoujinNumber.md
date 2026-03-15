# HoujinNumber — 法人番号インポートハンドラ

国税庁が公開している法人番号データ（UTF-8 CSV）を PostgreSQL へ一括インポートするライブラリ。

`IImportHandler` として `HandlerKey = "houjin-number"` で登録されており、`csv-importer` CLI から呼び出す。

## 元データ（国税庁 法人番号公表サイト）

| 種別 | 形式 | 備考 |
|------|------|------|
| 全件 | ZIP 内 CSV | 手動ダウンロードが必要（HTTP 取得非対応） |

- 文字コード: UTF-8
- 形式: CSV、ヘッダー行なし
- 更新頻度: 随時
- 入力: `--source` でローカル ZIP ファイルパスを指定（必須）
- HTTP 自動取得: 非対応（Full モードのみ）

## CSVカラム仕様（元データの列順）

国税庁の法人番号データ（全件ファイル）に含まれる代表的なカラム。

| # | 列名（raw テーブル） | 説明 |
|---|------|------|
| 1 | `corporate_number` | 法人番号（13桁） |
| 2 | `process` | 処理区分 |
| 3 | `correct` | 訂正区分 |
| 4 | `update_date` | 更新年月日 |
| 5 | `change_date` | 変更年月日 |
| 6 | `name` | 法人名 |
| 7 | `name_image_id` | 法人名イメージID |
| 8 | `kind` | 法人種別 |
| 9 | `prefecture_name` | 国内所在地（都道府県） |
| 10 | `city_name` | 国内所在地（市区町村） |
| 11 | `street_number` | 国内所在地（番地） |
| 12 | `address_image_id` | 国内所在地イメージID |
| 13 | `prefecture_code` | 都道府県コード |
| 14 | `city_code` | 市区町村コード |
| 15 | `post_code` | 郵便番号 |
| 16 | `address_outside` | 国外所在地 |
| 17 | `address_outside_image_id` | 国外所在地イメージID |
| 18 | `close_date` | 登記記録の閉鎖等年月日 |
| 19 | `close_cause` | 登記記録の閉鎖等の事由 |
| 20 | `successor_corporate_number` | 承継先法人番号 |
| 21 | `change_cause` | 変更事由の詳細 |
| 22 | `assignment_date` | 法人番号指定年月日 |
| 23 | `latest` | 最新履歴 |
| 24 | `en_name` | 法人名（英語） |
| 25 | `en_prefecture_name` | 国内所在地（都道府県・英語） |
| 26 | `en_city_name` | 国内所在地（市区町村・英語） |
| 27 | `en_address_outside` | 国外所在地（英語） |
| 28 | `furigana` | 法人名ふりがな |
| 29 | `hihyoji` | 非表示フラグ（`'0'`=表示、`'1'`=非表示） |

## データベーススキーマ

### `ext.raw_houjin_numbers`（ステージングテーブル）

COPY FROM STDIN の受け口。型変換を避けるため全列 `TEXT`。カラム構成は元 CSV の列と同じ。

### `ext.houjin_numbers`（本番テーブル）

| カラム | 型 | 説明 |
|--------|----|------|
| `id` | `BIGSERIAL` | 自動採番ID |
| `corporate_number` | `TEXT` | 法人番号（13桁） |
| `name` | `TEXT` | 法人名 |
| `postal_code` | `TEXT` | 郵便番号 |
| `prefecture_name` | `TEXT` | 都道府県名 |
| `city_name` | `TEXT` | 市区町村名 |
| `street_number` | `TEXT` | 番地 |

- インデックス: `houjin_numbers_ix1 ON (corporate_number)`
- フィルター条件（ステージング → 本番）:
  - `prefecture_name IS NOT NULL`
  - `post_code IS NOT NULL`
  - `close_date IS NULL`（閉鎖法人を除外）
  - `hihyoji = '0'`（非表示フラグが立っていないもののみ）

## インポートフロー

```
① --source で ZIP ファイルを指定
② ZIP 展開 → CSV ストリーム
③ COPY FROM STDIN → ext.raw_houjin_numbers
④ SQL（TRUNCATE + フィルター付き INSERT）→ ext.houjin_numbers（本番）
⑤ インデックス再作成（houjin_numbers_ix1）
⑥ ステージング TRUNCATE
```

| セクション | モード | 動作 |
|-----------|--------|------|
| Full のみ | 全件 | `TRUNCATE` + フィルター付き `INSERT SELECT` |

Delta モードは非対応（呼び出すと `NotSupportedException`）。

## 使い方

```bash
# 全件インポート（ローカルZIPを指定）
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" \
  houjin-number --source /path/to/houjin_all.zip
```

## 関連ファイル

| ファイル | 説明 |
|----------|------|
| `HoujinNumberImportHandler.cs` | ハンドラー本体 |
| `Extensions/ServiceCollectionExtensions.cs` | DI 登録（`AddHoujinNumberImport()`） |
