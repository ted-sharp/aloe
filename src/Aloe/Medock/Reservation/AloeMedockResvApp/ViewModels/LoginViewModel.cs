using Aloe.Common.AloeCoreLib.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

public class LoginViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public ReactivePropertySlim<string> User { get; } = new("");

    public ReactivePropertySlim<string> Password { get; } = new("");

    public ReactiveCommandSlim LoginCommand { get; } = new();

    private readonly ILogger _logger;
    private readonly WindowService _windowService;
    private readonly IAuthGrpcService _authGrpcService;

    public LoginViewModel(
        ILogger<LoginViewModel> logger,
        WindowService windowService,
        IAuthGrpcService authGrpcService)
    {
        this._logger = logger;
        this._windowService = windowService;
        this._authGrpcService = authGrpcService;

        this.LoginCommand
            .Subscribe(this.Login)
            .AddTo(this.Disposables);
    }

    private async void Login()
    {
        try
        {
            // TODO: AuthGrpcService を使う
            await Task.CompletedTask;
            //await this._authGrpcService.TestAsync();

            // とりあえずログイン成功したことにする
            App.Session = new SessionDto()
            {
                SessionId = Guid.NewGuid(),
                UserId = 1,
                UserDisplayName = "Test",
            };

            var loginWindow = this._windowService.GetWindow<LoginWindow>();

            // 閉じると終了してしまうので非表示とする
            loginWindow?.Hide();

            // ログインウィンドウの子として表示する
            var window = this._windowService.GetOrCreateWindow<ReservationMainWindow>();
            window.Owner = loginWindow;
            window.ShowOrActivate();
        }
        catch (Grpc.Core.RpcException ex)
        {
            // TODO: 例外表示: 接続エラーとか
            this._logger.LogError(ex, "gRPC error.");
        }
        catch (Exception ex)
        {
            // TODO: 例外表示
            this._logger.LogError(ex, "error.");
        }
    }
}
