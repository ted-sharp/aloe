namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// コンポーネント設定のルートクラス
/// </summary>
public class ComponentsConfiguration
{
    /// <summary>
    /// デフォルトサイズのリスト
    /// </summary>
    public List<ComponentDefaultSize> DefaultSizes { get; set; } = new();

    /// <summary>
    /// 指定されたコンポーネントタイプのデフォルトサイズを取得する
    /// </summary>
    public ComponentDefaultSize? GetDefaultSize(string componentType)
    {
        return this.DefaultSizes.FirstOrDefault(s =>
            s.ComponentType.Equals(componentType, StringComparison.OrdinalIgnoreCase));
    }
}
