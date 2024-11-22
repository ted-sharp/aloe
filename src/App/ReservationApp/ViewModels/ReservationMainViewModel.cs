using AloeReservationGrid.Lib.CoreLib.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
using AloeReservationGrid.Lib.ReservationLib.Grpc.Services;
using Microsoft.Extensions.Caching.Memory;
using Reactive.Bindings.TinyLinq;

namespace AloeReservationGrid.App.ReservationApp.ViewModels;


public class ReservationMainViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public Action<DateTime>? RefreshAction { get; set; }

    // TODO: バインドしてるからいらないかも
    public Action? RefreshDataAction { get; set; }

    public ReactivePropertySlim<string> StartDate { get; set; } = new(DateTime.Today.ToString("yyyy.MM.dd"));

    public ReactivePropertySlim<int> OffsetDayCount { get; set; } = new(31);

    public ReactivePropertySlim<string> FloorId1 { get; set; } = new("1");
    public ReactivePropertySlim<string> FloorName1 { get; set; } = new("Floor1");
    public ReadOnlyReactivePropertySlim<string> VerticalFloorName1 { get; }
    public ReactivePropertySlim<string> FloorId2 { get; set; } = new("2");
    public ReactivePropertySlim<string> FloorName2 { get; set; } = new("Floor2");
    public ReadOnlyReactivePropertySlim<string> VerticalFloorName2 { get; }

    public ReactivePropertySlim<bool?> IsAutoRefresh { get; set; } = new(true);

    public ReactivePropertySlim<string> SecondsToRefresh { get; set; } = new("60");

    public DataTable Schedules0 { get; set; } = new();
    public DataTable Schedules1 { get; set; } = new();
    public DataTable Schedules2 { get; set; } = new();

    private readonly ILogger _logger;
    private readonly IMemoryCache _cache;

    public ReservationMainViewModel(
        ILogger<ReservationMainViewModel> logger,
        IMemoryCache cache,
        IAuthGrpcService authGrpcService,
        FunctionBarViewModel functionBar)
    {
        this._logger = logger;
        this._cache = cache;

        #region SearchCondition

        this.StartDate
            .Subscribe(x =>
            {
                this.RefreshAction?.Invoke(x.ToDateOrToday());
                this.RefreshDataAction?.Invoke();
            })
            .AddTo(this.Disposables);

        this.FloorId1
            .Subscribe(x =>
            {
                // TODO: DBから取得
                this.FloorName1.Value = $"test {x}";
                this.RefreshDataAction?.Invoke();
            })
            .AddTo(this.Disposables);

        this.FloorId2
            .Subscribe(x =>
            {
                // TODO: DBから取得
                this.FloorName2.Value = $"test {x}";
                this.RefreshDataAction?.Invoke();
            })
            .AddTo(this.Disposables);

        this.VerticalFloorName1 = this.FloorName1
            .Select(x => String.Join(Environment.NewLine, x.ToCharArray()))
            .ToReadOnlyReactivePropertySlim<string>()
            .AddTo(this.Disposables);

        this.VerticalFloorName2 = this.FloorName2
            .Select(x => String.Join(Environment.NewLine, x.ToCharArray()))
            .ToReadOnlyReactivePropertySlim<string>()
            .AddTo(this.Disposables);

        #endregion SearchCondition

        #region RefreshTimer

        // TODO: IsAutoRefresh がオンならタイマー起動

        // SecondsToRefresh を減らしていく

        // SecondsToRefresh が0になったらリフレッシュ起動

        #endregion RefreshTimer

        #region Function

        var functions = this.CreateFunctions();
        functionBar.InitializeFunctions(functions);
        this.FunctionBar = functionBar;

        #endregion Function
    }

    #region Function

    public FunctionBarViewModel FunctionBar { get; set; }

    private Dictionary<string, Function> CreateFunctions()
    {
        var functions = new List<Function>
        {
            new(FunctionKey.Esc, "中止", () => this._logger.LogInformation("ESC")),

            new(FunctionKey.F4, "クリア", this.ExecuteClearCommand),

            new(FunctionKey.F5, "検索", this.ExecuteSearchCommand),
            new(FunctionKey.F6, "設備予約", () => this.FunctionBar.ExecuteOpenCommand<ReservationEquipWindow, ReservationMainWindow>()),
            new(FunctionKey.F7, "日別予約", () => this.FunctionBar.ExecuteOpenCommand<ReservationDailyWindow, ReservationMainWindow>()),
            new(FunctionKey.F8, "団体患者", () => this.FunctionBar.ExecuteOpenCommand<MaintenanceWindow, ReservationMainWindow>()),

            new(FunctionKey.F9, "前へ", () => this.FunctionBar.ExecutePrevMonthCommand(this.StartDate)),
            new(FunctionKey.F10, "次へ", () => this.FunctionBar.ExecuteNextMonthCommand(this.StartDate)),
            new(FunctionKey.F11, "今日", () => this.FunctionBar.ExecuteSetTodayCommand(this.StartDate)),
            new(FunctionKey.F12, "設定", () => this.FunctionBar.ExecuteOpenCommand<MaintenanceWindow, ReservationMainWindow>()),

            new(FunctionKey.AltF1, "Alt F1", () => this._logger.LogInformation("F1")),
            new(FunctionKey.AltF12, "Alt F12", () => this._logger.LogInformation("F12")),
        }.ToDictionary(x => x.Key);

        return functions;
    }

    private async void ExecuteSearchCommand()
    {
        try
        {
            this.FunctionBar.SharedCanExecute.Value = false;
            var isAlt = this.FunctionBar.IsAltKeyPressed.Value;
            if (isAlt)
            {
                this._logger.LogInformation("F3 with alt");
            }
            else
            {
                this._logger.LogInformation("F3");
            }
            await Task.Delay(3000);
        }
        finally
        {
            this.FunctionBar.SharedCanExecute.Value = true;
        }
    }

    private void ExecuteClearCommand()
    {
        try
        {
            this.FunctionBar.SharedCanExecute.Value = false;
            var isAlt = this.FunctionBar.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                this.Schedules0.Clear();
                this.Schedules1.Clear();
                this.Schedules2.Clear();

                // TODO: 検索条件のクリア
                // 色々消したら Subscribe してるのが動くので、どうするか・・・？
                // デフォルト値を入れる(変更ないなら何もしない)
                // 今日を入れる(変更ないなら何もしない)
            }
        }
        finally
        {
            this.FunctionBar.SharedCanExecute.Value = true;
        }
    }

    private async void ExecuteReloadCommand()
    {
        try
        {
            this.FunctionBar.SharedCanExecute.Value = false;
            var isAlt = this.FunctionBar.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                await this.LoadSchedulesAsync();
            }
        }
        finally
        {
            this.FunctionBar.SharedCanExecute.Value = true;
        }
    }

    public async Task LoadSchedulesAsync()
    {
        // TODO: データを作成する
        // 横が1ヶ月間の日付
        // 縦がRoom
        // 内容は件数
        //this.Schedules.

        await Task.Delay(3000);
    }

    #endregion Function

}
