using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 予約フォームサービスインターフェース
/// フォーム初期化、データロード、リソース取得などを提供
/// </summary>
public interface IAppointmentFormService
{
    /// <summary>
    /// 施設の利用可能な機器リソースを取得
    /// </summary>
    Task<List<(Guid Id, string Name)>> GetAvailableEquipmentResourcesAsync(Guid facilityId);

    /// <summary>
    /// 施設のデフォルト組織を取得
    /// </summary>
    Task<Guid> GetDefaultOrganizationAsync(Guid facilityId);

    /// <summary>
    /// 施設のデフォルトフロアを取得
    /// </summary>
    Task<Guid> GetDefaultFloorAsync(Guid facilityId);

    /// <summary>
    /// フロア情報を取得
    /// </summary>
    Task<Floor?> GetFloorAsync(Guid floorId);

    /// <summary>
    /// 患者名から患者を検索、またはなければ新規作成
    /// </summary>
    Task<Guid> GetOrCreatePatientAsync(Guid facilityId, string patientName);
}
