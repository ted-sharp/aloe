// <copyright file="NpgsqlDbContextOptions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Utils.Hosting.Npgsql;

/// <summary>
/// <see cref="NpgsqlExtensions.AddNpgsqlDbContext{TContext}"/> のオプション。
/// </summary>
public sealed class NpgsqlDbContextOptions
{
    /// <summary>
    /// SQL ログを抑制するかどうかを取得します。
    /// true の場合、<see cref="Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory"/> が適用されます。
    /// </summary>
    public bool SuppressSqlLogging { get; init; }

    /// <summary>
    /// DbContext のサービスライフタイムを取得します。デフォルトは <see cref="ServiceLifetime.Scoped"/>。
    /// </summary>
    public ServiceLifetime Lifetime { get; init; } = ServiceLifetime.Scoped;

    /// <summary>
    /// <see cref="IDbContextFactory{TContext}"/> も同時に DI 登録するかどうかを取得します。
    /// </summary>
    public bool AddFactory { get; init; }
}
