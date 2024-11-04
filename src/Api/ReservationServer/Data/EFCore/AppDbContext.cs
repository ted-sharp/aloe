using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AloeReservationGrid.Api.ReservationServer.Data.EFCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>
    /// コード記述の参考用です。
    /// 実際のテーブルなどは作成していないため動作させようとすると例外が発生します。
    /// </summary>
    public DbSet<Sample> Samples { get; set; }

    public DbSet<Session> Sessions { get; set; }
    public DbSet<User> Users { get; set; }
}
