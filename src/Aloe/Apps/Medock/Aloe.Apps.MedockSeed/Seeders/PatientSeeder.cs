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
        if (existingPatients)
        {
            Console.WriteLine("[SKIP] Patients already exist.");
            return;
        }
        if (!facilityId.HasValue)
        {
            Console.WriteLine("[SKIP] facilityId is null.");
            return;

        }

        Console.WriteLine("[INFO] Creating patient seed data...");
        var tenant = await context.Tenants.FirstOrDefaultAsync();
        var org = await context.Organizations.FirstOrDefaultAsync();

        if (tenant != null && org != null)
        {
            // TODO: ランダムで作成するジェネレーターがほしい
            var patients = new List<Patient>
            {
                // 20代
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0001",
                    KarteCode = "K001",
                    PtName = "佐藤 健太",
                    PtNameCompat = "佐藤健太",
                    PtNameKatakana = "サトウ ケンタ",
                    PtNameKatakanaCompat = "サトウケンタ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1998, 4, 12),
                    SexCode = 1,
                    PtMemo = "一般健診"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0002",
                    KarteCode = "K002",
                    PtName = "鈴木 美咲",
                    PtNameCompat = "鈴木美咲",
                    PtNameKatakana = "スズキ ミサキ",
                    PtNameKatakanaCompat = "スズキミサキ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1996, 8, 25),
                    SexCode = 2,
                    PtMemo = "一般健診"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0003",
                    KarteCode = "K003",
                    PtName = "高橋 翔太",
                    PtNameCompat = "高橋翔太",
                    PtNameKatakana = "タカハシ ショウタ",
                    PtNameKatakanaCompat = "タカハシショウタ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1999, 11, 3),
                    SexCode = 1,
                    PtMemo = "一般健診"
                },
                // 30代
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0004",
                    KarteCode = "K004",
                    PtName = "田中 太郎",
                    PtNameCompat = "田中太郎",
                    PtNameKatakana = "タナカ タロウ",
                    PtNameKatakanaCompat = "タナカタロウ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1988, 5, 15),
                    SexCode = 1,
                    PtMemo = "特定健診"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0005",
                    KarteCode = "K005",
                    PtName = "山田 花子",
                    PtNameCompat = "山田花子",
                    PtNameKatakana = "ヤマダ ハナコ",
                    PtNameKatakanaCompat = "ヤマダハナコ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1987, 3, 22),
                    SexCode = 2,
                    PtMemo = "特定健診"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0006",
                    KarteCode = "K006",
                    PtName = "伊藤 直樹",
                    PtNameCompat = "伊藤直樹",
                    PtNameKatakana = "イトウ ナオキ",
                    PtNameKatakanaCompat = "イトウナオキ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1990, 7, 8),
                    SexCode = 1,
                    PtMemo = "人間ドック"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0007",
                    KarteCode = "K007",
                    PtName = "中村 由美",
                    PtNameCompat = "中村由美",
                    PtNameKatakana = "ナカムラ ユミ",
                    PtNameKatakanaCompat = "ナカムラユミ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1991, 12, 18),
                    SexCode = 2,
                    PtMemo = "人間ドック"
                },
                // 40代
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0008",
                    KarteCode = "K008",
                    PtName = "小林 誠",
                    PtNameCompat = "小林誠",
                    PtNameKatakana = "コバヤシ マコト",
                    PtNameKatakanaCompat = "コバヤシマコト",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1978, 2, 14),
                    SexCode = 1,
                    PtMemo = "特定健診"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0009",
                    KarteCode = "K009",
                    PtName = "加藤 麻衣",
                    PtNameCompat = "加藤麻衣",
                    PtNameKatakana = "カトウ マイ",
                    PtNameKatakanaCompat = "カトウマイ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1979, 6, 30),
                    SexCode = 2,
                    PtMemo = "特定健診"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0010",
                    KarteCode = "K010",
                    PtName = "吉田 雄一",
                    PtNameCompat = "吉田雄一",
                    PtNameKatakana = "ヨシダ ユウイチ",
                    PtNameKatakanaCompat = "ヨシダユウイチ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1980, 9, 5),
                    SexCode = 1,
                    PtMemo = "人間ドック"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0011",
                    KarteCode = "K011",
                    PtName = "松本 恵子",
                    PtNameCompat = "松本恵子",
                    PtNameKatakana = "マツモト ケイコ",
                    PtNameKatakanaCompat = "マツモトケイコ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1982, 1, 20),
                    SexCode = 2,
                    PtMemo = "人間ドック"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0012",
                    KarteCode = "K012",
                    PtName = "井上 健一",
                    PtNameCompat = "井上健一",
                    PtNameKatakana = "イノウエ ケンイチ",
                    PtNameKatakanaCompat = "イノウエケンイチ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1983, 4, 7),
                    SexCode = 1,
                    PtMemo = "定期健診"
                },
                // 50代
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0013",
                    KarteCode = "K013",
                    PtName = "木村 正雄",
                    PtNameCompat = "木村正雄",
                    PtNameKatakana = "キムラ マサオ",
                    PtNameKatakanaCompat = "キムラマサオ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1968, 10, 12),
                    SexCode = 1,
                    PtMemo = "人間ドック"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0014",
                    KarteCode = "K014",
                    PtName = "林 久美子",
                    PtNameCompat = "林久美子",
                    PtNameKatakana = "ハヤシ クミコ",
                    PtNameKatakanaCompat = "ハヤシクミコ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1970, 7, 28),
                    SexCode = 2,
                    PtMemo = "人間ドック"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0015",
                    KarteCode = "K015",
                    PtName = "斎藤 和彦",
                    PtNameCompat = "斎藤和彦",
                    PtNameKatakana = "サイトウ カズヒコ",
                    PtNameKatakanaCompat = "サイトウカズヒコ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1971, 3, 15),
                    SexCode = 1,
                    PtMemo = "特定健診"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0016",
                    KarteCode = "K016",
                    PtName = "清水 真理",
                    PtNameCompat = "清水真理",
                    PtNameKatakana = "シミズ マリ",
                    PtNameKatakanaCompat = "シミズマリ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1973, 11, 9),
                    SexCode = 2,
                    PtMemo = "特定健診"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0017",
                    KarteCode = "K017",
                    PtName = "山口 敏夫",
                    PtNameCompat = "山口敏夫",
                    PtNameKatakana = "ヤマグチ トシオ",
                    PtNameKatakanaCompat = "ヤマグチトシオ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1974, 5, 22),
                    SexCode = 1,
                    PtMemo = "人間ドック"
                },
                // 60代
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0018",
                    KarteCode = "K018",
                    PtName = "森 一郎",
                    PtNameCompat = "森一郎",
                    PtNameKatakana = "モリ イチロウ",
                    PtNameKatakanaCompat = "モリイチロウ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1960, 8, 3),
                    SexCode = 1,
                    PtMemo = "人間ドック"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0019",
                    KarteCode = "K019",
                    PtName = "池田 静香",
                    PtNameCompat = "池田静香",
                    PtNameKatakana = "イケダ シズカ",
                    PtNameKatakanaCompat = "イケダシズカ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1962, 12, 17),
                    SexCode = 2,
                    PtMemo = "人間ドック"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0020",
                    KarteCode = "K020",
                    PtName = "橋本 清",
                    PtNameCompat = "橋本清",
                    PtNameKatakana = "ハシモト キヨシ",
                    PtNameKatakanaCompat = "ハシモトキヨシ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1963, 6, 25),
                    SexCode = 1,
                    PtMemo = "特定健診"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0021",
                    KarteCode = "K021",
                    PtName = "前田 幸子",
                    PtNameCompat = "前田幸子",
                    PtNameKatakana = "マエダ サチコ",
                    PtNameKatakanaCompat = "マエダサチコ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1964, 2, 11),
                    SexCode = 2,
                    PtMemo = "特定健診"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0022",
                    KarteCode = "K022",
                    PtName = "藤原 博",
                    PtNameCompat = "藤原博",
                    PtNameKatakana = "フジワラ ヒロシ",
                    PtNameKatakanaCompat = "フジワラヒロシ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1965, 9, 30),
                    SexCode = 1,
                    PtMemo = "人間ドック"
                },
                // 70代
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0023",
                    KarteCode = "K023",
                    PtName = "岡田 三郎",
                    PtNameCompat = "岡田三郎",
                    PtNameKatakana = "オカダ サブロウ",
                    PtNameKatakanaCompat = "オカダサブロウ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1952, 4, 18),
                    SexCode = 1,
                    PtMemo = "人間ドック"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0024",
                    KarteCode = "K024",
                    PtName = "長谷川 節子",
                    PtNameCompat = "長谷川節子",
                    PtNameKatakana = "ハセガワ セツコ",
                    PtNameKatakanaCompat = "ハセガワセツコ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1954, 7, 6),
                    SexCode = 2,
                    PtMemo = "人間ドック"
                },
                new()
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0025",
                    KarteCode = "K025",
                    PtName = "村上 義明",
                    PtNameCompat = "村上義明",
                    PtNameKatakana = "ムラカミ ヨシアキ",
                    PtNameKatakanaCompat = "ムラカミヨシアキ",
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1955, 10, 14),
                    SexCode = 1,
                    PtMemo = "定期健診"
                }
            };

            foreach (var patient in patients)
            {
                patient.IsDeleted = false;
                SeederHelper.InitializeAuditFields(patient, dateTimeProvider);
            }

            context.Patients.AddRange(patients);
            Console.WriteLine($"  [+] Patients: {patients.Count} entries");
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}


