# aloe

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet) ![License](https://img.shields.io/badge/ライブラリ-MIT-green) ![Platform](https://img.shields.io/badge/platform-Windows-blue)

Git Subtree で管理された .NET 10 アプリケーション＆ユーティリティライブラリ群のモノリポジトリです。

---

## リポジトリ構成

### アプリケーション（5個）

| フォルダ | 説明 | 主な技術 |
|---|---|---|
| [aloe-apps-filebridge](./aloe-apps-filebridge/) | ファイル監視・自動処理 | Blazor Server, SignalR |
| [aloe-apps-medock](./aloe-apps-medock/) | 医療予約管理 | Blazor Server, PostgreSQL, EF Core |
| [aloe-apps-rrd](./aloe-apps-rrd/) | Razor レポート生成 | Blazor Server |
| [aloe-apps-servicemonitor](./aloe-apps-servicemonitor/) | Windows サービス監視 | Blazor Server, WPF |
| [aloe-apps-syncbridge](./aloe-apps-syncbridge/) | アプリ配布ブートストラッパー | .NET Framework 4.6.1 |

### ユーティリティライブラリ（16個）

**汎用**

| フォルダ | 説明 |
|---|---|
| [aloe-utils](./aloe-utils/) | 共通ユーティリティ |
| [aloe-utils-async](./aloe-utils-async/) | 非同期処理ヘルパー |
| [aloe-utils-commandline](./aloe-utils-commandline/) | コマンドライン引数処理 |
| [aloe-utils-json](./aloe-utils-json/) | JSON 処理 |
| [aloe-utils-logging-dump](./aloe-utils-logging-dump/) | ログ・ダンプ出力 |
| [aloe-utils-safeio](./aloe-utils-safeio/) | 安全なファイル I/O |
| [aloe-utils-text](./aloe-utils-text/) | テキスト処理 |

**設定**

| フォルダ | 説明 |
|---|---|
| [aloe-utils-configuration-default](./aloe-utils-configuration-default/) | デフォルト設定プロバイダー |
| [aloe-utils-configuration-json](./aloe-utils-configuration-json/) | JSON 設定プロバイダー |

**描画**

| フォルダ | 説明 |
|---|---|
| [aloe-utils-drawing](./aloe-utils-drawing/) | 描画ユーティリティ |
| [aloe-utils-drawing-wpf](./aloe-utils-drawing-wpf/) | WPF 描画ユーティリティ |

**日本語処理**

| フォルダ | 説明 |
|---|---|
| [aloe-utils-wafu-date](./aloe-utils-wafu-date/) | 和暦・元号処理 |
| [aloe-utils-wafu-jiscompat](./aloe-utils-wafu-jiscompat/) | JIS 互換文字処理 |
| [aloe-utils-wafu-kansuji](./aloe-utils-wafu-kansuji/) | 漢数字変換 |
| [aloe-utils-wafu-romaji](./aloe-utils-wafu-romaji/) | ローマ字変換 |

**Windows**

| フォルダ | 説明 |
|---|---|
| [aloe-utils-win32-sccommand](./aloe-utils-win32-sccommand/) | Win32 サービス制御コマンド |

### ルートファイル

| ファイル | 説明 |
|---|---|
| `Aloe.slnx` | 全プロジェクトを束ねるルートソリューション（Visual Studio 2022+ 形式） |
| `Aloe.slnx.DotSettings` | ReSharper コードクリーンアップ設定 |
| `Directory.Build.props` | 全プロジェクト共通の MSBuild プロパティ（StyleCop 設定参照） |
| `stylecop.json` | StyleCop コードスタイル規約（会社名: `ted-sharp`） |
| `Taskfile.yml` | サブツリー管理タスク（`setup` / `treepull` / `treepush`） |
| `scripts/` | サブツリー操作 PowerShell スクリプト群 |

---

## 技術スタック

- **.NET 10.0 / C# 14**
- **Blazor Server**（InteractiveServerRendering）
- **PostgreSQL + EF Core 9 + Npgsql**
- **SignalR, Serilog, Tailwind CSS + daisyUI**

---

## 前提条件

| ツール | 用途 |
|---|---|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | ビルド・実行 |
| [Task](https://taskfile.dev) | タスクランナー |
| PostgreSQL 18+ | medock 使用時 |
| PowerShell | スクリプト実行 |

---

## セットアップ

```bash
# サブツリーリモートの登録と初回チェックアウト
task setup

# すべてのサブツリーを最新に更新
task treepull

# 特定サブツリーへ変更を書き戻す
task treepush PREFIX=aloe-utils
```

---

## ライセンス

- **ユーティリティライブラリ**: MIT
- **アプリケーション**: 個人利用目的
