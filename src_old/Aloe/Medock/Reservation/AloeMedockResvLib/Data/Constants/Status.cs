namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Constants;

/// <summary>
/// レコードステータス
/// </summary>
public enum Status
{
    /// <summary>
    /// 有効
    /// </summary>
    Active = 0,

    /// <summary>
    /// 非表示
    /// </summary>
    /// <remarks>
    /// 使わなくなったので非表示にしたい場合など。
    /// </remarks>
    Hidden = 80,

    /// <summary>
    /// 削除
    /// </summary>
    /// <remarks>
    /// 論理削除したい場合など。
    /// </remarks>
    Deleted = 90,

    /// <summary>
    /// パージ
    /// </summary>
    /// <remarks>
    /// 参照しているものがなく、物理的に消してもよい場合など。
    /// </remarks>
    Purged = 99,
}
