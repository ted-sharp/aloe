using Aloe.Apps.RazorReportServer.Models;
using Aloe.Apps.RazorReportLib.Models;

namespace Aloe.Apps.RazorReportServer.Services;

public class UndoRedoService
{
    private const int MaxHistorySize = 50;
    private readonly Stack<DocumentMemento> _undoStack = new();
    private readonly Stack<DocumentMemento> _redoStack = new();
    private readonly DocumentService _documentService;

    public event Action? OnHistoryChanged;

    public bool CanUndo => this._undoStack.Count > 0;
    public bool CanRedo => this._redoStack.Count > 0;

    public UndoRedoService(DocumentService documentService)
    {
        this._documentService = documentService;
    }

    public void SaveState(ReportDocument document)
    {
        try
        {
            var json = this._documentService.SerializeDocument(document);
            var memento = new DocumentMemento(json);

            this._undoStack.Push(memento);

            // Clear redo stack when new action is performed
            this._redoStack.Clear();

            // Limit history size
            if (this._undoStack.Count > MaxHistorySize)
            {
                var temp = new Stack<DocumentMemento>(this._undoStack.Count - 1);
                for (int i = 0; i < this._undoStack.Count - 1; i++)
                {
                    temp.Push(this._undoStack.Pop());
                }
                this._undoStack.Clear();
                foreach (var item in temp)
                {
                    this._undoStack.Push(item);
                }
            }

            this.NotifyHistoryChanged();
        }
        catch { }
    }

    public ReportDocument? Undo(ReportDocument currentDocument)
    {
        if (!this.CanUndo)
            return null;

        try
        {
            var memento = this._undoStack.Pop();
            var currentJson = this._documentService.SerializeDocument(currentDocument);
            this._redoStack.Push(new DocumentMemento(currentJson));

            var document = this._documentService.DeserializeDocument(memento.SerializedDocument);
            this.NotifyHistoryChanged();
            return document;
        }
        catch
        {
            return null;
        }
    }

    public ReportDocument? Redo(ReportDocument currentDocument)
    {
        if (!this.CanRedo)
            return null;

        try
        {
            var memento = this._redoStack.Pop();
            var currentJson = this._documentService.SerializeDocument(currentDocument);
            this._undoStack.Push(new DocumentMemento(currentJson));

            var document = this._documentService.DeserializeDocument(memento.SerializedDocument);
            this.NotifyHistoryChanged();
            return document;
        }
        catch
        {
            return null;
        }
    }

    public void Clear()
    {
        this._undoStack.Clear();
        this._redoStack.Clear();
        this.NotifyHistoryChanged();
    }

    private void NotifyHistoryChanged()
    {
        OnHistoryChanged?.Invoke();
    }
}
