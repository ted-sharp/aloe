// <copyright file="LoginGrpcClient.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Contracts.Services;
using Grpc.Net.Client;
using MagicOnion.Client;

namespace Aloe.Apps.Medock.MdLogin.Services;

/// <summary>
/// ログイン gRPC クライアント。
/// </summary>
public class LoginGrpcClient
{
    private readonly ILoginService _client;

    /// <summary>
    /// <see cref="LoginGrpcClient"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public LoginGrpcClient(GrpcChannel channel)
    {
        _client = MagicOnionClient.Create<ILoginService>(channel);
    }

    /// <summary>
    /// ログイン認証を行う。
    /// </summary>
    public async Task<LoginResponse> LoginAsync(string userCode, string password)
    {
        return await _client.LoginAsync(new LoginRequest
        {
            UserCode = userCode,
            Password = password,
        });
    }
}
