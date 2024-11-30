using AloeReservationGrid.Lib.CoreLib.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloeReservationGrid.App.ReservationApp.Views.Login;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using AloeReservationGrid.App.ReservationApp.Views.Resv;
using System.Collections.ObjectModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Windows.Input;
using AloeReservationGrid.App.ReservationApp.Views.Maint;
using AloeReservationGrid.Lib.CoreLib.Util;
using AloeReservationGrid.Lib.ReservationLib.Domain.Constants;
using Reactive.Bindings.TinyLinq;
using System.Reactive.Linq;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Windows.Documents;
using Microsoft.VisualBasic.CompilerServices;
using System.DirectoryServices.ActiveDirectory;
using AloeReservationGrid.App.ReservationApp.Services.CacheServices;

namespace AloeReservationGrid.App.ReservationApp.ViewModels;


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
    /// 動的にヘッダーを作る際に参照します。
    /// </summary>
    public List<string> Slots { get; set; } = new();

    /// <summary>
    /// DataGrid に表示するデータです。
    /// 縦軸にその月の日付、横軸に記号・氏名・備考をスロットの数だけ繰り返します。
    /// </summary>
    public DataTable DataTable { get; set; } = null!;

    /// <summary>
    /// 画面側の更新メソッドを登録します。
    /// </summary>
    public Action? RefreshAction { get; set; }

    private readonly ReservationEquipmentCacheService _cache;
    private ILogger _logger;

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
        var slots = await this._cache.GetOrFetchSlotStrings(this.Year, this.Month, this.EquipId);
        this.Slots = slots;

        var table = await this._cache.GetOrFetchBookingsTable(this.Year, this.Month, this.EquipId);
        this.DataTable = table;
    }
}
