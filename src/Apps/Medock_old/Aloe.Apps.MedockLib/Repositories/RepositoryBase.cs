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

    /// <summary>
    /// 読み取り操作をエラーハンドリング付きで実行します。
    /// DbUpdateException, DbUpdateConcurrencyException, その他の例外をDatabaseExceptionに変換します。
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="operation">実行する非同期操作</param>
    /// <param name="errorMessage">エラーメッセージテンプレート（例外オブジェクトが渡される）</param>
    /// <param name="logErrorAction">エラーログを記録するアクション</param>
    /// <returns>操作の結果</returns>
    protected async Task<T> ExecuteQueryAsync<T>(
        Func<Task<T>> operation,
        Func<Exception, string> errorMessage,
        Action<Exception>? logErrorAction = null)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            logErrorAction?.Invoke(ex);
            throw new DatabaseException(errorMessage(ex), ex);
        }
    }

    /// <summary>
    /// 書き込み操作をエラーハンドリング付きで実行します。
    /// DbUpdateConcurrencyException, DbUpdateException, NotFoundException を適切に処理します。
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="operation">実行する非同期操作</param>
    /// <param name="errorMessageDbUpdate">DbUpdateException用のエラーメッセージテンプレート</param>
    /// <param name="errorMessageConcurrency">DbUpdateConcurrencyException用のエラーメッセージテンプレート</param>
    /// <param name="logErrorAction">エラーログを記録するアクション</param>
    /// <returns>操作の結果</returns>
    protected async Task<T> ExecuteCommandAsync<T>(
        Func<Task<T>> operation,
        Func<Exception, string> errorMessageDbUpdate,
        Func<Exception, string> errorMessageConcurrency,
        Action<Exception>? logErrorAction = null)
    {
        try
        {
            return await operation();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logErrorAction?.Invoke(ex);
            throw new ConcurrencyException(errorMessageConcurrency(ex), ex);
        }
        catch (DbUpdateException ex)
        {
            logErrorAction?.Invoke(ex);
            throw new DatabaseException(errorMessageDbUpdate(ex), ex);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logErrorAction?.Invoke(ex);
            throw;
        }
    }

    /// <summary>
    /// 非同期コマンド操作をエラーハンドリング付きで実行します（戻り値なし）。
    /// DbUpdateConcurrencyException, DbUpdateException, NotFoundException を適切に処理します。
    /// </summary>
    /// <param name="operation">実行する非同期操作</param>
    /// <param name="errorMessageDbUpdate">DbUpdateException用のエラーメッセージテンプレート</param>
    /// <param name="errorMessageConcurrency">DbUpdateConcurrencyException用のエラーメッセージテンプレート</param>
    /// <param name="logErrorAction">エラーログを記録するアクション</param>
    protected async Task ExecuteCommandAsync(
        Func<Task> operation,
        Func<Exception, string> errorMessageDbUpdate,
        Func<Exception, string> errorMessageConcurrency,
        Action<Exception>? logErrorAction = null)
    {
        try
        {
            await operation();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logErrorAction?.Invoke(ex);
            throw new ConcurrencyException(errorMessageConcurrency(ex), ex);
        }
        catch (DbUpdateException ex)
        {
            logErrorAction?.Invoke(ex);
            throw new DatabaseException(errorMessageDbUpdate(ex), ex);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logErrorAction?.Invoke(ex);
            throw;
        }
    }
}
