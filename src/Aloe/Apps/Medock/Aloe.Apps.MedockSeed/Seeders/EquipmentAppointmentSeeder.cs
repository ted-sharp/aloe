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
                var today = dateTimeProvider.Today;
                
                // 腹部エコー設備を特定
                var abdominalEcho = equipments.FirstOrDefault(e => e.EquipName.Contains("腹部エコー") || e.EquipDesc.Contains("腹部"));
                
                // 過去60日から未来90日まで
                for (var i = -60; i <= 90; i++)
                {
                    var date = today.AddDays(i);
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    var apptDate = DateOnly.FromDateTime(date);
                    
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
                            var amCount = random.Next(3, 6);
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
                                    ApptStatusCode = i < 0 ? (random.Next(100) < 80 ? 2 : 3) : 1,
                                    ApptMemo = "腹部エコー AM枠",
                                    IsDeleted = false,
                                    CreatedAt = dateTimeProvider.Now.AddDays(i),
                                    UpdatedAt = dateTimeProvider.Now.AddDays(i),
                                    CreatedUserId = Guid.Empty,
                                    CreatedSessionId = Guid.Empty,
                                    UpdatedUserId = Guid.Empty,
                                    UpdatedSessionId = Guid.Empty
                                });
                            }
                            
                            // 午後: 13:00-17:00の大枠
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
                                    ApptStatusCode = i < 0 ? (random.Next(100) < 80 ? 2 : 3) : 1,
                                    ApptMemo = "腹部エコー PM枠",
                                    IsDeleted = false,
                                    CreatedAt = dateTimeProvider.Now.AddDays(i),
                                    UpdatedAt = dateTimeProvider.Now.AddDays(i),
                                    CreatedUserId = Guid.Empty,
                                    CreatedSessionId = Guid.Empty,
                                    UpdatedUserId = Guid.Empty,
                                    UpdatedSessionId = Guid.Empty
                                });
                            }
                        }
                        else
                        {
                            // その他の設備は通常の予約パターン
                            var count = random.Next(1, 4); // 1日あたり1-3件
                            for (var j = 0; j < count; j++)
                            {
                                var slotTimes = new[] { "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00" };
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
                                    ApptStatusCode = i < 0 ? (random.Next(100) < 80 ? 2 : 3) : 1,
                                    ApptMemo = "設備予約",
                                    IsDeleted = false,
                                    CreatedAt = dateTimeProvider.Now.AddDays(i),
                                    UpdatedAt = dateTimeProvider.Now.AddDays(i),
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
        }
        else
        {
            Console.WriteLine("[SKIP] EquipmentAppointments already exist.");
        }
    }
}


