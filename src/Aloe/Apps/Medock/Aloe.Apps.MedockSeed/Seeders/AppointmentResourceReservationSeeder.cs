using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class AppointmentResourceReservationSeeder
{
    private static readonly Random _random = new Random();

    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        // テーブルが存在するか確認
        try
        {
            var existingReservations = await context.AppointmentResourceReservations.AnyAsync();
            if (existingReservations)
            {
                Console.WriteLine("[SKIP] AppointmentResourceReservations already exist.");
                return;
            }
        }
        catch (PostgresException ex) when (ex.SqlState == "42P01")
        {
            // テーブルが存在しない場合は続行（初回実行時）
        }

        var appointments = await context.Appointments
            .Where(a => !a.IsDeleted)
            .ToListAsync();

        if (!appointments.Any())
        {
            Console.WriteLine("[SKIP] AppointmentResourceReservation: No appointments found.");
            return;
        }

        var resources = await context.AppointmentResources
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        if (!resources.Any())
        {
            Console.WriteLine("[SKIP] AppointmentResourceReservation: No appointment resources found.");
            return;
        }

        Console.WriteLine("[INFO] Creating appointment resource reservation seed data...");

        var reservations = new List<AppointmentResourceAssignment>();

        foreach (var appointment in appointments)
        {
            // 各予約に対して1～2個のリソースを関連付け
            var resourceCount = _random.Next(1, 3); // 1 or 2

            // 予約のフロアに属するリソースを優先
            var floorResources = resources.Where(r => r.FloorId == appointment.FloorId).ToList();
            var availableResources = floorResources.Any() ? floorResources : resources;

            var selectedResources = availableResources
                .OrderBy(_ => _random.Next())
                .Take(resourceCount)
                .ToList();

            foreach (var resource in selectedResources)
            {
                var reservation = new AppointmentResourceAssignment
                {
                    ApptResAssignId = Guid.NewGuid(),
                    ApptId = appointment.ApptId,
                    ApptResId = resource.ApptResId,
                    IsDeleted = false
                };

                SeederHelper.InitializeAuditFields(reservation, dateTimeProvider);
                reservations.Add(reservation);
            }
        }

        context.AppointmentResourceReservations.AddRange(reservations);
        Console.WriteLine($"  [+] AppointmentResourceReservations: {reservations.Count} entries");
    }
}

