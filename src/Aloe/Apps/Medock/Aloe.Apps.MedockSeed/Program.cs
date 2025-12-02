using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("=== Aloe Medock Seed Tool ===");
Console.WriteLine();

// ホストビルダーを構築
var builder = Host.CreateApplicationBuilder(args);

// 設定ファイル読み込み
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

// 接続文字列取得
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("[ERROR] Connection string 'DefaultConnection' not found in appsettings.json");
    return 1;
}

Console.WriteLine($"[INFO] Database: {connectionString.Split(';').FirstOrDefault(s => s.StartsWith("Database="))}");

// サービス登録
builder.Services.AddDbContext<MedockDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddSingleton(_ => PasswordHasher.Default);

var host = builder.Build();

// サービス取得
using var scope = host.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<MedockDbContext>();
var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();

try
{
    // データベース接続確認
    Console.WriteLine("[INFO] Checking database connection...");
    if (!await context.Database.CanConnectAsync())
    {
        Console.WriteLine("[ERROR] Cannot connect to database. Please check your connection string.");
        return 1;
    }
    Console.WriteLine("[OK] Database connection successful.");

    // マイグレーション適用（テーブル作成）
    Console.WriteLine("[INFO] Applying migrations (creating tables if needed)...");
    await context.Database.EnsureCreatedAsync();
    Console.WriteLine("[OK] Database schema is ready.");

    // 既存データチェック
    Console.WriteLine();
    Console.WriteLine("[INFO] Checking existing seed data...");
    
    var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.UserCode == "admin");
    var needsUserSeed = existingAdmin == null;
    
    if (needsUserSeed)
    {
        // Seedデータ投入
        Console.WriteLine("[INFO] Creating user seed data...");
        Console.WriteLine();

        // 1. テナント作成
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            TenantId = tenantId,
            TenantName = "デモテナント",
            IsActive = true,
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today),
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.Tenants.Add(tenant);
        Console.WriteLine($"  [+] Tenant: {tenant.TenantName} ({tenant.TenantId})");

        // 2. ユーザー作成（パスワードハッシュ化）
        var userId = Guid.NewGuid();
        var (hash, salt) = passwordHasher.HashPassword("admin");
        var user = new User
        {
            UserId = userId,
            UserCode = "admin",
            Email = "admin@example.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsSystemAdmin = true,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.Users.Add(user);
        Console.WriteLine($"  [+] User: {user.UserCode} ({user.UserId})");
        Console.WriteLine($"      Email: {user.Email}");
        Console.WriteLine($"      Password: admin (hashed)");
        Console.WriteLine($"      IsSystemAdmin: {user.IsSystemAdmin}");

        // 3. テナントユーザー作成（紐付け）
        var tenantUser = new TenantUser
        {
            TenantUserId = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            DisplayName = "管理者",
            TenantUserSeq = 1,
            IsTenantAdmin = true,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.TenantUsers.Add(tenantUser);
        Console.WriteLine($"  [+] TenantUser: {tenantUser.DisplayName} (Tenant: {tenant.TenantName})");
    }
    else
    {
        Console.WriteLine("[SKIP] Admin user already exists. Skipping user seed.");
    }

    // 4. 祝日データ作成（adminユーザーとは別にチェック）
    var existingHolidays = await context.Holidays.AnyAsync();
    if (existingHolidays)
    {
        Console.WriteLine("[SKIP] Holidays already exist. Skipping holiday seed.");
    }
    else
    {
        Console.WriteLine("[INFO] Creating holiday seed data...");
        var holidays = new List<Holiday>
        {
            // 2025年
            new() { HolidayDate = new DateOnly(2025, 1, 1), HolidayName = "元日" },
            new() { HolidayDate = new DateOnly(2025, 1, 13), HolidayName = "成人の日" },
            new() { HolidayDate = new DateOnly(2025, 2, 11), HolidayName = "建国記念の日" },
            new() { HolidayDate = new DateOnly(2025, 2, 23), HolidayName = "天皇誕生日" },
            new() { HolidayDate = new DateOnly(2025, 2, 24), HolidayName = "振替休日" },
            new() { HolidayDate = new DateOnly(2025, 3, 20), HolidayName = "春分の日" },
            new() { HolidayDate = new DateOnly(2025, 4, 29), HolidayName = "昭和の日" },
            new() { HolidayDate = new DateOnly(2025, 5, 3), HolidayName = "憲法記念日" },
            new() { HolidayDate = new DateOnly(2025, 5, 4), HolidayName = "みどりの日" },
            new() { HolidayDate = new DateOnly(2025, 5, 5), HolidayName = "こどもの日" },
            new() { HolidayDate = new DateOnly(2025, 5, 6), HolidayName = "振替休日" },
            new() { HolidayDate = new DateOnly(2025, 7, 21), HolidayName = "海の日" },
            new() { HolidayDate = new DateOnly(2025, 8, 11), HolidayName = "山の日" },
            new() { HolidayDate = new DateOnly(2025, 9, 15), HolidayName = "敬老の日" },
            new() { HolidayDate = new DateOnly(2025, 9, 23), HolidayName = "秋分の日" },
            new() { HolidayDate = new DateOnly(2025, 10, 13), HolidayName = "スポーツの日" },
            new() { HolidayDate = new DateOnly(2025, 11, 3), HolidayName = "文化の日" },
            new() { HolidayDate = new DateOnly(2025, 11, 23), HolidayName = "勤労感謝の日" },
            new() { HolidayDate = new DateOnly(2025, 11, 24), HolidayName = "振替休日" },
            // 2026年
            new() { HolidayDate = new DateOnly(2026, 1, 1), HolidayName = "元日" },
            new() { HolidayDate = new DateOnly(2026, 1, 12), HolidayName = "成人の日" },
            new() { HolidayDate = new DateOnly(2026, 2, 11), HolidayName = "建国記念の日" },
            new() { HolidayDate = new DateOnly(2026, 2, 23), HolidayName = "天皇誕生日" },
            new() { HolidayDate = new DateOnly(2026, 3, 20), HolidayName = "春分の日" },
            new() { HolidayDate = new DateOnly(2026, 4, 29), HolidayName = "昭和の日" },
            new() { HolidayDate = new DateOnly(2026, 5, 3), HolidayName = "憲法記念日" },
            new() { HolidayDate = new DateOnly(2026, 5, 4), HolidayName = "みどりの日" },
            new() { HolidayDate = new DateOnly(2026, 5, 5), HolidayName = "こどもの日" },
            new() { HolidayDate = new DateOnly(2026, 5, 6), HolidayName = "振替休日" },
            new() { HolidayDate = new DateOnly(2026, 7, 20), HolidayName = "海の日" },
            new() { HolidayDate = new DateOnly(2026, 8, 11), HolidayName = "山の日" },
            new() { HolidayDate = new DateOnly(2026, 9, 21), HolidayName = "敬老の日" },
            new() { HolidayDate = new DateOnly(2026, 9, 22), HolidayName = "国民の休日" },
            new() { HolidayDate = new DateOnly(2026, 9, 23), HolidayName = "秋分の日" },
            new() { HolidayDate = new DateOnly(2026, 10, 12), HolidayName = "スポーツの日" },
            new() { HolidayDate = new DateOnly(2026, 11, 3), HolidayName = "文化の日" },
            new() { HolidayDate = new DateOnly(2026, 11, 23), HolidayName = "勤労感謝の日" },
        };
        foreach (var holiday in holidays)
        {
            holiday.IsDeleted = false;
        }
        context.Holidays.AddRange(holidays);
        Console.WriteLine($"  [+] Holidays: {holidays.Count} entries (2025-2026)");
    }

    // 保存（ユーザーまたは祝日のいずれかが追加された場合）
    if (context.ChangeTracker.HasChanges())
    {
        await context.SaveChangesAsync();
        Console.WriteLine();
        Console.WriteLine("[OK] Seed data saved successfully.");
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("[INFO] No changes to save.");
    }
    
    Console.WriteLine();
    Console.WriteLine("=== Seed completed ===");
    
    if (needsUserSeed)
    {
        Console.WriteLine();
        Console.WriteLine("You can now login with:");
        Console.WriteLine("  UserCode: admin");
        Console.WriteLine("  Password: admin");
    }

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    return 1;
}
