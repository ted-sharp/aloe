using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockSeed.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("=== Aloe Medock Seed Tool ===");
Console.WriteLine();

// DateTime は EFCore 6.0 以降は with timezone にマッピングされるので、それを without timezone にします。
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ホストビルダーを構築
var builder = Host.CreateApplicationBuilder(args);

// 設定ファイル読み込み
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

// 接続文字列取得
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (String.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("[ERROR] Connection string 'DefaultConnection' not found in appsettings.json");
    return 1;
}

Console.WriteLine($"[INFO] Database: {connectionString.Split(';').FirstOrDefault(s => s.StartsWith("Database="))}");

// ログレベルを設定（Seed実行中のEFログを抑制）
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.SetMinimumLevel(LogLevel.Error);
    logging.AddConsole();
    logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.None);
    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
});

// サービス登録
builder.Services.AddDbContext<MedockDbContext>(options =>
    options.UseNpgsql(connectionString), ServiceLifetime.Scoped);
builder.Services.AddSingleton(_ => PasswordHasher.Default);
builder.Services.AddSingleton<IDateTimeProvider, JstDateTimeProvider>();

var host = builder.Build();

// サービス取得
using var scope = host.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<MedockDbContext>();
var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

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

    // テナント・施設関連（ユーザーより先に作成する必要がある）
    var tenantId = await TenantSeeder.SeedAsync(context, dateTimeProvider);
    var (facilityId, floorId) = await FacilitySeeder.SeedAsync(context, tenantId, dateTimeProvider);
    await FacilityBusinessHoursSeeder.SeedAsync(context, facilityId, dateTimeProvider);
    await EquipmentSeeder.SeedAsync(context, floorId, dateTimeProvider);

    // 施設が存在する状態で保存（FacilityUserの作成に必要）
    if (context.ChangeTracker.HasChanges())
    {
        await context.SaveChangesAsync();
    }

    // ユーザー関連（施設が存在する状態で実行）
    var needsUserSeed = await UserSeeder.SeedAsync(context, passwordHasher, dateTimeProvider);
    await EquipmentAppointmentStatsSeeder.SeedAsync(context, dateTimeProvider);

    // 予約スロット・統計関連
    await AppointmentSlotSeeder.SeedAsync(context, floorId, dateTimeProvider);
    await AppointmentStatsSeeder.SeedAsync(context, floorId, dateTimeProvider);

    // 祝日データ
    await HolidaySeeder.SeedAsync(context);

    // 団体・患者関連
    await OrganizationSeeder.SeedAsync(context, facilityId, dateTimeProvider);
    await PatientSeeder.SeedAsync(context, facilityId, dateTimeProvider);

    // 予約データ関連
    await AppointmentSeeder.SeedAsync(context, floorId, dateTimeProvider);
    await EquipmentSlotSeeder.SeedAsync(context, dateTimeProvider);
    await EquipmentAppointmentSeeder.SeedAsync(context, dateTimeProvider);

    // RBAC関連
    await ResourceSeeder.SeedAsync(context, dateTimeProvider);
    await RoleSeeder.SeedAsync(context, dateTimeProvider);

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
