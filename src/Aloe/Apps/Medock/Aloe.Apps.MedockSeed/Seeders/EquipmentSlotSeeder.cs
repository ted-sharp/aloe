using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class EquipmentSlotSeeder
{
    public static async Task SeedAsync(MedockDbContext context)
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
                    var slotsJson = System.Text.Json.JsonSerializer.Serialize(new
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

                    equipmentSlots.Add(new EquipmentSlot
                    {
                        EquipSlotId = Guid.NewGuid(),
                        EquipId = equipment.EquipId,
                        EquipSlots = slotsJson,
                        IsActive = true,
                        ActiveFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
                        ActiveTo = new DateOnly(9999, 12, 31),
                        IsDeleted = false,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
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


