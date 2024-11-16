using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AloeReservationGrid.Lib.ReservationLib.Data.EFCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>
    /// コード記述の参考用です。
    /// 実際のテーブルなどは作成していないため動作させようとすると例外が発生します。
    /// </summary>
    public DbSet<Sample> Samples { get; set; }

    #region AuthService

    public DbSet<Session> Sessions { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Policy> Policies { get; set; }

    #endregion AuthService

    #region ResvEquipService

    public DbSet<ReservationEquipment> Equipments { get; set; }

    public DbSet<ReservationEquipmentSlot> EquipmentSlots { get; set; }

    public DbSet<ReservationEquipmentBooking> EquipmentBookings { get; set; }

    #endregion ResvEquipService

    #region ResvDailyService

    public DbSet<ReservationFloor> Floors { get; set; }

    public DbSet<ReservationRoom> Rooms { get; set; }

    public DbSet<ReservationDailySlot> DailySlots { get; set; }

    public DbSet<ReservationDailyBooking> DailyBookings { get; set; }

    #endregion ResvDailyService
}
