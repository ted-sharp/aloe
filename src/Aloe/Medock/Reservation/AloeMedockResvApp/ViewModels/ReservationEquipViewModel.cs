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

    /// <summary>
    /// 検索のときに参照します。
    /// </summary>
    public ReactivePropertySlim<string> StartMonth { get; set; } = new(DateTime.Today.ToString(StartMonthFormat));

    public ObservableCollection<ReservationEquipTabItemViewModel> ReservationEquipTabItems { get; set; } = new([]);

    /// <summary>
    /// 検索のときに参照します。
    /// </summary>
    public int SelectedTabIndex { get; set; } = -1;

    private readonly ILogger _logger;

    private readonly ReservationEquipmentCacheService _cache;

    public ReservationEquipViewModel(
        ILogger<ReservationEquipViewModel> logger,
        ReservationEquipmentCacheService cache,
        FunctionBarViewModel functionBar)
    {
        this._logger = logger;
        this._cache = cache;

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
        var format = ReservationEquipViewModel.StartMonthFormat;

        var functions = new List<Function>
            {
                new(FunctionKey.F5, "検索", this.ExecuteSearchCommand),

                new(FunctionKey.F9, "前月へ", async Task () =>
                {
                    var time = new Timestamper("GoToPrevMonth");
                    await this.FunctionBar.ExecutePrevMonthCommand(this.StartMonth, format);
                    time.Stamp("Prev");
                    await this.SearchAsync();
                    time.Stamp("Searched");
                    time.DumpAsync();
                }),
                new(FunctionKey.F10, "次月へ", async Task () =>
                {
                    await this.FunctionBar.ExecuteNextMonthCommand(this.StartMonth, format);
                    await this.SearchAsync();
                }),
                new(FunctionKey.F11, "今月", async Task () =>
                {
                    await this.FunctionBar.ExecuteSetCurrentMonthCommand(this.StartMonth, format);
                    await this.SearchAsync();
                }),
                new(FunctionKey.F12, "閉じる", () => this.FunctionBar.ExecuteCloseCommand<ReservationEquipWindow>()),
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
            //this.FunctionBar.SharedCanExecute.Value = false;
            var isAlt = this.FunctionBar.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                await this.SearchAsync();
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
        }
        //finally
        //{
        //    this.FunctionBar.SharedCanExecute.Value = true;
        //}
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
            this._logger.LogError(ex, ex.ToString());
        }
    }

}
