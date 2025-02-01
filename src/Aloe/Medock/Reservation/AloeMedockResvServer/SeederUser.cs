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
    private static async Task<int> SeedUserAsync(AppDbContext context)
    {
        try
        {
            var count = 0;

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

            if (!context.Permissions.AsNoTracking().Any())
            {
                var permissions = PermissionService.CreateDefaultPermissions();
                context.Permissions.AddRange(permissions.Values);
            }

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

            if (!context.UserPreferences.AsNoTracking().Any())
            {
                // 挿入したデータを使うため保存する
                count += await context.SaveChangesAsync();

                var adminUser = context.Users.First(x => x.LoginName == "admin");

                context.UserPreferences.AddRange([
                    new (adminUser.UserId, PreferenceCode.WindowRememberPosition, "", true),
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

            count += await context.SaveChangesAsync();

            return count;
        }
        catch
        {
            throw;
        }
    }

}
