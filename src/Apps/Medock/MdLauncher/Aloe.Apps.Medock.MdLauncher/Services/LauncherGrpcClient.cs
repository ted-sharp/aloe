// <copyright file="LauncherGrpcClient.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Contracts.Services;
using Grpc.Net.Client;
using MagicOnion.Client;

namespace Aloe.Apps.Medock.MdLauncher.Services;

/// <summary>
/// gRPC 経由でナビゲーション設定を取得するクライアント。
/// </summary>
public class LauncherGrpcClient
{
    private readonly ILauncherService _naviService;
    private GetLauncherConfigResponse? _cachedConfig;

    /// <summary>
    /// <see cref="LauncherGrpcClient"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public LauncherGrpcClient(GrpcChannel channel)
    {
        _naviService = MagicOnionClient.Create<ILauncherService>(channel);
    }

    /// <summary>
    /// サーバーからナビゲーション設定を取得する。接続エラー時はキャッシュを返す。
    /// </summary>
    public async Task<GetLauncherConfigResponse> GetConfigAsync()
    {
        try
        {
            var config = await _naviService.GetConfigAsync();
            _cachedConfig = config;
            return config;
        }
        catch (Exception) when (_cachedConfig != null)
        {
            return _cachedConfig;
        }
    }
}
