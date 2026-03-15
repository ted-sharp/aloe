# HealthFacility — 特定健診実施機関インポートハンドラ

厚生労働省が公開している特定健診・特定保健指導実施機関一覧（SJIS CSV）を PostgreSQL へ一括インポートするライブラリ。

`IImportHandler` として `HandlerKey = "health-facility"` で登録されており、`csv-importer` CLI から呼び出す。

## 元データ（特定健診実施機関一覧）

| 種別 | 形式 | 備考 |
|------|------|------|
| 全件 | ZIP 内 CSV または CSV 直接 | ヘッダー行あり（1行スキップ） |

- 文字コード: CP932（Shift-JIS）
- 形式: CSV、ヘッダー行あり（1行目スキップ）
- 入力: `--source` でローカル ZIP または CSV ファイルパスを指定（必須）
  - `.zip` 拡張子 → ZIP 内の CSV を展開して読み込み
  - それ以外 → CSV ファイルとして直接読み込み
- HTTP 自動取得: 非対応（Full モードのみ）
- 更新頻度: 定期（厚生労働省の公表スケジュールによる）

## CSVカラム仕様（元データの列順）

ヘッダー行の1行目はスキップされる。カラム仕様は提供元の公表データに準拠。
詳細なカラム定義は `ext.raw_special_health_facility_codes` テーブル定義を参照。

## データベーススキーマ

### `ext.raw_special_health_facility_codes`（ステージングテーブル）

COPY FROM STDIN の受け口。型変換を避けるため全列 `TEXT`。

### `ext.facility_codes`（本番テーブル）

ステージングと同じカラム構成。`TRUNCATE` + `INSERT SELECT *` でマージ。

## インポートフロー

```
① --source で ZIP または CSV ファイルを指定
② ファイル種別判定（.zip / それ以外）
   ─ ZIP の場合: 展開 → CSV ストリーム（先頭1行スキップ）
   ─ CSV の場合: 直接ストリーム（先頭1行スキップ）
③ COPY FROM STDIN (CP932) → ext.raw_special_health_facility_codes
④ TRUNCATE + INSERT SELECT → ext.facility_codes（本番）
⑤ ステージング TRUNCATE
```

| モード | 動作 |
|--------|------|
| Full のみ | `TRUNCATE` + `INSERT SELECT *` |

Delta モードは非対応（呼び出すと `NotSupportedException`）。

## 使い方

```bash
# ZIP ファイルから取り込み
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" \
  health-facility --source /path/to/facility_list.zip

# CSV ファイルから直接取り込み
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" \
  health-facility --source /path/to/facility_list.csv
```

## 関連ファイル

| ファイル | 説明 |
|----------|------|
| `HealthFacilityImportHandler.cs` | ハンドラー本体 |
| `ZipEntryStream.cs` | ZIP エントリストリームラッパー |
| `Extensions/ServiceCollectionExtensions.cs` | DI 登録（`AddHealthFacilityImport()`） |
