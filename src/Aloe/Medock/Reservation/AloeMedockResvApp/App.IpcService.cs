using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using System.Text.RegularExpressions;
using NetTopologySuite.Utilities;
using Serilog;
using System.Runtime.ExceptionServices;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Medock.Reservation.AloeMedockResvApp.Configuration;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

/// <remarks>
/// App.IpcService
/// IPC: プロセス間通信(Interprocess communication)
/// </remarks>
public partial class App
{
    public async void IpcService_ArgumentsReceived(string[]? args)
    {
        if (args == null || args.Length == 0)
        {
            // 何も指定がなければ、既存 Window をアクティブにする
            var w = Application.Current.MainWindow;
            w?.Activate();

            this.LogInformation($"Window Activate: {w?.GetType().Name} [{w?.Title}]");

            return;
        }

        if (Application.Current.MainWindow is LoginWindow)
        {
            var config = AloeClientConfig.CreateConfigurationRoot(args);
            var configArgs = config.BindSection<AloeClientArgs>();

            // ユーザー、パスワード、画面が指定されていた場合ログインを試みる
            var isLoggedIn = await this.LoginAsync(
                Task.CompletedTask,
                configArgs.User,
                configArgs.Password,
                configArgs.ScreenCode
            );

            if (!isLoggedIn)
            {
                // ログイン失敗なら何もしない
                return;
            }
        }

        // ログインした、またはログイン済みだった場合

        // TODO: カルテ番号の指定があったら、画面を動かす


        // その後ログイン済みであれば、
        // TODO: ScreenCode があれば Window を立ち上げる
        // ただし、無制限に立ち上げるわけではなく、特定のものだけ立ち上げたい
        // TODO: Pt の指定が一緒にあれば、Window を立ち上げたうえで検索もする


        this.LogInformation("Received Args: " + String.Join(',', args));
    }

}
