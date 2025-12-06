using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid? floorId)
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
                var startDate = DateTime.Today.AddDays(1);

                for (var i = 0; i < 20; i++)
                {
                    var date = startDate.AddDays(i);
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    var apptDate = DateOnly.FromDateTime(date);
                    var slotTimes = new[] { "08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };
                    var slotTime = slotTimes[random.Next(slotTimes.Length)];
                    var apptStart = new DateTime(date.Year, date.Month, date.Day, Int32.Parse(slotTime.Split(':')[0]), 0, 0, DateTimeKind.Utc);
                    var apptEnd = apptStart.AddMinutes(60);

                    appointments.Add(new Appointment
                    {
                        ApptId = Guid.NewGuid(),
                        FloorId = floorId.Value,
                        OrgId = orgs[random.Next(orgs.Count)].OrgId,
                        PtId = patients[random.Next(patients.Count)].PtId,
                        ApptDate = apptDate,
                        ApptStartAt = apptStart,
                        ApptEndAt = apptEnd,
                        ApptStatusCode = 1,
                        IsDeleted = false,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        CreatedUserId = Guid.Empty,
                        CreatedSessionId = Guid.Empty,
                        UpdatedUserId = Guid.Empty,
                        UpdatedSessionId = Guid.Empty
                    });
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

