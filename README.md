# Aloe

## プロジェクト名について

Aloe は多肉植物の名前から。
植物の緑のカラー、アロエの健康的なイメージが医療とリンクする。
アロエのモチーフはロゴやアイコンに使いやすい。

Medock は Medical と人間ドックから。

## はじめ方

### データベースの準備

PostgreSQL の最新版をインストールする。
データディレクトリ(任意): `C:\postgres\pg_data`

PostgreSQL の bin のパスを通す。
`C:\Program Files\PostgreSQL\17\bin`

pgpass.conf をコピーする。
`COPY "[repo]\sql\pg_setup\pgpass.conf" "%APPDATA%\postgresql\"`
`[repo]` はこのリポジトリを指すものとする。

テーブル作成用スクリプトを実行する。
基本的にはダブルクリックでの実行を想定しています。

```cmd
> CD [repo]\sql\pg_setup\
> .\00_setup.bat
```

必要であれば PGTune で設定を作成するとよい。

ER図、DDL出力は `A5:SQL Mk-2` を使用する。

管理用クエリ集
`[repo]\sql\pg_setup\pg_queries.sql`

### 開発環境の準備

Visual Studio 2022 を推奨する。
WPF(.NET デスクトップ開発) と WEB API(ASP.NET と Web 開発) のワークロードが必要。
WinRT を使用している場合は Windows SDK xx(10.0.xxxxx.0) が必要。

拡張として SwitchStartupProject を入れる。
ReSharper があればなおよい。

`Seed` で起動して、サンプルデータを作成する。
`Svr + App` で起動すると、サーバーとクライアントが起動する。
`App(Standalone)` は、単体起動ができるのでデバッグしやすい。

パスワードやキーを管理したい場合は、ユーザー シークレットを使用する。
`appsettings.json` の接続文字列などの内容をシークレットで上書きできる。

コーディング標準
[C# CODING GUIDELINES 2024](https://qiita.com/Ted-HM/items/1d4ecdc2a252fe745871)

### WinRT を使用している場合



## 使用技術スタックについて

### システム・アーキテクチャ

#### 3層構造アーキテクチャ

クライアント - サーバー - データベース

クライアント: WPF
サーバー: ASP.NET Core API
データベース: PostgreSQL

クライアント-サーバー間: gRPC(MagicOnion)
サーバー-データベース間: EFCore(Npsql)

#### 2層構造アーキテクチャ

スタンドアローン起動の場合は、クライアントが直接DBアクセスを行う。
インターフェースとしてのgRPC定義は使用するが、通信は行わない。

### アプリケーション・アーキテクチャ

#### MVVM

UI（View）とロジック（ViewModel, Model）の明確な分離を目的としたUI設計パターン。

View: UI部分(Xaml)
ViewModel: 入力の検証や状態管理を担当、データを表示用に加工
Model: 上記以外

View に ViewModel をバインドするのに ReactiveProperty(R3) を使用している。

#### オニオンアーキテクチャ/クリーンアーキテクチャ

アプリケーションを玉ねぎ状に層に分け、最重要なドメイン（ビジネスロジック）を中心に置き、
外側ほど具体的な技術（DB, UIなど）を配置することで、技術変更を容易にする設計パターン。
DBアクセスなどは、ドメイン（中心）を技術依存（外側）から守るために、DIを使って依存関係を内側へ向ける。

(1. 中心)ドメイン層: エンティティ定義、ビジネスロジック
(2. サービス)アプリケーション層: ユースケース、ビジネスロジック
(3. インターフェース)インターフェース層: サービスインターフェース、DBアクセス
(4. UI)プレゼンテーション層: UI
(4. DB)インフラストラクチャ層: DB、公開API
