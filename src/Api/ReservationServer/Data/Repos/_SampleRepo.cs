using AloeReservationGrid.Api.ReservationServer.Data.EFCore;
using AloeReservationGrid.Lib.ReservationLib.Data.Dto;
using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using EFCore.BulkExtensions;
using MessagePack;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;

namespace AloeReservationGrid.Api.ReservationServer.Data.Repos;

// 必要であればインターフェースを用意します。
public interface ISampleDtoRepo : IRepository
{
    ValueTask<SampleDto?> FetchDtoAsync(int id);
    ValueTask<List<SampleDto>> FetchAllDtosAsync();
    ValueTask<List<SampleDto>> FetchWhereDtosAsync(
        Expression<Func<Sample, bool>> predicate,
        int pageIndex,
        int pageSize);
    ValueTask<int> FetchWhereCountAsync(
        Expression<Func<Sample, bool>> predicate);
    ValueTask AddOrUpdateAsync(Sample entity);
    ValueTask Delete(int id);
}

// 継承しているインターフェースは省略可能ですが、すべて列挙して読みやすさを優先します。
public class SampleDtoRepo(AppDbContext context) : ISampleDtoRepo, IRepository
{
    private readonly AppDbContext _context = context;

    // 各サービスからは直接DbContextは使いません。
    // そうすることで、生SQLを記述している個所などがリポジトリ層に集約されます。
    // トランザクションでのみ使用可能です。(UnitOfWorkパターンがないとき)
    public DbContext Context => this._context;

    public async ValueTask<SampleDto?> FetchDtoAsync(int id)
    {
        // 1件だけなので直接マッピングするよりもキャッシュを優先します。
        var entity = await this.FetchAsync<Sample, int>(id);

        if (entity == null)
        {
            return null;
        }

        return new SampleDto
        {
            SampleId = entity.SampleId,
            Name = entity.Name,
        };
    }

    // 件数の少ないマスターの一覧などで使用します。
    public async ValueTask<List<SampleDto>> FetchAllDtosAsync()
    {
        return await this._context.Samples
            // 式ツリーとして評価されてSQL文に変換されます。
            // Sample クラスは経由せずに SampleDto クラスが使用されます。
            .Select(x => new SampleDto
            {
                SampleId = x.SampleId,
                Name = x.Name,
            }).ToListAsync();
    }

    // 検索結果などで使用します。(オフセット法)
    public async ValueTask<List<SampleDto>> FetchWhereDtosAsync(
        Expression<Func<Sample, bool>> predicate,
        int pageIndex,
        int pageSize)
    {
        return await this._context.Samples
            .Where(predicate)
            .OrderBy(x => x.SampleId)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(x => new SampleDto
            {
                SampleId = x.SampleId,
                Name = x.Name,
            }).ToListAsync();
    }

    public async ValueTask<int> FetchWhereCountAsync(
        Expression<Func<Sample, bool>> predicate)
    {
        return await this._context.Samples
            .CountAsync(predicate);
    }

    // このメソッドを呼ばれる前に、セッション情報は反映しておきます。
    public async ValueTask AddOrUpdateAsync(Sample entity) => await this.AddOrUpdateAsync<Sample, int>(entity, isAutoSave: true).ConfigureAwait(false);

    // このメソッドを呼ばれる前に、セッション情報は反映しておきます。
    public async ValueTask Delete(int id) => await this.DeleteAsync<Sample, int>(id, isAutoSave: true).ConfigureAwait(false);
}
