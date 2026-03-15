// <copyright file="NpgsqlExtensionsTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Aloe.Utils.Hosting.Npgsql.Tests;

/// <summary>
/// テスト用の DbContext。
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }
}

public class NpgsqlExtensionsTests
{
    private const string TestConnectionString = "Host=localhost;Database=test_db;Username=test;Password=test";

    private static IConfiguration CreateConfiguration(string? connectionString = TestConnectionString)
    {
        var dict = new Dictionary<string, string?>();
        if (connectionString != null)
        {
            dict["ConnectionStrings:Default"] = connectionString;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    private static CoreOptionsExtension GetCoreExtension(TestDbContext context)
    {
        var options = ((IInfrastructure<IServiceProvider>)context).Instance
            .GetRequiredService<IDbContextOptions>();
        return options.FindExtension<CoreOptionsExtension>()!;
    }

    [Fact(DisplayName = "AddNpgsqlDbContext: DbContext が DI 解決できること")]
    public void AddNpgsqlDbContext_ResolvesDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddNpgsqlDbContext<TestDbContext>(configuration, "Default");

        // Assert
        var sp = services.BuildServiceProvider();
        var context = sp.GetService<TestDbContext>();
        Assert.NotNull(context);
    }

    [Fact(DisplayName = "AddNpgsqlDbContext: 接続文字列が見つからない場合に InvalidOperationException")]
    public void AddNpgsqlDbContext_MissingConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(connectionString: null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            services.AddNpgsqlDbContext<TestDbContext>(configuration, "Default"));
    }

    [Fact(DisplayName = "AddNpgsqlDbContext: Development 環境で DetailedErrors / SensitiveDataLogging が有効")]
    public void AddNpgsqlDbContext_Development_EnablesDetailedErrorsAndSensitiveDataLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        var hostEnv = new TestHostEnvironment { EnvironmentName = "Development" };
        services.AddSingleton<IHostEnvironment>(hostEnv);

        // Act
        services.AddNpgsqlDbContext<TestDbContext>(configuration, "Default");

        // Assert
        var sp = services.BuildServiceProvider();
        var context = sp.GetRequiredService<TestDbContext>();
        var coreExtension = GetCoreExtension(context);
        Assert.True(coreExtension.DetailedErrorsEnabled);
        Assert.True(coreExtension.IsSensitiveDataLoggingEnabled);
    }

    [Fact(DisplayName = "AddNpgsqlDbContext: Production 環境では DetailedErrors / SensitiveDataLogging が無効")]
    public void AddNpgsqlDbContext_Production_DisablesDetailedErrorsAndSensitiveDataLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        var hostEnv = new TestHostEnvironment { EnvironmentName = "Production" };
        services.AddSingleton<IHostEnvironment>(hostEnv);

        // Act
        services.AddNpgsqlDbContext<TestDbContext>(configuration, "Default");

        // Assert
        var sp = services.BuildServiceProvider();
        var context = sp.GetRequiredService<TestDbContext>();
        var coreExtension = GetCoreExtension(context);
        Assert.False(coreExtension.DetailedErrorsEnabled);
        Assert.False(coreExtension.IsSensitiveDataLoggingEnabled);
    }

    [Fact(DisplayName = "AddNpgsqlDbContext: SuppressSqlLogging で NullLoggerFactory 適用")]
    public void AddNpgsqlDbContext_SuppressSqlLogging_UsesNullLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        var options = new NpgsqlDbContextOptions { SuppressSqlLogging = true };

        // Act
        services.AddNpgsqlDbContext<TestDbContext>(configuration, "Default", options: options);

        // Assert
        var sp = services.BuildServiceProvider();
        var context = sp.GetRequiredService<TestDbContext>();
        var coreExtension = GetCoreExtension(context);
        Assert.IsType<Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory>(coreExtension.LoggerFactory);
    }

    [Fact(DisplayName = "AddNpgsqlDbContext: AddFactory = true で IDbContextFactory<T> 解決可能")]
    public void AddNpgsqlDbContext_AddFactory_ResolvesDbContextFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        var options = new NpgsqlDbContextOptions { AddFactory = true };

        // Act
        services.AddNpgsqlDbContext<TestDbContext>(configuration, "Default", options: options);

        // Assert
        var sp = services.BuildServiceProvider();
        var factory = sp.GetService<IDbContextFactory<TestDbContext>>();
        Assert.NotNull(factory);
    }

    [Fact(DisplayName = "AddNpgsqlDbContext: configure コールバックが実行されること")]
    public void AddNpgsqlDbContext_InvokesConfigureCallback()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        var callbackInvoked = false;

        // Act
        services.AddNpgsqlDbContext<TestDbContext>(configuration, "Default", configure: options =>
        {
            callbackInvoked = true;
        });

        // Assert — コールバックは DbContext 解決時に実行される
        var sp = services.BuildServiceProvider();
        _ = sp.GetRequiredService<TestDbContext>();
        Assert.True(callbackInvoked);
    }

    [Fact(DisplayName = "AddNpgsqlDataSource: NpgsqlDataSource が DI 登録されること")]
    public void AddNpgsqlDataSource_RegistersDataSource()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddNpgsqlDataSource(configuration, "Default");

        // Assert
        var sp = services.BuildServiceProvider();
        var dataSource = sp.GetService<NpgsqlDataSource>();
        Assert.NotNull(dataSource);
    }

    [Fact(DisplayName = "AddNpgsqlDataSource: 接続文字列が見つからない場合に InvalidOperationException")]
    public void AddNpgsqlDataSource_MissingConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(connectionString: null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            services.AddNpgsqlDataSource(configuration, "Default"));
    }

    [Fact(DisplayName = "AddNpgsqlDbContext: services が null の場合に ArgumentNullException をスロー")]
    public void AddNpgsqlDbContext_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        var configuration = CreateConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services!.AddNpgsqlDbContext<TestDbContext>(configuration, "Default"));
    }

    [Fact(DisplayName = "AddNpgsqlDbContext: configuration が null の場合に ArgumentNullException をスロー")]
    public void AddNpgsqlDbContext_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration? configuration = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddNpgsqlDbContext<TestDbContext>(configuration!, "Default"));
    }

    [Fact(DisplayName = "AddNpgsqlDbContext: connectionStringName が null の場合に ArgumentNullException をスロー")]
    public void AddNpgsqlDbContext_NullConnectionStringName_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddNpgsqlDbContext<TestDbContext>(configuration, null!));
    }

    [Fact(DisplayName = "AddNpgsqlDataSource: services が null の場合に ArgumentNullException をスロー")]
    public void AddNpgsqlDataSource_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        var configuration = CreateConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services!.AddNpgsqlDataSource(configuration, "Default"));
    }

    [Fact(DisplayName = "AddNpgsqlDataSource: configuration が null の場合に ArgumentNullException をスロー")]
    public void AddNpgsqlDataSource_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration? configuration = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddNpgsqlDataSource(configuration!, "Default"));
    }

    /// <summary>
    /// テスト用の <see cref="IHostEnvironment"/> 実装。
    /// </summary>
    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";

        public string ApplicationName { get; set; } = "Test";

        public string ContentRootPath { get; set; } = string.Empty;

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
            = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
