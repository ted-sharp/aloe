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

    public static async Task SeedAsync(MedockDbContext context, Guid floorId, IDateTimeProvider dateTimeProvider)
    {
        // テーブルが存在するか確認
        try
        {
            var hasExistingData = await context.Appointments.AnyAsync();
            if (hasExistingData)
            {
                Console.WriteLine("[SKIP] Appointments already exist.");
                return;
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
            return;
        }

        var patients = await context.Patients
            .Where(p => !p.IsDeleted && p.FacilityId == floor.FacilityId)
            .ToListAsync();

        if (!patients.Any())
        {
            Console.WriteLine("[SKIP] Appointment: No patients found.");
            return;
        }

        var organizations = await context.Organizations
            .Where(o => !o.IsDeleted && o.FacilityId == floor.FacilityId)
            .ToListAsync();

        if (!organizations.Any())
        {
            Console.WriteLine("[SKIP] Appointment: No organizations found.");
            return;
        }

        var holidays = await SeederHelper.LoadHolidaySetAsync(context);

        // メインリソースを取得
        var mainResource = await context.AppointmentResources
            .FirstOrDefaultAsync(r => r.ApptResTypeCode == (int)AppointmentResourceType.Main);

        if (mainResource == null)
        {
            Console.WriteLine("[SKIP] Appointment: Main resource not found.");
            return;
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

        // バッチ数を事前に計算（進捗表示用）
        var totalMonths = (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month) + 1;
        var totalBatches = (int)Math.Ceiling(totalMonths / 3.0);
        var currentBatch = 0;

        // 3ヶ月ごとにバッチ処理
        var batchStartDate = startDate;
        var batchEndDate = startDate.AddMonths(3).AddDays(-1);
        if (batchEndDate > endDate) batchEndDate = endDate;

        var totalAppointments = 0;
        var allResourceAssignments = new List<AppointmentResourceAssignment>();

        while (batchStartDate <= endDate)
        {
            currentBatch++;
            var batchStopwatch = Stopwatch.StartNew();
            var appointments = new List<Appointment>();
            var resourceAssignments = new List<AppointmentResourceAssignment>();
            var currentDate = batchStartDate;

            while (currentDate <= batchEndDate && currentDate <= endDate)
            {
                var dayContext = GetDayContext(currentDate, holidays);

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
                    var (appointmentsForDay, assignmentsForDay) = GenerateAppointmentsForDay(
                        currentDate,
                        dayContext,
                        floor,
                        patients,
                        organizations,
                        mainResource,
                        equipmentResources,
                        schedules,
                        dateTimeProvider);

                    appointments.AddRange(appointmentsForDay);
                    resourceAssignments.AddRange(assignmentsForDay);
                }

                currentDate = currentDate.AddDays(1);
            }

            if (appointments.Any())
            {
                await context.BulkInsertAsync(appointments, new BulkConfig
                {
                    SetOutputIdentity = false,
                    BatchSize = 1000
                });
                totalAppointments += appointments.Count;
                allResourceAssignments.AddRange(resourceAssignments);
                batchStopwatch.Stop();
                var progressPercent = (int)((double)currentBatch / totalBatches * 100);
                Console.WriteLine($"  [BATCH] {currentBatch}/{totalBatches} ({progressPercent}%) - Committed {appointments.Count} appointments ({batchStartDate:yyyy-MM-dd} to {batchEndDate:yyyy-MM-dd}) - took {batchStopwatch.Elapsed.TotalSeconds:F2}s");
            }

            // 次のバッチへ
            batchStartDate = batchEndDate.AddDays(1);
            batchEndDate = batchStartDate.AddMonths(3).AddDays(-1);
            if (batchEndDate > endDate) batchEndDate = endDate;
        }

        // リソース割り当てをまとめて挿入
        if (allResourceAssignments.Any())
        {
            await context.BulkInsertAsync(allResourceAssignments, new BulkConfig
            {
                SetOutputIdentity = false,
                BatchSize = 1000
            });
        }

        stopwatch.Stop();
        Console.WriteLine($"  [+] Appointments: {totalAppointments} entries total (from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}) - took {stopwatch.Elapsed.TotalSeconds:F2}s");
    }

    /// <summary>
    /// 日付の営業コンテキストを取得
    /// </summary>
    private static AppointmentDayContext GetDayContext(DateOnly date, HashSet<DateOnly> holidays)
    {
        var isHoliday = holidays.Contains(date);

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
    private static (List<Appointment>, List<AppointmentResourceAssignment>) GenerateAppointmentsForDay(
        DateOnly date,
        AppointmentDayContext dayContext,
        Floor floor,
        List<Patient> patients,
        List<Organization> organizations,
        AppointmentResource mainResource,
        List<AppointmentResource> equipmentResources,
        List<AppointmentSchedule> schedules,
        IDateTimeProvider dateTimeProvider)
    {
        var appointments = new List<Appointment>();
        var resourceAssignments = new List<AppointmentResourceAssignment>();

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

        // 月による繁忙度調整（占有率）
        decimal occupancyRate = date.Month switch
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

        // Main リソース：各スロットごとに予約数を計算（容量 × 占有率 × 時間帯乗数 + 超過 0-2）
        var slotAppointmentCounts = new List<int>();
        foreach (var slot in mainSlots)
        {
            // 時間帯に応じた乗数を適用（9:00-11:00がピーク、昼間は空いている、夕方やや混雑）
            var timeModifier = GetTimeModifier(slot.SlotStartMin);
            var baseCount = (int)(slot.SlotCap * occupancyRate * timeModifier);
            // 超過を追加（繁忙期は満室狙い、通常期は若干余裕）
            var overage = occupancyRate >= 0.95m ? _random.Next(0, 3) : 0; // 0, 1, 2
            var slotCount = baseCount + overage; // 時間帯乗数を正しく反映（上限なし）
            slotAppointmentCounts.Add(slotCount);
        }
        var mainAppointmentCount = slotAppointmentCounts.Sum();

        // Equipment リソースも時間帯乗数を考慮して計算（超過なし）
        var equipmentAppointmentCounts = new Dictionary<Guid, int>();
        foreach (var (resId, slots) in equipmentSlots)
        {
            var equipmentCount = 0;
            foreach (var slot in slots)
            {
                var timeModifier = GetTimeModifier(slot.SlotStartMin);
                var slotCount = (int)(slot.SlotCap * occupancyRate * timeModifier);
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

            // Main リソースのスロット情報がある場合はそれを使用
            if (mainSlots.Count > 0 && slotIndex < mainSlots.Count && slotAppointmentIndex < slotAppointmentCounts[slotIndex])
            {
                // 現在のスロット内でランダムな時刻を生成
                var slot = mainSlots[slotIndex];
                var slotDurationMin = slot.SlotEndMin - slot.SlotStartMin;
                var randomOffset = _random.Next(0, Math.Max(1, slotDurationMin - 15)); // 最大15分のバッファ
                startMin = slot.SlotStartMin + randomOffset;
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
                if (TimeOnly.TryParse(timeStr, out var parsedTime))
                {
                    startMin = parsedTime.Hour * 60 + parsedTime.Minute;
                }
            }
            else
            {
                // 全日営業
                var allTimes = new List<string>();
                allTimes.AddRange(SeederHelper.TimeSlots.MorningSlots);
                allTimes.AddRange(SeederHelper.TimeSlots.AfternoonSlots);

                var timeStr = allTimes[_random.Next(allTimes.Count)];
                if (TimeOnly.TryParse(timeStr, out var parsedTime))
                {
                    startMin = parsedTime.Hour * 60 + parsedTime.Minute;
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
        }

        // 時間外スロットへの予約生成（低確率で生成、グラフには描画されないが赤い縦ラインで存在の有無を表示）
        // 早朝スロット（07:00-09:00）：約10%の確率で1件生成
        if (_random.Next(100) < 10)
        {
            var patient = patients[_random.Next(patients.Count)];
            var organization = organizations[_random.Next(organizations.Count)];
            var earlyMorningTimes = SeederHelper.TimeSlots.EarlyMorningSlots;
            var timeStr = earlyMorningTimes[_random.Next(earlyMorningTimes.Length)];
            if (TimeOnly.TryParse(timeStr, out var parsedTime))
            {
                var startMin = parsedTime.Hour * 60 + parsedTime.Minute;

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
            }
        }

        // 昼休みスロット（12:00-13:00）：約10%の確率で1件生成
        if (_random.Next(100) < 10)
        {
            var patient = patients[_random.Next(patients.Count)];
            var organization = organizations[_random.Next(organizations.Count)];
            var lunchTimes = SeederHelper.TimeSlots.LunchSlots;
            var timeStr = lunchTimes[_random.Next(lunchTimes.Length)];
            if (TimeOnly.TryParse(timeStr, out var parsedTime))
            {
                var startMin = parsedTime.Hour * 60 + parsedTime.Minute;

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
            }
        }

        // 夕方スロット（17:00-18:00）：約10%の確率で1件生成
        if (_random.Next(100) < 10)
        {
            var patient = patients[_random.Next(patients.Count)];
            var organization = organizations[_random.Next(organizations.Count)];
            // 17:00-17:45の範囲で生成（15分単位）
            var eveningTimeStrs = new[] { "17:00", "17:15", "17:30", "17:45" };
            var timeStr = eveningTimeStrs[_random.Next(eveningTimeStrs.Length)];
            if (TimeOnly.TryParse(timeStr, out var parsedTime))
            {
                var startMin = parsedTime.Hour * 60 + parsedTime.Minute;

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

                                // カウント加算
                                equipmentCurrentCounts[equipment.ApptResId]++;
                            }
                        }
                    }
                }
            }
        }

        return (appointments, resourceAssignments);
    }

    /// <summary>
    /// スロット開始時刻に基づいて時間帯の乗数を計算
    /// 朝（9:00-11:00）：1.2倍（ピーク）
    /// 昼（11:00-13:00）：0.5倍（空いている）
    /// 夕方（13:00-17:00）：1.1倍（やや混雑）
    /// </summary>
    private static decimal GetTimeModifier(int slotStartMin)
    {
        return slotStartMin switch
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

