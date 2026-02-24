namespace Aloe.Apps.RazorReportServer.Models;

public class DocumentMemento
{
    public string SerializedDocument { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public DocumentMemento(string serializedDocument)
    {
        SerializedDocument = serializedDocument;
        Timestamp = DateTime.UtcNow;
    }
}
