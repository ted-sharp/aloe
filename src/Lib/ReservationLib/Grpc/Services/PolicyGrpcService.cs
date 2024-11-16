using AloeReservationGrid.Lib.ReservationLib.Data.EFCore;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Services;

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
    private readonly AppDbContext _context;

    public PolicyGrpcService(
        ILogger logger,
        AppDbContext context)
    {
        this._logger = logger;
        this._context = context;
    }

    public async UnaryResult<List<PolicyDto>> FetchPolicyDtosAsync()
    {
        var policies = await this._context.Policies
            .Where(x => x.IsDeleted == false)
            .Select(x => x.ToPolicyDto())
            .ToListAsync();
        return policies;
    }
}
