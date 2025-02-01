using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

internal partial class Seeder
{
    private static async Task<int> SeedResvAsync(AppDbContext context)
    {
        try
        {
            var count = 0;

            var rnd = new Random();

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
                "AM", "AM", "AM", "AM", "AM",
                "PM", "PM", "PM", "PM", "PM",
                "EX", "EX", "EX", "EX",
            };

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
                    new("7階(♀)", "レディースフロアです。", 2),
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
                    new("1900/1/1".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.None, "09:00 09:30 10:00 10:30 11:00 11:30 13:00 13:30 14:00 14:30 15:00 15:30"),
                    new("1900/1/2".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Sunday, ""),
                    new("1900/1/3".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Saturday, ""),
                ]);
            }


            if (!context.EquipmentBookings.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var equipments = context.Equipments.ToList();
                var equipmentMax = equipments.Count;

                slots = Seeder.CreateSlotStrings(slots);
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

            count += await context.SaveChangesAsync();

            return count;
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// 最大公約数的なスロットのリストを作成します。
    /// </summary>
    private static string[] CreateSlotStrings(string[] slots)
    {
        // 定義されている最大の slot の一覧を作成する
        var cols = new HashSet<string>(slots.Length);


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
