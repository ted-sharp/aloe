using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class EquipmentSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid? floorId)
    {
        var existingEquipments = await context.Equipments.AnyAsync();
        if (!existingEquipments && floorId.HasValue)
        {
            Console.WriteLine("[INFO] Creating equipment seed data...");
            var equipments = new List<Equipment>
            {
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "X線撮影装置1号機", EquipDesc = "胸部X線撮影用", EquipSeq = 1 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "X線撮影装置2号機", EquipDesc = "胸部X線撮影用（予備機）", EquipSeq = 2 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "CT装置", EquipDesc = "64列マルチスライスCT", EquipSeq = 3 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "MRI装置", EquipDesc = "1.5テスラMRI", EquipSeq = 4 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "超音波診断装置1号機", EquipDesc = "腹部エコー用", EquipSeq = 5 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "超音波診断装置2号機", EquipDesc = "心エコー用", EquipSeq = 6 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "内視鏡システム1号機", EquipDesc = "上部消化管内視鏡", EquipSeq = 7 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "内視鏡システム2号機", EquipDesc = "下部消化管内視鏡", EquipSeq = 8 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "心電計1号機", EquipDesc = "12誘導心電図", EquipSeq = 9 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "心電計2号機", EquipDesc = "12誘導心電図（予備機）", EquipSeq = 10 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "肺機能検査装置", EquipDesc = "スパイロメトリー", EquipSeq = 11 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "眼底カメラ", EquipDesc = "デジタル眼底撮影", EquipSeq = 12 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "聴力検査装置", EquipDesc = "オージオメーター", EquipSeq = 13 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "血液検査装置", EquipDesc = "自動血球計数器", EquipSeq = 14 },
                new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "生化学自動分析装置", EquipDesc = "血液生化学検査用", EquipSeq = 15 },
            };
            foreach (var equipment in equipments)
            {
                equipment.IsDeleted = false;
                equipment.CreatedAt = DateTimeOffset.UtcNow;
                equipment.UpdatedAt = DateTimeOffset.UtcNow;
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

