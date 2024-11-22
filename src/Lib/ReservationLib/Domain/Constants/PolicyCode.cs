using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Domain.Constants;

public static class PolicyCode
{
    /// <summary>
    /// ログイン失敗時にロックするための失敗回数です。
    /// </summary>
    public static readonly string LoginLockingFailAtempts = "LOGIN-LockingFailCount";

    /// <summary>
    /// ログイン失敗時にロックする秒数です。
    /// </summary>
    public static readonly string LoginLockingSeconds = "LOGIN-LockingSeconds";

    /// <summary>
    /// フロア1です。
    /// </summary>
    public static readonly string ResvDefaultFloor1 = "RESV-DefaultFloor1";

    /// <summary>
    /// フロア2です。
    /// </summary>
    public static readonly string ResvDefaultFloor2 = "RESV-DefaultFloor2";

}
