using System.ComponentModel;
using System.Windows;
using AloeReservationGrid.App.ReservationApp.ViewModels;
using AloeReservationGrid.App.ReservationApp.Views.Resv;

namespace AloeReservationGrid.App.ReservationApp.Views.Login;

/// <summary>
/// アプリケーションのメインとなるルートウィンドウです。
/// </summary>
public partial class LoginWindow : Window
{
    private bool _isForceClose = false;

    public LoginWindow(LoginViewModel vm)
    {
        this.InitializeComponent();

        this.DataContext = vm;
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
}
