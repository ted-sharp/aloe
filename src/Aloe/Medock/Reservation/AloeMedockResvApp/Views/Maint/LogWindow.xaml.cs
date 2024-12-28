using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;

/// <summary>
/// LoggerWindow.xaml の相互作用ロジック
/// </summary>
public partial class LogWindow : Window
{
    private bool _isForceClose = false;

    public LogWindow()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// 常駐アプリなので閉じないようにキャンセルします。
    /// </summary>
    private void LogWindow_OnClosing(object? sender, CancelEventArgs e)
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
