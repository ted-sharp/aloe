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
/// ResvRoomWnd.xaml の相互作用ロジック
/// </summary>
public partial class ReservationDailyWindow : Window
{
    private readonly ILogger _logger;
    private readonly ReservationDailyViewModel _vm;

    private bool _isLoading = false;

    public ReservationDailyWindow(
        ILogger<ReservationDailyWindow> logger,
        ReservationDailyViewModel vm)
    {
        this.InitializeComponent();
        this.Closed += App.Current.Window_OnClosed;

        // ItemsSource にバインドするため、デザイン時の内容をクリアする
        //this.RoomTabControl.Items.Clear();

        this._logger = logger;
        this._vm = vm;
        this.DataContext = vm;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        try
        {
            this._vm.InformationBarVm.StartProgress();
            this._vm.FunctionBarVm.SharedCanExecute.Value = false;
            this._isLoading = true;

            this.BeginInit();

            base.OnContentRendered(e);

            // TODO: ポリシーが有効なとき、SEQの最初のフロアをロードしておく？

            // 準備で、設備をロードしておく
            //await this._vm.Preload();

            // 初回自動実行(検索)
            //await this._vm.SearchAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
        }
        finally
        {
            this._isLoading = false;
            this._vm.FunctionBarVm.SharedCanExecute.Value = true;
            this._vm.InformationBarVm.StopProgress();
            this.EndInit();
        }
    }

    private async void DailyTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (sender != e.OriginalSource)
            {
                // 自身以外のイベントを除外
                // ListBox.OnSelectionChanged がバブルアップしてきます。
                return;
            }

            if (this._isLoading)
            {
                // タブのロード中に選択されるので除外
                return;
            }

            // 除外が必要なためイベントからコマンドを実行しています。
            await this._vm.ExecuteSearchCommand();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
        }
    }

    private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Ctrl 押しながらスクロールで拡大縮小
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            var delta = e.Delta > 0 ? InformationBarViewModel.WheelStepScale : -InformationBarViewModel.WheelStepScale;
            this._vm.InformationBarVm.ZoomInCommand.Execute(delta);

            e.Handled = true;
        }
    }

    private void Calendar_OnSelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
    {
        // CalendarItem にフォーカスがあると、他のコントロールの操作時に、
        // フォーカスを移す動作が必要になるので解除する
        if (Mouse.Captured is System.Windows.Controls.Primitives.CalendarItem)
        {
            Mouse.Capture(null);
        }
    }
}
