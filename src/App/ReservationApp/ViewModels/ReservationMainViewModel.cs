using AloeReservationGrid.Lib.CoreLib.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloeReservationGrid.Api.ReservationServer.Grpc.Services;
using AloeReservationGrid.App.ReservationApp.Views.Login;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using AloeReservationGrid.App.ReservationApp.Views.Resv;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace AloeReservationGrid.App.ReservationApp.ViewModels;


public class ReservationMainViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public ReactivePropertySlim<DateTime> StartDate { get; set; } = new(DateTime.Today);
    public ReactivePropertySlim<int> OffsetDayCount { get; set; } = new(31);

    // TODO: DataTable を使う
    //public ObservableCollection<RoomSchedule> Schedules { get; set; } = new();

    private readonly ILogger _logger;

    public ReservationMainViewModel(
        ILogger<LoginViewModel> logger,
        IAuthGrpcService authGrpcService)
    {
        this._logger = logger;
    }

    private Dictionary<string, int> GenerateSampleData(ObservableCollection<string> headers, Random random)
    {
        var data = new Dictionary<string, int>();
        foreach (var header in headers)
        {
            data[header] = random.Next(0, 50); // 件数をランダムで設定
        }
        return data;
    }
}
