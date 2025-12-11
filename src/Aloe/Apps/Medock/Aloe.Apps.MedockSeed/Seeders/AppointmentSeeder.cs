using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid? floorId, IDateTimeProvider dateTimeProvider)
    {
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
                var today = dateTimeProvider.TodayDateOnly;

                // ステータスコード: 0=仮押, 1=予約確定, 2=来院済み, 3=検査完了, 4=キャンセル, 5=無断キャンセル
                
                // ロッカー制約: AM/PMで一度に受け付ける最大人数（ロッカーの数）
                const int maxLockerCapacityAM = 20; // 午前中の最大人数
                const int maxLockerCapacityPM = 20; // 午後の最大人数
                
                // 基本的な営業時間内のスロット（始業9:00、就業18:00、昼休み12:00-13:00を考慮）
                var regularMorningSlots = new[] { "09:00", "09:15", "09:30", "09:45", "10:00", "10:15", "10:30", "10:45", "11:00", "11:15", "11:30", "11:45" }; // 始業～昼休み前（15分単位）
                var regularAfternoonSlots = new[] { "13:00", "13:15", "13:30", "13:45", "14:00", "14:15", "14:30", "14:45", "15:00", "15:15", "15:30", "15:45", "16:00", "16:15", "16:30", "16:45", "17:00" }; // 昼休み後～就業前（15分単位）
                
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

                // 過去90日分の予約（来院済み、検査完了、キャンセルなど）
                for (var i = -90; i < 0; i++)
                {
                    var date = today.AddDays(i);
                    var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                    var isConference = IsConferenceDay(date);
                    var isLowStaff = IsLowStaffDay(date);
                    
                    // 予約数の決定（ロッカー制約を考慮）
                    int baseCount;
                    if (isWeekend)
                    {
                        baseCount = random.Next(0, 5); // 休日は0-4件
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
                    var amCount = Math.Min(baseCount / 2 + random.Next(-2, 3), maxLockerCapacityAM);
                    var pmCount = Math.Min(baseCount - amCount, maxLockerCapacityPM);
                    
                    // 日付パターンのメモ
                    string? dateMemo = null;
                    if (isConference) dateMemo = "学会日（ドクター不在）";
                    else if (isLowStaff) dateMemo = "スタッフ不足日";
                    
                    // 午前中の予約を生成
                    for (var j = 0; j < amCount; j++)
                    {
                        string slotTime;
                        int duration;
                        string? memo = dateMemo;
                        
                        // 85%は通常パターン、15%はイレギュラーパターン
                        if (random.Next(100) < 85)
                        {
                            slotTime = regularMorningSlots[random.Next(regularMorningSlots.Length)];
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
                                slotTime = lunchSlots[random.Next(lunchSlots.Length)];
                                duration = 30;
                                memo = memo != null ? $"{memo} - 昼休み予約" : "昼休み予約";
                            }
                        }
                        
                        var (hour, minute) = ParseTime(slotTime);
                        var statusCode = random.Next(100) < 75 ? (random.Next(100) < 60 ? 2 : 3) : 4;
                        if (statusCode == 4)
                        {
                            memo = memo != null ? $"{memo} - キャンセル" : "キャンセル";
                        }
                        
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
                        var statusCode = random.Next(100) < 75 ? (random.Next(100) < 60 ? 2 : 3) : 4;
                        if (statusCode == 4)
                        {
                            memo = memo != null ? $"{memo} - キャンセル" : "キャンセル";
                        }
                        
                        appointments.Add(CreateAppointment(date, hour, minute, duration, statusCode, memo));
                    }
                }

                // 今日の予約（予約確定、来院済みなど）
                var todayIsWeekend = today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday;
                var todayIsConference = IsConferenceDay(today);
                var todayIsLowStaff = IsLowStaffDay(today);
                
                int todayBaseCount;
                if (todayIsWeekend)
                {
                    todayBaseCount = random.Next(0, 5);
                }
                else if (todayIsConference)
                {
                    todayBaseCount = random.Next(5, 12);
                }
                else if (todayIsLowStaff)
                {
                    todayBaseCount = random.Next(8, 18);
                }
                else
                {
                    todayBaseCount = random.Next(15, 35);
                }
                
                var todayAmCount = Math.Min(todayBaseCount / 2 + random.Next(-2, 3), maxLockerCapacityAM);
                var todayPmCount = Math.Min(todayBaseCount - todayAmCount, maxLockerCapacityPM);
                
                string? todayMemo = null;
                if (todayIsConference) todayMemo = "学会日（ドクター不在）";
                else if (todayIsLowStaff) todayMemo = "スタッフ不足日";
                
                // 午前中の予約
                for (var j = 0; j < todayAmCount; j++)
                {
                    string slotTime = regularMorningSlots[random.Next(regularMorningSlots.Length)];
                    int duration = random.Next(100) < 70 ? 60 : 90;
                    var (hour, minute) = ParseTime(slotTime);
                    var statusCode = random.Next(100) < 70 ? 1 : 2;
                    appointments.Add(CreateAppointment(today, hour, minute, duration, statusCode, todayMemo));
                }
                
                // 午後の予約
                for (var j = 0; j < todayPmCount; j++)
                {
                    string slotTime = regularAfternoonSlots[random.Next(regularAfternoonSlots.Length)];
                    int duration = random.Next(100) < 70 ? 60 : 90;
                    var (hour, minute) = ParseTime(slotTime);
                    var statusCode = random.Next(100) < 70 ? 1 : 2;
                    appointments.Add(CreateAppointment(today, hour, minute, duration, statusCode, todayMemo));
                }

                // 未来90日分の予約（仮押、予約確定）
                for (var i = 1; i <= 90; i++)
                {
                    var date = today.AddDays(i);
                    var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                    var isConference = IsConferenceDay(date);
                    var isLowStaff = IsLowStaffDay(date);
                    
                    int baseCount;
                    if (isWeekend)
                    {
                        baseCount = random.Next(0, 5);
                    }
                    else if (isConference)
                    {
                        baseCount = random.Next(5, 12);
                    }
                    else if (isLowStaff)
                    {
                        baseCount = random.Next(8, 18);
                    }
                    else
                    {
                        baseCount = random.Next(15, 35);
                    }
                    
                    var amCount = Math.Min(baseCount / 2 + random.Next(-2, 3), maxLockerCapacityAM);
                    var pmCount = Math.Min(baseCount - amCount, maxLockerCapacityPM);
                    
                    string? dateMemo = null;
                    if (isConference) dateMemo = "学会日（ドクター不在）";
                    else if (isLowStaff) dateMemo = "スタッフ不足日";
                    
                    // 午前中の予約
                    for (var j = 0; j < amCount; j++)
                    {
                        string slotTime;
                        int duration;
                        string? memo = dateMemo;
                        
                        if (random.Next(100) < 85)
                        {
                            slotTime = regularMorningSlots[random.Next(regularMorningSlots.Length)];
                            duration = random.Next(100) < 70 ? 60 : (random.Next(100) < 50 ? 90 : 120);
                        }
                        else
                        {
                            if (random.Next(100) < 70)
                            {
                                slotTime = earlyMorningSlots[random.Next(earlyMorningSlots.Length)];
                                duration = 60;
                                memo = memo != null ? $"{memo} - 早朝予約" : "早朝予約";
                            }
                            else
                            {
                                slotTime = lunchSlots[random.Next(lunchSlots.Length)];
                                duration = 30;
                                memo = memo != null ? $"{memo} - 昼休み予約" : "昼休み予約";
                            }
                        }
                        
                        var (hour, minute) = ParseTime(slotTime);
                        var statusCode = random.Next(100) < 25 ? 0 : 1;
                        appointments.Add(CreateAppointment(date, hour, minute, duration, statusCode, memo));
                    }
                    
                    // 午後の予約
                    for (var j = 0; j < pmCount; j++)
                    {
                        string slotTime;
                        int duration;
                        string? memo = dateMemo;
                        
                        if (random.Next(100) < 85)
                        {
                            slotTime = regularAfternoonSlots[random.Next(regularAfternoonSlots.Length)];
                            duration = random.Next(100) < 70 ? 60 : (random.Next(100) < 50 ? 90 : 120);
                        }
                        else
                        {
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
                        var statusCode = random.Next(100) < 25 ? 0 : 1;
                        appointments.Add(CreateAppointment(date, hour, minute, duration, statusCode, memo));
                    }
                }

                context.Appointments.AddRange(appointments);
                Console.WriteLine($"  [+] Appointments: {appointments.Count} entries");
            }
        }
        else if (existingAppointments)
        {
            Console.WriteLine("[SKIP] Appointments already exist.");
        }
    }
}


