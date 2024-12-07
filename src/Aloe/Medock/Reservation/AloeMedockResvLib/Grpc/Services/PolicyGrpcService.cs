using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;

/// <summary>
/// ポリシー用のサービスです。
/// </summary>
public interface IPolicyGrpcService : IService<IPolicyGrpcService>
{
    UnaryResult<List<PolicyDto>> FetchPolicyDtosAsync();
}

public class PolicyGrpcService : ServiceBase<IPolicyGrpcService>, IPolicyGrpcService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;

    public PolicyGrpcService(
        ILogger logger,
        IDbContextFactory<AppDbContext> factory)
    {
        this._logger = logger;
        this._factory = factory;
    }

    public async UnaryResult<List<PolicyDto>> FetchPolicyDtosAsync()
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var policies = await context.Policies
            .AsNoTracking()
            .Where(x => x.IsDeleted == false)
            .Select(x => x.ToPolicyDto())
            .ToListAsync();
        return policies;
    }
}
