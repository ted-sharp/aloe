using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class OrganizationSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingOrganizations = await context.Organizations.AnyAsync();
        if (existingOrganizations)
        {
            Console.WriteLine("[SKIP] Organizations already exist.");
            return;
        }

        Console.WriteLine("[INFO] Creating organization seed data...");
        var tenant = await context.Tenants.FirstOrDefaultAsync();
        if (tenant != null)
        {
            var random = new Random();
            var organizations = new List<Organization>(50);

            for (int i = 1; i <= 50; i++)
            {
                // 団体名を生成
                var (name, nameKatakana, nameKatakanaCompat, nameDisplay, namePrint) = NameGenerator.GenerateOrganizationName(random);

                // 団体コードを生成
                var orgCode = $"ORG{i:D3}";

                // メモをランダムに選択
                var memo = NameGenerator.GetRandomOrganizationMemo(random);

                organizations.Add(new Organization
                {
                    OrgId = Guid.CreateVersion7(),
                    FacilityId = facilityId,
                    ParentOrgId = null,
                    OrgCode = orgCode,
                    OrgName = name,
                    OrgNameKatakana = nameKatakana,
                    OrgNameKatakanaCompat = nameKatakanaCompat,
                    OrgNameDisplay = nameDisplay,
                    OrgNamePrint = namePrint,
                    OrgMemo = memo
                });
            }

            foreach (var org in organizations)
            {
                org.IsDeleted = false;
                SeederHelper.InitializeAuditFields(org, dateTimeProvider);
            }

            context.Organizations.AddRange(organizations);
            Console.WriteLine($"  [+] Organizations: {organizations.Count} entries");
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}


