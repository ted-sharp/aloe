using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class UserSeeder
{
    public static async Task<bool> SeedAsync(MedockDbContext context, PasswordHasher passwordHasher, IDateTimeProvider dateTimeProvider)
    {
        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.UserCode == "admin");
        var needsUserSeed = existingAdmin == null;

        if (needsUserSeed)
        {
            Console.WriteLine("[INFO] Creating user seed data...");
            Console.WriteLine();

            var userId = Guid.NewGuid();
            var (hash, salt) = passwordHasher.HashPassword("admin");
            var user = new User
            {
                UserId = userId,
                UserCode = "admin",
                Email = "admin@example.com",
                UserDisplayName = "システム管理者",
                PasswordHash = hash,
                PasswordSalt = salt,
                IsSystemAdmin = true,
                IsDeleted = false,
                CreatedAt = dateTimeProvider.Now,
                UpdatedAt = dateTimeProvider.Now,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            context.Users.Add(user);
            Console.WriteLine($"  [+] User: {user.UserDisplayName} - {user.UserCode} ({user.UserId})");
            Console.WriteLine($"      Email: {user.Email}");
            Console.WriteLine($"      Password: admin (hashed)");
            Console.WriteLine($"      IsSystemAdmin: {user.IsSystemAdmin}");

            var existingFacilityForUser = await context.Facilities.FirstOrDefaultAsync();
            if (existingFacilityForUser != null)
            {
                var facilityUser = new FacilityUser
                {
                    FacilityUserId = Guid.NewGuid(),
                    FacilityId = existingFacilityForUser.FacilityId,
                    UserId = userId,
                    FacilityUserSeq = 1,
                    IsFacilityAdmin = true,
                    IsDeleted = false,
                    CreatedAt = dateTimeProvider.Now,
                    UpdatedAt = dateTimeProvider.Now,
                    CreatedUserId = Guid.Empty,
                    CreatedSessionId = Guid.Empty,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                };
                context.FacilityUsers.Add(facilityUser);
                Console.WriteLine($"  [+] FacilityUser: UserId: {userId} (FacilityId: {existingFacilityForUser.FacilityId})");
            }
        }
        else
        {
            Console.WriteLine("[SKIP] Admin user already exists. Skipping user seed.");
        }

        var adminUser = existingAdmin ?? await context.Users.FirstOrDefaultAsync(u => u.UserCode == "admin");
        var existingFacilityUser = await context.FacilityUsers.AnyAsync();
        if (adminUser != null && !existingFacilityUser)
        {
            var firstFacility = await context.Facilities.FirstOrDefaultAsync();
            if (firstFacility != null)
            {
                Console.WriteLine("[INFO] Creating facility user for existing admin...");
                var facilityUser = new FacilityUser
                {
                    FacilityUserId = Guid.NewGuid(),
                    FacilityId = firstFacility.FacilityId,
                    UserId = adminUser.UserId,
                    FacilityUserSeq = 1,
                    IsFacilityAdmin = true,
                    IsDeleted = false,
                    CreatedAt = dateTimeProvider.Now,
                    UpdatedAt = dateTimeProvider.Now,
                    CreatedUserId = Guid.Empty,
                    CreatedSessionId = Guid.Empty,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                };
                context.FacilityUsers.Add(facilityUser);
                Console.WriteLine($"  [+] FacilityUser: UserId: {facilityUser.UserId} (FacilityId: {firstFacility.FacilityId})");
            }
        }

        return needsUserSeed;
    }
}


