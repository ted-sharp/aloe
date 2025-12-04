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

    // 4. 施設・フロア・部屋・設備データ作成
    var existingFacility = await context.Facilities.FirstOrDefaultAsync();
    Guid? facilityId = existingFacility?.FacilityId;
    Guid? floorId = null;
    
    if (existingFacility == null)
    {
        var tenantForFacility = await context.Tenants.FirstOrDefaultAsync();
        if (tenantForFacility != null)
        {
            Console.WriteLine("[INFO] Creating facility and floor seed data...");
            
            // 施設作成
            facilityId = Guid.NewGuid();
            var facility = new Facility
            {
                FacilityId = facilityId.Value,
                TenantId = tenantForFacility.TenantId,
                MedicalInstitutionCode = "1234567890",
                FacilityName = "アロエ健診センター",
                FacilityNameDisplay = "アロエ健診センター",
                IsActive = true,
                ActiveFrom = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.Facilities.Add(facility);
            Console.WriteLine($"  [+] Facility: {facility.FacilityName}");

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
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.Floors.Add(floor);
            Console.WriteLine($"  [+] Floor: {floor.FloorName}");
        }
    }
    else
    {
        Console.WriteLine("[SKIP] Facility already exists.");
        floorId = (await context.Floors.FirstOrDefaultAsync())?.FloorId;
    }

    // 5. 部屋データ作成
    var existingRooms = await context.Rooms.AnyAsync();
    if (!existingRooms && floorId.HasValue)
    {
        Console.WriteLine("[INFO] Creating room seed data...");
        var rooms = new List<Room>
        {
            new() { RoomId = Guid.NewGuid(), FloorId = floorId.Value, RoomName = "問診室1", RoomDesc = "一般問診", RoomSeq = 1 },
            new() { RoomId = Guid.NewGuid(), FloorId = floorId.Value, RoomName = "問診室2", RoomDesc = "一般問診", RoomSeq = 2 },
            new() { RoomId = Guid.NewGuid(), FloorId = floorId.Value, RoomName = "問診室3", RoomDesc = "専門問診", RoomSeq = 3 },
            new() { RoomId = Guid.NewGuid(), FloorId = floorId.Value, RoomName = "検査室1", RoomDesc = "血液検査・尿検査", RoomSeq = 4 },
            new() { RoomId = Guid.NewGuid(), FloorId = floorId.Value, RoomName = "検査室2", RoomDesc = "心電図・肺機能", RoomSeq = 5 },
            new() { RoomId = Guid.NewGuid(), FloorId = floorId.Value, RoomName = "検査室3", RoomDesc = "眼底・聴力", RoomSeq = 6 },
            new() { RoomId = Guid.NewGuid(), FloorId = floorId.Value, RoomName = "X線室", RoomDesc = "胸部X線", RoomSeq = 7 },
            new() { RoomId = Guid.NewGuid(), FloorId = floorId.Value, RoomName = "CT室", RoomDesc = "CT検査", RoomSeq = 8 },
            new() { RoomId = Guid.NewGuid(), FloorId = floorId.Value, RoomName = "MRI室", RoomDesc = "MRI検査", RoomSeq = 9 },
            new() { RoomId = Guid.NewGuid(), FloorId = floorId.Value, RoomName = "内視鏡室", RoomDesc = "胃・大腸内視鏡", RoomSeq = 10 },
        };
        foreach (var room in rooms)
        {
            room.IsDeleted = false;
            room.CreatedAt = DateTimeOffset.UtcNow;
            room.UpdatedAt = DateTimeOffset.UtcNow;
        }
        context.Rooms.AddRange(rooms);
        Console.WriteLine($"  [+] Rooms: {rooms.Count} entries");
    }
    else if (existingRooms)
    {
        Console.WriteLine("[SKIP] Rooms already exist.");
    }

    // 6. 部屋予約統計データ生成
    var existingRoomStats = await context.RoomAppointmentStats.AnyAsync();
    if (!existingRoomStats)
    {
        Console.WriteLine("[INFO] Creating room appointment stats seed data...");
        var rooms = await context.Rooms.Where(r => !r.IsDeleted).ToListAsync();
        if (rooms.Any())
        {
            var random = new Random(42); // 固定シードで再現性のあるデータ
            var startDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
            var endDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(2));
            var roomStats = new List<RoomAppointmentStats>();

            foreach (var room in rooms)
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
                    var slotMaxes = new[] { 2, 3, 3, 3, 3, 3, 3, 2 }; // 部屋ごとの最大数は少なめ

                    for (var i = 0; i < slotTimes.Length; i++)
                    {
                        var max = slotMaxes[i];
                        var count = random.Next(0, max + 1);
                        slots.Add(new { time = slotTimes[i], count, max });
                        totalCount += count;
                        totalMax += max;
                    }

                    var graphJson = System.Text.Json.JsonSerializer.Serialize(new { slots });

                    roomStats.Add(new RoomAppointmentStats
                    {
                        ApptStatId = Guid.NewGuid(),
                        RoomId = room.RoomId,
                        ApptDate = date,
                        ApptCount = totalCount,
                        ApptMax = totalMax,
                        ApptGraph = graphJson,
                        IsDeleted = false,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                }
            }

            context.RoomAppointmentStats.AddRange(roomStats);
            Console.WriteLine($"  [+] Room Appointment Stats: {roomStats.Count} entries ({rooms.Count} rooms × ~90 days)");
        }
        else
        {
            Console.WriteLine("[WARN] No rooms found. Skipping room appointment stats seed.");
        }
    }
    else
    {
        Console.WriteLine("[SKIP] Room appointment stats already exist.");
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
