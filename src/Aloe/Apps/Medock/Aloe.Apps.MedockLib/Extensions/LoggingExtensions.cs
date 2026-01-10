using Aloe.Apps.MedockLib.Common.Exceptions;
using Aloe.Apps.MedockLib.Services;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Extensions;

/// <summary>
/// ロギング関連の拡張メソッド
/// テナントコンテキストの取得とログ出力を統一的に処理します
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// リポジトリ・サービス層で発生したエラーをログして、DatabaseException をスロー
    /// </summary>
    /// <param name="logger">ロガーインスタンス</param>
    /// <param name="userContextService">ユーザーコンテキストサービス</param>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="ex">元の例外</param>
    /// <exception cref="DatabaseException">常にスロー</exception>
    public static void LogDataAccessError(
        this ILogger logger,
        IUserContextService userContextService,
        string message,
        Exception ex)
    {
        var (tenantId, facilityId, userId) = userContextService.GetTenantContext();
        logger.LogError(
            ex,
            "Data access error: {Message} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}",
            message,
            tenantId,
            facilityId,
            userId);

        throw new DatabaseException(message, ex);
    }

    /// <summary>
    /// リポジトリ・サービス層でアクセス制御エラーをログして、UnauthorizedException をスロー
    /// </summary>
    /// <param name="logger">ロガーインスタンス</param>
    /// <param name="userContextService">ユーザーコンテキストサービス</param>
    /// <param name="message">エラーメッセージ</param>
    /// <exception cref="UnauthorizedException">常にスロー</exception>
    public static void LogAccessDenied(
        this ILogger logger,
        IUserContextService userContextService,
        string message)
    {
        var (tenantId, facilityId, userId) = userContextService.GetTenantContext();
        logger.LogWarning(
            "Access denied: {Message} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}",
            message,
            tenantId,
            facilityId,
            userId);

        throw new UnauthorizedException(message);
    }

    /// <summary>
    /// リポジトリ・サービス層でリソース未検出エラーをログして、NotFoundException をスロー
    /// </summary>
    /// <param name="logger">ロガーインスタンス</param>
    /// <param name="userContextService">ユーザーコンテキストサービス</param>
    /// <param name="resourceType">リソースタイプ名</param>
    /// <param name="resourceId">リソースID</param>
    /// <exception cref="NotFoundException">常にスロー</exception>
    public static void LogResourceNotFound(
        this ILogger logger,
        IUserContextService userContextService,
        string resourceType,
        Guid resourceId)
    {
        var (tenantId, facilityId, userId) = userContextService.GetTenantContext();
        logger.LogWarning(
            "{ResourceType} not found: {ResourceId} | TenantId={TenantId}, FacilityId={FacilityId}, UserId={UserId}",
            resourceType,
            resourceId,
            tenantId,
            facilityId,
            userId);

        throw new NotFoundException(resourceType, resourceId);
    }

    /// <summary>
    /// パフォーマンス測定を開始して、完了時にログ出力
    /// </summary>
    /// <param name="logger">ロガーインスタンス</param>
    /// <param name="operationName">操作名</param>
    /// <returns>操作実行後に呼び出す Action</returns>
    public static Action<long> StartPerformanceLogging(
        this ILogger logger,
        string operationName)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        return (elapsedMs) =>
        {
            sw.Stop();
            logger.LogInformation(
                "[PERF] {Operation} completed in {ElapsedMs}ms",
                operationName,
                sw.ElapsedMilliseconds);
        };
    }

    /// <summary>
    /// パフォーマンス測定をラップ（Func 用）
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="logger">ロガーインスタンス</param>
    /// <param name="operationName">操作名</param>
    /// <param name="operation">実行する操作</param>
    /// <returns>操作の実行結果</returns>
    public static T MeasurePerformance<T>(
        this ILogger logger,
        string operationName,
        Func<T> operation)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            return operation();
        }
        finally
        {
            sw.Stop();
            logger.LogInformation(
                "[PERF] {Operation} completed in {ElapsedMs}ms",
                operationName,
                sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// 非同期パフォーマンス測定をラップ（Task 用）
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="logger">ロガーインスタンス</param>
    /// <param name="operationName">操作名</param>
    /// <param name="operation">実行する非同期操作</param>
    /// <returns>操作の実行結果</returns>
    public static async Task<T> MeasurePerformanceAsync<T>(
        this ILogger logger,
        string operationName,
        Func<Task<T>> operation)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            return await operation();
        }
        finally
        {
            sw.Stop();
            logger.LogInformation(
                "[PERF] {Operation} completed in {ElapsedMs}ms",
                operationName,
                sw.ElapsedMilliseconds);
        }
    }
}
