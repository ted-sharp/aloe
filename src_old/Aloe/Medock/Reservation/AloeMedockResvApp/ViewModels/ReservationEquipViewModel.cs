using R3;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using System.Collections.ObjectModel;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using MaterialDesignThemes.Wpf;
using Aloe.Common.AloeCoreLib.Mvvm;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;


public class ReservationEquipViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// 検索のときに参照します。
    /// </summary>
    public BindableReactiveProperty<DateOnly> StartMonth { get; set; } = new(DateHelper.GetToday());

    public ObservableCollection<ReservationEquipTabItemViewModel> ReservationEquipTabItems { get; set; } = new([]);

    /// <summary>
    /// 検索のときに参照します。
    /// </summary>
    /// <remarks>
    /// OneWayToSource なので、通知の仕組みは不要。
    /// イベントも発火させないので、ReactiveProperty も不要。
    /// </remarks>
    public int SelectedTabIndexInput { get; set; } = -1;

    public SnackbarMessageQueue SnackbarMessageQueue { get; } = new();

    private readonly ILogger _logger;

    private readonly ReservationCacheService _cache;

    public ReservationEquipViewModel(
        ILogger<ReservationEquipViewModel> logger,
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
    }
    public required InformationBarViewModel InformationBarVm { get; set; }

    #region Function

    public FunctionBarViewModel FunctionBarVm { get; set; }

    private Dictionary<string, Function> CreateFunctions()
    {
        var functions = new List<Function>
            {
                new(FunctionKey.F5, "検索", this.ExecuteSearchCommand),

                new(FunctionKey.F9, "前月へ", this.ExecutePrevMonthSearchCommand),
                new(FunctionKey.F10, "次月へ", this.ExecuteNextMonthSearchCommand),
                new(FunctionKey.F11, "今月", this.ExecuteCurrentMonthSearchCommand),
                new(FunctionKey.F12, "閉じる", () => this.FunctionBarVm.ExecuteCloseCommand<ReservationEquipMonthlyWindow>()),
            }
            .ToDictionary(x => x.Key);

        return functions;
    }

    #endregion Function

    /// <summary>
    /// 初期化よりあとのタイミングで、あらかじめ必要なデータをロードします。
    /// </summary>
    /// <remarks>
    /// 画面から同期的に呼べる ContentRendered イベントで呼びます。
    /// </remarks>
    public async Task Preload()
    {
        var equips = await this._cache.GetOrFetchEquipments();

        // 全部作ると時間がかかるので、入れ物だけ用意する
        // バインドされているので、タブが選択されて Refresh が走る
        foreach (var equip in equips)
        {
            var vm = new ReservationEquipTabItemViewModel(this._logger, this._cache)
            {
                EquipId = equip.EquipId,
                EquipName = equip.EquipName,
            };

            this.ReservationEquipTabItems.Add(vm);
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

            if (this.ReservationEquipTabItems == null! ||
                this.ReservationEquipTabItems.Count == 0 ||
                tabIndex < 0)
            {
                // Preload 前は回避
                return;
            }

            var tabItem = this.ReservationEquipTabItems[tabIndex];
            if (tabItem.RefreshFuncAsync is null)
            {
                return;
            }

            var startMonth = this.StartMonth.Value;
            var endDate = DateHelper.GetEndDate(startMonth);
            await tabItem.RefreshFuncAsync.Invoke(endDate, tabItem.EquipId);
        }
        catch (Exception ex)
        {
            this.SnackbarMessageQueue.ShowMessage($"検索に失敗しました。({ex.Message})");
            this._logger.LogError(ex, ex.ToString());
        }
    }

    public async Task ExecutePrevMonthSearchCommand()
    {
        try
        {
            this.InformationBarVm.StartProgress();
            var isAlt = this.FunctionBarVm.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                await this.FunctionBarVm.ExecutePrevMonthCommand(this.StartMonth);
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


    public async Task ExecuteNextMonthSearchCommand()
    {
        try
        {
            this.InformationBarVm.StartProgress();
            var isAlt = this.FunctionBarVm.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                await this.FunctionBarVm.ExecuteNextMonthCommand(this.StartMonth);
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


    public async Task ExecuteCurrentMonthSearchCommand()
    {
        try
        {
            this.InformationBarVm.StartProgress();
            var isAlt = this.FunctionBarVm.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                await this.FunctionBarVm.ExecuteSetCurrentMonthCommand(this.StartMonth);
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
