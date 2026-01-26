using System;

namespace Aloe.Apps.RazorReportServer.Services;

/// <summary>
/// コンポーネント配置モードの状態を管理するサービス
/// </summary>
public class PlacementModeService
{
    private string? _selectedComponentType;
    private bool _isPlacementModeActive;
    private bool _isDraggingFromPalette;
    private (double x, double y)? _dragStartPosition;

    /// <summary>
    /// 配置モードが有効かどうか
    /// </summary>
    public bool IsPlacementModeActive => this._isPlacementModeActive;

    /// <summary>
    /// 選択されたコンポーネントタイプ
    /// </summary>
    public string? SelectedComponentType => this._selectedComponentType;

    /// <summary>
    /// パレットからドラッグ中かどうか
    /// </summary>
    public bool IsDraggingFromPalette => this._isDraggingFromPalette;

    /// <summary>
    /// ドラッグ開始位置
    /// </summary>
    public (double x, double y)? DragStartPosition => this._dragStartPosition;

    /// <summary>
    /// 配置モードの状態が変更されたときに発生するイベント
    /// </summary>
    public event Action? PlacementModeChanged;

    /// <summary>
    /// コンポーネントを選択して配置モードを有効化
    /// </summary>
    /// <param name="componentType">コンポーネントタイプ（"Text", "Image", "Table", "Container"など）</param>
    public void ActivatePlacementMode(string componentType)
    {
        this._selectedComponentType = componentType;
        this._isPlacementModeActive = true;
        PlacementModeChanged?.Invoke();
    }

    /// <summary>
    /// 配置モードを解除
    /// </summary>
    public void CancelPlacementMode()
    {
        this._selectedComponentType = null;
        this._isPlacementModeActive = false;
        PlacementModeChanged?.Invoke();
    }

    /// <summary>
    /// 配置を完了して配置モードを解除
    /// </summary>
    public void CompletePlacement()
    {
        this._selectedComponentType = null;
        this._isPlacementModeActive = false;
        PlacementModeChanged?.Invoke();
    }

    /// <summary>
    /// パレットからのドラッグを開始する
    /// </summary>
    public void StartDragFromPalette(string componentType, double x, double y)
    {
        this._selectedComponentType = componentType;
        this._isDraggingFromPalette = true;
        this._dragStartPosition = (x, y);
    }

    /// <summary>
    /// パレットからのドラッグを完了する
    /// </summary>
    public void CompleteDragFromPalette()
    {
        this._selectedComponentType = null;
        this._isDraggingFromPalette = false;
        this._dragStartPosition = null;
        PlacementModeChanged?.Invoke();
    }
}
