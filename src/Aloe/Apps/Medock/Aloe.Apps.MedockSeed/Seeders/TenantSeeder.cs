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
        if (existingTenant != null)
        {
            Console.WriteLine("[SKIP] Tenant already exists.");
            return existingTenant.TenantId;
        }

        Console.WriteLine("[INFO] Creating tenant seed data...");
        var tenantId = Guid.CreateVersion7();
        var tenant = new Tenant
        {
            TenantId = tenantId,
            TenantName = "DEMO-テナント",
            IsActive = true,
            ActiveFrom = DateOnly.FromDateTime(dateTimeProvider.Today),
            IsDeleted = false
        };
        SeederHelper.InitializeAuditFields(tenant, dateTimeProvider);
        context.Tenants.Add(tenant);
        Console.WriteLine($"  [+] Tenant: {tenant.TenantName} ({tenant.TenantId})");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }

        return tenantId;
    }
}
