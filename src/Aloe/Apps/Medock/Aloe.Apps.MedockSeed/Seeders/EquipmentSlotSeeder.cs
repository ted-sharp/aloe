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

                foreach (var equipment in equipments)
                {
                    string slotsJson;
                    
                    // 腹部エコーはAM/PMで大枠スロット
                    if (equipment.EquipName.Contains("腹部エコー") || equipment.EquipDesc.Contains("腹部"))
                    {
                        slotsJson = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            slots = new[]
                            {
                                new { time = "09:00", max = 5, duration = 180 }, // AM枠（9:00-12:00、3時間）
                                new { time = "13:00", max = 5, duration = 240 }, // PM枠（13:00-17:00、4時間）
                            }
                        });
                    }
                    else
                    {
                        // その他の設備は通常のスロット
                        slotsJson = System.Text.Json.JsonSerializer.Serialize(new
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
                    }

                    equipmentSlots.Add(new EquipmentSlot
                    {
                        EquipSlotId = Guid.NewGuid(),
                        EquipId = equipment.EquipId,
                        EquipSlots = slotsJson,
                        IsActive = true,
                        ActiveFrom = DateOnly.FromDateTime(dateTimeProvider.Today.AddYears(-1)),
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
                Console.WriteLine($"  [+] EquipmentSlots: {equipmentSlots.Count} entries ({equipments.Count} equipments)");
            }
        }
        else
        {
            Console.WriteLine("[SKIP] EquipmentSlots already exist.");
        }
    }
}


