# MedisCodes — MEDIS コードインポートハンドラ群

MEDIS-DC が提供する医療系マスターデータを PostgreSQL へ一括インポートするライブラリ。
以下の3つのハンドラーを含む。

| HandlerKey | 対象データ | 入力形式 |
|-----------|-----------|---------|
| `medis-disease` | 病名マスター（ICD-10準拠） | ZIP 内 CSV ×3 |
| `medis-hot` | HOT13 薬品コード | ZIP 内 CSV |
| `medis-jlac10` | JLAC10 検査コード | XLSX |

## 元データ（MEDIS-DC）

| ハンドラー | データ名 | 形式 | 文字コード |
|-----------|---------|------|-----------|
| `medis-disease` | 病名マスター | ZIP 内 CSV 3ファイル（SJIS） | CP932 |
| `medis-hot` | HOT13薬品コード | ZIP 内 CSV（SJIS、ヘッダー行あり） | CP932 |
| `medis-jlac10` | JLAC10検査コード | XLSX（シート「17桁コード表」） | — |

- 入手先: MEDIS-DC（一般社団法人 医療情報システム開発センター）の会員サービス
- 入力: `--source` でローカルファイルパスを指定（必須）
- HTTP 自動取得: 非対応（Full モードのみ）
- 更新頻度: 年次または随時（MEDIS-DC より配布）

---

## medis-disease — 病名マスター（ICD-10準拠）

### CSVファイル構成

ZIP 内に以下の3ファイルが含まれる。ファイル名キーワードで自動識別。

| キーワード | 内容 | ステージングテーブル |
|-----------|------|---------------------|
| `IYK` / `病名` / `diagnosis` | 病名基本情報 | `ext.raw_icd10_diagnosis_codes` |
| `IYM` / `修飾` / `modifier` | 修飾語基本情報 | `ext.raw_icd10_modifier_codes` |
| `IYI` / `索引` / `index` | 索引語情報 | `ext.raw_icd10_index_terms` |

キーワードに一致しない場合はアルファベット順で割り当て（先頭=病名、次=修飾語、末尾=索引語）。

### データベーススキーマ

#### ステージングテーブル
- `ext.raw_icd10_diagnosis_codes` — 病名基本情報（全列 TEXT）
- `ext.raw_icd10_modifier_codes` — 修飾語基本情報（全列 TEXT）
- `ext.raw_icd10_index_terms` — 索引語情報（全列 TEXT）

#### 本番テーブル
- `ext.icd10_diagnosis_codes` — 病名基本情報
- `ext.icd10_modifier_codes` — 修飾語基本情報
- `ext.icd10_index_terms` — 索引語情報

ステージング → 本番は `TRUNCATE` + `INSERT SELECT *`（全カラムコピー）。

### インポートフロー

```
① --source で ZIP ファイルを指定
② ZIP 展開 → CSV ×3（病名 / 修飾語 / 索引語）
③ COPY FROM STDIN (SJIS) → ext.raw_icd10_diagnosis_codes
④ TRUNCATE + INSERT SELECT → ext.icd10_diagnosis_codes（本番） + ステージング TRUNCATE
⑤ （修飾語・索引語も同様に③④を繰り返す）
```

### 使い方

```bash
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" \
  medis-disease --source /path/to/medis_disease.zip
```

---

## medis-hot — HOT13 薬品コード

### CSVカラム仕様

ZIP 内の CSV（SJIS、1行目はヘッダー行でスキップ）。

### データベーススキーマ

| テーブル | 説明 |
|---------|------|
| `ext.raw_hot13_codes` | ステージング（全列 TEXT） |
| `ext.hot13_codes` | 本番テーブル |

ステージング → 本番は `TRUNCATE` + `INSERT SELECT *`。

### インポートフロー

```
① --source で ZIP ファイルを指定
② ZIP 展開 → CSV ストリーム（先頭1行スキップ）
③ COPY FROM STDIN (CP932) → ext.raw_hot13_codes
④ TRUNCATE + INSERT SELECT → ext.hot13_codes（本番）
⑤ ステージング TRUNCATE
```

### 使い方

```bash
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" \
  medis-hot --source /path/to/medis_hot13.zip
```

---

## medis-jlac10 — JLAC10 検査コード

### Excelファイル仕様

| 項目 | 値 |
|------|-----|
| シート名 | `17桁コード表` |
| 開始セル | C5（5行目、C列から） |
| 読み取りカラム | C列=`jlac10_code`、D列=`name` |

### データベーススキーマ

#### `ext.raw_jlac10_codes`（ステージングテーブル）

| カラム | 型 | 説明 |
|--------|----|------|
| `jlac10_code` | `TEXT` | JLAC10コード（17桁） |
| `name` | `TEXT` | 検査名称 |

#### `ext.jlac10_codes`（本番テーブル）

ステージングと同じカラム構成。`TRUNCATE` + `INSERT SELECT *` でマージ。

### インポートフロー

```
① --source で XLSX ファイルを指定
② ClosedXML でシート「17桁コード表」の C5 セルから読み込み
③ COPY BINARY → ext.raw_jlac10_codes (jlac10_code, name)
④ TRUNCATE + INSERT SELECT → ext.jlac10_codes（本番）
⑤ ステージング TRUNCATE
```

### 使い方

```bash
csv-importer --connection "Host=localhost;Database=mydb;Username=user;Password=pass" \
  medis-jlac10 --source /path/to/jlac10.xlsx
```

---

## 関連ファイル

| ファイル | 説明 |
|----------|------|
| `MedisDiseaseImportHandler.cs` | 病名マスターハンドラー（`medis-disease`） |
| `MedisHotImportHandler.cs` | HOT13薬品コードハンドラー（`medis-hot`） |
| `MedisJlac10ImportHandler.cs` | JLAC10検査コードハンドラー（`medis-jlac10`） |
| `ZipEntryStream.cs` | ZIP エントリストリームラッパー |
| `Extensions/ServiceCollectionExtensions.cs` | DI 登録（`AddMedisCodesImport()`） |
