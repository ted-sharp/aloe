using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.Options;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;

/// <summary>
/// 予約のメイン画面です。
/// 指定日から1ヶ月間の予約件数を表示できます。
/// </summary>
public partial class ReservationMainWindow : Window
{
    private readonly ReservationMainViewModel _vm;

    private readonly ScrollViewerSynchronizer _synchronizer = new();

    public ReservationMainWindow(ReservationMainViewModel vm)
    {
        this.InitializeComponent();
        this.Closed += App.Current.Window_OnClosed;

        this._vm = vm;

    }


}
