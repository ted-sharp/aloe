using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloeReservationGrid.Lib.CoreLib.Mvvm;
using Reactive.Bindings.Extensions;

namespace AloeReservationGrid.App.ReservationApp.ViewModels;

public class NotifyIconViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public ReactivePropertySlim<bool> IsWindowVisible { get; }

    public ReactiveCommandSlim ShowWindowCommand { get; }
    public ReactiveCommandSlim HideWindowCommand { get; }
    public ReactiveCommandSlim ExitApplicationCommand { get; }

    public NotifyIconViewModel()
    {
        // 初期状態：メインウィンドウが表示されていない状態を設定
        this.IsWindowVisible = new ReactivePropertySlim<bool>(false);

        // ShowWindowCommand: ウィンドウが非表示のときのみ有効
        this.ShowWindowCommand = this.IsWindowVisible
            .Inverse() // 反転（IsWindowVisible が false のときに true になる）
            .ToReactiveCommandSlim();
        this.ShowWindowCommand.Subscribe(this.ShowWindow).AddTo(this.Disposables);

        // HideWindowCommand: ウィンドウが表示されているときのみ有効
        this.HideWindowCommand = this.IsWindowVisible
            .ToReactiveCommandSlim();
        this.HideWindowCommand.Subscribe(this.HideWindow).AddTo(this.Disposables);

        // ExitApplicationCommand: 常に実行可能
        this.ExitApplicationCommand = new ReactiveCommandSlim();
        this.ExitApplicationCommand.Subscribe(this.ExitApplication).AddTo(this.Disposables);

        if (Application.Current.MainWindow != null)
        {
            // ウィンドウを閉じてもアプリが終了しないようにする
            Application.Current.MainWindow.Closing += (sender, e) =>
            {
                if (sender is Window w)
                {
                    e.Cancel = true;
                    w.Hide();
                }

                this.IsWindowVisible.Value = false;
            };
        }
    }

    private void ShowWindow()
    {
        // TODO: ついでに最前面表示にしたい
        Application.Current.MainWindow?.Show();
        this.IsWindowVisible.Value = true;
    }

    private void HideWindow()
    {
        Application.Current.MainWindow?.Hide();
        this.IsWindowVisible.Value = false;
    }

    private void ExitApplication()
    {
        Application.Current.Shutdown();
    }
}
