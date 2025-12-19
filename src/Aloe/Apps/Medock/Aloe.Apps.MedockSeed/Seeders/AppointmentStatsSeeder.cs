using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Constants;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentStatsSeeder
{
    private static readonly Random _random = new Random();

    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
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

        // 必要なデータを取得
        var appointments = await context.Appointments
            .Where(a => !a.IsDeleted && a.ApptDate.HasValue)
            .ToListAsync();

        var resourceAssignments = await context.AppointmentResourceAssignments
            .Where(ara => !ara.IsDeleted)
            .ToListAsync();

        // mainリソースの場合、既存の予約データがない場合に予約データを生成
        // AppointmentResourceTypeは[NotMapped]のため、ApptResTypeCodeを使用
        var mainResources = await context.AppointmentResources
            .Where(r => !r.IsDeleted && r.ApptResTypeCode == (int)AppointmentResourceType.Main)
            .ToListAsync();

        if (mainResources.Any() && (!appointments.Any() || !resourceAssignments.Any()))
        {
            Console.WriteLine("[INFO] Generating appointment data for main resources...");
            var holidays = await SeederHelper.LoadHolidaySetAsync(context);

            // Floor、Organization、Patientを取得
            var floors = await context.Floors
                .Where(f => !f.IsDeleted)
                .ToListAsync();

            if (!floors.Any())
            {
                Console.WriteLine("[SKIP] AppointmentStats: No floors found for generating appointments.");
            }
            else
            {
                foreach (var mainResource in mainResources)
                {
                    var floor = floors.FirstOrDefault(f => f.FloorId == mainResource.FloorId);
                    if (floor == null)
                    {
                        Console.WriteLine($"  [!] Floor not found for resource: {mainResource.ApptResName}");
                        continue;
                    }

                    var patients = await context.Patients
                        .Where(p => !p.IsDeleted && p.FacilityId == floor.FacilityId)
                        .ToListAsync();

                    var organizations = await context.Organizations
                        .Where(o => !o.IsDeleted && o.FacilityId == floor.FacilityId)
                        .ToListAsync();

                    if (!patients.Any() || !organizations.Any())
                    {
                        Console.WriteLine($"  [!] Patients or Organizations not found for resource: {mainResource.ApptResName}");
                        continue;
                    }

                    // スロット定義を取得
                    var slot = await context.AppointmentSlots
                        .Where(s => !s.IsDeleted && s.IsActive && s.ApptResId == mainResource.ApptResId)
                        .FirstOrDefaultAsync();

                    if (slot?.ApptSlotsData == null || !slot.ApptSlotsData.Slots.Any())
                    {
                        Console.WriteLine($"  [!] Slot definition not found for resource: {mainResource.ApptResName}");
                        continue;
                    }

                    // 予約データを生成
                    var (generatedAppointments, generatedAssignments) = GenerateAppointmentsForMainResource(
                        mainResource,
                        startDate,
                        endDate,
                        slot.ApptSlotsData,
                        slot.ActiveFrom,
                        slot.ActiveTo,
                        holidays,
                        floor,
                        patients,
                        organizations,
                        dateTimeProvider);

                    if (generatedAppointments.Any())
                    {
                        context.Appointments.AddRange(generatedAppointments);
                        context.AppointmentResourceAssignments.AddRange(generatedAssignments);
                        Console.WriteLine($"  [+] Generated {generatedAppointments.Count} appointments for {mainResource.ApptResName}");
                    }
                }

                // 生成した予約データを保存
                if (context.ChangeTracker.HasChanges())
                {
                    await context.SaveChangesAsync();
                }

                // 予約データを再取得
                appointments = await context.Appointments
                    .Where(a => !a.IsDeleted && a.ApptDate.HasValue)
                    .ToListAsync();

                resourceAssignments = await context.AppointmentResourceAssignments
                    .Where(ara => !ara.IsDeleted)
                    .ToListAsync();
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

        var slots = await context.AppointmentSlots
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToListAsync();

        if (!slots.Any())
        {
            Console.WriteLine("[SKIP] AppointmentStats: No appointment slots found.");
            return;
        }

        var slotOverrides = await context.AppointmentSlotOverrides
            .Where(so => !so.IsDeleted)
            .ToListAsync();

        var resources = await context.AppointmentResources
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        Console.WriteLine("[INFO] Creating appointment stats seed data...");

        // 日付・リソースごとにグループ化
        var statsMap = new Dictionary<(DateOnly Date, Guid ResourceId), AppointmentStatsData>();

        // 1. 予約数を集計（appointment_resource_assignmentsとappointmentsをJOIN）
        var appointmentDict = appointments.ToDictionary(a => a.ApptId);
        var assignmentGroups = resourceAssignments
            .Where(ara => appointmentDict.ContainsKey(ara.ApptId))
            .GroupBy(ara => new
            {
                Date = appointmentDict[ara.ApptId].ApptDate!.Value,
                ResourceId = ara.ApptResId
            })
            .ToList();

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
        var slotDict = slots.ToDictionary(s => s.ApptResId);
        var overrideDict = slotOverrides
            .GroupBy(so => (so.ApptDate, so.ApptResId))
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var resource in resources)
        {
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
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

                    // キャパシティを計算（各スロットのCap値を合計）
                    statsData.Capacity = slotDef.Slots.Sum(s => s.Cap);

                    // グラフデータを生成
                    var graphSlots = new List<AppointmentGraphItem>();
                    foreach (var slotItem in slotDef.Slots)
                    {
                        // スロットの時間範囲内にある予約数をカウント
                        int count = 0;
                        foreach (var (timeKey, cnt) in statsData.TimeSlotCounts)
                        {
                            if (TimeOnly.TryParse(timeKey, out var appointmentTime))
                            {
                                // 予約の開始時刻がスロットの時間範囲内にあるかチェック
                                if (appointmentTime >= slotItem.Start && appointmentTime < slotItem.End)
                                {
                                    count += cnt;
                                }
                            }
                        }

                        graphSlots.Add(new AppointmentGraphItem
                        {
                            Start = slotItem.Start,
                            End = slotItem.End,
                            Count = count,
                            Cap = slotItem.Cap
                        });
                    }

                    statsData.GraphData = graphSlots;
                }

                currentDate = currentDate.AddDays(1);
            }
        }

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

        context.AppointmentStats.AddRange(statsList);
        Console.WriteLine($"  [+] AppointmentStats: {statsList.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
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
        IDateTimeProvider dateTimeProvider)
    {
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

            // 営業日判定
            var isBusinessDay = SeederHelper.IsBusinessDay(currentDate, holidays);
            
            // 忙しい時期（営業日）か閑散期（週末・祝日）かを判定
            var isBusyPeriod = isBusinessDay;
            
            foreach (var slotItem in slotDef.Slots)
            {
                // 埋まり具合を決定
                int appointmentCount;
                if (isBusyPeriod)
                {
                    // 忙しい時期: Cap値の100%（10割埋まる）
                    appointmentCount = slotItem.Cap;
                }
                else
                {
                    // 閑散期: Cap値の50%（半分埋まる、切り上げ）
                    appointmentCount = (int)Math.Ceiling(slotItem.Cap * 0.5);
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
                        ApptMemo = $"Generated for main resource: {resource.ApptResName}",
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

            currentDate = currentDate.AddDays(1);
        }

        return (appointments, assignments);
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

