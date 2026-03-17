// <copyright file="LoginWindow.xaml.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Windows;
using System.Windows.Input;
using Aloe.Apps.Medock.MdLogin.ViewModels;

namespace Aloe.Apps.Medock.MdLogin;

/// <summary>
/// ログインウィンドウ。
/// </summary>
public partial class LoginWindow : Window
{
    /// <summary>
    /// <see cref="LoginWindow"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public LoginWindow()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) => UserCodeTextBox.Focus();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (this.DataContext is LoginViewModel vm)
        {
            vm.Password = PasswordBox.Password;
        }
    }

    private async void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter && this.DataContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null))
        {
            await vm.LoginCommand.ExecuteAsync(null);
        }
    }
}
