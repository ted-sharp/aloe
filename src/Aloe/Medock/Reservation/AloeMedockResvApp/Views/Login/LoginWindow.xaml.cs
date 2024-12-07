using System.ComponentModel;
using System.Windows;
using Aloe.Common.AloeCoreLib.Ini;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;

/// <summary>
/// アプリケーションの開始画面となるメインウィンドウです。
/// </summary>
/// <remarks>
/// できるだけ初期表示時間を早めるため、DI も MVVM も使用しません。
/// </remarks>
public partial class LoginWindow : Window
{
    private bool _isForceClose = false;

    public LoginWindow(LoginIni ini)
    {
        this.InitializeComponent();

        if (ini.IsUserRemembered.HasValue && ini.IsUserRemembered.Value)
        {
            this.UserTextBox.Text = ini.User ?? "";
        }
        if (ini.IsPasswordRemembered.HasValue && ini.IsPasswordRemembered.Value)
        {
            this.PasswordTextBox.Text = ini.Password ?? "";
        }

        // TODO: ファイルに保存してある前回値などを入れたい
        // Window位置の前回値もそこに記録してあるはず
        // そのためには、Configurationだけ別で先にやる必要がある
        // ログイン画面をスキップする場合もこの設定に記述されるはず
        // 最速にしたいのでini形式にする

        // HostUrl=http://192.168.100.1:81
        // IsUserRemembered=true
        // IsPasswordRemembered
        // IsLoginSkipped=true
        // User=xxxx
        // Password=xxxx


        // この画面をスキップする場合はどうする？
        // 引数でusr/pwd指定された場合とか？
        // すでに起動済みのときにusr/pwd違いで指定されたらどうする？
        // 複数ログインに対応する？つまりSessionContextが必要？
        // 他の画面を開く際にSessionContextを要求すればよいか
    }

    public void StopIndicator()
    {
        // 非表示にする
        this.ProgressBar.Visibility = Visibility.Collapsed;
        // ボタンを押せるようにする
        this.LoginButton.IsEnabled = true;
    }

    /// <summary>
    /// 常駐アプリなので閉じないようにキャンセルします。
    /// </summary>
    private void LoginWindow_OnClosing(object? sender, CancelEventArgs e)
    {
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
    /// キャンセルせずに閉じます。
    /// </summary>
    public void ForceClose()
    {
        this._isForceClose = true;
        this.Close();
    }

    private void LoginButton_OnClick(object sender, RoutedEventArgs e)
    {
        // TODO: ログインを試す

        // 設定ファイルからサーバー接続先情報を取得して、チャネルを作ってgRPCでコールする(ここではDIは使わない)
        // ログインができたらWindowを表示する

        // ログインできたら、この画面は隠す
        // 以降、ログアウトしたときだけ再表示される


    }
}
