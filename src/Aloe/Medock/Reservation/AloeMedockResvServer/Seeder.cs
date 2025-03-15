using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

internal partial class Seeder
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;

    public Seeder(
        ILogger<Seeder> logger,
        IDbContextFactory<AppDbContext> factory)
    {
        this._logger = logger;
        this._factory = factory;
    }

    /// <summary>
    /// 必要なサンプルデータを作成します。
    /// すでにデータが存在する場合は何もしません。
    /// </summary>
    internal async ValueTask InsertDataAsync()
    {
        AppDbContext? context = null;
        IDbContextTransaction? trans = null;

        try
        {
            this._logger.LogInformation("Seeding...");

            context = await this._factory.CreateDbContextAsync();
            trans = await context.Database.BeginTransactionAsync();

            var totalCount = 0;

            var userCount = await this.SeedUserAsync(context);
            this._logger.LogInformation($"{nameof(this.SeedUserAsync)}() Inserted: {userCount}");
            totalCount += userCount;

            var orgPtCount = await this.SeedOrgPtAsync(context);
            this._logger.LogInformation($"{nameof(this.SeedOrgPtAsync)}() Inserted: {orgPtCount}");
            totalCount += orgPtCount;

            var planCount = await this.SeedPlanAsync(context);
            this._logger.LogInformation($"{nameof(this.SeedPlanAsync)}() Inserted: {planCount}");
            totalCount += planCount;

            var contractCount = await this.SeedContractAsync(context);
            this._logger.LogInformation($"{nameof(this.SeedContractAsync)}() Inserted: {contractCount}");
            totalCount += contractCount;

            var resvCount = await this.SeedResvAsync(context);
            this._logger.LogInformation($"{nameof(this.SeedResvAsync)}() Inserted: {resvCount}");
            totalCount += resvCount;

            await trans.CommitAsync();

            this._logger.LogInformation($"{nameof(Seeder)}.{nameof(this.InsertDataAsync)}() Inserted in total: {totalCount}");
        }
        catch (Exception ex)
        {
            if (trans is not null)
            {
                await trans.RollbackAsync();
            }

            this._logger.LogError(ex, ex.Message);
        }
        finally
        {
            if (context is not null)
            {
                await context.DisposeAsync();
            }
        }
    }
}
