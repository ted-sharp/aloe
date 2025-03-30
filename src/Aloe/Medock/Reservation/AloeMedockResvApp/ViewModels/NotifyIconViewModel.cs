using R3;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Aloe.Common.AloeCoreLib.Wpf.Extensions;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using Microsoft.Extensions.Logging;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using Aloe.Common.AloeCoreLib.Mvvm;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

public class NotifyIconViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public ReactiveCommand ShowReservationMainWindowCommand { get; } = new();
    public ReactiveCommand ShowXxxWindowCommand { get; } = new();
    public ReactiveCommand ShowLogWindowCommand { get; } = new();
    public ReactiveCommand ShowMaintenanceWindowCommand { get; } = new();
    public ReactiveCommand LogoutCommand { get; } = new();
    public ReactiveCommand RestartAppCommand { get; } = new();
    public ReactiveCommand ExitAppCommand { get; } = new();

    private readonly ILogger _logger;
    private readonly WindowService _windowService;

    public NotifyIconViewModel(
        ILogger<NotifyIconViewModel> logger,
        WindowService windowService)
    {
        this._logger = logger;
        this._windowService = windowService;

        var d = R3.Disposable.CreateBuilder();

        this.ShowReservationMainWindowCommand
            .Subscribe(this.ShowReservationMainWindow)
            .AddTo(ref d);

        this.ShowLogWindowCommand
            .Subscribe(this.ShowLogWindow)
            .AddTo(ref d);

        this.ShowMaintenanceWindowCommand
            .Subscribe(this.ShowMaintenanceWindow)
            .AddTo(ref d);

        this.LogoutCommand
            .Subscribe(this.Logout)
            .AddTo(ref d);

        this.RestartAppCommand
            .Subscribe(this.RestartApplication)
            .AddTo(ref d);

        this.ExitAppCommand
            .Subscribe(this.ExitApplication)
            .AddTo(ref d);

        this.Disposable = d.Build();
    }

    private T? ShowWindow<T>()
        where T: Window
    {
        var window = this._windowService.GetOrCreateWindow<T>();
        window.Owner = Application.Current.MainWindow;
        window.ShowOrActivate();
        return window;
    }

    private void ShowReservationMainWindow(Unit _)
    {
        if (App.HasSession)
        {
            this.ShowWindow<ReservationMainWindow>();
        }
        else
        {
            this.Logout(_);
        }
    }

    private void ShowLogWindow(Unit _)
    {
        if (App.HasSession)
        {
            this.ShowWindow<LogWindow>();
        }
        else
        {
            this.Logout(_);
        }
    }

    private void ShowMaintenanceWindow(Unit _)
    {
        if (App.HasSession)
        {
            this.ShowWindow<MaintenanceWindow>();
        }
        else
        {
            this.Logout(_);
        }
    }

    private async void Logout(Unit _)
    {
        try
        {
            // TODO: ログアウトするか確認する

            // セッションを破棄する
            await App.TryLogoutAsync();

            // 既存の LoginWindow を取得する
            var oldLoginWindow = this._windowService.GetWindow<LoginWindow>();
            Application.Current.MainWindow = oldLoginWindow;

            //// 新しい LoginWindow を作る
            //var newLoginWindow = this._windowService.CreateWindow<LoginWindow>();

            //// キャンセルさせない
            //oldLoginWindow?.ForceClose();

            // 念の為、他の Window も探す
            var oldWindows = Application.Current.Windows
                .OfType<Window>()
                .Where(x => x != oldLoginWindow)
                .ToArray();

            // すべて閉じる
            foreach (var oldWindow in oldWindows)
            {
                oldWindow.Close();
            }

            // 新しい LoginWindow を表示する
            oldLoginWindow?.ShowOrActivate();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
        }
    }

    private void RestartApplication(Unit _)
    {
        // TODO: リスタート
        // 引数も同じにしておく必要がありそう
        throw new NotImplementedException();
    }

    private void ExitApplication(Unit _)
    {
        Application.Current.Shutdown();
    }
}
