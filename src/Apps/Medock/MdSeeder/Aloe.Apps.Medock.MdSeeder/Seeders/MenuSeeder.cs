// <copyright file="MenuSeeder.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Data;
using Aloe.Apps.Medock.MdLauncherLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdSeeder.Seeders;

internal static class MenuSeeder
{
    public static async Task SeedAsync(MdLauncherDbContext db)
    {
        var existing = await db.Menus
            .IgnoreQueryFilters()
            .Select(m => m.MenuCode)
            .ToListAsync();

        var now = DateTime.UtcNow;

        var menus = new List<MenuEntity>
        {
            new()
            {
                MenuCode = "PTFIND",
                MenuName = "患者検索",
                MenuSeq = 10,
                AppCode = "PTFINDER",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new()
            {
                MenuCode = "PTVIEW",
                MenuName = "患者参照",
                MenuSeq = 20,
                AppCode = "PTVIEWER",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new()
            {
                MenuCode = "PTEDIT",
                MenuName = "患者編集",
                MenuSeq = 30,
                AppCode = "PTEDITOR",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            },
        };

        var toInsert = menus.Where(m => !existing.Contains(m.MenuCode)).ToList();

        if (toInsert.Count == 0)
        {
            Console.Write("SKIP ");
            return;
        }

        db.Menus.AddRange(toInsert);
        await db.SaveChangesAsync();
        Console.Write($"{toInsert.Count} inserted ");
    }
}
