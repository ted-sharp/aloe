# Aloe Medock

## プロジェクト名について

Aloe は多肉植物の名前から。
植物の緑のカラー、アロエの健康的なイメージが医療とリンクする。
アロエのモチーフはロゴやアイコンに使いやすい。

Medock は Medical と人間ドックから。
もしくは、Medical とドキュメントから。

## はじめ方

### データベースの準備

PostgreSQL 18 以降をインストールする。
データディレクトリ(任意): `C:\postgres\pg_data`

PostgreSQL の bin のパスを通す。
`C:\Program Files\PostgreSQL\17\bin`

pgpass.conf をコピーして書き換える。
`COPY "[repo]\sql\pg_setup\pgpass.conf" "%APPDATA%\postgresql\"`
`[repo]` はこのリポジトリを指すものとする。

初回のみ拡張を有効にする。

拡張有効化
`[repo]\sql\pg_setup\ext_create_extensions.sql`

テーブル作成用スクリプトを実行する。

```cmd
> CD [repo]\sql\pg_setup\
> .\00_setup.bat
```

必要であれば PGTune で設定を作成するとよい。

ER図、DDL出力は `A5:SQL Mk-2` を使用する。

管理用クエリ集
`[repo]\sql\pg_setup\pg_queries.sql`

### 開発環境の準備

Visual Studio 2026 を推奨する。
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
[C# CODING GUIDELINES 2025](https://qiita.com/Ted-HM/items/1d4ecdc2a252fe745871)

### WinRT を使用している場合

## 使用技術スタックについて

Blazor Server
MVVM / オニオンアーキテクチャ
Page.razor は View で @value のバインドをゆるく使う。
Page.razor.cs は ViewModel で表示に関わるものを保持する。
それ以外のロジックなどは Model として分離する。
画面のデザインやレイアウトに関しては、HTML/CSSベースとし、Blazorコンポーネントは最低限とする。
Tailwind CSS / daisyUI, FlyonUI, Flowbite, PrelineUI あたりを想定している。

Magic Onion (gRPC/SignalR) / REST API
通信は基本的には Blazor で行うが、それ以外の部分に関しては gRPC や REST を使用する。
gRPC / REST に関してはステートレスで組む。

EFCore / Npgsql
DB は基本的に PostgreSQL 専用とし、事前にセットアップしておくものとする。

## 画面構成

### ログイン画面
お知らせや重いファイルのプリロードを行っておく。

### 予約管理画面
ログイン後に表示しておく画面。
メニューや大枠は Blazor で表示するが、メインのカレンダー部分は Canvas で描画する。
D3.js で計算を行い、Konva.js でカレンダーを描画したい。
SignalR Hub で他のログイン者がどこを参照しているかリアルタイムに反映したい。

#### 年間カレンダー/月間カレンダー
1日は左右2分割の円グラフを表示し、AM/PMの予約状況が一目でわかるようにする。

#### 週間スケジューラー(31日/14日/7日/3日/1日)
1日は、縦に時間軸が並び、スロットによる表示と詳細な予約者の表示を切り替えられる。
横軸は日付で、1日～31日まで切り替えられる。




