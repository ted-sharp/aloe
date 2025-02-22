using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using System.Collections.ObjectModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Windows.Input;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Reactive.Bindings.TinyLinq;
using System.Reactive.Linq;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Windows.Documents;
using Microsoft.VisualBasic.CompilerServices;
using System.DirectoryServices.ActiveDirectory;
using Aloe.Common.AloeCoreLib.Client.Mvvm;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using System.Collections.Concurrent;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

/// <summary>
/// DataGrid に表示するためのデータと、割り当てられなかった残りのデータです。
/// キャッシュするときにひとまとめで扱います。
/// </summary>
public record BookingData(List<BookingRow> Rows, List<ReservationEquipmentBookingDto> Overflows);
public record BookingData2(List<RecyclingBookingRow> Rows, List<ReservationEquipmentBookingDto> Overflows);

/// <summary>
/// DataGrid に表示するためのローデータです。
/// </summary>
public record BookingRow(
    string Slot,
    string SlotDisplay,
    Dictionary<DateOnly, BookingCell> DateCells);

/// <summary>
/// DataGrid に表示するためのセルデータです。
/// </summary>
public record BookingCell(
    string Symbol,
    string Name,
    string Remark,
    string Error)
{
    public BookingCell() : this("", "", "", "") { }

    public string DisplayText { get; set; } = $"{Symbol} | {Name} | {Remark}";
}

public record RecyclingBookingRow(
    string Slot,
    string SlotDisplay,
    Dictionary<int /* Day */, BookingCell> DateCells);

public class ReservationEquipTabItemViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// TabItem のヘッダーに表示します。
    /// </summary>
    public required int EquipId { get; init; }

    /// <summary>
    /// TabItem のヘッダーに表示します。
    /// </summary>
    public required string EquipName { get; init; } = String.Empty;

    /// <summary>
    /// 祝日のデータです。
    /// </summary>
    public Dictionary<DateOnly, HolidayDto> Holidays { get; set; } = [];

    /// <summary>
    /// 対象年月設備毎の予約データです。
    /// </summary>
    public List<BookingRow> Rows { get; set; } = [];
    public List<RecyclingBookingRow> RecyclingRows { get; set; } = [];

    /// <summary>
    /// 対象年月設備でスロットに割り振れなかった残りの予約データです。
    /// </summary>
    public List<ReservationEquipmentBookingDto> Overflows { get; set; } = [];

    /// <summary>
    /// 画面側の更新メソッドを登録します。
    /// </summary>
    public Func<DateOnly /* monthEndDate */, int /* equipId */, Task>? RefreshFuncAsync { get; set; }

    private readonly ReservationEquipmentCacheService _cache;
    private readonly ILogger _logger;

    /// <summary>
    /// ILogger で受けるので DI 用ではなく、手動で作ります。
    /// </summary>
    public ReservationEquipTabItemViewModel(
        ILogger logger,
        ReservationEquipmentCacheService cache)
    {
        this._logger = logger;
        this._cache = cache;
    }

    public async Task LoadHolidaysAsync(DateOnly monthEndDate)
    {
        var year = monthEndDate.Year;
        var month = monthEndDate.Month;

        // キャッシュがあるなら使う
        var holidays = await this._cache.GetOrFetchHolidays(year, month);
        if (holidays is not null && holidays is { Count: > 0 })
        {
            this.Holidays = holidays.ToDictionary(x => x.HolidayDate);
            return;
        }
    }

    public async Task LoadAsync(DateOnly monthEndDate, int equipId)
    {
        var year = monthEndDate.Year;
        var month = monthEndDate.Month;

        // キャッシュがあるなら使う
        var data = this._cache.GetBookingData(year, month, equipId);
        if (data is not null && data.Rows is { Count: > 0 })
        {
            this.Rows = data.Rows;
            this.Overflows = data.Overflows;
            return;
        }

        var rows = new List<BookingRow>();
        var slots = await this._cache.GetOrFetchSlotStrings(year, month, equipId);
        var monthBookings = await this._cache.GetOrFetchBookings(year, month, equipId);

        // スロット分の行を生成する
        foreach (var slot in slots)
        {
            var slotDisplay = slot.Contains('(') ? "" : slot;
            var slotRowData = new BookingRow(slot, slotDisplay, []);

            for (var day = 1; day <= monthEndDate.Day; day++)
            {
                var date = new DateOnly(year, month, day);
                var booking = monthBookings.Find(x => x.BkgDate == date && x.Slot == slot && x.EquipId == equipId);
                if (booking == null)
                {
                    // 空
                    slotRowData.DateCells.Add(date, new());
                }
                else
                {
                    // 予定あり
                    slotRowData.DateCells.Add(date, new(booking.BkgSymbolText, booking.PtId.ToString(), booking.BkgRemarkText, ""));
                    monthBookings.Remove(booking);
                }
                // TODO: スロットが使えなくなっている箇所はどうする？
                // 入力できないことを示す何かが必要
                // 土日祝は？
            }

            rows.Add(slotRowData);
        }

        this.Rows.Clear();
        this.Rows.AddRange(rows);
        this.Overflows.Clear();

        monthBookings = monthBookings
            .OrderBy(x => x.Slot)
            .ThenBy(x => x.BkgAt)
            .ToList();

        this.Overflows.AddRange(monthBookings);

        data = new BookingData(rows, monthBookings);
        this._cache.SetBookingData(year, month, equipId, data);
    }
    public async Task LoadAsync2(DateTime monthEndDate, int equipId)
    {
        var year = monthEndDate.Year;
        var month = monthEndDate.Month;

        var slots = await this._cache.GetOrFetchSlotStrings(year, month, equipId);

        // スロット分ないときは作る
        if (this.RecyclingRows.Count != slots.Count)
        {
            this.RecyclingRows.Clear();
            foreach (var slot in slots)
            {
                var slotDisplay = slot.Contains('(') ? "" : slot;
                this.RecyclingRows.Add(new(slot, slotDisplay, []));
            }
        }

        var monthBookings = await this._cache.GetOrFetchBookings(year, month, equipId);

        foreach (var slot in slots)
        {
            var row = this.RecyclingRows.Find(x => x.Slot == slot);
            if (row is null)
            {
                continue;
            }

            for (var day = 1; day <= 31; day++)
            {
                if (day <= monthEndDate.Day)
                {
                    var date = new DateOnly(year, month, day);
                    var booking = monthBookings.Find(x => x.BkgDate == date && x.Slot == slot && x.EquipId == equipId);
                    if (booking == null)
                    {
                        // 空
                        row.DateCells[day] = new();
                    }
                    else
                    {
                        // 予定あり
                        row.DateCells[day] = new(booking.BkgSymbolText, booking.PtId.ToString(), booking.BkgRemarkText, "");
                        monthBookings.Remove(booking);
                    }
                }
                else
                {
                    row.DateCells[day] = new();
                }
            }
        }

        this.Overflows.Clear();

        monthBookings = monthBookings
            .OrderBy(x => x.Slot)
            .ThenBy(x => x.BkgAt)
            .ToList();

        this.Overflows.AddRange(monthBookings);
    }
}
