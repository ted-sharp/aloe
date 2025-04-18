using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AloeSsmixSample;

// ここでは汎用的な設定を行うため、CA1859 は抑制します。
#pragma warning disable IDE0079 // 不要な抑制を削除する (IDE0079)
#pragma warning disable CA1859 // パフォーマンスの向上のために可能な場合は具象型を使用する

internal static class HostExtensions
{
    /// <summary>
    /// 構成の追加を行います。
    /// </summary>
    internal static T ConfigureAloeSsmixSample<T>(this T builder)
        where T : IHostApplicationBuilder
    {
        builder
            .AddSecrets()
            .AddSerilog();

        return builder;
    }

    private static IHostApplicationBuilder AddSecrets(this IHostApplicationBuilder builder)
    {
        // ユーザーシークレットIDが .csproj に設定されている前提
        builder.Configuration.AddUserSecrets<App>(optional: true);

        return builder;
    }

}
