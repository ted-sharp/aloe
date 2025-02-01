using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using MagicOnion.Server;
using MagicOnion;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;

public interface IPermissionService
{
    Task LoadPermissionsAsync();

    Task<List<Permission>> FetchUserPermissionsAsync(int userId);
}

public class PermissionService : IPermissionService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IMemoryCache _cache;

    public PermissionService(
        ILogger<PermissionService> logger,
        IDbContextFactory<AppDbContext> factory,
        IMemoryCache cache)
    {
        this._logger = logger;
        this._factory = factory;
        this._cache = cache;
    }

    public async Task LoadPermissionsAsync()
    {
        await Task.Run(() => this.FetchPermissions(false));
    }

    private async Task<Dictionary<string, Permission>> FetchPermissionsAsync()
    {
        return await Task.Run(() => this.FetchPermissions(true));
    }

    private Dictionary<string, Permission> FetchPermissions(bool useCache = true)
    {
        try
        {
            var key = $"{nameof(PermissionService)}_{nameof(this.FetchPermissions)}";
            if (useCache && this._cache.TryGetValue<Dictionary<string, Permission>>(
                    key, out var prefs))
            {
                return prefs ?? [];
            }

            // デフォルトをもとにする
            prefs = PermissionService.CreateDefaultPermissions();

            // DBにある設定で上書きする
            using var context = this._factory.CreateDbContext();
            var prefList = context.Permissions
                .AsNoTracking()
                .ToList();
            foreach (var pref in prefList)
            {
                prefs[pref.PermCode] = pref;
            }

            // キャッシュを更新する
            this._cache.Set(key, prefs, new MemoryCacheEntryOptions
            {
                // 朝のログイン時に集中するためしばらく保持しておく
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            });
            this._logger.LogDebug("Permission loaded count: {Count}", prefs.Count);

            return prefs;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred during preference loading.");
        }

        return [];
    }

    public async Task<List<Permission>> FetchUserPermissionsAsync(int roleId)
    {
        try
        {
            var prefs = await this.FetchPermissionsAsync();

            // ユーザーの設定で上書きする
            await using var context = await this._factory.CreateDbContextAsync();
            var permList = context.RolePermissions
                .AsNoTracking()
                .Where(x => x.RoleId == roleId)
                .ToList();
            foreach (var perm in permList)
            {
                prefs[perm.PermCode].IsActive = perm.IsActive;
            }

            return prefs.Values.ToList();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred during preference loading.");
        }

        return [];
    }


    #region CreateDefaultPermissions

    public static Dictionary<string, Permission> CreateDefaultPermissions()
    {
        var policies = new Dictionary<string, Permission>
        {
            [PermissionCode.MaintPoliciesR] = new()
            {
                PermCode = PermissionCode.MaintPoliciesR,
                PermName = nameof(PermissionCode.MaintPoliciesR),
                PermDesc = "ポリシーマスターの表示権限です。",
            },
        };

        return policies;
    }

    #endregion CreateDefaultPermissions
}
