using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class PatientSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingPatients = await context.Patients.AnyAsync();
        if (existingPatients)
        {
            Console.WriteLine("[SKIP] Patients already exist.");
            return;
        }

        Console.WriteLine("[INFO] Creating patient seed data...");
        var tenant = await context.Tenants.FirstOrDefaultAsync();
        var org = await context.Organizations.FirstOrDefaultAsync();

        if (tenant != null && org != null)
        {
            var random = new Random();
            var patients = new List<Patient>(1000);

            // 現在の日付を基準に年齢層を定義（2025年基準）
            var today = DateOnly.FromDateTime(DateTime.Today);
            var ageRanges = new[]
            {
                (Start: today.AddYears(-29), End: today.AddYears(-20)), // 20代
                (Start: today.AddYears(-39), End: today.AddYears(-30)), // 30代
                (Start: today.AddYears(-49), End: today.AddYears(-40)), // 40代
                (Start: today.AddYears(-59), End: today.AddYears(-50)), // 50代
                (Start: today.AddYears(-69), End: today.AddYears(-60)), // 60代
                (Start: today.AddYears(-79), End: today.AddYears(-70)), // 70代
            };

            for (int i = 1; i <= 1000; i++)
            {
                // 年齢層をランダムに選択
                var ageRange = ageRanges[random.Next(ageRanges.Length)];
                var daysDiff = (ageRange.End.ToDateTime(TimeOnly.MinValue) - ageRange.Start.ToDateTime(TimeOnly.MinValue)).Days;
                var randomDays = random.Next(daysDiff);
                var birthDate = ageRange.Start.AddDays(randomDays);

                // 性別をランダムに割り当て（1: 男性、2: 女性）
                var sexCode = random.Next(2) == 0 ? 1 : 2;

                // 患者名を生成
                var (name, nameCompat, katakana, katakanaCompat) = NameGenerator.GeneratePatientName(random);

                // コードを生成
                var ptCode = $"PT{i:D4}";
                var karteCode = $"K{i:D3}";

                // メモをランダムに選択
                var memo = NameGenerator.GetRandomPatientMemo(random);

                patients.Add(new Patient
                {
                    PtId = Guid.CreateVersion7(),
                    CanonicalPtId = Guid.CreateVersion7(),
                    FacilityId = facilityId,
                    PrimaryOrgId = org.OrgId,
                    PtCode = ptCode,
                    KarteCode = karteCode,
                    PtName = name,
                    PtNameCompat = nameCompat,
                    PtNameKatakana = katakana,
                    PtNameKatakanaCompat = katakanaCompat,
                    PtMaidenName = "",
                    PtAliasName = "",
                    BirthDate = birthDate,
                    SexCode = sexCode,
                    PtMemo = memo
                });
            }

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


