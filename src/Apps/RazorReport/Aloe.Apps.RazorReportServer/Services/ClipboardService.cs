using Aloe.Apps.RazorReportLib.Models;
using System.Text.Json;

namespace Aloe.Apps.RazorReportServer.Services;

public class ClipboardService
{
    private List<DesignElement> _clipboardData = new();
    private readonly DocumentService _documentService;

    public event Action? OnClipboardChanged;

    public bool HasData => this._clipboardData.Count > 0;
    public int DataCount => this._clipboardData.Count;

    public ClipboardService(DocumentService documentService)
    {
        this._documentService = documentService;
    }

    public void Copy(List<DesignElement> elements)
    {
        try
        {
            this._clipboardData = this.DeepCloneElements(elements);
            this.NotifyClipboardChanged();
        }
        catch { }
    }

    public void Cut(List<DesignElement> elements)
    {
        this.Copy(elements);
    }

    public List<DesignElement> Paste()
    {
        if (!this.HasData)
            return new();

        try
        {
            var pasted = this.DeepCloneElements(this._clipboardData);

            // Assign new IDs and offset positions
            foreach (var element in pasted)
            {
                element.Id = Guid.NewGuid();

                // Offset position by 2 grid cells
                element.Position = new GridPosition(
                    Math.Max(0, element.Position.Column + 2),
                    Math.Max(0, element.Position.Row + 2),
                    element.Position.ColumnSpan,
                    element.Position.RowSpan
                );
            }

            return pasted;
        }
        catch
        {
            return new();
        }
    }

    public void Clear()
    {
        this._clipboardData.Clear();
        this.NotifyClipboardChanged();
    }

    private List<DesignElement> DeepCloneElements(List<DesignElement> elements)
    {
        try
        {
            var options = this._documentService.GetSerializerOptions();
            var json = JsonSerializer.Serialize(elements, options);
            var cloned = JsonSerializer.Deserialize<List<DesignElement>>(json, options);
            return cloned ?? new();
        }
        catch
        {
            return new();
        }
    }

    private void NotifyClipboardChanged()
    {
        OnClipboardChanged?.Invoke();
    }
}
