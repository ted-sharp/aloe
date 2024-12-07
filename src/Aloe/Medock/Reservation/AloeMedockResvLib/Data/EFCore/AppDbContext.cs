using System.Diagnostics;
using Aloe.Common.AloeCoreLib.Security;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    /// <summary>
    /// コード記述の参考用です。
    /// 実際のテーブルなどは作成していないため動作させようとすると例外が発生します。
    /// </summary>
    public DbSet<Sample> Samples { get; set; } = null!;

    #region AuthService

    public DbSet<Session> Sessions { get; set; } = null!;

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Policy> Policies { get; set; } = null!;

    #endregion AuthService

    #region Patient

    public DbSet<Patient> Patients { get; set; } = null!;

    public DbSet<Organization> Organizations { get; set; } = null!;

    #endregion Patient

    #region ResvEquipService

    public DbSet<ReservationEquipment> Equipments { get; set; } = null!;

    public DbSet<ReservationEquipmentSlot> EquipmentSlots { get; set; } = null!;

    public DbSet<ReservationEquipmentBooking> EquipmentBookings { get; set; } = null!;

    #endregion ResvEquipService

    #region ResvDailyService

    public DbSet<ReservationFloor> Floors { get; set; } = null!;

    public DbSet<ReservationRoom> Rooms { get; set; } = null!;

    public DbSet<ReservationDailySlot> DailySlots { get; set; } = null!;

    public DbSet<ReservationDailyBooking> DailyBookings { get; set; } = null!;

    #endregion ResvDailyService

    /// <summary>
    /// サンプルデータ挿入用のメソッドです。
    /// </summary>
    public async Task<int> SeedAsync()
    {
        try
        {
            if (!this.Users.Any())
            {
                this.Users.AddRange([
                    new("Administrator", "admin", "admin@example.com", "admin"),
                    new("User 1", "1", "user@example.com", "1"),
                    new("User 2", "2", "user@example.com", "2"),
                    new("User 3", "3", "user@example.com", "3"),
                    new("User 4", "4", "user@example.com", "4"),
                    new("User 5", "5", "user@example.com", "5"),
                    new("User 6", "6", "user@example.com", "6"),
                    new("User 7", "7", "user@example.com", "7"),
                    new("User 8", "8", "user@example.com", "8"),
                    new("User 9", "9", "user@example.com", "9"),
                ]);
            }

            if (!this.Policies.Any())
            {
                var policies = PolicyService.CreateDefaultPolicies();
                this.Policies.AddRange(policies.Values);
            }

            if (!this.Patients.Any())
            {
                this.Patients.AddRange([
                    new("1", "山田　太郎", "ヤマダ　タロウ", "2000/4/1".ToDateOrToday(), (int)SexCode.Male),
                    new("2", "山田　花子", "ヤマダ　ハナコ", "1980/12/31".ToDateOrToday(), (int)SexCode.Female),
                    new("3", "名無しの　権兵衛", "ナナシノ　ゴンベエ", "1900/1/1".ToDateOrToday(), (int)SexCode.NotKnown),
                ]);
            }

            if (!this.Organizations.Any())
            {
                this.Organizations.AddRange([
                    new("株式会社 ABC", "カブシキガイシャ　エービーシー", "ABC", "ABC"),
                ]);
            }

            if (!this.Equipments.Any())
            {
                this.Equipments.AddRange([
                    new("胃カメラ", "院内の胃カメラです。", 1),
                    new("胃カメラ(外)", "外部に委託している胃カメラです。", 2),
                    new("CT", "CT", 3),
                    new("MRI", "MRI", 4),
                    new("大腸カメラ", "大腸カメラ", 5),
                    new("頸動脈エコー", "頸動脈エコー", 6),
                ]);
            }

            if (!this.EquipmentSlots.Any())
            {
                this.EquipmentSlots.AddRange([
                    new("1900/1/1".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.None, "09:00 09:00 10:00 10:00 11:00 11:00 13:00 13:00 14:00 14:00 15:00 15:00"),
                    new("1900/1/2".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Sunday, ""),
                    new("1900/1/3".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Saturday, ""),
                ]);
            }

            if (!this.Floors.Any())
            {
                this.Floors.AddRange([
                    new("8階", "メインフロアです。", 1),
                    new("7階", "レディースフロアです。", 2),
                    new("巡回", "バス健診用です。", 3),
                    new("ダミー", "ダミーです。", 9),
                ]);
            }

            if (!this.Rooms.Any())
            {
                this.Rooms.AddRange([
                    new("子宮細胞診", "子宮細胞診です。", 1),
                    new("婦人科超音波", "婦人科超音波です。", 2),
                    new("マンモ", "マンモグラフィーです。", 3),
                    new("乳腺エコー", "乳腺エコーです。", 4),
                    new("腹部エコー", "腹部エコーです。", 5),
                    new("胃バリウム", "胃バリウムです。", 6),
                    new("VDT", "VDTです。", 7),
                    new("CT", "CTです。", 8),
                    new("MRI", "MRIです。", 9),
                    new("心電図", "心電図です。", 10),
                    new("肺機能", "肺機能です。", 11),
                    new("眼底", "眼底です。", 12),
                    new("ABI", "血管年齢です。", 13),
                    new("乳腺+腹部エコー", "乳腺エコーと腹部エコーです。", 14),
                    new("胃カメラ", "胃カメラです。", 15),
                ]);
            }

            if (!this.DailySlots.Any())
            {
                this.DailySlots.AddRange([
                    new("1900/1/1".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.None, 40, 40, "09:00 09:30 10:00 10:30 11:00 11:30 13:00 13:30 14:00 14:30 15:00 15:30", "6 6 7 7 7 7 6 6 7 7 7 7"),
                    new("1900/1/2".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Sunday, 0, 0, "", ""),
                    new("1900/1/3".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Saturday, 0, 0, "", ""),
                ]);
            }

            return await this.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            throw;
        }
    }
}
