// <copyright file="FacilityUserSeeder.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Data;
using Aloe.Apps.Medock.MdLauncherLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdSeeder.Seeders;

internal static class FacilityUserSeeder
{
    /// <summary>
    /// 両 DB で共通の施設 ID。
    /// </summary>
    internal static readonly Guid WellKnownFacilityId = new("00000000-0000-0000-0000-000000000001");

    public static async Task<Dictionary<string, Guid>> SeedAsync(
        MdLauncherDbContext db,
        Dictionary<string, Guid> userIds)
    {
        var existing = await db.FacilityUsers
            .IgnoreQueryFilters()
            .Where(fu => fu.FacilityId == WellKnownFacilityId)
            .ToDictionaryAsync(fu => fu.UserId, fu => fu.FacilityUserId);

        var result = new Dictionary<string, Guid>();
        var toInsert = new List<FacilityUserEntity>();

        foreach (var (userCode, userId) in userIds)
        {
            if (existing.TryGetValue(userId, out var facilityUserId))
            {
                result[userCode] = facilityUserId;
            }
            else
            {
                var entity = new FacilityUserEntity
                {
                    FacilityUserId = Guid.CreateVersion7(),
                    FacilityId = WellKnownFacilityId,
                    UserId = userId,
                };
                toInsert.Add(entity);
                result[userCode] = entity.FacilityUserId;
            }
        }

        if (toInsert.Count == 0)
        {
            Console.Write("SKIP ");
        }
        else
        {
            db.FacilityUsers.AddRange(toInsert);
            await db.SaveChangesAsync();
            Console.Write($"{toInsert.Count} inserted ");
        }

        return result;
    }
}
