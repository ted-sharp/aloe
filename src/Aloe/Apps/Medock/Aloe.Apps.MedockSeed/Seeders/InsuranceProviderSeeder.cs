using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class InsuranceProviderSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var existingProviders = await context.InsuranceProviders.AnyAsync();
        if (existingProviders)
        {
            Console.WriteLine("[SKIP] InsuranceProviders already exist.");
            return;
        }

        // TODO: DB修正後にデータ生成を有効化
        // データなしでスキップ
        Console.WriteLine("[SKIP] InsuranceProviders: No seed data (skipped - TODO: enable after DB fix).");
        return;

        /* TODO: DB修正後にコメントアウトを解除
        Console.WriteLine("[INFO] Creating insurance provider seed data...");

        var providers = new List<InsuranceProvider>
        {
            // 全国健康保険協会（協会けんぽ）
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 1,
                InsurerCode = "0101",
                InsurerName = "全国健康保険協会",
                InsurerShortName = "協会けんぽ",
                InsurerDesc = "全国健康保険協会（協会けんぽ）",
                InsurerSeq = 1,
                IsDeleted = false
            },
            // 組合健保（主要なもの）
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 2,
                InsurerCode = "0201",
                InsurerName = "関東ITソフトウェア健康保険組合",
                InsurerShortName = "関東IT健保",
                InsurerDesc = "関東ITソフトウェア健康保険組合",
                InsurerSeq = 2,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 2,
                InsurerCode = "0202",
                InsurerName = "東京IT健康保険組合",
                InsurerShortName = "東京IT健保",
                InsurerDesc = "東京IT健康保険組合",
                InsurerSeq = 3,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 2,
                InsurerCode = "0203",
                InsurerName = "関東自動車健康保険組合",
                InsurerShortName = "関東自動車健保",
                InsurerDesc = "関東自動車健康保険組合",
                InsurerSeq = 4,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 2,
                InsurerCode = "0204",
                InsurerName = "関東建設健康保険組合",
                InsurerShortName = "関東建設健保",
                InsurerDesc = "関東建設健康保険組合",
                InsurerSeq = 5,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 2,
                InsurerCode = "0205",
                InsurerName = "関東金属健康保険組合",
                InsurerShortName = "関東金属健保",
                InsurerDesc = "関東金属健康保険組合",
                InsurerSeq = 6,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 2,
                InsurerCode = "0206",
                InsurerName = "関東化学健康保険組合",
                InsurerShortName = "関東化学健保",
                InsurerDesc = "関東化学健康保険組合",
                InsurerSeq = 7,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 2,
                InsurerCode = "0207",
                InsurerName = "関東食品健康保険組合",
                InsurerShortName = "関東食品健保",
                InsurerDesc = "関東食品健康保険組合",
                InsurerSeq = 8,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 2,
                InsurerCode = "0209",
                InsurerName = "関東運輸健康保険組合",
                InsurerShortName = "関東運輸健保",
                InsurerDesc = "関東運輸健康保険組合",
                InsurerSeq = 9,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 2,
                InsurerCode = "0210",
                InsurerName = "関東電気健康保険組合",
                InsurerShortName = "関東電気健保",
                InsurerDesc = "関東電気健康保険組合",
                InsurerSeq = 10,
                IsDeleted = false
            },
            // 国民健康保険組合
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 3,
                InsurerCode = "0301",
                InsurerName = "東京国民健康保険組合",
                InsurerShortName = "東京国保組合",
                InsurerDesc = "東京国民健康保険組合",
                InsurerSeq = 11,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 3,
                InsurerCode = "0302",
                InsurerName = "横浜国民健康保険組合",
                InsurerShortName = "横浜国保組合",
                InsurerDesc = "横浜国民健康保険組合",
                InsurerSeq = 12,
                IsDeleted = false
            },
            // 国保（市区町村）
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 4,
                InsurerCode = "0401",
                InsurerName = "千代田区国民健康保険",
                InsurerShortName = "千代田区国保",
                InsurerDesc = "千代田区国民健康保険",
                InsurerSeq = 13,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 4,
                InsurerCode = "0402",
                InsurerName = "中央区国民健康保険",
                InsurerShortName = "中央区国保",
                InsurerDesc = "中央区国民健康保険",
                InsurerSeq = 14,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 4,
                InsurerCode = "0403",
                InsurerName = "港区国民健康保険",
                InsurerShortName = "港区国保",
                InsurerDesc = "港区国民健康保険",
                InsurerSeq = 15,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 4,
                InsurerCode = "0404",
                InsurerName = "新宿区国民健康保険",
                InsurerShortName = "新宿区国保",
                InsurerDesc = "新宿区国民健康保険",
                InsurerSeq = 16,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 4,
                InsurerCode = "0405",
                InsurerName = "文京区国民健康保険",
                InsurerShortName = "文京区国保",
                InsurerDesc = "文京区国民健康保険",
                InsurerSeq = 17,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 4,
                InsurerCode = "0406",
                InsurerName = "渋谷区国民健康保険",
                InsurerShortName = "渋谷区国保",
                InsurerDesc = "渋谷区国民健康保険",
                InsurerSeq = 18,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 4,
                InsurerCode = "0407",
                InsurerName = "世田谷区国民健康保険",
                InsurerShortName = "世田谷区国保",
                InsurerDesc = "世田谷区国民健康保険",
                InsurerSeq = 19,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 4,
                InsurerCode = "0408",
                InsurerName = "大田区国民健康保険",
                InsurerShortName = "大田区国保",
                InsurerDesc = "大田区国民健康保険",
                InsurerSeq = 20,
                IsDeleted = false
            },
            // その他
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 5,
                InsurerCode = "0501",
                InsurerName = "後期高齢者医療広域連合",
                InsurerShortName = "後期高齢者医療",
                InsurerDesc = "後期高齢者医療広域連合",
                InsurerSeq = 21,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 5,
                InsurerCode = "0502",
                InsurerName = "生活保護",
                InsurerShortName = "生活保護",
                InsurerDesc = "生活保護",
                InsurerSeq = 22,
                IsDeleted = false
            },
            new()
            {
                InsurerId = Guid.CreateVersion7(),
                InsurerTypeCode = 5,
                InsurerCode = "0503",
                InsurerName = "その他",
                InsurerShortName = "その他",
                InsurerDesc = "その他の保険者",
                InsurerSeq = 23,
                IsDeleted = false
            }
        };

        foreach (var provider in providers)
        {
            SeederHelper.InitializeAuditFields(provider, dateTimeProvider);
        }

        context.InsuranceProviders.AddRange(providers);
        Console.WriteLine($"  [+] InsuranceProviders: {providers.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
        // TODO: DB修正後にコメントアウトを解除
        */
    }
}

