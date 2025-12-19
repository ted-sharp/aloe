using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class FacilityAddressSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingAddresses = await context.FacilityAddresses.AnyAsync();
        if (existingAddresses)
        {
            Console.WriteLine("[SKIP] FacilityAddresses already exist.");
            return;
        }

        var facility = await context.Facilities
            .FirstOrDefaultAsync(f => f.FacilityId == facilityId);

        if (facility == null)
        {
            Console.WriteLine("[SKIP] FacilityAddress: Facility not found.");
            return;
        }

        Console.WriteLine("[INFO] Creating facility address seed data...");

        var addresses = new List<FacilityAddress>
        {
            // 本社住所
            new()
            {
                FacilityAdrId = Guid.CreateVersion7(),
                FacilityId = facilityId,
                AdrTypeCode = 1, // 1=本社
                PostalCode = "1000001",
                Adr1 = "東京都千代田区千代田",
                Adr2 = "1-1",
                Adr3 = "DEMO健診センター",
                AttentionName = String.Empty,
                Tel = "03-1234-5678",
                Tel2 = String.Empty,
                Fax = "03-1234-5679",
                Email = "info@demo-medock.example.com",
                AdrMemo = "本社所在地",
                AdrSeq = 1,
                IsDeleted = false
            }
        };

        foreach (var address in addresses)
        {
            SeederHelper.InitializeAuditFields(address, dateTimeProvider);
        }

        context.FacilityAddresses.AddRange(addresses);
        Console.WriteLine($"  [+] FacilityAddresses: {addresses.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}

