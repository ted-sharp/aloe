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

// TODO: Window 位置とサイズを覚えておきたい(iniにする？)

/// <summary>
/// 直近のログを確認できるログウィンドウです。
/// </summary>
/// <remarks>
/// できるだけ初期表示時間を早めるため、初期表示には DI を使用しません。
/// DI が使用できないので ViewModel も使用しません。
/// 起動時に必要な画面以外は MVVM で実装しています。
/// </remarks>
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
        // TODO: 閉じるときにWindow位置を記憶

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
