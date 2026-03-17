// <copyright file="UserSeeder.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Data;
using Aloe.Apps.Medock.MdLauncherLib.Data.Entities;
using Aloe.Apps.Medock.MdLauncherLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdSeeder.Seeders;

internal static class UserSeeder
{
    public static async Task<Dictionary<string, Guid>> SeedAsync(MdLauncherDbContext db)
    {
        var existing = await db.Users
            .IgnoreQueryFilters()
            .ToDictionaryAsync(u => u.UserCode, u => u.UserId);

        var userCodes = new[] { "admin", "user01" };
        var toInsert = new List<UserEntity>();

        foreach (var code in userCodes)
        {
            if (!existing.ContainsKey(code))
            {
                var (hash, salt) = PasswordHasher.Default.HashPassword("password");
                var entity = new UserEntity
                {
                    UserId = Guid.CreateVersion7(),
                    UserCode = code,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                };
                toInsert.Add(entity);
                existing[code] = entity.UserId;
            }
        }

        if (toInsert.Count == 0)
        {
            Console.Write("SKIP ");
        }
        else
        {
            db.Users.AddRange(toInsert);
            await db.SaveChangesAsync();
            Console.Write($"{toInsert.Count} inserted ");
        }

        return existing
            .Where(kv => userCodes.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
