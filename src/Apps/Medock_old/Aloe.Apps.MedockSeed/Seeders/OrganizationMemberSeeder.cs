using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class OrganizationMemberSeeder
{
    private static readonly Random _random = new Random();

    // 部署名のサンプル
    private static readonly string[] Departments =
    [
        "総務部", "人事部", "経理部", "営業部", "開発部",
        "製造部", "品質管理部", "研究開発部", "マーケティング部",
        "情報システム部", "法務部", "広報部", "購買部", "物流部"
    ];

    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingMembers = await context.OrganizationMembers.AnyAsync();
        if (existingMembers)
        {
            Console.WriteLine("[SKIP] OrganizationMembers already exist.");
            return;
        }

        var patients = await context.Patients
            .Where(p => !p.IsDeleted && p.FacilityId == facilityId)
            .ToListAsync();

        if (!patients.Any())
        {
            Console.WriteLine("[SKIP] OrganizationMember: No patients found.");
            return;
        }

        var organizations = await context.Organizations
            .Where(o => !o.IsDeleted && o.FacilityId == facilityId)
            .ToListAsync();

        if (!organizations.Any())
        {
            Console.WriteLine("[SKIP] OrganizationMember: No organizations found.");
            return;
        }

        Console.WriteLine("[INFO] Creating organization member seed data...");

        var members = new List<OrganizationMember>();
        var assignedPatientIds = new HashSet<Guid>();

        // 患者の60%を団体メンバーとして登録
        var targetCount = (int)(patients.Count * 0.6);
        var shuffledPatients = patients.OrderBy(_ => _random.Next()).Take(targetCount).ToList();

        foreach (var patient in shuffledPatients)
        {
            // ランダムに団体を選択
            var organization = organizations[_random.Next(organizations.Count)];

            // 個人コード（社員番号など）を生成
            var personalCode = $"EMP{_random.Next(1000, 9999):D4}";

            // 部署をランダムに選択（70%の確率で部署あり）
            var department = _random.Next(100) < 70
                ? Departments[_random.Next(Departments.Length)]
                : String.Empty;

            // 有効フラグ（90%が有効）
            var isActive = _random.Next(100) < 90;

            // 無効化日（無効な場合のみ設定）
            var deactivatedOn = !isActive && _random.Next(100) < 50
                ? dateTimeProvider.TodayDateOnly.AddDays(-_random.Next(1, 365))
                : (DateOnly?)null;

            var member = new OrganizationMember
            {
                OrgMemberId = Guid.CreateVersion7(),
                OrgId = organization.OrgId,
                PtId = patient.PtId,
                PersonalCode = personalCode,
                Department = department,
                IsActive = isActive,
                DeactivatedOn = deactivatedOn,
                OrgMemberMemo = String.Empty,
                IsDeleted = false
            };

            SeederHelper.InitializeAuditFields(member, dateTimeProvider);
            members.Add(member);
            assignedPatientIds.Add(patient.PtId);
        }

        context.OrganizationMembers.AddRange(members);
        Console.WriteLine($"  [+] OrganizationMembers: {members.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}

