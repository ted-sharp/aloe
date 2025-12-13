using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentSeeder
{
    private static readonly Random _random = new Random();

    public static async Task SeedAsync(MedockDbContext context, Guid? floorId, IDateTimeProvider dateTimeProvider)
    {
        if (!floorId.HasValue)
        {
            Console.WriteLine("[SKIP] Appointment: No floor ID provided.");
            return;
        }

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

        var floor = await context.Floors.FirstOrDefaultAsync(f => f.FloorId == floorId.Value);
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

        var (startDate, endDate) = SeederHelper.GetDefaultDateRange(dateTimeProvider);

        Console.WriteLine("[INFO] Creating appointment seed data...");

        var appointments = new List<Appointment>();
        var currentDate = startDate;

        while (currentDate <= endDate)
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
                var appointmentsForDay = GenerateAppointmentsForDay(
                    currentDate,
                    dayContext,
                    floor,
                    patients,
                    organizations,
                    dateTimeProvider);

                appointments.AddRange(appointmentsForDay);
            }

            currentDate = currentDate.AddDays(1);
        }

        Console.WriteLine($"  [+] Appointments: {appointments.Count} entries (from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})");

        context.Appointments.AddRange(appointments);
    }

    /// <summary>
    /// 日付の営業コンテキストを取得
    /// </summary>
    private static AppointmentDayContext GetDayContext(DateOnly date, HashSet<DateOnly> holidays)
    {
        var dayOfWeek = date.DayOfWeek;
        var isHoliday = holidays.Contains(date);

        // 日曜または祝日は休み
        if (dayOfWeek == DayOfWeek.Sunday || isHoliday)
        {
            return new AppointmentDayContext
            {
                IsOpen = false,
                IsMorningOnly = false,
                IsIrregular = false
            };
        }

        // 水曜・土曜は午前のみ
        if (dayOfWeek == DayOfWeek.Wednesday || dayOfWeek == DayOfWeek.Saturday)
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
    private static List<Appointment> GenerateAppointmentsForDay(
        DateOnly date,
        AppointmentDayContext dayContext,
        Floor floor,
        List<Patient> patients,
        List<Organization> organizations,
        IDateTimeProvider dateTimeProvider)
    {
        var appointments = new List<Appointment>();

        // 1日あたり10～20件の予約を生成
        var appointmentCount = _random.Next(10, 21);

        for (int i = 0; i < appointmentCount; i++)
        {
            var patient = patients[_random.Next(patients.Count)];
            var organization = organizations[_random.Next(organizations.Count)];

            // 時間帯を決定
            TimeOnly? startTime = null;
            int? durationMin = null;
            TimeOnly? endTime = null;

            if (dayContext.IsMorningOnly)
            {
                // 午前のみ（09:00-12:00）
                var morningTimes = SeederHelper.TimeSlots.MorningSlots;
                var timeStr = morningTimes[_random.Next(morningTimes.Length)];
                if (TimeOnly.TryParse(timeStr, out var parsedTime))
                {
                    startTime = parsedTime;
                    durationMin = 30 + _random.Next(3) * 15; // 30, 45, 60分
                    endTime = startTime.Value.AddMinutes(durationMin.Value);
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
                    startTime = parsedTime;
                    durationMin = 30 + _random.Next(3) * 15; // 30, 45, 60分
                    endTime = startTime.Value.AddMinutes(durationMin.Value);
                }
            }

            if (!startTime.HasValue)
            {
                continue;
            }

            // 予約ステータス（約95%が予約済み、5%がその他）
            var statusCode = _random.Next(100) < 95 ? 0 : _random.Next(1, 5);

            var appointment = new Appointment
            {
                ApptId = Guid.NewGuid(),
                FloorId = floor.FloorId,
                OrgId = organization.OrgId,
                PtId = patient.PtId,
                ApptDate = date,
                ApptStartTime = startTime,
                ApptDurationMin = durationMin,
                ApptEndTime = endTime,
                ApptStatusCode = statusCode,
                ApptMemo = dayContext.IsIrregular ? "イレギュラー営業" : String.Empty,
                IsDeleted = false
            };

            SeederHelper.InitializeAuditFields(appointment, dateTimeProvider);
            appointments.Add(appointment);
        }

        return appointments;
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

