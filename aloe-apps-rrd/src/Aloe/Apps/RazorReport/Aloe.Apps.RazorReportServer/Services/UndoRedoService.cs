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

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public UndoRedoService(DocumentService documentService)
    {
        _documentService = documentService;
    }

    public void SaveState(ReportDocument document)
    {
        try
        {
            var json = _documentService.SerializeDocument(document);
            var memento = new DocumentMemento(json);

            _undoStack.Push(memento);

            // Clear redo stack when new action is performed
            _redoStack.Clear();

            // Limit history size
            if (_undoStack.Count > MaxHistorySize)
            {
                var temp = new Stack<DocumentMemento>(_undoStack.Count - 1);
                for (int i = 0; i < _undoStack.Count - 1; i++)
                {
                    temp.Push(_undoStack.Pop());
                }
                _undoStack.Clear();
                foreach (var item in temp)
                {
                    _undoStack.Push(item);
                }
            }

            NotifyHistoryChanged();
        }
        catch { }
    }

    public ReportDocument? Undo(ReportDocument currentDocument)
    {
        if (!CanUndo)
            return null;

        try
        {
            var memento = _undoStack.Pop();
            var currentJson = _documentService.SerializeDocument(currentDocument);
            _redoStack.Push(new DocumentMemento(currentJson));

            var document = _documentService.DeserializeDocument(memento.SerializedDocument);
            NotifyHistoryChanged();
            return document;
        }
        catch
        {
            return null;
        }
    }

    public ReportDocument? Redo(ReportDocument currentDocument)
    {
        if (!CanRedo)
            return null;

        try
        {
            var memento = _redoStack.Pop();
            var currentJson = _documentService.SerializeDocument(currentDocument);
            _undoStack.Push(new DocumentMemento(currentJson));

            var document = _documentService.DeserializeDocument(memento.SerializedDocument);
            NotifyHistoryChanged();
            return document;
        }
        catch
        {
            return null;
        }
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        NotifyHistoryChanged();
    }

    private void NotifyHistoryChanged()
    {
        OnHistoryChanged?.Invoke();
    }
}
