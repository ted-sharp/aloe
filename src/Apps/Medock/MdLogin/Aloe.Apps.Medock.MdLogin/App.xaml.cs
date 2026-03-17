// <copyright file="App.xaml.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Windows;
using Aloe.Apps.Medock.MdLogin.Models;
using Aloe.Apps.Medock.MdLogin.Services;
using Aloe.Apps.Medock.MdLogin.ViewModels;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application = System.Windows.Application;

namespace Aloe.Apps.Medock.MdLogin;

/// <summary>
/// アプリケーションのエントリポイント。
/// </summary>
public partial class App : Application
{
    private Mutex? _mutex;
    private ServiceProvider? _serviceProvider;

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, "MdLogin_SingleInstance", out var createdNew);
        if (!createdNew)
        {
            this.Shutdown();
            return;
        }

        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var options = new MdLoginClientOptions();
        configuration.GetSection("MdLogin").Bind(options);

        var services = new ServiceCollection();

        var channel = GrpcChannel.ForAddress(options.ServerAddress);
        services.AddSingleton(channel);
        services.AddSingleton<LoginGrpcClient>();
        services.AddSingleton(new SessionWriter(options.ResolvedSessionFilePath));
        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<LoginWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var window = _serviceProvider.GetRequiredService<LoginWindow>();
        var viewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
        viewModel.LoginSucceeded += (s, e) => this.Shutdown();

        window.DataContext = viewModel;
        window.Show();
    }

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        _mutex?.Dispose();

        base.OnExit(e);
    }
}
