using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentSeeder
{
    private static readonly Random _random = new Random();

    // Slot-level aggregation: (ResId, Date, SlotStartMin) -> Count
    private static Dictionary<(Guid ApptResId, DateOnly ApptDate, int SlotStartMin), int> _slotAggregation = new();

    public static async Task<Dictionary<(Guid ApptResId, DateOnly ApptDate, int SlotStartMin), int>> SeedAsync(MedockDbContext context, Guid floorId, IDateTimeProvider dateTimeProvider)
    {
        // テーブルが存在するか確認
        try
        {
            var hasExistingData = await context.Appointments.AnyAsync();
            if (hasExistingData)
            {
                Console.WriteLine("[SKIP] Appointments already exist.");
                return new Dictionary<(Guid, DateOnly, int), int>();  // Empty aggregation
            }
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
        {
            // テーブルが存在しない場合は続行（初回実行時）
        }

        var floor = await context.Floors.FirstOrDefaultAsync(f => f.FloorId == floorId);
        if (floor == null)
        {
            Console.WriteLine("[SKIP] Appointment: Floor not found.");
            return new Dictionary<(Guid, DateOnly, int), int>();
        }

        var patients = await context.Patients
            .Where(p => !p.IsDeleted && p.FacilityId == floor.FacilityId)
            .ToListAsync();

        if (!patients.Any())
        {
            Console.WriteLine("[SKIP] Appointment: No patients found.");
            return new Dictionary<(Guid, DateOnly, int), int>();
        }

        var organizations = await context.Organizations
            .Where(o => !o.IsDeleted && o.FacilityId == floor.FacilityId)
            .ToListAsync();

        if (!organizations.Any())
        {
            Console.WriteLine("[SKIP] Appointment: No organizations found.");
            return new Dictionary<(Guid, DateOnly, int), int>();
        }

        var holidays = await SeederHelper.LoadHolidaySetAsync(context);

        // メインリソースを取得
        var mainResource = await context.AppointmentResources
            .FirstOrDefaultAsync(r => r.ApptResTypeCode == (int)AppointmentResourceType.Main);

        if (mainResource == null)
        {
            Console.WriteLine("[SKIP] Appointment: Main resource not found.");
            return new Dictionary<(Guid, DateOnly, int), int>();
        }

        // Equipment リソースを取得
        var equipmentResources = await context.AppointmentResources
            .Where(r => r.ApptResTypeCode == (int)AppointmentResourceType.Equipment && !r.IsDeleted)
            .ToListAsync();

        var (startDate, endDate) = SeederHelper.GetDefaultDateRange(dateTimeProvider);

        // スケジュールを取得（スロット容量を計算するため）
        var schedules = await context.AppointmentSchedules
            .AsNoTracking()
            .Include(s => s.AppointmentScheduleSlots)
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToListAsync();

        Console.WriteLine("[INFO] Creating appointment seed data...");
        var stopwatch = Stopwatch.StartNew();

        // Initialize slot aggregation dictionary
        _slotAggregation = new Dictionary<(Guid, DateOnly, int), int>();

        // バッチ数を正確に計算（進捗表示用）
        // 3ヶ月ごとのバッチで何バッチ必要かを実際に数える
        var totalBatches = 0;
        var countBatchDate = startDate;
        while (countBatchDate <= endDate)
        {
            totalBatches++;
            countBatchDate = countBatchDate.AddMonths(3);
        }
        var currentBatch = 0;

        // 3ヶ月ごとにバッチ処理
        var batchStartDate = startDate;
        var batchEndDate = startDate.AddMonths(3).AddDays(-1);
        if (batchEndDate > endDate) batchEndDate = endDate;

        var totalAppointments = 0;
        var today = dateTimeProvider.TodayDateOnly;

        while (batchStartDate <= endDate)
        {
            currentBatch++;
            var batchStopwatch = Stopwatch.StartNew();
            var appointments = new List<Appointment>();
            var resourceAssignments = new List<AppointmentResourceAssignment>();
            var currentDate = batchStartDate;

            while (currentDate <= batchEndDate && currentDate <= endDate)
            {
                var isToday = currentDate == today;
                var dayContext = GetDayContext(currentDate, holidays, isToday);

                // イレギュラーチェック（約1%の確率で例外）
                var isIrregular = _random.Next(100) < 1;
                if (isIrregular)
                {
                    // イレギュラーデータを生成
                    dayContext = ApplyIrregularRule(dayContext, currentDate);
                }

                // 営業日の場合は予約を生成
                if (dayContext.IsOpen)
                {
                    var (appointmentsForDay, assignmentsForDay, dayAggregation) = GenerateAppointmentsForDay(
                        currentDate,
                        dayContext,
                        floor,
                        patients,
                        organizations,
                        mainResource,
                        equipmentResources,
                        schedules,
                        dateTimeProvider,
                        isToday);

                    appointments.AddRange(appointmentsForDay);
                    resourceAssignments.AddRange(assignmentsForDay);
                    // dayAggregation is already merged into _slotAggregation in GenerateAppointmentsForDay
                }

                currentDate = currentDate.AddDays(1);
            }

            if (appointments.Any())
            {
                var appointmentSw = Stopwatch.StartNew();
                await context.BulkInsertAsync(appointments, new BulkConfig
                {
                    SetOutputIdentity = false,
                    BatchSize = 5000  // Increased for better throughput
                });
                appointmentSw.Stop();
                totalAppointments += appointments.Count;

                // Insert resource assignments immediately after appointments
                var assignmentSw = Stopwatch.StartNew();
                if (resourceAssignments.Any())
                {
                    await context.BulkInsertAsync(resourceAssignments, new BulkConfig
                    {
                        SetOutputIdentity = false,
                        BatchSize = 5000  // Increased for better throughput
                    });
                }
                assignmentSw.Stop();

                batchStopwatch.Stop();
                var progressPercent = (int)((double)currentBatch / totalBatches * 100);
                Console.WriteLine($"  [BATCH] {currentBatch}/{totalBatches} ({progressPercent}%) - Appointments: {appointments.Count} ({appointmentSw.Elapsed.TotalSeconds:F2}s), Assignments: {resourceAssignments.Count} ({assignmentSw.Elapsed.TotalSeconds:F2}s)");
            }

            // 次のバッチへ
            batchStartDate = batchEndDate.AddDays(1);
            batchEndDate = batchStartDate.AddMonths(3).AddDays(-1);
            if (batchEndDate > endDate) batchEndDate = endDate;
        }

        stopwatch.Stop();
        Console.WriteLine($"  [+] Appointments: {totalAppointments} entries total (from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}) - took {stopwatch.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"  [+] Aggregated {_slotAggregation.Count} unique slot assignments");

        return _slotAggregation;
    }

    /// <summary>
    /// 日付の営業コンテキストを取得
    /// </summary>
    private static AppointmentDayContext GetDayContext(DateOnly date, HashSet<DateOnly> holidays, bool isToday = false)
    {
        var isHoliday = holidays.Contains(date);

        // 今日の場合は強制的に営業日として返す（デフォルトは通常営業、但し日曜・祝日は午前のみ）
        if (isToday)
        {
            // 今日が日曜・祝日の場合は、午前のみとして返す
            // 水曜・土曜でも全日営業に変更（オーバーライド）
            bool isTodayHolidayOrSunday = SeederHelper.IsSunday(date) || isHoliday;

            return new AppointmentDayContext
            {
                IsOpen = true,
                IsMorningOnly = isTodayHolidayOrSunday,  // 日曜・祝日のみ午前のみ、他は全日営業
                IsIrregular = false
            };
        }

        // 日曜または祝日は休み
        if (SeederHelper.IsSunday(date) || isHoliday)
        {
            return new AppointmentDayContext
            {
                IsOpen = false,
                IsMorningOnly = false,
                IsIrregular = false
            };
        }

        // 水曜・土曜は午前のみ
        if (SeederHelper.IsHalfDay(date))
        {
            return new AppointmentDayContext
            {
                IsOpen = true,
                IsMorningOnly = true,
                IsIrregular = false
            };
        }

        // 平日（月・火・木・金）は全日営業
        return new AppointmentDayContext
        {
            IsOpen = true,
            IsMorningOnly = false,
            IsIrregular = false
        };
    }

    /// <summary>
    /// イレギュラールールを適用（約1%の確率）
    /// </summary>
    private static AppointmentDayContext ApplyIrregularRule(AppointmentDayContext context, DateOnly date)
    {
        var irregularType = _random.Next(3);

        return irregularType switch
        {
            0 => // 日曜や休みの日に臨時営業
                new AppointmentDayContext
                {
                    IsOpen = !context.IsOpen, // 休みの日を営業日に
                    IsMorningOnly = context.IsOpen ? context.IsMorningOnly : false, // 休みだった場合は全日営業
                    IsIrregular = true
                },
            1 => // 水曜・土曜の午後も営業
                new AppointmentDayContext
                {
                    IsOpen = context.IsOpen,
                    IsMorningOnly = context.IsMorningOnly ? false : context.IsMorningOnly, // 午前のみ→全日に
                    IsIrregular = true
                },
            _ => // 平日が臨時休診
                new AppointmentDayContext
                {
                    IsOpen = context.IsOpen ? false : context.IsOpen, // 営業日を休みに
                    IsMorningOnly = false,
                    IsIrregular = true
                }
        };
    }

    /// <summary>
    /// 1日分の予約データを生成
    /// </summary>
    private static (List<Appointment>, List<AppointmentResourceAssignment>, Dictionary<(Guid ApptResId, DateOnly ApptDate, int SlotStartMin), int>) GenerateAppointmentsForDay(
        DateOnly date,
        AppointmentDayContext dayContext,
        Floor floor,
        List<Patient> patients,
        List<Organization> organizations,
        AppointmentResource mainResource,
        List<AppointmentResource> equipmentResources,
        List<AppointmentSchedule> schedules,
        IDateTimeProvider dateTimeProvider,
        bool isToday = false)
    {
        var appointments = new List<Appointment>();
        var resourceAssignments = new List<AppointmentResourceAssignment>();
        var daySlotAggregation = new Dictionary<(Guid ApptResId, DateOnly ApptDate, int SlotStartMin), int>();

        // その日に該当する曜日のスロット情報をリソース別に取得
        var dayOfWeek = (int)date.DayOfWeek;
        List<(int SlotStartMin, int SlotEndMin, int SlotCap)> mainSlots = new();
        var equipmentSlots = new Dictionary<Guid, List<(int SlotStartMin, int SlotEndMin, int SlotCap)>>();

        foreach (var schedule in schedules)
        {
            var daySlots = schedule.AppointmentScheduleSlots
                .Where(s => !s.IsDeleted && s.DaysOfWeek.Contains(dayOfWeek))
                .ToList();

            if (schedule.ApptResId == mainResource.ApptResId)
            {
                mainSlots = daySlots.Select(s => (s.SlotStartMin, s.SlotEndMin, s.SlotCap)).ToList();
            }
            else
            {
                equipmentSlots[schedule.ApptResId] = daySlots.Select(s => (s.SlotStartMin, s.SlotEndMin, s.SlotCap)).ToList();
            }
        }

        // 月による繁忙度調整（占有率）- ベース値
        decimal baseOccupancyRate = date.Month switch
        {
            1 => 0.25m,   // 1月：閑散期（正月）
            2 => 0.60m,   // 2月：通常期
            3 => 0.65m,   // 3月：入職時検診に向けて増加
            4 => 0.85m,   // 4月：入職時検診（中程度）
            5 => 1.0m,    // 5月：入職時検診（ピーク）
            6 => 0.80m,   // 6月：入職時検診（下降）
            7 => 0.50m,   // 7月：やや閑散期（夏季）
            8 => 0.30m,   // 8月：閑散期（盆休み）
            9 => 0.85m,   // 9月：秋の健診（開始、中程度）
            10 => 1.0m,   // 10月：秋の健診（ピーク）
            11 => 0.90m,  // 11月：秋の健診（緩やかに下降）
            12 => 0.70m,  // 12月：秋の健診（更に下降、年末へ）
            _ => 0.60m
        };

        // 曜日による変動（月曜は混雑、金曜はやや空き）
        decimal dayOfWeekModifier = date.DayOfWeek switch
        {
            DayOfWeek.Monday => 1.15m,    // 月曜：週明けで混雑
            DayOfWeek.Tuesday => 1.05m,   // 火曜：やや混雑
            DayOfWeek.Wednesday => 0.90m, // 水曜：午前のみなので調整済み
            DayOfWeek.Thursday => 1.0m,   // 木曜：平均的
            DayOfWeek.Friday => 0.85m,    // 金曜：週末前で空きやすい
            DayOfWeek.Saturday => 0.95m,  // 土曜：午前のみだが需要あり
            _ => 1.0m
        };

        // 月内の位置による変動（月初・月中・月末）
        decimal weekOfMonthModifier = date.Day switch
        {
            <= 7 => 1.1m,    // 月初：やや混雑
            <= 14 => 0.95m,  // 第2週：落ち着く
            <= 21 => 1.05m,  // 第3週：やや回復
            _ => 0.90m       // 月末：空きやすい
        };

        // 日ごとのランダム変動（±25%）
        decimal dailyRandomVariation = 0.75m + (decimal)(_random.NextDouble() * 0.50);

        // 最終的な占有率を計算（上限1.0）
        decimal occupancyRate = Math.Min(1.0m, baseOccupancyRate * dayOfWeekModifier * weekOfMonthModifier * dailyRandomVariation);

        // Main リソース：各スロットごとに予約数を計算（容量 × 占有率 × 時間帯乗数 + スロット変動 + 超過）
        var slotAppointmentCounts = new List<int>();
        bool hasFullSlot = false;
        bool hasEmptySlot = false;
        bool hasOvercapacitySlot = false;

        for (int i = 0; i < mainSlots.Count; i++)
        {
            var slot = mainSlots[i];
            int slotCount;

            // 今日の場合、特別な処理
            if (isToday)
            {
                if (!hasFullSlot)
                {
                    // 最初のスロット → 満杯
                    slotCount = slot.SlotCap;
                    hasFullSlot = true;
                }
                else if (!hasEmptySlot)
                {
                    // 2番目のスロット → 空き
                    slotCount = 0;
                    hasEmptySlot = true;
                }
                else if (!hasOvercapacitySlot)
                {
                    // 3番目のスロット → キャパオーバー
                    slotCount = slot.SlotCap + 2;
                    hasOvercapacitySlot = true;
                }
                else
                {
                    // 4番目以降 → 通常の計算
                    var timeModifier = GetTimeModifier(slot.SlotStartMin);
                    decimal slotRandomVariation = 0.70m + (decimal)(_random.NextDouble() * 0.60);
                    var baseCount = (int)(slot.SlotCap * occupancyRate * timeModifier * slotRandomVariation);
                    int overage = 0;
                    if (occupancyRate >= 0.90m)
                    {
                        overage = _random.Next(100) < 30 ? _random.Next(1, 3) : 0;
                    }
                    else if (occupancyRate >= 0.70m)
                    {
                        overage = _random.Next(100) < 10 ? 1 : 0;
                    }
                    if (_random.Next(100) < 5)
                    {
                        baseCount = baseCount / 2;
                    }
                    var maxAllowed = slot.SlotCap + 2;
                    slotCount = Math.Clamp(baseCount + overage, 0, maxAllowed);
                }
            }
            else
            {
                // 通常の処理
                var timeModifier = GetTimeModifier(slot.SlotStartMin);

                // スロットごとのランダム変動（±30%）- 各スロットで独立
                decimal slotRandomVariation = 0.70m + (decimal)(_random.NextDouble() * 0.60);

                var baseCount = (int)(slot.SlotCap * occupancyRate * timeModifier * slotRandomVariation);

                // 超過を追加（繁忙期は満室超過あり、通常期は稀に超過）- 最大2件まで
                int overage = 0;
                if (occupancyRate >= 0.90m)
                {
                    // 繁忙期: 30%の確率で1-2件超過
                    overage = _random.Next(100) < 30 ? _random.Next(1, 3) : 0;
                }
                else if (occupancyRate >= 0.70m)
                {
                    // 中程度: 10%の確率で1件超過
                    overage = _random.Next(100) < 10 ? 1 : 0;
                }

                // 稀に極端に空いているスロット（5%の確率で半減）
                if (_random.Next(100) < 5)
                {
                    baseCount = baseCount / 2;
                }

                // ハードリミット: キャパ+2を超えないように制限
                var maxAllowed = slot.SlotCap + 2;
                slotCount = Math.Clamp(baseCount + overage, 0, maxAllowed);
            }

            slotAppointmentCounts.Add(slotCount);
        }
        var mainAppointmentCount = slotAppointmentCounts.Sum();

        // Equipment リソースも時間帯乗数とスロット変動を考慮して計算
        var equipmentAppointmentCounts = new Dictionary<Guid, int>();
        foreach (var (resId, slots) in equipmentSlots)
        {
            var equipmentCount = 0;
            foreach (var slot in slots)
            {
                var timeModifier = GetTimeModifier(slot.SlotStartMin);
                // Equipment もスロットごとに独立した変動（±35%）
                decimal equipSlotVariation = 0.65m + (decimal)(_random.NextDouble() * 0.70);
                var slotCount = (int)(slot.SlotCap * occupancyRate * timeModifier * equipSlotVariation);
                equipmentCount += slotCount;
            }
            equipmentAppointmentCounts[resId] = Math.Max(0, equipmentCount);
        }

        var appointmentCount = mainAppointmentCount;

        // Equipment の現在カウントを追跡
        var equipmentCurrentCounts = new Dictionary<Guid, int>();
        foreach (var resId in equipmentAppointmentCounts.Keys)
        {
            equipmentCurrentCounts[resId] = 0;
        }

        // Main リソースのスロット時間帯を使用（複数回参照されるため事前に用意）
        var mainSlotTimes = mainSlots.Count > 0
            ? mainSlots.SelectMany(slot => Enumerable.Range(0, 1).Select(_ => slot)).ToList()
            : new List<(int SlotStartMin, int SlotEndMin, int SlotCap)>();

        int slotIndex = 0;
        int slotAppointmentIndex = 0;

        for (int i = 0; i < appointmentCount; i++)
        {
            var patient = patients[_random.Next(patients.Count)];
            var organization = organizations[_random.Next(organizations.Count)];

            // 時間帯を決定（分単位）
            int? startMin = null;
            int? assignedSlotStartMin = null;  // スロット内の場合、所属スロットの SlotStartMin

            // Main リソースのスロット情報がある場合はそれを使用
            if (mainSlots.Count > 0 && slotIndex < mainSlots.Count && slotAppointmentIndex < slotAppointmentCounts[slotIndex])
            {
                // 現在のスロット内でランダムな時刻を生成
                var slot = mainSlots[slotIndex];
                var slotDurationMin = slot.SlotEndMin - slot.SlotStartMin;
                var randomOffset = _random.Next(0, Math.Max(1, slotDurationMin - 15)); // 最大15分のバッファ
                startMin = slot.SlotStartMin + randomOffset;
                assignedSlotStartMin = slot.SlotStartMin;  // このスロットに所属
                slotAppointmentIndex++;

                // 次のスロットへ
                if (slotAppointmentIndex >= slotAppointmentCounts[slotIndex])
                {
                    slotIndex++;
                    slotAppointmentIndex = 0;
                }
            }
            else if (dayContext.IsMorningOnly)
            {
                // 午前のみ（09:00-12:00）
                var morningTimes = SeederHelper.TimeSlots.MorningSlots;
                var timeStr = morningTimes[_random.Next(morningTimes.Length)];
                var parsedMin = TimeConstants.TryTimeStringToMinutes(timeStr);
                if (parsedMin.HasValue)
                {
                    startMin = parsedMin.Value;
                }
            }
            else
            {
                // 全日営業
                var allTimes = new List<string>();
                allTimes.AddRange(SeederHelper.TimeSlots.MorningSlots);
                allTimes.AddRange(SeederHelper.TimeSlots.AfternoonSlots);

                var timeStr = allTimes[_random.Next(allTimes.Count)];
                var parsedMin = TimeConstants.TryTimeStringToMinutes(timeStr);
                if (parsedMin.HasValue)
                {
                    startMin = parsedMin.Value;
                }
            }

            if (!startMin.HasValue)
            {
                continue;
            }

            // 予約ステータス（約95%が予約済み、5%がその他）
            var statusCode = _random.Next(100) < 95 ? 0 : _random.Next(1, 5);

            var appointment = new Appointment
            {
                ApptId = Guid.CreateVersion7(),
                FloorId = floor.FloorId,
                OrgId = organization.OrgId,
                PtId = patient.PtId,
                ApptDate = date,
                ApptStartMin = startMin.Value,
                ApptStatusCode = statusCode,
                ApptMemo = dayContext.IsIrregular ? "イレギュラー営業" : String.Empty,
                IsDeleted = false
            };

            SeederHelper.InitializeAuditFields(appointment, dateTimeProvider);
            appointments.Add(appointment);

            // Track aggregation: Use assigned slot if appointment is within configured slot, otherwise use actual startMin
            int aggregationKey = assignedSlotStartMin ?? startMin.Value;
            var agg = (mainResource.ApptResId, date, aggregationKey);
            if (daySlotAggregation.ContainsKey(agg))
            {
                daySlotAggregation[agg]++;
            }
            else
            {
                daySlotAggregation[agg] = 1;
            }
        }

        // 時間外スロットへの予約生成（低確率で生成、グラフには描画されないが赤い縦ラインで存在の有無を表示）
        // 今日の場合は必ず生成、それ以外は約10%の確率で生成

        // 早朝スロット（07:00-09:00）
        if (isToday || _random.Next(100) < 10)
        {
            var patient = patients[_random.Next(patients.Count)];
            var organization = organizations[_random.Next(organizations.Count)];
            var earlyMorningTimes = SeederHelper.TimeSlots.EarlyMorningSlots;
            var timeStr = earlyMorningTimes[_random.Next(earlyMorningTimes.Length)];
            var parsedMin = TimeConstants.TryTimeStringToMinutes(timeStr);
            if (parsedMin.HasValue)
            {
                var startMin = parsedMin.Value;

                var appointment = new Appointment
                {
                    ApptId = Guid.CreateVersion7(),
                    FloorId = floor.FloorId,
                    OrgId = organization.OrgId,
                    PtId = patient.PtId,
                    ApptDate = date,
                    ApptStartMin = startMin,
                    ApptStatusCode = _random.Next(100) < 95 ? 0 : _random.Next(1, 5),
                    ApptMemo = String.Empty,
                    IsDeleted = false
                };

                SeederHelper.InitializeAuditFields(appointment, dateTimeProvider);
                appointments.Add(appointment);

                // Track aggregation for out-of-hours appointment
                var outOfHoursAgg = (mainResource.ApptResId, date, startMin);
                if (daySlotAggregation.ContainsKey(outOfHoursAgg))
                {
                    daySlotAggregation[outOfHoursAgg]++;
                }
                else
                {
                    daySlotAggregation[outOfHoursAgg] = 1;
                }
            }
        }

        // 昼休みスロット（12:00-13:00）
        if (isToday || _random.Next(100) < 10)
        {
            var patient = patients[_random.Next(patients.Count)];
            var organization = organizations[_random.Next(organizations.Count)];
            var lunchTimes = SeederHelper.TimeSlots.LunchSlots;
            var timeStr = lunchTimes[_random.Next(lunchTimes.Length)];
            var parsedMin = TimeConstants.TryTimeStringToMinutes(timeStr);
            if (parsedMin.HasValue)
            {
                var startMin = parsedMin.Value;

                var appointment = new Appointment
                {
                    ApptId = Guid.CreateVersion7(),
                    FloorId = floor.FloorId,
                    OrgId = organization.OrgId,
                    PtId = patient.PtId,
                    ApptDate = date,
                    ApptStartMin = startMin,
                    ApptStatusCode = _random.Next(100) < 95 ? 0 : _random.Next(1, 5),
                    ApptMemo = String.Empty,
                    IsDeleted = false
                };

                SeederHelper.InitializeAuditFields(appointment, dateTimeProvider);
                appointments.Add(appointment);

                // Track aggregation for out-of-hours appointment
                var outOfHoursAgg = (mainResource.ApptResId, date, startMin);
                if (daySlotAggregation.ContainsKey(outOfHoursAgg))
                {
                    daySlotAggregation[outOfHoursAgg]++;
                }
                else
                {
                    daySlotAggregation[outOfHoursAgg] = 1;
                }
            }
        }

        // 夕方スロット（17:00-18:00）
        // ただし、午前のみの日（水曜・土曜）は除外（営業時間が17:00までに満たない）
        if (!dayContext.IsMorningOnly && (isToday || _random.Next(100) < 10))
        {
            var patient = patients[_random.Next(patients.Count)];
            var organization = organizations[_random.Next(organizations.Count)];
            // 17:00-17:45の範囲で生成（15分単位）
            var eveningTimeStrs = new[] { "17:00", "17:15", "17:30", "17:45" };
            var timeStr = eveningTimeStrs[_random.Next(eveningTimeStrs.Length)];
            var parsedMin = TimeConstants.TryTimeStringToMinutes(timeStr);
            if (parsedMin.HasValue)
            {
                var startMin = parsedMin.Value;

                var appointment = new Appointment
                {
                    ApptId = Guid.CreateVersion7(),
                    FloorId = floor.FloorId,
                    OrgId = organization.OrgId,
                    PtId = patient.PtId,
                    ApptDate = date,
                    ApptStartMin = startMin,
                    ApptStatusCode = _random.Next(100) < 95 ? 0 : _random.Next(1, 5),
                    ApptMemo = String.Empty,
                    IsDeleted = false
                };

                SeederHelper.InitializeAuditFields(appointment, dateTimeProvider);
                appointments.Add(appointment);

                // Track aggregation for out-of-hours appointment
                var outOfHoursAgg = (mainResource.ApptResId, date, startMin);
                if (daySlotAggregation.ContainsKey(outOfHoursAgg))
                {
                    daySlotAggregation[outOfHoursAgg]++;
                }
                else
                {
                    daySlotAggregation[outOfHoursAgg] = 1;
                }
            }
        }

        // 各appointmentに対してリソース割り当てを作成
        foreach (var appointment in appointments)
        {
            // 1. 必ずメインリソースのassignmentを作成
            var mainAssignment = new AppointmentResourceAssignment
            {
                ApptResAssignId = Guid.CreateVersion7(),
                ApptId = appointment.ApptId,
                ApptResId = mainResource.ApptResId,
                IsDeleted = false
            };

            SeederHelper.InitializeAuditFields(mainAssignment, dateTimeProvider);
            resourceAssignments.Add(mainAssignment);

            // 2. Equipmentリソースをランダムに追加（50%の確率で、上限を超えない範囲で）
            if (equipmentResources.Any() && _random.Next(100) < 50)
            {
                // 利用可能なEquipmentをフィルタ（上限に余裕があるもの）
                var availableEquipments = equipmentResources
                    .Where(e => equipmentCurrentCounts.ContainsKey(e.ApptResId) &&
                                equipmentCurrentCounts[e.ApptResId] < equipmentAppointmentCounts[e.ApptResId])
                    .ToList();

                if (availableEquipments.Any())
                {
                    // 選択するEquipmentリソースの数をランダムに決定（最大3個、ただし利用可能な範囲内）
                    var maxEquipmentCount = Math.Min(3, availableEquipments.Count);
                    var equipmentCount = _random.Next(0, maxEquipmentCount + 1);

                    if (equipmentCount > 0)
                    {
                        // ランダムに選んだEquipmentリソースをシャッフルして先頭から選ぶ
                        var selectedEquipments = availableEquipments
                            .OrderBy(_ => _random.Next())
                            .Take(equipmentCount)
                            .ToList();


                        foreach (var equipment in selectedEquipments)
                        {
                            // 上限チェック（念のため）
                            if (equipmentCurrentCounts[equipment.ApptResId] < equipmentAppointmentCounts[equipment.ApptResId])
                            {
                                var equipmentAssignment = new AppointmentResourceAssignment
                                {
                                    ApptResAssignId = Guid.CreateVersion7(),
                                    ApptId = appointment.ApptId,
                                    ApptResId = equipment.ApptResId,
                                    IsDeleted = false
                                };

                                SeederHelper.InitializeAuditFields(equipmentAssignment, dateTimeProvider);
                                resourceAssignments.Add(equipmentAssignment);

                                // Track Equipment resource assignment in aggregation
                                // Determine the correct slot start time (aggregation key) based on equipment's schedule
                                // This matches the logic used for Main resources (line 500): use slot start time if within a configured slot,
                                // otherwise use the actual appointment start time (for out-of-hours appointments)
                                var aggTime = appointment.ApptStartMin;
                                if (equipmentSlots.TryGetValue(equipment.ApptResId, out var slots) && slots.Any())
                                {
                                    // Find if this appointment time falls into any configured slot for this equipment
                                    // Match logic: appointment time >= slot start AND < slot end
                                    var matchedSlot = slots.FirstOrDefault(s => aggTime >= s.SlotStartMin && aggTime < s.SlotEndMin);

                                    // Check if a valid slot was matched (not default tuple value)
                                    // Default tuple is (0, 0, 0), but we also need to handle edge case where 0:00 might be a valid slot
                                    // More robust check: ensure both SlotStartMin and SlotEndMin are set (SlotEndMin > SlotStartMin)
                                    if (matchedSlot.SlotEndMin > matchedSlot.SlotStartMin)
                                    {
                                        aggTime = matchedSlot.SlotStartMin;
                                    }
                                    // If no slot matched, aggTime remains as appointment.ApptStartMin (out-of-hours appointment)
                                }

                                var equipmentAgg = (equipment.ApptResId, appointment.ApptDate!.Value, aggTime);
                                if (daySlotAggregation.ContainsKey(equipmentAgg))
                                {
                                    daySlotAggregation[equipmentAgg]++;
                                }
                                else
                                {
                                    daySlotAggregation[equipmentAgg] = 1;
                                }

                                // カウント加算
                                equipmentCurrentCounts[equipment.ApptResId]++;
                            }
                        }

                    }
                }
            }
        }

        // Merge day aggregation into global aggregation
        foreach (var kvp in daySlotAggregation)
        {
            if (_slotAggregation.ContainsKey(kvp.Key))
            {
                _slotAggregation[kvp.Key] += kvp.Value;
            }
            else
            {
                _slotAggregation[kvp.Key] = kvp.Value;
            }
        }

        return (appointments, resourceAssignments, daySlotAggregation);
    }

    /// <summary>
    /// スロット集計にカウントを追加
    /// </summary>
    private static void IncrementSlotAggregation(Guid apptResId, DateOnly apptDate, int apptStartMin)
    {
        var key = (apptResId, apptDate, apptStartMin);
        if (_slotAggregation.ContainsKey(key))
        {
            _slotAggregation[key]++;
        }
        else
        {
            _slotAggregation[key] = 1;
        }
    }

    /// <summary>
    /// スロット開始時刻に基づいて時間帯の乗数を計算（ベース値±15%の変動あり）
    /// 朝（9:00-11:00）：1.2倍（ピーク）
    /// 昼（11:00-13:00）：0.5倍（空いている）
    /// 夕方（13:00-17:00）：1.1倍（やや混雑）
    /// </summary>
    private static decimal GetTimeModifier(int slotStartMin)
    {
        decimal baseModifier = slotStartMin switch
        {
            // 朝のピーク：9:00-11:00（540-660分）
            >= 540 and < 660 => 1.2m,
            // 昼間：11:00-13:00（660-780分）
            >= 660 and < 780 => 0.5m,
            // 夕方：13:00-17:00（780-1020分）
            >= 780 and < 1020 => 1.1m,
            // その他
            _ => 1.0m
        };

        // 時間帯乗数にも±15%の変動を追加
        decimal variation = 0.85m + (decimal)(_random.NextDouble() * 0.30);
        return baseModifier * variation;
    }

    /// <summary>
    /// 予約日の営業コンテキスト
    /// </summary>
    private class AppointmentDayContext
    {
        public bool IsOpen { get; set; }
        public bool IsMorningOnly { get; set; }
        public bool IsIrregular { get; set; }
    }
}

