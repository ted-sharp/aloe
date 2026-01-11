using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 予約リソース割り当てサービス
/// </summary>
public class AppointmentResourceAssignmentService : IAppointmentResourceAssignmentService
{
    private readonly IDbContextFactory<MedockDbContext> _dbContextFactory;
    private readonly ILogger<AppointmentResourceAssignmentService> _logger;

    public AppointmentResourceAssignmentService(
        IDbContextFactory<MedockDbContext> dbContextFactory,
        ILogger<AppointmentResourceAssignmentService> logger)
    {
        this._dbContextFactory = dbContextFactory;
        this._logger = logger;
    }

    /// <summary>
    /// Mainリソースを予約に自動的に割り当てます
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="apptId">予約ID</param>
    /// <param name="floorId">フロアID</param>
    /// <param name="timestamp">タイムスタンプ</param>
    public async Task AssignMainResourcesAsync(MedockDbContext context, Guid apptId, Guid floorId, DateTime timestamp)
    {
        var mainResources = await context.AppointmentResources
            .AsNoTracking()
            .Where(r => r.FloorId == floorId &&
                       !r.IsDeleted &&
                       r.ApptResTypeCode == (int)AppointmentResourceType.Main)
            .OrderBy(r => r.ApptResSeq)
            .ToListAsync();

        foreach (var mainResource in mainResources)
        {
            var assignment = new AppointmentResourceAssignment
            {
                ApptResAssignId = Guid.CreateVersion7(),
                ApptId = apptId,
                ApptResId = mainResource.ApptResId,
                IsDeleted = false,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            };

            await context.AppointmentResourceAssignments.AddAsync(assignment);
        }

        this._logger.LogDebug("Created {Count} main resource assignments for appointment {ApptId}",
            mainResources.Count, apptId);
    }

    /// <summary>
    /// 機器リソースを予約に割り当てます
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="apptId">予約ID</param>
    /// <param name="equipmentResourceIds">機器リソースIDのコレクション</param>
    /// <param name="timestamp">タイムスタンプ</param>
    public async Task AssignEquipmentResourcesAsync(MedockDbContext context, Guid apptId, IEnumerable<Guid> equipmentResourceIds, DateTime timestamp)
    {
        foreach (var resourceId in equipmentResourceIds)
        {
            var assignment = new AppointmentResourceAssignment
            {
                ApptResAssignId = Guid.CreateVersion7(),
                ApptId = apptId,
                ApptResId = resourceId,
                IsDeleted = false,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            };

            await context.AppointmentResourceAssignments.AddAsync(assignment);
        }

        this._logger.LogDebug("Created {Count} equipment resource assignments for appointment {ApptId}",
            equipmentResourceIds.Count(), apptId);
    }

    /// <summary>
    /// 予約の既存のリソース割り当てをすべてソフト削除します
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="apptId">予約ID</param>
    /// <param name="timestamp">タイムスタンプ</param>
    public async Task SoftDeleteAllAssignmentsAsync(MedockDbContext context, Guid apptId, DateTime timestamp)
    {
        var existingAssignments = await context.AppointmentResourceAssignments
            .Where(a => a.ApptId == apptId && !a.IsDeleted)
            .ToListAsync();

        foreach (var assignment in existingAssignments)
        {
            assignment.IsDeleted = true;
            assignment.UpdatedAt = timestamp;
        }

        this._logger.LogDebug("Soft deleted {Count} resource assignments for appointment {ApptId}",
            existingAssignments.Count, apptId);
    }
}
