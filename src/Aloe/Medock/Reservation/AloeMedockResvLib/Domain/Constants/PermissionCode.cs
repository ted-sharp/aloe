using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;

/// <summary>
/// アプリケーション内の権限に影響する設定コードです。
/// </summary>
public static class PermissionCode
{
    /// <summary>
    /// ポリシーマスターの表示権限です。
    /// </summary>
    public static readonly string MaintPoliciesR = "Maint_Policies_R";

    /// <summary>
    /// ポリシーマスターの書き込み権限です。
    /// 追加、更新、削除が行えます。
    /// </summary>
    public static readonly string MaintPoliciesW = "Maint_Policies_W";

    /// <summary>
    /// ユーザーマスターの表示権限です。
    /// </summary>
    public static readonly string MaintUsersR = "Maint_Users_R";

    /// <summary>
    /// ユーザーマスターの書き込み権限です。
    /// 追加、更新、削除が行えます。
    /// </summary>
    public static readonly string MaintUsersW = "Maint_Users_W";

}
