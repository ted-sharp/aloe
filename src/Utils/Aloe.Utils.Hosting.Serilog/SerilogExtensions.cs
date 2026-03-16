// <copyright file="SerilogExtensions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Aloe.Utils.Hosting.Serilog;

/// <summary>
/// Serilog の標準設定を一括で追加する拡張メソッドを提供します。
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// <see cref="IHostBuilder"/> に対して、Serilog の標準設定を追加します。
    /// Worker / CLI / Blazor 共通で使用できます。
    /// </summary>
    /// <param name="hostBuilder">ホストビルダーインスタンス。</param>
    /// <param name="configure">
    /// アプリ固有のカスタマイズ用コールバック。
    /// <see cref="LoggerConfiguration"/> に対して追加の設定を行えます。null の場合は無視されます。
    /// </param>
    /// <returns>チェーン呼び出し可能なホストビルダー。</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="hostBuilder"/> が null の場合にスローされます。
    /// </exception>
    public static IHostBuilder AddSerilogDefaults(
        this IHostBuilder hostBuilder,
        Action<LoggerConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        hostBuilder.UseSerilog((context, loggerConfiguration) =>
        {
            ConfigureFromSettings(loggerConfiguration, context.Configuration, configure);
        });

        return hostBuilder;
    }

    /// <summary>
    /// <see cref="HostApplicationBuilder"/> に対して、Serilog の標準設定を追加します。
    /// Worker / CLI / Blazor 共通で使用できます。
    /// </summary>
    /// <param name="builder">ホストアプリケーションビルダーインスタンス。</param>
    /// <param name="configure">
    /// アプリ固有のカスタマイズ用コールバック。
    /// <see cref="LoggerConfiguration"/> に対して追加の設定を行えます。null の場合は無視されます。
    /// </param>
    /// <returns>チェーン呼び出し可能なホストアプリケーションビルダー。</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> が null の場合にスローされます。
    /// </exception>
    public static HostApplicationBuilder AddSerilogDefaults(
        this HostApplicationBuilder builder,
        Action<LoggerConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            ConfigureFromSettings(loggerConfiguration, builder.Configuration, configure);
        });

        return builder;
    }

    /// <summary>
    /// Host 不使用の CLI 用に、<see cref="IConfiguration"/> から Serilog を設定します。
    /// <see cref="Log.Logger"/> を直接設定し、戻り値の <see cref="IDisposable"/> を Dispose すると
    /// <see cref="Log.CloseAndFlush"/> が呼ばれます。
    /// </summary>
    /// <param name="configuration">構成インスタンス。</param>
    /// <param name="configure">
    /// アプリ固有のカスタマイズ用コールバック。null の場合は無視されます。
    /// </param>
    /// <returns>
    /// Dispose 時に <see cref="Log.CloseAndFlush"/> を呼び出す <see cref="IDisposable"/>。
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="configuration"/> が null の場合にスローされます。
    /// </exception>
    public static IDisposable ConfigureSerilogFromSettings(
        IConfiguration configuration,
        Action<LoggerConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var loggerConfiguration = new LoggerConfiguration();
        ConfigureFromSettings(loggerConfiguration, configuration, configure);

        Log.Logger = loggerConfiguration.CreateLogger();

        return new SerilogCleanup();
    }

    /// <summary>
    /// <see cref="LoggerConfiguration"/> に対して appsettings の Serilog セクションを読み取り、
    /// デフォルトシンクの追加とカスタマイズコールバックの実行を行います。
    /// </summary>
    /// <param name="loggerConfiguration">ロガー構成。</param>
    /// <param name="configuration">構成インスタンス。</param>
    /// <param name="configure">カスタマイズ用コールバック。</param>
    private static void ConfigureFromSettings(
        LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        Action<LoggerConfiguration>? configure)
    {
        loggerConfiguration.ReadFrom.Configuration(configuration);

        // MinimumLevel:Override が未設定の場合のみデフォルトを追加
        var overrideSection = configuration.GetSection("Serilog:MinimumLevel:Override");
        if (!overrideSection.GetChildren().Any())
        {
            loggerConfiguration
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Npgsql", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning);
        }

        // WriteTo セクションが未設定の場合のみデフォルトシンクを追加
        var writeToSection = configuration.GetSection("Serilog:WriteTo");
        if (!writeToSection.GetChildren().Any())
        {
            loggerConfiguration
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/app-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30);
        }

        configure?.Invoke(loggerConfiguration);
    }

    /// <summary>
    /// Dispose 時に <see cref="Log.CloseAndFlush"/> を呼び出すヘルパークラス。
    /// </summary>
    private sealed class SerilogCleanup : IDisposable
    {
        public void Dispose()
        {
            Log.CloseAndFlush();
        }
    }
}
