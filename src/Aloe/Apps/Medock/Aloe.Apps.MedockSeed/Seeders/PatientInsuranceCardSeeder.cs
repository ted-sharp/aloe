using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class PatientInsuranceCardSeeder
{
    private static readonly Random _random = new Random();

    public static async Task SeedAsync(MedockDbContext context, Guid facilityId, IDateTimeProvider dateTimeProvider)
    {
        var existingCards = await context.PatientInsuranceCards.AnyAsync();
        if (existingCards)
        {
            Console.WriteLine("[SKIP] PatientInsuranceCards already exist.");
            return;
        }

        var patients = await context.Patients
            .Where(p => !p.IsDeleted && p.FacilityId == facilityId)
            .ToListAsync();

        if (!patients.Any())
        {
            Console.WriteLine("[SKIP] PatientInsuranceCard: No patients found.");
            return;
        }

        var insuranceProviders = await context.InsuranceProviders
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        if (!insuranceProviders.Any())
        {
            Console.WriteLine("[SKIP] PatientInsuranceCard: No insurance providers found (no seed data).");
            return;
        }

        // TODO: DB修正後にデータ生成を有効化
        // データなしでスキップ
        Console.WriteLine("[SKIP] PatientInsuranceCards: No seed data (skipped - TODO: enable after DB fix).");
        return;

        /* TODO: DB修正後にコメントアウトを解除
        Console.WriteLine("[INFO] Creating patient insurance card seed data...");

        var cards = new List<PatientInsuranceCard>();

        foreach (var patient in patients)
        {
            // 70%の患者に1件、20%の患者に2件、10%は0件
            var cardCount = _random.Next(100) switch
            {
                < 70 => 1,
                < 90 => 2,
                _ => 0
            };

            for (int i = 0; i < cardCount; i++)
            {
                var provider = insuranceProviders[_random.Next(insuranceProviders.Count)];

                // 被保険者番号を生成
                var symbol = $"{_random.Next(1, 10):D2}";
                var number = $"{_random.Next(100000, 999999)}";
                var branchNumber = _random.Next(100) < 80 ? "" : $"{_random.Next(1, 10)}";
                var insuredCode = branchNumber == "" ? $"{symbol}-{number}" : $"{symbol}-{number}-{branchNumber}";

                // 本人・家族区分（80%が本人、20%が家族）
                var selfFamilyCode = _random.Next(100) < 80 ? "1" : "2";

                // 負担割合（90%が1割、10%が0割）
                var assistanceCode = _random.Next(100) < 90 ? "A" : "0";

                // 継続区分（95%がなし、5%が継続）
                var continuationCode = _random.Next(100) < 95 ? "0" : (_random.Next(2) == 0 ? "1" : "2");

                var card = new PatientInsuranceCard
                {
                    PtInsurCardId = Guid.CreateVersion7(),
                    PtId = patient.PtId,
                    IsPrimary = i == 0, // 最初のカードが主保険
                    InsurerId = provider.InsurerId,
                    InsurerTypeCode = provider.InsurerTypeCode,
                    InsurerCode = provider.InsurerCode,
                    InsurerName = provider.InsurerName,
                    InsuredCode = insuredCode,
                    InsuredCodeSymbol = symbol,
                    InsuredCodeNumber = number,
                    InsuredCodeBranchNumber = branchNumber,
                    InsuredPersonName = patient.PtName,
                    SelfFamilyRelationshipCode = selfFamilyCode,
                    AssistanceCode = assistanceCode,
                    ContinuationCode = continuationCode,
                    IsActive = true,
                    DeactivatedOn = null,
                    PtInsureCardMemo = String.Empty,
                    PtInsureCardSeq = i + 1,
                    IsDeleted = false
                };

                SeederHelper.InitializeAuditFields(card, dateTimeProvider);
                cards.Add(card);
            }
        }

        context.PatientInsuranceCards.AddRange(cards);
        Console.WriteLine($"  [+] PatientInsuranceCards: {cards.Count} entries");

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
        // TODO: DB修正後にコメントアウトを解除
        */
    }
}

