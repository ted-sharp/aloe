// <copyright file="LoginViewModel.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLogin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aloe.Apps.Medock.MdLogin.ViewModels;

/// <summary>
/// ログイン画面の ViewModel。
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly LoginGrpcClient _grpcClient;
    private readonly SessionWriter _sessionWriter;

    [ObservableProperty]
    private string _userCode = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoggingIn;

    /// <summary>
    /// ログイン成功時に発火するイベント。
    /// </summary>
    public event EventHandler? LoginSucceeded;

    /// <summary>
    /// <see cref="LoginViewModel"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public LoginViewModel(LoginGrpcClient grpcClient, SessionWriter sessionWriter)
    {
        _grpcClient = grpcClient;
        _sessionWriter = sessionWriter;
    }

    /// <summary>
    /// ログインコマンド。
    /// </summary>
    [RelayCommand]
    private async Task LoginAsync()
    {
        this.ErrorMessage = string.Empty;
        this.IsLoggingIn = true;

        try
        {
            var response = await _grpcClient.LoginAsync(this.UserCode, this.Password);

            if (!response.Success)
            {
                this.ErrorMessage = response.ErrorMessage;
                return;
            }

            await _sessionWriter.WriteAsync(response);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"サーバーに接続できません: {ex.Message}";
        }
        finally
        {
            this.IsLoggingIn = false;
        }
    }
}
