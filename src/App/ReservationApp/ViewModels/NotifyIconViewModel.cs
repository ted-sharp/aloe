using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloeReservationGrid.App.ReservationApp.Views.Login;
using AloeReservationGrid.App.ReservationApp.Views.Maint;
using AloeReservationGrid.App.ReservationApp.Views.Resv;
using AloeReservationGrid.Lib.CoreLib.Mvvm;
using Reactive.Bindings.Extensions;

namespace AloeReservationGrid.App.ReservationApp.ViewModels;

public class NotifyIconViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public ReactiveCommandSlim ShowReservationMainWindowCommand { get; } = new();
    public ReactiveCommandSlim ShowXxxWindowCommand { get; } = new();
    public ReactiveCommandSlim ShowMaintenanceWindowCommand { get; } = new();
    public ReactiveCommandSlim ShowLoginWindowCommand { get; } = new();
    public ReactiveCommandSlim RestartAppCommand { get; } = new();
    public ReactiveCommandSlim ExitAppCommand { get; } = new();

    public NotifyIconViewModel()
    {
        this.ShowReservationMainWindowCommand
            .Subscribe(this.ShowReservationMainWindow)
            .AddTo(this.Disposables);

        this.ShowMaintenanceWindowCommand
            .Subscribe(this.ShowMaintenanceWindow)
            .AddTo(this.Disposables);

        this.ShowLoginWindowCommand
            .Subscribe(this.ShowLoginWindow)
            .AddTo(this.Disposables);

        this.RestartAppCommand
            .Subscribe(this.RestartApplication)
            .AddTo(this.Disposables);

        this.ExitAppCommand
            .Subscribe(this.ExitApplication)
            .AddTo(this.Disposables);
    }

    //private void ShowWindow(Type type)
    //{
    //    // ログインしてなければ、ログイン画面を表示？
    //    // 自動ログアウトして、ログイン時に再開したいなら、閉じないで非表示にしておけばよいか？
    //}

    private void ShowWindow(Type type)
    {
        var window = Application.Current.Windows
            .Cast<Window>()
            .FirstOrDefault(x => x.GetType() == type);

        if (window == null)
        {
            window = App.Resolve(type) as Window;
            window?.Show();
        }
        else
        {
            window.ActivateOrShow();
        }
    }

    private void ShowReservationMainWindow()
    {
        Application.Current.MainWindow?.ActivateOrShow();
    }

    private void ShowMaintenanceWindow()
    {
        this.ShowWindow(typeof(MaintenanceWindow));
    }

    private void ShowLoginWindow()
    {
        this.ShowWindow(typeof(LoginWindow));
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
