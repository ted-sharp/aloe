using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("=== Aloe Medock Seed Tool ===");
Console.WriteLine();

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

    // テナント確認（存在しなければ作成）
    var existingTenant = await context.Tenants.FirstOrDefaultAsync();
    Guid tenantId;

    if (existingTenant == null)
    {
        Console.WriteLine("[INFO] Creating tenant seed data...");
        tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            TenantId = tenantId,
            TenantName = "デモテナント",
            IsActive = true,
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today),
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedUserId = Guid.Empty,
            CreatedSessionId = Guid.Empty,
            UpdatedUserId = Guid.Empty,
            UpdatedSessionId = Guid.Empty
        };
        context.Tenants.Add(tenant);
        Console.WriteLine($"  [+] Tenant: {tenant.TenantName} ({tenant.TenantId})");
    }
    else
    {
        tenantId = existingTenant.TenantId;
        Console.WriteLine("[SKIP] Tenant already exists.");
    }

    // ユーザー確認
    var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.UserCode == "admin");
    var needsUserSeed = existingAdmin == null;

    if (needsUserSeed)
    {
        // Seedデータ投入
        Console.WriteLine("[INFO] Creating user seed data...");
        Console.WriteLine();

        // ユーザー作成（パスワードハッシュ化）
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
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedUserId = Guid.Empty,
            CreatedSessionId = Guid.Empty,
            UpdatedUserId = Guid.Empty,
            UpdatedSessionId = Guid.Empty
        };
        context.Users.Add(user);
        Console.WriteLine($"  [+] User: {user.UserCode} ({user.UserId})");
        Console.WriteLine($"      Email: {user.Email}");
        Console.WriteLine($"      Password: admin (hashed)");
        Console.WriteLine($"      IsSystemAdmin: {user.IsSystemAdmin}");

        // 施設が既に存在する場合は、施設ユーザーも作成
        var existingFacilityForUser = await context.Facilities.FirstOrDefaultAsync();
        if (existingFacilityForUser != null)
        {
            var facilityUser = new FacilityUser
            {
                FacilityUserId = Guid.NewGuid(),
                FacilityId = existingFacilityForUser.FacilityId,
                UserId = userId,
                DisplayName = "管理者",
                FacilityUserSeq = 1,
                IsFacilityAdmin = true,
                PermissionLevel = "full",
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            context.FacilityUsers.Add(facilityUser);
            Console.WriteLine($"  [+] FacilityUser: {facilityUser.DisplayName} (FacilityId: {existingFacilityForUser.FacilityId})");
        }
    }
    else
    {
        Console.WriteLine("[SKIP] Admin user already exists. Skipping user seed.");
    }

    // FacilityUser確認（adminユーザーが存在するが、施設ユーザーが存在しない場合）
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
                DisplayName = "管理者",
                FacilityUserSeq = 1,
                IsFacilityAdmin = true,
                PermissionLevel = "full",
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            context.FacilityUsers.Add(facilityUser);
            Console.WriteLine($"  [+] FacilityUser: {facilityUser.DisplayName} (FacilityId: {firstFacility.FacilityId})");
        }
    }

    // 施設・フロア・設備データ作成
    var existingFacility = await context.Facilities.FirstOrDefaultAsync();
    Guid? facilityId = existingFacility?.FacilityId;
    Guid? floorId = null;

    if (existingFacility == null)
    {
        Console.WriteLine("[INFO] Creating facility and floor seed data...");

        // 施設作成
        facilityId = Guid.NewGuid();
        var facility = new Facility
        {
            FacilityId = facilityId.Value,
            TenantId = tenantId,
            MedicalInstitutionCode = "1234567890",
            FacilityName = "アロエ健診センター",
            FacilityNameDisplay = "アロエ健診センター",
            IsActive = true,
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedUserId = Guid.Empty,
            CreatedSessionId = Guid.Empty,
            UpdatedUserId = Guid.Empty,
            UpdatedSessionId = Guid.Empty
        };
        context.Facilities.Add(facility);
        Console.WriteLine($"  [+] Facility: {facility.FacilityName} (TenantId: {tenantId})");

        // フロア作成
        floorId = Guid.NewGuid();
        var floor = new Floor
        {
            FloorId = floorId.Value,
            FacilityId = facilityId.Value,
            FloorCode = "1F",
            FloorName = "1階（健診フロア）",
            FloorDesc = "一般健診・人間ドック",
            FloorSeq = 1,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedUserId = Guid.Empty,
            CreatedSessionId = Guid.Empty,
            UpdatedUserId = Guid.Empty,
            UpdatedSessionId = Guid.Empty
        };
        context.Floors.Add(floor);
        Console.WriteLine($"  [+] Floor: {floor.FloorName} (FacilityId: {facilityId})");
    }
    else
    {
        Console.WriteLine("[SKIP] Facility already exists.");
        facilityId = existingFacility.FacilityId;
        floorId = (await context.Floors.FirstOrDefaultAsync())?.FloorId;
    }

    // 5. 設備データ作成
    var existingEquipments = await context.Equipments.AnyAsync();
    if (!existingEquipments && floorId.HasValue)
    {
        Console.WriteLine("[INFO] Creating equipment seed data...");
        var equipments = new List<Equipment>
        {
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "X線撮影装置1号機", EquipDesc = "胸部X線撮影用", EquipSeq = 1 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "X線撮影装置2号機", EquipDesc = "胸部X線撮影用（予備機）", EquipSeq = 2 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "CT装置", EquipDesc = "64列マルチスライスCT", EquipSeq = 3 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "MRI装置", EquipDesc = "1.5テスラMRI", EquipSeq = 4 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "超音波診断装置1号機", EquipDesc = "腹部エコー用", EquipSeq = 5 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "超音波診断装置2号機", EquipDesc = "心エコー用", EquipSeq = 6 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "内視鏡システム1号機", EquipDesc = "上部消化管内視鏡", EquipSeq = 7 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "内視鏡システム2号機", EquipDesc = "下部消化管内視鏡", EquipSeq = 8 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "心電計1号機", EquipDesc = "12誘導心電図", EquipSeq = 9 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "心電計2号機", EquipDesc = "12誘導心電図（予備機）", EquipSeq = 10 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "肺機能検査装置", EquipDesc = "スパイロメトリー", EquipSeq = 11 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "眼底カメラ", EquipDesc = "デジタル眼底撮影", EquipSeq = 12 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "聴力検査装置", EquipDesc = "オージオメーター", EquipSeq = 13 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "血液検査装置", EquipDesc = "自動血球計数器", EquipSeq = 14 },
            new() { EquipId = Guid.NewGuid(), FloorId = floorId.Value, EquipName = "生化学自動分析装置", EquipDesc = "血液生化学検査用", EquipSeq = 15 },
        };
        foreach (var equipment in equipments)
        {
            equipment.IsDeleted = false;
            equipment.CreatedAt = DateTimeOffset.UtcNow;
            equipment.UpdatedAt = DateTimeOffset.UtcNow;
            equipment.CreatedUserId = Guid.Empty;
            equipment.CreatedSessionId = Guid.Empty;
            equipment.UpdatedUserId = Guid.Empty;
            equipment.UpdatedSessionId = Guid.Empty;
        }
        context.Equipments.AddRange(equipments);
        Console.WriteLine($"  [+] Equipments: {equipments.Count} entries");
    }
    else if (existingEquipments)
    {
        Console.WriteLine("[SKIP] Equipments already exist.");
    }

    // 6. 設備予約統計データ生成
    var existingEquipmentStats = await context.EquipmentAppointmentStats.AnyAsync();
    if (!existingEquipmentStats)
    {
        Console.WriteLine("[INFO] Creating equipment appointment stats seed data...");
        var equipments = await context.Equipments.Where(e => !e.IsDeleted).ToListAsync();
        if (equipments.Any())
        {
            var random = new Random(42); // 固定シードで再現性のあるデータ
            var startDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
            var endDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(2));
            var equipmentStats = new List<EquipmentAppointmentStats>();

            foreach (var equipment in equipments)
            {
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Skip weekends
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    // 時間帯枠ごとのデータを生成
                    var slots = new List<object>();
                    var totalCount = 0;
                    var totalMax = 0;

                    var slotTimes = new[] { "08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };
                    var slotMaxes = new[] { 2, 3, 3, 3, 3, 3, 3, 2 }; // 設備ごとの最大数は少なめ

                    for (var i = 0; i < slotTimes.Length; i++)
                    {
                        var max = slotMaxes[i];
                        var count = random.Next(0, max + 1);
                        slots.Add(new { time = slotTimes[i], count, max });
                        totalCount += count;
                        totalMax += max;
                    }

                    var graphJson = System.Text.Json.JsonSerializer.Serialize(new { slots });

                    equipmentStats.Add(new EquipmentAppointmentStats
                    {
                        ApptStatId = Guid.NewGuid(),
                        EquipId = equipment.EquipId,
                        ApptDate = date,
                        ApptCount = totalCount,
                        ApptMax = totalMax,
                        ApptGraph = graphJson,
                        IsDeleted = false,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        CreatedUserId = Guid.Empty,
                        CreatedSessionId = Guid.Empty,
                        UpdatedUserId = Guid.Empty,
                        UpdatedSessionId = Guid.Empty
                    });
                }
            }

            context.EquipmentAppointmentStats.AddRange(equipmentStats);
            Console.WriteLine($"  [+] Equipment Appointment Stats: {equipmentStats.Count} entries ({equipments.Count} equipments × ~90 days)");
        }
        else
        {
            Console.WriteLine("[WARN] No equipments found. Skipping equipment appointment stats seed.");
        }
    }
    else
    {
        Console.WriteLine("[SKIP] Equipment appointment stats already exist.");
    }

    // 7. 予約スロット定義作成
    var existingApptSlots = await context.AppointmentSlots.AnyAsync();
    if (!existingApptSlots && floorId.HasValue)
    {
        Console.WriteLine("[INFO] Creating appointment slot seed data...");
        var slotsJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            slots = new[]
            {
                new { time = "08:00", max = 5, duration = 60 },
                new { time = "09:00", max = 8, duration = 60 },
                new { time = "10:00", max = 8, duration = 60 },
                new { time = "11:00", max = 8, duration = 60 },
                new { time = "13:00", max = 8, duration = 60 },
                new { time = "14:00", max = 8, duration = 60 },
                new { time = "15:00", max = 8, duration = 60 },
                new { time = "16:00", max = 5, duration = 60 },
            }
        });
        var apptSlot = new AppointmentSlot
        {
            ApptSlotId = Guid.NewGuid(),
            FloorId = floorId.Value,
            ApptSlots = slotsJson,
            IsActive = true,
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ActiveTo = new DateOnly(9999, 12, 31),
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.AppointmentSlots.Add(apptSlot);
        Console.WriteLine($"  [+] AppointmentSlot: 8 time slots defined");
    }
    else if (existingApptSlots)
    {
        Console.WriteLine("[SKIP] AppointmentSlots already exist.");
    }

    // 8. 予約統計データ作成（1年分）
    var existingStats = await context.AppointmentStats.AnyAsync();
    if (!existingStats && floorId.HasValue)
    {
        Console.WriteLine("[INFO] Creating appointment stats seed data (1 year)...");
        var random = new Random(42); // 固定シードで再現性のあるデータ
        var startDate = new DateOnly(DateTime.Today.Year, 1, 1);
        var endDate = new DateOnly(DateTime.Today.Year, 12, 31);
        var statsList = new List<AppointmentStats>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dayOfWeek = date.DayOfWeek;
            var isWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;

            // 時間帯枠ごとのデータを生成
            var slots = new List<object>();
            var totalCount = 0;
            var totalMax = 0;

            var slotTimes = new[] { "08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };
            var slotMaxes = new[] { 5, 8, 8, 8, 8, 8, 8, 5 };

            for (var i = 0; i < slotTimes.Length; i++)
            {
                var max = isWeekend ? slotMaxes[i] / 2 : slotMaxes[i];
                var count = random.Next(0, max + 1);
                slots.Add(new { time = slotTimes[i], count, max });
                totalCount += count;
                totalMax += max;
            }

            var graphJson = System.Text.Json.JsonSerializer.Serialize(new { slots });

            statsList.Add(new AppointmentStats
            {
                ApptStatId = Guid.NewGuid(),
                FloorId = floorId.Value,
                ApptDate = date,
                ApptCount = totalCount,
                ApptMax = totalMax,
                ApptGraph = graphJson,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        context.AppointmentStats.AddRange(statsList);
        Console.WriteLine($"  [+] AppointmentStats: {statsList.Count} days with slot data");
    }
    else if (existingStats)
    {
        Console.WriteLine("[SKIP] AppointmentStats already exist.");
    }

    // 9. 祝日データ作成（adminユーザーとは別にチェック）
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

    // 10. 団体（Organization）データ作成
    var existingOrganizations = await context.Organizations.AnyAsync();
    if (!existingOrganizations && facilityId.HasValue)
    {
        Console.WriteLine("[INFO] Creating organization seed data...");
        var tenant = await context.Tenants.FirstOrDefaultAsync();
        if (tenant != null)
        {
            var organizations = new List<Organization>
            {
                new()
                {
                    OrgId = Guid.NewGuid(),
                    FacilityId = facilityId.Value,
                    ParentOrgId = null,
                    OrgCode = "ORG001",
                    OrgName = "総合診療部",
                    OrgNameKatakana = "ソウゴウシンリョウブ",
                    OrgNameKatakanaCompat = "ソウゴウシンリョウブ",
                    OrgNameDisplay = "総合診療部",
                    OrgNamePrint = "総合診療部",
                    OrgMemo = "一般的な診療部門"
                },
                new()
                {
                    OrgId = Guid.NewGuid(),
                    FacilityId = facilityId.Value,
                    ParentOrgId = null,
                    OrgCode = "ORG002",
                    OrgName = "内科",
                    OrgNameKatakana = "ナイカ",
                    OrgNameKatakanaCompat = "ナイカ",
                    OrgNameDisplay = "内科",
                    OrgNamePrint = "内科",
                    OrgMemo = "内科診療"
                },
                new()
                {
                    OrgId = Guid.NewGuid(),
                    FacilityId = facilityId.Value,
                    ParentOrgId = null,
                    OrgCode = "ORG003",
                    OrgName = "外科",
                    OrgNameKatakana = "ゲカ",
                    OrgNameKatakanaCompat = "ゲカ",
                    OrgNameDisplay = "外科",
                    OrgNamePrint = "外科",
                    OrgMemo = "外科診療"
                },
                new()
                {
                    OrgId = Guid.NewGuid(),
                    FacilityId = facilityId.Value,
                    ParentOrgId = null,
                    OrgCode = "ORG004",
                    OrgName = "放射線科",
                    OrgNameKatakana = "ホウシャセンカ",
                    OrgNameKatakanaCompat = "ホウシャセンカ",
                    OrgNameDisplay = "放射線科",
                    OrgNamePrint = "放射線科",
                    OrgMemo = "放射線診療"
                }
            };

            foreach (var org in organizations)
            {
                org.IsDeleted = false;
                org.CreatedAt = DateTimeOffset.UtcNow;
                org.UpdatedAt = DateTimeOffset.UtcNow;
                org.CreatedUserId = Guid.Empty;
                org.CreatedSessionId = Guid.Empty;
                org.UpdatedUserId = Guid.Empty;
                org.UpdatedSessionId = Guid.Empty;
            }

            context.Organizations.AddRange(organizations);
            Console.WriteLine($"  [+] Organizations: {organizations.Count} entries");
        }
    }
    else if (existingOrganizations)
    {
        Console.WriteLine("[SKIP] Organizations already exist.");
    }

    // 11. 患者（Patient）データ作成
    var existingPatients = await context.Patients.AnyAsync();
    if (!existingPatients && facilityId.HasValue)
    {
        Console.WriteLine("[INFO] Creating patient seed data...");
        var tenant = await context.Tenants.FirstOrDefaultAsync();
        var org = await context.Organizations.FirstOrDefaultAsync();

        if (tenant != null && org != null)
        {
            var patients = new List<Patient>
            {
                new()
                {
                    PtId = Guid.NewGuid(),
                    CanonicalPtId = Guid.NewGuid(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0001",
                    KarteCode = "K001",
                    PtName = "田中 太郎",
                    PtNameCompat = "田中太郎",
                    PtNameKatakana = "タナカ タロウ",
                    PtNameKatakanaCompat = "タナカタロウ",
                    PtMadenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1965, 5, 15),
                    SexCode = 1, // Man
                    PtMemo = "定期健診患者"
                },
                new()
                {
                    PtId = Guid.NewGuid(),
                    CanonicalPtId = Guid.NewGuid(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0002",
                    KarteCode = "K002",
                    PtName = "山田 花子",
                    PtNameCompat = "山田花子",
                    PtNameKatakana = "ヤマダ ハナコ",
                    PtNameKatakanaCompat = "ヤマダハナコ",
                    PtMadenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1972, 3, 22),
                    SexCode = 2, // Woman
                    PtMemo = "定期健診患者"
                },
                new()
                {
                    PtId = Guid.NewGuid(),
                    CanonicalPtId = Guid.NewGuid(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0003",
                    KarteCode = "K003",
                    PtName = "佐藤 次郎",
                    PtNameCompat = "佐藤次郎",
                    PtNameKatakana = "サトウ ジロウ",
                    PtNameKatakanaCompat = "サトウジロウ",
                    PtMadenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1980, 7, 10),
                    SexCode = 1, // Man
                    PtMemo = "人間ドック患者"
                },
                new()
                {
                    PtId = Guid.NewGuid(),
                    CanonicalPtId = Guid.NewGuid(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0004",
                    KarteCode = "K004",
                    PtName = "鈴木 美咲",
                    PtNameCompat = "鈴木美咲",
                    PtNameKatakana = "スズキ ミサキ",
                    PtNameKatakanaCompat = "スズキミサキ",
                    PtMadenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1988, 11, 5),
                    SexCode = 2, // Woman
                    PtMemo = "健診患者"
                },
                new()
                {
                    PtId = Guid.NewGuid(),
                    CanonicalPtId = Guid.NewGuid(),
                    FacilityId = facilityId.Value,
                    PrimaryOrgId = org.OrgId,
                    PtCode = "PT0005",
                    KarteCode = "K005",
                    PtName = "高橋 健一",
                    PtNameCompat = "高橋健一",
                    PtNameKatakana = "タカハシ ケンイチ",
                    PtNameKatakanaCompat = "タカハシケンイチ",
                    PtMadenName = "",
                    PtAliasName = "",
                    BirthDate = new DateOnly(1955, 2, 18),
                    SexCode = 1, // Man
                    PtMemo = "定期健診患者"
                }
            };

            foreach (var patient in patients)
            {
                patient.IsDeleted = false;
                patient.CreatedAt = DateTimeOffset.UtcNow;
                patient.UpdatedAt = DateTimeOffset.UtcNow;
                patient.CreatedUserId = Guid.Empty;
                patient.CreatedSessionId = Guid.Empty;
                patient.UpdatedUserId = Guid.Empty;
                patient.UpdatedSessionId = Guid.Empty;
            }

            context.Patients.AddRange(patients);
            Console.WriteLine($"  [+] Patients: {patients.Count} entries");
        }
    }
    else if (existingPatients)
    {
        Console.WriteLine("[SKIP] Patients already exist.");
    }

    // 12. 予約（Appointment）データ作成
    var existingAppointments = await context.Appointments.AnyAsync();
    if (!existingAppointments && floorId.HasValue)
    {
        Console.WriteLine("[INFO] Creating appointment seed data...");
        var patients = await context.Patients.Where(p => !p.IsDeleted).ToListAsync();
        var orgs = await context.Organizations.Where(o => !o.IsDeleted).ToListAsync();

        if (patients.Any() && orgs.Any())
        {
            var appointments = new List<Appointment>();
            var random = new Random(42);
            var startDate = DateTime.Today.AddDays(1);

            for (var i = 0; i < 20; i++)
            {
                var date = startDate.AddDays(i);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var apptDate = DateOnly.FromDateTime(date);
                var slotTimes = new[] { "08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };
                var slotTime = slotTimes[random.Next(slotTimes.Length)];
                var apptStart = new DateTime(date.Year, date.Month, date.Day, Int32.Parse(slotTime.Split(':')[0]), 0, 0, DateTimeKind.Utc);
                var apptEnd = apptStart.AddMinutes(60);

                appointments.Add(new Appointment
                {
                    ApptId = Guid.NewGuid(),
                    FloorId = floorId.Value,
                    OrgId = orgs[random.Next(orgs.Count)].OrgId,
                    PtId = patients[random.Next(patients.Count)].PtId,
                    ApptDate = apptDate,
                    ApptStartAt = apptStart,
                    ApptEndAt = apptEnd,
                    ApptStatusCode = 1, // Reservation
                    IsDeleted = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    CreatedUserId = Guid.Empty,
                    CreatedSessionId = Guid.Empty,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                });
            }

            context.Appointments.AddRange(appointments);
            Console.WriteLine($"  [+] Appointments: {appointments.Count} entries");
        }
    }
    else if (existingAppointments)
    {
        Console.WriteLine("[SKIP] Appointments already exist.");
    }

    // 13. 設備スロット（EquipmentSlot）データ作成
    var existingEquipmentSlots = await context.EquipmentSlots.AnyAsync();
    if (!existingEquipmentSlots)
    {
        Console.WriteLine("[INFO] Creating equipment slot seed data...");
        var equipments = await context.Equipments.Where(e => !e.IsDeleted).ToListAsync();

        if (equipments.Any())
        {
            var equipmentSlots = new List<EquipmentSlot>();

            foreach (var equipment in equipments)
            {
                var slotsJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    slots = new[]
                    {
                        new { time = "08:00", max = 1, duration = 30 },
                        new { time = "09:00", max = 2, duration = 30 },
                        new { time = "10:00", max = 2, duration = 30 },
                        new { time = "11:00", max = 2, duration = 30 },
                        new { time = "13:00", max = 2, duration = 30 },
                        new { time = "14:00", max = 2, duration = 30 },
                        new { time = "15:00", max = 2, duration = 30 },
                        new { time = "16:00", max = 1, duration = 30 },
                    }
                });

                equipmentSlots.Add(new EquipmentSlot
                {
                    EquipSlotId = Guid.NewGuid(),
                    EquipId = equipment.EquipId,
                    EquipSlots = slotsJson,
                    IsActive = true,
                    ActiveFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
                    ActiveTo = new DateOnly(9999, 12, 31),
                    IsDeleted = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    CreatedUserId = Guid.Empty,
                    CreatedSessionId = Guid.Empty,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                });
            }

            context.EquipmentSlots.AddRange(equipmentSlots);
            Console.WriteLine($"  [+] EquipmentSlots: {equipmentSlots.Count} entries ({equipments.Count} equipments)");
        }
    }
    else
    {
        Console.WriteLine("[SKIP] EquipmentSlots already exist.");
    }

    // 14. 設備予約（EquipmentAppointment）データ作成
    var existingEquipmentAppointments = await context.EquipmentAppointments.AnyAsync();
    if (!existingEquipmentAppointments)
    {
        Console.WriteLine("[INFO] Creating equipment appointment seed data...");
        var equipments = await context.Equipments.Where(e => !e.IsDeleted).ToListAsync();
        var patients = await context.Patients.Where(p => !p.IsDeleted).ToListAsync();
        var orgs = await context.Organizations.Where(o => !o.IsDeleted).ToListAsync();

        if (equipments.Any() && patients.Any() && orgs.Any())
        {
            var equipmentAppointments = new List<EquipmentAppointment>();
            var random = new Random(42);
            var startDate = DateTime.Today.AddDays(1);

            for (var i = 0; i < 15; i++)
            {
                var date = startDate.AddDays(i);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var apptDate = DateOnly.FromDateTime(date);
                var slotTimes = new[] { "08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };
                var slotTime = slotTimes[random.Next(slotTimes.Length)];
                var apptStart = new DateTime(date.Year, date.Month, date.Day, Int32.Parse(slotTime.Split(':')[0]), 0, 0, DateTimeKind.Utc);
                var apptEnd = apptStart.AddMinutes(30);

                equipmentAppointments.Add(new EquipmentAppointment
                {
                    EquipApptId = Guid.NewGuid(),
                    EquipId = equipments[random.Next(equipments.Count)].EquipId,
                    OrgId = orgs[random.Next(orgs.Count)].OrgId,
                    PtId = patients[random.Next(patients.Count)].PtId,
                    ApptDate = apptDate,
                    ApptStartAt = apptStart,
                    ApptEndAt = apptEnd,
                    ApptStatusCode = 1, // Reservation
                    ApptMemo = "設備予約",
                    IsDeleted = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    CreatedUserId = Guid.Empty,
                    CreatedSessionId = Guid.Empty,
                    UpdatedUserId = Guid.Empty,
                    UpdatedSessionId = Guid.Empty
                });
            }

            context.EquipmentAppointments.AddRange(equipmentAppointments);
            Console.WriteLine($"  [+] EquipmentAppointments: {equipmentAppointments.Count} entries");
        }
    }
    else
    {
        Console.WriteLine("[SKIP] EquipmentAppointments already exist.");
    }

    // 15. リソース・操作マスタデータ作成
    var existingResources = await context.Resources.AnyAsync();
    if (!existingResources)
    {
        Console.WriteLine("[INFO] Creating resource and operation master data...");

        // リソース作成
        var resources = new List<Resource>
        {
            new() { ResourceCode = "APPT" },
            new() { ResourceCode = "PATIENT" },
            new() { ResourceCode = "CALENDAR" },
            new() { ResourceCode = "USER" },
        };

        foreach (var resource in resources)
        {
            resource.IsDeleted = false;
            resource.CreatedAt = DateTimeOffset.UtcNow;
            resource.UpdatedAt = DateTimeOffset.UtcNow;
            resource.CreatedUserId = Guid.Empty;
            resource.CreatedSessionId = Guid.Empty;
            resource.UpdatedUserId = Guid.Empty;
            resource.UpdatedSessionId = Guid.Empty;
        }

        context.Resources.AddRange(resources);
        Console.WriteLine($"  [+] Resources: {resources.Count} entries");

        // 操作作成
        var operations = new List<Operation>
        {
            new() { OperationCode = "CREATE" },
            new() { OperationCode = "READ" },
            new() { OperationCode = "UPDATE" },
            new() { OperationCode = "DELETE" },
            new() { OperationCode = "ADMIN" },
        };

        foreach (var operation in operations)
        {
            operation.IsDeleted = false;
            operation.CreatedAt = DateTimeOffset.UtcNow;
            operation.UpdatedAt = DateTimeOffset.UtcNow;
            operation.CreatedUserId = Guid.Empty;
            operation.CreatedSessionId = Guid.Empty;
            operation.UpdatedUserId = Guid.Empty;
            operation.UpdatedSessionId = Guid.Empty;
        }

        context.Operations.AddRange(operations);
        Console.WriteLine($"  [+] Operations: {operations.Count} entries");
    }
    else
    {
        Console.WriteLine("[SKIP] Resources and Operations already exist.");
    }

    // 16. ロール・パーミッション・RBAC データ作成
    var existingRoles = await context.Roles.AnyAsync();
    if (!existingRoles)
    {
        Console.WriteLine("[INFO] Creating role, permission, and RBAC seed data...");

        // ロール作成
        var roles = new List<Role>
        {
            new() { RoleCode = "ADMIN", RoleName = "管理者", RoleDesc = "システム管理者", RoleSeq = 1 },
            new() { RoleCode = "DOCTOR", RoleName = "医師", RoleDesc = "医師ユーザー", RoleSeq = 2 },
            new() { RoleCode = "NURSE", RoleName = "看護師", RoleDesc = "看護師ユーザー", RoleSeq = 3 },
            new() { RoleCode = "RECEPTION", RoleName = "受付", RoleDesc = "受付ユーザー", RoleSeq = 4 },
            new() { RoleCode = "VIEWER", RoleName = "閲覧", RoleDesc = "データ閲覧ユーザー", RoleSeq = 5 },
        };

        foreach (var role in roles)
        {
            role.IsDeleted = false;
            role.CreatedAt = DateTimeOffset.UtcNow;
            role.UpdatedAt = DateTimeOffset.UtcNow;
            role.CreatedUserId = Guid.Empty;
            role.CreatedSessionId = Guid.Empty;
            role.UpdatedUserId = Guid.Empty;
            role.UpdatedSessionId = Guid.Empty;
        }

        context.Roles.AddRange(roles);
        Console.WriteLine($"  [+] Roles: {roles.Count} entries");

        // パーミッション作成
        var permissions = new List<Permission>
        {
            // 予約関連
            new() { PermissionCode = "APPT_CREATE", ResourceCode = "APPT", OperationCode = "CREATE" },
            new() { PermissionCode = "APPT_READ", ResourceCode = "APPT", OperationCode = "READ" },
            new() { PermissionCode = "APPT_UPDATE", ResourceCode = "APPT", OperationCode = "UPDATE" },
            new() { PermissionCode = "APPT_DELETE", ResourceCode = "APPT", OperationCode = "DELETE" },
            // 患者関連
            new() { PermissionCode = "PATIENT_CREATE", ResourceCode = "PATIENT", OperationCode = "CREATE" },
            new() { PermissionCode = "PATIENT_READ", ResourceCode = "PATIENT", OperationCode = "READ" },
            new() { PermissionCode = "PATIENT_UPDATE", ResourceCode = "PATIENT", OperationCode = "UPDATE" },
            new() { PermissionCode = "PATIENT_DELETE", ResourceCode = "PATIENT", OperationCode = "DELETE" },
            // カレンダー関連
            new() { PermissionCode = "CALENDAR_VIEW", ResourceCode = "CALENDAR", OperationCode = "READ" },
            // ユーザー管理
            new() { PermissionCode = "USER_ADMIN", ResourceCode = "USER", OperationCode = "ADMIN" },
        };

        foreach (var permission in permissions)
        {
            permission.IsDeleted = false;
            permission.CreatedAt = DateTimeOffset.UtcNow;
            permission.UpdatedAt = DateTimeOffset.UtcNow;
            permission.CreatedUserId = Guid.Empty;
            permission.CreatedSessionId = Guid.Empty;
            permission.UpdatedUserId = Guid.Empty;
            permission.UpdatedSessionId = Guid.Empty;
        }

        context.Permissions.AddRange(permissions);
        Console.WriteLine($"  [+] Permissions: {permissions.Count} entries");

        // ロール-パーミッション関連付け作成
        var rolePermissions = new List<RolePermission>
        {
            // ADMIN: すべてのパーミッションを持つ
            new() { RolePermissionCode = "ADMIN_APPT_CREATE", RoleCode = "ADMIN", PermissionCode = "APPT_CREATE" },
            new() { RolePermissionCode = "ADMIN_APPT_READ", RoleCode = "ADMIN", PermissionCode = "APPT_READ" },
            new() { RolePermissionCode = "ADMIN_APPT_UPDATE", RoleCode = "ADMIN", PermissionCode = "APPT_UPDATE" },
            new() { RolePermissionCode = "ADMIN_APPT_DELETE", RoleCode = "ADMIN", PermissionCode = "APPT_DELETE" },
            new() { RolePermissionCode = "ADMIN_PATIENT_CREATE", RoleCode = "ADMIN", PermissionCode = "PATIENT_CREATE" },
            new() { RolePermissionCode = "ADMIN_PATIENT_READ", RoleCode = "ADMIN", PermissionCode = "PATIENT_READ" },
            new() { RolePermissionCode = "ADMIN_PATIENT_UPDATE", RoleCode = "ADMIN", PermissionCode = "PATIENT_UPDATE" },
            new() { RolePermissionCode = "ADMIN_PATIENT_DELETE", RoleCode = "ADMIN", PermissionCode = "PATIENT_DELETE" },
            new() { RolePermissionCode = "ADMIN_CALENDAR_VIEW", RoleCode = "ADMIN", PermissionCode = "CALENDAR_VIEW" },
            new() { RolePermissionCode = "ADMIN_USER_ADMIN", RoleCode = "ADMIN", PermissionCode = "USER_ADMIN" },
            // DOCTOR: 予約・患者・カレンダーを読み書き可能
            new() { RolePermissionCode = "DOCTOR_APPT_CREATE", RoleCode = "DOCTOR", PermissionCode = "APPT_CREATE" },
            new() { RolePermissionCode = "DOCTOR_APPT_READ", RoleCode = "DOCTOR", PermissionCode = "APPT_READ" },
            new() { RolePermissionCode = "DOCTOR_APPT_UPDATE", RoleCode = "DOCTOR", PermissionCode = "APPT_UPDATE" },
            new() { RolePermissionCode = "DOCTOR_PATIENT_READ", RoleCode = "DOCTOR", PermissionCode = "PATIENT_READ" },
            new() { RolePermissionCode = "DOCTOR_PATIENT_UPDATE", RoleCode = "DOCTOR", PermissionCode = "PATIENT_UPDATE" },
            new() { RolePermissionCode = "DOCTOR_CALENDAR_VIEW", RoleCode = "DOCTOR", PermissionCode = "CALENDAR_VIEW" },
            // NURSE: 予約・患者を読み取り可能、カレンダーを閲覧
            new() { RolePermissionCode = "NURSE_APPT_READ", RoleCode = "NURSE", PermissionCode = "APPT_READ" },
            new() { RolePermissionCode = "NURSE_PATIENT_READ", RoleCode = "NURSE", PermissionCode = "PATIENT_READ" },
            new() { RolePermissionCode = "NURSE_CALENDAR_VIEW", RoleCode = "NURSE", PermissionCode = "CALENDAR_VIEW" },
            // RECEPTION: 予約・患者を読み書き可能、カレンダーを閲覧
            new() { RolePermissionCode = "RECEPTION_APPT_CREATE", RoleCode = "RECEPTION", PermissionCode = "APPT_CREATE" },
            new() { RolePermissionCode = "RECEPTION_APPT_READ", RoleCode = "RECEPTION", PermissionCode = "APPT_READ" },
            new() { RolePermissionCode = "RECEPTION_APPT_UPDATE", RoleCode = "RECEPTION", PermissionCode = "APPT_UPDATE" },
            new() { RolePermissionCode = "RECEPTION_PATIENT_READ", RoleCode = "RECEPTION", PermissionCode = "PATIENT_READ" },
            new() { RolePermissionCode = "RECEPTION_CALENDAR_VIEW", RoleCode = "RECEPTION", PermissionCode = "CALENDAR_VIEW" },
            // VIEWER: カレンダーのみ閲覧可能
            new() { RolePermissionCode = "VIEWER_CALENDAR_VIEW", RoleCode = "VIEWER", PermissionCode = "CALENDAR_VIEW" },
        };

        foreach (var rolePermission in rolePermissions)
        {
            rolePermission.IsDeleted = false;
            rolePermission.CreatedAt = DateTimeOffset.UtcNow;
            rolePermission.UpdatedAt = DateTimeOffset.UtcNow;
            rolePermission.CreatedUserId = Guid.Empty;
            rolePermission.CreatedSessionId = Guid.Empty;
            rolePermission.UpdatedUserId = Guid.Empty;
            rolePermission.UpdatedSessionId = Guid.Empty;
        }

        context.RolePermissions.AddRange(rolePermissions);
        Console.WriteLine($"  [+] RolePermissions: {rolePermissions.Count} entries ({roles.Count} roles × permissions)");
    }
    else
    {
        Console.WriteLine("[SKIP] Roles already exist.");
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
