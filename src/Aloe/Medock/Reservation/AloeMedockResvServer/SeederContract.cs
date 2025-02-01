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
    private static async Task<int> SeedContractAsync(AppDbContext context)
    {
        try
        {
            var count = 0;

            if (!context.Contracts.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var kk = context.InsuranceProviders
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
                kkCt = context.Contracts.FirstOrDefault(x => x.InsurProvId == kk.InsurProvId);

                var orgs = context.Organizations.Take(100).ToList();

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
                        context.Contracts.Add(ct);
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
                        context.Contracts.Add(ct);
                    }
                }
            }

            if (!context.ContractPlans.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var cts = context.Contracts.ToList();

                foreach (var ct in cts)
                {
                    if (!String.IsNullOrWhiteSpace(ct.ParentCtCode))
                    {
                        // 親があるならスキップ
                        continue;
                    }

                    var plans = context.CheckupPlans
                        .Where(x => x.InsurProvId == ct.InsurProvId && x.OrgId == ct.OrgId)
                        .AsNoTracking()
                        .ToList();
                    if (plans.Count == 0)
                    {
                        // 基になるプランがないならスキップ
                        continue;
                    }

                    foreach (var plan in plans)
                    {
                        context.ContractPlans.Add(new(ct.CtId, plan));
                    }
                }
            }

            if (!context.ContractOptions.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var cts = context.Contracts.ToList();

                foreach (var ct in cts)
                {
                    if (!String.IsNullOrWhiteSpace(ct.ParentCtCode))
                    {
                        // 親があるならスキップ
                        continue;
                    }

                    var plans = context.CheckupOptions
                        .Where(x => x.InsurProvId == ct.InsurProvId && x.OrgId == ct.OrgId)
                        .AsNoTracking()
                        .ToList();
                    if (plans.Count == 0)
                    {
                        // 基になるプランがないならスキップ
                        continue;
                    }

                    foreach (var plan in plans)
                    {
                        context.ContractOptions.Add(new(ct.CtId, plan));
                    }
                }
            }

            // TODO: prices, caps, cap_details

            count += await context.SaveChangesAsync();

            return count;
        }
        catch
        {
            throw;
        }
    }

}
