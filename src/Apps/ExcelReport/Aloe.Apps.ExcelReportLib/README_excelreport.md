# Aloe.Apps.ExcelReport

Excel 方眼紙テンプレートから PDF を生成・印刷するツール群。セル書式・罫線・図形・画像を忠実に再現し、テンプレート変数（`${keyword}`）の置換にも対応。

## 必要な環境

- .NET 10.0
- 印刷機能は Windows のみ対応

## ソリューション構成

| プロジェクト | 説明 |
|-------------|------|
| **Aloe.Apps.ExcelReportCli** | CLI ツール（アセンブリ名: `excel-report`） |
| **Aloe.Apps.ExcelReportApi** | REST / gRPC API サーバー（MagicOnion） |
| **Aloe.Apps.ExcelReportLib** | コアライブラリ（Excel 読み取り・PDF 描画・印刷） |
| **Aloe.Apps.ExcelReportLib.Contracts** | gRPC サービス定義（MagicOnion インターフェース） |

## ビルド・実行

```powershell
# CLI ビルド
dotnet build src/Apps/ExcelReport/Aloe.Apps.ExcelReportCli/

# API サーバー ビルド
dotnet build src/Apps/ExcelReport/Aloe.Apps.ExcelReportApi/

# CLI 実行
dotnet run --project src/Apps/ExcelReport/Aloe.Apps.ExcelReportCli/ -- generate template.xlsx output.pdf
```

## CLI の使い方

```
excel-report <サブコマンド> [options]
```

### サブコマンド一覧

| サブコマンド | 説明 |
|-------------|------|
| `generate` | Excel テンプレートから PDF を生成する |
| `print` | Excel テンプレートをプリンターへ印刷する（Windows のみ） |
| `printers` | インストール済みプリンターの一覧を表示する（Windows のみ） |

---

### `generate` — PDF 生成

```
excel-report generate <input> <output> [options]
```

| 引数 / オプション | 説明 |
|-----------------|------|
| `<input>` | 入力 Excel ファイルのパス（.xlsx） |
| `<output>` | 出力 PDF ファイルのパス |
| `--sheet, -s <index>` | シートインデックス（0 始まり）。省略時は先頭シート |
| `--var, -v <Key=Value>` | テンプレート変数。複数指定可 |
| `--var-file, -f <path>` | 変数を定義した JSON ファイル。`--var` より低優先 |
| `--excel-reader <library>` | `npoi`（デフォルト） / `closedxml` |
| `--pdf-renderer <library>` | `pdfsharp`（デフォルト） / `questpdf` |

```powershell
# 基本的な変換
excel-report generate template.xlsx output.pdf

# シート指定 + テンプレート変数
excel-report generate template.xlsx output.pdf --sheet 0 --var "Name=山田太郎" --var "Date=2024-01-15"

# JSON ファイルから変数を読み込み
excel-report generate template.xlsx output.pdf --var-file vars.json

# ClosedXML + QuestPDF の組み合わせ
excel-report generate form.xlsx result.pdf --excel-reader closedxml --pdf-renderer questpdf
```

---

### `print` — 印刷（Windows のみ）

```
excel-report print <input> --printer <name> [options]
```

| 引数 / オプション | 説明 |
|-----------------|------|
| `<input>` | 入力 Excel ファイルのパス（.xlsx） |
| `--printer, -p <name>` | 送信先プリンター名（必須） |
| `--output, -o <path>` | 中間 PDF の保存先。省略時は一時ファイルを使用して印刷後に削除 |
| `--sheet, -s <index>` | シートインデックス（0 始まり）。省略時は先頭シート |
| `--var, -v <Key=Value>` | テンプレート変数。複数指定可 |
| `--var-file, -f <path>` | 変数を定義した JSON ファイル |
| `--copies, -c <n>` | 印刷部数（デフォルト: 1） |
| `--dpi <n>` | 印刷解像度（デフォルト: 300） |

```powershell
# プリンター一覧を確認
excel-report printers

# 印刷（中間 PDF は一時ファイル）
excel-report print template.xlsx --printer "Microsoft Print to PDF"

# テンプレート変数 + 2 部印刷
excel-report print template.xlsx --printer "HP LaserJet" --var "Name=山田太郎" --copies 2

# 中間 PDF を保存しながら印刷
excel-report print template.xlsx --printer "HP LaserJet" --output /tmp/output.pdf
```

---

## REST API の使い方

`Aloe.Apps.ExcelReportApi` を起動すると以下のエンドポイントが利用できます。

### PDF 生成ジョブ

| メソッド | パス | 説明 |
|---------|------|------|
| `POST` | `/api/reports` | PDF 生成ジョブをキューに投入 |
| `GET` | `/api/reports` | ジョブ一覧（`?status=Pending/Running/Completed/Failed` で絞り込み可） |
| `GET` | `/api/reports/{jobId}/status` | ジョブのステータスを取得 |
| `GET` | `/api/reports/{jobId}/download` | 生成済み PDF をダウンロード |

**POST /api/reports** — `multipart/form-data`

| フィールド | 型 | 説明 |
|-----------|-----|------|
| `template` | ファイル | Excel ファイル（`templateName` と排他） |
| `templateName` | string | サーバー側保存済みテンプレート名（`template` と排他） |
| `variables` | string | 置換変数（JSON 形式: `{"key": "value"}`） |
| `sheetIndex` | int | シートインデックス（デフォルト: 0） |

```powershell
# ファイルをアップロードして PDF 生成
$jobId = (curl -s -X POST http://localhost:5000/api/reports `
  -F "template=@template.xlsx" `
  -F 'variables={"Name":"山田太郎"}' | ConvertFrom-Json).jobId

# ステータス確認
curl http://localhost:5000/api/reports/$jobId/status

# PDF ダウンロード
curl -o output.pdf http://localhost:5000/api/reports/$jobId/download
```

### テンプレート管理

| メソッド | パス | 説明 |
|---------|------|------|
| `GET` | `/api/templates` | 保存済みテンプレート一覧 |
| `POST` | `/api/templates` | テンプレートをサーバーに保存 |
| `DELETE` | `/api/templates/{templateName}` | テンプレートを削除 |

### 印刷ジョブ（Windows のみ）

| メソッド | パス | 説明 |
|---------|------|------|
| `GET` | `/api/printers` | インストール済みプリンター一覧 |
| `POST` | `/api/printjobs` | 印刷ジョブをキューに投入 |
| `GET` | `/api/printjobs/{jobId}/status` | 印刷ジョブのステータスを取得 |

**POST /api/printjobs** — `multipart/form-data`

| フィールド | 型 | 説明 |
|-----------|-----|------|
| `template` | ファイル | Excel ファイル（必須） |
| `printerName` | string | 送信先プリンター名（必須） |
| `variables` | string | 置換変数（JSON 形式） |
| `sheetIndex` | int | シートインデックス（デフォルト: 0） |
| `copies` | int | 印刷部数（デフォルト: 1） |

### API サーバー設定（appsettings.json）

```json
{
  "ExcelReport": {
    "TemplatePath": "templates",
    "MaxJobAgeMins": 60,
    "PrintOutputPath": "printjobs",
    "MaxPrintedPdfCount": 100
  }
}
```

---

## gRPC の使い方（MagicOnion）

`IReportService` インターフェース（`Aloe.Apps.ExcelReportLib.Contracts`）経由で REST と同等の機能を gRPC で利用できます。

| メソッド | 説明 |
|---------|------|
| `SubmitJobAsync` | PDF 生成ジョブを投入 |
| `GetJobStatusAsync` | ジョブステータスを取得 |
| `ListJobsAsync` | ジョブ一覧を取得 |
| `UploadTemplateAsync` | テンプレートをアップロード |
| `ListTemplatesAsync` | テンプレート一覧を取得 |
| `ListPrintersAsync` | プリンター一覧を取得 |
| `SubmitPrintJobAsync` | 印刷ジョブを投入 |
| `GetPrintJobStatusAsync` | 印刷ジョブステータスを取得 |

---

## テンプレート変数

Excel のセル値や図形テキストに含まれる `${keyword}` を `--var` で指定した値に置換します。

```
セル内容: 請求書番号 ${InvoiceId}
コマンド: --var "InvoiceId=A-001"
結果:     請求書番号 A-001
```

### 変数ファイル（JSON）

```json
{
  "InvoiceId": "A-001",
  "Name": "山田太郎",
  "Date": "2024-01-15"
}
```

`--var` と併用した場合、`--var` の値が優先されます。
プレフィックス（`${`）とサフィックス（`}`）はライブラリ利用時に `TemplateOptions` でカスタマイズ可能です。

---

## アーキテクチャ

### PDF 生成パイプライン

```
IExcelReader → ITemplateEngine → IPdfRenderer → PDF ファイル
```

### 印刷パイプライン

```
IExcelReader → ITemplateEngine → SkiaSheetRenderer → ISheetPrinter → プリンター
```

### Excel リーダー

| ライブラリ | 図形・画像 | 備考 |
|-----------|-----------|------|
| NPOI（デフォルト） | 対応 | フル機能 |
| ClosedXML | 非対応 | 図形不要な場合に利用 |

### PDF レンダラー

| ライブラリ | 備考 |
|-----------|------|
| PDFsharp（デフォルト） | 軽量 |
| QuestPDF | SkiaSharp ベース |

### プリンター

| 実装 | 対応 OS | 備考 |
|-----|---------|------|
| `WindowsSheetPrinter` | Windows のみ | SkiaSharp でラスタライズして印刷 |

---

## ライブラリとして利用

DI 拡張メソッドで簡単に組み込めます:

```csharp
// デフォルト構成（NPOI + PDFsharp）
services.AddExcelReport();

// 個別に選択
services.AddExcelReportCore();
services.AddExcelReportWithNpoi();
services.AddExcelReportWithClosedXml();
services.AddExcelReportWithPdfSharp();
services.AddExcelReportWithQuestPdf();

// 印刷機能付き（NPOI + QuestPDF + WindowsSheetPrinter）
services.AddExcelReportWithWindowsPrinter();
```
