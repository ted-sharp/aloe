using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;

public class ReservationEquipmentCacheService
{

    private readonly ILogger _logger;
    private readonly IMemoryCache _cache;
    private readonly IHolidayGrpcService _holidayGrpcService;
    private readonly IReservationEquipmentGrpcService _equipGrpcService;

    public ReservationEquipmentCacheService(
        ILogger<ReservationEquipmentCacheService> logger,
        IMemoryCache cache,
        IHolidayGrpcService holidayGrpcService,
        IReservationEquipmentGrpcService equipGrpcService)
    {
        this._logger = logger;
        this._cache = cache;
        this._holidayGrpcService = holidayGrpcService;
        this._equipGrpcService = equipGrpcService;
    }

    public async Task<List<HolidayDto>> GetOrFetchHolidays(int year, int month, bool useCache = true)
    {

        var key = $"holidays_{year:0000}{month:00}";
        if (useCache && this._cache.TryGetValue<List<HolidayDto>>(
                key, out var holidays))
        {
            return holidays ?? [];
        }

        holidays = await this._holidayGrpcService.FetchHolidayDtosAsync(year, month);

        this._cache.Set(key, holidays, new MemoryCacheEntryOptions
        {
            // 祝日はめったに変わらないのでしばらく保持しておく
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        });

        return holidays;
    }

    public async Task<List<ReservationEquipmentDto>> GetOrFetchEquipments(bool useCache = true)
    {
        var key = "equipments";
        if (useCache && this._cache.TryGetValue<List<ReservationEquipmentDto>>(
                key, out var equipments))
        {
            return equipments ?? [];
        }

        equipments = await this._equipGrpcService.FetchEquipmentDtosAsync();

        this._cache.Set(key, equipments, new MemoryCacheEntryOptions
        {
            // マスター情報はまず変更がないのでしばらく保持しておく
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        });

        return equipments;
    }

    public async Task<List<ReservationEquipmentSlotDto>> GetOrFetchSlots(int year, int month, int? equipId, bool useCache = true)
    {

        var key = $"equipmentSlots_{year:0000}{month:00}_{equipId ?? 0}";
        if (useCache && this._cache.TryGetValue<List<ReservationEquipmentSlotDto>>(
                key, out var slots))
        {
            return slots ?? [];
        }

        slots = await this._equipGrpcService.FetchEquipmentSlotDtosAsync(year, month, equipId);

        this._cache.Set(key, slots, new MemoryCacheEntryOptions
        {
            // スロット情報は毎日変更があるが、リアルタイムの変更もあるので程々とする
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        });

        return slots;
    }

    public async Task<List<string>> GetOrFetchSlotStrings(int year, int month, int? equipId, bool useCache = true)
    {
        var key = $"equipmentSlotStrings_{year:0000}{month:00}_{equipId ?? 0}";
        if (useCache && this._cache.TryGetValue<List<string>>(
                key, out var slotStrings))
        {
            return slotStrings ?? [];
        }

        var slots = await this.GetOrFetchSlots(year, month, equipId, useCache);
        slotStrings = this.CreateSlotStrings(slots, equipId);

        this._cache.Set(key, slots, new MemoryCacheEntryOptions
        {
            // スロット情報は毎日変更があるが、リアルタイムの変更もあるので程々とする
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        });

        return slotStrings;
    }

    /// <summary>
    /// 最大公約数的なスロットのリストを作成します。
    /// </summary>
    private List<string> CreateSlotStrings(List<ReservationEquipmentSlotDto> definitions, int? equipId)
    {
        // 定義されている最大の slot の一覧を作成する
        var cols = new HashSet<string>();

        // 全対象(EquipId == 0) と 条件なしあり(equipId == null || x.EquipId == equipId) にしぼる
        var defs = definitions
            .Where(x => x.EquipId == 0 || (equipId == null || x.EquipId == equipId))
            .ToList();

        foreach (var def in defs)
        {
            // def 毎の slot 重複数を数える
            var slotCounts = new Dictionary<string, int>();

            for (var i = 0; i < def.Slots.Length; i++)
            {
                var slot = def.Slots[i];

                if (!slotCounts.TryAdd(slot, 1))
                {
                    slotCounts[slot]++;
                    // 重複したら (n) をつける
                    slot = $"{slot}({slotCounts[slot]})";
                    def.Slots[i] = slot;
                }

                cols.Add(slot);
            }
        }

        return cols.OrderBy(x => x).ToList();
    }

    public async Task<List<ReservationEquipmentBookingDto>> GetOrFetchBookings(string monthString, int equipId, bool useCache = true)
    {
        var date = monthString.ToDateOrToday();
        var year = date.Year;
        var month = date.Month;
        return await this.GetOrFetchBookings(year, month, equipId, useCache);
    }

    public async Task<List<ReservationEquipmentBookingDto>> GetOrFetchBookings(int year, int month, int equipId, bool useCache = true)
    {
        var key = $"equipmentBookings_{year:0000}{month:00}_{equipId}";
        if (useCache && this._cache.TryGetValue<List<ReservationEquipmentBookingDto>>(
                key, out var bookings))
        {
            return bookings ?? [];
        }

        bookings = await this._equipGrpcService.FetchEquipmentBookingDtosAsync(year, month, equipId);

        this._cache.Set(key, bookings, new MemoryCacheEntryOptions
        {
            // リアルタイムなので数秒とする
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5),
            SlidingExpiration = null,
        });

        return bookings;
    }

    public void SetColumn(int year, int month, int day, object column)
    {
        var key = $"equipmentBookings_DataGridColumn_{year:0000}{month:00}{day:00}";

        this._cache.Set(key, column, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
            SlidingExpiration = null,
        });
    }

    public object? GetColumn(int year, int month, int day)
    {
        var key = $"equipmentBookings_DataGridColumn_{year:0000}{month:00}{day:00}";
        if (this._cache.TryGetValue<object>(
                key, out var column))
        {
            return column;
        }

        return null;
    }

    public void SetBookingData(int year, int month, int equipId, BookingData data)
    {
        var key = $"equipmentBookings_{nameof(BookingRow)}List_{year:0000}{month:00}_{equipId}";

        this._cache.Set(key, data, new MemoryCacheEntryOptions
        {
            // リアルタイムなので数秒とする
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5),
            SlidingExpiration = null,
        });
    }

    public BookingData? GetBookingData(int year, int month, int equipId)
    {
        var key = $"equipmentBookings_{nameof(BookingRow)}List_{year:0000}{month:00}_{equipId}";
        if (this._cache.TryGetValue<BookingData>(
                key, out var data))
        {
            return data;
        }

        return null;
    }
    public void SetBookingData2(int year, int month, int equipId, BookingData2 data)
    {
        var key = $"equipmentBookings_{nameof(RecyclingBookingRow)}List_{year:0000}{month:00}_{equipId}";

        this._cache.Set(key, data, new MemoryCacheEntryOptions
        {
            // リアルタイムなので数秒とする
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5),
            SlidingExpiration = null,
        });
    }

    public BookingData2? GetBookingData2(int year, int month, int equipId)
    {
        var key = $"equipmentBookings_{nameof(RecyclingBookingRow)}List_{year:0000}{month:00}_{equipId}";
        if (this._cache.TryGetValue<BookingData2>(
                key, out var data))
        {
            return data;
        }

        return null;
    }
}
