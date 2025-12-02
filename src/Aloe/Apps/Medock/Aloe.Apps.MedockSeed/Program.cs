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
    if (existingAdmin != null)
    {
        Console.WriteLine("[SKIP] Admin user already exists. Skipping seed.");
        Console.WriteLine();
        Console.WriteLine("=== Seed completed (no changes) ===");
        return 0;
    }

    // Seedデータ投入
    Console.WriteLine("[INFO] Creating seed data...");
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

    // 保存
    await context.SaveChangesAsync();
    Console.WriteLine();
    Console.WriteLine("[OK] Seed data saved successfully.");
    Console.WriteLine();
    Console.WriteLine("=== Seed completed ===");
    Console.WriteLine();
    Console.WriteLine("You can now login with:");
    Console.WriteLine("  UserCode: admin");
    Console.WriteLine("  Password: admin");

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    return 1;
}
