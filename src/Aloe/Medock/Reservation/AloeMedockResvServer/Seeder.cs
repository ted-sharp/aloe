using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

internal partial class Seeder
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public Seeder(
        IDbContextFactory<AppDbContext> factory)
    {
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
        var count = 0;

        try
        {
            Console.WriteLine("Seeding...");

            context = await this._factory.CreateDbContextAsync();
            trans = await context.Database.BeginTransactionAsync();

            count += await Seeder.SeedUserAsync(context);
            count += await Seeder.SeedOrgPtAsync(context);
            count += await Seeder.SeedPlanAsync(context);
            count += await Seeder.SeedContractAsync(context);
            count += await Seeder.SeedResvAsync(context);

            count += await context.SaveChangesAsync();

            await trans.CommitAsync();

            Console.WriteLine($"{nameof(Seeder)}.{nameof(this.InsertDataAsync)}() Inserted: {count}");
        }
        catch (Exception ex)
        {
            if (trans is not null)
            {
                await trans.RollbackAsync();
            }

            Console.WriteLine($"{nameof(Seeder)}.{nameof(this.InsertDataAsync)}() Error: {ex.Message}");
            Console.WriteLine(ex.ToString());
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
