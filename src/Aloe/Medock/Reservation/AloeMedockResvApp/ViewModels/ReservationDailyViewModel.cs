using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
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
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MaterialDesignThemes.Wpf;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;


public class ReservationDailyViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// 検索のときに参照します。
    /// </summary>
    public ReactivePropertySlim<string> FloorCode { get; set; } = new();

    private int? _floorId = null;

    /// <summary>
    /// Floor Code 入力時にフロア名を表示します。
    /// </summary>
    public ReactivePropertySlim<string> FloorName { get; set; } = new();

    /// <summary>
    /// 検索のときに参照します。
    /// </summary>
    public ReactivePropertySlim<DateOnly> SelectedDate { get; set; } = new (DateOnlyHelper.GetToday());

    /// <summary>
    /// 検索のときに参照します。
    /// </summary>
    /// <remarks>
    /// OneWayToSource なので、通知の仕組みは不要。
    /// イベントも発火させないので、ReactiveProperty も不要。
    /// </remarks>
    public int SelectedTabIndexInput { get; set; } = -1;

    public ReactiveCollection<ReservationDailyNoteDto> ReservationDailyNotes { get; set; } = new();

    public ReactiveCollection<ReservationDailyBookingDto> ReservationDailyBookings { get; set; } = new();

    public ReactiveCollection<ReservationDailyNoteDto> ReservationDailyNotes2 { get; set; } = new();

    public ReactiveCollection<ReservationDailyNoteDto> ReservationDailyNotes3 { get; set; } = new();

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


        this.FloorCode
            .Subscribe(this.LoadFloor)
            .AddTo(this.Disposables);

        //this.SelectedDate
        //    .SubscribeAsync(this.ExecuteSearchCommand, this._logger)
        //    .AddTo(this.Disposables);
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
            this.ReservationDailyNotes.Clear();
            this.ReservationDailyNotes.AddRangeOnScheduler(notes);

            // TODO: List
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
