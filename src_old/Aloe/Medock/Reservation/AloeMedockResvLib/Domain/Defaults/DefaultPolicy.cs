using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Defaults;

public static class DefaultPolicy
{
    // TODO: Lazy にしたい

    public static Dictionary<string, Policy> CreateDefaultPolicies()
    {
        var policies = new Dictionary<string, Policy>
        {
            [PolicyCode.LoginLockingFailAttempts] = new()
            {
                PolicyCode = PolicyCode.LoginLockingFailAttempts,
                PolicyName = nameof(PolicyCode.LoginLockingFailAttempts),
                PolicyDesc = "ログイン失敗時にロックするための失敗回数です。",
                DataType = DataType.Int32,
                PolicyValue = "3",
            },

            [PolicyCode.LoginLockingSeconds] = new()
            {
                PolicyCode = PolicyCode.LoginLockingSeconds,
                PolicyName = nameof(PolicyCode.LoginLockingSeconds),
                PolicyDesc = "ログイン失敗時にロックする秒数です。",
                DataType = DataType.Int32,
                PolicyValue = "30",
            },

            [PolicyCode.ResvDefaultFloor1] = new()
            {
                PolicyCode = PolicyCode.ResvDefaultFloor1,
                PolicyName = nameof(PolicyCode.ResvDefaultFloor1),
                PolicyDesc = "デフォルトで呼び出すフロア1のコード(FloorCode)です。",
                DataType = DataType.Int32,
                PolicyValue = "1",
            },

            [PolicyCode.ResvDefaultFloor2] = new()
            {
                PolicyCode = PolicyCode.ResvDefaultFloor2,
                PolicyName = nameof(PolicyCode.ResvDefaultFloor2),
                PolicyDesc = "デフォルトで呼び出すフロア2のコード(FloorCode)です。",
                DataType = DataType.Int32,
                PolicyValue = "2",
            },
        };

        return policies;
    }
}
