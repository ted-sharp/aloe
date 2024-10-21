using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloeReservationGrid.Lib.CoreLib.Logging;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.App.ReservationApp;

/// <summary>
/// Host を実行すると呼び出されます。
/// </summary>
/// <remarks>
/// エントリポイントからの起動は次の通りです。
/// Program.Main → WpfHostService.StartAsync → MainWindow
/// </remarks>
internal class WpfHostService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly Application _app;

    public WpfHostService(
        ILogger<WpfHostService> logger,
        IHostApplicationLifetime lifetime,
        Application app)
    {
        this._logger = logger;
        this._logger.Info("初期化");

        this._lifetime = lifetime;
        this._app = app;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this._logger.Info("開始");
        this._app.Run();

        this._lifetime.StopApplication();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this._logger.Info("終了");
        return Task.CompletedTask;
    }
}
