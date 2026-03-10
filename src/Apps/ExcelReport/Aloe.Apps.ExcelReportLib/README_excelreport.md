# Aloe.Apps.ExcelReport

Excel 方眼紙テンプレートから PDF を生成する CLI ツール。セル書式・罫線・図形・画像を忠実に再現し、テンプレート変数（`${keyword}`）の置換にも対応。

## 必要な環境

- .NET 10.0

## ソリューション構成

- **Aloe.Apps.ExcelReport** … CLI ツール（System.CommandLine ベース、アセンブリ名: `excel-report`）
- **Aloe.Apps.ExcelReportLib** … Excel 読み取り・PDF 描画・テンプレート置換のコアライブラリ

## ビルド・実行

```powershell
# ビルド
dotnet build src/Apps/ExcelReport/Aloe.Apps.ExcelReport/

# 実行例
dotnet run --project src/Apps/ExcelReport/Aloe.Apps.ExcelReport/ -- template.xlsx output.pdf
```

## CLI の使い方

```
excel-report <input> <output> [options]
```

### 引数

| 引数 | 説明 |
|------|------|
| `<input>` | 入力 Excel ファイルのパス（.xlsx） |
| `<output>` | 出力 PDF ファイルのパス |

### オプション

| オプション | 説明 |
|-----------|------|
| `--sheet, -s <index>` | シートインデックス（0 始まり）。省略時は先頭シート |
| `--var, -v <Key=Value>` | テンプレート変数。複数指定可 |
| `--var-file, -f <path>` | 置換変数を定義した JSON ファイルのパス。`--var` と併用時は `--var` が優先 |
| `--excel-reader <library>` | Excel 読み取りライブラリ（`npoi` / `closedxml`）。デフォルト: `npoi` |
| `--pdf-renderer <library>` | PDF 描画ライブラリ（`pdfsharp` / `questpdf`）。デフォルト: `pdfsharp` |

### 使用例

```powershell
# 基本的な変換
excel-report template.xlsx output.pdf

# シート指定 + テンプレート変数
excel-report template.xlsx output.pdf --sheet 0 --var "Name=山田太郎" --var "Date=2024-01-15"

# JSON ファイルから変数を読み込み
excel-report template.xlsx output.pdf --var-file vars.json

# JSON ファイル + --var 併用（--var が優先）
excel-report template.xlsx output.pdf --var-file vars.json --var "Name=上書き太郎"

# ClosedXML + QuestPDF の組み合わせ
excel-report form.xlsx result.pdf --excel-reader closedxml --pdf-renderer questpdf
```

## テンプレート変数

Excel のセル値や図形テキストに含まれる `${keyword}` を、`--var` で指定した値に置換します。

```
セル内容: 請求書番号 ${InvoiceId}
コマンド: --var "InvoiceId=A-001"
結果:     請求書番号 A-001
```

### 変数ファイル（JSON）

`--var-file` で JSON ファイルから変数を一括読み込みできます。フラットな `{ "key": "value" }` 形式のみ対応。

```json
{
  "InvoiceId": "A-001",
  "Name": "山田太郎",
  "Date": "2024-01-15"
}
```

`--var` と併用した場合、`--var` の値が優先されます（ファイルの値を上書き）。

プレフィックス（`${`）とサフィックス（`}`）はライブラリ利用時に `TemplateOptions` でカスタマイズ可能です。

## アーキテクチャ

3 段階のパイプラインで処理します:

1. **IExcelReader** — Excel を読み取り、中間モデル（`SheetModel`）に変換
2. **ITemplateEngine** — テンプレート変数を置換
3. **IPdfRenderer** — `SheetModel` から PDF を描画

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

## ライブラリとして利用

DI 拡張メソッドで簡単に組み込めます:

```csharp
// デフォルト構成（NPOI + PDFsharp）
services.AddExcelReport();

// 個別に選択
services.AddExcelReportCore();
services.AddExcelReportWithClosedXml();
services.AddExcelReportWithQuestPdf();
```
