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
    private readonly ILauncherMenuService _menuService;

    /// <summary>
    /// <see cref="GrpcLauncherService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public GrpcLauncherService(ILauncherMenuService menuService)
    {
        _menuService = menuService;
    }

    /// <inheritdoc/>
    public async UnaryResult<GetLauncherConfigResponse> GetConfigAsync()
    {
        return await this.GetMenusByUserAsync(new GetMenusByUserRequest { UserCode = string.Empty });
    }

    /// <inheritdoc/>
    public async UnaryResult<GetLauncherConfigResponse> GetMenusByUserAsync(GetMenusByUserRequest request)
    {
        var menus = await _menuService.GetMenusByUserCodeAsync(request.UserCode);

        var response = new GetLauncherConfigResponse
        {
            Version = 1,
            Categories =
            [
                new LauncherCategoryItem
                {
                    Id = "menu",
                    Name = "メニュー",
                    SortOrder = 1,
                    Apps = menus
                        .Select(m => new LauncherAppItem
                        {
                            Id = m.MenuCode,
                            Name = m.MenuName,
                            Description = m.MenuDesc,
                            ExePath = m.AppPath,
                            Arguments = m.AppArgs,
                            WorkingDirectory = m.AppDir,
                            IconPath = m.AppIcon,
                            PipeName = m.PipeName,
                            SortOrder = m.MenuSeq,
                            Enabled = true,
                        })
                        .ToList(),
                },
            ],
        };

        return response;
    }
}
