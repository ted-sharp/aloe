using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloeReservationGrid.App.ReservationApp.Services;
using AloeReservationGrid.App.ReservationApp.Views.Login;
using AloeReservationGrid.App.ReservationApp.Views.Maint;
using AloeReservationGrid.App.ReservationApp.Views.Resv;
using AloeReservationGrid.Lib.CoreLib.Mvvm;

namespace AloeReservationGrid.App.ReservationApp.ViewModels;

public class NotifyIconViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public ReactiveCommandSlim ShowReservationMainWindowCommand { get; } = new();
    public ReactiveCommandSlim ShowXxxWindowCommand { get; } = new();
    public ReactiveCommandSlim ShowMaintenanceWindowCommand { get; } = new();
    public ReactiveCommandSlim LogoutCommand { get; } = new();
    public ReactiveCommandSlim RestartAppCommand { get; } = new();
    public ReactiveCommandSlim ExitAppCommand { get; } = new();

    private readonly WindowService _windowService;

    public NotifyIconViewModel(WindowService windowService)
    {
        this._windowService = windowService;

        this.ShowReservationMainWindowCommand
            .Subscribe(this.ShowReservationMainWindow)
            .AddTo(this.Disposables);

        this.ShowMaintenanceWindowCommand
            .Subscribe(this.ShowMaintenanceWindow)
            .AddTo(this.Disposables);

        this.LogoutCommand
            .Subscribe(this.Logout)
            .AddTo(this.Disposables);

        this.RestartAppCommand
            .Subscribe(this.RestartApplication)
            .AddTo(this.Disposables);

        this.ExitAppCommand
            .Subscribe(this.ExitApplication)
            .AddTo(this.Disposables);
    }

    private T? ShowWindow<T>()
        where T: Window
    {
        var window = this._windowService.GetOrCreateWindow<T>();
        window.Owner = Application.Current.MainWindow;
        window.ActivateOrShow();
        return window;
    }

    private void ShowReservationMainWindow()
    {
        if (App.HasSession)
        {
            this.ShowWindow<ReservationMainWindow>();
        }
        else
        {
            this.Logout();
        }
    }

    private void ShowMaintenanceWindow()
    {
        if (App.HasSession)
        {
            this.ShowWindow<MaintenanceWindow>();
        }
        else
        {
            this.Logout();
        }
    }

    private void Logout()
    {
        // TODO: ログアウトするか確認する

        // セッションを破棄する
        App.Session = null;

        // 既存の LoginWindow を取得する
        var oldLoginWindow = this._windowService.GetWindow<LoginWindow>();

        // 新しい LoginWindow を作る
        var newLoginWindow = this._windowService.CreateWindow<LoginWindow>();
        Application.Current.MainWindow = newLoginWindow;

        // キャンセルさせない
        oldLoginWindow?.ForceClose();

        // 念の為、他の Window も探す
        var oldWindows = Application.Current.Windows
            .OfType<Window>()
            .Where(x => x != newLoginWindow)
            .ToArray();

        // すべて閉じる
        foreach (var oldWindow in oldWindows)
        {
            oldWindow.Close();
        }

        // 新しい LoginWindow を表示する
        newLoginWindow.ActivateOrShow();
    }

    private void RestartApplication()
    {
        // TODO: リスタート
        // 引数も同じにしておく必要がありそう
        throw new NotImplementedException();
    }

    private void ExitApplication()
    {
        Application.Current.Shutdown();
    }
}
