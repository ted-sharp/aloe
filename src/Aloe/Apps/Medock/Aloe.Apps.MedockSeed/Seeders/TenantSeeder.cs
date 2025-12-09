using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class TenantSeeder
{
    public static async Task<Guid> SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var existingTenant = await context.Tenants.FirstOrDefaultAsync();
        Guid tenantId;

        if (existingTenant == null)
        {
            Console.WriteLine("[INFO] Creating tenant seed data...");
            tenantId = Guid.NewGuid();
            var tenant = new Tenant
            {
                TenantId = tenantId,
                TenantName = "デモテナント",
                IsActive = true,
                ActiveFrom = DateOnly.FromDateTime(dateTimeProvider.Today),
                IsDeleted = false,
                CreatedAt = dateTimeProvider.Now,
                UpdatedAt = dateTimeProvider.Now,
                CreatedUserId = Guid.Empty,
                CreatedSessionId = Guid.Empty,
                UpdatedUserId = Guid.Empty,
                UpdatedSessionId = Guid.Empty
            };
            context.Tenants.Add(tenant);
            Console.WriteLine($"  [+] Tenant: {tenant.TenantName} ({tenant.TenantId})");
        }
        else
        {
            tenantId = existingTenant.TenantId;
            Console.WriteLine("[SKIP] Tenant already exists.");
        }

        return tenantId;
    }
}


