using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Defaults;

public static class DefaultPermission
{

    public static Dictionary<string, Permission> CreateDefaultPermissions()
    {
        var policies = new Dictionary<string, Permission>
        {
            [PermissionCode.MaintPoliciesR] = new()
            {
                PermCode = PermissionCode.MaintPoliciesR,
                PermName = nameof(PermissionCode.MaintPoliciesR),
                PermDesc = "ポリシーマスターの表示権限です。",
            },
        };

        return policies;
    }

}
