using Aloe.Apps.RazorReportLib.Models;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.RazorReportServer.Services;

/// <summary>
/// コンポーネント生成を行うサービス
/// </summary>
public class ComponentFactoryService
{
    private readonly IOptions<ComponentsConfiguration> _componentsConfig;

    public ComponentFactoryService(IOptions<ComponentsConfiguration> componentsConfig)
    {
        this._componentsConfig = componentsConfig;
    }

    /// <summary>
    /// 指定された位置と設定でコンポーネントを生成する
    /// </summary>
    public DesignElement CreateComponent(string componentType, GridPosition position)
    {
        var element = this.CreateElementByType(componentType);
        element.Position = position;
        this.ApplyDefaultProperties(element, componentType);
        return element;
    }

    /// <summary>
    /// デフォルトサイズでコンポーネントを生成する
    /// </summary>
    public DesignElement CreateComponentWithDefaultSize(string componentType, int column, int row)
    {
        var defaultSize = this._componentsConfig.Value.GetDefaultSize(componentType);
        var columnSpan = defaultSize?.ColumnSpan ?? 6;
        var rowSpan = defaultSize?.RowSpan ?? 1;

        var position = new GridPosition(column, row, columnSpan, rowSpan);
        return this.CreateComponent(componentType, position);
    }

    /// <summary>
    /// コンポーネントタイプに基づいて要素インスタンスを生成する
    /// </summary>
    private DesignElement CreateElementByType(string componentType)
    {
        return componentType.ToLower() switch
        {
            "text" => new TextElement(),
            "image" => new ImageElement(),
            "table" => new TableElement(),
            "container" => new ContainerElement(),
            _ => throw new ArgumentException($"Unknown component type: {componentType}")
        };
    }

    /// <summary>
    /// コンポーネントにデフォルトプロパティを適用する
    /// </summary>
    private void ApplyDefaultProperties(DesignElement element, string componentType)
    {
        var defaultSize = this._componentsConfig.Value.GetDefaultSize(componentType);
        if (defaultSize == null)
            return;

        // テキスト要素の場合
        if (element is TextElement textElement && !String.IsNullOrEmpty(defaultSize.DefaultText))
        {
            textElement.Text = defaultSize.DefaultText;
        }

        // テーブル要素の場合
        if (element is TableElement tableElement)
        {
            var rows = defaultSize.DefaultRows ?? 2;
            var columns = defaultSize.DefaultColumns ?? 3;

            tableElement.Properties["rows"] = rows;
            tableElement.Properties["columns"] = columns;

            // セルの初期データを生成
            var cells = new List<List<string>>();
            for (int r = 0; r < rows; r++)
            {
                var row = new List<string>();
                for (int c = 0; c < columns; c++)
                {
                    row.Add(String.Empty);
                }
                cells.Add(row);
            }
            tableElement.Properties["cells"] = cells;
        }
    }
}
