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

public interface IPreferenceService
{
    Task LoadPreferencesAsync();

    Task<List<Preference>> FetchUserPreferencesAsync(int userId);
}

public class PreferenceService : IPreferenceService
{
    private readonly ILogger _logger;
    private readonly IMemoryCache _cache;
    private readonly AppDbContext _context;

    public PreferenceService(
        ILogger<PreferenceService> logger,
        IMemoryCache cache,
        AppDbContext context)
    {
        this._logger = logger;
        this._cache = cache;
        this._context = context;
    }

    public async Task LoadPreferencesAsync()
    {
        await Task.Run(() => this.FetchPreferences(false));
    }

    private async Task<Dictionary<string, Preference>> FetchPreferencesAsync()
    {
        return await Task.Run(() => this.FetchPreferences(true));
    }

    private Dictionary<string, Preference> FetchPreferences(bool useCache = true)
    {
        try
        {
            var key = $"{nameof(PreferenceService)}_{nameof(this.FetchPreferences)}";
            if (useCache && this._cache.TryGetValue<Dictionary<string, Preference>>(
                    key, out var prefs))
            {
                return prefs ?? [];
            }

            // デフォルトをもとにする
            prefs = PreferenceService.CreateDefaultPreferences();

            // DBにある設定で上書きする
            var prefList = this._context.Preferences
                .AsNoTracking()
                .ToList();
            foreach (var pref in prefList)
            {
                prefs[pref.PrefCode] = pref;
            }

            // キャッシュを更新する
            this._cache.Set(key, prefs, new MemoryCacheEntryOptions
            {
                // 朝のログイン時に集中するためしばらく保持しておく
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            });
            this._logger.LogDebug("Preference loaded count: {Count}", prefs.Count);

            return prefs;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred during preference loading.");
        }

        return [];
    }

    public async Task<List<Preference>> FetchUserPreferencesAsync(int userId)
    {
        try
        {
            var prefs = await this.FetchPreferencesAsync();

            // ユーザーの設定で上書きする
            var prefList = this._context.UserPreferences
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToList();
            foreach (var pref in prefList)
            {
                prefs[pref.PrefCode].PrefValue = pref.PrefValue;
                prefs[pref.PrefCode].IsActive = pref.IsActive;
            }

            return prefs.Values.ToList();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred during preference loading.");
        }

        return [];
    }


    #region CreateDefaultPreferences

    public static Dictionary<string, Preference> CreateDefaultPreferences()
    {
        var policies = new Dictionary<string, Preference>
        {
            [PreferenceCode.WindowRememberPosition] = new()
            {
                PrefCode = PreferenceCode.WindowRememberPosition,
                PrefName = nameof(PreferenceCode.WindowRememberPosition),
                PrefDesc = "Window ポジションを記憶します。",
                DataType = Constants.DataType.String,
                PrefValue = "",
                IsActive = true,
            },
        };

        return policies;
    }

    #endregion CreateDefaultPreferences
}
