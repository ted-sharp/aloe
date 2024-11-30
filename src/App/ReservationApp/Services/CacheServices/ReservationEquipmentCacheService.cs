using AloeReservationGrid.Lib.CoreLib.Util;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.App.ReservationApp.Services.CacheServices;

public class ReservationEquipmentCacheService
{

    private readonly ILogger _logger;
    private readonly IMemoryCache _cache;
    private readonly IReservationEquipmentGrpcService _equipGrpcService;

    public ReservationEquipmentCacheService(
        ILogger<ReservationEquipmentCacheService> logger,
        IMemoryCache cache,
        IReservationEquipmentGrpcService equipGrpcService)
    {
        this._logger = logger;
        this._cache = cache;
        this._equipGrpcService = equipGrpcService;
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

        // マスター情報はまず変更がないのでしばらく保持しておく
        var expiration = TimeSpan.FromHours(1);
        this._cache.Set(key, equipments, expiration);

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

        // スロット情報は毎日変更があるが、リアルタイムの変更もあるので程々とする
        var expiration = TimeSpan.FromMinutes(10);
        this._cache.Set(key, slots, expiration);

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

        // スロット情報は毎日変更があるが、リアルタイムの変更もあるので程々とする
        var expiration = TimeSpan.FromMinutes(10);
        this._cache.Set(key, slots, expiration);

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

        return cols.OrderBy(x => x).ToList(); ;
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

        // リアルタイムの変更もあるので数分とする
        var expiration = TimeSpan.FromMinutes(3);
        this._cache.Set(key, bookings, expiration);

        return bookings;
    }

    public async Task<DataTable> GetOrFetchBookingsTable(int year, int month, int equipId, bool useCache = true)
    {
        var key = $"equipmentBookingsTable_{year:0000}{month:00}_{equipId}";
        if (useCache && this._cache.TryGetValue<DataTable>(
                key, out var bookingsTable))
        {
            return bookingsTable ?? new();
        }

        // 列: スロット(記号、氏名、備考の繰り返し)
        // 行ヘッダー: 日付、曜日、合計
        bookingsTable = new DataTable();

        try
        {
            bookingsTable.BeginInit();

            var slots = await this.GetOrFetchSlotStrings(year, month, equipId, useCache);
            foreach (var slot in slots)
            {
                bookingsTable.Columns.Add(new DataColumn($"{slot}_symbol"));
                bookingsTable.Columns.Add(new DataColumn($"{slot}_pt"));
                bookingsTable.Columns.Add(new DataColumn($"{slot}_remark"));
            }

            var bookings = await this._equipGrpcService.FetchEquipmentBookingDtosAsync(year, month, equipId);
            var days = new DateTime(year, month, 1).AddMonths(1).AddDays(-1).Day;
            for (var i = 1; i <= days; i++)
            {
                // TODO: 日付毎に作っていく
                //bookings.FindAll(x => x.BkgDate )

                // TODO: スロットに当てはめる

                var row = bookingsTable.NewRow();

                bookingsTable.Rows.Add(row);
            }

            // リアルタイムの変更もあるので数分とする
            var expiration = TimeSpan.FromMinutes(1);
            this._cache.Set(key, bookingsTable, expiration);
        }
        finally
        {
            bookingsTable.EndInit();
        }

        return bookingsTable;
    }

}
