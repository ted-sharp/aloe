using R3;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Common.AloeCoreLib.Mvvm;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

/// <summary>
/// Grid に表示するためのセルデータです。
/// </summary>
public record DailyBookingCell(
    int Row,
    int RowSpan,
    int Column,
    string Text)
{
    public DailyBookingCell() : this(0, 0, 0, "") { }
}

public class ReservationDailyGridViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// 対象日の予約データです。
    /// </summary>
    public List<DailyBookingCell> Cells { get; set; } = [];

    /// <summary>
    /// 画面側の更新メソッドを登録します。
    /// </summary>
    public Func<int /* floorId */, DateOnly /* bkgDate */, Task>? RefreshFuncAsync { get; set; }

    private readonly ReservationCacheService _cache;
    private readonly ILogger _logger;

    /// <summary>
    /// ILogger で受けるので DI 用ではなく、手動で作ります。
    /// </summary>
    public ReservationDailyGridViewModel(
        ILogger logger,
        ReservationCacheService cache)
    {
        this._logger = logger;
        this._cache = cache;
    }

    public async Task LoadAsync(int floorId, DateOnly bkgDate)
    {
        await Task.CompletedTask;

        return;

        //var slots = await this._cache.GetOrFetchDaliySlotStrings(year, month, equipId);

        //// スロット分ないときは作る
        //if (this.RecyclingRows.Count != slots.Count)
        //{
        //    this.RecyclingRows.Clear();
        //    foreach (var slot in slots)
        //    {
        //        var slotDisplay = slot.Contains('(') ? "" : slot;
        //        this.RecyclingRows.Add(new(slot, slotDisplay, []));
        //    }
        //}

        //var monthBookings = await this._cache.GetOrFetchBookings(year, month, equipId);

        //foreach (var slot in slots)
        //{
        //    var row = this.RecyclingRows.Find(x => x.Slot == slot);
        //    if (row is null)
        //    {
        //        continue;
        //    }

        //    for (var day = 1; day <= 31; day++)
        //    {
        //        if (day <= monthEndDate.Day)
        //        {
        //            var date = new DateOnly(year, month, day);
        //            var booking = monthBookings.Find(x => x.BkgDate == date && x.Slot == slot && x.EquipId == equipId);
        //            if (booking == null)
        //            {
        //                // 空
        //                row.DateCells[day] = new();
        //            }
        //            else
        //            {
        //                // 予定あり
        //                row.DateCells[day] = new(booking.BkgSymbolText, booking.PtId.ToString(), booking.BkgRemarkText, "");
        //                monthBookings.Remove(booking);
        //            }
        //        }
        //        else
        //        {
        //            row.DateCells[day] = new();
        //        }
        //    }
        //}

        //this.Overflows.Clear();

        //monthBookings = monthBookings
        //    .OrderBy(x => x.Slot)
        //    .ThenBy(x => x.BkgAt)
        //    .ToList();

        //this.Overflows.AddRange(monthBookings);
    }
}
