# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## ビルド・テスト・タスク

```bash
# ソリューション全体をビルド
dotnet build Aloe.slnx

# 特定サブツリーをビルド（例）
dotnet build aloe-utils-commandline/src/Aloe.Utils.CommandLine.slnx

# テスト実行（プロジェクト指定）
dotnet test aloe-apps-medock/src/Aloe/Tests/Aloe.Apps.MedockServer.Tests/
dotnet test aloe-utils-commandline/src/Aloe.Utils.CommandLine.Tests/

# 特定テストを名前フィルタで実行
dotnet test --filter "FullyQualifiedName~<TestClassName>.<MethodName>"

# サブツリー管理
task setup                       # 全サブツリーの初期登録（冪等）
task treepull                    # 全サブツリーを最新に更新
task treepush PREFIX=aloe-utils  # 特定サブツリーへ変更を書き戻す
```

## アーキテクチャ概要

### モノリポジトリの構造

各サブフォルダは独立した GitHub リポジトリを **git subtree** でマージしたもの。`Aloe.slnx` がすべてのプロジェクトを束ねるルートソリューション。`scripts/subtrees.ps1` にサブツリーの URL 一覧が定義されている。

### アプリプロジェクトのパターン（Medock を例に）

```
aloe-apps-medock/src/Aloe/Apps/Medock/
├── Aloe.Apps.MedockServer/   ← Blazor Server + REST API（SDK: Microsoft.NET.Sdk.Web）
│   ├── Components/           ← Razor コンポーネント（.razor + .razor.cs コードビハインド）
│   └── Controllers/          ← REST API エンドポイント
├── Aloe.Apps.MedockLib/      ← ビジネスロジック（Class Library）
│   ├── Data/                 ← EF Core DbContext・エンティティ
│   ├── Repositories/         ← データアクセス層
│   └── Services/             ← ビジネスサービス層
└── Tests/                    ← xUnit テスト
```

Blazor コンポーネントは `.razor`（表示）と `.razor.cs` コードビハインド（ロジック）の 2 ファイル構成。

### ユーティリティライブラリのパターン

```
aloe-utils-<name>/src/
├── Aloe.Utils.<Name>/        ← ライブラリ本体
├── Aloe.Utils.<Name>.Tests/  ← xUnit テスト
├── Aloe.Utils.<Name>.Samples/← サンプルプロジェクト
└── Aloe.Utils.<Name>.slnx    ← 独立ソリューション
```

ユーティリティは AOT 互換（`IsAotCompatible=true`）・トリミング対応（`IsTrimmable=true`）・NuGet 自動生成（`GeneratePackageOnBuild=true`）が基本。

### 共通設定

- `Directory.Build.props` — StyleCop の共有設定（`stylecop.json` を参照）
- `stylecop.json` — コードスタイル規約（会社名: `ted-sharp`）
- テストフレームワーク: xUnit 2.9.3 + Moq + FluentAssertions + coverlet.collector

### 技術スタック

- .NET 10.0 / C# 14、ソリューション形式は `.slnx`
- Blazor Server（InteractiveServerRendering）
- PostgreSQL + EF Core 9 + Npgsql（medock のみ）
- SignalR、Serilog、Tailwind CSS + daisyUI

## サブツリーのワークフロー

1. サブツリーディレクトリ内で編集
2. ルートリポジトリに通常通りコミット
3. `task treepush PREFIX=<subtree名>` で対応する GitHub リポジトリへ反映
