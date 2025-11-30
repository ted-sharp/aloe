using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
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
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;

public class ReservationCacheService
{

    private readonly ILogger _logger;
    private readonly IMemoryCache _cache;
    private readonly IHolidayGrpcService _holidayGrpcService;
    private readonly IReservationDailyGrpcService _dailyGrpcService;
    private readonly IReservationEquipmentGrpcService _equipGrpcService;

    public ReservationCacheService(
        ILogger<ReservationCacheService> logger,
        IMemoryCache cache,
        IHolidayGrpcService holidayGrpcService,
        IReservationDailyGrpcService dailyGrpcService,
        IReservationEquipmentGrpcService equipGrpcService)
    {
        this._logger = logger;
        this._cache = cache;
        this._holidayGrpcService = holidayGrpcService;
        this._dailyGrpcService = dailyGrpcService;
        this._equipGrpcService = equipGrpcService;
    }

    #region Masters

    public async Task<List<ReservationFloorDto>> GetOrFetchFloors(bool useCache = true)
    {
        var key = "floors";
        if (useCache && this._cache.TryGetValue<List<ReservationFloorDto>>(
                key, out var floors))
        {
            return floors ?? [];
        }

        floors = await this._dailyGrpcService.FetchFloorDtosAsync();

        this._cache.Set(key, floors, new MemoryCacheEntryOptions
        {
            // マスター情報はまず変更がないのでしばらく保持しておく
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        });

        return floors;
    }

    public async Task<List<ReservationRoomDto>> GetOrFetchRooms(bool useCache = true)
    {
        var key = "rooms";
        if (useCache && this._cache.TryGetValue<List<ReservationRoomDto>>(
                key, out var rooms))
        {
            return rooms ?? [];
        }

        rooms = await this._dailyGrpcService.FetchRoomDtosAsync();

        this._cache.Set(key, rooms, new MemoryCacheEntryOptions
        {
            // マスター情報はまず変更がないのでしばらく保持しておく
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        });

        return rooms;
    }

    public async Task<List<ReservationRoomDetailDto>> GetOrFetchRoomDetails(bool useCache = true)
    {
        var key = "rooms";
        if (useCache && this._cache.TryGetValue<List<ReservationRoomDetailDto>>(
                key, out var roomDetails))
        {
            return roomDetails ?? [];
        }

        roomDetails = await this._dailyGrpcService.FetchRoomDetailDtosAsync();

        this._cache.Set(key, roomDetails, new MemoryCacheEntryOptions
        {
            // マスター情報はまず変更がないのでしばらく保持しておく
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        });

        return roomDetails;
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

    #endregion Masters

    #region Holidays

    public async Task<List<HolidayDto>> GetOrFetchHolidays(int year, int month, bool useCache = true)
    {
        // 何年も昔のデータは不要なので、月ごとに保存します。
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

    #endregion Holidays

    #region Daily

    public async Task<List<ReservationDailySlotDto>> GetOrFetchDailySlots(int year, int month, bool useCache = true)
    {

        var key = $"dailySlots_{year:0000}{month:00}";
        if (useCache && this._cache.TryGetValue<List<ReservationDailySlotDto>>(
                key, out var slots))
        {
            return slots ?? [];
        }

        slots = await this._dailyGrpcService.FetchDailySlotDtosAsync(year, month);

        this._cache.Set(key, slots, new MemoryCacheEntryOptions
        {
            // スロット情報は毎日変更があるが、リアルタイムの変更もあるので程々とする
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        });

        return slots;
    }

    // TODO: その月が含まれるスロットを全部取得する
    // そのデータを基に、Dictionary<date, slotId> のデータを作る
    // あとは選択した日のslotIdが変われば、再描画をすればよい


    public async Task<List<string>> GetOrFetchDailySlotStrings(DateOnly date, int? floorId, bool useCache = true)
    {

        var key = $"dailySlotStrings_{date:yyyyMMdd}_{floorId ?? 0}";
        if (useCache && this._cache.TryGetValue<List<string>>(
                key, out var slotStrings))
        {
            return slotStrings ?? [];
        }

        var slots = await this.GetOrFetchDailySlots(date.Year, date.Month, useCache);
        slotStrings = this.CreateDailySlotStrings(slots, date, floorId);

        this._cache.Set(key, slotStrings, new MemoryCacheEntryOptions
        {
            // スロット情報は毎日変更があるが、リアルタイムの変更もあるので程々とする
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        });

        return slotStrings;
    }

    /// <summary>
    /// 対象日のスロットのリストを作成します。
    /// </summary>
    private List<string> CreateDailySlotStrings(List<ReservationDailySlotDto> definitions, DateOnly date, int? floorId)
    {
        var def = definitions
            // 範囲内にしぼる
            .Where(x => x.StartDate <= date && date <= x.EndDate)
            // 全対象(FloorId == 0) と 条件なしあり(floorId == null || x.FloorId == floorId) にしぼる
            .Where(x => x.FloorId == 0 || (floorId == null || x.FloorId == floorId))
            // 全対象(DowCode == -1) と 条件あり(x.DowCode == date.DayOfWeek) にしぼる
            .Where(x => x.DowCode == (int)DowCode.None || x.DowCode == (int)date.DayOfWeek)
            // 日付の新しいものを優先する
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefault();

        if (def is null)
        {
            return [];
        }

        // 定義されている最大の slot の一覧を作成する
        var cols = new HashSet<string>();

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

        return cols.OrderBy(x => x).ToList();
    }

    public async Task<List<ReservationDailyNoteDto>> GetOrFetchDailyNotes(DateOnly date, int? orFloorId, bool useCache = true)
    {

        var key = $"dailyNotes_{date:yyyyMMdd}_{orFloorId ?? 0}";
        if (useCache && this._cache.TryGetValue<List<ReservationDailyNoteDto>>(
                key, out var notes))
        {
            return notes ?? [];
        }

        notes = await this._dailyGrpcService.FetchDailyNoteDtosAsync(date, orFloorId);

        this._cache.Set(key, notes, new MemoryCacheEntryOptions
        {
            // スロット情報は毎日変更があるが、リアルタイムの変更もあるので程々とする
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        });

        return notes;
    }

    public async Task<List<ReservationDailyBookingDto>> GetOrFetchDailyBookings(DateOnly date, int? orFloorId, bool useCache = true)
    {

        var key = $"dailyBookings_{date:yyyyMMdd}_{orFloorId ?? 0}";
        if (useCache && this._cache.TryGetValue<List<ReservationDailyBookingDto>>(
                key, out var bookings))
        {
            return bookings ?? [];
        }

        bookings = await this._dailyGrpcService.FetchDailyBookingDtosAsync(date, orFloorId);

        this._cache.Set(key, bookings, new MemoryCacheEntryOptions
        {
            // スロット情報は毎日変更があるが、リアルタイムの変更もあるので程々とする
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        });

        return bookings;
    }

    #endregion Daily

    #region Equipments

    public async Task<List<ReservationEquipmentSlotDto>> GetOrFetchEquipSlots(int year, int month, int? orEquipId, bool useCache = true)
    {

        var key = $"equipmentSlots_{year:0000}{month:00}_{orEquipId ?? 0}";
        if (useCache && this._cache.TryGetValue<List<ReservationEquipmentSlotDto>>(
                key, out var slots))
        {
            return slots ?? [];
        }

        slots = await this._equipGrpcService.FetchEquipmentSlotDtosAsync(year, month, orEquipId);

        this._cache.Set(key, slots, new MemoryCacheEntryOptions
        {
            // スロット情報は毎日変更があるが、リアルタイムの変更もあるので程々とする
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        });

        return slots;
    }

    public async Task<List<string>> GetOrFetchEquipSlotStrings(int year, int month, int? equipId, bool useCache = true)
    {
        var key = $"equipmentSlotStrings_{year:0000}{month:00}_{equipId ?? 0}";
        if (useCache && this._cache.TryGetValue<List<string>>(
                key, out var slotStrings))
        {
            return slotStrings ?? [];
        }

        var slots = await this.GetOrFetchEquipSlots(year, month, equipId, useCache);
        slotStrings = this.CreateEquipSlotStrings(slots, equipId);

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
    private List<string> CreateEquipSlotStrings(List<ReservationEquipmentSlotDto> definitions, int? equipId)
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

    public async Task<List<ReservationEquipmentBookingDto>> GetOrFetchEquipBookings(string monthString, int equipId, bool useCache = true)
    {
        var date = monthString.ToDateOrToday();
        var year = date.Year;
        var month = date.Month;
        return await this.GetOrFetchEquipBookings(year, month, equipId, useCache);
    }

    public async Task<List<ReservationEquipmentBookingDto>> GetOrFetchEquipBookings(int year, int month, int equipId, bool useCache = true)
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

    #endregion Equipments
}
