using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

internal partial class Seeder
{
    private async Task<int> SeedResvAsync(AppDbContext context)
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

            var slotMax = slots.Length;

            if (!context.Equipments.Any())
            {
                this._logger.LogInformation("ReservationEquipment creating...");

                var equipments = new ReservationEquipment[]
                {
                    new("胃カメラ", "院内の胃カメラです。", 1),
                    new("胃カメラ(外)", "外部に委託している胃カメラです。", 2),
                    new("CT", "CT", 3),
                    new("MRI", "MRI", 4),
                    new("大腸カメラ", "大腸カメラ", 5),
                    new("頸動脈エコー", "頸動脈エコー", 6),
                };

                await context.BulkInsertAsync(equipments);
                count += equipments.Length;
            }

            if (!context.EquipmentSlots.Any())
            {
                this._logger.LogInformation("ReservationEquipmentSlot creating...");

                var equipmentSlots = new ReservationEquipmentSlot[]
                {
                    new(new DateTime(1900, 1, 1), DowCode.None, slots),
                    new(new DateTime(1900, 1, 2), DowCode.Sunday, ""),
                    new(new DateTime(1900, 1, 3), DowCode.Saturday, ""),
                };

                var bulkConfig = new BulkConfig
                {
                    // DateOnly 型は除外
                    PropertiesToExclude = [
                        nameof(ReservationEquipmentSlot.StartDate),
                        nameof(ReservationEquipmentSlot.EndDate),
                    ],
                };

                await context.BulkInsertAsync(equipmentSlots, bulkConfig);
                count += equipmentSlots.Length;
            }

            if (!context.Floors.Any())
            {
                this._logger.LogInformation("Floors creating...");

                var floors = new ReservationFloor[]
                {
                    new("1", "8階", "メインフロアです。", 1),
                    new("2", "7階(♀)", "レディースフロアです。", 2),
                    new("3", "巡回1", "バス健診用(1号車)です。", 3),
                    new("4", "巡回2", "バス健診用(2号車)です。", 4),
                    new("9", "ダミー", "ダミーです。", 9),
                };

                await context.BulkInsertAsync(floors);
                count += floors.Length;
            }

            if (!context.Rooms.Any())
            {
                this._logger.LogInformation("Rooms creating...");

                var rooms = new ReservationRoom[]
                {
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
                };

                await context.BulkInsertAsync(rooms);
                count += rooms.Length;
            }

            if (!context.Holidays.Any())
            {
                this._logger.LogInformation("Holidays creating...");

                var holidays = new Holiday[]
                {
                    new(new DateTime(2025,1,1), "元旦"),
                    new(new DateTime(2025,1,13), "成人の日"),
                    new(new DateTime(2025,2,11), "建国記念日"),
                    new(new DateTime(2025,2,23), "天皇誕生日"),
                    new(new DateTime(2025,2,24), "振替休日"),
                    new(new DateTime(2025,3,20), "春分の日"),
                    new(new DateTime(2025,4,29), "昭和の日"),
                    new(new DateTime(2025,5,3), "憲法記念日"),
                    new(new DateTime(2025,5,4), "みどりの日"),
                    new(new DateTime(2025,5,5), "こどもの日"),
                    new(new DateTime(2025,5,6), "振替休日"),
                    new(new DateTime(2025,7,21), "海の日"),
                    new(new DateTime(2025,8,11), "山の日"),
                    new(new DateTime(2025,9,15), "敬老の日"),
                    new(new DateTime(2025,9,23), "秋分の日"),
                    new(new DateTime(2025,10,13), "スポーツの日"),
                    new(new DateTime(2025,11,3), "文化の日"),
                    new(new DateTime(2025,11,23), "勤労感謝の日"),
                    new(new DateTime(2025,11,14), "振替休日"),

                    new(new DateTime(2026,1,1), "元旦"),
                    new(new DateTime(2026,1,12), "成人の日"),
                    new(new DateTime(2026,2,11), "建国記念日"),
                    new(new DateTime(2026,2,23), "天皇誕生日"),
                    new(new DateTime(2026,3,20), "春分の日"),
                    new(new DateTime(2026,4,29), "昭和の日"),
                    new(new DateTime(2026,5,3), "憲法記念日"),
                    new(new DateTime(2026,5,4), "みどりの日"),
                    new(new DateTime(2026,5,5), "こどもの日"),
                    new(new DateTime(2026,5,6), "振替休日"),
                    new(new DateTime(2026,7,20), "海の日"),
                    new(new DateTime(2026,8,11), "山の日"),
                    new(new DateTime(2026,9,21), "敬老の日"),
                    new(new DateTime(2026,9,22), "振替休日"),
                    new(new DateTime(2026,9,23), "秋分の日"),
                    new(new DateTime(2026,10,12), "スポーツの日"),
                    new(new DateTime(2026,11,3), "文化の日"),
                    new(new DateTime(2026,11,23), "勤労感謝の日"),
                };

                await context.BulkInsertAsync(holidays);
                count += holidays.Length;
            }

            if (!context.DailySlots.Any())
            {
                this._logger.LogInformation("DailySlots creating...");

                var dailySlots = new ReservationDailySlot[]
                {
                    new(new DateTime(1900,1,1), DowCode.None, "09:00 09:30 10:00 10:30 11:00 11:30 13:00 13:30 14:00 14:30 15:00 15:30"),
                    new(new DateTime(1900,1,2), DowCode.Sunday, ""),
                    new(new DateTime(1900,1,3), DowCode.Saturday, ""),
                };

                var bulkConfig = new BulkConfig
                {
                    // DateOnly 型は除外
                    PropertiesToExclude = [
                        nameof(ReservationDailySlot.StartDate),
                        nameof(ReservationDailySlot.EndDate),
                    ],
                };

                await context.BulkInsertAsync(dailySlots, bulkConfig);
                count += dailySlots.Length;
            }

            if (!context.DailyCaps.Any())
            {
                this._logger.LogInformation("DailyCaps creating...");

                var caps = new ReservationDailyCap[]
                {
                    new(new DateTime(1900,1,1), DowCode.None, 40, 30),
                    new(new DateTime(1900,1,2), DowCode.Sunday, 0, 0),
                    new(new DateTime(1900,1,3), DowCode.Saturday, 0, 0),
                };

                var bulkConfig = new BulkConfig
                {
                    // DateOnly 型は除外
                    PropertiesToExclude = [
                        nameof(ReservationDailyCap.StartDate),
                        nameof(ReservationDailyCap.EndDate),
                    ],
                };

                await context.BulkInsertAsync(caps, bulkConfig);
                count += caps.Length;
            }

            if (!context.DailyNotes.Any())
            {
                this._logger.LogInformation("DailyNotes creating...");

                var firstDate = DateHelper.GetFirstDateTime();

                var floors = await context.Floors
                    .AsNoTracking()
                    .ToArrayAsync();
                var floorMax = floors.Length;

                var len1 = 300;
                var len2 = 600;

                var notes = new List<ReservationDailyNote>(len2);

                // フロア指定なし
                for (var i = 0; i < len1; i++)
                {
                    var date = firstDate.AddDays(rnd.Next(0, 180));
                    var note = new ReservationDailyNote(date, 0, $"note_{i}", $"user_{i}");
                    notes.Add(note);
                }

                // フロア指定あり
                for (var i = len1; i < len2; i++)
                {
                    var date = firstDate.AddDays(rnd.Next(0, 180));
                    var floorId = floors.Skip(rnd.Next(0, floorMax)).First().FloorId;
                    var note = new ReservationDailyNote(date, floorId, $"note_{i}(floorId={floorId})", $"user_{i}");
                    notes.Add(note);
                }

                await context.BulkInsertAsync(notes);
                count += notes.Count;
            }

            if (!context.DailyBookings.Any())
            {
                this._logger.LogInformation("DailyBookings creating...");

                var firstDate = DateHelper.GetFirstDateTime();

                var floors = context.Floors
                    .AsNoTracking()
                    .ToList();
                var floorMax = floors.Count;

                var max = 3000;
                var bookings = new List<ReservationDailyBooking>(max);
                for (var i = 0; i < max; i++)
                {
                    var date = firstDate.AddDays(rnd.Next(0, 60));
                    var floorId = floors.Skip(rnd.Next(0, floorMax)).First().FloorId;
                    var slot = slots[rnd.Next(0, slotMax)];
                    var remark = $"remark_{i}";
                    var booking = new ReservationDailyBooking(
                        date,
                        floorId,
                        slot,
                        remark,
                        true);
                    bookings.Add(booking);
                }

                await context.BulkInsertAsync(bookings);
                count += bookings.Count;
            }

            if (!context.EquipmentBookings.Any())
            {
                this._logger.LogInformation("EquipmentBookings creating...");

                var equipments = context.Equipments.AsNoTracking().ToList();
                var equipmentMax = equipments.Count;

                var slotStrings = Seeder.CreateSlotStrings(slots);

                var symbols = new[] { "", "鼻", "口", "★" };
                var symbolMax = symbols.Length;

                var firstDate = DateHelper.GetFirstDateTime();

                var max = 3000;
                var bookings = new List<ReservationEquipmentBooking>(max);
                for (var i = 0; i < max; i++)
                {
                    var equipId = equipments.Skip(rnd.Next(0, equipmentMax)).First().EquipId;
                    var date = firstDate.AddDays(rnd.Next(0, 60));
                    var slot = slotStrings[rnd.Next(0, slotMax)];
                    var symbol = symbols[rnd.Next(0, symbolMax)];
                    var remark = $"remark_{i}";
                    var booking = new ReservationEquipmentBooking(
                        equipId,
                        date,
                        slot,
                        symbol,
                        remark,
                        true);
                    bookings.Add(booking);
                }

                await context.BulkInsertAsync(bookings);
                count += bookings.Count;
            }

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
