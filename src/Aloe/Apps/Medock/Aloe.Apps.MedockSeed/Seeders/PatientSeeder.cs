using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class PatientSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid? facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingPatients = await context.Patients.AnyAsync();
        if (!existingPatients && facilityId.HasValue)
        {
            Console.WriteLine("[INFO] Creating patient seed data...");
            var tenant = await context.Tenants.FirstOrDefaultAsync();
            var org = await context.Organizations.FirstOrDefaultAsync();

            if (tenant != null && org != null)
            {
                var patients = new List<Patient>
                {
                    new()
                    {
                        PtId = Guid.NewGuid(),
                        CanonicalPtId = Guid.NewGuid(),
                        FacilityId = facilityId.Value,
                        PrimaryOrgId = org.OrgId,
                        PtCode = "PT0001",
                        KarteCode = "K001",
                        PtName = "田中 太郎",
                        PtNameCompat = "田中太郎",
                        PtNameKatakana = "タナカ タロウ",
                        PtNameKatakanaCompat = "タナカタロウ",
                        PtMaidenName = "",
                        PtAliasName = "",
                        BirthDate = new DateOnly(1965, 5, 15),
                        SexCode = 1,
                        PtMemo = "定期健診患者"
                    },
                    new()
                    {
                        PtId = Guid.NewGuid(),
                        CanonicalPtId = Guid.NewGuid(),
                        FacilityId = facilityId.Value,
                        PrimaryOrgId = org.OrgId,
                        PtCode = "PT0002",
                        KarteCode = "K002",
                        PtName = "山田 花子",
                        PtNameCompat = "山田花子",
                        PtNameKatakana = "ヤマダ ハナコ",
                        PtNameKatakanaCompat = "ヤマダハナコ",
                        PtMaidenName = "",
                        PtAliasName = "",
                        BirthDate = new DateOnly(1972, 3, 22),
                        SexCode = 2,
                        PtMemo = "定期健診患者"
                    },
                    new()
                    {
                        PtId = Guid.NewGuid(),
                        CanonicalPtId = Guid.NewGuid(),
                        FacilityId = facilityId.Value,
                        PrimaryOrgId = org.OrgId,
                        PtCode = "PT0003",
                        KarteCode = "K003",
                        PtName = "佐藤 次郎",
                        PtNameCompat = "佐藤次郎",
                        PtNameKatakana = "サトウ ジロウ",
                        PtNameKatakanaCompat = "サトウジロウ",
                        PtMaidenName = "",
                        PtAliasName = "",
                        BirthDate = new DateOnly(1980, 7, 10),
                        SexCode = 1,
                        PtMemo = "人間ドック患者"
                    },
                    new()
                    {
                        PtId = Guid.NewGuid(),
                        CanonicalPtId = Guid.NewGuid(),
                        FacilityId = facilityId.Value,
                        PrimaryOrgId = org.OrgId,
                        PtCode = "PT0004",
                        KarteCode = "K004",
                        PtName = "鈴木 美咲",
                        PtNameCompat = "鈴木美咲",
                        PtNameKatakana = "スズキ ミサキ",
                        PtNameKatakanaCompat = "スズキミサキ",
                        PtMaidenName = "",
                        PtAliasName = "",
                        BirthDate = new DateOnly(1988, 11, 5),
                        SexCode = 2,
                        PtMemo = "健診患者"
                    },
                    new()
                    {
                        PtId = Guid.NewGuid(),
                        CanonicalPtId = Guid.NewGuid(),
                        FacilityId = facilityId.Value,
                        PrimaryOrgId = org.OrgId,
                        PtCode = "PT0005",
                        KarteCode = "K005",
                        PtName = "高橋 健一",
                        PtNameCompat = "高橋健一",
                        PtNameKatakana = "タカハシ ケンイチ",
                        PtNameKatakanaCompat = "タカハシケンイチ",
                        PtMaidenName = "",
                        PtAliasName = "",
                        BirthDate = new DateOnly(1955, 2, 18),
                        SexCode = 1,
                        PtMemo = "定期健診患者"
                    }
                };

                foreach (var patient in patients)
                {
                    patient.IsDeleted = false;
                    patient.CreatedAt = dateTimeProvider.Now;
                    patient.UpdatedAt = dateTimeProvider.Now;
                    patient.CreatedUserId = Guid.Empty;
                    patient.CreatedSessionId = Guid.Empty;
                    patient.UpdatedUserId = Guid.Empty;
                    patient.UpdatedSessionId = Guid.Empty;
                }

                context.Patients.AddRange(patients);
                Console.WriteLine($"  [+] Patients: {patients.Count} entries");
            }
        }
        else if (existingPatients)
        {
            Console.WriteLine("[SKIP] Patients already exist.");
        }
    }
}


