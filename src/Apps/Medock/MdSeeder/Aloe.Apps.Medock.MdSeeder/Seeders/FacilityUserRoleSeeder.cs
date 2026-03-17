// <copyright file="FacilityUserRoleSeeder.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Data;
using Aloe.Apps.Medock.MdLauncherLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdSeeder.Seeders;

internal static class FacilityUserRoleSeeder
{
    public static async Task SeedAsync(
        MdLauncherDbContext db,
        Dictionary<string, Guid> facilityUserIds)
    {
        var existing = await db.FacilityUserRoles
            .IgnoreQueryFilters()
            .Select(r => new { r.FacilityUserId, r.RoleCode })
            .ToListAsync();

        var existingSet = existing
            .Select(r => (r.FacilityUserId, r.RoleCode))
            .ToHashSet();

        var assignments = new (string UserCode, string RoleCode)[]
        {
            ("admin", "ADMIN"),
            ("user01", "USER"),
        };

        var toInsert = new List<FacilityUserRoleEntity>();

        foreach (var (userCode, roleCode) in assignments)
        {
            if (!facilityUserIds.TryGetValue(userCode, out var facilityUserId))
            {
                continue;
            }

            if (!existingSet.Contains((facilityUserId, roleCode)))
            {
                toInsert.Add(new FacilityUserRoleEntity
                {
                    FacilityUserRoleId = Guid.CreateVersion7(),
                    FacilityUserId = facilityUserId,
                    RoleCode = roleCode,
                });
            }
        }

        if (toInsert.Count == 0)
        {
            Console.Write("SKIP ");
            return;
        }

        db.FacilityUserRoles.AddRange(toInsert);
        await db.SaveChangesAsync();
        Console.Write($"{toInsert.Count} inserted ");
    }
}
