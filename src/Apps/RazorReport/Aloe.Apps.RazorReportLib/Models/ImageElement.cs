namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// 画像コンポーネント
/// </summary>
public class ImageElement : DesignElement
{
    public ImageElement()
    {
        this.Properties["imagePath"] = "";
        this.Properties["sampleImagePath"] = "";
        this.Properties["maintainAspectRatio"] = true;
    }

    public override string ElementType => "Image";
}
