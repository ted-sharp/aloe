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

`MedockSeed` を起動して、サンプルデータを作成する。
`MedockServer` を起動してブラウザでアクセスする。

パスワードやキーを管理したい場合は、ユーザー シークレットを使用する。
`appsettings.json` の接続文字列などの内容をシークレットで上書きできる。

コーディング標準
[C# CODING GUIDELINES 2025](https://qiita.com/Ted-HM/items/1d4ecdc2a252fe745871)

### WinRT を使用している場合

## 使用技術スタックについて
.NET10 / C#14 を想定している。

Blazor Server
MVVM / オニオンアーキテクチャ
Page.razor は View で @value のバインドをゆるく使う。
Page.razor.cs は ViewModel で表示に関わるものを保持する。
それ以外のロジックなどは Model として分離する。

Magic Onion (gRPC/SignalR) / REST API
通信は基本的には Blazor で行うが、それ以外の部分に関しては gRPC や REST を使用する。
gRPC / REST に関してはステートレスで組む。

EFCore / Npgsql
DB は基本的に PostgreSQL 18 以降とし、事前にセットアップしておくものとする。

### テストファースト
構造とテストを先に考えて、その後実装を進めていきたい。
AI実装するときは、テストの定義だけを置いておき、失敗しているものから順に実装していきたい。
マニュアル実装するときはテストを失敗させてから小さな成功を積み重ねるTDD方式としたい。

### 認証
基本的にはユーザーとパスワードとします。
パスキーにも対応したい。

JWTでベアラートークンを使用した独自認証とします。
Issue, Refresh, Revokeできる。
JWTトークンの内容はOIDCに寄せてください。

### 閉域でインターネットに接続できない場合
WebView2 をWPFアプリなどで配布する想定。

## サーバー

### Seed
基礎データを挿入するためのコンソールプロジェクト。
パスワードハッシュなどはライブラリの機能であらかじめ作っておきたい。
またある程度デモで動かせるようなサンプルデータを最初から用意しておきたい。

### Medock 本体
Blazor Server と、APIを提供するサーバー。
DBとも接続する。

## 画面構成

### デザインとレイアウトについて
画面のデザインやレイアウトに関しては、HTML/CSSベースとし、Blazorコンポーネントは最低限とする。
Tailwind CSS / daisyUI を想定している。
不足していれば FlyonUI, Flowbite, PrelineUI あたりから追加予定。

#### CSSフレームワークのセットアップ（CDN利用・Node.js不要）
開発段階では CDN を使用して Tailwind CSS と daisyUI を導入する。
Node.js / npm は使用しない方針とする。

```html
<!-- App.razor の <head> に追加 -->
<script src="https://cdn.tailwindcss.com"></script>
<link href="https://cdn.jsdelivr.net/npm/daisyui@5/dist/full.min.css" rel="stylesheet" />
```

本番環境への移行時は、LibMan または手動でビルド済みCSSを配置する。

#### CSSコンテナクエリによるレスポンシブ対応
`@media` クエリではなく `@container` クエリを使用してレスポンシブ対応を行う。
ビューポート幅ではなく、コンポーネントの親コンテナ幅に基づいてレイアウトを切り替える。

```css
/* コンテナ要素の定義 */
.responsive-container {
  container-type: inline-size;
}

/* コンテナクエリによるスタイル切り替え */
@container (min-width: 768px) {
  .sidebar { display: block; }
}
@container (max-width: 767px) {
  .sidebar { display: none; }
}
```

業務アプリなので、サイドバーのナビゲーションをメインに考える。
スマホで見る場合はサイドドロワーとし、コンテナクエリで切り替える。

### ログイン画面
ログイン前に重いファイルのプリロードを行っておく。
お知らせを表示してロード時間を稼ぐ。
「RememberMe」でログイン情報を覚えておく。
「セッションを維持」で前のセッションが生きていればログインをスキップできる。
基本的にはユーザーとパスワードでログインする方式。

### テナント選択画面
基本的には1ユーザー1テナントなので、ログインしたら自動的にテナントが決まるので、この画面は表示されない。
システム管理者は複数テナントを可能とし、複数テナントがあったらログイン後に選択画面を表示したい。
複数テナントに結び付いていないシステム管理者の場合は自動的にテナントが決まる。

### 予約管理画面
ログイン後に表示しておく画面。
メニューや大枠は Blazor で表示するが、メインのカレンダー部分は Canvas で描画する。
D3.js で計算を行い、Konva.js でカレンダーを描画したい。
SignalR Hub で他のログイン者がどこを参照しているかリアルタイムに反映したい。
変更履歴はEXCEL365のようにすべて記録しておく想定。

#### 年間カレンダー/月間カレンダー
1日は左右2分割の円グラフを表示し、AM/PMの予約状況が一目でわかるようにする。

#### 週間スケジューラー(31日/14日/7日/3日/1日)
1日は、縦に時間軸が並び、スロットによる表示と詳細な予約者の表示を切り替えられる。
横軸は日付で、1日～31日まで切り替えられる。

### その他の画面(まだ不要)

#### マスタ系
テナント管理
施設管理、公開管理
ユーザー管理、ユーザーロール管理

#### 業務系
画面管理
職務管理
アクセス権限

団体管理
患者管理
患者連携、ファイル取り込み、ソケット通信
契約管理、健保組合、差分管理
書類管理、差分管理

アクセスログ、変更ログ

### 外部ビューワー(まだ不要)
将来的な想定だが、画像などを閲覧する場合は外部ビューワーを起動する。
レジストリにプロトコルを登録しておき、独自スキーマで起動できるようにする。
インストーラーが必要。
