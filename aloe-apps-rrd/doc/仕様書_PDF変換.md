# PDF変換定義

## 変換方法

- **方法**: Playwright for .NET を使用
- **ブラウザ**: 
  - **デフォルト**: Microsoft Edge（Chromiumベース、システムにインストール済みのEdgeを使用）
  - **代替**: Playwrightが管理するChromiumブラウザ（オフライン環境やEdgeがインストールされていない場合）
- **処理フロー**:
  1. HTML文字列をメモリに保持
  2. Playwrightでブラウザを起動（`Channel = "msedge"` または `Channel = "chromium"` を指定）
  3. HTMLをブラウザに読み込み
  4. PDF出力
  5. バイト配列をファイルに保存

## 出力

- **出力形式**: PDFファイルとして保存
- **保存ダイアログ**: ファイル保存ダイアログを表示
- **デフォルトファイル名**: テンプレート名 + ".pdf"
- **出力方式**: バイト配列として取得し、ユーザーがダウンロード

## PDF設定

PDF設定は`appsettings.pdf.json`でデフォルト値を管理し、UIで上書き可能です。

### appsettings.pdf.jsonの構造

PDF変換のデフォルト設定を定義：

```json
{
  "pdf": {
    "format": "A4",
    "width": null,
    "height": null,
    "landscape": false,
    "margin": {
      "top": "5mm",
      "bottom": "5mm",
      "left": "5mm",
      "right": "5mm"
    },
    "scale": 1.0,
    "printBackground": true,
    "preferCSSPageSize": false,
    "displayHeaderFooter": false,
    "headerTemplate": null,
    "footerTemplate": null,
    "pageRanges": null
  }
}
```

**Playwright PDFオプションの対応項目**:

#### ページサイズ・向き設定

- **`format`**: 用紙フォーマット（`"A4"`, `"A3"`, `"A5"`, `"Letter"`, `"Legal"`, `"B4"`, `"B5"`等）。指定時は`width`/`height`より優先されます
- **`width`**: カスタム用紙幅（単位付き文字列、例：`"210mm"`, `"8.5in"`, `"793.7px"`）。`format`が指定されていない場合に使用
- **`height`**: カスタム用紙高さ（単位付き文字列）。`format`が指定されていない場合に使用
- **`landscape`**: 横向きにするか（`true`/`false`）。デフォルトは`false`（縦向き）

**単位**: `px`（ピクセル）、`mm`（ミリメートル）、`cm`（センチメートル）、`in`（インチ）が使用可能

#### マージン設定

- **`margin`**: マージン設定（オブジェクト）
  - `top`: 上マージン（単位付き文字列、例：`"5mm"`）
  - `bottom`: 下マージン
  - `left`: 左マージン
  - `right`: 右マージン
  - 各値は単位付き文字列（`"5mm"`, `"0.5in"`, `"20px"`等）

#### スケール・品質設定

- **`scale`**: スケール（倍率、数値）。`1.0`が100%、`0.5`が50%、`2.0`が200%。デフォルトは`1.0`
- **`printBackground`**: 背景の印刷を有効にするか（`true`/`false`）。CSS背景色・画像を含める。デフォルトは`false`（Playwrightのデフォルトに合わせる場合は`false`、背景を印刷する場合は`true`）
- **`preferCSSPageSize`**: CSSの`@page`ルールを優先するか（`true`/`false`）。`true`の場合、HTML内の`@page`ルールが`format`/`width`/`height`より優先されます。デフォルトは`false`

#### ヘッダー・フッター設定

- **`displayHeaderFooter`**: ヘッダー・フッターを表示するか（`true`/`false`）。デフォルトは`false`
- **`headerTemplate`**: カスタムヘッダーのHTMLテンプレート（文字列、`null`で無効）。`displayHeaderFooter`が`true`の場合に使用
- **`footerTemplate`**: カスタムフッターのHTMLテンプレート（文字列、`null`で無効）。`displayHeaderFooter`が`true`の場合に使用

**ヘッダー・フッターテンプレートの変数**:
- `{{title}}`: ページタイトル
- `{{url}}`: ページURL
- `{{date}}`: 現在の日付
- `{{time}}`: 現在の時刻
- `{{pageNumber}}`: 現在のページ番号
- `{{totalPages}}`: 総ページ数

#### ページ範囲設定

- **`pageRanges`**: 印刷するページ範囲（文字列、`null`で全ページ）。例：`"1-5"`, `"1,3,5"`, `"1-3,5-7"`

**デザイン時ページサイズとの連携**:
- PDF出力時のデフォルト値は、デザイン時に設定したページサイズと向きを使用
- デザイン時の設定がない場合は、`appsettings.pdf.json`の設定を使用
- PDF出力時にページサイズと向きを上書き可能
- カスタムサイズでデザインした場合も、PDF出力時に別のサイズに変更可能

**設定の優先順位**:
1. UIで指定された値（最優先）
2. デザイン時に設定したページサイズ・向き
3. `appsettings.pdf.json`の設定値

### 印刷時の非印刷領域との関係

プリンタによって非印刷領域（印刷できない端の領域）は異なります。きれいに印刷するためには、PDFに適切なマージンを設定することを推奨します。

**マージン設定の推奨値**:
- **一般的なプリンタ**: 上下左右 各5mm以上を推奨
- **レーザープリンタ**: 上下左右 各3-5mm程度
- **インクジェットプリンタ**: 上下左右 各5-10mm程度（機種により異なる）
- **業務用プリンタ**: プリンタの仕様書を確認して適切な値を設定

**印刷時の動作**:

1. **100%表示（スケール100%）の場合**:
   - PDFに設定されたマージン内のコンテンツは印刷可能領域として扱われます
   - マージン外（非印刷領域）のコンテンツは、プリンタの設定によっては印刷されない可能性があります
   - プリンタが「実際のサイズ」または「100%」で印刷する場合、マージン外の部分は切り取られる可能性があります

2. **FIT設定（用紙に合わせる）の場合**:
   - プリンタが自動的にPDF全体を印刷可能領域内に収めるように縮小します
   - マージンが適切に設定されていれば、コンテンツが縮小されてもレイアウトが崩れにくくなります
   - マージンが小さすぎる場合、コンテンツが非印刷領域にかかって一部が切れる可能性があります

**きれいに印刷するための推奨設定**:

1. **PDFに適切なマージンを設定**: 使用するプリンタの非印刷領域を考慮して、上下左右に十分なマージン（推奨：各5mm以上）を設定
2. **プリンタ側の設定**: 
   - 「実際のサイズ」または「100%」で印刷する場合：PDFのマージン内にすべてのコンテンツが収まるように設計
   - 「用紙に合わせる」または「FIT」で印刷する場合：マージンが適切に設定されていれば、自動縮小されてもレイアウトが維持される
3. **テスト印刷**: 実際に使用するプリンタでテスト印刷を行い、マージン値を調整

**マージン設定の単位**:
- mm（ミリメートル）: 印刷用途に適した単位
- px（ピクセル）: 画面表示基準の単位
- inch（インチ）: 海外のプリンタ仕様に合わせる場合

## 初回セットアップ

- **Edge使用**: システムにインストール済みのMicrosoft Edgeを使用（追加ダウンロード不要）
- **Playwrightドライバ**: 初回実行時にPlaywrightドライバを自動ダウンロード（約10MB）
  - ダウンロード位置: ユーザーの `.playwright` ディレクトリ（`~/.playwright/`）
  - インターネット接続が必要（初回セットアップ時のみ）
  - トリガー:
    - `await Playwright.CreateAsync()` 初回呼び出し時
    - または `playwright.ps1 install` PowerShell スクリプトで事前ダウンロード

## オフライン環境での対応

### システムブラウザ（Edge）が利用可能な場合

システムにMicrosoft Edgeがインストールされている場合は、追加のセットアップは不要です。Edgeはオフライン環境でも利用可能です。

### システムブラウザが利用できない場合

オフライン環境でEdgeがインストールされていない、またはPlaywrightドライバの事前ダウンロードが必要な場合は、以下の方法があります：

#### 方法1: Playwrightが管理するブラウザ（Chromium）を使用

- **ブラウザ**: Playwrightが管理するChromiumブラウザを使用
- **事前準備**: インターネット接続がある環境で事前にブラウザをダウンロード
  - `playwright.ps1 install chromium` を実行してChromiumをダウンロード
  - `.playwright` ディレクトリごとオフライン環境に転送
- **設定**: Playwrightの起動時に `Channel = "chromium"` を指定（または `Channel` を指定せずにデフォルトのChromiumを使用）

#### 方法2: 事前ダウンロードしたブラウザを配布

- **手順**:
  1. インターネット接続がある環境で `playwright.ps1 install` を実行
  2. `.playwright` ディレクトリをアプリケーションの配布パッケージに含める
  3. オフライン環境でアプリケーションをインストール時に、`.playwright` ディレクトリを適切な場所に配置
- **注意**: `.playwright` ディレクトリは約100-200MB程度のサイズになります（Chromiumのみの場合）

#### 方法3: カスタムブラウザパスの指定

- **設定**: Playwrightの起動時に `ExecutablePath` を指定して、事前にインストールしたブラウザのパスを直接指定
- **用途**: 企業内で標準化されたブラウザを使用する場合など

## 実装詳細

Playwright for .NETを使用し、Edgeブラウザを起動してHTMLをPDFに変換。実装詳細は[Playwright for .NET 公式ドキュメント](https://playwright.dev/dotnet/)を参照。

### Playwright PDFオプションの実装

`appsettings.pdf.json`の設定値をPlaywrightの`PagePdfOptions`にマッピングして使用します。

**マッピング例**:
- `format`: `PagePdfOptions.Format`（列挙型、`"A4"` → `PaperFormat.A4`）
- `width`/`height`: `PagePdfOptions.Width`/`Height`（単位付き文字列をそのまま使用）
- `landscape`: `PagePdfOptions.Landscape`（bool）
- `margin`: `PagePdfOptions.Margin`（`Margin`オブジェクト、各値を単位付き文字列で設定）
- `scale`: `PagePdfOptions.Scale`（double）
- `printBackground`: `PagePdfOptions.PrintBackground`（bool）
- `preferCSSPageSize`: `PagePdfOptions.PreferCSSPageSize`（bool）
- `displayHeaderFooter`: `PagePdfOptions.DisplayHeaderFooter`（bool）
- `headerTemplate`/`footerTemplate`: `PagePdfOptions.HeaderTemplate`/`FooterTemplate`（string）
- `pageRanges`: `PagePdfOptions.PageRanges`（string）

**注意事項**:
- PDF生成はChromiumベースのブラウザ（Edge、Chromium）でのみサポートされます
- `format`と`width`/`height`の両方が指定されている場合、`format`が優先されます
- マージンの単位は`px`、`mm`、`cm`、`in`が使用可能です
- ヘッダー・フッターテンプレートはHTML形式で、Playwrightの変数（`{{pageNumber}}`等）が使用可能です
