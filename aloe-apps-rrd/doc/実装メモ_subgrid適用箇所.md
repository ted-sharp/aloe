# subgrid適用箇所の実装メモ

## 実装状況

現在、デザイナーコンポーネントやコンテナコンポーネントはまだ実装されていません。
このドキュメントは、将来の実装時にsubgridを適用すべき箇所を記録したものです。

## subgrid適用箇所

### 1. コンテナコンポーネントのCSS

**ファイル**: `Components/Container.razor.css` または類似のCSSファイル

```css
.container-element {
  display: grid;
  grid-template-columns: subgrid;
  grid-template-rows: subgrid;
  /* コンテナの位置は親グリッドに対して設定される */
  grid-column: var(--container-column-start) / var(--container-column-end);
  grid-row: var(--container-row-start) / var(--container-row-end);
}
```

### 2. コンテナコンポーネントのRazor実装

**ファイル**: `Components/Container.razor` または類似のRazorファイル

コンテナコンポーネントでは、以下のようにsubgridを適用：

```razor
<div class="container-element" 
     style="grid-column: @(Position.Column) / span @(Position.ColumnSpan);
            grid-row: @(Position.Row) / span @(Position.RowSpan);
            @GetContainerStyles()">
  @foreach (var child in Children)
  {
    @* 子要素はコンテナ内での相対座標で配置 *@
    <DesignElement Element="@child" />
  }
</div>
```

### 3. デザイナーエリアのルートグリッド

**ファイル**: `Components/Designer.razor` または類似のデザイナーコンポーネント

ルートグリッドは36列×51行で定義：

```razor
<div class="designer-grid" 
     style="display: grid;
            grid-template-columns: repeat(36, 1fr);
            grid-template-rows: repeat(51, auto);">
  @foreach (var element in Elements)
  {
    @if (element.Type == "ContainerElement")
    {
      <Container Element="@element" />
    }
    else
    {
      <DesignElement Element="@element" />
    }
  }
</div>
```

### 4. JSONデータ構造

**ファイル**: モデルクラス（`Aloe.Apps.RazorReportLib`内）

コンテナ要素のJSON構造：

```json
{
  "type": "ContainerElement",
  "id": "container-1",
  "position": {
    "column": 2,
    "row": 2,
    "columnSpan": 5,
    "rowSpan": 3
  },
  "properties": {
    "backgroundColor": "#f0f0f0",
    "border": "1px solid #ccc",
    "padding": "10px"
  },
  "children": [
    {
      "type": "TextElement",
      "id": "text-1",
      "position": {
        "column": 1,  // コンテナ内での相対座標（1から始まる）
        "row": 1,
        "columnSpan": 2,
        "rowSpan": 1
      },
      "properties": { ... }
    }
  ]
}
```

### 5. 座標検証ロジック

**ファイル**: サービスクラス（`Aloe.Apps.RazorReportLib`内）

子要素の座標がコンテナのスパン範囲を超えないように検証：

```csharp
public bool ValidateChildPosition(ContainerElement container, DesignElement child)
{
    return child.Position.Column >= 1 &&
           child.Position.Column + child.Position.ColumnSpan - 1 <= container.Position.ColumnSpan &&
           child.Position.Row >= 1 &&
           child.Position.Row + child.Position.RowSpan - 1 <= container.Position.RowSpan;
}
```

## 実装時の注意事項

1. **ブラウザサポート**: Chrome 117+, Firefox 71+, Safari 16+でサポート（PlaywrightのEdgeでも対応済み）
2. **座標の範囲チェック**: 子要素の座標がコンテナのスパン範囲を超えないように検証が必要
3. **ネストされたコンテナ**: コンテナ内にコンテナを配置する場合も、同様にsubgridを使用可能
4. **相対座標**: コンテナ内の子要素は、コンテナ内での相対座標（1から始まる）で管理

## 参考資料

- [MDN: CSS Subgrid](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Grid_Layout/Subgrid)
- 仕様書: `doc/仕様書.md` - 3.2.1 グリッドシステム - コンテナとsubgrid
- 仕様書: `doc/仕様書_コンポーネントとパラメータ.md` - レイアウトコンポーネント
