namespace Aloe.Apps.RazorReportLib.Models;

/// <summary>
/// テーブルコンポーネント
/// </summary>
public class TableElement : DesignElement
{
    public TableElement()
    {
        this.Properties["rows"] = 1;
        this.Properties["columns"] = 1;
        this.Properties["cells"] = new List<List<string>>();
    }

    public override string ElementType => "Table";
}
