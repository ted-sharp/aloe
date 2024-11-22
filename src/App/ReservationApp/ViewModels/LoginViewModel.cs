using AloeReservationGrid.Lib.CoreLib.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloeReservationGrid.App.ReservationApp.Views.Login;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using AloeReservationGrid.App.ReservationApp.Views.Resv;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Services;

namespace AloeReservationGrid.App.ReservationApp.ViewModels;

public class LoginViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public ReactivePropertySlim<string> User { get; } = new("");

    public ReactivePropertySlim<string> Password { get; } = new("");

    public ReactiveCommandSlim LoginCommand { get; } = new();

    private readonly ILogger _logger;
    private readonly IAuthGrpcService _authGrpcService;

    public LoginViewModel(
        ILogger<LoginViewModel> logger,
        IAuthGrpcService authGrpcService)
    {
        this._logger = logger;
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

            var loginWindow = App.GetWindow<LoginWindow>();

            // 閉じると終了してしまうので非表示とする
            loginWindow?.Hide();

            // ログインウィンドウの子として表示する
            var window = App.GetOrCreateWindow<ReservationMainWindow>();
            window.Owner = loginWindow;
            window.ActivateOrShow();
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
