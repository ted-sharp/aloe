using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;

/// <summary>
/// ログイン, ログアウト用のサービスです。
/// </summary>
public interface IAuthGrpcService : IService<IAuthGrpcService>
{
    /// <summary>
    /// データベース接続文字列の Host/Server の記述を取得します。
    /// </summary>
    UnaryResult<string> GetDbHostAsync();

    /// <summary>
    /// データベース接続文字列の Database の記述を取得します。
    /// </summary>
    UnaryResult<string> GetDbNameAsync();

    /// <summary>
    /// DB接続とデータアクセスを試行します。
    /// EFCore は初回アクセス時にマッピングなどが行われるため時間がかかります。
    /// </summary>
    UnaryResult PreloadAsync();

    /// <summary>
    /// ログインを試行します。
    /// </summary>
    UnaryResult<LoginResult> LoginAsync(LoginRequest request);

    /// <summary>
    /// ログアウトします。
    /// </summary>
    UnaryResult LogoutAsync(SessionDto session);
}

public class AuthGrpcService : ServiceBase<IAuthGrpcService>, IAuthGrpcService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IAuthService _authService;

    public AuthGrpcService(
        ILogger<AuthGrpcService> logger,
        IDbContextFactory<AppDbContext> factory,
        IAuthService authService)
    {
        this._logger = logger;
        this._factory = factory;
        this._authService = authService;
    }

    public async UnaryResult<string> GetDbHostAsync()
    {
        return await this._authService.GetDbHostAsync();
    }

    public async UnaryResult<string> GetDbNameAsync()
    {
        return await this._authService.GetDbNameAsync();
    }

    public async UnaryResult PreloadAsync()
    {
        await this._authService.PreloadAsync();
    }

    public async UnaryResult<LoginResult> LoginAsync(LoginRequest request)
    {
        if (String.IsNullOrWhiteSpace(request.ClientEndpoint))
        {
            // スタンドアローンモードだと gRPC 経由にならないため null になる
            request.ClientEndpoint = this.Context?.CallContext.Peer ?? "";
        }

        return await this._authService.LoginAsync(request);
    }

    public async UnaryResult LogoutAsync(SessionDto sessionDto)
    {
        await this._authService.LogoutAsync(sessionDto);
    }
}
