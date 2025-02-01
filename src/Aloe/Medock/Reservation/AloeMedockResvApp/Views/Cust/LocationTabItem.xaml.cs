using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Cust;

public partial class LocationTabItem : UserControl
{
    private readonly ILogger _logger;
    //private readonly ReservationRoommentCacheService _cache;
    //private ReservationRoomTabItemViewModel? _vm;

    public static bool IsInDesignMode =>
        (bool)DesignerProperties.IsInDesignModeProperty
            .GetMetadata(typeof(DependencyObject)).DefaultValue;

    public LocationTabItem()
    {
        this.InitializeComponent();

        //if (ReservationRoomDailyGrid.IsInDesignMode)
        //{
        //    // デザイナーでエラーになるので回避
        //    this._logger = null!;
        //    //this._cache = null!;
        //    return;
        //}

        //this._logger = App.Resolve<ILogger<ReservationRoomDailyGrid>>();
        //this._cache = App.Resolve<ReservationRoommentCacheService>();
    }

    /// <summary>
    /// 親要素から DataContext が設定されたときに追加で関連付けます。
    /// </summary>
    private void ContactTabItem_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        //this._vm = this.DataContext as ReservationRoomTabItemViewModel;
        //if (this._vm != null)
        //{
        //    this._vm.RefreshFuncAsync = this.RefreshAsync;
        //}
    }


}
