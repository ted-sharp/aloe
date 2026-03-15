// <copyright file="App.xaml.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Windows;
using Aloe.Apps.Medock.MdPatientEditor.Services;
using Aloe.Apps.Medock.MdPatientEditor.ViewModels;
using Aloe.Apps.Medock.MdPatientLib.Contracts.Services;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.Medock.MdPatientEditor;

/// <summary>
/// アプリケーションのエントリポイント。
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;
    private PipeServer? _pipeServer;

    /// <summary>
    /// <see cref="App"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public App()
    {
        var services = new ServiceCollection();

        var channel = GrpcChannel.ForAddress("http://localhost:5091");
        services.AddSingleton(channel);
        services.AddSingleton(MagicOnionClient.Create<IPatientService>(channel));
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <inheritdoc/>
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        var viewModel = (MainViewModel)mainWindow.DataContext;

        // NamedPipe サーバーを起動
        _pipeServer = new PipeServer("MdPatient_Editor");
        _pipeServer.PatientIdReceived += patientId =>
        {
            mainWindow.Dispatcher.Invoke(async () =>
            {
                await viewModel.LoadPatientCommand.ExecuteAsync(patientId);
                mainWindow.Activate();
            });
        };
        _pipeServer.Start();

        mainWindow.Show();

        // 起動引数で PatientId が指定されていれば読み込む
        var args = Environment.GetCommandLineArgs();
        var ptIdIndex = Array.IndexOf(args, "--patient-id");
        if (ptIdIndex >= 0 && ptIdIndex + 1 < args.Length && Guid.TryParse(args[ptIdIndex + 1], out var patientId))
        {
            await viewModel.LoadPatientCommand.ExecuteAsync(patientId);
        }
    }

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e)
    {
        _pipeServer?.Dispose();
        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}
