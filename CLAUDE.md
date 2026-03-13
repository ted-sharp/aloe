# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## ビルド・テスト・タスク

```bash
# ソリューション全体をビルド
dotnet build src/Aloe.slnx

# テスト実行（全体）
dotnet test src/Aloe.slnx

# テスト実行（プロジェクト指定）
dotnet test src/Apps/Medock/Aloe.Apps.MedockServer.Tests/
dotnet test src/Utils/Aloe.Utils.CommandLine.Tests/

# 特定テストを名前フィルタで実行
dotnet test --filter "FullyQualifiedName~<TestClassName>.<MethodName>"

# EnvChecker を self-contained でパブリッシュ
dotnet publish src/Apps/EnvChecker/Aloe.Apps.EnvChecker -c Release -r win-x64 --self-contained true
```

## アーキテクチャ概要

### モノリポジトリの構造

`src/Aloe.slnx` がすべてのプロジェクトを束ねるルートソリューション。

```
src/
├── Apps/
│   ├── Medock/              医薬品在庫管理（Blazor Server + PostgreSQL）
│   ├── ExcelReport/         Excel帳票 PDF生成・印刷（CLI + REST/gRPC API）
│   ├── FileBridge/          ディレクトリ監視・外部exe起動
│   ├── RazorReport/         Razor帳票エンジン
│   ├── WindowsServiceMonitor/ Windowsサービス監視（Blazor Server + WPF）
│   ├── SyncBootBridge/      ネットワーク同期ブートストラッパー（.NET Framework 4.6.1）
│   └── EnvChecker/          環境情報収集CLIツール
├── Libs/
│   └── Aloe.Libs.CoreLib/   共有ライブラリ
└── Utils/                   NuGet公開ユーティリティ群
```

### 各アプリの構成パターン

**Medock（代表的なBlazorアプリ）**

```
Aloe.Apps.MedockServer/   Blazor Server + REST API（Microsoft.NET.Sdk.Web）
  Components/             .razor（表示）+ .razor.cs コードビハインド（ロジック）
  Controllers/            REST API エンドポイント
Aloe.Apps.MedockLib/      ビジネスロジック（Class Library）
  Data/                   EF Core DbContext・エンティティ
  Repositories/           データアクセス層
  Services/               ビジネスサービス層
Aloe.Apps.MedockServer.Tests/  xUnit テスト
Aloe.Apps.MedockMcp/      MCP サーバー（NuGet配布）
Aloe.Apps.MedockSeed/     サンプルデータ投入コンソール
```

Blazor は MVVM 的に組む: `.razor` = View、`.razor.cs` = ViewModel（表示状態）、`Lib/Services` = Model（ロジック）。

**ExcelReport（CLIとAPIが共存するパターン）**

```
Aloe.Apps.ExcelReportCli/         CLI（excel-report、generate/print/printersサブコマンド）
Aloe.Apps.ExcelReportApi/         REST + MagicOnion gRPC APIサーバー
Aloe.Apps.ExcelReportLib/         コアライブラリ（Excel読み取り・PDF描画・印刷）
Aloe.Apps.ExcelReportLib.Contracts/ gRPCサービス定義（IReportService）
```

PDF生成パイプライン: `IExcelReader → ITemplateEngine → IPdfRenderer`
印刷パイプライン: `IExcelReader → ITemplateEngine → SkiaSheetRenderer → ISheetPrinter`
DI登録: `AddExcelReport()`（NPOI+PDFsharp）、`AddExcelReportWithWindowsPrinter()`（印刷機能付き）

**WindowsServiceMonitor（Server + Desktop Clientパターン）**

Blazor Server（Web UI）と WPF（タスクトレイ常駐クライアント）を SignalR で繋ぐ構成。WPF クライアントは WebView2 で Server UI を埋め込み、SignalR + HTTP ポーリングでフォールバック付きリアルタイム更新を実現する。

**SyncBootBridge**

.NET Framework 4.6.1 で動作するブートストラッパー（ClickOnce配布）。Win32 API（`GetPrivateProfileString`）でINI設定を読み、ネットワーク共有からアプリを同期して起動する。NuGetパッケージ不使用。

### ユーティリティライブラリのパターン

```
src/Utils/
├── Aloe.Utils.<Name>/        ライブラリ本体
├── Aloe.Utils.<Name>.Tests/  xUnit テスト
└── Aloe.Utils.<Name>.Samples/サンプルプロジェクト
```

ユーティリティは AOT 互換（`IsAotCompatible=true`）・トリミング対応（`IsTrimmable=true`）・NuGet 自動生成（`GeneratePackageOnBuild=true`）が基本。

### 共通設定

- `src/Directory.Build.props` — StyleCop の共有設定（`stylecop.json` を参照、会社名: `ted-sharp`）
- テストフレームワーク: xUnit 2.9.3 + Moq + FluentAssertions + coverlet.collector
- 各アプリの詳細は `README_<appname>.md`（`Lib` プロジェクト内）を参照

### 技術スタック

- .NET 10.0 / C# 14、ソリューション形式は `.slnx`
- Blazor Server（InteractiveServerRendering）+ Tailwind CSS + daisyUI（Medock）
- PostgreSQL + EF Core 9 + Npgsql（Medock のみ）
- MagicOnion（gRPC/SignalR）、Serilog、SkiaSharp、NPOI、PDFsharp

### アーキテクチャ上の方針

- Medock の通信: Blazor コンポーネント内は直接サービス呼び出し、外部からは gRPC（MagicOnion）または REST をステートレスで組む
- レスポンシブ: `@media` クエリではなく CSS Container Query（`@container`）を使用
- Medock の認証: Cookie 認証（独自実装、Issue/Refresh/Revoke 対応）
- テスト方針: テストファースト（構造とテストを先に定義し、失敗テストから順に実装）
