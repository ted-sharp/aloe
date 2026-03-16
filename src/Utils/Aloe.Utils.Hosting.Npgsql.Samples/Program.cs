// <copyright file="Program.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Utils.Hosting.Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// 構成を読み込み
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// サービスコレクションに DbContext を登録
var services = new ServiceCollection();
services.AddNpgsqlDbContext<SampleDbContext>(
    configuration,
    "DefaultConnection",
    options: new NpgsqlDbContextOptions
    {
        AddFactory = true,
    });

// NpgsqlDataSource も登録
services.AddNpgsqlDataSource(configuration, "DefaultConnection");

Console.WriteLine("=== Npgsql DI Registration Sample ===");
Console.WriteLine("DbContext と NpgsqlDataSource が DI に登録されました。");
Console.WriteLine("（実際のDB接続にはPostgreSQLサーバーが必要です）");

/// <summary>
/// サンプル用の DbContext。
/// </summary>
public class SampleDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SampleDbContext"/> class.
    /// </summary>
    /// <param name="options">DbContext オプション。</param>
    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options)
    {
    }
}
