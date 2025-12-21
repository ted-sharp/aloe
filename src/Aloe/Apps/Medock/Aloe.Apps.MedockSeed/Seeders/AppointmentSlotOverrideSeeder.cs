using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentSlotOverrideSeeder
{
    private static readonly Random _random = new Random();

    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var existingOverrides = await context.AppointmentSlotOverrides.AnyAsync();
        if (existingOverrides)
        {
            Console.WriteLine("[SKIP] AppointmentSlotOverrides already exist.");
            return;
        }

        // リソースを取得（Mainリソースとその他のリソース）
        var resources = await context.AppointmentResources
            .Where(r => !r.IsDeleted)
            .Include(r => r.Floor)
            .ToListAsync();

        if (!resources.Any())
        {
            Console.WriteLine("[SKIP] AppointmentSlotOverride: No appointment resources found.");
            return;
        }

        // 施設のビジネスアワーを取得
        var facilityIds = resources.Select(r => r.Floor.FacilityId).Distinct().ToList();
        var businessHoursDict = await context.FacilityBusinessHours
            .Where(fbh => facilityIds.Contains(fbh.FacilityId) && fbh.IsActive && !fbh.IsDeleted)
            .ToDictionaryAsync(fbh => fbh.FacilityId, fbh => fbh.BusinessHoursData);

        Console.WriteLine("[INFO] Creating appointment slot override seed data...");

        var overrides = new List<AppointmentSlotOverride>();
        var today = dateTimeProvider.TodayDateOnly;

        // パターン1: 特定日の営業時間短縮（Mainリソース）
        var mainResources = resources.Where(r => r.ApptResTypeCode == (int)Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Main).ToList();
        if (mainResources.Any())
        {
            var mainResource = mainResources.First();
            var facilityId = mainResource.Floor.FacilityId;
            var businessHours = businessHoursDict.GetValueOrDefault(facilityId) ?? new FacilityBusinessHoursRoot();

            // 来週の月曜日を短縮営業（午前のみ）
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7; // 今日が月曜日の場合は来週の月曜日
            var nextMonday = today.AddDays(daysUntilMonday);

            var shortSlots = CreateShortenedDaySlots(businessHours, morningOnly: true);
            overrides.Add(CreateOverride(mainResource.ApptResId, nextMonday, shortSlots, dateTimeProvider));

            // 今月末の最終営業日を午後のみ
            var lastDayOfMonth = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            if (lastDayOfMonth >= today)
            {
                var afternoonOnlySlots = CreateShortenedDaySlots(businessHours, morningOnly: false);
                overrides.Add(CreateOverride(mainResource.ApptResId, lastDayOfMonth, afternoonOnlySlots, dateTimeProvider));
            }
        }

        // パターン2: 臨時休診（リソースを無効化）
        var medicalResources = resources.Where(r => r.ApptResTypeCode != (int)Aloe.Apps.MedockLib.Constants.AppointmentResourceType.Main).ToList();
        if (medicalResources.Any())
        {
            var resource = medicalResources[_random.Next(medicalResources.Count)];
            // 2週間後の火曜日を臨時休診
            var futureTuesday = today.AddDays((int)DayOfWeek.Tuesday - (int)today.DayOfWeek + 14);
            if (futureTuesday.DayOfWeek != DayOfWeek.Tuesday)
            {
                futureTuesday = futureTuesday.AddDays((int)DayOfWeek.Tuesday - (int)futureTuesday.DayOfWeek);
            }

            var emptySlots = new AppointmentSlotRoot { Slots = new List<AppointmentSlotItem>() };
            overrides.Add(CreateOverride(resource.ApptResId, futureTuesday, emptySlots, dateTimeProvider));
        }

        // パターン3: キャパシティ増加（Mainリソースの特定日）
        if (mainResources.Any())
        {
            var mainResource = mainResources[_random.Next(mainResources.Count)];
            var facilityId = mainResource.Floor.FacilityId;
            var businessHours = businessHoursDict.GetValueOrDefault(facilityId) ?? new FacilityBusinessHoursRoot();

            // 3週間後の金曜日をキャパシティ増加
            var futureFriday = today.AddDays((int)DayOfWeek.Friday - (int)today.DayOfWeek + 21);
            if (futureFriday.DayOfWeek != DayOfWeek.Friday)
            {
                futureFriday = futureFriday.AddDays((int)DayOfWeek.Friday - (int)futureFriday.DayOfWeek);
            }

            var increasedCapSlots = CreateIncreasedCapacitySlots(businessHours, multiplier: 1.5);
            overrides.Add(CreateOverride(mainResource.ApptResId, futureFriday, increasedCapSlots, dateTimeProvider));
        }

        // パターン4: 時間外スロット追加（早朝・夕方）
        if (medicalResources.Any())
        {
            var resource = medicalResources[_random.Next(medicalResources.Count)];
            var facilityId = resource.Floor.FacilityId;
            var businessHours = businessHoursDict.GetValueOrDefault(facilityId) ?? new FacilityBusinessHoursRoot();

            // 1ヶ月後の水曜日に時間外スロット追加
            var futureWednesday = today.AddDays((int)DayOfWeek.Wednesday - (int)today.DayOfWeek + 28);
            if (futureWednesday.DayOfWeek != DayOfWeek.Wednesday)
            {
                futureWednesday = futureWednesday.AddDays((int)DayOfWeek.Wednesday - (int)futureWednesday.DayOfWeek);
            }

            var extendedSlots = CreateExtendedHoursSlots(businessHours);
            overrides.Add(CreateOverride(resource.ApptResId, futureWednesday, extendedSlots, dateTimeProvider));
        }

        // パターン5: 複数リソースの同日上書き（Mainリソース）
        if (mainResources.Count > 1)
        {
            var facilityId = mainResources.First().Floor.FacilityId;
            var businessHours = businessHoursDict.GetValueOrDefault(facilityId) ?? new FacilityBusinessHoursRoot();

            // 2ヶ月後の第1月曜日を特別営業日として上書き
            var targetMonth = today.AddMonths(2);
            var firstMonday = new DateOnly(targetMonth.Year, targetMonth.Month, 1);
            while (firstMonday.DayOfWeek != DayOfWeek.Monday)
            {
                firstMonday = firstMonday.AddDays(1);
            }

            var specialDaySlots = CreateSpecialDaySlots(businessHours);
            // 最初の2つのMainリソースに適用
            foreach (var resource in mainResources.Take(2))
            {
                overrides.Add(CreateOverride(resource.ApptResId, firstMonday, specialDaySlots, dateTimeProvider));
            }
        }

        context.AppointmentSlotOverrides.AddRange(overrides);
        Console.WriteLine($"  [+] AppointmentSlotOverrides: {overrides.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 上書きオブジェクトを作成
    /// </summary>
    private static AppointmentSlotOverride CreateOverride(
        Guid resourceId,
        DateOnly date,
        AppointmentSlotRoot slots,
        IDateTimeProvider dateTimeProvider)
    {
        var json = JsonSerializer.Serialize(slots, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var overrideEntity = new AppointmentSlotOverride
        {
            ApptSlotOverrideId = Guid.CreateVersion7(),
            ApptDate = date,
            ApptResId = resourceId,
            ApptSlots = json,
            ApptSlotsData = slots,
            IsDeleted = false
        };

        SeederHelper.InitializeAuditFields(overrideEntity, dateTimeProvider);
        return overrideEntity;
    }

    /// <summary>
    /// 短縮営業日のスロットを作成（午前のみまたは午後のみ）
    /// </summary>
    private static AppointmentSlotRoot CreateShortenedDaySlots(
        FacilityBusinessHoursRoot businessHours,
        bool morningOnly)
    {
        var slots = new List<AppointmentSlotItem>();
        var businessStart = TimeOnly.Parse(businessHours.Start ?? "09:00");
        var businessEnd = TimeOnly.Parse(businessHours.End ?? "18:00");
        var lunchStart = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.Start) : new TimeOnly(12, 0);
        var lunchEnd = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.End) : new TimeOnly(13, 0);

        const int intervalMinutes = 30;
        const int maxPerSlot = 20;

        if (morningOnly)
        {
            // 午前のみ（09:00-12:00、30分区切り）
            var currentTime = businessStart;
            while (currentTime < lunchStart)
            {
                var endTime = currentTime.AddMinutes(intervalMinutes);
                if (endTime > lunchStart) endTime = lunchStart;

                slots.Add(new AppointmentSlotItem
                {
                    Start = currentTime,
                    End = endTime,
                    Cap = maxPerSlot
                });
                currentTime = endTime;
            }
        }
        else
        {
            // 午後のみ（13:00-18:00、30分区切り）
            var currentTime = lunchEnd;
            while (currentTime < businessEnd)
            {
                var endTime = currentTime.AddMinutes(intervalMinutes);
                if (endTime > businessEnd) endTime = businessEnd;

                slots.Add(new AppointmentSlotItem
                {
                    Start = currentTime,
                    End = endTime,
                    Cap = maxPerSlot
                });
                currentTime = endTime;
            }
        }

        return new AppointmentSlotRoot { Slots = slots };
    }

    /// <summary>
    /// キャパシティ増加スロットを作成
    /// </summary>
    private static AppointmentSlotRoot CreateIncreasedCapacitySlots(
        FacilityBusinessHoursRoot businessHours,
        double multiplier)
    {
        var slots = new List<AppointmentSlotItem>();
        var businessStart = TimeOnly.Parse(businessHours.Start ?? "09:00");
        var businessEnd = TimeOnly.Parse(businessHours.End ?? "18:00");
        var lunchStart = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.Start) : new TimeOnly(12, 0);
        var lunchEnd = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.End) : new TimeOnly(13, 0);

        const int intervalMinutes = 30;
        const int baseMaxPerSlot = 20;
        var increasedCap = (int)(baseMaxPerSlot * multiplier);

        // 午前のスロット
        var currentTime = businessStart;
        while (currentTime < lunchStart)
        {
            var endTime = currentTime.AddMinutes(intervalMinutes);
            if (endTime > lunchStart) endTime = lunchStart;

            slots.Add(new AppointmentSlotItem
            {
                Start = currentTime,
                End = endTime,
                Cap = increasedCap
            });
            currentTime = endTime;
        }

        // 午後のスロット
        currentTime = lunchEnd;
        while (currentTime < businessEnd)
        {
            var endTime = currentTime.AddMinutes(intervalMinutes);
            if (endTime > businessEnd) endTime = businessEnd;

            slots.Add(new AppointmentSlotItem
            {
                Start = currentTime,
                End = endTime,
                Cap = increasedCap
            });
            currentTime = endTime;
        }

        return new AppointmentSlotRoot { Slots = slots };
    }

    /// <summary>
    /// 時間外スロットを含む拡張営業日のスロットを作成
    /// </summary>
    private static AppointmentSlotRoot CreateExtendedHoursSlots(
        FacilityBusinessHoursRoot businessHours)
    {
        var slots = new List<AppointmentSlotItem>();
        var businessStart = TimeOnly.Parse(businessHours.Start ?? "09:00");
        var businessEnd = TimeOnly.Parse(businessHours.End ?? "18:00");
        var lunchStart = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.Start) : new TimeOnly(12, 0);
        var lunchEnd = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.End) : new TimeOnly(13, 0);

        // 早朝スロット（07:00-09:00、20分区切り）
        var earlyStart = new TimeOnly(7, 0);
        var currentTime = earlyStart;
        while (currentTime < businessStart)
        {
            var endTime = currentTime.AddMinutes(20);
            if (endTime > businessStart) endTime = businessStart;

            slots.Add(new AppointmentSlotItem
            {
                Start = currentTime,
                End = endTime,
                Cap = 1,
                IsOutsideHours = true
            });
            currentTime = endTime;
        }

        // 通常営業時間のスロット（20分区切り）
        currentTime = businessStart;
        while (currentTime < lunchStart)
        {
            var endTime = currentTime.AddMinutes(20);
            if (endTime > lunchStart) endTime = lunchStart;

            slots.Add(new AppointmentSlotItem
            {
                Start = currentTime,
                End = endTime,
                Cap = 1
            });
            currentTime = endTime;
        }

        currentTime = lunchEnd;
        while (currentTime < businessEnd)
        {
            var endTime = currentTime.AddMinutes(20);
            if (endTime > businessEnd) endTime = businessEnd;

            slots.Add(new AppointmentSlotItem
            {
                Start = currentTime,
                End = endTime,
                Cap = 1
            });
            currentTime = endTime;
        }

        // 夕方拡張スロット（18:00-20:00、20分区切り）
        var eveningEnd = new TimeOnly(20, 0);
        currentTime = businessEnd;
        while (currentTime < eveningEnd)
        {
            var endTime = currentTime.AddMinutes(20);
            if (endTime > eveningEnd) endTime = eveningEnd;

            slots.Add(new AppointmentSlotItem
            {
                Start = currentTime,
                End = endTime,
                Cap = 1,
                IsOutsideHours = true
            });
            currentTime = endTime;
        }

        return new AppointmentSlotRoot { Slots = slots };
    }

    /// <summary>
    /// 特別営業日のスロットを作成（15分区切り、キャパシティ増加）
    /// </summary>
    private static AppointmentSlotRoot CreateSpecialDaySlots(
        FacilityBusinessHoursRoot businessHours)
    {
        var slots = new List<AppointmentSlotItem>();
        var businessStart = TimeOnly.Parse(businessHours.Start ?? "09:00");
        var businessEnd = TimeOnly.Parse(businessHours.End ?? "18:00");
        var lunchStart = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.Start) : new TimeOnly(12, 0);
        var lunchEnd = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.End) : new TimeOnly(13, 0);

        const int intervalMinutes = 15;
        const int maxPerSlot = 25; // 通常より多い

        // 午前のスロット（15分区切り）
        var currentTime = businessStart;
        while (currentTime < lunchStart)
        {
            var endTime = currentTime.AddMinutes(intervalMinutes);
            if (endTime > lunchStart) endTime = lunchStart;

            slots.Add(new AppointmentSlotItem
            {
                Start = currentTime,
                End = endTime,
                Cap = maxPerSlot
            });
            currentTime = endTime;
        }

        // 午後のスロット（15分区切り）
        currentTime = lunchEnd;
        while (currentTime < businessEnd)
        {
            var endTime = currentTime.AddMinutes(intervalMinutes);
            if (endTime > businessEnd) endTime = businessEnd;

            slots.Add(new AppointmentSlotItem
            {
                Start = currentTime,
                End = endTime,
                Cap = maxPerSlot
            });
            currentTime = endTime;
        }

        return new AppointmentSlotRoot { Slots = slots };
    }
}

