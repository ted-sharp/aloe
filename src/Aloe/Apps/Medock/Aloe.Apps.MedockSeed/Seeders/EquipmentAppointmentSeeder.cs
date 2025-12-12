using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class EquipmentAppointmentSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var today = dateTimeProvider.TodayDateOnly;
        var (startDate, endDate) = SeederHelper.GetDefaultDateRange(dateTimeProvider);

        // 過去3年〜未来1年の範囲に既存データが存在するかチェック
        var existingEquipmentAppointmentsInRange = await context.EquipmentAppointments
            .Where(ea => !ea.IsDeleted && ea.ApptDate >= startDate && ea.ApptDate <= endDate)
            .AnyAsync();

        if (existingEquipmentAppointmentsInRange)
        {
            Console.WriteLine("[SKIP] EquipmentAppointment data already exists in the range (past 3 years to future 1 year).");
            return;
        }

        Console.WriteLine("[INFO] Creating equipment appointment seed data...");
        var equipments = await context.Equipments.Where(e => !e.IsDeleted).ToListAsync();
        var patients = await context.Patients.Where(p => !p.IsDeleted).ToListAsync();
        var orgs = await context.Organizations.Where(o => !o.IsDeleted).ToListAsync();
        var holidays = await SeederHelper.LoadHolidaySetAsync(context);

        if (equipments.Any() && patients.Any() && orgs.Any())
        {
            var equipmentAppointments = new List<EquipmentAppointment>();
            var random = new Random(42);

                // 腹部エコー設備を特定
                var abdominalEcho = equipments.FirstOrDefault(e => e.EquipName.Contains("腹部エコー") || e.EquipDesc.Contains("腹部"));

                // 過去3年から未来1年まで（営業カレンダー準拠、例外あり）
                for (var apptDate = startDate; apptDate <= endDate; apptDate = apptDate.AddDays(1))
                {
                    var dayCtx = SeedBusinessCalendar.GetDayContext(apptDate, holidays, random);
                    if (dayCtx.DayType == SeedDayType.Closed)
                        continue;

                    var isSaturday = dayCtx.DayType == SeedDayType.SaturdayMorning;
                    var isIrregularOpen = dayCtx.DayType == SeedDayType.IrregularOpen;
                    var date = apptDate.ToDateTime(new TimeOnly(0, 0));

                    // 各設備に対して予約を生成
                    foreach (var equipment in equipments)
                    {
                        var isAbdominalEcho = equipment.EquipId == abdominalEcho?.EquipId;

                        if (isAbdominalEcho)
                        {
                            // 腹部エコーはAM/PMで大枠予約
                            // 午前中: 9:00-12:00の大枠
                            var amStart = new DateTime(date.Year, date.Month, date.Day, 9, 0, 0, DateTimeKind.Utc);
                            var amEnd = new DateTime(date.Year, date.Month, date.Day, 12, 0, 0, DateTimeKind.Utc);

                            // 午前中に3-5人の予約
                            var amCount = isIrregularOpen ? random.Next(1, 3) : random.Next(3, 6);
                            for (var j = 0; j < amCount; j++)
                            {
                                var minutesOffset = random.Next(0, 180); // 9:00-12:00の間でランダム
                                var apptStart = amStart.AddMinutes(minutesOffset);
                                var apptEnd = apptStart.AddMinutes(30);

                                equipmentAppointments.Add(new EquipmentAppointment
                                {
                                    EquipApptId = Guid.NewGuid(),
                                    EquipId = equipment.EquipId,
                                    OrgId = orgs[random.Next(orgs.Count)].OrgId,
                                    PtId = patients[random.Next(patients.Count)].PtId,
                                    ApptDate = apptDate,
                                    ApptStartAt = apptStart,
                                    ApptEndAt = apptEnd,
                                    ApptStatusCode = GetStatusCode(apptDate, today, random),
                                    ApptMemo = isIrregularOpen ? "腹部エコー AM枠（イレギュラー）" : "腹部エコー AM枠",
                                    IsDeleted = false,
                                    CreatedAt = dateTimeProvider.Now.AddDays(apptDate.DayNumber - today.DayNumber),
                                    UpdatedAt = dateTimeProvider.Now.AddDays(apptDate.DayNumber - today.DayNumber),
                                    CreatedUserId = Guid.Empty,
                                    CreatedSessionId = Guid.Empty,
                                    UpdatedUserId = Guid.Empty,
                                    UpdatedSessionId = Guid.Empty
                                });
                            }

                            // 午後: 13:00-17:00の大枠（平日のみ）
                            if (!isSaturday && !isIrregularOpen)
                            {
                                var pmStart = new DateTime(date.Year, date.Month, date.Day, 13, 0, 0, DateTimeKind.Utc);
                                var pmEnd = new DateTime(date.Year, date.Month, date.Day, 17, 0, 0, DateTimeKind.Utc);

                                // 午後に3-5人の予約
                                var pmCount = random.Next(3, 6);
                                for (var j = 0; j < pmCount; j++)
                                {
                                    var minutesOffset = random.Next(0, 240); // 13:00-17:00の間でランダム
                                    var apptStart = pmStart.AddMinutes(minutesOffset);
                                    var apptEnd = apptStart.AddMinutes(30);

                                    equipmentAppointments.Add(new EquipmentAppointment
                                    {
                                        EquipApptId = Guid.NewGuid(),
                                        EquipId = equipment.EquipId,
                                        OrgId = orgs[random.Next(orgs.Count)].OrgId,
                                        PtId = patients[random.Next(patients.Count)].PtId,
                                        ApptDate = apptDate,
                                        ApptStartAt = apptStart,
                                        ApptEndAt = apptEnd,
                                        ApptStatusCode = GetStatusCode(apptDate, today, random),
                                        ApptMemo = "腹部エコー PM枠",
                                        IsDeleted = false,
                                        CreatedAt = dateTimeProvider.Now.AddDays(apptDate.DayNumber - today.DayNumber),
                                        UpdatedAt = dateTimeProvider.Now.AddDays(apptDate.DayNumber - today.DayNumber),
                                        CreatedUserId = Guid.Empty,
                                        CreatedSessionId = Guid.Empty,
                                        UpdatedUserId = Guid.Empty,
                                        UpdatedSessionId = Guid.Empty
                                    });
                                }
                            }
                        }
                        else
                        {
                            // その他の設備は通常の予約パターン
                            var count = isIrregularOpen ? random.Next(0, 2) : (isSaturday ? random.Next(0, 3) : random.Next(1, 4)); // 土曜は少なめ
                            for (var j = 0; j < count; j++)
                            {
                                var slotTimes = isSaturday || isIrregularOpen
                                    ? new[] { "09:00", "10:00", "11:00" }
                                    : new[] { "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };
                                var slotTime = slotTimes[random.Next(slotTimes.Length)];
                                var hour = Int32.Parse(slotTime.Split(':')[0]);
                                var apptStart = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0, DateTimeKind.Utc);
                                var apptEnd = apptStart.AddMinutes(30);

                                equipmentAppointments.Add(new EquipmentAppointment
                                {
                                    EquipApptId = Guid.NewGuid(),
                                    EquipId = equipment.EquipId,
                                    OrgId = orgs[random.Next(orgs.Count)].OrgId,
                                    PtId = patients[random.Next(patients.Count)].PtId,
                                    ApptDate = apptDate,
                                    ApptStartAt = apptStart,
                                    ApptEndAt = apptEnd,
                                    ApptStatusCode = GetStatusCode(apptDate, today, random),
                                    ApptMemo = isIrregularOpen ? "設備予約（イレギュラー）" : "設備予約",
                                    IsDeleted = false,
                                    CreatedAt = dateTimeProvider.Now.AddDays(apptDate.DayNumber - today.DayNumber),
                                    UpdatedAt = dateTimeProvider.Now.AddDays(apptDate.DayNumber - today.DayNumber),
                                    CreatedUserId = Guid.Empty,
                                    CreatedSessionId = Guid.Empty,
                                    UpdatedUserId = Guid.Empty,
                                    UpdatedSessionId = Guid.Empty
                                });
                            }
                        }
                    }
                }

                context.EquipmentAppointments.AddRange(equipmentAppointments);
                Console.WriteLine($"  [+] EquipmentAppointments: {equipmentAppointments.Count} entries");
            }
            else
            {
                Console.WriteLine("[SKIP] No equipments, patients, or organizations found. Skipping equipment appointment seed data.");
            }
    }

    private static int GetStatusCode(DateOnly date, DateOnly today, Random random)
    {
        if (date < today)
        {
            return random.Next(100) < 80 ? 2 : 3;
        }

        if (date == today)
        {
            return random.Next(100) < 70 ? 1 : 2;
        }

        return random.Next(100) < 20 ? 0 : 1;
    }
}


