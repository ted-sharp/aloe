using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class ResourceSeeder
{
    public static async Task SeedAsync(MedockDbContext context)
    {
        var existingResources = await context.Resources.AnyAsync();
        if (!existingResources)
        {
            Console.WriteLine("[INFO] Creating resource and operation master data...");

            var resources = new List<Resource>
            {
                new() { ResourceCode = "APPT" },
                new() { ResourceCode = "PATIENT" },
                new() { ResourceCode = "CALENDAR" },
                new() { ResourceCode = "USER" },
            };

            foreach (var resource in resources)
            {
                resource.IsDeleted = false;
                resource.CreatedAt = DateTimeOffset.UtcNow;
                resource.UpdatedAt = DateTimeOffset.UtcNow;
                resource.CreatedUserId = Guid.Empty;
                resource.CreatedSessionId = Guid.Empty;
                resource.UpdatedUserId = Guid.Empty;
                resource.UpdatedSessionId = Guid.Empty;
            }

            context.Resources.AddRange(resources);
            Console.WriteLine($"  [+] Resources: {resources.Count} entries");

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
                operation.CreatedAt = DateTimeOffset.UtcNow;
                operation.UpdatedAt = DateTimeOffset.UtcNow;
                operation.CreatedUserId = Guid.Empty;
                operation.CreatedSessionId = Guid.Empty;
                operation.UpdatedUserId = Guid.Empty;
                operation.UpdatedSessionId = Guid.Empty;
            }

            context.Operations.AddRange(operations);
            Console.WriteLine($"  [+] Operations: {operations.Count} entries");
        }
        else
        {
            Console.WriteLine("[SKIP] Resources and Operations already exist.");
        }
    }
}

