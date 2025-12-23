using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Services;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentStatsSeeder
{
    private static readonly Random _random = new Random();

    public static async Task SeedAsync(MedockDbContext context, IDbContextFactory<MedockDbContext> contextFactory, IDateTimeProvider dateTimeProvider)
    {
        // テーブルが存在するか確認
        try
        {
            var hasExistingData = await context.AppointmentStats.AnyAsync();
            if (hasExistingData)
            {
                Console.WriteLine("[SKIP] AppointmentStats already exist.");
                return;
            }
        }
        catch (PostgresException ex) when (ex.SqlState == "42P01")
        {
            // テーブルが存在しない場合は続行（初回実行時）
        }

        // 日付範囲を取得（後続の処理で使用）
        var (startDate, endDate) = SeederHelper.GetDefaultDateRange(dateTimeProvider);

        var dataLoadStopwatch = Stopwatch.StartNew();
        Console.WriteLine("[INFO] Loading initial data...");

        // 必要なデータを取得（AsNoTrackingでパフォーマンス向上）
        var appointments = await context.Appointments
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.ApptDate.HasValue)
            .ToListAsync();

        var resourceAssignments = await context.AppointmentResourceAssignments
            .AsNoTracking()
            .Where(ara => !ara.IsDeleted)
            .ToListAsync();

        // 祝日データを読み込み（予約生成と統計計算の両方で使用）
        var holidays = await SeederHelper.LoadHolidaySetAsync(context);

        // mainリソースの場合、既存の予約データがない場合に予約データを生成
        // AppointmentResourceTypeは[NotMapped]のため、ApptResTypeCodeを使用
        var mainResources = await context.AppointmentResources
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.ApptResTypeCode == (int)AppointmentResourceType.Main)
            .ToListAsync();

        dataLoadStopwatch.Stop();
        Console.WriteLine($"  [+] Initial data loaded - took {dataLoadStopwatch.Elapsed.TotalSeconds:F2}s");

        if (mainResources.Any() && (!appointments.Any() || !resourceAssignments.Any()))
        {
            Console.WriteLine("[INFO] Generating appointment data for main resources...");
            var generationStopwatch = Stopwatch.StartNew();

            // 必要なデータを事前に一括取得（ループ内でのawaitを避ける、AsNoTrackingでパフォーマンス向上）
            var preloadStopwatch = Stopwatch.StartNew();
            var floors = await context.Floors
                .AsNoTracking()
                .Where(f => !f.IsDeleted)
                .ToListAsync();

            var allPatients = await context.Patients
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            var allOrganizations = await context.Organizations
                .AsNoTracking()
                .Where(o => !o.IsDeleted)
                .ToListAsync();

            var allSlots = await context.AppointmentSlots
                .AsNoTracking()
                .Where(s => !s.IsDeleted && s.IsActive)
                .ToListAsync();

            preloadStopwatch.Stop();
            Console.WriteLine($"  [+] Preloaded {floors.Count} floors, {allPatients.Count} patients, {allOrganizations.Count} organizations, {allSlots.Count} slots - took {preloadStopwatch.Elapsed.TotalSeconds:F2}s");

            if (!floors.Any())
            {
                Console.WriteLine("[SKIP] AppointmentStats: No floors found for generating appointments.");
            }
            else
            {
                // ビジネスアワーを事前に取得（AsNoTrackingでパフォーマンス向上）
                var facilityIdsForGen = floors.Select(f => f.FacilityId).Distinct().ToList();
                var businessHoursDictForGen = await context.FacilityBusinessHours
                    .AsNoTracking()
                    .Where(fbh => facilityIdsForGen.Contains(fbh.FacilityId) && fbh.IsActive && !fbh.IsDeleted)
                    .ToDictionaryAsync(fbh => fbh.FacilityId, fbh => fbh.BusinessHoursData);

                // 並列処理：Mainリソースが複数の場合、各リソースごとに独立したDbContextを使用して並列処理
                if (mainResources.Count > 1)
                {
                    Console.WriteLine($"  [INFO] Processing {mainResources.Count} resources in parallel...");
                    var parallelTasks = mainResources.Select(async (mainResource, index) =>
                    {
                        await using var resourceContext = await contextFactory.CreateDbContextAsync();
                        await ProcessResourceAsync(
                            resourceContext,
                            mainResource,
                            index + 1,
                            mainResources.Count,
                            floors,
                            allPatients,
                            allOrganizations,
                            allSlots,
                            businessHoursDictForGen,
                            holidays,
                            startDate,
                            endDate,
                            dateTimeProvider);
                    }).ToArray();
                    await Task.WhenAll(parallelTasks);
                }
                else
                {
                    // リソースが1つの場合は並列化しない（オーバーヘッドを避ける）
                    var resourceIndex = 0;
                    foreach (var mainResource in mainResources)
                    {
                        resourceIndex++;
                        await ProcessResourceAsync(
                            context,
                            mainResource,
                            resourceIndex,
                            mainResources.Count,
                            floors,
                            allPatients,
                            allOrganizations,
                            allSlots,
                            businessHoursDictForGen,
                            holidays,
                            startDate,
                            endDate,
                            dateTimeProvider);
                    }
                }

                generationStopwatch.Stop();
                Console.WriteLine($"  [+] Appointment generation completed - took {generationStopwatch.Elapsed.TotalSeconds:F2}s");

                // 予約データを再取得（AsNoTrackingでパフォーマンス向上）
                var reloadStopwatch = Stopwatch.StartNew();
                appointments = await context.Appointments
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted && a.ApptDate.HasValue)
                    .ToListAsync();

                resourceAssignments = await context.AppointmentResourceAssignments
                    .AsNoTracking()
                    .Where(ara => !ara.IsDeleted)
                    .ToListAsync();
                reloadStopwatch.Stop();
                Console.WriteLine($"  [+] Reloaded {appointments.Count} appointments and {resourceAssignments.Count} assignments - took {reloadStopwatch.Elapsed.TotalSeconds:F2}s");
            }
        }

        if (!appointments.Any())
        {
            Console.WriteLine("[SKIP] AppointmentStats: No appointments found.");
            return;
        }

        if (!resourceAssignments.Any())
        {
            Console.WriteLine("[SKIP] AppointmentStats: No appointment resource assignments found.");
            return;
        }

        var statsDataLoadStopwatch = Stopwatch.StartNew();
        var slots = await context.AppointmentSlots
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToListAsync();

        if (!slots.Any())
        {
            Console.WriteLine("[SKIP] AppointmentStats: No appointment slots found.");
            return;
        }

        var slotOverrides = await context.AppointmentSlotOverrides
            .AsNoTracking()
            .Where(so => !so.IsDeleted)
            .ToListAsync();

        var resources = await context.AppointmentResources
            .AsNoTracking()
            .Include(r => r.Floor)
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        // 施設のビジネスアワーを取得（AsNoTrackingでパフォーマンス向上）
        var facilityIds = resources.Select(r => r.Floor.FacilityId).Distinct().ToList();
        var businessHoursDict = await context.FacilityBusinessHours
            .AsNoTracking()
            .Where(fbh => facilityIds.Contains(fbh.FacilityId) && fbh.IsActive && !fbh.IsDeleted)
            .ToDictionaryAsync(fbh => fbh.FacilityId, fbh => fbh.BusinessHoursData);

        statsDataLoadStopwatch.Stop();
        Console.WriteLine($"  [+] Stats data loaded - took {statsDataLoadStopwatch.Elapsed.TotalSeconds:F2}s");

        Console.WriteLine("[INFO] Creating appointment stats seed data...");
        var statsStopwatch = Stopwatch.StartNew();

        var aggregationStopwatch = Stopwatch.StartNew();
        // 日付・リソースごとにグループ化
        var statsMap = new Dictionary<(DateOnly Date, Guid ResourceId), AppointmentStatsData>();

        // 1. 予約数を集計（最適化：appointmentDictを先に作成し、GroupByで効率化）
        var appointmentDict = appointments.ToDictionary(a => a.ApptId);
        var assignmentGroups = resourceAssignments
            .Where(ara => appointmentDict.ContainsKey(ara.ApptId))
            .GroupBy(ara => new
            {
                Date = appointmentDict[ara.ApptId].ApptDate!.Value,
                ResourceId = ara.ApptResId
            })
            .ToList();
        aggregationStopwatch.Stop();
        Console.WriteLine($"  [+] Aggregation completed - took {aggregationStopwatch.Elapsed.TotalSeconds:F2}s");

        foreach (var group in assignmentGroups)
        {
            var key = (group.Key.Date, group.Key.ResourceId);
            if (!statsMap.ContainsKey(key))
            {
                statsMap[key] = new AppointmentStatsData
                {
                    Date = group.Key.Date,
                    ResourceId = group.Key.ResourceId,
                    AppointmentCount = 0,
                    TimeSlotCounts = new Dictionary<string, int>()
                };
            }

            // 予約数をカウント
            var appointmentsInGroup = group
                .Select(ara => appointmentDict[ara.ApptId])
                .Where(a => a.ApptDate.HasValue)
                .ToList();

            statsMap[key].AppointmentCount = appointmentsInGroup.Count;

            // 時間帯ごとの予約数を集計
            // 予約の開始時刻をTimeOnlyとして、スロットの時間範囲とマッチング
            foreach (var appointment in appointmentsInGroup)
            {
                if (appointment.ApptStartTime.HasValue)
                {
                    var appointmentTime = appointment.ApptStartTime.Value;
                    // 時間範囲のキーを作成（"HH:mm"形式）
                    var timeKey = appointmentTime.ToString("HH:mm");
                    // 後でスロット定義とマッチングするため、開始時刻のみをキーとして使用
                    // 実際のマッチングはスロット定義のStart/End範囲で行う
                    if (!statsMap[key].TimeSlotCounts.ContainsKey(timeKey))
                    {
                        statsMap[key].TimeSlotCounts[timeKey] = 0;
                    }
                    statsMap[key].TimeSlotCounts[timeKey]++;
                }
            }
        }

        // 2. キャパシティとグラフデータを計算（appointment_slotsから）
        var capacityCalcStopwatch = Stopwatch.StartNew();
        var slotDict = slots.ToDictionary(s => s.ApptResId);
        var overrideDict = slotOverrides
            .GroupBy(so => (so.ApptDate, so.ApptResId))
            .ToDictionary(g => g.Key, g => g.First());

        // ビジネスアワーのパース結果をメモ化（リソースごとに1回だけパース）
        var parsedBusinessHoursByFacility = new Dictionary<Guid, ParsedBusinessHours>();
        foreach (var facilityId in facilityIds)
        {
            var businessHours = businessHoursDict.GetValueOrDefault(facilityId)
                ?? new FacilityBusinessHoursRoot();
            parsedBusinessHoursByFacility[facilityId] = new ParsedBusinessHours
            {
                BusinessStartTime = TimeOnly.Parse(businessHours.Start),
                BusinessEndTime = TimeOnly.Parse(businessHours.End),
                LunchStartTime = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.Start) : new TimeOnly(12, 0),
                LunchEndTime = businessHours.Lunch != null ? TimeOnly.Parse(businessHours.Lunch.End) : new TimeOnly(13, 0)
            };
        }

        foreach (var resource in resources)
        {
            // メモ化されたビジネスアワーを使用
            var parsedBusinessHours = parsedBusinessHoursByFacility.GetValueOrDefault(resource.Floor.FacilityId)
                ?? new ParsedBusinessHours
                {
                    BusinessStartTime = new TimeOnly(9, 0),
                    BusinessEndTime = new TimeOnly(17, 0),
                    LunchStartTime = new TimeOnly(12, 0),
                    LunchEndTime = new TimeOnly(13, 0)
                };
            var businessStartTime = parsedBusinessHours.BusinessStartTime;
            var businessEndTime = parsedBusinessHours.BusinessEndTime;
            var lunchStartTime = parsedBusinessHours.LunchStartTime;
            var lunchEndTime = parsedBusinessHours.LunchEndTime;

            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                // 休診日（日曜・祝日）はスキップ
                if (!SeederHelper.IsBusinessDay(currentDate, holidays))
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                var key = (currentDate, resource.ApptResId);

                // スロット定義を取得（overrideがあれば優先）
                AppointmentSlotRoot? slotDef = null;
                if (overrideDict.TryGetValue((currentDate, resource.ApptResId), out var slotOverride))
                {
                    slotDef = slotOverride.ApptSlotsData;
                }
                else if (slotDict.TryGetValue(resource.ApptResId, out var slot))
                {
                    // 有効期間内かチェック
                    if (slot.ActiveFrom <= currentDate && currentDate <= slot.ActiveTo)
                    {
                        slotDef = slot.ApptSlotsData;
                    }
                }

                if (slotDef != null && slotDef.Slots.Any())
                {
                    // 水曜・土曜は半日営業（午前のみ）
                    var isHalfDay = SeederHelper.IsHalfDay(currentDate);

                    // 半日営業の場合、昼休み開始前のスロットのみを対象とする
                    // 時間外スロット（IsOutsideHours = true）は除外する（別途処理するため）
                    var activeSlots = isHalfDay
                        ? slotDef.Slots.Where(s => s.Start < lunchStartTime && !s.IsOutsideHours).ToList()
                        : slotDef.Slots.Where(s => !s.IsOutsideHours).ToList();

                    if (!statsMap.ContainsKey(key))
                    {
                        statsMap[key] = new AppointmentStatsData
                        {
                            Date = currentDate,
                            ResourceId = resource.ApptResId,
                            AppointmentCount = 0,
                            TimeSlotCounts = new Dictionary<string, int>()
                        };
                    }

                    var statsData = statsMap[key];

                    // キャパシティを計算（営業時間内のスロットのみ）
                    statsData.Capacity = activeSlots.Sum(s => s.Cap);

                    // グラフデータを生成（営業時間内のスロット + 時間外スロット）
                    // 時間外の予約は時間外スロットに集計する
                    var graphSlots = new List<AppointmentGraphItem>();

                    // 時間外スロットを取得（スロット定義から動的に取得）
                    var outsideHoursSlots = slotDef.Slots.Where(s => s.IsOutsideHours).ToList();
                    var morningSlot = outsideHoursSlots.FirstOrDefault(s => s.End <= businessStartTime);
                    var lunchSlot = outsideHoursSlots.FirstOrDefault(s => s.Start >= lunchStartTime && s.End <= lunchEndTime);
                    var eveningSlot = outsideHoursSlots.FirstOrDefault(s => s.Start >= businessEndTime);
                    
                    // 最適化：TimeSlotCountsをTimeOnlyに変換（1回だけパース）
                    var parsedTimeSlotCounts = new List<(TimeOnly Time, int Count)>();
                    foreach (var (timeKey, cnt) in statsData.TimeSlotCounts)
                    {
                        if (TimeOnly.TryParse(timeKey, out var appointmentTime))
                        {
                            parsedTimeSlotCounts.Add((appointmentTime, cnt));
                        }
                    }

                    // 時間外予約を事前に分類（重複判定を避ける）
                    var morningAppointments = new List<(TimeOnly Time, int Count)>();
                    var lunchAppointments = new List<(TimeOnly Time, int Count)>();
                    var eveningAppointments = new List<(TimeOnly Time, int Count)>();
                    var businessHoursAppointments = new List<(TimeOnly Time, int Count)>();

                    foreach (var (appointmentTime, cnt) in parsedTimeSlotCounts)
                    {
                        if (appointmentTime < businessStartTime)
                        {
                            morningAppointments.Add((appointmentTime, cnt));
                        }
                        else if (appointmentTime >= lunchStartTime && appointmentTime < lunchEndTime)
                        {
                            lunchAppointments.Add((appointmentTime, cnt));
                        }
                        else if (appointmentTime >= businessEndTime)
                        {
                            eveningAppointments.Add((appointmentTime, cnt));
                        }
                        else
                        {
                            businessHoursAppointments.Add((appointmentTime, cnt));
                        }
                    }

                    // 通常のスロット（営業時間内）を処理
                    foreach (var slotItem in activeSlots)
                    {
                        // スロットの時間範囲内にある予約数をカウント
                        int count = 0;
                        foreach (var (appointmentTime, cnt) in businessHoursAppointments)
                        {
                            if (appointmentTime >= slotItem.Start && appointmentTime < slotItem.End)
                            {
                                count += cnt;
                            }
                        }

                        graphSlots.Add(new AppointmentGraphItem
                        {
                            Start = slotItem.Start,
                            End = slotItem.End,
                            Count = count,
                            Cap = slotItem.Cap,
                            HasOutsideHours = false
                        });
                    }

                    // 時間外スロットを処理（事前分類済みのリストを使用）
                    if (morningSlot != null)
                    {
                        var count = morningAppointments.Sum(x => x.Count);
                        graphSlots.Add(new AppointmentGraphItem
                        {
                            Start = morningSlot.Start,
                            End = morningSlot.End,
                            Count = count,
                            Cap = morningSlot.Cap,
                            HasOutsideHours = true
                        });
                    }

                    if (lunchSlot != null)
                    {
                        var count = lunchAppointments.Sum(x => x.Count);
                        graphSlots.Add(new AppointmentGraphItem
                        {
                            Start = lunchSlot.Start,
                            End = lunchSlot.End,
                            Count = count,
                            Cap = lunchSlot.Cap,
                            HasOutsideHours = true
                        });
                    }

                    if (eveningSlot != null)
                    {
                        var count = eveningAppointments.Sum(x => x.Count);
                        graphSlots.Add(new AppointmentGraphItem
                        {
                            Start = eveningSlot.Start,
                            End = eveningSlot.End,
                            Count = count,
                            Cap = eveningSlot.Cap,
                            HasOutsideHours = true
                        });
                    }

                    statsData.GraphData = graphSlots;
                }

                currentDate = currentDate.AddDays(1);
            }
        }
        capacityCalcStopwatch.Stop();
        Console.WriteLine($"  [+] Capacity and graph data calculation completed - took {capacityCalcStopwatch.Elapsed.TotalSeconds:F2}s");

        // 3. AppointmentStatsエンティティを作成
        var statsList = new List<AppointmentStats>();
        foreach (var (key, statsData) in statsMap)
        {
            var stat = new AppointmentStats
            {
                ApptStatId = Guid.CreateVersion7(),
                ApptDate = statsData.Date,
                ApptResId = statsData.ResourceId,
                ApptCap = statsData.Capacity,
                ApptCount = statsData.AppointmentCount,
                ApptGraphData = new AppointmentGraphRoot
                {
                    Slots = statsData.GraphData
                },
                IsDeleted = false
            };

            SeederHelper.InitializeAuditFields(stat, dateTimeProvider);
            statsList.Add(stat);
        }

        if (statsList.Any())
        {
            var insertStatsStopwatch = Stopwatch.StartNew();
            await context.BulkInsertAsync(statsList, new BulkConfig
            {
                SetOutputIdentity = false,
                BatchSize = 5000 // パフォーマンス向上のため1000→5000に変更
            });
            insertStatsStopwatch.Stop();
            statsStopwatch.Stop();
            Console.WriteLine($"  [+] AppointmentStats: {statsList.Count} entries - took {statsStopwatch.Elapsed.TotalSeconds:F2}s (insert: {insertStatsStopwatch.Elapsed.TotalSeconds:F2}s)");
        }
    }


    /// <summary>
    /// パース済みビジネスアワー情報（メモ化用）
    /// </summary>
    private class ParsedBusinessHours
    {
        public TimeOnly BusinessStartTime { get; set; }
        public TimeOnly BusinessEndTime { get; set; }
        public TimeOnly LunchStartTime { get; set; }
        public TimeOnly LunchEndTime { get; set; }
    }

    /// <summary>
    /// リソースごとの予約データ生成処理（並列処理対応）
    /// </summary>
    private static async Task ProcessResourceAsync(
        MedockDbContext resourceContext,
        AppointmentResource mainResource,
        int resourceIndex,
        int totalResources,
        List<Floor> floors,
        List<Patient> allPatients,
        List<Organization> allOrganizations,
        List<AppointmentSlot> allSlots,
        Dictionary<Guid, FacilityBusinessHoursRoot?> businessHoursDictForGen,
        HashSet<DateOnly> holidays,
        DateOnly startDate,
        DateOnly endDate,
        IDateTimeProvider dateTimeProvider)
    {
        var floor = floors.FirstOrDefault(f => f.FloorId == mainResource.FloorId);
        if (floor == null)
        {
            Console.WriteLine($"  [!] Floor not found for resource: {mainResource.ApptResName}");
            return;
        }

        // メモリ内でフィルタリング
        var patients = allPatients.Where(p => p.FacilityId == floor.FacilityId).ToList();
        var organizations = allOrganizations.Where(o => o.FacilityId == floor.FacilityId).ToList();

        if (!patients.Any() || !organizations.Any())
        {
            Console.WriteLine($"  [!] Patients or Organizations not found for resource: {mainResource.ApptResName}");
            return;
        }

        // メモリ内でスロット定義を取得
        var slot = allSlots.FirstOrDefault(s => s.ApptResId == mainResource.ApptResId);

        if (slot?.ApptSlotsData == null || !slot.ApptSlotsData.Slots.Any())
        {
            Console.WriteLine($"  [!] Slot definition not found for resource: {mainResource.ApptResName}");
            return;
        }

        // ビジネスアワーを取得して事前に解析（メモ化）
        var businessHoursForGen = businessHoursDictForGen.GetValueOrDefault(floor.FacilityId)
            ?? new FacilityBusinessHoursRoot();
        var businessStartTime = TimeOnly.Parse(businessHoursForGen.Start);
        var businessEndTime = TimeOnly.Parse(businessHoursForGen.End);
        var lunchStartTime = businessHoursForGen.Lunch != null ? TimeOnly.Parse(businessHoursForGen.Lunch.Start) : new TimeOnly(12, 0);
        var lunchEndTime = businessHoursForGen.Lunch != null ? TimeOnly.Parse(businessHoursForGen.Lunch.End) : new TimeOnly(13, 0);
        
        // パース済みの値を直接渡す（メモ化された値を使用）
        var parsedBusinessHours = new ParsedBusinessHours
        {
            BusinessStartTime = businessStartTime,
            BusinessEndTime = businessEndTime,
            LunchStartTime = lunchStartTime,
            LunchEndTime = lunchEndTime
        };

        // バッチ数を事前に計算（進捗表示用、6ヶ月ごとに変更）
        var totalMonths = (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month) + 1;
        var totalBatches = (int)Math.Ceiling(totalMonths / 6.0);
        var currentBatch = 0;

        // 6ヶ月ごとにバッチ処理（パフォーマンス向上のため3ヶ月→6ヶ月に変更）
        var batchStartDate = startDate;
        var batchEndDate = startDate.AddMonths(6).AddDays(-1);
        if (batchEndDate > endDate) batchEndDate = endDate;

        var totalAppointments = 0;

        while (batchStartDate <= endDate)
        {
            currentBatch++;
            var batchStopwatch = Stopwatch.StartNew();
            // バッチの日付範囲がスロットの有効期間内に収まるように調整
            var effectiveBatchStart = batchStartDate < slot.ActiveFrom ? slot.ActiveFrom : batchStartDate;
            var effectiveBatchEnd = batchEndDate > slot.ActiveTo ? slot.ActiveTo : batchEndDate;

            if (effectiveBatchStart <= effectiveBatchEnd)
            {
                // 予約データを生成（パース済みのbusinessHoursを渡す）
                var (generatedAppointments, generatedAssignments) = GenerateAppointmentsForMainResource(
                    mainResource,
                    effectiveBatchStart,
                    effectiveBatchEnd,
                    slot.ApptSlotsData,
                    slot.ActiveFrom,
                    slot.ActiveTo,
                    holidays,
                    floor,
                    patients,
                    organizations,
                    dateTimeProvider,
                    parsedBusinessHours);

                if (generatedAppointments.Any())
                {
                    var insertStopwatch = Stopwatch.StartNew();
                    await resourceContext.BulkInsertAsync(generatedAppointments, new BulkConfig
                    {
                        SetOutputIdentity = false,
                        BatchSize = 5000 // パフォーマンス向上のため1000→5000に変更
                    });
                    await resourceContext.BulkInsertAsync(generatedAssignments, new BulkConfig
                    {
                        SetOutputIdentity = false,
                        BatchSize = 5000 // パフォーマンス向上のため1000→5000に変更
                    });
                    insertStopwatch.Stop();
                    totalAppointments += generatedAppointments.Count;
                    batchStopwatch.Stop();
                    var progressPercent = (int)((double)currentBatch / totalBatches * 100);
                    Console.WriteLine($"  [BATCH] Resource {resourceIndex}/{totalResources} - Batch {currentBatch}/{totalBatches} ({progressPercent}%) - Committed {generatedAppointments.Count} appointments for {mainResource.ApptResName} ({effectiveBatchStart:yyyy-MM-dd} to {effectiveBatchEnd:yyyy-MM-dd}) - took {batchStopwatch.Elapsed.TotalSeconds:F2}s (insert: {insertStopwatch.Elapsed.TotalSeconds:F2}s)");
                }
            }

            // 次のバッチへ（6ヶ月ごと）
            batchStartDate = batchEndDate.AddDays(1);
            batchEndDate = batchStartDate.AddMonths(6).AddDays(-1);
            if (batchEndDate > endDate) batchEndDate = endDate;
        }

        if (totalAppointments > 0)
        {
            Console.WriteLine($"  [+] Generated {totalAppointments} appointments total for {mainResource.ApptResName}");
        }
    }

    /// <summary>
    /// Mainリソース用の予約データを生成
    /// </summary>
    private static (List<Appointment> Appointments, List<AppointmentResourceAssignment> Assignments) GenerateAppointmentsForMainResource(
        AppointmentResource resource,
        DateOnly startDate,
        DateOnly endDate,
        AppointmentSlotRoot slotDef,
        DateOnly slotActiveFrom,
        DateOnly slotActiveTo,
        HashSet<DateOnly> holidays,
        Floor floor,
        List<Patient> patients,
        List<Organization> organizations,
        IDateTimeProvider dateTimeProvider,
        ParsedBusinessHours parsedBusinessHours)
    {
        // パース済みのビジネスアワーを使用（メモ化された値）
        var businessStartTime = parsedBusinessHours.BusinessStartTime;
        var businessEndTime = parsedBusinessHours.BusinessEndTime;
        var lunchStartTime = parsedBusinessHours.LunchStartTime;
        var lunchEndTime = parsedBusinessHours.LunchEndTime;
        var appointments = new List<Appointment>();
        var assignments = new List<AppointmentResourceAssignment>();

        var currentDate = startDate;
        while (currentDate <= endDate)
        {
            // スロット定義が有効な日付範囲内かチェック
            if (currentDate < slotActiveFrom || currentDate > slotActiveTo)
            {
                currentDate = currentDate.AddDays(1);
                continue;
            }

            // 営業日判定（日曜・祝日は休診）
            var isBusinessDay = SeederHelper.IsBusinessDay(currentDate, holidays);
            var isHoliday = holidays.Contains(currentDate);
            var isSunday = currentDate.DayOfWeek == DayOfWeek.Sunday;

            // 休診日の場合は緊急予約をチェックしてからスキップ
            if (!isBusinessDay)
            {
                // 日曜・祝日に稀に緊急予約を生成（0.5-1%の確率）
                if ((isSunday || isHoliday) && _random.Next(1000) < 8) // 0.8%の確率
                {
                    var emergencyCount = _random.Next(1, 4); // 1-3件

                    for (int i = 0; i < emergencyCount; i++)
                    {
                        var patient = patients[_random.Next(patients.Count)];
                        var organization = organizations[_random.Next(organizations.Count)];

                        // 緊急予約の時間はランダム（早朝・午前・午後・夕方）- ビジネスアワーに基づく
                        TimeOnly emergencyTime;
                        var timePattern = _random.Next(4);
                        var outsideHoursStart = new TimeOnly(7, 0);
                        var outsideHoursEnd = new TimeOnly(19, 0);
                        switch (timePattern)
                        {
                            case 0: // 早朝（07:00 ～ businessStart）
                                var morningRange = (int)(businessStartTime - outsideHoursStart).TotalMinutes;
                                if (morningRange > 0)
                                {
                                    emergencyTime = outsideHoursStart.AddMinutes(_random.Next(0, morningRange));
                                }
                                else
                                {
                                    emergencyTime = businessStartTime;
                                }
                                break;
                            case 1: // 午前（businessStart ～ lunchStart）
                                var amRange = (int)(lunchStartTime - businessStartTime).TotalMinutes;
                                emergencyTime = businessStartTime.AddMinutes(_random.Next(0, amRange));
                                break;
                            case 2: // 午後（lunchEnd ～ businessEnd）
                                var pmRange = (int)(businessEndTime - lunchEndTime).TotalMinutes;
                                emergencyTime = lunchEndTime.AddMinutes(_random.Next(0, pmRange));
                                break;
                            default: // 夕方（businessEnd ～ 19:00）
                                var eveningRange = (int)(outsideHoursEnd - businessEndTime).TotalMinutes;
                                if (eveningRange > 0)
                                {
                                    emergencyTime = businessEndTime.AddMinutes(_random.Next(0, eveningRange));
                                }
                                else
                                {
                                    emergencyTime = businessEndTime;
                                }
                                break;
                        }

                        var emergencyAppointment = new Appointment
                        {
                            ApptId = Guid.CreateVersion7(),
                            FloorId = floor.FloorId,
                            OrgId = organization.OrgId,
                            PtId = patient.PtId,
                            ApptDate = currentDate,
                            ApptStartTime = emergencyTime,
                            ApptDurationMin = 30,
                            ApptStatusCode = 0,
                            ApptMemo = "緊急予約",
                            IsDeleted = false
                        };

                        SeederHelper.InitializeAuditFields(emergencyAppointment, dateTimeProvider);
                        appointments.Add(emergencyAppointment);

                        var emergencyAssignment = new AppointmentResourceAssignment
                        {
                            ApptResAssignId = Guid.CreateVersion7(),
                            ApptId = emergencyAppointment.ApptId,
                            ApptResId = resource.ApptResId,
                            ApptStartTime = emergencyTime,
                            IsDeleted = false
                        };

                        SeederHelper.InitializeAuditFields(emergencyAssignment, dateTimeProvider);
                        assignments.Add(emergencyAssignment);
                    }
                }

                currentDate = currentDate.AddDays(1);
                continue;
            }

            // 水曜・土曜は半日営業（午前のみ）
            var isHalfDay = SeederHelper.IsHalfDay(currentDate);

            // 季節による繁忙度を取得して予約数を決定
            var seasonalRate = GetSeasonalOccupancyRate(currentDate.Month);
            // 日ごとの変動率を取得（±20-30%の変動）
            var dailyVariationRate = GetDailyVariationRate();

            foreach (var slotItem in slotDef.Slots)
            {
                // 半日営業の場合、午後のスロット（12:00以降）はスキップ
                if (isHalfDay && slotItem.Start >= lunchStartTime)
                {
                    continue;
                }

                // 時間帯ごとの充足率を取得（時間帯による偏りを反映）
                var timeSlotRate = GetTimeSlotOccupancyRate(slotItem.Start);
                // スロットごとの微細な変動（±5%）
                var slotVariationRate = 0.95 + _random.NextDouble() * 0.1;

                // 埋まり具合を決定（季節 + 日ごとの変動 + 時間帯の偏り + スロットごとの変動）
                var effectiveRate = seasonalRate * dailyVariationRate * timeSlotRate * slotVariationRate;
                // 最低でもキャパシティの30%は埋まるようにする（極端に少なくならないように）
                effectiveRate = Math.Max(effectiveRate, 0.3);
                var appointmentCount = (int)Math.Ceiling(slotItem.Cap * effectiveRate);

                // キャパオーバー：基本的に1件、ごくまれに2件（2%の確率で1件、0.5%の確率で2件）
                var overCapacityChance = _random.Next(1000);
                if (overCapacityChance < 5) // 0.5%の確率で2件
                {
                    appointmentCount += 2;
                }
                else if (overCapacityChance < 25) // 2%の確率で1件
                {
                    appointmentCount += 1;
                }

                // 予約を生成
                for (int i = 0; i < appointmentCount; i++)
                {
                    // ランダムにPatientとOrganizationを選択
                    var patient = patients[_random.Next(patients.Count)];
                    var organization = organizations[_random.Next(organizations.Count)];

                    // Appointmentを作成
                    var appointment = new Appointment
                    {
                        ApptId = Guid.CreateVersion7(),
                        FloorId = floor.FloorId,
                        OrgId = organization.OrgId,
                        PtId = patient.PtId,
                        ApptDate = currentDate,
                        ApptStartTime = slotItem.Start,
                        ApptDurationMin = (int)slotItem.Duration.TotalMinutes,
                        ApptStatusCode = 0,
                        ApptMemo = String.Empty,
                        IsDeleted = false
                    };

                    SeederHelper.InitializeAuditFields(appointment, dateTimeProvider);
                    appointments.Add(appointment);

                    // AppointmentResourceAssignmentを作成
                    var assignment = new AppointmentResourceAssignment
                    {
                        ApptResAssignId = Guid.CreateVersion7(),
                        ApptId = appointment.ApptId,
                        ApptResId = resource.ApptResId,
                        ApptStartTime = slotItem.Start,
                        IsDeleted = false
                    };

                    SeederHelper.InitializeAuditFields(assignment, dateTimeProvider);
                    assignments.Add(assignment);
                }
            }

            // たまに時間外・お昼休みのイレギュラー予約を生成（3-5%の確率）
            // 早朝（～businessStart）、お昼休み（lunchStart～lunchEnd）、夕方（businessEnd～）の3パターン
            var irregularChance = _random.Next(100);
            if (irregularChance < 4) // 4%の確率
            {
                var patient = patients[_random.Next(patients.Count)];
                var organization = organizations[_random.Next(organizations.Count)];

                TimeOnly irregularTime;
                string memo;
                var outsideHoursStart = new TimeOnly(7, 0);

                // 3パターンからランダムに選択
                var pattern = _random.Next(3);
                switch (pattern)
                {
                    case 0: // 早朝（07:00 ～ businessStart）
                        var earlyRange = (int)(businessStartTime - outsideHoursStart).TotalMinutes;
                        irregularTime = earlyRange > 0
                            ? outsideHoursStart.AddMinutes(_random.Next(0, earlyRange))
                            : businessStartTime.AddMinutes(-30);
                        memo = "時間外予約（早朝）";
                        break;
                    case 1: // お昼休み（lunchStart ～ lunchEnd）
                        var lunchRange = (int)(lunchEndTime - lunchStartTime).TotalMinutes;
                        irregularTime = lunchStartTime.AddMinutes(_random.Next(0, lunchRange));
                        memo = "時間外予約（お昼休み）";
                        break;
                    default: // 夕方（businessEnd ～ businessEnd+1h）
                        irregularTime = businessEndTime.AddMinutes(_random.Next(0, 60));
                        memo = "時間外予約（夕方）";
                        break;
                }

                var irregularAppointment = new Appointment
                {
                    ApptId = Guid.CreateVersion7(),
                    FloorId = floor.FloorId,
                    OrgId = organization.OrgId,
                    PtId = patient.PtId,
                    ApptDate = currentDate,
                    ApptStartTime = irregularTime,
                    ApptDurationMin = 30,
                    ApptStatusCode = 0,
                    ApptMemo = memo,
                    IsDeleted = false
                };

                SeederHelper.InitializeAuditFields(irregularAppointment, dateTimeProvider);
                appointments.Add(irregularAppointment);

                var irregularAssignment = new AppointmentResourceAssignment
                {
                    ApptResAssignId = Guid.CreateVersion7(),
                    ApptId = irregularAppointment.ApptId,
                    ApptResId = resource.ApptResId,
                    ApptStartTime = irregularTime,
                    IsDeleted = false
                };

                SeederHelper.InitializeAuditFields(irregularAssignment, dateTimeProvider);
                assignments.Add(irregularAssignment);
            }

            currentDate = currentDate.AddDays(1);
        }

        return (appointments, assignments);
    }

    /// <summary>
    /// 月に応じた繁忙度（予約充足率）を取得
    /// ランダム性を持たせて現実的なデータを生成
    /// </summary>
    private static double GetSeasonalOccupancyRate(int month)
    {
        // 基準となる充足率
        var baseRate = month switch
        {
            // 最繁忙期（秋）: 9〜12月 → 95〜100%
            9 or 10 or 11 or 12 => 0.95,
            // 繁忙期（春）: 4〜6月 → 80〜90%
            4 or 5 or 6 => 0.85,
            // 閑散期: 1〜3月, 7〜8月 → 40〜60%
            _ => 0.50
        };

        // ±10%のランダム変動を追加（0.9〜1.1倍）
        var randomFactor = 0.9 + _random.NextDouble() * 0.2;
        var rate = baseRate * randomFactor;

        // 0〜1の範囲に収める
        return Math.Clamp(rate, 0.0, 1.0);
    }

    /// <summary>
    /// 日ごとの変動率を取得（±20-30%の変動）
    /// 日々の予約数を分散させるために使用
    /// </summary>
    private static double GetDailyVariationRate()
    {
        // ±25%のランダム変動を追加（0.75〜1.25倍）
        var variationFactor = 0.75 + _random.NextDouble() * 0.5;
        return variationFactor;
    }

    /// <summary>
    /// 時間帯ごとの充足率を取得（時間帯による偏りを反映）
    /// 午前の早い時間や午後の遅い時間など、時間帯ごとに異なる混雑度を表現
    /// </summary>
    private static double GetTimeSlotOccupancyRate(TimeOnly startTime)
    {
        var hour = startTime.Hour;
        var minute = startTime.Minute;
        var totalMinutes = hour * 60 + minute;

        // 時間帯ごとの基準充足率（1.0を基準として、時間帯による偏りを表現）
        double baseRate;

        if (totalMinutes < 8 * 60) // 8:00より前（早朝）
        {
            baseRate = 0.6; // 空いている
        }
        else if (totalMinutes < 9 * 60) // 8:00-9:00
        {
            baseRate = 0.75; // やや空いている
        }
        else if (totalMinutes < 10 * 60) // 9:00-10:00
        {
            baseRate = 1.05; // 混雑し始める
        }
        else if (totalMinutes < 11 * 60) // 10:00-11:00
        {
            baseRate = 1.15; // 最も混雑
        }
        else if (totalMinutes < 12 * 60) // 11:00-12:00
        {
            baseRate = 1.05; // 混雑
        }
        else if (totalMinutes < 13 * 60) // 12:00-13:00（お昼休み）
        {
            baseRate = 0.3; // 非常に空いている
        }
        else if (totalMinutes < 14 * 60) // 13:00-14:00
        {
            baseRate = 0.95; // やや混雑
        }
        else if (totalMinutes < 15 * 60) // 14:00-15:00
        {
            baseRate = 1.0; // 標準
        }
        else if (totalMinutes < 16 * 60) // 15:00-16:00
        {
            baseRate = 0.9; // やや空いている
        }
        else if (totalMinutes < 17 * 60) // 16:00-17:00
        {
            baseRate = 0.8; // 空いている
        }
        else // 17:00以降（夕方）
        {
            baseRate = 0.5; // 非常に空いている
        }

        // ±10%のランダム変動を追加して、同じ時間帯でも多少のばらつきを持たせる
        var randomFactor = 0.9 + _random.NextDouble() * 0.2;
        var rate = baseRate * randomFactor;

        // 0.5〜1.2の範囲に収める（マイナス側を小さくし、キャパオーバーも控えめに）
        return Math.Clamp(rate, 0.5, 1.2);
    }

    /// <summary>
    /// 統計データの一時保持用クラス
    /// </summary>
    private class AppointmentStatsData
    {
        public DateOnly Date { get; set; }
        public Guid ResourceId { get; set; }
        public int AppointmentCount { get; set; }
        public int Capacity { get; set; }
        public Dictionary<string, int> TimeSlotCounts { get; set; } = new();
        public List<AppointmentGraphItem> GraphData { get; set; } = new();
    }
}

