// <copyright file="RoleMenuSeeder.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Data;
using Aloe.Apps.Medock.MdLauncherLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdSeeder.Seeders;

internal static class RoleMenuSeeder
{
    public static async Task SeedAsync(MdLauncherDbContext db)
    {
        var existing = await db.RoleMenus
            .IgnoreQueryFilters()
            .Select(rm => new { rm.RoleCode, rm.MenuCode })
            .ToListAsync();

        var existingSet = existing
            .Select(rm => (rm.RoleCode, rm.MenuCode))
            .ToHashSet();

        var now = DateTime.UtcNow;

        var assignments = new (string RoleCode, string MenuCode)[]
        {
            ("ADMIN", "PTFIND"),
            ("ADMIN", "PTVIEW"),
            ("ADMIN", "PTEDIT"),
            ("USER", "PTFIND"),
            ("USER", "PTVIEW"),
        };

        var toInsert = new List<RoleMenuEntity>();

        foreach (var (roleCode, menuCode) in assignments)
        {
            if (!existingSet.Contains((roleCode, menuCode)))
            {
                toInsert.Add(new RoleMenuEntity
                {
                    RoleMenuId = Guid.CreateVersion7(),
                    RoleCode = roleCode,
                    MenuCode = menuCode,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }
        }

        if (toInsert.Count == 0)
        {
            Console.Write("SKIP ");
            return;
        }

        db.RoleMenus.AddRange(toInsert);
        await db.SaveChangesAsync();
        Console.Write($"{toInsert.Count} inserted ");
    }
}
