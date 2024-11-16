using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Domain.Constants;

public static class PolicyCodes
{
    /// <summary>
    /// ログイン失敗時にロックするための失敗回数です。
    /// </summary>
    public static string LoginLockingFailAtempts => "LOGIN-LockingFailCount";

    /// <summary>
    /// ログイン失敗時にロックする秒数です。
    /// </summary>
    public static string LoginLockingSeconds => "LOGIN-LockingSeconds";
}
