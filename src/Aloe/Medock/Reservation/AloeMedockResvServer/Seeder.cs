using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

internal class Seeder
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public Seeder(
        IDbContextFactory<AppDbContext> factory)
    {
        this._factory = factory;
    }

    /// <summary>
    /// 必要なサンプルデータを作成します。
    /// すでにデータが存在する場合は何もしません。
    /// </summary>
    internal async ValueTask InsertDataAsync()
    {
        try
        {
            Console.WriteLine("Seeding...");
            await using var context = await this._factory.CreateDbContextAsync();
            var count = await Seeder.SeedAsync(context);
            Console.WriteLine($"{nameof(Seeder)}.{nameof(this.InsertDataAsync)}() Inserted: {count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{nameof(Seeder)}.{nameof(this.InsertDataAsync)}() Error: {ex.Message}");
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// サンプルデータ挿入用のメソッドです。
    /// </summary>
    private static async Task<int> SeedAsync(AppDbContext context)
    {
        IDbContextTransaction? trans = null;

        var count = 0;

        var rnd = new Random();

        try
        {
            var slots = new[] {
                "08:30", "08:30", "08:30", "08:30",
                "09:00", "09:00", "09:00", "09:00",
                "09:30", "09:30", "09:30", "09:30",
                "10:00", "10:00", "10:00",
                "10:30", "10:30", "10:30",
                "11:00", "11:00", "11:00",
                "11:30", "11:30", "11:30",
                "12:00", "12:00",
                "13:30", "13:30", "13:30",
                "14:00", "14:00", "14:00",
                "14:30", "14:30", "14:30",
                "15:00", "15:00", "15:00",
                "15:30", "15:30", "15:30",
                "16:00", "16:00", "16:00",
                "16:30", "16:30", "16:30",
                "17:00", "17:00",
                "AM", "AM", "AM", "AM", "AM",
                "PM", "PM", "PM", "PM", "PM",
                "EX", "EX", "EX", "EX",
            };

            trans = await context.Database.BeginTransactionAsync();

            #region ユーザー関連

            if (!context.Users.AsNoTracking().Any())
            {
                context.Users.AddRange([
                    new("Administrator", "admin", "admin@example.com", "admin"),
                    new("Manager", "mgr", "mgr@example.com", "mgr"),
                    new("User", "usr", "user@example.com", "usr"),
                    new("User 1", "1", "user@example.com", "1"),
                    new("User 2", "2", "user@example.com", "2"),
                    new("User 3", "3", "user@example.com", "3"),
                    new("User 4", "4", "user@example.com", "4"),
                    new("User 5", "5", "user@example.com", "5"),
                    new("User 6", "6", "user@example.com", "6"),
                    new("User 7", "7", "user@example.com", "7"),
                    new("User 8", "8", "user@example.com", "8"),
                    new("User 9", "9", "user@example.com", "9"),
                    new("Guest", "guest", "user@example.com", "guest"),
                ]);
            }

            if (!context.Roles.AsNoTracking().Any())
            {
                context.Roles.AddRange([
                    new("Administrator", "管理者です。"),
                    new("Manager", "マネージャーです。(ポリシーを除く)"),
                    new("User", "ユーザーです。(閲覧登録)"),
                    new("Guest", "ゲストです。(閲覧のみ)"),
                ]);
            }

            if (!context.Permissions.AsNoTracking().Any())
            {
                context.Permissions.AddRange([
                    new("Maint_Policies_R", "ポリシーマスタ表示権限です。"),
                    new("Maint_Policies_W", "ポリシーマスタ変更権限です。"),
                    new("Maint_Users_R", "ユーザーマスタ表示権限です。"),
                    new("Maint_Users_W", "ユーザーマスタ変更権限です。"),
                    new("Maint_Others_R", "その他マスタ表示権限です。"),
                    new("Maint_Others_W", "その他マスタ変更権限です。"),
                    new("Resv_Slot_R", "スロットマスタ表示権限です。"),
                    new("Resv_Slot_W", "スロットマスタ表示権限です。"),
                    new("Resv_Others_R", "予約機能表示権限です。"),
                    new("Resv_Others_W", "予約機能表示権限です。"),
                ]);
            }

            if (!context.UserRoles.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var adminUser = context.Users.First(x => x.LoginName == "admin");
                var adminRole = context.Roles.First(x => x.RoleName == "Administrator");
                var mgrUser = context.Users.First(x => x.LoginName == "mgr");
                var mgrRole = context.Roles.First(x => x.RoleName == "Manager");
                var usrUser = context.Users.First(x => x.LoginName == "usr");
                var usrRole = context.Roles.First(x => x.RoleName == "User");
                var guestUser = context.Users.First(x => x.LoginName == "guest");
                var guestRole = context.Roles.First(x => x.RoleName == "Guest");
                context.UserRoles.AddRange([
                    new(adminUser.UserId, adminRole.RoleId),
                    new(mgrUser.UserId, mgrRole.RoleId),
                    new(usrUser.UserId, usrRole.RoleId),
                    new(guestUser.UserId, guestRole.RoleId),
                ]);
            }

            if (!context.RolePermissions.AsNoTracking().Any())
            {
                //var adminRoll = context.Rolls.First(x => x.RollName == "Administrator");
                //var mgrRoll = context.Rolls.First(x => x.RollName == "Manager");
                //var usrRoll = context.Rolls.First(x => x.RollName == "User");
                //var guestRoll = context.Rolls.First(x => x.RollName == "Guest");
                //context.RollPermissions.AddRange([
                //    new(adminUser.UserId, adminRoll.RollId),
                //    new(mgrUser.UserId, mgrRoll.RollId),
                //    new(usrUser.UserId, usrRoll.RollId),
                //    new(guestUser.UserId, guestRoll.RollId),
                //]);
            }

            if (!context.Policies.AsNoTracking().Any())
            {
                var policies = PolicyService.CreateDefaultPolicies();
                context.Policies.AddRange(policies.Values);
            }

            if (!context.Preferences.AsNoTracking().Any())
            {
                var preferences = PreferenceService.CreateDefaultPreferences();
                context.Preferences.AddRange(preferences.Values);
            }

            if (!context.UserPreferences.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var adminUser = context.Users.First(x => x.LoginName == "admin");

                context.UserPreferences.AddRange([
                    new (adminUser.UserId, PreferenceCode.WindowRememberPosition, "", true),
                ]);
            }

            #endregion ユーザー関連

            #region 団体患者関連

            if (!context.InsuranceProviders.AsNoTracking().Any())
            {
                context.InsuranceProviders.AddRange([
                    // 協会けんぽ
                    new((int)InsuranceProviderType.KyokaiKenpo, "7010005013337", "協会けんぽ", ""),
                    // 代行機関
                    new((int)InsuranceProviderType.DelegateAgency, "91399048", "バリューＨＲ", ""),
                    new((int)InsuranceProviderType.DelegateAgency, "91399055", "ホームネット", ""),
                    new((int)InsuranceProviderType.DelegateAgency, "91399063", "イーウェル", ""),
                    new((int)InsuranceProviderType.DelegateAgency, "91399097", "ＪＴＢベネフィット", ""),
                    new((int)InsuranceProviderType.DelegateAgency, "91499103", "バイオコミュニケーションズ", ""),
                    new((int)InsuranceProviderType.DelegateAgency, "91399113", "ベネフィット・ワン", ""),
                    new((int)InsuranceProviderType.DelegateAgency, "91399154", "ニッセイコム", ""),
                    new((int)InsuranceProviderType.DelegateAgency, "91399170", "ヘルスケアトータルソリューションズ", ""),
                    new((int)InsuranceProviderType.DelegateAgency, "91399212", "フィッツプラス", ""),
                    new((int)InsuranceProviderType.DelegateAgency, "92399229", "グッドライフデザイン", ""),
                    new((int)InsuranceProviderType.DelegateAgency, "91399246", "ウィーメックス", ""),
                    new((int)InsuranceProviderType.DelegateAgency, "91499293", "みるたす", ""),
                    // 健康保険組合
                    new((int)InsuranceProviderType.HealthInsuranceSociety, "1700150032168", "北海道コンピュ－タ関連産業健康保険組合", ""),
                    new((int)InsuranceProviderType.HealthInsuranceSociety, "5700150032049", "エア・ウォーター健康保険組合", ""),
                    new((int)InsuranceProviderType.HealthInsuranceSociety, "4700150031927", "北海道医療健康保険組合", ""),
                    // 国保
                    new((int)InsuranceProviderType.NationalHealthInsurance, "90199027", "北海道国民健康保険団体連合会", ""),
                ]);
            }

            if (!context.Organizations.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var insurances = context.InsuranceProviders.ToList();
                var insuranceMax = insurances.Count;

                for (var i = 0; i < 1000; i++)
                {
                    var insurance = insurances.Skip(rnd.Next(0, insuranceMax)).First();
                    var insurProvType = insurance.InsurProvTypeCode;
                    var insurProvId = insurance.InsurProvId;
                    var insurProvName = insurance.InsurProvName;
                    var org = new Organization(insurProvType, insurProvId, $"団体{i} ({insurProvName} 使用)", $"ダンタイ{i}", $"団体{i}(表示)", $"団体{i} 御中");
                    context.Organizations.Add(org);
                }
            }

            if (!context.OrganizationContacts.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var orgs = context.Organizations.ToList();
                foreach (var org in orgs)
                {
                    var ctc = new OrganizationContact(org.Id, "本社", "000-0000", "北海道札幌市中央区", "(代表) 011-000-0000", "");
                    context.OrganizationContacts.Add(ctc);
                }
            }

            if (!context.Patients.AsNoTracking().Any())
            {
                var sexes = Enum.GetValues<SexCode>();
                var sexMax = sexes.Length;

                var ptMax = 1000;
                var names = GenerateSampleNames(ptMax);

                for (var i = 0; i < ptMax; i++)
                {
                    var karteNumber = rnd.Next(1, Int32.MaxValue).ToString("000000000");
                    var birthDate = DateTime.Today.AddDays(-rnd.Next(3650, 365000));
                    var sex = sexes.Skip(rnd.Next(0, sexMax)).First();
                    var pt = new Patient(karteNumber, names[i].Kanji, names[i].Kana, birthDate, (int)sex);
                    context.Patients.AddRange(pt);
                }
            }

            if (!context.PatientContacts.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var pts = context.Patients.ToList();
                foreach (var pt in pts)
                {
                    var ctc = new PatientContact(pt.PtId, "自宅", "000-0000", "北海道札幌市中央区", "(携帯) 090-000-0000", "");
                    context.PatientContacts.Add(ctc);
                }
            }

            if (!context.OrganizationPatients.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var orgs = context.Organizations.ToList();
                var pts = context.Patients.ToList();
                var ptMax = pts.Count;

                foreach (var org in orgs)
                {
                    for (var i = 0; i < 20; i++)
                    {
                        var pt = pts.Skip(rnd.Next(0, ptMax)).First();
                        var orgPt = new OrganizationPatient(org.OrgId, pt.PtId, i.ToString(), "{部署}", true);
                        context.OrganizationPatients.AddRange(orgPt);
                    }
                }
            }

            // TODO: remarks, insurance_cards もいれたい

            #endregion 団体患者関連

            #region プラン関連

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

            if (!context.CheckupPlans.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var orgs = context.Organizations.Take(100).ToList();

                var normal = context.CheckupPlanCategories.First(x => x.PlanCatShortName == "一般");
                var sp = context.CheckupPlanCategories.First(x => x.PlanCatShortName == "特定");

                var kk = context.InsuranceProviders.FirstOrDefault(x => x.InsurProvNumber == "7010005013337");

                foreach (var org in orgs)
                {
                    var plan = new CheckupPlan(
                        normal.PlanCatId,
                        org.InsurProvId,
                        org.OrgId,
                        "",
                        org.OrgName + "向けプラン",
                        org.OrgNameDisplay + "向け",
                        "{abbr}",
                        "{desc}");
                    context.CheckupPlans.Add(plan);
                }

                context.CheckupPlans.AddRange([
                    new(normal.PlanCatId, 0, 0, "N001", "一般プラン1", "一般1", "般1", "{desc}"),
                    new(normal.PlanCatId, 0, 0, "N002", "一般プラン2", "一般2", "般2", "{desc}"),
                    new(sp.PlanCatId, kk.InsurProvId, 0, "KK001", "協会けんぽ1", "協会1", "協1", "協会けんぽ共通プラン1"),
                    new(sp.PlanCatId, kk.InsurProvId, 0, "KK002", "協会けんぽ2", "協会2", "協2", "協会けんぽ共通プラン2"),
                ]);
            }

            if (!context.CheckupOptions.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var orgs = context.Organizations.Take(100).ToList();

                var kk = context.InsuranceProviders
                    .FirstOrDefault(x => x.InsurProvNumber == "7010005013337");

                foreach (var org in orgs)
                {
                    var plan = new CheckupOption(
                        org.InsurProvId,
                        org.OrgId,
                        "",
                        org.OrgName + "向けオプション",
                        org.OrgNameDisplay + "向け",
                        "{abbr}",
                        "{desc}");
                    context.CheckupOptions.Add(plan);
                }

                context.CheckupOptions.AddRange([
                    new(0, 0, "OP001", "オプション1", "オプ1", "OP1", "{desc}"),
                    new(0, 0, "OP002", "オプション2", "オプ2", "OP2", "{desc}"),
                    new(kk.InsurProvId, 0, "KK-OP001", "オプション1(KK)", "オプ1(KK)", "KK-OP1", "{desc}"),
                    new(kk.InsurProvId, 0, "KK-OP002", "オプション2(KK)", "オプ2(KK)", "KK-OP2", "{desc}"),
                ]);
            }

            #endregion プラン関連

            #region 契約関連

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

            #endregion 契約関連

            #region 予約関連

            if (!context.Equipments.AsNoTracking().Any())
            {
                context.Equipments.AddRange([
                    new("胃カメラ", "院内の胃カメラです。", 1),
                    new("胃カメラ(外)", "外部に委託している胃カメラです。", 2),
                    new("CT", "CT", 3),
                    new("MRI", "MRI", 4),
                    new("大腸カメラ", "大腸カメラ", 5),
                    new("頸動脈エコー", "頸動脈エコー", 6),
                ]);
            }

            if (!context.EquipmentSlots.AsNoTracking().Any())
            {
                context.EquipmentSlots.AddRange([
                    new("1900/1/1".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.None, slots),
                    new("1900/1/2".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Sunday, ""),
                    new("1900/1/3".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Saturday, ""),
                ]);
            }

            if (!context.Floors.AsNoTracking().Any())
            {
                context.Floors.AddRange([
                    new("8階", "メインフロアです。", 1),
                    new("7階(♀)", "レディースフロアです。", 2),
                    new("巡回", "バス健診用です。", 3),
                    new("ダミー", "ダミーです。", 9),
                ]);
            }

            if (!context.Rooms.AsNoTracking().Any())
            {
                context.Rooms.AddRange([
                    new("子宮細胞診", "子宮細胞診です。", 1),
                    new("婦人科超音波", "婦人科超音波です。", 2),
                    new("マンモ", "マンモグラフィーです。", 3),
                    new("乳腺エコー", "乳腺エコーです。", 4),
                    new("腹部エコー", "腹部エコーです。", 5),
                    new("胃バリウム", "胃バリウムです。", 6),
                    new("VDT", "VDTです。", 7),
                    new("CT", "CTです。", 8),
                    new("MRI", "MRIです。", 9),
                    new("心電図", "心電図です。", 10),
                    new("肺機能", "肺機能です。", 11),
                    new("眼底", "眼底です。", 12),
                    new("ABI", "血管年齢です。", 13),
                    new("乳腺+腹部エコー", "乳腺エコーと腹部エコーです。", 14),
                    new("胃カメラ", "胃カメラです。", 15),
                ]);
            }

            if (!context.DailySlots.AsNoTracking().Any())
            {
                context.DailySlots.AddRange([
                    new("1900/1/1".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.None, 40, 40, "09:00 09:30 10:00 10:30 11:00 11:30 13:00 13:30 14:00 14:30 15:00 15:30", "6 6 7 7 7 7 6 6 7 7 7 7"),
                    new("1900/1/2".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Sunday, 0, 0, "", ""),
                    new("1900/1/3".ToDateOrToday(), DateTime.MaxValue.Date, DowCode.Saturday, 0, 0, "", ""),
                ]);
            }


            if (!context.EquipmentBookings.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var equipments = context.Equipments.ToList();
                var equipmentMax = equipments.Count;

                slots = Seeder.CreateSlotStrings(slots);
                var slotMax = slots.Length;

                var symbols = new[] { "", "鼻", "口", "★" };
                var symbolMax = symbols.Length;

                var firstDate = DateTime.Today.AddDays(1 - DateTime.Today.Day);
                for (var i = 0; i < 3000; i++)
                {
                    var equipId = equipments.Skip(rnd.Next(0, equipmentMax)).First().EquipId;
                    var date = firstDate.AddDays(rnd.Next(0, 60));
                    var slot = slots[rnd.Next(0, slotMax)];
                    var symbol = symbols[rnd.Next(0, symbolMax)];
                    var booking = new ReservationEquipmentBooking(equipId, date, slot, symbol, $"remark_{i}", true);
                    context.EquipmentBookings.Add(booking);
                }
            }


            #endregion 予約関連


            count += await context.SaveChangesAsync();

            await trans.CommitAsync();

            return count;
        }
        catch (Exception ex)
        {
            if (trans is not null)
            {
                await trans.RollbackAsync();
            }
            Debug.WriteLine(ex.ToString());
            throw;
        }
    }

    #region 名前

    private static readonly List<(string Kanji, string Kana)> s_fullNames =
    [
        ("日本 太郎", "ニッポン タロウ"),
        ("勘解由小路 安里朱羅", "カデノコウジ アサトアシュラ"),
        ("小鳥遊 一二三四五六", "タカナシ ヒフミシゴロク"),
        ("四月一日 三十郎", "ワタヌキ サンジュウロウ"),
        ("一 一", "ニノマエ ハジメ"),
        ("九十九 夜羅", "ツクモ ヤラ"),
        ("微睡 夢芽", "マドロミ ユメ"),
        ("終宴 鐘火", "フィナーレ ショウカ"),
        ("五十鈴 狂詩曲", "イスズ ラプソディ"),
        ("喝采 鎮魂", "カッサイ レクイエム"),
        ("常闇 姫緋", "トコヤミ ピンク"),
        ("羅喉 波動", "ラゴ ハドウ"),
        ("虚空 夢観", "コクウ ムゲン"),
        ("焔魔 煙羅", "エンマ スモーク"),
        ("鬼蓮 石蜜", "キレン ハニー"),
        ("千歳 ノア", "チトセ ノア"),
        ("夢扉 真白", "ユトビラ マシロ"),
        ("幽鳥 城杜", "ユウチョウ ジョウト"),
        ("不如帰 九命", "ホトトギス キュウメイ"),
        ("星涙 長明", "セイルイ チョウメイ"),
        ("微笑 呪術", "ビショウ ジュジュツ"),
        ("月詠 遥灯", "ツクヨミ ハルト"),
        ("鵺姫 繭夜", "ヌエキ マユヤ"),
        ("音無 想夢", "オトナシ ソウム"),
        ("囁砂 掌心", "ササヤサ テノヒラ"),

        ("夏目 漱石", "ナツメ ソウセキ"),
        ("太宰 治", "ダザイ オサム"),
        ("芥川 龍之介", "アクタガワ リュウノスケ"),
        ("谷崎 潤一郎", "タニザキ ジュンイチロウ"),
        ("三島 由紀夫", "ミシマ ユキオ"),
        ("川端 康成", "カワバタ ヤスナリ"),
        ("正岡 子規", "マサオカ シキ"),
        ("森 鴎外", "モリ オウガイ"),
        ("北原 白秋", "キタハラ ハクシュウ"),
        ("夢野 久作", "ユメノ キュウサク"),
        ("武者小路 実篤", "ムシャノコウジ サネアツ"),
        ("石川 啄木", "イシカワ タクボク"),
        ("内田 百閒", "ウチダ ヒャッケン"),    // 「百間」でなく「百閒」表記
        ("幸田 露伴", "コウダ ロハン"),
        ("江戸川 乱歩", "エドガワ ランポ"),
        ("井伏 鱒二", "イブセ マスジ"),
        ("大佛 次郎", "オサラギ ジロウ"),      // 「大仏」ではなく「大佛」
        ("火野 葦平", "ヒノ アシヘイ"),
        ("久生 十蘭", "ヒサオ ジュウラン"),
        ("小泉 八雲", "コイズミ ヤクモ"),      // ラフカディオ・ハーンの日本名
        ("江戸川 コナン", "エドガワ コナン"),  // 漫画『名探偵コナン』より
        ("範馬 刃牙", "ハンマ バキ"),          // 漫画『刃牙』より
        ("夜神 月", "ヤガミ ライト"),          // 『DEATH NOTE』
        ("夢野 幻太郎", "ユメノ ゲンタロウ"),  // 複数作品で見られる「夢野」姓の一例
        ("墨須 恵", "スミス メグミ"),          // あえてカタカナでなく漢字で「スミス」
        ("鬼舞辻 無惨", "キブツジ ムザン"),    // 『鬼滅の刃』
        ("モンキー・D・ルフィ", "モンキー ディー ルフィ"), // 『ONE PIECE』
        ("蒼井 そら", "アオイ ソラ"),          // 実在・芸名両面で話題になりやすい
        ("鹿目 まどか", "カナメ マドカ"),      // 『魔法少女まどか☆マギカ』
        ("草薙 素子", "クサナギ モトコ"),      // 『攻殻機動隊』
        ("涼宮 ハルヒ", "スズミヤ ハルヒ"),    // 『涼宮ハルヒ』シリーズ
        ("星野 鉄郎", "ホシノ テツロウ"),      // 『銀河鉄道999』
        ("嘉門 達夫", "カモン タツオ"),        // 実在の方だが名の響きが面白いと話題
        ("両儀 式", "リョウギ シキ"),          // 『空の境界』
        ("吉良 吉影", "キラ ヨシカゲ"),        // 『ジョジョの奇妙な冒険』
        ("風早 翔太", "カゼハヤ ショウタ"),    // 『君に届け』
        ("涼風 青葉", "スズカゼ アオバ"),       // 『NEW GAME!』
        ("土方 十四郎", "ヒジカタ ジュウシロウ"), // 『銀魂』
        ("坂田 銀時", "サカタ ギントキ"),      // 『銀魂』
        ("おそ松 カラ松", "オソマツ カラマツ"), // 『おそ松さん』
        ("竹取の翁 かぐや", "タケトリノオキナ カグヤ"), // 古典+創作的アレンジの面白さ
        ("空条 承太郎", "クウジョウ ジョウタロウ"),  // 『ジョジョの奇妙な冒険』Part3
        ("東方 仗助", "ヒガシカタ ジョウスケ"),    // 『ジョジョの奇妙な冒険』Part4
        ("花京院 典明", "カキョウイン ノリアキ"), // 『ジョジョの奇妙な冒険』Part3
        ("渦巻 ナルト", "ウズマキ ナルト"),       // 『NARUTO』
        ("黒崎 一護", "クロサキ イチゴ"),         // 『BLEACH』
        ("石神 千空", "イシガミ センクウ"),       // 『Dr.STONE』
        ("諸星 あたる", "モロボシ アタル"),       // 『うる星やつら』
        ("早乙女 乱馬", "サオトメ ランマ"),       // 『らんま1/2』
        ("犬夜叉", "イヌヤシャ"),                 // 『犬夜叉』
        ("殺生丸", "セッショウマル"),             // 『犬夜叉』
        ("緑谷 出久", "ミドリヤ イズク"),         // 『僕のヒーローアカデミア』
        ("爆豪 勝己", "バクゴウ カツキ"),         // 『僕のヒーローアカデミア』
        ("轟 焦凍", "トドロキ ショウト"),         // 『僕のヒーローアカデミア』
        ("虎杖 悠仁", "イタドリ ユウジ"),         // 『呪術廻戦』
        ("伏黒 恵", "フシグロ メグミ"),           // 『呪術廻戦』
        ("五条 悟", "ゴジョウ サトル"),           // 『呪術廻戦』
        ("竈門 炭治郎", "カマド タンジロウ"),     // 『鬼滅の刃』
        ("我妻 善逸", "アガツマ ゼンイツ"),       // 『鬼滅の刃』
        ("嘴平 伊之助", "ハシビラ イノスケ"),     // 『鬼滅の刃』
        ("菜月 昴", "ナツキ スバル"),             // 『Re:ゼロから始める異世界生活』

        ("สมชาย พิมพ์ทอง", "ソムチャイ ピムトン"),         // タイ語表記
        // ミャンマー語（ビルマ語）
        ("မြပါဝါ", "ミャーパワー"),                        // (Mya Pawar) 等、実際には複数のローマ字転写あり
        // ジョージア語
        ("თამარ ყიფიანი", "タマル キピアニ"),               // (Tamar Kipiani)
        // アルメニア語
        ("Հովհաննես Թումանյան", "ホヴハンネス トゥマニャン"), // (Hovhannes Tumanyan)
        // ヘブライ語 (イスラエル)
        ("משה כהן", "モーシェ コーヘン"),                   // (Moshe Cohen)
        // アラビア語
        ("فاطمة الشيخ", "ファーティマ アッシェイク"),       // (Fatimah Al-Sheikh)
        // モンゴル語 (キリル表記)
        ("Бат-Эрдэнэ Чүлтэм", "バトエルデネ チュルテム"),   // (Bat-Erdene Chultem)

        // ドイツ(探検家)
        ("アレクサンダー フリードリヒ ヴィルヘルム フォン フンボルト", "アレクサンダー フリードリヒ ヴィルヘルム フォン フンボルト"),

        ("John Smith", "ジョン スミス"),                                // USA / UK (英語圏)
        ("Max Mustermann", "マックス ムスターマン"),                  // ドイツ
        ("Jean Dupont", "ジャン デュポン"),                            // フランス
        ("Mario Rossi", "マリオ ロッシ"),                              // イタリア
        ("Juan Pérez", "フアン ペレス"),                               // スペイン
        ("张伟", "チャン ウェイ"),                                     // 中国
        ("劉德華", "リウ デーフア"),                      // 中国 (繁体字・香港などで有名な表記)
        ("范冰冰", "ファン ビンビン"),                     // 中国 (簡体字) / 台湾・香港では「範氷氷」
        ("김철수", "キム チョルス"),                                   // 韓国
        ("朴智星", "パク チソン"),                         // 韓国 (ハングル)
        ("김영희", "キム ヨンヒ"),                         // 韓国 (ハングル) 女性名の例
        ("Иван Иванов", "イワン イワノフ"),                           // ロシア
        ("José da Silva", "ジョゼ ダ シルバ"),                         // ブラジル
        ("Amit Kumar", "アミット クマール"),                            // インド
        ("Jean Tremblay", "ジャン トレンブレ"),                        // カナダ(仏語圏)
        ("Amelia Brown", "アメリア ブラウン"),                         // オーストラリア (英語圏)
        ("John Okafor", "ジョン オカフォー"),                           // ナイジェリア
        ("Sven Svensson", "スヴェン スヴェンソン"),                    // スウェーデン
        ("Jan Kowalski", "ヤン コヴァルスキ"),                        // ポーランド
        ("Giorgos Papadopoulos", "ギオルゴス パパドプーロス"),         // ギリシャ
        ("Ahmet Yılmaz", "アフメト イルマズ"),                          // トルコ
        ("Nguyễn Văn A", "グエン ヴァン ア"),                           // ベトナム
        ("Siti Nurhaliza", "シティ ヌルハリザ"),          // マレーシア
        ("Muhammad Rizki", "ムハンマド リズキ"),           // インドネシア
        ("Somsak Chaiyaphum", "ソムサック チャイヤプーミ"), // タイ
        ("Juan dela Cruz", "フアン デラ クルス"),          // フィリピン
        ("Sreylin Sok", "スレイリン ソク"),               // カンボジア
        ("La Min Myo", "ラ ミン ミョ"),                   // ミャンマー
        ("Michel Tremblay", "ミシェル トレンブレ"),                    // カナダ(仏語圏)例
        ("Fatima Al-Farsi", "ファティマ アルファルシ"),               // オマーン (中東)
        ("Sayed Hassan", "サイード ハッサン"),                         // アフガニスタン / 中東地域
        ("Rebecca Clarke", "レベッカ クラーク"),                       // ニュージーランド (英語圏)
        ("Koffi Atta", "コフィ アッタ"),                               // ガーナ (アフリカ)
        ("Arman Baghbani", "アルマン バグバニ"),                       // イラン (中東)
        ("Chloe Martin", "クロエ マルタン"),                           // ベルギー (仏語圏)
        ("Paolo Bianchi", "パオロ ビアンキ"),                          // イタリア (追加例)
        ("Alina Petrova", "アリーナ ペトロヴァ"),                      // ウクライナ
        ("Jacques Leroy", "ジャック ルロワ"),                          // フランス (追加例)
        ("Fatih Özdemir", "ファティフ オズデミル"),                     // トルコ (追加例)

        ("Johann Wolfgang von Goethe", "ヨハン ヴォルフガング フォン ゲーテ"),         // ドイツ(文豪)
        ("Fitzgerald Patrick O'Connor III", "フィッツジェラルド パトリック オコナー サード"), // 英語圏(アイルランド系など)
        ("Maximilienne Éléonore d'Artois", "マクシミリアンヌ エレオノール ダルトワ"), // フランス
        ("Jean-Pierre Lefèvre d'Ormesson", "ジャン ピエール ルフェーヴル ドルメッソン"), // フランス
    ];

    // 姓とフリガナ
    private static readonly List<(string Kanji, string Kana)> s_familyNames =
    [
        ("健診",   "ケンシン"),   // 「鳥が遊ばない」→「鷹がいるから」→「タカナシ」
        ("小鳥遊",   "タカナシ"),   // 「鳥が遊ばない」→「鷹がいるから」→「タカナシ」
        ("四月一日", "ワタヌキ"),   // 衣替えで「綿を抜く」時期→「ワタヌキ」
        ("五月七日", "ツユリ"),     // 諸説あり・地域により異なる読み
        ("鬼束",     "オニツカ"),
        ("勘解由小路", "カデノコウジ"),
        ("百目鬼",   "ドウメキ"),
        ("七五三掛", "シメカケ"),   // 「しめかけ」「しめがけ」など諸説
        ("無量塔",   "ムヤタ"),     // 「むりょうとう」「むやた」など諸説
        ("道祖土",   "サイド"),     // 「どうそど」「さいど」など地域によって異なる
        ("夜神",     "ヤガミ"),
        ("一尺八寸",   "イッシャクハッスン"),
        ("四十物谷",   "アイモノヤ"),
        ("蕨",         "ワラビ"),
        ("春夏冬",     "アキナイ"), // 「春・夏・冬」があって「秋」がない→「あきない」
        ("八月一日",   "ホヅミ"),   // 「はちがつついたち」ではなく「ほづみ」と読むことがある
        ("猫屋敷",     "ネコヤシキ"),
        ("集道",       "アダリ"),
        ("七種",       "ナナクサ"), // 地域により「サイシュ」「ななくさ」など多様
        ("我孫子",     "アビコ"),   // 「がそんし」「あまこ」など地域差あり
        ("上官",       "ジョウカン"),
        ("鰐渕",       "ワニブチ"),
        ("燕",         "ツバメ"),
        ("杁",         "エブリ"),   // 漢字1文字でありながら珍しい苗字
        ("錦",         "ニシキ"),   // 一見シンプルだが、実は世帯数が少ないケース
        ("鴨志田",     "カモシダ"),
        ("石動",       "イスルギ"),
        ("薬袋",       "ミナイ"),   // 地域により「やくたい」「やたい」「みない」等の読みがある
        ("出利葉",     "イヅリハ"),
        ("下間",       "シモツマ"), // 「げま」「しもま」など他の読みもある
        ("四十九院",   "シジュウクイン"),
        ("九十九",     "ツクモ"),
        ("百々",       "ドド"),
        ("黒葛原",     "ツヅラハラ"),
        ("古波蔵",     "コハグラ"),  // 沖縄に多い苗字とされる
        ("左衛門尉",   "サエモンノジョウ"),
        ("万年",       "マンネン"),
        ("皆実",       "ミナミ"),
        ("散布",       "チルシ"),
        ("右近",       "ウコン"),
        ("恵良",       "エラ"),
        ("小豆沢",     "アズサワ"),
        ("百々瀬",     "ドドセ"),
        ("土師",       "ハジ"),
        ("壹岐",       "イキ"),
        ("於保",       "オホ"),
        ("源五郎丸",   "ゲンゴロウマル"),
        ("小槻",       "オヅキ"),
        ("金生",       "キンジョウ"),   // 「カナオ」など、他の読みも存在
        ("魚返",       "オガエリ"),
        ("羽後",       "ウゴ"),
        ("烏丸",       "カラスマ"),     // 京都の地名としても有名だが、苗字としては珍しい部類
        ("庵原",       "イハラ"),       // 「あんばら」「いはら」など地域差あり
        ("空閑",       "クガ"),         // 「くうかん」ではなく「くが」と読む
        ("十六",       "ジュウロク"),   // 数字だけに見えるが、苗字としても存在
        ("生居",       "ナマイ"),       // 「いきょ」「しょうご」などの異読も
        ("土生",       "ハブ"),         // 「どしょう」「つちう」「はぶ」等の地域差あり
        ("大豆生田",   "オオマメウダ"), // 「だいずうだ」「おおまめだ」など多数の読みが存在
        ("栴檀",       "センダン"),     // 難字で、仏教用語の「栴檀(せんだん)」に通じる
        ("一尺屋",     "イッシャクヤ"), // 由来は諸説あるが画数が多く珍しい
        ("伯耆",       "ホウキ"),       // 地名由来で鳥取県方面に見られる
        ("彌永",       "ヤナガ"),       // 「いやなが」「やえい」などの読み違いあり
        ("外間",       "ホカマ"),       // 沖縄独特の苗字の一つ
        ("木幡",       "コハタ"),       // 「きばた」「こばた」などの読み方も存在
        ("冷泉",       "レイゼイ"),     // 古くは貴族の家系名だったとも
        ("桃生",       "モノウ"),       // 宮城県の地名が起源とされる
        ("長柄",       "ナガラ"),       // 「ながえ」「ながら」など地名に由来する説
        ("日置",       "ヘキ"),         // 「ひおき」「へき」など、地域によって大きく異なる
        ("鹿糠",       "カヌカ"),       // 「しかぬか」「かぬか」などの読みがある
        ("四方",       "ヨモ"),         // 「しかた」「しほう」「よも」など多様な読み
        ("鴾",         "トキ"),         // 鳥の「朱鷺」とも関係あるかもしれない難字
        ("五百旗頭",   "イオキベ"),     // 「いおきとう」と読まれることもあるが一般的には「いおきべ」
        ("東雲",       "シノノメ"),     // 「とううん」ではなく「しののめ」
        ("正親町",     "オオギマチ"),   // 古くは公家由来の難読苗字
        ("御手洗",     "ミタライ"),     // 「おてあらい」ではなく「みたらい」
        ("廿楽",       "ツヅラ"),       // 「にじゅうらく」ではなく「つづら」
        ("五十公野",   "イジミノ"),     // 数字や公の字が含まれ紛らわしい
        ("百足",       "ムカデ"),       // そのまま虫の名だが「むかで」と読む苗字
        ("舎人",       "トネリ"),       // 「しゃにん」ではなく「とねり」
        ("屋慶名",     "ヤケナ"),       // 沖縄系独特の読みによる難読
        ("読谷山",     "ヨミタンザン"), // 地名由来の苗字だが「よみたんざん」
        ("瑞慶覧",     "ズケラン"),     // 沖縄では比較的見かけるが、本土では難読
        ("上與那原",   "カミヨナバル"), // 「かみよなばる」「うえよなはら」など複数の読み方がある
        ("城間",       "グスクマ"),     // 「しろま」ではなく「ぐすくま」と読むケース
        ("饒平名",     "ヨヘナ"),       // 「にょうへいな」ではなく「よへな」
        ("豊見城",     "トミグスク"),   // 沖縄の地名由来で「とみぐすく」
        ("加加美",     "カカミ"),       // 画数が多めで読みが想像しづらい
        ("御厨",       "ミクリヤ"),     // 「みくり」「おくりや」などと誤読されがち
        ("我那覇",     "ガナハ"),       // 沖縄特有の難読苗字
        ("摩文仁",     "マブニ"),       // こちらも沖縄系で「まぶに」
    ];

    // 名とフリガナ
    private static readonly List<(string Kanji, string Kana)> s_givenNames =
    [
        ("太郎", "タロウ"),
        ("一郎", "イチロウ"),
        ("次郎", "ジロウ"),
        ("三郎", "サブロウ"),
        ("士郎", "シロウ"),
        ("吾郎", "ゴロウ"),
        ("六助", "ロクスケ"),
        ("奈々", "ナナ"),
        ("八雲", "ヤクモ"),
        ("勘九郎", "カンクロウ"),
        ("清十郎", "セイジュウロウ"),
        ("花子", "ハナコ"),
        ("健一", "ケンイチ"),
        ("健二", "ケンジ"),
        ("賢治", "ケンジ"),
        ("ひかり", "ヒカリ"),
        ("一", "ハジメ"),
        ("始", "ハジメ"),
        ("壱", "ハジメ"),
        ("肇", "ハジメ"),
        ("一二三", "ヒフミ"),
        ("結衣", "ユイ"),
        ("大輔", "ダイスケ"),
        ("陽菜", "ハルナ"),
        ("未来", "ミライ"),
        ("藍",   "アイ"),
        ("希空", "ノア"),
        ("詩恩", "シオン"),
        ("一花", "イチカ"),
        ("円花", "マドカ"),
        ("雨音", "アマネ"),
        ("麗",   "レイ"),
        ("碧",   "アオイ"),
        ("湊",   "ミナト"),
        ("蘭",   "ラン"),
        ("胡蝶",  "コチョウ"),
        ("澪",    "ミオ"),
        ("藍花",  "アイカ"),
        ("想良",  "ソラ"),
        ("朔也",  "サクヤ"),
        ("琥珀",  "コハク"),
        ("瑠璃",  "ルリ"),
        ("透真",  "トウマ"),
        ("幸仁",  "コウジン"),
        ("弦",    "ユズル"),  // 「ゲン」など、複数の読みが存在する場合あり
        ("雫",    "シズク"),
        ("奏",    "カナデ"),
        ("芭菜",  "ハナ"),
        ("泉貴",  "イズキ"),
        ("翡翠",  "ヒスイ"),
        ("椛",    "モミジ"),
        ("匡真",  "キョウマ"),
        ("結人",  "ユイト"),
        ("翠",    "スイ"),
        ("楓香",  "フウカ"),
    ];

    /// <summary>
    /// 指定した件数分の「姓・名 + フリガナ」を組み合わせた PersonName のリストを生成します。
    /// </summary>
    /// <param name="count">生成件数</param>
    /// <returns>PersonName のリスト</returns>
    private static List<(string Kanji, string Kana)> GenerateSampleNames(int count)
    {
        var rnd = new Random();
        var results = new List<(string Kanji, string Kana)>();
        results.AddRange(s_fullNames);

        count -= results.Count;
        for (var i = 0; i < count; i++)
        {
            var last = s_familyNames[rnd.Next(s_familyNames.Count)];
            var first = s_givenNames[rnd.Next(s_givenNames.Count)];

            var person = (
                String.Join(' ', last.Kanji, first.Kanji),
                String.Join(' ', last.Kana, first.Kana)
            );

            results.Add(person);
        }

        return results;
    }

    #endregion 名前

    /// <summary>
    /// 最大公約数的なスロットのリストを作成します。
    /// </summary>
    private static string[] CreateSlotStrings(string[] slots)
    {
        // 定義されている最大の slot の一覧を作成する
        var cols = new HashSet<string>(slots.Length);


        // def 毎の slot 重複数を数える
        var slotCounts = new Dictionary<string, int>();

        for (var i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];

            if (!slotCounts.TryAdd(slot, 1))
            {
                slotCounts[slot]++;
                // 重複したら (n) をつける
                slot = $"{slot}({slotCounts[slot]})";
                slots[i] = slot;
            }

            cols.Add(slot);
        }

        return cols.OrderBy(x => x).ToArray();
    }
}
