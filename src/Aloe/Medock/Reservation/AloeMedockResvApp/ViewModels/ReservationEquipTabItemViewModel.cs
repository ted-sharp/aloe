using Aloe.Common.AloeCoreLib.Mvvm;
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
using Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

// TODO: 表示用Slot文字列を用意して、(2)以降は空欄にしたい
/// <summary>
/// DataGrid に表示するためのローデータです。
/// </summary>
public record SlotRowData(string Slot, Dictionary<DateTime, SlotCellData> DateCells);

// TODO: エラーテキストもあった方が良い
/// <summary>
/// DataGrid に表示するためのセルデータです。
/// </summary>
public record SlotCellData(string Symbol, string Name, string Remark)
{
    public SlotCellData() : this("", "", "") { }
}

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
    /// 対象年です。
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 対象月です。
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// 対象年月設備毎の予約データです。
    /// </summary>
    public List<SlotRowData> Rows { get; set; } = [];

    /// <summary>
    /// 画面側の更新メソッドを登録します。
    /// </summary>
    public Action? RefreshAction { get; set; }

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

    public async Task LoadAsync()
    {
        var year = this.Year;
        var month = this.Month;
        var equipId = this.EquipId;

        // キャッシュがあるなら使う
        var rows = this._cache.GetSlotRowDataList(year, month, equipId);
        if (rows is { Count: > 0 })
        {
            this.Rows = rows;
            return;
        }

        var slots = await this._cache.GetOrFetchSlotStrings(year, month, equipId);
        var monthBookings = await this._cache.GetOrFetchBookings(year, month, equipId);

        var lastDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);

        rows = [];

        // スロット分の行を生成する
        foreach (var slot in slots)
        {
            var slotRowData = new SlotRowData(slot, []);

            for (var day = 1; day <= lastDate.Day; day++)
            {
                var date = new DateTime(year, month, day);
                var booking = monthBookings.Find(x => x.BkgDate == date && x.Slot == slot && x.EquipId == equipId);
                if (booking == null)
                {
                    slotRowData.DateCells.Add(date, new());
                }
                else
                {
                    slotRowData.DateCells.Add(date, new(booking.BkgSymbolText, booking.PtId.ToString(), booking.BkgRemarkText));
                }
            }

            rows.Add(slotRowData);
        }

        this.Rows = rows;
        this._cache.SetSlotRowDataList(year, month, equipId, rows);
    }
}
