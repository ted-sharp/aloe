// <copyright file="AppSeeder.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Data;
using Aloe.Apps.Medock.MdLauncherLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdSeeder.Seeders;

internal static class AppSeeder
{
    public static async Task SeedAsync(MdLauncherDbContext db)
    {
        var existing = await db.Apps
            .IgnoreQueryFilters()
            .Select(a => a.AppCode)
            .ToListAsync();

        var now = DateTime.UtcNow;

        var apps = new List<AppEntity>
        {
            new()
            {
                AppCode = "PTFINDER",
                AppName = "患者検索",
                AppSeq = 10,
                AppPath = "Aloe.Apps.Medock.MdPatientFinder.exe",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new()
            {
                AppCode = "PTVIEWER",
                AppName = "患者参照",
                AppSeq = 20,
                AppPath = "Aloe.Apps.Medock.MdPatientViewer.exe",
                PipeName = "MdPatient_Viewer",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new()
            {
                AppCode = "PTEDITOR",
                AppName = "患者編集",
                AppSeq = 30,
                AppPath = "Aloe.Apps.Medock.MdPatientEditor.exe",
                PipeName = "MdPatient_Editor",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            },
        };

        var toInsert = apps.Where(a => !existing.Contains(a.AppCode)).ToList();

        if (toInsert.Count == 0)
        {
            Console.Write("SKIP ");
            return;
        }

        db.Apps.AddRange(toInsert);
        await db.SaveChangesAsync();
        Console.Write($"{toInsert.Count} inserted ");
    }
}
