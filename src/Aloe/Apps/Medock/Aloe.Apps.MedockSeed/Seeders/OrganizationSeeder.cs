using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class OrganizationSeeder
{
    public static async Task SeedAsync(MedockDbContext context, Guid? facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingOrganizations = await context.Organizations.AnyAsync();
        if (!existingOrganizations && facilityId.HasValue)
        {
            Console.WriteLine("[INFO] Creating organization seed data...");
            var tenant = await context.Tenants.FirstOrDefaultAsync();
            if (tenant != null)
            {
                var organizations = new List<Organization>
                {
                    new()
                    {
                        OrgId = Guid.NewGuid(),
                        FacilityId = facilityId.Value,
                        ParentOrgId = null,
                        OrgCode = "ORG001",
                        OrgName = "総合診療部",
                        OrgNameKatakana = "ソウゴウシンリョウブ",
                        OrgNameKatakanaCompat = "ソウゴウシンリョウブ",
                        OrgNameDisplay = "総合診療部",
                        OrgNamePrint = "総合診療部",
                        OrgMemo = "一般的な診療部門"
                    },
                    new()
                    {
                        OrgId = Guid.NewGuid(),
                        FacilityId = facilityId.Value,
                        ParentOrgId = null,
                        OrgCode = "ORG002",
                        OrgName = "内科",
                        OrgNameKatakana = "ナイカ",
                        OrgNameKatakanaCompat = "ナイカ",
                        OrgNameDisplay = "内科",
                        OrgNamePrint = "内科",
                        OrgMemo = "内科診療"
                    },
                    new()
                    {
                        OrgId = Guid.NewGuid(),
                        FacilityId = facilityId.Value,
                        ParentOrgId = null,
                        OrgCode = "ORG003",
                        OrgName = "外科",
                        OrgNameKatakana = "ゲカ",
                        OrgNameKatakanaCompat = "ゲカ",
                        OrgNameDisplay = "外科",
                        OrgNamePrint = "外科",
                        OrgMemo = "外科診療"
                    },
                    new()
                    {
                        OrgId = Guid.NewGuid(),
                        FacilityId = facilityId.Value,
                        ParentOrgId = null,
                        OrgCode = "ORG004",
                        OrgName = "放射線科",
                        OrgNameKatakana = "ホウシャセンカ",
                        OrgNameKatakanaCompat = "ホウシャセンカ",
                        OrgNameDisplay = "放射線科",
                        OrgNamePrint = "放射線科",
                        OrgMemo = "放射線診療"
                    }
                };

                foreach (var org in organizations)
                {
                    org.IsDeleted = false;
                    org.CreatedAt = dateTimeProvider.Now;
                    org.UpdatedAt = dateTimeProvider.Now;
                    org.CreatedUserId = Guid.Empty;
                    org.CreatedSessionId = Guid.Empty;
                    org.UpdatedUserId = Guid.Empty;
                    org.UpdatedSessionId = Guid.Empty;
                }

                context.Organizations.AddRange(organizations);
                Console.WriteLine($"  [+] Organizations: {organizations.Count} entries");
            }
        }
        else if (existingOrganizations)
        {
            Console.WriteLine("[SKIP] Organizations already exist.");
        }
    }
}


