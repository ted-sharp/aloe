namespace Aloe.Apps.MedockLib.Constants;

/// <summary>
/// 予約ステータスコードの定義
/// </summary>
/// <remarks>
/// 予約の状態を表すコード値を定義します。UIやAPIで使用される色分けと対応しています。
/// </remarks>
public static class AppointmentStatusCode
{
    /// <summary>
    /// 予約済み（Reserved）- グレー (#9ca3af）
    /// </summary>
    public const int Reserved = 0;

    /// <summary>
    /// 待機中（Waiting）- 青 (#60a5fa)
    /// </summary>
    public const int Waiting = 1;

    /// <summary>
    /// 来院済み（Visited）- 緑 (#34d399)
    /// </summary>
    public const int Visited = 2;

    /// <summary>
    /// キャンセル（Cancelled）- 赤 (#f87171)
    /// </summary>
    public const int Cancelled = 3;

    /// <summary>
    /// すべての有効なステータスコード
    /// </summary>
    public static readonly int[] AllCodes = { Reserved, Waiting, Visited, Cancelled };

    /// <summary>
    /// ステータスコード名を取得します（デバッグ・ログ用）
    /// </summary>
    /// <param name="code">ステータスコード</param>
    /// <returns>ステータス名</returns>
    public static string GetName(int code) => code switch
    {
        Reserved => "Reserved",
        Waiting => "Waiting",
        Visited => "Visited",
        Cancelled => "Cancelled",
        _ => "Unknown"
    };

    /// <summary>
    /// ステータスコードが有効かどうか判定します。
    /// </summary>
    /// <param name="code">ステータスコード</param>
    /// <returns>有効な場合true、無効な場合false</returns>
    public static bool IsValid(int code) => AllCodes.Contains(code);
}
