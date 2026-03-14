# Aloe.Libs.PicoBlazor

Blazor Server アプリ向けの Pico CSS 共有アセットライブラリ。

## 概要

Pico CSS・Lucide Icons・Animate.css などのフロントエンドアセットと、Blazor 共通コンポーネントをまとめた RCL（Razor Class Library）。WindowsServiceMonitor・Dashboard など複数の Blazor Server アプリが参照する。

## 内包アセット

| パス | 内容 |
|---|---|
| `wwwroot/lib/pico-css/css/pico.min.css` | **Pico CSS v2.1.1** |
| `wwwroot/lib/lucide/lucide.min.js` | Lucide アイコンライブラリ |
| `wwwroot/lib/animate-css/animate.min.css` | Animate.css |
| `wwwroot/pico-blazor.css` | Blazor + Pico CSS 統合用カスタムスタイル |

## コンポーネント

### `<ReconnectModal />`

Blazor Server の接続切断時に表示される再接続モーダル。`<dialog>` 要素ベースで、接続状態に応じて表示内容が切り替わる（再接続中 / 再試行中 / 失敗 / 一時停止）。

## 使い方

### 1. プロジェクト参照を追加

```xml
<ProjectReference Include="..\..\Libs\Aloe.Libs.PicoBlazor\Aloe.Libs.PicoBlazor.csproj" />
```

### 2. `App.razor` にアセットを追加

```razor
<html lang="ja" data-theme="light">
<head>
    <link rel="stylesheet" href="_content/Aloe.Libs.PicoBlazor/lib/pico-css/css/pico.min.css" />
    <link rel="stylesheet" href="_content/Aloe.Libs.PicoBlazor/pico-blazor.css" />
    <script src="_content/Aloe.Libs.PicoBlazor/lib/lucide/lucide.min.js"></script>
</head>
<body>
    <Routes />
    <ReconnectModal />
    <script src="@Assets["_framework/blazor.web.js"]"></script>
    <script>
        document.addEventListener('DOMContentLoaded', () => lucide.createIcons());
        Blazor.addEventListener('enhancedload', () => lucide.createIcons());
    </script>
</body>
```

> **注意**: Lucide アイコンは Blazor の Enhanced Navigation でページ遷移後に再初期化が必要なため、`enhancedload` イベントでも `lucide.createIcons()` を呼ぶこと。

### 3. `_Imports.razor` に using を追加

```razor
@using Aloe.Libs.PicoBlazor.Components
```

## Pico CSS v2 についての注意

このライブラリは **Pico CSS v2**（v2.1.1）を使用しています。v1 とは互換性のない変更が含まれます。

### v1 からの主な変更点

| 項目 | v1 | v2 |
|---|---|---|
| CSS 変数のプレフィックス | `--color-*` など独自 | `--pico-*` 統一 |
| テーマ切替 | `data-theme="dark"` | 同じ（`data-theme="light"` / `"dark"`） |
| クラスレスモード | デフォルト | デフォルト（`pico.min.css`） |
| クラスベースモード | なし | `pico.classless.min.css` で切替可能 |

### カスタムスタイル (`pico-blazor.css`) の内容

- **カラー変数**: success / danger / warning / delete の 4 色を `--color-*` で定義
- **Blazor バリデーション**: `.valid` / `.invalid` / `.validation-message` のスタイル
- **Blazor エラー境界**: `.blazor-error-boundary`、`#blazor-error-ui` のスタイル
- **ボタンカラーバリアント**: IEC 60073 準拠（`button.success` / `button.danger` / `button.warning` / `button.delete`）
- **アイコンボタン**: `button.icon-button`（Lucide アイコン用）、`button.icon-only`
- **ステータスバッジ**: `.status-badge-running` / `-stopped` / `-paused` / `-transition` / `-unknown`
- **テーブル**: `.table-overflow`（横スクロール対応）
