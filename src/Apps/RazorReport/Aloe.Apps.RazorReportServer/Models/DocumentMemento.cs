namespace Aloe.Apps.RazorReportServer.Models;

public class DocumentMemento
{
    public string SerializedDocument { get; set; } = String.Empty;
    public DateTime Timestamp { get; set; }

    public DocumentMemento(string serializedDocument)
    {
        this.SerializedDocument = serializedDocument;
        this.Timestamp = DateTime.UtcNow;
    }
}
