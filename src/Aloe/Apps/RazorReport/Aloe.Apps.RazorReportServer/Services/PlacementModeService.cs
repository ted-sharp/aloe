using System;

namespace Aloe.Apps.RazorReportServer.Services;

/// <summary>
/// コンポーネント配置モードの状態を管理するサービス
/// </summary>
public class PlacementModeService
{
    private string? _selectedComponentType;
    private bool _isPlacementModeActive;

    /// <summary>
    /// 配置モードが有効かどうか
    /// </summary>
    public bool IsPlacementModeActive => this._isPlacementModeActive;

    /// <summary>
    /// 選択されたコンポーネントタイプ
    /// </summary>
    public string? SelectedComponentType => this._selectedComponentType;

    /// <summary>
    /// 配置モードの状態が変更されたときに発生するイベント
    /// </summary>
    public event Action? PlacementModeChanged;

    /// <summary>
    /// コンポーネントを選択して配置モードを有効化
    /// </summary>
    /// <param name="componentType">コンポーネントタイプ（"Text", "Image", "Table", "RazorCodeBlock", "Container"など）</param>
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
}
