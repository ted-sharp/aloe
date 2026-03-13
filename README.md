# aloe

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet) ![License](https://img.shields.io/badge/ライブラリ-MIT-green) ![Platform](https://img.shields.io/badge/platform-Windows-blue)

.NET 10 アプリケーション＆ユーティリティライブラリ群のモノリポジトリです。

---

## リポジトリ構成

```
aloe/
├── docs/                              ドキュメント
├── sql/                               DB スクリプト（medock）
└── src/
    ├── Aloe.slnx                      ルートソリューション
    ├── Directory.Build.props           共通 MSBuild プロパティ（StyleCop）
    ├── stylecop.json                   コードスタイル規約
    ├── Apps/                           アプリケーション
    │   ├── Medock/                     医薬品在庫管理
    │   ├── FileBridge/                 ファイル転送
    │   ├── RazorReport/               帳票エンジン
    │   ├── WindowsServiceMonitor/     サービス監視
    │   └── SyncBootBridge/            配布同期（.NET Framework 4.6.1）
    ├── Libs/                           共有ライブラリ
    │   └── Aloe.Libs.CoreLib/
    └── Utils/                          ユーティリティ（NuGet 公開）
```

### アプリケーション

| フォルダ | 説明 | 主な技術 |
|---|---|---|
| `Apps/FileBridge` | ファイル監視・自動処理 | Blazor Server, SignalR |
| `Apps/Medock` | 医薬品在庫管理 | Blazor Server, PostgreSQL, EF Core |
| `Apps/RazorReport` | Razor レポート生成 | Blazor Server |
| `Apps/WindowsServiceMonitor` | Windows サービス監視 | Blazor Server |
| `Apps/SyncBootBridge` | アプリ配布ブートストラッパー | .NET Framework 4.6.1 |

### ユーティリティライブラリ（NuGet 公開 / 13本）

| パッケージ | 説明 |
|---|---|
| `Aloe.Utils.CommandLine` | コマンドライン引数処理 |
| `Aloe.Utils.Configuration.Default` | デフォルト設定プロバイダー |
| `Aloe.Utils.Configuration.Json` | JSON 設定プロバイダー |
| `Aloe.Utils.Drawing` | 描画ユーティリティ |
| `Aloe.Utils.Drawing.Wpf` | WPF 描画ユーティリティ |
| `Aloe.Utils.Json` | JSON 処理 |
| `Aloe.Utils.Logging.Dump` | ログ・ダンプ出力 |
| `Aloe.Utils.SafeIO` | 安全なファイル I/O |
| `Aloe.Utils.Text` | テキスト処理 |
| `Aloe.Utils.Wafu.Date` | 和暦・元号処理 |
| `Aloe.Utils.Wafu.JisCompat` | JIS 互換文字処理 |
| `Aloe.Utils.Wafu.Kansuji` | 漢数字変換 |
| `Aloe.Utils.Wafu.Romaji` | ローマ字変換 |
| `Aloe.Utils.Win32.ScCommand` | Win32 サービス制御コマンド |

---

## 技術スタック

- **.NET 10.0 / C# 14**、ソリューション形式は `.slnx`
- **Blazor Server**（InteractiveServerRendering）
- **PostgreSQL + EF Core 9 + Npgsql**（medock のみ）
- **SignalR, Serilog, Tailwind CSS + daisyUI**

---

## 前提条件

| ツール | 用途 |
|---|---|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | ビルド・実行 |
| [Task](https://taskfile.dev) | タスクランナー（`winget install Task.Task`） |
| PostgreSQL 18+ | medock 使用時 |

---

## ビルド・テスト

```bash
# ソリューション全体を Release ビルド
task build

# 全テストを実行
task test

# ビルド成果物を削除
task clean
```

---

## NuGet リリース

### ローカルフィードへの登録

```bash
task push:local
```

- `artifacts/local-feed/` にパッケージを登録します。
- 個別タスクとして実行した場合でも、事前に `pack` が自動実行されます。
- ローカルフィードを参照するには `dotnet nuget add source` で登録してください。

```bash
dotnet nuget add source "$(pwd)/artifacts/local-feed" --name aloe-local
```

### nuget.org への公開

**必須の環境変数**

| 変数名 | 説明 |
|---|---|
| `NUGET_API_KEY` | nuget.org の API キー（[取得方法](https://www.nuget.org/account/apikeys)） |

```bash
export NUGET_API_KEY="your-api-key-here"
task push:nuget
```

`NUGET_API_KEY` が未設定の場合はエラーで停止します（意図的）。

### 一括リリース（pack → ローカル → nuget.org）

```bash
export NUGET_API_KEY="your-api-key-here"
task release
```

`pack` → `push:local` → `push:nuget` の順に実行されます。

### パッケージのみ生成

```bash
task pack
# → artifacts/nuget/*.nupkg に 13本生成される
```

---

## アプリ Publish

成果物はすべて `artifacts/publish/` 以下に出力されます。

```bash
task publish:medock       # MedockServer → artifacts/publish/medock/
task publish:mcp          # MedockMcp (全6RID) → artifacts/publish/mcp/{rid}/
task publish:filebridge   # FileBridge → artifacts/publish/filebridge/
task publish:razorreport  # RazorReportServer → artifacts/publish/razorreport/
task publish:wsmonitor    # WindowsServiceMonitorServer → artifacts/publish/wsmonitor/
task publish:syncbootbridge   # SyncBootBridge (dotnet build) → artifacts/publish/syncbootbridge/

# 全アプリを並列 publish
task publish:all
```

`publish:mcp` は以下の 6 RID すべてに対して自己完結型バイナリを生成します：

```
win-x64 / win-arm64 / osx-arm64 / linux-x64 / linux-arm64 / linux-musl-x64
```

> **注意**: SyncBootBridge は .NET Framework 4.6.1 のため `dotnet publish` 非対応です。
> `dotnet build` によるビルドのみ行います。

---

## 成果物ディレクトリ

```
artifacts/
├── nuget/          .nupkg / .snupkg
├── local-feed/     ローカル NuGet フィード
└── publish/
    ├── medock/
    ├── mcp/
    │   ├── win-x64/
    │   ├── win-arm64/
    │   ├── osx-arm64/
    │   ├── linux-x64/
    │   ├── linux-arm64/
    │   └── linux-musl-x64/
    ├── filebridge/
    ├── razorreport/
    ├── wsmonitor/
    └── syncbootbridge/
```

---

## ライセンス

- **ユーティリティライブラリ**: MIT
- **アプリケーション**: 個人利用目的
