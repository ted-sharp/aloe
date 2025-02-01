using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

internal partial class Seeder
{
    private static async Task<int> SeedPlanAsync(AppDbContext context)
    {
        try
        {
            var count = 0;

            if (!context.CheckupPlanCategories.AsNoTracking().Any())
            {
                context.CheckupPlanCategories.AddRange([
                    new("宿泊ドック", "2dock", "泊まりで宿泊券が出ます。"),
                    new("日帰りドック", "1dock", "予約はあさイチでいれます。"),
                    new("特定健診", "特定", "40才以上で必ずうけるやつ。"),
                    new("一般健診", "一般", "年一回受けるやつ。"),
                    new("特殊健診", "特殊", "危険物とかのやつ。"),
                    new("その他", "その他", "その他"),
                ]);
            }

            // 挿入したデータを使うため保存する
            count += await context.SaveChangesAsync();

            var orgs = context.Organizations.Take(100).ToList();

            var kk = context.InsuranceProviders
                .First(x => x.InsurProvNumber == "7010005013337");
            var sp = context.CheckupPlanCategories
                .First(x => x.PlanCatShortName == "特定");

            if (!context.CheckupPlans.AsNoTracking().Any())
            {
                var normal = context.CheckupPlanCategories.First(x => x.PlanCatShortName == "一般");

                var plans = orgs.Select(x => new CheckupPlan(
                    normal.PlanCatId,
                    x.InsurProvId,
                    x.OrgId,
                    "",
                    x.OrgName + "向けオプション",
                    x.OrgNameDisplay + "向け",
                    "{abbr}",
                    "{desc}"));
                context.CheckupPlans.AddRange(plans);

                // 協会けんぽ向けプラン
                context.CheckupPlans.AddRange([
                    new(normal.PlanCatId, 0, 0, "N001", "一般プラン1", "一般1", "般1", "{desc}"),
                    new(normal.PlanCatId, 0, 0, "N002", "一般プラン2", "一般2", "般2", "{desc}"),
                    new(sp.PlanCatId, kk.InsurProvId, 0, "KK001", "協会けんぽ1", "協会1", "協1", "協会けんぽ共通プラン1"),
                    new(sp.PlanCatId, kk.InsurProvId, 0, "KK002", "協会けんぽ2", "協会2", "協2", "協会けんぽ共通プラン2"),
                ]);
            }

            if (!context.CheckupOptions.AsNoTracking().Any())
            {
                var options = orgs.Select(x => new CheckupOption(
                    x.InsurProvId,
                    x.OrgId,
                    "",
                    x.OrgName + "向けオプション",
                    x.OrgNameDisplay + "向け",
                    "{abbr}",
                    "{desc}"));
                context.CheckupOptions.AddRange(options);

                context.CheckupOptions.AddRange([
                    new(0, 0, "OP001", "オプション1", "オプ1", "OP1", "{desc}"),
                    new(0, 0, "OP002", "オプション2", "オプ2", "OP2", "{desc}"),
                    new(kk.InsurProvId, 0, "KK-OP001", "オプション1(KK)", "オプ1(KK)", "KK-OP1", "{desc}"),
                    new(kk.InsurProvId, 0, "KK-OP002", "オプション2(KK)", "オプ2(KK)", "KK-OP2", "{desc}"),
                ]);
            }

            count += await context.SaveChangesAsync();

            return count;
        }
        catch
        {
            throw;
        }
    }

}
