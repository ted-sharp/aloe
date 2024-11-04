using AloeReservationGrid.Api.ReservationServer.Data.EFCore;
using AloeReservationGrid.Lib.ReservationLib.Data.Dto;
using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace AloeReservationGrid.Api.ReservationServer.Data.Repos;

public interface IRepository
{
    DbContext Context { get; }
}

public static class RepositoryExtensions
{
    /// <summary>
    /// 1件のみ取得します。
    /// </summary>
    /// <remarks>
    /// 1件のみの取得のため、キャッシュ効率の効きやすい TEntity 型のままとします。
    /// FindAsync() を使うことで、Where().Select() よりキャッシュが効きやすくなります。
    /// </remarks>
    public static async ValueTask<TEntity?> FetchAsync<TEntity, TKey>(
        this IRepository repo,
        TKey id)
        where TEntity : AuditableEntityBase<TKey>
        where TKey : struct
    {
        return await repo.Context.Set<TEntity>().FindAsync(id);
    }

    /// <summary>
    /// 1件のみ更新します。
    /// 存在しない場合は作成されます。
    /// </summary>
    public static async ValueTask AddOrUpdateAsync<TEntity, TKey>(
        this IRepository repo,
        TEntity entity,
        bool isAutoSave = true)
        where TEntity : AuditableEntityBase<TKey>
        where TKey : struct
    {
        var existingEntity = await repo.Context.Set<TEntity>().FindAsync(entity.Id);
        if (existingEntity == null)
        {
            repo.Context.Set<TEntity>().Add(entity);
        }
        else
        {
            repo.Context.Entry(existingEntity).CurrentValues.SetValues(entity);
        }

        if (isAutoSave)
        {
            await repo.Context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 1件のみ削除します。
    /// 論理削除となります。
    /// </summary>
    public static async ValueTask DeleteAsync<TEntity, TKey>(
        this IRepository repo,
        TKey id,
        bool isAutoSave = true)
        where TEntity : AuditableEntityBase<TKey>
        where TKey : struct
    {
        var existingEntity = await repo.Context.Set<TEntity>().FindAsync(id);
        if (existingEntity != null)
        {
            existingEntity.IsDeleted = true;
        }

        if (isAutoSave)
        {
            await repo.Context.SaveChangesAsync();
        }
    }
}
