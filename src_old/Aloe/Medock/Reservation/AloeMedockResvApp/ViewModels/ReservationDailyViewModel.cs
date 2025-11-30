using R3;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using MaterialDesignThemes.Wpf;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using ObservableCollections;
using Aloe.Common.AloeCoreLib.Mvvm;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;


public class ReservationDailyViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// 選択中のフロア番号です。
    /// </summary>
    /// <remarks>
    /// OneWayToSource なので、通知の仕組みは不要。
    /// (R3.ReactiveProperty は INotifyPropertyChanged を実装しません。)
    /// 変更されたとき、フロアID、フロア名、を更新します。
    /// </remarks>
    public ReactiveProperty<string> FloorCode { get; set; } = new();

    /// <summary>
    /// フロアIDです。
    /// </summary>
    /// <remarks>
    /// 検索のときに参照します。
    /// </remarks>
    private int? _floorId = null;

    /// <summary>
    /// 選択中のフロア名です。
    /// </summary>
    /// <remarks>
    /// View側でバインドして値を表示します。
    /// (R3.BindableReactiveProperty は INotifyPropertyChanged を実装しています。)
    /// </remarks>
    public BindableReactiveProperty<string> FloorName { get; set; } = new();

    /// <summary>
    /// 選択中の日付です。
    /// </summary>
    /// <remarks>
    /// 検索のときに参照します。
    /// </remarks>
    public BindableReactiveProperty<DateOnly> SelectedDate { get; set; } = new (DateHelper.GetToday());

    /// <summary>
    /// 現在選択中のタブです。
    /// </summary>
    /// <remarks>
    /// 検索のときに参照します。
    /// OneWayToSource なので、通知の仕組みは不要。
    /// イベントも発火させないので、ReactiveProperty も不要。
    /// </remarks>
    public int SelectedTabIndexInput { get; set; } = -1;

    private readonly ObservableList<ReservationDailyNoteDto> _reservationDailyNotes = new();

    public NotifyCollectionChangedSynchronizedViewList<ReservationDailyNoteDto> ReservationDailyNotesView { get; }

    private readonly ObservableList<ReservationDailyBookingDto> _reservationDailyBookings = new();

    public NotifyCollectionChangedSynchronizedViewList<ReservationDailyBookingDto> ReservationDailyBookings { get; }

    public SnackbarMessageQueue SnackbarMessageQueue { get; } = new();

    private readonly ILogger _logger;

    private readonly ReservationCacheService _cache;

    public ReservationDailyViewModel(
        ILogger<ReservationDailyViewModel> logger,
        ReservationCacheService cache,
        InformationBarViewModel informationBarVm,
        FunctionBarViewModel functionBarVm)
    {
        this._logger = logger;
        this._cache = cache;

        this.InformationBarVm = informationBarVm;

        #region Function

        var functions = this.CreateFunctions();
        functionBarVm.InitializeFunctions(functions);
        this.FunctionBarVm = functionBarVm;

        #endregion Function

        var d = R3.Disposable.CreateBuilder();

        this.FloorCode
            .Subscribe(this.LoadFloor)
            .AddTo(ref d);

        this.SelectedDate
            .Subscribe(this.ExecuteSearchCommandWrapper)
            .AddTo(ref d);

        this.ReservationDailyNotesView = this._reservationDailyNotes
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .AddTo(ref d);

        this.ReservationDailyBookings = this._reservationDailyBookings
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .AddTo(ref d);

        this.Disposable = d.Build();
    }

    public required InformationBarViewModel InformationBarVm { get; set; }

    #region Function

    public FunctionBarViewModel FunctionBarVm { get; set; }

    private Dictionary<string, Function> CreateFunctions()
    {
        var functions = new List<Function>
            {
                //new(FunctionKey.F2, "仮予約", this.ExecuteReserveTentativeCommand),
                //new(FunctionKey.F3, "予約", this.ExecuteReserveCommand),

                new(FunctionKey.F5, "検索", this.ExecuteSearchCommand),

                new(FunctionKey.F9, "前日へ", this.ExecutePrevDaySearchCommand),
                new(FunctionKey.F10, "次日へ", this.ExecuteNextDaySearchCommand),
                new(FunctionKey.F11, "今日", this.ExecuteTodaySearchCommand),
                new(FunctionKey.F12, "閉じる", () => this.FunctionBarVm.ExecuteCloseCommand<ReservationDailyWindow>()),
            }
            .ToDictionary(x => x.Key);

        return functions;
    }

    #endregion Function

    /// <summary>
    /// フロア情報をロードします。
    /// </summary>
    public async void LoadFloor(string code)
    {
        try
        {
            var floors = await this._cache.GetOrFetchFloors();
            var floor = floors.FirstOrDefault(x => x.FloorCode == code);

            if (floor == null)
            {
                this._floorId = null;
                this.FloorName.Value = "";
            }
            else
            {
                this._floorId = floor.FloorId;
                this.FloorName.Value = floor.FloorName;
            }
        }
        catch (Exception ex)
        {
            this.SnackbarMessageQueue.ShowMessage($"フロアのロードに失敗しました。({ex.Message})");
            this._logger.LogError(ex, ex.ToString());
        }
    }

    private async void ExecuteSearchCommandWrapper(DateOnly _)
    {
        try
        {
            await this.ExecuteSearchCommand();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
        }
    }

    /// <summary>
    /// 検索を実施時、アクティブなタブの内容を更新します。
    /// </summary>
    public async Task ExecuteSearchCommand()
    {
        try
        {
            this.InformationBarVm.StartProgress();
            var isAlt = this.FunctionBarVm.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                await this.SearchAsync();
            }
        }
        catch (Exception ex)
        {
            this.SnackbarMessageQueue.ShowMessage($"検索に失敗しました。({ex.Message})");
            this._logger.LogError(ex, ex.ToString());
        }
        finally
        {
            this.InformationBarVm.StopProgress();
        }
    }

    public async Task SearchAsync()
    {
        try
        {
            var tabIndex = this.SelectedTabIndexInput;
            if (tabIndex < 0)
            {
                // バインド前は回避
                return;
            }

            var date = this.SelectedDate.Value;
            if (date == DateOnly.MinValue)
            {
                // 未入力なら回避
                return;
            }

            // TODO: Notes
            // ReservationDailyNotes

            // 0: List
            // 1: カテゴリ別
            // 2: ルーム別
            var orFloorId = this._floorId;

            var notes = await this._cache.GetOrFetchDailyNotes(date, orFloorId);
            this._reservationDailyNotes.Clear();
            this._reservationDailyNotes.AddRange(notes);

            // TODO: List
            var bookings = await this._cache.GetOrFetchDailyBookings(date, orFloorId);
            this._reservationDailyBookings.Clear();
            this._reservationDailyBookings.AddRange(bookings);

            // TODO: Cat
            // TODO: Room
            //var endDate = startMonth.ToMonthEndDateOrCurrentMonth();
            //await tabItem.RefreshFuncAsync.Invoke(endDate, tabItem.EquipId);
        }
        catch (Exception ex)
        {
            this.SnackbarMessageQueue.ShowMessage($"検索に失敗しました。({ex.Message})");
            this._logger.LogError(ex, ex.ToString());
        }
    }

    public async Task ExecutePrevDaySearchCommand()
    {
        try
        {
            this.InformationBarVm.StartProgress();
            var isAlt = this.FunctionBarVm.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                await this.FunctionBarVm.ExecutePrevDateCommand(this.SelectedDate);
                await this.SearchAsync();
            }
        }
        catch (Exception ex)
        {
            this.SnackbarMessageQueue.ShowMessage($"検索に失敗しました。({ex.Message})");
            this._logger.LogError(ex, ex.ToString());
        }
        finally
        {
            this.InformationBarVm.StopProgress();
        }
    }


    public async Task ExecuteNextDaySearchCommand()
    {
        try
        {
            this.InformationBarVm.StartProgress();
            var isAlt = this.FunctionBarVm.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                await this.FunctionBarVm.ExecuteNextDateCommand(this.SelectedDate);
                await this.SearchAsync();
            }
        }
        catch (Exception ex)
        {
            this.SnackbarMessageQueue.ShowMessage($"検索に失敗しました。({ex.Message})");
            this._logger.LogError(ex, ex.ToString());
        }
        finally
        {
            this.InformationBarVm.StopProgress();
        }
    }


    public async Task ExecuteTodaySearchCommand()
    {
        try
        {
            this.InformationBarVm.StartProgress();
            var isAlt = this.FunctionBarVm.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                await this.FunctionBarVm.ExecuteSetTodayCommand(this.SelectedDate);
                await this.SearchAsync();
            }
        }
        catch (Exception ex)
        {
            this.SnackbarMessageQueue.ShowMessage($"検索に失敗しました。({ex.Message})");
            this._logger.LogError(ex, ex.ToString());
        }
        finally
        {
            this.InformationBarVm.StopProgress();
        }
    }
}
