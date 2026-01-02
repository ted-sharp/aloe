using Aloe.Apps.MedockLib.Constants;
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

        // リソースを取得（Main と Equipment のみ）
        var resources = await context.AppointmentResources
            .Where(r => !r.IsDeleted)
            .Include(r => r.Floor)
            .Where(r => r.ApptResTypeCode == (int)AppointmentResourceType.Main ||
                        r.ApptResTypeCode == (int)AppointmentResourceType.Equipment)
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

        // 祝日を取得
        var holidays = await SeederHelper.LoadHolidaySetAsync(context);
        var holidaysByFacility = await context.FacilityHolidays
            .AsNoTracking()
            .Where(fh => !fh.IsDeleted)
            .GroupBy(fh => fh.FacilityId)
            .ToDictionaryAsync(
                g => g.Key,
                g => new HashSet<DateOnly>(g.Select(fh => fh.HolidayDate).Union(holidays)));

        Console.WriteLine("[INFO] Creating appointment slot override seed data...");

        var overrides = new List<AppointmentSlotOverride>();
        var (startDate, endDate) = SeederHelper.GetDefaultDateRange(dateTimeProvider);
        var mainResources = resources.Where(r => r.ApptResTypeCode == (int)AppointmentResourceType.Main).ToList();
        var equipmentResources = resources.Where(r => r.ApptResTypeCode == (int)AppointmentResourceType.Equipment).ToList();

        // 月単位で処理
        var currentMonth = new DateOnly(startDate.Year, startDate.Month, 1);
        while (currentMonth <= endDate)
        {
            // 月の末日
            var lastDay = new DateOnly(currentMonth.Year, currentMonth.Month,
                DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month));
            var monthEndDate = lastDay < endDate ? lastDay : endDate;

            // 月内の営業日をすべて取得
            var businessDaysInMonth = GetBusinessDaysInMonth(currentMonth, monthEndDate, holidaysByFacility);
            if (!businessDaysInMonth.Any())
            {
                currentMonth = currentMonth.AddMonths(1);
                continue;
            }

            // 閑散期か繁忙期か判定
            // 時間外データと同程度の頻度（約4%）: 月20営業日×4% ≒ 0.8件/月
            var isOffSeason = IsOffSeasonMonth(currentMonth.Month);
            var overrideCountRange = isOffSeason ? (0, 1) : (0, 2);
            var overrideCount = Math.Min(_random.Next(overrideCountRange.Item1, overrideCountRange.Item2 + 1), businessDaysInMonth.Count);

            // この月のランダムな営業日をシャッフル
            var selectedDates = businessDaysInMonth.OrderBy(_ => _random.Next()).Take(overrideCount).ToList();

            // 選択された日付ごとに、ランダムなオーバーライドを生成
            foreach (var selectedDate in selectedDates)
            {
                // Main リソースと Equipment リソースをランダムに選択
                var isMainResource = mainResources.Any() && (_random.Next(100) < 50 || !equipmentResources.Any());
                var selectedResource = isMainResource
                    ? mainResources[_random.Next(mainResources.Count)]
                    : equipmentResources[_random.Next(equipmentResources.Count)];

                var facilityId = selectedResource.Floor.FacilityId;
                var businessHours = businessHoursDict.GetValueOrDefault(facilityId) ?? new FacilityBusinessHoursRoot();

                // リソースタイプに応じたランダムパターンを選択
                AppointmentSlotRoot? slotDef = null;
                if (isMainResource)
                {
                    var mainPattern = _random.Next(4);
                    slotDef = mainPattern switch
                    {
                        0 => CreateShortenedDaySlots(businessHours, morningOnly: true),
                        1 => CreateShortenedDaySlots(businessHours, morningOnly: false),
                        2 => CreateIncreasedCapacitySlots(businessHours, multiplier: 1.3 + _random.NextDouble() * 0.4),
                        _ => CreateSpecialDaySlots(businessHours)
                    };
                }
                else
                {
                    var equipmentPattern = _random.Next(4);
                    slotDef = equipmentPattern switch
                    {
                        0 => new AppointmentSlotRoot { Slots = new List<AppointmentSlotItem>() }, // 臨時休診
                        1 => CreateIncreasedCapacitySlots(businessHours, multiplier: 1.2 + _random.NextDouble() * 0.3),
                        2 => CreateExtendedHoursSlots(businessHours),
                        _ => new AppointmentSlotRoot { Slots = new List<AppointmentSlotItem>() } // 点検日
                    };
                }

                if (slotDef != null)
                {
                    overrides.Add(CreateOverride(selectedResource.ApptResId, selectedDate, slotDef, dateTimeProvider));
                }
            }

            // 次の月へ
            currentMonth = currentMonth.AddMonths(1);
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
        var businessStart = TimeOnly.Parse(businessHours.Start ?? BusinessHoursConstants.DefaultStartTime);
        var businessEnd = TimeOnly.Parse(businessHours.End ?? BusinessHoursConstants.DefaultEndTime);
        var lunchStart = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.Start) : TimeOnly.Parse(BusinessHoursConstants.DefaultLunchStartTime);
        var lunchEnd = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.End) : TimeOnly.Parse(BusinessHoursConstants.DefaultLunchEndTime);

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
                    Start = currentTime.Hour * 60 + currentTime.Minute,
                    End = endTime.Hour * 60 + endTime.Minute,
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
                    Start = currentTime.Hour * 60 + currentTime.Minute,
                    End = endTime.Hour * 60 + endTime.Minute,
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
        var businessStart = TimeOnly.Parse(businessHours.Start ?? BusinessHoursConstants.DefaultStartTime);
        var businessEnd = TimeOnly.Parse(businessHours.End ?? BusinessHoursConstants.DefaultEndTime);
        var lunchStart = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.Start) : TimeOnly.Parse(BusinessHoursConstants.DefaultLunchStartTime);
        var lunchEnd = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.End) : TimeOnly.Parse(BusinessHoursConstants.DefaultLunchEndTime);

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
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
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
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
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
        var businessStart = TimeOnly.Parse(businessHours.Start ?? BusinessHoursConstants.DefaultStartTime);
        var businessEnd = TimeOnly.Parse(businessHours.End ?? BusinessHoursConstants.DefaultEndTime);
        var lunchStart = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.Start) : TimeOnly.Parse(BusinessHoursConstants.DefaultLunchStartTime);
        var lunchEnd = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.End) : TimeOnly.Parse(BusinessHoursConstants.DefaultLunchEndTime);

        // 早朝スロット（07:00-09:00、20分区切り）
        var earlyStart = new TimeOnly(7, 0);
        var currentTime = earlyStart;
        while (currentTime < businessStart)
        {
            var endTime = currentTime.AddMinutes(20);
            if (endTime > businessStart) endTime = businessStart;

            slots.Add(new AppointmentSlotItem
            {
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
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
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
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
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
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
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
                Cap = 1,
                IsOutsideHours = true
            });
            currentTime = endTime;
        }

        return new AppointmentSlotRoot { Slots = slots };
    }

    /// <summary>
    /// 閑散期（1-3月、7-8月）かどうかを判定
    /// </summary>
    private static bool IsOffSeasonMonth(int month)
    {
        return month is >= 1 and <= 3 or >= 7 and <= 8;
    }

    /// <summary>
    /// 指定範囲内の営業日をすべて取得
    /// </summary>
    private static List<DateOnly> GetBusinessDaysInMonth(
        DateOnly startDate,
        DateOnly endDate,
        Dictionary<Guid, HashSet<DateOnly>> holidaysByFacility)
    {
        var businessDays = new List<DateOnly>();
        var allHolidays = new HashSet<DateOnly>(holidaysByFacility.Values.SelectMany(h => h));

        var currentDate = startDate;
        while (currentDate <= endDate)
        {
            // 営業日判定（日曜・祝日以外）
            if (currentDate.DayOfWeek != DayOfWeek.Sunday && !allHolidays.Contains(currentDate))
            {
                businessDays.Add(currentDate);
            }
            currentDate = currentDate.AddDays(1);
        }

        return businessDays;
    }

    /// <summary>
    /// 指定範囲内のランダムな営業日を取得
    /// </summary>
    private static DateOnly GetRandomBusinessDay(
        DateOnly startDate,
        DateOnly endDate,
        Dictionary<Guid, HashSet<DateOnly>> holidaysByFacility)
    {
        var attempts = 0;
        var maxAttempts = 30;
        var allHolidays = new HashSet<DateOnly>(holidaysByFacility.Values.SelectMany(h => h));

        while (attempts < maxAttempts)
        {
            // DateTime を使って日数差を計算
            var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            var endDateTime = endDate.ToDateTime(TimeOnly.MinValue);
            var daysRange = (int)(endDateTime - startDateTime).TotalDays + 1;

            if (daysRange <= 0) return default;

            var randomDays = _random.Next(0, daysRange);
            var randomDate = startDate.AddDays(randomDays);

            // 営業日判定（日曜・祝日以外）
            if (randomDate.DayOfWeek != DayOfWeek.Sunday && !allHolidays.Contains(randomDate))
            {
                return randomDate;
            }

            attempts++;
        }

        return default;
    }

    /// <summary>
    /// 特別営業日のスロットを作成（15分区切り、キャパシティ増加）
    /// </summary>
    private static AppointmentSlotRoot CreateSpecialDaySlots(
        FacilityBusinessHoursRoot businessHours)
    {
        var slots = new List<AppointmentSlotItem>();
        var businessStart = TimeOnly.Parse(businessHours.Start ?? BusinessHoursConstants.DefaultStartTime);
        var businessEnd = TimeOnly.Parse(businessHours.End ?? BusinessHoursConstants.DefaultEndTime);
        var lunchStart = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.Start) : TimeOnly.Parse(BusinessHoursConstants.DefaultLunchStartTime);
        var lunchEnd = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.End) : TimeOnly.Parse(BusinessHoursConstants.DefaultLunchEndTime);

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
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
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
                Start = currentTime.Hour * 60 + currentTime.Minute,
                End = endTime.Hour * 60 + endTime.Minute,
                Cap = maxPerSlot
            });
            currentTime = endTime;
        }

        return new AppointmentSlotRoot { Slots = slots };
    }
}

