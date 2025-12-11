using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class EquipmentSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid? floorId, IDateTimeProvider dateTimeProvider)
    {
        var existingEquipments = await context.Equipments.AnyAsync();
        if (!existingEquipments && floorId.HasValue)
        {
            Console.WriteLine("[INFO] Creating equipment seed data...");
            var equipments = new List<Equipment>
            {
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "CT装置", EquipDesc = "64列マルチスライスCT", EquipSeq = 1 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "MRI装置", EquipDesc = "1.5テスラMRI", EquipSeq = 2 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "内視鏡システム1号機", EquipDesc = "上部消化管内視鏡", EquipSeq = 3 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "内視鏡システム2号機", EquipDesc = "下部消化管内視鏡", EquipSeq = 4 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "内視鏡システム3号機", EquipDesc = "経鼻内視鏡", EquipSeq = 5 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "超音波診断装置1号機", EquipDesc = "腹部エコー用", EquipSeq = 6 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "超音波診断装置2号機", EquipDesc = "心エコー用", EquipSeq = 7 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "超音波診断装置3号機", EquipDesc = "甲状腺エコー用", EquipSeq = 8 },
            };
            foreach (var equipment in equipments)
            {
                equipment.IsDeleted = false;
                equipment.CreatedAt = dateTimeProvider.Now;
                equipment.UpdatedAt = dateTimeProvider.Now;
                equipment.CreatedUserId = Guid.Empty;
                equipment.CreatedSessionId = Guid.Empty;
                equipment.UpdatedUserId = Guid.Empty;
                equipment.UpdatedSessionId = Guid.Empty;
            }
            context.Equipments.AddRange(equipments);
            Console.WriteLine($"  [+] Equipments: {equipments.Count} entries");
        }
        else if (existingEquipments)
        {
            Console.WriteLine("[SKIP] Equipments already exist.");
        }
    }
}


