using Aloe.Apps.RazorReportLib.Models;

namespace Aloe.Apps.RazorReportServer.Services;

/// <summary>
/// レイアウトデータを管理するサービス
/// </summary>
public class LayoutService
{
    private LayoutData _layoutData = new();

    /// <summary>
    /// 現在のレイアウトデータ
    /// </summary>
    public LayoutData CurrentLayout => this._layoutData;

    /// <summary>
    /// 要素が追加されたときに発生するイベント
    /// </summary>
    public event Action<DesignElement>? ElementAdded;

    /// <summary>
    /// レイアウトデータが変更されたときに発生するイベント
    /// </summary>
    public event Action? LayoutChanged;

    /// <summary>
    /// コンポーネントを追加
    /// </summary>
    public void AddElement(DesignElement element)
    {
        this._layoutData.Elements.Add(element);
        ElementAdded?.Invoke(element);
        LayoutChanged?.Invoke();
    }

    /// <summary>
    /// コンポーネントを作成して追加
    /// </summary>
    public DesignElement CreateAndAddElement(string componentType, int column, int row, int columnSpan, int rowSpan)
    {
        DesignElement element = componentType switch
        {
            "Text" => new TextElement(),
            "Image" => new ImageElement(),
            "Table" => new TableElement(),
            "Container" => new ContainerElement(),
            _ => throw new ArgumentException($"Unknown component type: {componentType}")
        };

        element.Position = new GridPosition(column, row, columnSpan, rowSpan);

        this.AddElement(element);
        return element;
    }

    /// <summary>
    /// 要素を削除
    /// </summary>
    public void RemoveElement(Guid elementId)
    {
        var element = this._layoutData.Elements.FirstOrDefault(e => e.Id == elementId);
        if (element != null)
        {
            this._layoutData.Elements.Remove(element);
            LayoutChanged?.Invoke();
        }
    }

    /// <summary>
    /// 要素を取得
    /// </summary>
    public DesignElement? GetElement(Guid elementId)
    {
        return this._layoutData.Elements.FirstOrDefault(e => e.Id == elementId);
    }
}
