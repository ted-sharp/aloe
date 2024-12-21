using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

internal class Seeder
{
    ILogger _logger;
    IDbContextFactory<AppDbContext> _factory;

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
        try
        {
            Console.WriteLine("Seeding...");
            await using var context = await this._factory.CreateDbContextAsync();
            var count = await this.SeedAsync(context);
            count += await this.SeedAsync(context);
            Console.WriteLine($"Insert: {count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InitializeSeedAsync Error: {ex.Message}");
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// サンプルデータ挿入用のメソッドです。
    /// </summary>
    private async Task<int> SeedAsync(AppDbContext context)
    {
        try
        {

            var slots = new[] {
                "08:30", "08:30", "08:30", "08:30",
                "09:00", "09:00", "09:00", "09:00",
                "09:30", "09:30", "09:30", "09:30",
                "10:00", "10:00", "10:00",
                "10:30", "10:30", "10:30",
                "11:00", "11:00", "11:00",
                "11:30", "11:30", "11:30",
                "12:00", "12:00",
                "13:30", "13:30", "13:30",
                "14:00", "14:00", "14:00",
                "14:30", "14:30", "14:30",
                "15:00", "15:00", "15:00",
                "15:30", "15:30", "15:30",
                "16:00", "16:00", "16:00",
                "16:30", "16:30", "16:30",
                "17:00", "17:00",
                "EX", "EX", "EX", "EX",
            };

            #region マスターデータ

            if (!context.Users.AsNoTracking().Any())
            {
                context.Users.AddRange([
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

            if (!context.Policies.AsNoTracking().Any())
            {
                var policies = PolicyService.CreateDefaultPolicies();
                context.Policies.AddRange(policies.Values);
            }

            if (!context.Equipments.AsNoTracking().Any())
            {
                context.Equipments.AddRange([
                    new("胃カメラ", "院内の胃カメラです。", 1),
                    new("胃カメラ(外)", "外部に委託している胃カメラです。", 2),
                    new("CT", "CT", 3),
                    new("MRI", "MRI", 4),
                    new("大腸カメラ", "大腸カメラ", 5),
                    new("頸動脈エコー", "頸動脈エコー", 6),
                ]);
            }


            if (!context.EquipmentSlots.AsNoTracking().Any())
            {
                context.EquipmentSlots.AddRange([
                    new("1900/1/1".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.None, slots),
                    new("1900/1/2".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Sunday, ""),
                    new("1900/1/3".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Saturday, ""),
                ]);
            }


            if (!context.Floors.AsNoTracking().Any())
            {
                context.Floors.AddRange([
                    new("8階", "メインフロアです。", 1),
                    new("7階", "レディースフロアです。", 2),
                    new("巡回", "バス健診用です。", 3),
                    new("ダミー", "ダミーです。", 9),
                ]);
            }

            if (!context.Rooms.AsNoTracking().Any())
            {
                context.Rooms.AddRange([
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

            if (!context.DailySlots.AsNoTracking().Any())
            {
                context.DailySlots.AddRange([
                    new("1900/1/1".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.None, 40, 40, "09:00 09:30 10:00 10:30 11:00 11:30 13:00 13:30 14:00 14:30 15:00 15:30", "6 6 7 7 7 7 6 6 7 7 7 7"),
                    new("1900/1/2".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Sunday, 0, 0, "", ""),
                    new("1900/1/3".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Saturday, 0, 0, "", ""),
                ]);
            }

            #endregion マスターデータ

            // マスターデータを使うため保存する
            var count = await context.SaveChangesAsync();

            #region トランザクションデータ

            if (!context.Patients.AsNoTracking().Any())
            {
                context.Patients.AddRange([
                    new("1", "山田　太郎", "ヤマダ　タロウ", "2000/4/1".ToDateOrToday(), (int)SexCode.Male),
                    new("2", "山田　花子", "ヤマダ　ハナコ", "1980/12/31".ToDateOrToday(), (int)SexCode.Female),
                    new("3", "名無しの　権兵衛", "ナナシノ　ゴンベエ", "1900/1/1".ToDateOrToday(), (int)SexCode.NotKnown),
                ]);
            }

            if (!context.Organizations.AsNoTracking().Any())
            {
                context.Organizations.AddRange([
                    new("株式会社 ABC", "カブシキガイシャ　エービーシー", "ABC", "ABC"),
                ]);
            }


            if (!context.EquipmentBookings.AsNoTracking().Any())
            {
                var rnd = new Random();

                var equipments = context.Equipments.ToList();
                var equipmentMax = equipments.Count;

                slots = this.CreateSlotStrings(slots);
                var slotMax = slots.Length;

                var symbols = new[] { "", "鼻", "口", "★" };
                var symbolMax = symbols.Length;

                var firstDate = DateTime.Today.AddDays(1 - DateTime.Today.Day);
                for (var i = 0; i < 3000; i++)
                {
                    var equipId = equipments.Skip(rnd.Next(0, equipmentMax)).First().EquipId;
                    var date = firstDate.AddDays(rnd.Next(0, 60));
                    var slot = slots[rnd.Next(0, slotMax)];
                    var symbol = symbols[rnd.Next(0, symbolMax)];
                    var booking = new ReservationEquipmentBooking(equipId, date, slot, symbol, $"remark_{i}", true);
                    context.EquipmentBookings.Add(booking);
                }
            }


            #endregion トランザクションデータ

            count += await context.SaveChangesAsync();
            return count;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// 最大公約数的なスロットのリストを作成します。
    /// </summary>
    private string[] CreateSlotStrings(string[] slots)
    {
        // 定義されている最大の slot の一覧を作成する
        var cols = new HashSet<string>();


        // def 毎の slot 重複数を数える
        var slotCounts = new Dictionary<string, int>();

        for (var i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];

            if (!slotCounts.TryAdd(slot, 1))
            {
                slotCounts[slot]++;
                // 重複したら (n) をつける
                slot = $"{slot}({slotCounts[slot]})";
                slots[i] = slot;
            }

            cols.Add(slot);
        }

        return cols.OrderBy(x => x).ToArray();
    }
}
