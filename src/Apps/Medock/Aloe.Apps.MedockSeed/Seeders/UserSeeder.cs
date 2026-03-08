using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class UserSeeder
{
    public static async Task<bool> SeedAsync(MedockDbContext context, PasswordHasher passwordHasher, IDateTimeProvider dateTimeProvider, Guid facilityId, Guid floorId)
    {
        // mcp-system ユーザーのシード（adminとは独立）
        var existingMcpSystem = await context.Users.FirstOrDefaultAsync(u => u.UserCode == "mcp-system");
        if (existingMcpSystem == null)
        {
            Console.WriteLine("[INFO] Creating mcp-system user...");
            var (mcpHash, mcpSalt) = passwordHasher.HashPassword("mcp-system-password");
            var mcpSystemUser = new User
            {
                UserId = Guid.CreateVersion7(),
                UserCode = "mcp-system",
                Email = "mcp-system@example.com",
                UserDisplayName = "MCPシステム",
                PasswordHash = mcpHash,
                PasswordSalt = mcpSalt,
                IsSystemAdmin = true,
                IsDeleted = false,
                CreatedAt = dateTimeProvider.Now,
                UpdatedAt = dateTimeProvider.Now,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            context.Users.Add(mcpSystemUser);
            await context.SaveChangesAsync();
            Console.WriteLine($"  [+] User: {mcpSystemUser.UserDisplayName} - {mcpSystemUser.UserCode}");
        }
        else
        {
            Console.WriteLine("[SKIP] mcp-system user already exists. Skipping.");
        }

        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.UserCode == "admin");
        var needsUserSeed = existingAdmin == null;

        if (!needsUserSeed)
        {
            Console.WriteLine("[SKIP] Admin user already exists. Skipping user seed.");
            return needsUserSeed;
        }

        Console.WriteLine("[INFO] Creating user seed data...");
        Console.WriteLine();

        var users = new List<User>();
        var facilityUsers = new List<FacilityUser>();

        // システム管理者
        var adminUserId = Guid.CreateVersion7();
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

        facilityUsers.Add(new FacilityUser
        {
            FacilityUserId = Guid.CreateVersion7(),
            FacilityId = facilityId,
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

        // 看護師ユーザー1
        var nurse1Id = Guid.CreateVersion7();
        var (nurse1Hash, nurse1Salt) = passwordHasher.HashPassword("n1");
        var nurse1 = new User
        {
            UserId = nurse1Id,
            UserCode = "n1",
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

        facilityUsers.Add(new FacilityUser
        {
            FacilityUserId = Guid.CreateVersion7(),
            FacilityId = facilityId,
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

        // 看護師ユーザー2
        var nurse2Id = Guid.CreateVersion7();
        var (nurse2Hash, nurse2Salt) = passwordHasher.HashPassword("n2");
        var nurse2 = new User
        {
            UserId = nurse2Id,
            UserCode = "n2",
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

        facilityUsers.Add(new FacilityUser
        {
            FacilityUserId = Guid.CreateVersion7(),
            FacilityId = facilityId,
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

        // 受付ユーザー1
        var reception1Id = Guid.CreateVersion7();
        var (reception1Hash, reception1Salt) = passwordHasher.HashPassword("r1");
        var reception1 = new User
        {
            UserId = reception1Id,
            UserCode = "r1",
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

        facilityUsers.Add(new FacilityUser
        {
            FacilityUserId = Guid.CreateVersion7(),
            FacilityId = facilityId,
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

        // 受付ユーザー2
        var reception2Id = Guid.CreateVersion7();
        var (reception2Hash, reception2Salt) = passwordHasher.HashPassword("r2");
        var reception2 = new User
        {
            UserId = reception2Id,
            UserCode = "r2",
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

        facilityUsers.Add(new FacilityUser
        {
            FacilityUserId = Guid.CreateVersion7(),
            FacilityId = facilityId,
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

        // 医師ユーザー
        var doctor1Id = Guid.CreateVersion7();
        var (doctor1Hash, doctor1Salt) = passwordHasher.HashPassword("d1");
        var doctor1 = new User
        {
            UserId = doctor1Id,
            UserCode = "d1",
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

        facilityUsers.Add(new FacilityUser
        {
            FacilityUserId = Guid.CreateVersion7(),
            FacilityId = facilityId,
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

        context.Users.AddRange(users);
        context.FacilityUsers.AddRange(facilityUsers);
        Console.WriteLine($"  [+] FacilityUsers: {facilityUsers.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        return needsUserSeed;
    }
}


