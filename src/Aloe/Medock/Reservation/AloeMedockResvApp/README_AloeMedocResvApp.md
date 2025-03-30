# Aloe Medock Resvation Application

クラサバ構成のクライアント側。

UIはMVVMで構成し、ViewとViewModelをアプリケーション側で管理している。
Modelはライブラリ側で管理している。

スタンドアローンモードで起動すると、サーバーを経由せずに直接DBアクセスする。

## 事前準備

リポジトリ直下の README.md に従ってデータベースと開発環境を準備します。

スタンドアローンモードで動かす場合は、ユーザーシークレットなどでDB接続文字列を変更します。

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Database=aloedb;Username=postgres;Password=postgres"
  }
}
```

## デバッグ方法

SwitchStartupProject で引数付きで起動します。

`App(Standalone)`: 単体起動ができるので開発中はこちらを使う。
`Svr + App`: サーバーとクライアントが起動する。
