using Aloe.Apps.RazorReportLib.Models;

namespace Aloe.Apps.RazorReportServer.Services;

/// <summary>
/// UI選択状態を管理するサービス
/// </summary>
public class SelectionService
{
    private HashSet<Guid> _selectedElementIds = new();

    /// <summary>
    /// 選択されている要素IDの読み取り専用コレクション
    /// </summary>
    public IReadOnlySet<Guid> SelectedElementIds => _selectedElementIds;

    /// <summary>
    /// 選択状態が変更された時のイベント
    /// </summary>
    public event Action? OnSelectionChanged;

    /// <summary>
    /// 要素を選択する
    /// </summary>
    public void SelectElement(Guid elementId, bool multiSelect = false)
    {
        if (!multiSelect)
            _selectedElementIds.Clear();

        _selectedElementIds.Add(elementId);
        OnSelectionChanged?.Invoke();
    }

    /// <summary>
    /// 要素の選択状態をトグルする
    /// </summary>
    public void ToggleSelection(Guid elementId)
    {
        if (_selectedElementIds.Contains(elementId))
            _selectedElementIds.Remove(elementId);
        else
            _selectedElementIds.Add(elementId);

        OnSelectionChanged?.Invoke();
    }

    /// <summary>
    /// すべての選択をクリアする
    /// </summary>
    public void ClearSelection()
    {
        if (_selectedElementIds.Count > 0)
        {
            _selectedElementIds.Clear();
            OnSelectionChanged?.Invoke();
        }
    }

    /// <summary>
    /// 要素が選択されているかどうかを判定する
    /// </summary>
    public bool IsSelected(Guid elementId) => _selectedElementIds.Contains(elementId);

    /// <summary>
    /// ドラッグ中かどうか
    /// </summary>
    public bool IsDragging { get; set; }

    /// <summary>
    /// ドラッグ開始時のグリッド位置
    /// </summary>
    public GridPosition? DragStartPosition { get; set; }

    /// <summary>
    /// サイズ変更中かどうか
    /// </summary>
    public bool IsResizing { get; set; }

    /// <summary>
    /// アクティブなリサイズハンドル
    /// </summary>
    public ResizeHandle? ActiveResizeHandle { get; set; }
}

/// <summary>
/// リサイズハンドルの位置
/// </summary>
public enum ResizeHandle
{
    TopLeft,
    Top,
    TopRight,
    Left,
    Right,
    BottomLeft,
    Bottom,
    BottomRight
}
