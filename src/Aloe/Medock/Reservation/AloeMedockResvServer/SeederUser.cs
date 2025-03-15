using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Defaults;
using EFCore.BulkExtensions;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

internal partial class Seeder
{
    private async Task<int> SeedUserAsync(AppDbContext context)
    {
        try
        {
            var count = 0;

            if (!context.Policies.Any())
            {
                this._logger.LogInformation("Policies creating...");
                var policies = DefaultPolicy.CreateDefaultPolicies();
                await context.BulkInsertAsync(policies.Values);
                count += policies.Count;
            }

            if (!context.Preferences.Any())
            {
                this._logger.LogInformation("Preferences creating...");
                var preferences = DefaultPreference.CreateDefaultPreferences();
                await context.BulkInsertAsync(preferences.Values);
                count += preferences.Count;
            }

            if (!context.Permissions.Any())
            {
                this._logger.LogInformation("Permissions creating...");
                var permissions = DefaultPermission.CreateDefaultPermissions();
                await context.BulkInsertAsync(permissions.Values);
                count += permissions.Count;
            }

            if (!context.Users.Any())
            {
                this._logger.LogInformation("Users creating...");

                var users = new User[]
                {
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
                };

                var bulkConfig = new BulkConfig
                {
                    // DateOnly 型は除外
                    PropertiesToExclude = [nameof(User.ExpireDate),],
                };

                await context.BulkInsertAsync(users, bulkConfig);
                count += users.Length;
            }

            if (!context.Roles.Any())
            {
                this._logger.LogInformation("Roles creating...");

                var roles = new Role[]
                {
                    new("Administrator", "管理者です。"),
                    new("Manager", "マネージャーです。(ポリシーを除く)"),
                    new("User", "ユーザーです。(閲覧登録)"),
                    new("Guest", "ゲストです。(閲覧のみ)"),
                };

                await context.BulkInsertAsync(roles);
                count += roles.Length;
            }

            if (!context.UserPreferences.Any())
            {
                this._logger.LogInformation("UserPreferences creating...");

                var adminUser = context.Users.First(x => x.LoginName == "admin");

                var prefs = new UserPreference[]
                {
                    new (adminUser.UserId, PreferenceCode.WindowRememberPosition, "", true),
                };

                await context.BulkInsertAsync(prefs);
                count += prefs.Length;
            }

            if (!context.UserRoles.Any())
            {
                this._logger.LogInformation("UserRoles creating...");

                var adminUser = context.Users.First(x => x.LoginName == "admin");
                var adminRole = context.Roles.First(x => x.RoleName == "Administrator");
                var mgrUser = context.Users.First(x => x.LoginName == "mgr");
                var mgrRole = context.Roles.First(x => x.RoleName == "Manager");
                var usrUser = context.Users.First(x => x.LoginName == "usr");
                var usrRole = context.Roles.First(x => x.RoleName == "User");
                var guestUser = context.Users.First(x => x.LoginName == "guest");
                var guestRole = context.Roles.First(x => x.RoleName == "Guest");

                var roles = new UserRole[]
                {
                    new(adminUser.UserId, adminRole.RoleId),
                    new(mgrUser.UserId, mgrRole.RoleId),
                    new(usrUser.UserId, usrRole.RoleId),
                    new(guestUser.UserId, guestRole.RoleId),
                };

                await context.BulkInsertAsync(roles);
                count += roles.Length;
            }

            if (!context.RolePermissions.Any())
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

                //count += await context.SaveChangesAsync();
            }

            return count;
        }
        catch
        {
            throw;
        }
    }

}
