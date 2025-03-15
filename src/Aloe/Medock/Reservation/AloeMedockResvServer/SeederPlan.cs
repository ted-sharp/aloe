using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

internal partial class Seeder
{
    private async Task<int> SeedPlanAsync(AppDbContext context)
    {
        try
        {
            var count = 0;

            if (!context.CheckupPlanCategories.Any())
            {
                var cats = new CheckupPlanCategory[]
                {
                    new("宿泊ドック", "2dock", "泊まりで宿泊券が出ます。"),
                    new("日帰りドック", "1dock", "予約はあさイチでいれます。"),
                    new("特定健診", "特定", "40才以上で必ずうけるやつ。"),
                    new("一般健診", "一般", "年一回受けるやつ。"),
                    new("特殊健診", "特殊", "危険物とかのやつ。"),
                    new("その他", "その他", "その他"),
                };

                await context.BulkInsertAsync(cats);
                count += cats.Length;
            }

            var orgs = context.Organizations
                .AsNoTracking()
                .Take(100)
                .ToArray();

            // kk: 協会けんぽ
            var kk = context.InsuranceProviders
                .AsNoTracking()
                .First(x => x.InsurProvNumber == "7010005013337");

            var special = context.CheckupPlanCategories
                .AsNoTracking()
                .First(x => x.PlanCatShortName == "特定");

            var normal = context.CheckupPlanCategories
                .AsNoTracking()
                .First(x => x.PlanCatShortName == "一般");

            if (!context.CheckupPlans.Any())
            {
                var plans = orgs
                    .Select(x => new CheckupPlan(
                        normal.PlanCatId,
                        x.InsurProvId,
                        x.OrgId,
                        "",
                        x.OrgName + "向けプラン",
                        x.OrgNameDisplay + "向け",
                        "{abbr}",
                        "{desc}"))
                    .ToArray();

                var bulkConfig = new BulkConfig
                {
                    // DateOnly 型は除外
                    PropertiesToExclude = [
                        nameof(CheckupPlan.StartDate),
                        nameof(CheckupPlan.EndDate),
                    ],
                };

                await context.BulkInsertAsync(plans, bulkConfig);
                count += plans.Length;

                // 協会けんぽ向けプラン
                var kkPlans = new CheckupPlan[]
                {
                    new(normal.PlanCatId, 0, 0, "N001", "一般プラン1", "一般1", "般1", "{desc}"),
                    new(normal.PlanCatId, 0, 0, "N002", "一般プラン2", "一般2", "般2", "{desc}"),
                    new(special.PlanCatId, kk.InsurProvId, 0, "KK001", "協会けんぽ1", "協会1", "協1", "協会けんぽ共通プラン1"),
                    new(special.PlanCatId, kk.InsurProvId, 0, "KK002", "協会けんぽ2", "協会2", "協2", "協会けんぽ共通プラン2"),
                };

                await context.BulkInsertAsync(kkPlans, bulkConfig);
                count += kkPlans.Length;
            }

            if (!context.CheckupOptions.Any())
            {
                var options = orgs
                    .Select(x => new CheckupOption(
                        x.InsurProvId,
                        x.OrgId,
                        "",
                        x.OrgName + "向けオプション",
                        x.OrgNameDisplay + "向け",
                        "{abbr}",
                        "{desc}"))
                    .ToArray();

                var bulkConfig = new BulkConfig
                {
                    // DateOnly 型は除外
                    PropertiesToExclude = [
                        nameof(CheckupOption.StartDate),
                        nameof(CheckupOption.EndDate),
                    ],
                };

                await context.BulkInsertAsync(options, bulkConfig);
                count += options.Length;

                // 協会けんぽ向けオプション
                var kkOptions = new CheckupOption[]
                {
                    new(0, 0, "OP001", "オプション1", "オプ1", "OP1", "{desc}"),
                    new(0, 0, "OP002", "オプション2", "オプ2", "OP2", "{desc}"),
                    new(kk.InsurProvId, 0, "KK-OP001", "オプション1(KK)", "オプ1(KK)", "KK-OP1", "{desc}"),
                    new(kk.InsurProvId, 0, "KK-OP002", "オプション2(KK)", "オプ2(KK)", "KK-OP2", "{desc}"),
                };

                await context.BulkInsertAsync(kkOptions, bulkConfig);
                count += kkOptions.Length;
            }

            return count;
        }
        catch
        {
            throw;
        }
    }

}
