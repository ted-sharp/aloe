using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockSeed.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Diagnostics;

Console.WriteLine("=== Aloe Medock Seed Tool ===");
Console.WriteLine();

// DateTime は EFCore 6.0 以降は with timezone にマッピングされるので、それを without timezone にします。
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ホストビルダーを構築
var builder = Host.CreateApplicationBuilder(args);

// 接続文字列取得
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (String.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("[ERROR] Connection string 'DefaultConnection' not found in appsettings.json");
    return 1;
}

// 接続文字列にパフォーマンス設定を追加
var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
{
    CommandTimeout = 0 // タイムアウトなし（大量データ処理用）
    // Poolingはデフォルトで有効、MaxPoolSizeは接続文字列に直接指定（必要に応じて）
};
var optimizedConnectionString = connectionStringBuilder.ConnectionString;

Console.WriteLine($"[INFO] Database: {optimizedConnectionString.Split(';').FirstOrDefault(s => s.StartsWith("Database="))}");

// ログレベルを設定（Seed実行中のEFログを抑制）
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.SetMinimumLevel(LogLevel.Error);
    logging.AddConsole();
    logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.None);
    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
});

// サービス登録（DbContextFactoryも追加して並列処理に対応）
builder.Services.AddDbContext<MedockDbContext>(options =>
{
    options.UseNpgsql(optimizedConnectionString);
    // 読み取り専用クエリが多いため、NoTrackingをデフォルトに
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}, ServiceLifetime.Scoped);

builder.Services.AddDbContextFactory<MedockDbContext>(options =>
{
    options.UseNpgsql(optimizedConnectionString);
    // 読み取り専用クエリが多いため、NoTrackingをデフォルトに
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});
builder.Services.AddSingleton(_ => PasswordHasher.Default);
builder.Services.AddSingleton<IDateTimeProvider, JstDateTimeProvider>();

var host = builder.Build();

// サービス取得
using var scope = host.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<MedockDbContext>();
var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MedockDbContext>>();
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

    var totalStopwatch = Stopwatch.StartNew();
    var seederCount = 0;
    var totalSeeders = 21; // 概算値（実際のSeeder数に合わせて調整）

    // ヘルパー関数：Seeder実行と時間計測
    async Task<T> RunSeederWithResultAsync<T>(string seederName, Func<Task<T>> seederAction)
    {
        seederCount++;
        Console.WriteLine($"[{seederCount}/{totalSeeders}] {seederName}...");
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await seederAction();
            sw.Stop();
            Console.WriteLine($"  ✓ {seederName} completed - took {sw.Elapsed.TotalSeconds:F2}s");
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"  ✗ {seederName} failed after {sw.Elapsed.TotalSeconds:F2}s: {ex.Message}");
            throw;
        }
    }

    async Task RunSeederAsync(string seederName, Func<Task> seederAction)
    {
        seederCount++;
        Console.WriteLine($"[{seederCount}/{totalSeeders}] {seederName}...");
        var sw = Stopwatch.StartNew();
        try
        {
            await seederAction();
            sw.Stop();
            Console.WriteLine($"  ✓ {seederName} completed - took {sw.Elapsed.TotalSeconds:F2}s");
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"  ✗ {seederName} failed after {sw.Elapsed.TotalSeconds:F2}s: {ex.Message}");
            throw;
        }
    }

    // テナント・施設関連（ユーザーより先に作成する必要がある）
    var tenantId = await RunSeederWithResultAsync<Guid>("TenantSeeder", () => TenantSeeder.SeedAsync(context, dateTimeProvider));
    var (facilityId, floorId) = await RunSeederWithResultAsync<(Guid, Guid)>("FacilitySeeder", () => FacilitySeeder.SeedAsync(context, tenantId, dateTimeProvider));
    await RunSeederAsync("FacilityBusinessHoursSeeder", () => FacilityBusinessHoursSeeder.SeedAsync(context, facilityId, dateTimeProvider));
    await RunSeederAsync("FacilityAddressSeeder", () => FacilityAddressSeeder.SeedAsync(context, facilityId, dateTimeProvider));
    await RunSeederAsync("FacilityHolidaySeeder", () => FacilityHolidaySeeder.SeedAsync(context, facilityId, dateTimeProvider));

    // ユーザー関連（施設が存在する状態で実行）
    var needsUserSeed = await RunSeederWithResultAsync<bool>("UserSeeder", () => UserSeeder.SeedAsync(context, passwordHasher, dateTimeProvider, facilityId, floorId));

    // 祝日データ
    await RunSeederAsync("HolidaySeeder", () => HolidaySeeder.SeedAsync(context));

    // マスタデータ（独立）
    await RunSeederAsync("InsuranceProviderSeeder", () => InsuranceProviderSeeder.SeedAsync(context, dateTimeProvider));
    await RunSeederAsync("PlanConditionSeeder", () => PlanConditionSeeder.SeedAsync(context, dateTimeProvider));

    // 団体・患者関連
    await RunSeederAsync("OrganizationSeeder", () => OrganizationSeeder.SeedAsync(context, facilityId, dateTimeProvider));
    await RunSeederAsync("PatientSeeder", () => PatientSeeder.SeedAsync(context, facilityId, dateTimeProvider));

    // 団体・患者の関連データ
    await RunSeederAsync("OrganizationAddressSeeder", () => OrganizationAddressSeeder.SeedAsync(context, facilityId, dateTimeProvider));
    await RunSeederAsync("OrganizationInsuranceSeeder", () => OrganizationInsuranceSeeder.SeedAsync(context, facilityId, dateTimeProvider));
    await RunSeederAsync("PatientAddressSeeder", () => PatientAddressSeeder.SeedAsync(context, facilityId, dateTimeProvider));
    await RunSeederAsync("PatientInsuranceCardSeeder", () => PatientInsuranceCardSeeder.SeedAsync(context, facilityId, dateTimeProvider));
    await RunSeederAsync("OrganizationMemberSeeder", () => OrganizationMemberSeeder.SeedAsync(context, facilityId, dateTimeProvider));

    // プラン関連
    await RunSeederAsync("PlanSeeder", () => PlanSeeder.SeedAsync(context, facilityId, dateTimeProvider));
    await RunSeederAsync("PlanOptionSeeder", () => PlanOptionSeeder.SeedAsync(context, facilityId, dateTimeProvider));
    await RunSeederAsync("PlanConditionMemberSeeder", () => PlanConditionMemberSeeder.SeedAsync(context, facilityId, dateTimeProvider));

    // 予約リソース関連（施設・フロアの後）
    var resourceIds = await RunSeederWithResultAsync<Dictionary<string, Guid>>("AppointmentResourceSeeder", () => AppointmentResourceSeeder.SeedAsync(context, floorId, dateTimeProvider));
    await RunSeederAsync("AppointmentResourceGroupSeeder", () => AppointmentResourceGroupSeeder.SeedAsync(context, facilityId, dateTimeProvider));
    await RunSeederAsync("AppointmentResourceGroupMemberSeeder", () => AppointmentResourceGroupMemberSeeder.SeedAsync(context, facilityId, dateTimeProvider));

    // プランリソース要件（プラン・リソースの後）
    await RunSeederAsync("PlanResourceRequirementSeeder", () => PlanResourceRequirementSeeder.SeedAsync(context, facilityId, dateTimeProvider));

    // 予約スケジュール（新システム）
    await RunSeederAsync("AppointmentScheduleSeeder", () => AppointmentScheduleSeeder.SeedAsync(context, dateTimeProvider));

    // 予約データと統計
    // 1. AppointmentSeeder: 実際の appointment レコードを生成（月別繁忙度と時間帯による揺らぎを反映）
    //    同時にメモリ上でスロット単位の集計を行い、結果を返す
    // 2. AppointmentStatsSeeder: 集計データを使用して統計情報を作成（正確なスロット分布）
    var slotAggregation = await RunSeederWithResultAsync<Dictionary<(Guid ApptResId, DateOnly ApptDate, int SlotStartMin), int>>(
        "AppointmentSeeder",
        () => AppointmentSeeder.SeedAsync(context, floorId, dateTimeProvider));

    // Pass aggregation data to AppointmentStatsSeeder
    await RunSeederAsync("AppointmentStatsSeeder", () => AppointmentStatsSeeder.SeedAsync(context, contextFactory, dateTimeProvider, slotAggregation));

    // RBAC関連
    await RunSeederAsync("FeatureSeeder", () => FeatureSeeder.SeedAsync(context, dateTimeProvider));
    await RunSeederAsync("RoleSeeder", () => RoleSeeder.SeedAsync(context, dateTimeProvider));

    totalStopwatch.Stop();
    Console.WriteLine();
    Console.WriteLine($"=== Seed completed in {totalStopwatch.Elapsed.TotalSeconds:F2}s ===");

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
