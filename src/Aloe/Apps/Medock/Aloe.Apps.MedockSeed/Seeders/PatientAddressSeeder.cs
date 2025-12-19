using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class PatientAddressSeeder
{
    private static readonly Random _random = new Random();

    // 郵便番号のサンプル（東京都内）
    private static readonly string[] PostalCodes =
    [
        "1000001", "1000002", "1000003", "1000004", "1000005",
        "1000006", "1000007", "1000008", "1000009", "1000010",
        "1000011", "1000012", "1000013", "1000014", "1000015",
        "1500001", "1500002", "1500003", "1500004", "1500005",
        "1600001", "1600002", "1600003", "1600004", "1600005",
    ];

    // 都道府県
    private static readonly string[] Prefectures =
    [
        "東京都", "神奈川県", "埼玉県", "千葉県", "大阪府", "兵庫県", "愛知県", "福岡県"
    ];

    // 市区町村
    private static readonly string[] Cities =
    [
        "千代田区", "中央区", "港区", "新宿区", "文京区",
        "台東区", "墨田区", "江東区", "品川区", "目黒区",
        "大田区", "世田谷区", "渋谷区", "中野区", "杉並区",
        "横浜市", "川崎市", "さいたま市", "千葉市", "大阪市"
    ];

    // 町名・番地のサンプル
    private static readonly string[] StreetNames =
    [
        "1丁目", "2丁目", "3丁目", "4丁目", "5丁目",
        "1番地", "2番地", "3番地", "4番地", "5番地",
        "1-1", "1-2", "1-3", "2-1", "2-2", "3-1", "3-2"
    ];

    // 建物名
    private static readonly string[] BuildingNames =
    [
        "", "", "", "", "", // 建物名なしも多い
        "マンション", "アパート", "ビル", "ハイツ", "コーポ",
        "○○マンション", "○○アパート", "○○ビル", "○○ハイツ"
    ];

    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingAddresses = await context.PatientAddresses.AnyAsync();
        if (existingAddresses)
        {
            Console.WriteLine("[SKIP] PatientAddresses already exist.");
            return;
        }

        var patients = await context.Patients
            .Where(p => !p.IsDeleted && p.FacilityId == facilityId)
            .ToListAsync();

        if (!patients.Any())
        {
            Console.WriteLine("[SKIP] PatientAddress: No patients found.");
            return;
        }

        Console.WriteLine("[INFO] Creating patient address seed data...");

        var addresses = new List<PatientAddress>();

        foreach (var patient in patients)
        {
            // 80%の患者に1件、20%の患者に2件の住所を生成
            var addressCount = _random.Next(100) < 80 ? 1 : 2;

            for (int i = 0; i < addressCount; i++)
            {
                var postalCode = PostalCodes[_random.Next(PostalCodes.Length)];
                var prefecture = Prefectures[_random.Next(Prefectures.Length)];
                var city = Cities[_random.Next(Cities.Length)];
                var street = StreetNames[_random.Next(StreetNames.Length)];
                var building = BuildingNames[_random.Next(BuildingNames.Length)];

                var adr1 = prefecture + city;
                var adr2 = street;
                var adr3 = building;

                // 電話番号を生成（03-XXXX-XXXX形式）
                var tel = $"0{_random.Next(2, 10)}-{_random.Next(1000, 10000)}-{_random.Next(1000, 10000)}";

                // メールアドレスを生成（50%の確率）
                var email = _random.Next(100) < 50
                    ? $"patient{patient.PtId.ToString("N")[..8]}@example.com"
                    : String.Empty;

                var address = new PatientAddress
                {
                    PtAdrId = Guid.CreateVersion7(),
                    PtId = patient.PtId,
                    AdrTypeCode = i == 0 ? 1 : 2, // 1=現住所、2=連絡先
                    PostalCode = postalCode,
                    Adr1 = adr1,
                    Adr2 = adr2,
                    Adr3 = adr3,
                    AttentionName = String.Empty,
                    Tel = tel,
                    Tel2 = String.Empty,
                    Fax = String.Empty,
                    Email = email,
                    AdrMemo = String.Empty,
                    AdrSeq = i + 1,
                    IsDeleted = false
                };

                SeederHelper.InitializeAuditFields(address, dateTimeProvider);
                addresses.Add(address);
            }
        }

        context.PatientAddresses.AddRange(addresses);
        Console.WriteLine($"  [+] PatientAddresses: {addresses.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}

