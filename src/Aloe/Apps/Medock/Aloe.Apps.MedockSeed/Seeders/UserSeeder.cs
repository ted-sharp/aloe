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

            var existingFacilityForUser = await context.Facilities.FirstOrDefaultAsync();
            var users = new List<User>();
            var facilityUsers = new List<FacilityUser>();

            // システム管理者
            var adminUserId = Guid.NewGuid();
            var (adminHash, adminSalt) = passwordHasher.HashPassword("admin");
            var newAdminUser = new User
            {
                UserId = adminUserId,
                UserCode = "admin",
                Email = "admin@example.com",
                UserDisplayName = "システム管理者",
                PasswordHash = adminHash,
                PasswordSalt = adminSalt,
                IsSystemAdmin = true,
                IsDeleted = false,
                CreatedAt = dateTimeProvider.Now,
                UpdatedAt = dateTimeProvider.Now,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            users.Add(newAdminUser);
            Console.WriteLine($"  [+] User: {newAdminUser.UserDisplayName} - {newAdminUser.UserCode}");
            Console.WriteLine($"      Email: {newAdminUser.Email}");
            Console.WriteLine($"      Password: admin (hashed)");
            Console.WriteLine($"      IsSystemAdmin: {newAdminUser.IsSystemAdmin}");

            if (existingFacilityForUser != null)
            {
                facilityUsers.Add(new FacilityUser
                {
                    FacilityUserId = Guid.NewGuid(),
                    FacilityId = existingFacilityForUser.FacilityId,
                    UserId = adminUserId,
                    FacilityUserSeq = 1,
                    IsFacilityAdmin = true,
                    IsDeleted = false,
                    CreatedAt = dateTimeProvider.Now,
                    UpdatedAt = dateTimeProvider.Now,
                    CreatedUserId = Guid.Empty,
                    CreatedSessionId = Guid.Empty,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                });
            }

            // 看護師ユーザー1
            var nurse1Id = Guid.NewGuid();
            var (nurse1Hash, nurse1Salt) = passwordHasher.HashPassword("nurse1");
            var nurse1 = new User
            {
                UserId = nurse1Id,
                UserCode = "nurse1",
                Email = "nurse1@example.com",
                UserDisplayName = "看護師 花子",
                PasswordHash = nurse1Hash,
                PasswordSalt = nurse1Salt,
                IsSystemAdmin = false,
                IsDeleted = false,
                CreatedAt = dateTimeProvider.Now,
                UpdatedAt = dateTimeProvider.Now,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            users.Add(nurse1);
            Console.WriteLine($"  [+] User: {nurse1.UserDisplayName} - {nurse1.UserCode}");
            Console.WriteLine($"      Email: {nurse1.Email}");
            Console.WriteLine($"      Password: nurse1 (hashed)");

            if (existingFacilityForUser != null)
            {
                facilityUsers.Add(new FacilityUser
                {
                    FacilityUserId = Guid.NewGuid(),
                    FacilityId = existingFacilityForUser.FacilityId,
                    UserId = nurse1Id,
                    FacilityUserSeq = 2,
                    IsFacilityAdmin = false,
                    IsDeleted = false,
                    CreatedAt = dateTimeProvider.Now,
                    UpdatedAt = dateTimeProvider.Now,
                    CreatedUserId = Guid.Empty,
                    CreatedSessionId = Guid.Empty,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                });
            }

            // 看護師ユーザー2
            var nurse2Id = Guid.NewGuid();
            var (nurse2Hash, nurse2Salt) = passwordHasher.HashPassword("nurse2");
            var nurse2 = new User
            {
                UserId = nurse2Id,
                UserCode = "nurse2",
                Email = "nurse2@example.com",
                UserDisplayName = "看護師 太郎",
                PasswordHash = nurse2Hash,
                PasswordSalt = nurse2Salt,
                IsSystemAdmin = false,
                IsDeleted = false,
                CreatedAt = dateTimeProvider.Now,
                UpdatedAt = dateTimeProvider.Now,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            users.Add(nurse2);
            Console.WriteLine($"  [+] User: {nurse2.UserDisplayName} - {nurse2.UserCode}");
            Console.WriteLine($"      Email: {nurse2.Email}");
            Console.WriteLine($"      Password: nurse2 (hashed)");

            if (existingFacilityForUser != null)
            {
                facilityUsers.Add(new FacilityUser
                {
                    FacilityUserId = Guid.NewGuid(),
                    FacilityId = existingFacilityForUser.FacilityId,
                    UserId = nurse2Id,
                    FacilityUserSeq = 3,
                    IsFacilityAdmin = false,
                    IsDeleted = false,
                    CreatedAt = dateTimeProvider.Now,
                    UpdatedAt = dateTimeProvider.Now,
                    CreatedUserId = Guid.Empty,
                    CreatedSessionId = Guid.Empty,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                });
            }

            // 受付ユーザー1
            var reception1Id = Guid.NewGuid();
            var (reception1Hash, reception1Salt) = passwordHasher.HashPassword("reception1");
            var reception1 = new User
            {
                UserId = reception1Id,
                UserCode = "reception1",
                Email = "reception1@example.com",
                UserDisplayName = "受付 美咲",
                PasswordHash = reception1Hash,
                PasswordSalt = reception1Salt,
                IsSystemAdmin = false,
                IsDeleted = false,
                CreatedAt = dateTimeProvider.Now,
                UpdatedAt = dateTimeProvider.Now,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            users.Add(reception1);
            Console.WriteLine($"  [+] User: {reception1.UserDisplayName} - {reception1.UserCode}");
            Console.WriteLine($"      Email: {reception1.Email}");
            Console.WriteLine($"      Password: reception1 (hashed)");

            if (existingFacilityForUser != null)
            {
                facilityUsers.Add(new FacilityUser
                {
                    FacilityUserId = Guid.NewGuid(),
                    FacilityId = existingFacilityForUser.FacilityId,
                    UserId = reception1Id,
                    FacilityUserSeq = 4,
                    IsFacilityAdmin = false,
                    IsDeleted = false,
                    CreatedAt = dateTimeProvider.Now,
                    UpdatedAt = dateTimeProvider.Now,
                    CreatedUserId = Guid.Empty,
                    CreatedSessionId = Guid.Empty,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                });
            }

            // 受付ユーザー2
            var reception2Id = Guid.NewGuid();
            var (reception2Hash, reception2Salt) = passwordHasher.HashPassword("reception2");
            var reception2 = new User
            {
                UserId = reception2Id,
                UserCode = "reception2",
                Email = "reception2@example.com",
                UserDisplayName = "受付 健太",
                PasswordHash = reception2Hash,
                PasswordSalt = reception2Salt,
                IsSystemAdmin = false,
                IsDeleted = false,
                CreatedAt = dateTimeProvider.Now,
                UpdatedAt = dateTimeProvider.Now,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            users.Add(reception2);
            Console.WriteLine($"  [+] User: {reception2.UserDisplayName} - {reception2.UserCode}");
            Console.WriteLine($"      Email: {reception2.Email}");
            Console.WriteLine($"      Password: reception2 (hashed)");

            if (existingFacilityForUser != null)
            {
                facilityUsers.Add(new FacilityUser
                {
                    FacilityUserId = Guid.NewGuid(),
                    FacilityId = existingFacilityForUser.FacilityId,
                    UserId = reception2Id,
                    FacilityUserSeq = 5,
                    IsFacilityAdmin = false,
                    IsDeleted = false,
                    CreatedAt = dateTimeProvider.Now,
                    UpdatedAt = dateTimeProvider.Now,
                    CreatedUserId = Guid.Empty,
                    CreatedSessionId = Guid.Empty,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                });
            }

            // 医師ユーザー
            var doctor1Id = Guid.NewGuid();
            var (doctor1Hash, doctor1Salt) = passwordHasher.HashPassword("doctor1");
            var doctor1 = new User
            {
                UserId = doctor1Id,
                UserCode = "doctor1",
                Email = "doctor1@example.com",
                UserDisplayName = "医師 一郎",
                PasswordHash = doctor1Hash,
                PasswordSalt = doctor1Salt,
                IsSystemAdmin = false,
                IsDeleted = false,
                CreatedAt = dateTimeProvider.Now,
                UpdatedAt = dateTimeProvider.Now,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            users.Add(doctor1);
            Console.WriteLine($"  [+] User: {doctor1.UserDisplayName} - {doctor1.UserCode}");
            Console.WriteLine($"      Email: {doctor1.Email}");
            Console.WriteLine($"      Password: doctor1 (hashed)");

            if (existingFacilityForUser != null)
            {
                facilityUsers.Add(new FacilityUser
                {
                    FacilityUserId = Guid.NewGuid(),
                    FacilityId = existingFacilityForUser.FacilityId,
                    UserId = doctor1Id,
                    FacilityUserSeq = 6,
                    IsFacilityAdmin = false,
                    IsDeleted = false,
                    CreatedAt = dateTimeProvider.Now,
                    UpdatedAt = dateTimeProvider.Now,
                    CreatedUserId = Guid.Empty,
                    CreatedSessionId = Guid.Empty,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                });
            }

            context.Users.AddRange(users);
            if (facilityUsers.Any())
            {
                context.FacilityUsers.AddRange(facilityUsers);
                Console.WriteLine($"  [+] FacilityUsers: {facilityUsers.Count} entries");
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


