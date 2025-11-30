using R3;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using System.Data;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using Microsoft.Extensions.Caching.Memory;
using Aloe.Common.AloeCoreLib.Mvvm;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;


public class ReservationMainViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public Action<DateOnly>? RefreshAction { get; set; }

    // TODO: バインドしてるからいらないかも
    public Action? RefreshDataAction { get; set; }

    public ReactiveProperty<DateOnly> StartDate { get; set; } = new(DateHelper.GetToday());

    public ReactiveProperty<int> OffsetDayCount { get; set; } = new(31);

    public ReactiveProperty<string> FloorId1 { get; set; } = new("1");
    public ReactiveProperty<string> FloorName1 { get; set; } = new("Floor1");
    public ReadOnlyReactiveProperty<string> VerticalFloorName1 { get; }
    public ReactiveProperty<string> FloorId2 { get; set; } = new("2");
    public ReactiveProperty<string> FloorName2 { get; set; } = new("Floor2");
    public ReadOnlyReactiveProperty<string> VerticalFloorName2 { get; }

    public ReactiveProperty<bool?> IsAutoRefresh { get; set; } = new(true);

    public ReactiveProperty<string> SecondsToRefresh { get; set; } = new("60");

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

        var d = R3.Disposable.CreateBuilder();

        #region SearchCondition

        this.StartDate
            .Subscribe(x =>
            {
                this.RefreshAction?.Invoke(x);
                this.RefreshDataAction?.Invoke();
            })
            .AddTo(ref d);

        this.FloorId1
            .Subscribe(x =>
            {
                // TODO: DBから取得
                this.FloorName1.Value = $"test {x}";
                this.RefreshDataAction?.Invoke();
            })
            .AddTo(ref d);

        this.FloorId2
            .Subscribe(x =>
            {
                // TODO: DBから取得
                this.FloorName2.Value = $"test {x}";
                this.RefreshDataAction?.Invoke();
            })
            .AddTo(ref d);

        this.VerticalFloorName1 = this.FloorName1
            .Select(x => String.Join(Environment.NewLine, x.ToCharArray()))
            .ToReadOnlyReactiveProperty<string>()
            .AddTo(ref d);

        this.VerticalFloorName2 = this.FloorName2
            .Select(x => String.Join(Environment.NewLine, x.ToCharArray()))
            .ToReadOnlyReactiveProperty<string>()
            .AddTo(ref d);

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

        this.Disposable = d.Build();
    }

    #region Function

    public FunctionBarViewModel FunctionBar { get; set; }

    private Dictionary<string, Function> CreateFunctions()
    {
        var functions = new List<Function>
        {
            new(FunctionKey.Esc, "中止", () =>
            {
                this._logger.LogInformation("ESC");
                return Task.CompletedTask;
            }),

            new(FunctionKey.F4, "クリア", this.ExecuteClearCommand),

            new(FunctionKey.F5, "検索", this.ExecuteSearchCommand),
            new(FunctionKey.F6, "設備予約", () => this.FunctionBar.ExecuteOpenCommand<ReservationEquipMonthlyWindow, ReservationMainWindow>()),
            new(FunctionKey.F7, "日別予約", () => this.FunctionBar.ExecuteOpenCommand<ReservationDailyWindow, ReservationMainWindow>()),
            new(FunctionKey.F8, "団体患者", () => this.FunctionBar.ExecuteOpenCommand<MaintenanceWindow, ReservationMainWindow>()),

            new(FunctionKey.F9, "前へ", () => this.FunctionBar.ExecutePrevMonthCommand(this.StartDate)),
            new(FunctionKey.F10, "次へ", () => this.FunctionBar.ExecuteNextMonthCommand(this.StartDate)),
            new(FunctionKey.F11, "今日", () => this.FunctionBar.ExecuteSetTodayCommand(this.StartDate)),
            new(FunctionKey.F12, "設定", () => this.FunctionBar.ExecuteOpenCommand<MaintenanceWindow, ReservationMainWindow>()),

            new(FunctionKey.AltF1, "Alt F1", () =>
            {
                this._logger.LogInformation("F1");
                return Task.CompletedTask;
            }),
            new(FunctionKey.AltF12, "Alt F12", () =>
            {
                this._logger.LogInformation("F12");
                return Task.CompletedTask;
            }),
        }.ToDictionary(x => x.Key);

        return functions;
    }

    private Task ExecuteSearchCommand()
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
            return Task.Delay(3000);
        }
        finally
        {
            this.FunctionBar.SharedCanExecute.Value = true;
        }
    }

    private Task ExecuteClearCommand()
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

            return Task.CompletedTask;
        }
        finally
        {
            this.FunctionBar.SharedCanExecute.Value = true;
        }
    }

    private Task ExecuteReloadCommand()
    {
        try
        {
            this.FunctionBar.SharedCanExecute.Value = false;
            var isAlt = this.FunctionBar.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                return this.LoadSchedulesAsync();
            }
        }
        finally
        {
            this.FunctionBar.SharedCanExecute.Value = true;
        }

        return Task.CompletedTask;
    }

    public Task LoadSchedulesAsync()
    {
        // TODO: データを作成する
        // 横が1ヶ月間の日付
        // 縦がRoom
        // 内容は件数
        //this.Schedules.

        return Task.Delay(3000);
    }

    #endregion Function

}
