using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class FeatureSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var existingFeatures = await context.Features.AnyAsync();
        if (existingFeatures)
        {
            Console.WriteLine("[SKIP] Resources and Operations already exist.");
        }
        Console.WriteLine("[INFO] Creating feature and operation master data...");

        var features = new List<Feature>
        {
            new() { FeatureCode = "APPT" },
            new() { FeatureCode = "PATIENT" },
            new() { FeatureCode = "CALENDAR" },
            new() { FeatureCode = "USER" },
        };

        foreach (var feature in features)
        {
            feature.IsDeleted = false;
            feature.CreatedAt = dateTimeProvider.Now;
            feature.UpdatedAt = dateTimeProvider.Now;
            feature.CreatedUserId = Guid.Empty;
            feature.CreatedSessionId = Guid.Empty;
            feature.UpdatedUserId = Guid.Empty;
            feature.UpdatedSessionId = Guid.Empty;
        }

        context.Features.AddRange(features);
        Console.WriteLine($"  [+] Features: {features.Count} entries");

        var operations = new List<Operation>
        {
            new() { OperationCode = "CREATE" },
            new() { OperationCode = "READ" },
            new() { OperationCode = "UPDATE" },
            new() { OperationCode = "DELETE" },
            new() { OperationCode = "ADMIN" },
        };

        foreach (var operation in operations)
        {
            operation.IsDeleted = false;
            operation.CreatedAt = dateTimeProvider.Now;
            operation.UpdatedAt = dateTimeProvider.Now;
            operation.CreatedUserId = Guid.Empty;
            operation.CreatedSessionId = Guid.Empty;
            operation.UpdatedUserId = Guid.Empty;
            operation.UpdatedSessionId = Guid.Empty;
        }

        context.Operations.AddRange(operations);
        Console.WriteLine($"  [+] Operations: {operations.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}
