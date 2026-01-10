using Aloe.Apps.MedockLib.Common.Exceptions;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Logging;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Repositories;

/// <summary>
/// リポジトリの基底クラス。共通のクエリパターンとエラーハンドリングを提供します。
/// </summary>
/// <remarks>
/// 重複するIncludeパターン、IsDeletedフィルタリング、エラーハンドリングを統一します。
/// </remarks>
public abstract class RepositoryBase
{
    protected readonly MedockDbContext Context;
    protected readonly ILogger Logger;
    protected readonly IUserContextService UserContextService;
    protected readonly IDateTimeProvider DateTimeProvider;

    protected RepositoryBase(
        MedockDbContext context,
        ILogger logger,
        IUserContextService userContextService,
        IDateTimeProvider dateTimeProvider)
    {
        this.Context = context;
        this.Logger = logger;
        this.UserContextService = userContextService;
        this.DateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// 標準的なIsDeletedフィルタリングを適用されたAppointmentクエリを作成します。
    /// </summary>
    protected IQueryable<Data.Entities.Appointment> CreateAppointmentQuery(bool noTracking = true)
    {
        var query = noTracking
            ? this.Context.Appointments.AsNoTracking()
            : this.Context.Appointments;

        return query
            .Include(a => a.Floor)
            .Include(a => a.Organization)
            .Include(a => a.Patient)
            .Include(a => a.AppointmentResourceAssignments)
                .ThenInclude(r => r.AppointmentResource)
            .Where(a => !a.IsDeleted);
    }

    /// <summary>
    /// テナントコンテキスト情報を取得します。
    /// </summary>
    protected (Guid? TenantId, Guid? FacilityId, Guid? UserId) GetTenantContext()
    {
        return this.UserContextService.GetTenantContext();
    }

    /// <summary>
    /// データベースエラーをスローします。
    /// </summary>
    protected void ThrowDatabaseException(string message, Exception ex)
    {
        throw new DatabaseException(message, ex);
    }
}
