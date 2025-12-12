using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid? floorId, IDateTimeProvider dateTimeProvider)
    {
        if (!floorId.HasValue)
        {
            Console.WriteLine("[SKIP] FloorId is not set. Skipping appointment seed data.");
            return;
        }

        var today = dateTimeProvider.TodayDateOnly;
        var startDate = today.AddYears(-3);
        var endDate = today.AddYears(1);

        // 過去3年〜未来1年の範囲に既存データが存在するかチェック
        var existingAppointmentsInRange = await context.Appointments
            .Where(a => !a.IsDeleted && a.ApptDate >= startDate && a.ApptDate <= endDate)
            .AnyAsync();

        if (existingAppointmentsInRange)
        {
            Console.WriteLine("[SKIP] Appointment data already exists in the range (past 3 years to future 1 year).");
            return;
        }

        Console.WriteLine("[INFO] Creating appointment seed data...");
        var patients = await context.Patients.Where(p => !p.IsDeleted).ToListAsync();
        var orgs = await context.Organizations.Where(o => !o.IsDeleted).ToListAsync();
        var holidays = await SeedBusinessCalendar.LoadHolidaySetAsync(context);

        if (patients.Any() && orgs.Any())
        {
            var appointments = new List<Appointment>();
            var random = new Random(42);

                // ステータスコード: 0=仮押, 1=予約確定, 2=来院済み, 3=検査完了, 4=キャンセル, 5=無断キャンセル

                // ロッカー制約: AM/PMで一度に受け付ける最大人数（ロッカーの数）
                const int maxLockerCapacityAM = 20; // 午前中の最大人数
                const int maxLockerCapacityPM = 20; // 午後の最大人数

                // 基本的な営業時間内のスロット（始業9:00、就業18:00、昼休み12:00-13:00を考慮）
                var regularMorningSlots = new[] { "09:00", "09:15", "09:30", "09:45", "10:00", "10:15", "10:30", "10:45", "11:00", "11:15", "11:30", "11:45" }; // 始業～昼休み前（15分単位）
                var regularAfternoonSlots = new[] { "13:00", "13:15", "13:30", "13:45", "14:00", "14:15", "14:30", "14:45", "15:00", "15:15", "15:30", "15:45", "16:00", "16:15", "16:30", "16:45", "17:00" }; // 昼休み後～就業前（15分単位）
                var saturdayMorningSlots = new[] { "09:00", "09:30", "10:00", "10:30", "11:00", "11:30" }; // 土曜午前（30分単位）

                // イレギュラーパターン
                var earlyMorningSlots = new[] { "07:00", "07:30", "08:00", "08:30" }; // 早朝
                var eveningSlots = new[] { "18:00", "18:30", "19:00", "19:30" }; // 夜間
                var lunchSlots = new[] { "12:00", "12:15", "12:30", "12:45" }; // 昼休み中（イレギュラー）

                // 日付パターンの判定
                bool IsConferenceDay(DateOnly date) => date.Day % 15 == 0 || date.Day % 23 == 0; // 学会日（月に2-3回程度）
                bool IsLowStaffDay(DateOnly date) => date.Day % 7 == 0 || date.Day % 11 == 0; // スタッフ不足日（月に4-5回程度）

                // 予約を作成するヘルパー関数
                Appointment CreateAppointment(DateOnly apptDate, int hour, int minute, int durationMinutes, int statusCode, string? memo = null)
                {
                    var apptStart = new DateTime(apptDate.Year, apptDate.Month, apptDate.Day, hour, minute, 0, DateTimeKind.Utc);
                    var apptEnd = apptStart.AddMinutes(durationMinutes);

                    return new Appointment
                    {
                        ApptId = Guid.NewGuid(),
                        FloorId = floorId.Value,
                        OrgId = orgs[random.Next(orgs.Count)].OrgId,
                        PtId = patients[random.Next(patients.Count)].PtId,
                        ApptDate = apptDate,
                        ApptStartAt = apptStart,
                        ApptEndAt = apptEnd,
                        ApptStatusCode = statusCode,
                        ApptMemo = memo ?? "",
                        IsDeleted = false,
                        CreatedAt = dateTimeProvider.Now.AddDays((apptDate.DayNumber - today.DayNumber)),
                        UpdatedAt = dateTimeProvider.Now.AddDays((apptDate.DayNumber - today.DayNumber)),
                        CreatedUserId = Guid.Empty,
                        CreatedSessionId = Guid.Empty,
                        UpdatedUserId = Guid.Empty,
                        UpdatedSessionId = Guid.Empty
                    };
                }

                // 時間文字列を解析するヘルパー関数
                (int hour, int minute) ParseTime(string timeStr)
                {
                    var parts = timeStr.Split(':');
                    return (Int32.Parse(parts[0]), Int32.Parse(parts[1]));
                }

                // 過去3年〜未来1年の予約（営業カレンダー準拠、例外あり）
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var offsetDays = date.DayNumber - today.DayNumber;
                    var dayCtx = SeedBusinessCalendar.GetDayContext(date, holidays, random);
                    if (dayCtx.DayType == SeedDayType.Closed)
                    {
                        continue;
                    }

                    var isSaturday = dayCtx.DayType == SeedDayType.SaturdayMorning;
                    var isIrregularOpen = dayCtx.DayType == SeedDayType.IrregularOpen;
                    var isConference = IsConferenceDay(date);
                    var isLowStaff = IsLowStaffDay(date);

                    // 予約数の決定（ロッカー制約を考慮）
                    int baseCount;
                    if (isIrregularOpen)
                    {
                        baseCount = random.Next(1, 5); // 例外営業は1-4件
                    }
                    else if (isSaturday)
                    {
                        baseCount = random.Next(6, 18); // 土曜午前は6-17件
                    }
                    else if (isConference)
                    {
                        baseCount = random.Next(5, 12); // 学会日は通常の30-50%（5-11件）
                    }
                    else if (isLowStaff)
                    {
                        baseCount = random.Next(8, 18); // スタッフ不足日は通常の50-70%（8-17件）
                    }
                    else
                    {
                        baseCount = random.Next(15, 35); // 通常日は15-34件
                    }

                    // AM/PMでロッカー制約を考慮
                    var amCount = 0;
                    var pmCount = 0;
                    if (isSaturday || isIrregularOpen)
                    {
                        amCount = Math.Min(baseCount, maxLockerCapacityAM);
                        pmCount = 0;
                    }
                    else
                    {
                        amCount = Math.Min(baseCount / 2 + random.Next(-2, 3), maxLockerCapacityAM);
                        pmCount = Math.Min(baseCount - amCount, maxLockerCapacityPM);
                    }

                    // 日付パターンのメモ
                    string? dateMemo = null;
                    if (isConference) dateMemo = "学会日（ドクター不在）";
                    else if (isLowStaff) dateMemo = "スタッフ不足日";
                    if (!String.IsNullOrWhiteSpace(dayCtx.DayMemo))
                    {
                        dateMemo = dateMemo != null ? $"{dateMemo} - {dayCtx.DayMemo}" : dayCtx.DayMemo;
                    }

                    // 午前中の予約を生成
                    for (var j = 0; j < amCount; j++)
                    {
                        string slotTime;
                        int duration;
                        string? memo = dateMemo;

                        // 85%は通常パターン、15%はイレギュラーパターン
                        if (random.Next(100) < 85)
                        {
                            slotTime = isSaturday || isIrregularOpen
                                ? saturdayMorningSlots[random.Next(saturdayMorningSlots.Length)]
                                : regularMorningSlots[random.Next(regularMorningSlots.Length)];
                            duration = random.Next(100) < 70 ? 60 : (random.Next(100) < 50 ? 90 : 120);
                        }
                        else
                        {
                            // イレギュラーパターン（早朝または昼休み）
                            if (random.Next(100) < 70)
                            {
                                slotTime = earlyMorningSlots[random.Next(earlyMorningSlots.Length)];
                                duration = 60;
                                memo = memo != null ? $"{memo} - 早朝予約" : "早朝予約";
                            }
                            else
                            {
                                // 土曜/例外営業は昼休み予約を抑制
                                if (isSaturday || isIrregularOpen)
                                {
                                    slotTime = saturdayMorningSlots[random.Next(saturdayMorningSlots.Length)];
                                    duration = 60;
                                }
                                else
                                {
                                    slotTime = lunchSlots[random.Next(lunchSlots.Length)];
                                    duration = 30;
                                    memo = memo != null ? $"{memo} - 昼休み予約" : "昼休み予約";
                                }
                            }
                        }

                        var (hour, minute) = ParseTime(slotTime);
                        var statusCode = GetStatusCode(date, today, random, ref memo);

                        appointments.Add(CreateAppointment(date, hour, minute, duration, statusCode, memo));
                    }

                    // 午後の予約を生成
                    for (var j = 0; j < pmCount; j++)
                    {
                        string slotTime;
                        int duration;
                        string? memo = dateMemo;

                        // 85%は通常パターン、15%はイレギュラーパターン
                        if (random.Next(100) < 85)
                        {
                            slotTime = regularAfternoonSlots[random.Next(regularAfternoonSlots.Length)];
                            duration = random.Next(100) < 70 ? 60 : (random.Next(100) < 50 ? 90 : 120);
                        }
                        else
                        {
                            // イレギュラーパターン（夜間または長時間）
                            if (random.Next(100) < 70)
                            {
                                slotTime = eveningSlots[random.Next(eveningSlots.Length)];
                                duration = random.Next(100) < 70 ? 60 : 90;
                                memo = memo != null ? $"{memo} - 夜間予約" : "夜間予約";
                            }
                            else
                            {
                                slotTime = regularAfternoonSlots[random.Next(regularAfternoonSlots.Length)];
                                duration = random.Next(100) < 50 ? 180 : 120;
                                memo = memo != null ? $"{memo} - 長時間予約" : "長時間予約";
                            }
                        }

                        var (hour, minute) = ParseTime(slotTime);
                        var statusCode = GetStatusCode(date, today, random, ref memo);

                        appointments.Add(CreateAppointment(date, hour, minute, duration, statusCode, memo));
                    }
                }

                context.Appointments.AddRange(appointments);
                Console.WriteLine($"  [+] Appointments: {appointments.Count} entries");
            }
            else
            {
                Console.WriteLine("[SKIP] No patients or organizations found. Skipping appointment seed data.");
            }
    }

    private static int GetStatusCode(DateOnly date, DateOnly today, Random random, ref string? memo)
    {
        // 過去：来院済み/検査完了中心、キャンセルも混ぜる
        if (date < today)
        {
            var roll = random.Next(100);
            if (roll < 55) return 2;
            if (roll < 80) return 3;
            if (roll < 92)
            {
                memo = memo != null ? $"{memo} - キャンセル" : "キャンセル";
                return 4;
            }

            memo = memo != null ? $"{memo} - 無断キャンセル" : "無断キャンセル";
            return 5;
        }

        // 当日：確定/来院済み
        if (date == today)
        {
            return random.Next(100) < 70 ? 1 : 2;
        }

        // 未来：仮押/確定（稀にキャンセル）
        var futureRoll = random.Next(100);
        if (futureRoll < 20) return 0;
        if (futureRoll < 95) return 1;
        memo = memo != null ? $"{memo} - キャンセル" : "キャンセル";
        return 4;
    }
}



