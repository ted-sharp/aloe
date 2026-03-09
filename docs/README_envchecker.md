# Aloe.Apps.EnvChecker

環境チェッカー。アプリケーションのトラブルシューティング時に、OS・ハードウェア・ネットワーク・ランタイム等の環境情報を収集するコンソールツール。

## 使い方

### 全チェック実行

```powershell
env-checker.exe
```

### ファイルに出力

```powershell
env-checker.exe > report.txt 2>&1
```

### セクション指定（--only / --exclude）

```powershell
# システム情報、ディスク、メモリ、ポートのみ
env-checker.exe --only system,disk,memory,port

# イベントログと証明書を除外
env-checker.exe --exclude eventlog,cert
```

### JSON プロファイルで詳細制御

```powershell
# サンプルプロファイルを生成
env-checker.exe --init > mycheck.json

# プロファイルを指定して実行
env-checker.exe --profile mycheck.json
```

## セクション一覧

| キー | 内容 |
|------|------|
| `system` | OS、コンピュータ名、アーキテクチャ、稼働時間、タイムゾーン、ユーザー、管理者権限 |
| `cpu` | CPU モデル、コア数、クロック速度 |
| `memory` | 物理メモリ合計・使用可能・使用率 |
| `disk` | ドライブ一覧、容量、使用率（閾値警告付き） |
| `dotnet` | 現在のランタイム、インストール済みランタイム・SDK |
| `vcruntime` | Visual C++ Redistributable のインストール状況 |
| `network` | ネットワークアダプタ、DNS、ゲートウェイ、DNS解決テスト、Ping テスト |
| `port` | 指定ポートのリスニング状態確認 |
| `env` | 指定した環境変数の値（PATH エントリ展開対応） |
| `firewall` | Windows Firewall の各プロファイル状態 |
| `service` | 指定した Windows サービスの状態 |
| `software` | 指定コマンドの存在確認とバージョン |
| `eventlog` | Windows イベントログの直近エラー |
| `cert` | LocalMachine 証明書ストアの有効期限チェック |

## プロファイル例

```json
{
  "system": { "enabled": true },
  "cpu": { "enabled": true },
  "memory": { "enabled": true },
  "disk": {
    "enabled": true,
    "warningThresholdPercent": 90
  },
  "dotnet": { "enabled": true },
  "vcruntime": { "enabled": true },
  "network": {
    "enabled": true,
    "dnsTestHost": "www.google.com",
    "pingTestHost": "8.8.8.8"
  },
  "port": {
    "enabled": true,
    "ports": [80, 443, 5432, 5000, 5001]
  },
  "env": {
    "enabled": true,
    "variables": ["PATH", "DOTNET_ROOT", "ASPNETCORE_ENVIRONMENT", "PGDATA", "PGHOST", "PGPORT", "PGUSER"],
    "showPathEntries": true,
    "hideIfNotSet": true
  },
  "firewall": { "enabled": true },
  "service": {
    "enabled": true,
    "services": ["postgresql-x64-16", "W3SVC", "wuauserv", "W32Time"]
  },
  "software": {
    "enabled": true,
    "commands": ["dotnet", "git", "node", "npm", "psql", "docker", "python"]
  },
  "eventlog": {
    "enabled": true,
    "logNames": ["Application", "System"],
    "hours": 24,
    "maxEntries": 5
  },
  "cert": {
    "enabled": true,
    "warningDays": 30
  }
}
```

### プロファイルの設計

- セクションの `enabled` が `true` で有効、`false` で無効
- `--init` で全セクション有効のサンプルを生成できる
- プロファイル内のリスト（ports, variables, services, commands）で項目レベルの絞り込みが可能
- `env.hideIfNotSet: true` で未設定の環境変数を非表示

### 優先順位

| 条件 | 動作 |
|------|------|
| `--profile` 指定 | プロファイルの設定に従う |
| `--only` 指定 | 指定セクションのみデフォルト設定で実行 |
| `--exclude` 指定 | 全セクションから除外分を引く |
| 引数なし | 全セクションをデフォルト設定で実行 |

## ビルド・パブリッシュ

```powershell
# ビルド
dotnet build src/Apps/EnvChecker/Aloe.Apps.EnvChecker -c Release

# Self-contained パブリッシュ（ターゲットマシンに .NET 不要）
dotnet publish src/Apps/EnvChecker/Aloe.Apps.EnvChecker -c Release -r win-x64 --self-contained true -o artifacts/publish/envchecker

# Task で実行
task publish:envchecker
```
