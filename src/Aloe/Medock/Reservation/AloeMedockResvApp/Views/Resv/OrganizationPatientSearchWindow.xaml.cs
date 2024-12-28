using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Aloe.Common.AloeCoreLib.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Serilog.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;

/// <summary>
/// OrganizationPatientSearchWindow.xaml の相互作用ロジック
/// </summary>
public partial class OrganizationPatientSearchWindow : Window
{
    private readonly ILogger _logger;
    private readonly ReservationEquipViewModel _vm;

    public OrganizationPatientSearchWindow(
        ILogger<ReservationEquipWindow> logger,
        ReservationEquipViewModel vm)
    {
        this.InitializeComponent();

        // ItemsSource にバインドするため、デザイン時の内容をクリアする
        this.EquipTabControl.Items.Clear();

        this._logger = logger;
        this._vm = vm;
        this.DataContext = vm;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        try
        {
            base.OnContentRendered(e);

            this.BeginInit();
            this._vm.FunctionBarVm.SharedCanExecute.Value = false;

            // 準備で、設備をロードしておく
            await this._vm.Preload();

            // 初回自動実行(検索)
            await this._vm.SearchAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
        }
        finally
        {
            this._vm.FunctionBarVm.SharedCanExecute.Value = true;
            this.EndInit();
        }
    }

    private async void EquipTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (sender != e.OriginalSource)
            {
                // 自身以外のイベントを除外
                // ListBox.OnSelectionChanged がバブルアップしてきます。
                return;
            }

            this._vm.FunctionBarVm.SharedCanExecute.Value = false;

            var index = this.EquipTabControl.SelectedIndex;
            if (index >= 0)
            {
                this._vm.SelectedTabIndex = index;
                await this._vm.SearchAsync();
            }
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
        }
        finally
        {
            this._vm.FunctionBarVm.SharedCanExecute.Value = true;
        }
    }

    private void EquipTabControl_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control)
        {
            return;
        }

        var delta = e.Delta > 0 ? InformationBarViewModel.WheelStepScale : -InformationBarViewModel.WheelStepScale;
        this._vm.InformationBarVm.ZoomInCommand.Execute(delta);

        e.Handled = true;
    }
}
