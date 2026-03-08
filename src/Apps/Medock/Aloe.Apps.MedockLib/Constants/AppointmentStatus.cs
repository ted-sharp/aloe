namespace Aloe.Apps.MedockLib.Constants;

/// <summary>
/// 予約ステータスの定義
/// </summary>
/// <remarks>
/// 予約の状態を表すEnumです。UIやAPIで使用される色分けと対応しています。
/// データベースとの互換性のため、intベースで定義されています。
/// </remarks>
public enum AppointmentStatus
{
    /// <summary>
    /// 予約済み（Reserved）- グレー (#9ca3af)
    /// </summary>
    Reserved = 0,

    /// <summary>
    /// 待機中（Waiting）- 青 (#60a5fa)
    /// </summary>
    Waiting = 1,

    /// <summary>
    /// 来院済み（Visited）- 緑 (#34d399)
    /// </summary>
    Visited = 2,

    /// <summary>
    /// キャンセル（Cancelled）- 赤 (#f87171)
    /// </summary>
    Cancelled = 3
}
