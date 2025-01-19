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
using MaterialDesignThemes.Wpf;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;


public class ReservationEquipViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public static readonly string StartMonthFormat = "yyyy.MM";

    /// <summary>
    /// 検索のときに参照します。
    /// </summary>
    public ReactivePropertySlim<string> StartMonth { get; set; } = new(DateTime.Today.ToString(StartMonthFormat));

    public ObservableCollection<ReservationEquipTabItemViewModel> ReservationEquipTabItems { get; set; } = new([]);

    /// <summary>
    /// 検索のときに参照します。
    /// </summary>
    public int SelectedTabIndex { get; set; } = -1;

    public SnackbarMessageQueue SnackbarMessageQueue { get; } = new();

    private readonly ILogger _logger;

    private readonly ReservationEquipmentCacheService _cache;

    public ReservationEquipViewModel(
        ILogger<ReservationEquipViewModel> logger,
        ReservationEquipmentCacheService cache,
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
                new(FunctionKey.F12, "閉じる", () => this.FunctionBarVm.ExecuteCloseCommand<ReservationEquipWindow>()),
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
            var tabIndex = this.SelectedTabIndex;

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
            var endDate = startMonth.ToMonthEndDateOrCurrentMonth();
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
                await this.FunctionBarVm.ExecutePrevMonthCommand(this.StartMonth, ReservationEquipViewModel.StartMonthFormat);
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
                await this.FunctionBarVm.ExecuteNextMonthCommand(this.StartMonth, ReservationEquipViewModel.StartMonthFormat);
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
                await this.FunctionBarVm.ExecuteSetCurrentMonthCommand(this.StartMonth, ReservationEquipViewModel.StartMonthFormat);
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
