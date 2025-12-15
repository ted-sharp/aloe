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
        if (existingOrganizations)
        {
            Console.WriteLine("[SKIP] Organizations already exist.");
            return;
        }
        if (!facilityId.HasValue)
        {
            // TODO: 例外でよいかも
            Console.WriteLine("[SKIP] facilityId is null.");
            return;
        }

        Console.WriteLine("[INFO] Creating organization seed data...");
        var tenant = await context.Tenants.FirstOrDefaultAsync();
        if (tenant != null)
        {
            var organizations = new List<Organization>
            {
                new()
                {
                    OrgId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    ParentOrgId = null,
                    OrgCode = "ORG001",
                    OrgName = "株式会社アロエ商事",
                    OrgNameKatakana = "カブシキガイシャアロエショウジ",
                    OrgNameKatakanaCompat = "カブシキガイシャアロエショウジ",
                    OrgNameDisplay = "株式会社アロエ商事",
                    OrgNamePrint = "株式会社アロエ商事",
                    OrgMemo = "健診契約企業"
                },
                new()
                {
                    OrgId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    ParentOrgId = null,
                    OrgCode = "ORG002",
                    OrgName = "アロエ工業株式会社",
                    OrgNameKatakana = "アロエコウギョウカブシキガイシャ",
                    OrgNameKatakanaCompat = "アロエコウギョウカブシキガイシャ",
                    OrgNameDisplay = "アロエ工業株式会社",
                    OrgNamePrint = "アロエ工業株式会社",
                    OrgMemo = "健診契約企業"
                },
                new()
                {
                    OrgId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    ParentOrgId = null,
                    OrgCode = "ORG003",
                    OrgName = "アロエ建設株式会社",
                    OrgNameKatakana = "アロエケンセツカブシキガイシャ",
                    OrgNameKatakanaCompat = "アロエケンセツカブシキガイシャ",
                    OrgNameDisplay = "アロエ建設株式会社",
                    OrgNamePrint = "アロエ建設株式会社",
                    OrgMemo = "健診契約企業"
                },
                new()
                {
                    OrgId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    ParentOrgId = null,
                    OrgCode = "ORG004",
                    OrgName = "アロエサービス株式会社",
                    OrgNameKatakana = "アロエサービスカブシキガイシャ",
                    OrgNameKatakanaCompat = "アロエサービスカブシキガイシャ",
                    OrgNameDisplay = "アロエサービス株式会社",
                    OrgNamePrint = "アロエサービス株式会社",
                    OrgMemo = "健診契約企業"
                },
                new()
                {
                    OrgId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    ParentOrgId = null,
                    OrgCode = "ORG005",
                    OrgName = "株式会社アロエテクノ",
                    OrgNameKatakana = "カブシキガイシャアロエテクノ",
                    OrgNameKatakanaCompat = "カブシキガイシャアロエテクノ",
                    OrgNameDisplay = "株式会社アロエテクノ",
                    OrgNamePrint = "株式会社アロエテクノ",
                    OrgMemo = "健診契約企業"
                },
                new()
                {
                    OrgId = Guid.CreateVersion7(),
                    FacilityId = facilityId.Value,
                    ParentOrgId = null,
                    OrgCode = "ORG006",
                    OrgName = "アロエ物流株式会社",
                    OrgNameKatakana = "アロエブツリュウカブシキガイシャ",
                    OrgNameKatakanaCompat = "アロエブツリュウカブシキガイシャ",
                    OrgNameDisplay = "アロエ物流株式会社",
                    OrgNamePrint = "アロエ物流株式会社",
                    OrgMemo = "健診契約企業"
                }
            };

            foreach (var org in organizations)
            {
                org.IsDeleted = false;
                SeederHelper.InitializeAuditFields(org, dateTimeProvider);
            }

            context.Organizations.AddRange(organizations);
            Console.WriteLine($"  [+] Organizations: {organizations.Count} entries");
        }
    }
}


