// <copyright file="App.xaml.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Windows;
using Aloe.Apps.Medock.MdLauncher.Models;
using Application = System.Windows.Application;
using Aloe.Apps.Medock.MdLauncher.Services;
using Aloe.Apps.Medock.MdLauncher.ViewModels;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.Medock.MdLauncher;

/// <summary>
/// アプリケーションのエントリポイント。
/// </summary>
public partial class App : Application
{
    private Mutex? _mutex;
    private ServiceProvider? _serviceProvider;
    private TrayIconManager? _trayIcon;
    private GlobalHotkeyService? _hotkeyService;
    private HotCornerService? _hotCornerService;
    private MainWindow? _mainWindow;

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        // 単一インスタンス制御
        _mutex = new Mutex(true, "MdLauncher_SingleInstance", out var createdNew);
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

        var options = new MdLauncherClientOptions();
        configuration.GetSection("MdLauncher").Bind(options);

        var services = new ServiceCollection();

        var channel = GrpcChannel.ForAddress(options.ServerAddress);
        services.AddSingleton(channel);
        services.AddSingleton<LauncherGrpcClient>();
        services.AddSingleton<AppLauncher>();
        services.AddSingleton(options.Behavior);
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        _mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();

        // トレイアイコン
        _trayIcon = new TrayIconManager();
        _trayIcon.ShowMenuRequested += async (s, e) => await viewModel.ShowMenuCommand.ExecuteAsync(null);
        _trayIcon.ExitRequested += (s, e) => this.Shutdown();

        // グローバルホットキー
        if (options.Hotkey.Enabled)
        {
            _mainWindow.Show();
            _mainWindow.Hide();

            _hotkeyService = new GlobalHotkeyService();
            _hotkeyService.HotkeyPressed += async (s, e) => await viewModel.ShowMenuCommand.ExecuteAsync(null);
            _hotkeyService.Register(_mainWindow, options.Hotkey);
        }

        // ホットコーナー
        if (options.HotCorner.Enabled)
        {
            _hotCornerService = new HotCornerService(options.HotCorner);
            _hotCornerService.HotCornerActivated += (s, e) =>
                this.Dispatcher.Invoke(async () => await viewModel.ShowMenuCommand.ExecuteAsync(null));
            _hotCornerService.Start();
        }
    }

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyService?.Dispose();
        _hotCornerService?.Dispose();
        _trayIcon?.Dispose();
        _serviceProvider?.Dispose();
        _mutex?.Dispose();

        base.OnExit(e);
    }
}
