using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class FeatureSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        Console.WriteLine("[INFO] Creating feature and operation master data...");

        // 既存のFeatureコードを取得
        var existingFeatureCodes = await context.Features
            .Select(f => f.FeatureCode)
            .ToListAsync();

        var features = new List<Feature>
        {
            new() { FeatureCode = "APPT" },
            new() { FeatureCode = "PATIENT" },
            new() { FeatureCode = "CALENDAR" },
            new() { FeatureCode = "USER" },
        };

        // 既に存在しないFeatureだけを追加
        var newFeatures = features
            .Where(f => !existingFeatureCodes.Contains(f.FeatureCode))
            .ToList();

        foreach (var feature in newFeatures)
        {
            feature.IsDeleted = false;
            feature.CreatedAt = dateTimeProvider.Now;
            feature.UpdatedAt = dateTimeProvider.Now;
            feature.CreatedUserId = Guid.Empty;
            feature.CreatedSessionId = Guid.Empty;
            feature.UpdatedUserId = Guid.Empty;
            feature.UpdatedSessionId = Guid.Empty;
        }

        if (newFeatures.Any())
        {
            context.Features.AddRange(newFeatures);
            Console.WriteLine($"  [+] Features: {newFeatures.Count} entries");
        }
        else
        {
            Console.WriteLine($"  [SKIP] Features: All features already exist.");
        }

        // 既存のOperationコードを取得
        var existingOperationCodes = await context.Operations
            .Select(o => o.OperationCode)
            .ToListAsync();

        var operations = new List<Operation>
        {
            new() { OperationCode = "CREATE" },
            new() { OperationCode = "READ" },
            new() { OperationCode = "UPDATE" },
            new() { OperationCode = "DELETE" },
            new() { OperationCode = "ADMIN" },
        };

        // 既に存在しないOperationだけを追加
        var newOperations = operations
            .Where(o => !existingOperationCodes.Contains(o.OperationCode))
            .ToList();

        foreach (var operation in newOperations)
        {
            operation.IsDeleted = false;
            operation.CreatedAt = dateTimeProvider.Now;
            operation.UpdatedAt = dateTimeProvider.Now;
            operation.CreatedUserId = Guid.Empty;
            operation.CreatedSessionId = Guid.Empty;
            operation.UpdatedUserId = Guid.Empty;
            operation.UpdatedSessionId = Guid.Empty;
        }

        if (newOperations.Any())
        {
            context.Operations.AddRange(newOperations);
            Console.WriteLine($"  [+] Operations: {newOperations.Count} entries");
        }
        else
        {
            Console.WriteLine($"  [SKIP] Operations: All operations already exist.");
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}
