namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// コンテナコンポーネント
/// </summary>
public class ContainerElement : DesignElement
{
    public ContainerElement()
    {
        this.Properties["backgroundColor"] = "transparent";
        this.Properties["borderColor"] = "transparent";
        this.Properties["borderWidth"] = 0;
        this.Properties["padding"] = 0;
    }

    public override string ElementType => "Container";

    public List<DesignElement> Children { get; set; } = new();
}
