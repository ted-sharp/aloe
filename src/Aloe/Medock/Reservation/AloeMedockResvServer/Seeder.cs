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
            count += await Seeder.SeedAsync(context);
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

        int count = 0;

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
                "EX", "EX", "EX", "EX",
            };

            trans = await context.Database.BeginTransactionAsync();

            #region マスターデータ

            #region マスターデータ/ユーザー

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

            #endregion マスターデータ/ユーザー

            #region マスターデータ/予約

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

            #endregion マスターデータ/予約

            #endregion マスターデータ

            #region トランザクションデータ

            #region トランザクションデータ/団体

            if (!context.InsuranceProviders.AsNoTracking().Any())
            {
                // TODO: テストなので全保険者を挿入したい

                context.InsuranceProviders.AddRange([
                    new((int)InsuranceProviderType.KyokaiKenpo, "01060025", "協会けんぽ 北海道", ""),
                    new((int)InsuranceProviderType.KyokaiKenpo, "13060025", "協会けんぽ 東京都", ""),
                ]);
            }

            if (!context.Organizations.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var insurances = context.InsuranceProviders.ToList();
                var insuranceMax = insurances.Count;

                // TODO: テストなので全企業を挿入したい

                for (var i = 0; i < 3000; i++)
                {
                    var insurance = insurances.Skip(rnd.Next(0, insuranceMax)).First();
                    var insurProvType = insurance.InsurProvTypeCode;
                    var insurProvId = insurance.InsurProvId;
                    var org = new Organization(insurProvType, insurProvId, $"株式会社 ABC{i}", $"カブシキガイシャ　エービーシー{i}", $"ABC{i}", $"ABC{i} 御中");
                    context.Organizations.Add(org);
                }

                // TODO: contacts, remarks もいれたい

                //context.Organizations.AddRange([
                //    new("株式会社 ABC", "カブシキガイシャ　エービーシー", "ABC", "ABC"),
                //]);
            }

            if (!context.Patients.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var orgs = context.Organizations.ToList();
                var orgMax = orgs.Count;
                var sexes = Enum.GetValues<SexCode>();
                var sexMax = sexes.Length;

                for (var i = 0; i < 3000; i++)
                {
                    var org = orgs.Skip(rnd.Next(0, orgMax)).First();
                    var orgId = org.OrgId;
                    var insurProvId = org.InsurProvId;
                    var insurProvTypeCode = org.InsurProvTypeCode;
                    var karteNumber = rnd.Next(1, Int32.MaxValue).ToString("000000000");
                    var birthDate = DateTime.Today.AddDays(-rnd.Next(3650, 365000));
                    var sex = sexes.Skip(rnd.Next(0, sexMax)).First();
                    var pt = new Patient(karteNumber, $"健診　太郎{i}", $"ケンシン　タロウ{i}", birthDate, (int)sex);
                    context.Patients.AddRange(pt);
                }

                // TODO: contacts, remarks, insurance_cards もいれたい

                //context.Patients.AddRange([
                //    new("1", "山田　太郎", "ヤマダ　タロウ", "2000/4/1".ToDateOrToday(), (int)SexCode.Male),
                //    new("2", "山田　花子", "ヤマダ　ハナコ", "1980/12/31".ToDateOrToday(), (int)SexCode.Female),
                //    new("3", "名無しの　権兵衛", "ナナシノ　ゴンベエ", "1900/1/1".ToDateOrToday(), (int)SexCode.NotKnown),
                //]);
            }

            #endregion トランザクションデータ/団体

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


            #endregion トランザクションデータ

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
