using AloeReservationGrid.App.ReservationApp.ViewModels;
using AloeReservationGrid.Lib.CoreLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AloeReservationGrid.App.ReservationApp.Views.Resv;

/// <summary>
/// ResvEquipWnd.xaml の相互作用ロジック
/// </summary>
public partial class ReservationEquipWindow : Window
{
    private readonly ReservationEquipViewModel _vm;

    public ReservationEquipWindow(ReservationEquipViewModel vm)
    {
        this.InitializeComponent();

        // ItemsSource にバインドするため、デザイン時の内容をクリアする
        this.EquipTabControl.Items.Clear();

        this._vm = vm;
        this.DataContext = vm;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        // 準備
        await this._vm.Preload();

        // 初回自動実行(検索)
        this._vm.ExecuteFirstCommand();
    }
}
