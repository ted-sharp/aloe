// <copyright file="App.xaml.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Windows;
using Aloe.Apps.Medock.MdPatientFinder.Services;
using Aloe.Apps.Medock.MdPatientFinder.ViewModels;
using Aloe.Apps.Medock.MdPatientLib.Contracts.Services;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.Medock.MdPatientFinder;

/// <summary>
/// アプリケーションのエントリポイント。
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    /// <summary>
    /// <see cref="App"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public App()
    {
        var services = new ServiceCollection();

        var channel = GrpcChannel.ForAddress("http://localhost:5091");
        services.AddSingleton(channel);
        services.AddSingleton(MagicOnionClient.Create<IPatientService>(channel));
        services.AddSingleton<ViewerLauncher>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
