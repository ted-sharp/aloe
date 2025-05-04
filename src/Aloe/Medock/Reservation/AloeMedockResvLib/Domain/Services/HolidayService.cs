using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MagicOnion.Server;
using MagicOnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Constants;
using Microsoft.Extensions.Caching.Memory;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;

/// <summary>
/// 祝日用のサービスです。
/// </summary>
public interface IHolidayService
{
    Task<List<HolidayDto>> GetOrFetchHolidayDtosAsync(int year, int month);
}

public class HolidayService : IHolidayService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;

    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private readonly string _cacheKeyPrefix = "holiday_";

    public HolidayService(
        ILogger<HolidayService> logger,
        IDbContextFactory<AppDbContext> factory,
        IMemoryCache cache)
    {
        this._logger = logger;
        this._factory = factory;
        this._cache = cache;

        this._cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        };
    }

    public async Task<List<HolidayDto>> GetOrFetchHolidayDtosAsync(int year, int month)
    {
        var firstDate = DateHelper.GetFirstDateTime(year, month);
        var endDate = DateHelper.GetEndDateTime(firstDate);

        var cacheKey = this._cacheKeyPrefix + $"{year:0000}{month:00}";

        // キャッシュから取得
        if (this._cache.TryGetValue<List<HolidayDto>>(cacheKey, out var cacheHolidays) &&
            cacheHolidays is not null)
        {
            return cacheHolidays;
        }

        // DBから取得
        await using var context = await this._factory.CreateDbContextAsync();
        var holidays = await context.Holidays
            .AsNoTracking()
            .Where(x =>
                firstDate <= x.HolidayDate
                && x.HolidayDate <= endDate
                && x.IsDeleted == false
            )
            .Select(x => new HolidayDto
            {
                HolidayDate = x.HolidayDate.ToDateOnly(),
                HolidayName = x.HolidayName,
            })
            .ToListAsync() ?? [];

        // DBから取得したPolicyをキャッシュに保存
        this._cache.Set(cacheKey, holidays, this._cacheOptions);
        return holidays;
    }
}
