using Microsoft.Extensions.Configuration;

// ReSharper disable ArrangeStaticMemberQualifier

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Configuration;

public static class AloeClientGrpcConfig
{
    /// <summary>
    /// 設定から gRPC 用のURLを取得します。
    /// </summary>
    public static string GetGrpcUrl(this IConfiguration config)
    {
        var grpcUrl = config
            .GetValue<string>("Client:Targets:gRPC:Url") ?? "";
        return grpcUrl;
    }
}
