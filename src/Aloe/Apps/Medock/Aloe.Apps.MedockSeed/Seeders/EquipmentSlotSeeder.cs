using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class EquipmentSlotSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var existingEquipmentSlots = await context.EquipmentSlots.AnyAsync();
        if (!existingEquipmentSlots)
        {
            Console.WriteLine("[INFO] Creating equipment slot seed data...");
            var equipments = await context.Equipments.Where(e => !e.IsDeleted).ToListAsync();

            if (equipments.Any())
            {
                var equipmentSlots = new List<EquipmentSlot>();
                var rangeStart = dateTimeProvider.TodayDateOnly.AddYears(-3);
                var changeDate = dateTimeProvider.TodayDateOnly.AddYears(-1);

                foreach (var equipment in equipments)
                {
                    string slotsJsonV1;
                    string slotsJsonV2;
                    
                    // 腹部エコーはAM/PMで大枠スロット（v1/v2でmaxや開始時刻を変える）
                    if (equipment.EquipName.Contains("腹部エコー") || equipment.EquipDesc.Contains("腹部"))
                    {
                        slotsJsonV1 = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            slots = new[]
                            {
                                new { time = "09:00", max = 4, duration = 180 }, // AM枠（9:00-12:00、3時間）
                                new { time = "13:00", max = 4, duration = 240 }, // PM枠（13:00-17:00、4時間）
                            }
                        });

                        slotsJsonV2 = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            slots = new[]
                            {
                                new { time = "08:30", max = 6, duration = 210 }, // AM枠（8:30-12:00、3.5時間相当）
                                new { time = "13:00", max = 6, duration = 240 }, // PM枠（13:00-17:00、4時間）
                            }
                        });
                    }
                    else
                    {
                        // その他の設備は通常スロット（v1/v2でmaxや刻みを変更）
                        slotsJsonV1 = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            slots = new[]
                            {
                                new { time = "08:00", max = 1, duration = 30 },
                                new { time = "09:00", max = 2, duration = 30 },
                                new { time = "10:00", max = 2, duration = 30 },
                                new { time = "11:00", max = 2, duration = 30 },
                                new { time = "13:00", max = 2, duration = 30 },
                                new { time = "14:00", max = 2, duration = 30 },
                                new { time = "15:00", max = 2, duration = 30 },
                                new { time = "16:00", max = 1, duration = 30 },
                            }
                        });

                        slotsJsonV2 = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            slots = new[]
                            {
                                new { time = "09:00", max = 3, duration = 30 },
                                new { time = "09:30", max = 3, duration = 30 },
                                new { time = "10:00", max = 3, duration = 30 },
                                new { time = "10:30", max = 3, duration = 30 },
                                new { time = "11:00", max = 2, duration = 30 },
                                new { time = "13:00", max = 3, duration = 30 },
                                new { time = "13:30", max = 3, duration = 30 },
                                new { time = "14:00", max = 3, duration = 30 },
                                new { time = "14:30", max = 3, duration = 30 },
                                new { time = "15:00", max = 2, duration = 30 },
                                new { time = "16:00", max = 1, duration = 30 },
                            }
                        });
                    }

                    // v1（過去3年開始〜変更前日）
                    equipmentSlots.Add(new EquipmentSlot
                    {
                        EquipSlotId = Guid.NewGuid(),
                        EquipId = equipment.EquipId,
                        EquipSlots = slotsJsonV1,
                        IsActive = false,
                        ActiveFrom = rangeStart,
                        ActiveTo = changeDate.AddDays(-1),
                        IsDeleted = false,
                        CreatedAt = dateTimeProvider.Now,
                        UpdatedAt = dateTimeProvider.Now,
                        CreatedUserId = Guid.Empty,
                        CreatedSessionId = Guid.Empty,
                        UpdatedUserId = Guid.Empty,
                        UpdatedSessionId = Guid.Empty
                    });

                    // v2（変更日〜）
                    equipmentSlots.Add(new EquipmentSlot
                    {
                        EquipSlotId = Guid.NewGuid(),
                        EquipId = equipment.EquipId,
                        EquipSlots = slotsJsonV2,
                        IsActive = true,
                        ActiveFrom = changeDate,
                        ActiveTo = new DateOnly(9999, 12, 31),
                        IsDeleted = false,
                        CreatedAt = dateTimeProvider.Now,
                        UpdatedAt = dateTimeProvider.Now,
                        CreatedUserId = Guid.Empty,
                        CreatedSessionId = Guid.Empty,
                        UpdatedUserId = Guid.Empty,
                        UpdatedSessionId = Guid.Empty
                    });
                }

                context.EquipmentSlots.AddRange(equipmentSlots);
                Console.WriteLine($"  [+] EquipmentSlots: {equipmentSlots.Count} entries ({equipments.Count} equipments × 2 versions)");
            }
        }
        else
        {
            Console.WriteLine("[SKIP] EquipmentSlots already exist.");
        }
    }
}


