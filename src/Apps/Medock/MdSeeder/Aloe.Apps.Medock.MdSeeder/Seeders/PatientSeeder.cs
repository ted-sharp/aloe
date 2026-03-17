// <copyright file="PatientSeeder.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdPatientLib.Data;
using Aloe.Apps.Medock.MdPatientLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdSeeder.Seeders;

internal static class PatientSeeder
{
    public static async Task SeedAsync(MdPatientDbContext db)
    {
        var existing = await db.Patients
            .IgnoreQueryFilters()
            .Select(p => p.PtCode)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var facilityId = FacilityUserSeeder.WellKnownFacilityId;
        var orgId = Guid.Empty;

        var patients = new List<Patient>
        {
            CreatePatient("P0001", "山田 太郎", "ヤマダ タロウ", new DateOnly(1980, 1, 15), 1, facilityId, orgId, now),
            CreatePatient("P0002", "鈴木 花子", "スズキ ハナコ", new DateOnly(1975, 6, 20), 2, facilityId, orgId, now),
            CreatePatient("P0003", "田中 一郎", "タナカ イチロウ", new DateOnly(1990, 3, 10), 1, facilityId, orgId, now),
            CreatePatient("P0004", "佐藤 美咲", "サトウ ミサキ", new DateOnly(2000, 12, 25), 2, facilityId, orgId, now),
            CreatePatient("P0005", "高橋 健太", "タカハシ ケンタ", new DateOnly(1965, 9, 5), 1, facilityId, orgId, now),
        };

        var toInsert = patients.Where(p => !existing.Contains(p.PtCode)).ToList();

        if (toInsert.Count == 0)
        {
            Console.Write("SKIP ");
            return;
        }

        db.Patients.AddRange(toInsert);
        await db.SaveChangesAsync();
        Console.Write($"{toInsert.Count} inserted ");
    }

    private static Patient CreatePatient(
        string ptCode,
        string ptName,
        string ptNameKatakana,
        DateOnly birthDate,
        int sexCode,
        Guid facilityId,
        Guid orgId,
        DateTime now)
    {
        var ptId = Guid.CreateVersion7();
        return new Patient
        {
            PtId = ptId,
            CanonicalPtId = ptId,
            FacilityId = facilityId,
            PrimaryOrgId = orgId,
            PtCode = ptCode,
            PtName = ptName,
            PtNameCompat = ptName,
            PtNameKatakana = ptNameKatakana,
            PtNameKatakanaCompat = ptNameKatakana,
            BirthDate = birthDate,
            SexCode = sexCode,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }
}
