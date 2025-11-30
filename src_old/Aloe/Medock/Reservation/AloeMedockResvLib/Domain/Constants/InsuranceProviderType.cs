using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;

/// <summary>
/// 保険者の大まかな種類(アプリ固有)
/// </summary>
public enum InsuranceProviderType
{
    /// <summary>
    /// なし
    /// </summary>
    None = 0,

    /// <summary>
    /// 協会けんぽ
    /// </summary>
    KyokaiKenpo = 1,

    /// <summary>
    /// 代行機関
    /// </summary>
    DelegateAgency = 2,

    /// <summary>
    /// 健康保険組合
    /// </summary>
    HealthInsuranceSociety = 3,

    /// <summary>
    /// 国保
    /// </summary>
    NationalHealthInsurance = 4,

    /// <summary>
    /// 共済
    /// </summary>
    MutualAidAssociation = 5,

    /// <summary>
    /// 船員保険
    /// </summary>
    MarinersInsurance = 6,

    /// <summary>
    /// その他
    /// </summary>
    Others = 7,
}
