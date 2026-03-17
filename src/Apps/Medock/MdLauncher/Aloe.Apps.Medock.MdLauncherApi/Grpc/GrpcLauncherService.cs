// <copyright file="GrpcLauncherService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Contracts.Services;
using Aloe.Apps.Medock.MdLauncherLib.Services;
using MagicOnion;
using MagicOnion.Server;

namespace Aloe.Apps.Medock.MdLauncherApi.Grpc;

/// <summary>
/// ナビゲーションサービスの MagicOnion gRPC 実装。
/// </summary>
public class GrpcLauncherService : ServiceBase<ILauncherService>, ILauncherService
{
    private readonly ILauncherConfigService _configService;

    /// <summary>
    /// <see cref="GrpcLauncherService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public GrpcLauncherService(ILauncherConfigService configService)
    {
        _configService = configService;
    }

    /// <inheritdoc/>
    public UnaryResult<GetLauncherConfigResponse> GetConfigAsync()
    {
        var config = _configService.GetConfig();

        var response = new GetLauncherConfigResponse
        {
            Version = config.Version,
            Categories = config.Categories
                .Select(c => new LauncherCategoryItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    SortOrder = c.SortOrder,
                    Apps = c.Apps
                        .Where(a => a.Enabled)
                        .Select(a => new LauncherAppItem
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Description = a.Description,
                            ExePath = a.ExePath,
                            Arguments = a.Arguments,
                            WorkingDirectory = a.WorkingDirectory,
                            IconPath = a.IconPath,
                            PipeName = a.PipeName,
                            SortOrder = a.SortOrder,
                            Enabled = a.Enabled,
                        })
                        .OrderBy(a => a.SortOrder)
                        .ToList(),
                })
                .OrderBy(c => c.SortOrder)
                .ToList(),
        };

        return UnaryResult.FromResult(response);
    }
}
