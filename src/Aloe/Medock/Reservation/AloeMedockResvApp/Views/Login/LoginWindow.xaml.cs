using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Aloe.Medock.Reservation.AloeMedockResvApp.Ini;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using MagicOnion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;

/// <summary>
/// アプリケーションの開始画面となるメインウィンドウです。
/// </summary>
/// <remarks>
/// できるだけ初期表示時間を早めるため、初期表示には DI を使用しません。
/// DI が使用できないので ViewModel も使用しません。
/// 起動時に必要な画面以外は MVVM で実装しています。
/// </remarks>
public partial class LoginWindow
{
    private readonly LoginIni _ini;

    private bool _isForceClose;

    public LoginWindow(LoginIni ini)
    {
        this.InitializeComponent();

        this.Title = App.AppName;
        this.VersionText.Text = App.AppVersion;

        this._ini = ini;
        this.InitializeValue(ini);
    }

    /// <summary>
    /// 設定ファイルの値を反映します。
    /// </summary>
    private void InitializeValue(LoginIni ini)
    {
        this.UserRememberedCheckBox.IsChecked = ini.IsUserRemembered ?? false;

        if (ini.IsUserRemembered.HasValue && ini.IsUserRemembered.Value)
        {
            this.UserTextBox.Text = ini.User ?? "";
        }
        if (ini.IsPasswordRemembered.HasValue && ini.IsPasswordRemembered.Value)
        {
            this.PasswordTextBox.Text = ini.Password ?? "";
        }
    }

    /// <summary>
    /// IHost 初期化の完了を通知します。
    /// 一部のボタンを押せるようにします。
    /// </summary>
    public void InvokeCompleteInitHost(string message)
    {
        this.Dispatcher.InvokeIfNeeded(() =>
        {
            this.StatusText.Text = message;

            // ボタンを押せるようにする
            this.IniRemoveButton.IsEnabled = true;
            this.ShowLogWindowButton.IsEnabled = true;
        });
    }

    /// <summary>
    /// 初期化タスクの完了を通知します。
    /// プログレスバーを止めて、ログインボタンを押せるようにします。
    /// </summary>
    public void InvokeCompleteInitTask(string message)
    {
        this.Dispatcher.InvokeIfNeeded(() =>
        {
            // 非表示にする
            this.ProgressBar.Visibility = Visibility.Collapsed;

            this.StatusText.Text = message;

            // ボタンを押せるようにする
            this.LoginButton.IsEnabled = true;

        });
    }

    /// <summary>
    /// ステータスバーのテキストを更新します。
    /// </summary>
    public void InvokeSetStatus(string message)
    {
        this.Dispatcher.InvokeIfNeeded(() =>
        {
            this.StatusText.Text = message;
        });
    }

    /// <summary>
    /// スナックバーを表示します。
    /// すでに表示中のスナックバーがある場合はクリアします。
    /// </summary>
    public void InvokeShowSnackbar(string? message)
    {
        this.Dispatcher.InvokeIfNeeded(() =>
        {
            if (String.IsNullOrWhiteSpace(message))
            {
                return;
            }

            this.Snackbar.MessageQueue ??= new();
            this.Snackbar.MessageQueue.Clear();
            this.Snackbar.MessageQueue.Enqueue(message);
        });
    }

    /// <summary>
    /// 常駐アプリなので閉じないようにキャンセルします。
    /// </summary>
    private void LoginWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        // 閉じるときに設定を保存します。
        // 閉じる関連のイベントで await にはしません。
        this.SaveIniFile();

        // 強制閉じるだとキャンセルしません。
        if (this._isForceClose)
        {
            return;
        }

        // Close() をキャンセルして非表示にします。
        e.Cancel = true;
        this.Hide();
    }

    /// <summary>
    /// ログイン画面の情報をINIファイルに保存します。
    /// </summary>
    private void SaveIniFile()
    {
        try
        {
            var isRemembered = this.UserRememberedCheckBox.IsChecked ?? false;

            this._ini.IsUserRemembered = isRemembered;
            this._ini.IsPasswordRemembered = isRemembered;
            if (isRemembered)
            {
                this._ini.User = this.UserTextBox.Text;
                this._ini.Password = this.PasswordTextBox.Text;
            }
            else
            {
                this._ini.User = "";
                this._ini.Password = "";
            }

            // 変更になることを考慮して毎回取得し直します。
            var config = App.Resolve<IConfiguration>();
            this._ini.HostUrl = config.GetGrpcUrl();

            this._ini.Save(App.IniFilePath);
        }
        catch (Exception ex)
        {
            LoginWindow.LogError(ex);
        }
    }

    /// <summary>
    /// 例外をログ出力します。
    /// ただし、IHost.IServiceProvider の準備が整っていなければ、DebugConsole にのみ出力します。
    /// </summary>
    private static void LogError(Exception ex)
    {
        try
        {
            var logger = App.Resolve<ILogger<LoginWindow>>();
            logger.LogError(ex, ex.ToString());
        }
        catch
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// キャンセルせずに閉じます。
    /// </summary>
    public void ForceClose()
    {
        this._isForceClose = true;
        this.Close();
    }

    private void IniRemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        this.UserRememberedCheckBox.IsChecked = false;

        this._ini.Clear();

        // TODO: Window位置情報ファイルもリセットする
    }

    private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            this.LoginButton.IsEnabled = false;

            await this.LoginAsync(
                this.UserTextBox.Text,
                this.PasswordTextBox.Text);
        }
        catch (Exception ex)
        {
            LoginWindow.LogError(ex);
        }
        finally
        {
            this.LoginButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// ログインを試行します。
    /// </summary>
    private async Task LoginAsync(string user, string password)
    {
        this.InvokeSetStatus("Login trying...");

        // 連続で実行できないように、少し間を置く
        var delayTask = Task.Delay(1000);

        var request = new LoginRequest()
        {
            LoginName = user,
            Password = password,
            ClientAppName = App.AppName,
        };

        var auth = App.Resolve<IAuthGrpcService>();
        var result = await auth.LoginAsync(request);

        await delayTask;

        if (result.IsSuccess)
        {
            this.InvokeSetStatus("Login successful.");

            App.Session = result.SessionDto;

            var window = App.Resolve<ReservationMainWindow>();
            window.Show();
            this.Close();
        }
        else
        {
            var msg = "Login failed.";
            this.InvokeSetStatus(msg);
            this.InvokeShowSnackbar(result.ErrorMessage);
        }
    }

    private void ShowLogWindowButton_OnClick(object sender, RoutedEventArgs e)
    {
        LoginWindow.ShowLogWindow();
    }

    /// <summary>
    /// LogWindow を表示します。
    /// </summary>
    private static void ShowLogWindow()
    {
        try
        {
            var windowService = App.Resolve<WindowService>();
            var window = windowService.GetWindow<LogWindow>();
            window?.ShowOrActivate();
        }
        catch (Exception ex)
        {
            LoginWindow.LogError(ex);
        }
    }
}
