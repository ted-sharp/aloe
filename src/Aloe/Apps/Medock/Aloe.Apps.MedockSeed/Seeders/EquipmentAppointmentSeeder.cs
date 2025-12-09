using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class EquipmentAppointmentSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var existingEquipmentAppointments = await context.EquipmentAppointments.AnyAsync();
        if (!existingEquipmentAppointments)
        {
            Console.WriteLine("[INFO] Creating equipment appointment seed data...");
            var equipments = await context.Equipments.Where(e => !e.IsDeleted).ToListAsync();
            var patients = await context.Patients.Where(p => !p.IsDeleted).ToListAsync();
            var orgs = await context.Organizations.Where(o => !o.IsDeleted).ToListAsync();

            if (equipments.Any() && patients.Any() && orgs.Any())
            {
                var equipmentAppointments = new List<EquipmentAppointment>();
                var random = new Random(42);
                var startDate = dateTimeProvider.Today.AddDays(1);

                for (var i = 0; i < 15; i++)
                {
                    var date = startDate.AddDays(i);
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    var apptDate = DateOnly.FromDateTime(date);
                    var slotTimes = new[] { "08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };
                    var slotTime = slotTimes[random.Next(slotTimes.Length)];
                    var apptStart = new DateTime(date.Year, date.Month, date.Day, Int32.Parse(slotTime.Split(':')[0]), 0, 0, DateTimeKind.Utc);
                    var apptEnd = apptStart.AddMinutes(30);

                    equipmentAppointments.Add(new EquipmentAppointment
                    {
                        EquipApptId = Guid.NewGuid(),
                        EquipId = equipments[random.Next(equipments.Count)].EquipId,
                        OrgId = orgs[random.Next(orgs.Count)].OrgId,
                        PtId = patients[random.Next(patients.Count)].PtId,
                        ApptDate = apptDate,
                        ApptStartAt = apptStart,
                        ApptEndAt = apptEnd,
                        ApptStatusCode = 1,
                        ApptMemo = "設備予約",
                        IsDeleted = false,
                        CreatedAt = dateTimeProvider.Now,
                        UpdatedAt = dateTimeProvider.Now,
                        CreatedUserId = Guid.Empty,
                        CreatedSessionId = Guid.Empty,
                        UpdatedUserId = Guid.Empty,
                        UpdatedSessionId = Guid.Empty
                    });
                }

                context.EquipmentAppointments.AddRange(equipmentAppointments);
                Console.WriteLine($"  [+] EquipmentAppointments: {equipmentAppointments.Count} entries");
            }
        }
        else
        {
            Console.WriteLine("[SKIP] EquipmentAppointments already exist.");
        }
    }
}


