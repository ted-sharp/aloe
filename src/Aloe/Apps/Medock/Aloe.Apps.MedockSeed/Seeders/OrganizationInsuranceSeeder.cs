using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class OrganizationInsuranceSeeder
{
    private static readonly Random _random = new Random();

    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingInsurances = await context.OrganizationInsurances.AnyAsync();
        if (existingInsurances)
        {
            Console.WriteLine("[SKIP] OrganizationInsurances already exist.");
            return;
        }

        var organizations = await context.Organizations
            .Where(o => !o.IsDeleted && o.FacilityId == facilityId)
            .ToListAsync();

        if (!organizations.Any())
        {
            Console.WriteLine("[SKIP] OrganizationInsurance: No organizations found.");
            return;
        }

        var insuranceProviders = await context.InsuranceProviders
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        if (!insuranceProviders.Any())
        {
            Console.WriteLine("[SKIP] OrganizationInsurance: No insurance providers found (no seed data).");
            return;
        }

        // TODO: DB修正後にデータ生成を有効化
        // データなしでスキップ
        Console.WriteLine("[SKIP] OrganizationInsurances: No seed data (skipped - TODO: enable after DB fix).");
        return;

        /* TODO: DB修正後にコメントアウトを解除
        Console.WriteLine("[INFO] Creating organization insurance seed data...");

        var insurances = new List<OrganizationInsurance>();

        foreach (var organization in organizations)
        {
            // 団体の50%に1件の保険情報を生成
            if (_random.Next(100) < 50)
            {
                var provider = insuranceProviders[_random.Next(insuranceProviders.Count)];

                var insurance = new OrganizationInsurance
                {
                    OrgInsuranceId = Guid.CreateVersion7(),
                    OrgId = organization.OrgId,
                    IsPrimary = true, // 団体保険は通常1件なので主保険
                    InsurerId = provider.InsurerId,
                    InsurerTypeCode = provider.InsurerTypeCode,
                    InsurerCode = provider.InsurerCode,
                    IsActive = true,
                    DeactivatedOn = null,
                    OrgInsuranceMemo = String.Empty,
                    OrgInsuranceSeq = 1,
                    IsDeleted = false
                };

                SeederHelper.InitializeAuditFields(insurance, dateTimeProvider);
                insurances.Add(insurance);
            }
        }

        context.OrganizationInsurances.AddRange(insurances);
        Console.WriteLine($"  [+] OrganizationInsurances: {insurances.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
        // TODO: DB修正後にコメントアウトを解除
        */
    }
}

