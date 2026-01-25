using System.Text.Json.Serialization;

namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// デザイン要素の抽象基底クラス
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(TextElement), "text")]
public abstract class DesignElement
{
    /// <summary>
    /// 要素の一意識別子
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// グリッド上の位置
    /// </summary>
    public GridPosition Position { get; set; } = new(0, 0);

    /// <summary>
    /// 要素のタイプ名
    /// </summary>
    public abstract string ElementType { get; }
}
