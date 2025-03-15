using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

internal partial class Seeder
{
    private async Task<int> SeedContractAsync(AppDbContext context)
    {
        try
        {
            var count = 0;

            if (!context.Contracts.Any())
            {
                var kk = context.InsuranceProviders
                    .AsNoTracking()
                    .FirstOrDefault(x => x.InsurProvNumber == "7010005013337");

                var kkCt = new Contract(
                    kk.InsurProvId,
                    0,
                    "",
                    "KK",
                    "協会けんぽ共通契約",
                    "{desc}");
                context.Contracts.Add(kkCt);

                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                // IDが更新されたので再取得
                kkCt = context.Contracts
                    .AsNoTracking()
                    .FirstOrDefault(x => x.InsurProvId == kk.InsurProvId);

                var orgs = await context.Organizations
                    .AsNoTracking()
                    .Take(100)
                    .ToArrayAsync();

                var cts = new List<Contract>(orgs.Length);
                foreach (var org in orgs)
                {
                    if (org.InsurProvId == kk.InsurProvId)
                    {
                        var parentCtCode = kkCt.CtCode;
                        var ct = new Contract(
                            org.InsurProvId,
                            org.OrgId,
                            parentCtCode,
                            org.OrgId.ToString(),
                            org.OrgName + "向け契約",
                            "協会けんぽ共通契約を使用");
                        cts.Add(ct);
                    }
                    else
                    {
                        var ct = new Contract(
                            org.InsurProvId,
                            org.OrgId,
                            "",
                            org.OrgId.ToString(),
                            org.OrgName + "向け契約",
                            "{desc}");
                        cts.Add(ct);
                    }
                }

                await context.BulkInsertAsync(orgs);
                count += orgs.Length;
            }

            if (!context.ContractPlans.Any())
            {
                var cts = await context.Contracts
                    .AsNoTracking()
                    .ToArrayAsync();

                foreach (var ct in cts)
                {
                    if (!String.IsNullOrWhiteSpace(ct.ParentCtCode))
                    {
                        // 親があるならスキップ
                        continue;
                    }

                    var plans = await context.CheckupPlans
                        .Where(x => x.InsurProvId == ct.InsurProvId && x.OrgId == ct.OrgId)
                        .AsNoTracking()
                        .Select(x => new ContractPlan(ct.CtId, x))
                        .ToArrayAsync();
                    if (plans.Length == 0)
                    {
                        // 基になるプランがないならスキップ
                        continue;
                    }

                    await context.BulkInsertAsync(plans);
                    count += plans.Length;
                }
            }

            if (!context.ContractOptions.Any())
            {
                var cts = await context.Contracts
                    .AsNoTracking()
                    .ToArrayAsync();

                foreach (var ct in cts)
                {
                    if (!String.IsNullOrWhiteSpace(ct.ParentCtCode))
                    {
                        // 親があるならスキップ
                        continue;
                    }

                    var options = await context.CheckupOptions
                        .Where(x => x.InsurProvId == ct.InsurProvId && x.OrgId == ct.OrgId)
                        .AsNoTracking()
                        .Select(x => new ContractOption(ct.CtId, x))
                        .ToArrayAsync();
                    if (options.Length == 0)
                    {
                        // 基になるプランがないならスキップ
                        continue;
                    }

                    await context.BulkInsertAsync(options);
                    count += options.Length;
                }
            }

            // TODO: prices, caps, cap_details

            return count;
        }
        catch
        {
            throw;
        }
    }

}
