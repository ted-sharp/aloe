// <copyright file="GrpcLoginService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Contracts.Services;
using Aloe.Apps.Medock.MdLauncherLib.Services;
using MagicOnion;
using MagicOnion.Server;

namespace Aloe.Apps.Medock.MdLauncherApi.Grpc;

/// <summary>
/// ログインサービスの MagicOnion gRPC 実装。
/// </summary>
public class GrpcLoginService : ServiceBase<ILoginService>, ILoginService
{
    private readonly ILoginUserService _loginUserService;

    /// <summary>
    /// <see cref="GrpcLoginService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public GrpcLoginService(ILoginUserService loginUserService)
    {
        _loginUserService = loginUserService;
    }

    /// <inheritdoc/>
    public async UnaryResult<LoginResponse> LoginAsync(LoginRequest request)
    {
        var (success, userId, errorMessage) = await _loginUserService.ValidateAsync(request.UserCode, request.Password);

        if (!success)
        {
            return new LoginResponse
            {
                Success = false,
                ErrorMessage = errorMessage,
            };
        }

        return new LoginResponse
        {
            Success = true,
            UserCode = request.UserCode,
            UserId = userId,
            SessionKey = Guid.CreateVersion7(),
            IssuedAt = DateTimeOffset.Now,
        };
    }
}
