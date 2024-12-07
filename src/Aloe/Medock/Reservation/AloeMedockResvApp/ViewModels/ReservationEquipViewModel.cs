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

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;


public class ReservationEquipViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public static readonly string StartMonthFormat = "yyyy.MM";

    public ReactivePropertySlim<string> StartMonth { get; set; } = new(DateTime.Today.ToString(StartMonthFormat));

    public ObservableCollection<ReservationEquipTabItemViewModel> ReservationEquipTabItems { get; set; } = new([]);

    public ReactivePropertySlim<int> SelectedTabIndex { get; set; } = new();

    private readonly ILogger _logger;

    private readonly ReservationEquipmentCacheService _cache;

    public ReservationEquipViewModel(
        ILogger<ReservationEquipViewModel> logger,
        ReservationEquipmentCacheService cache,
        FunctionBarViewModel functionBar)
    {
        this._logger = logger;
        this._cache = cache;

        #region SearchCondition

        this.StartMonth
            .Subscribe(this.StartMonth_OnChanged)
            .AddTo(this.Disposables);

        this.SelectedTabIndex
            .Subscribe(this.SelectedTabIndex_OnChanged)
            .AddTo(this.Disposables);

        #endregion SearchCondition

        #region Function

        var functions = this.CreateFunctions();
        functionBar.InitializeFunctions(functions);
        this.FunctionBar = functionBar;

        #endregion Function
    }

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
        // バインドされているので、タブが選択されて Refresh が走るはず
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
    /// 開始年月が変わったときに、アクティブなタブの内容を更新します。
    /// </summary>
    private async void StartMonth_OnChanged(string startMonth)
    {
        try
        {
            var tabIndex = this.SelectedTabIndex.Value;
            await this.RefreshAsync(startMonth, tabIndex);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error!");
        }
    }

    /// <summary>
    /// タブが切り替わったときに、アクティブなタブの内容を更新します。
    /// </summary>
    private async void SelectedTabIndex_OnChanged(int tabIndex)
    {
        try
        {
            var startMonth = this.StartMonth.Value;
            await this.RefreshAsync(startMonth, tabIndex);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error!");
        }
    }

    private async Task RefreshAsync(string startMonth, int tabIndex)
    {
        if (this.ReservationEquipTabItems == null! ||
            this.ReservationEquipTabItems.Count == 0)
        {
            // Preload 前は回避
            return;
        }

        var tabItem = this.ReservationEquipTabItems[tabIndex];
        var date = startMonth.ToDateOrToday();
        tabItem.Year = date.Year;
        tabItem.Month = date.Month;
        await tabItem.LoadAsync();
        tabItem.RefreshAction?.Invoke();
    }

    #region Function

    public FunctionBarViewModel FunctionBar { get; set; }

    private Dictionary<string, Function> CreateFunctions()
    {
        var format = "yyyy.MM";

        var functions = new List<Function>
        {
            new(FunctionKey.F5, "検索", this.ExecuteSearchCommand),

            new(FunctionKey.F9, "前月へ", () => this.FunctionBar.ExecutePrevMonthCommand(this.StartMonth, format)),
            new(FunctionKey.F10, "次月へ", () => this.FunctionBar.ExecuteNextMonthCommand(this.StartMonth, format)),
            new(FunctionKey.F11, "今月", () => this.FunctionBar.ExecuteSetCurrentMonthCommand(this.StartMonth, format)),
            new(FunctionKey.F12, "閉じる", () => this.FunctionBar.ExecuteCloseCommand<ReservationEquipWindow>()),
        }.ToDictionary(x => x.Key);

        return functions;
    }

    private async void ExecuteSearchCommand()
    {
        try
        {
            this.FunctionBar.SharedCanExecute.Value = false;
            var isAlt = this.FunctionBar.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                var startMonth = this.StartMonth.Value;
                var tabIndex = this.SelectedTabIndex.Value;
                await this.RefreshAsync(startMonth, tabIndex);
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error!");
        }
        finally
        {
            this.FunctionBar.SharedCanExecute.Value = true;
        }
    }

    /// <summary>
    /// 検索など、ユーザーが初回に必ず実行するコマンドを呼び出します。
    /// </summary>
    public void ExecuteFirstCommand()
    {
        if (this.FunctionBar.F5Command.CanExecute())
        {
            this.FunctionBar.F5Command.Execute();
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    #endregion Function

}
