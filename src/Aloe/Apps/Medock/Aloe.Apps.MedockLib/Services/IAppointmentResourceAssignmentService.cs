using Aloe.Apps.MedockLib.Data;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 予約リソース割当サービスのインターフェース
/// </summary>
public interface IAppointmentResourceAssignmentService
{
    /// <summary>
    /// メインリソースを予約に割り当てます
    /// </summary>
    Task AssignMainResourcesAsync(
        MedockDbContext context,
        Guid apptId,
        Guid floorId,
        DateTime timestamp);

    /// <summary>
    /// 機器リソースを予約に割り当てます
    /// </summary>
    Task AssignEquipmentResourcesAsync(
        MedockDbContext context,
        Guid apptId,
        IEnumerable<Guid> equipmentResourceIds,
        DateTime timestamp);

    /// <summary>
    /// 予約のすべてのリソース割当を論理削除します
    /// </summary>
    Task SoftDeleteAllAssignmentsAsync(
        MedockDbContext context,
        Guid apptId,
        DateTime timestamp);
}
