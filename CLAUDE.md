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
```

## アーキテクチャ概要

### モノリポジトリの構造

シンプルなモノリポ構成。`src/Aloe.slnx` がすべてのプロジェクトを束ねるルートソリューション。

```
aloe/
├── docs/                              ドキュメント
├── sql/                               DB スクリプト（medock）
└── src/
    ├── Aloe.slnx                      ルートソリューション
    ├── Directory.Build.props           StyleCop 共有設定
    ├── stylecop.json                   コードスタイル規約
    ├── assets/                         共有アセット（Aloe.png）
    ├── Apps/                           アプリケーション
    │   ├── Medock/                     医薬品在庫管理
    │   ├── FileBridge/                 ファイル転送
    │   ├── RazorReport/               帳票エンジン
    │   ├── WindowsServiceMonitor/     サービス監視
    │   └── SyncBridge/                配布同期
    ├── Libs/                           共有ライブラリ
    │   └── Aloe.Libs.CoreLib/
    └── Utils/                          ユーティリティ（NuGet公開）
        ├── Aloe.Utils.CommandLine/
        ├── Aloe.Utils.CommandLine.Tests/
        ├── Aloe.Utils.CommandLine.Samples/
        └── ...
```

### アプリプロジェクトのパターン（Medock を例に）

```
src/Apps/Medock/
├── Aloe.Apps.MedockServer/   ← Blazor Server + REST API（SDK: Microsoft.NET.Sdk.Web）
│   ├── Components/           ← Razor コンポーネント（.razor + .razor.cs コードビハインド）
│   └── Controllers/          ← REST API エンドポイント
├── Aloe.Apps.MedockLib/      ← ビジネスロジック（Class Library）
│   ├── Data/                 ← EF Core DbContext・エンティティ
│   ├── Repositories/         ← データアクセス層
│   └── Services/             ← ビジネスサービス層
└── Aloe.Apps.MedockServer.Tests/  ← xUnit テスト
```

Blazor コンポーネントは `.razor`（表示）と `.razor.cs` コードビハインド（ロジック）の 2 ファイル構成。

### ユーティリティライブラリのパターン

```
src/Utils/
├── Aloe.Utils.<Name>/        ← ライブラリ本体
├── Aloe.Utils.<Name>.Tests/  ← xUnit テスト
└── Aloe.Utils.<Name>.Samples/← サンプルプロジェクト
```

ユーティリティは AOT 互換（`IsAotCompatible=true`）・トリミング対応（`IsTrimmable=true`）・NuGet 自動生成（`GeneratePackageOnBuild=true`）が基本。

### 共通設定

- `src/Directory.Build.props` — StyleCop の共有設定（`stylecop.json` を参照）
- `src/stylecop.json` — コードスタイル規約（会社名: `ted-sharp`）
- テストフレームワーク: xUnit 2.9.3 + Moq + FluentAssertions + coverlet.collector

### 技術スタック

- .NET 10.0 / C# 14、ソリューション形式は `.slnx`
- Blazor Server（InteractiveServerRendering）
- PostgreSQL + EF Core 9 + Npgsql（medock のみ）
- SignalR、Serilog、Tailwind CSS + daisyUI
