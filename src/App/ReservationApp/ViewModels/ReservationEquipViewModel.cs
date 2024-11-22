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

namespace AloeReservationGrid.App.ReservationApp.ViewModels;


// TODO: キャッシュキー用のreadonly record structを作るのが良さそう

public class ReservationEquipViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public static readonly string StartMonthFormat = "yyyy.MM";

    public Action<DateTime>? RefreshAction { get; set; }

    public ReactivePropertySlim<string> StartMonth { get; set; } = new(DateTime.Today.ToString(StartMonthFormat));


    //public ReactivePropertySlim<int> OffsetDayCount { get; set; } = new(31);

    //public ReactivePropertySlim<string> FloorId1 { get; set; } = new("1");
    //public ReactivePropertySlim<string> FloorName1 { get; set; } = new("Floor1");
    //public ReadOnlyReactivePropertySlim<string> VerticalFloorName1 { get; }
    //public ReactivePropertySlim<string> FloorId2 { get; set; } = new("2");
    //public ReactivePropertySlim<string> FloorName2 { get; set; } = new("Floor2");
    //public ReadOnlyReactivePropertySlim<string> VerticalFloorName2 { get; }

    //public ReactivePropertySlim<bool?> IsAutoRefresh { get; set; } = new(true);

    //public ReactivePropertySlim<string> SecondsToRefresh { get; set; } = new("60");

    //public DataTable Schedules { get; set; } = new();
    //public DataTable Schedules2 { get; set; } = new();
    //public DataTable Schedules3 { get; set; } = new();

    private readonly ILogger _logger;

    #region Cache

    private readonly IMemoryCache _cache;
    private readonly IReservationEquipmentGrpcService _equipGrpcService;

    public async Task<List<ReservationEquipmentDto>> GetOrFetchEquipments(bool useCache = true)
    {
        var key = "equipments";
        if (useCache && this._cache.TryGetValue<List<ReservationEquipmentDto>>(
                key, out var equipments))
        {
            return equipments ?? [];
        }

        equipments = await this._equipGrpcService.FetchEquipmentDtosAsync();

        // マスター情報はまず変更がないのでしばらく保持しておく
        var expiration = TimeSpan.FromHours(1);
        this._cache.Set(key, equipments, expiration);

        return equipments;
    }

    public async Task<List<ReservationEquipmentSlotDto>> GetOrFetchSlots(string monthString, bool useCache = true)
    {
        var date = monthString.ToDateOrToday();
        var year = date.Year;
        var month = date.Month;
        return await this.GetOrFetchSlots(year, month, useCache);
    }

    public async Task<List<ReservationEquipmentSlotDto>> GetOrFetchSlots(int year, int month, bool useCache = true)
    {

        var key = $"equipmentSlots_{year:0000}{month:00}";
        if (useCache && this._cache.TryGetValue<List<ReservationEquipmentSlotDto>>(
                key, out var slots))
        {
            return slots ?? [];
        }

        slots = await this._equipGrpcService.FetchEquipmentSlotDtosAsync(year, month);

        // スロット情報は毎日変更があるが、リアルタイムの変更もあるので程々とする
        var expiration = TimeSpan.FromMinutes(10);
        this._cache.Set(key, slots, expiration);

        return slots;
    }

    public async Task<List<ReservationEquipmentBookingDto>> GetOrFetchBookings(string monthString, int equipId, bool useCache = true)
    {
        var date = monthString.ToDateOrToday();
        var year = date.Year;
        var month = date.Month;
        return await this.GetOrFetchBookings(year, month, equipId, useCache);
    }

    public async Task<List<ReservationEquipmentBookingDto>> GetOrFetchBookings(int year, int month, int equipId, bool useCache = true)
    {
        var key = $"equipmentBookings_{year:0000}{month:00}_{equipId}";
        if (useCache && this._cache.TryGetValue<List<ReservationEquipmentBookingDto>>(
                key, out var bookings))
        {
            return bookings ?? [];
        }

        bookings = await this._equipGrpcService.FetchEquipmentBookingDtosAsync(year, month, equipId);

        // リアルタイムの変更もあるので数分とする
        var expiration = TimeSpan.FromMinutes(3);
        this._cache.Set(key, bookings, expiration);

        return bookings;
    }

    public async Task<DataTable> GetOrFetchBookingsTable(int year, int month, int equipId, bool useCache = true)
    {
        var key = $"equipmentBookingsTable_{year:0000}{month:00}_{equipId}";
        if (useCache && this._cache.TryGetValue<DataTable>(
                key, out var bookingsTable))
        {
            return bookingsTable ?? new();
        }

        // 列: スロット(記号、氏名、備考の繰り返し)
        // 行: 日付
        bookingsTable = new DataTable();

        var slots = await this.GetOrFetchSlots(year, month, useCache);
        var cols = await this.CreateColumns(slots);
        foreach (var col in cols)
        {
            bookingsTable.Columns.Add(new DataColumn($"{col}_symbol"));
            bookingsTable.Columns.Add(new DataColumn($"{col}_pt"));
            bookingsTable.Columns.Add(new DataColumn($"{col}_remark"));
        }

        var bookings = await this._equipGrpcService.FetchEquipmentBookingDtosAsync(year, month, equipId);
        var days = new DateTime(year, month + 1, 1).AddDays(-1).Day;
        for (var i = 1; i <= days; i++)
        {
            // 日付毎に作っていく
            //bookings.FindAll(x => x.BkgDate )

        }

        // リアルタイムの変更もあるので数分とする
        var expiration = TimeSpan.FromMinutes(3);
        this._cache.Set(key, bookingsTable, expiration);

        return bookingsTable;
    }

    private async Task<List<string>> CreateColumns(List<ReservationEquipmentSlotDto> definitions)
    {
        // 定義されている最大の slot の一覧を作成する
        var cols = new HashSet<string>();

        foreach (var def in definitions)
        {
            // def 毎の slot 重複数を数える
            var slotCounts = new Dictionary<string, int>();

            for (var i = 0; i < def.Slots.Length; i++)
            {
                var slot = def.Slots[i];

                if (!slotCounts.TryAdd(slot, 1))
                {
                    slotCounts[slot]++;
                    // 重複したら (n) をつける
                    slot = $"{slot}({slotCounts[slot]})";
                    def.Slots[i] = slot;
                }

                cols.Add(slot);
            }
        }

        return cols.OrderBy(x => x).ToList(); ;
    }

    #endregion Cache

    public ReservationEquipViewModel(
        ILogger<ReservationEquipViewModel> logger,
        IMemoryCache cache,
        IReservationEquipmentGrpcService equipGrpcService,
        FunctionBarViewModel functionBar)
    {
        this._logger = logger;
        this._cache = cache;
        this._equipGrpcService = equipGrpcService;

        #region SearchCondition

        this.StartMonth
            .Subscribe(this.StartMonth_OnChanged)
            .AddTo(this.Disposables);

        // TODO: FloorName1を更新、データも更新
        //this.FloorId1.Subscribe(x => )

        //this.VerticalFloorName1 = this.FloorName1
        //    .Select(x => String.Join(Environment.NewLine, x.ToCharArray()))
        //    .ToReadOnlyReactivePropertySlim<string>()
        //    .AddTo(this.Disposables);

        //this.VerticalFloorName2 = this.FloorName2
        //    .Select(x => String.Join(Environment.NewLine, x.ToCharArray()))
        //    .ToReadOnlyReactivePropertySlim<string>()
        //    .AddTo(this.Disposables);

        #endregion SearchCondition

        #region Function

        var functions = this.CreateFunctions();
        functionBar.InitializeFunctions(functions);
        this.FunctionBar = functionBar;

        #endregion Function
    }

    private async void StartMonth_OnChanged(string startMonth)
    {


        await this.LoadDataAsync();

        this.RefreshAction?.Invoke(startMonth.ToDateOrToday());

    }

    #region Function

    public FunctionBarViewModel FunctionBar { get; set; }

    private Dictionary<string, Function> CreateFunctions()
    {
        var format = "yyyy.MM";

        var functions = new List<Function>
        {
            new(FunctionKey.F5, "検索", this.ExecuteReloadCommand),

            new(FunctionKey.F9, "前月へ", () => this.FunctionBar.ExecutePrevMonthCommand(this.StartMonth, format)),
            new(FunctionKey.F10, "次月へ", () => this.FunctionBar.ExecuteNextMonthCommand(this.StartMonth, format)),
            new(FunctionKey.F11, "今月", () => this.FunctionBar.ExecuteSetCurrentMonthCommand(this.StartMonth, format)),
            new(FunctionKey.F12, "閉じる", () => this.FunctionBar.ExecuteCloseCommand<ReservationEquipWindow>()),
        }.ToDictionary(x => x.Key);

        return functions;
    }

    private async void ExecuteReloadCommand()
    {
        try
        {
            this.FunctionBar.SharedCanExecute.Value = false;
            var isAlt = this.FunctionBar.IsAltKeyPressed.Value;
            if (!isAlt)
            {
                await this.LoadDataAsync();
                this._logger.LogInformation("F5");
            }
        }
        finally
        {
            this.FunctionBar.SharedCanExecute.Value = true;
        }
    }

    public async Task LoadDataAsync()
    {
        // TODO
        // equipId をキーとしたDicにDataTableを入れる？

        await Task.Delay(3000);
    }

    #endregion Function

}
