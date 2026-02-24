using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 予約フォームサービス実装
/// フォーム初期化に必要なデータロードおよびリソース取得を担当
/// </summary>
public class AppointmentFormService : IAppointmentFormService
{
    private readonly IDbContextFactory<MedockDbContext> _contextFactory;
    private readonly ILogger<AppointmentFormService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AppointmentFormService(
        IDbContextFactory<MedockDbContext> contextFactory,
        ILogger<AppointmentFormService> logger,
        IDateTimeProvider dateTimeProvider)
    {
        this._contextFactory = contextFactory;
        this._logger = logger;
        this._dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public async Task<List<(Guid Id, string Name)>> GetAvailableEquipmentResourcesAsync(Guid facilityId)
    {
        try
        {
            await using var context = await this._contextFactory.CreateDbContextAsync();

            var resources = await context.AppointmentResources
                .AsNoTracking()
                .Where(r => r.Floor.FacilityId == facilityId &&
                           !r.IsDeleted &&
                           r.ApptResTypeCode == (int)AppointmentResourceType.Equipment)
                .OrderBy(r => r.ApptResSeq)
                .ThenBy(r => r.ApptResName)
                .Select(r => new { r.ApptResId, r.ApptResName })
                .ToListAsync();

            return resources
                .Select(r => (r.ApptResId, r.ApptResName))
                .ToList();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error loading available equipment resources for facility {FacilityId}", facilityId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Guid> GetDefaultOrganizationAsync(Guid facilityId)
    {
        try
        {
            await using var context = await this._contextFactory.CreateDbContextAsync();

            var defaultOrg = await context.Organizations
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.FacilityId == facilityId && !o.IsDeleted);

            return defaultOrg?.OrgId ?? Guid.Empty;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting default organization for facility {FacilityId}", facilityId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Guid> GetDefaultFloorAsync(Guid facilityId)
    {
        try
        {
            await using var context = await this._contextFactory.CreateDbContextAsync();

            var defaultFloor = await context.Floors
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FacilityId == facilityId && !f.IsDeleted);

            return defaultFloor?.FloorId ?? Guid.Empty;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting default floor for facility {FacilityId}", facilityId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Floor?> GetFloorAsync(Guid floorId)
    {
        try
        {
            await using var context = await this._contextFactory.CreateDbContextAsync();

            return await context.Floors
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FloorId == floorId && !f.IsDeleted);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting floor {FloorId}", floorId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Guid> GetOrCreatePatientAsync(Guid facilityId, string patientName)
    {
        if (String.IsNullOrWhiteSpace(patientName))
        {
            throw new ArgumentException("Patient name cannot be empty or whitespace", nameof(patientName));
        }

        try
        {
            await using var context = await this._contextFactory.CreateDbContextAsync();

            // 患者名が指定されている場合は検索
            var existingPatient = await context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PtName == patientName && p.FacilityId == facilityId && !p.IsDeleted);

            if (existingPatient != null)
            {
                this._logger.LogInformation("Found existing patient: {PatientName} ({PatientId})", patientName, existingPatient.PtId);
                return existingPatient.PtId;
            }

            // 患者が見つからない場合は新規作成
            var now = this._dateTimeProvider.UtcNow;
            var primaryOrgId = await this.GetDefaultOrganizationAsync(facilityId);

            var newPatient = new Patient
            {
                PtId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                CanonicalPtId = Guid.CreateVersion7(),
                PtCode = $"PT{new DateTimeOffset(now).ToUnixTimeSeconds()}",
                PtName = patientName,
                PtNameCompat = patientName,
                CreatedAt = now,
                UpdatedAt = now
            };

            // デフォルト組織がある場合のみ設定
            if (primaryOrgId != Guid.Empty)
            {
                newPatient.PrimaryOrgId = primaryOrgId;
            }

            context.Patients.Add(newPatient);
            await context.SaveChangesAsync();

            this._logger.LogInformation("Created new patient: {PatientName} ({PatientId})", patientName, newPatient.PtId);

            return newPatient.PtId;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting or creating patient: {PatientName}", patientName);
            throw;
        }
    }
}
