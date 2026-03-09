using Aloe.Apps.FileBridge;
using Aloe.Apps.FileBridgeLib.Models;
using Aloe.Apps.FileBridgeLib.Services;
using Serilog;

// WindowsサービスはSCM起動時に作業ディレクトリがSystem32になるため、
// 実行ファイルのディレクトリに明示的に変更する
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Serilogの設定
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    builder.Services.AddSerilog();

    // Windows Serviceとして動作するための設定
    builder.Services.AddWindowsService(options => { options.ServiceName = "FileBridge"; });

    // FileBridge設定の読み込み
    var fileBridgeOptions = builder.Configuration.GetSection("FileBridge").Get<FileBridgeOptions>() ?? new FileBridgeOptions();
    builder.Services.Configure<FileBridgeOptions>(builder.Configuration.GetSection("FileBridge"));

    // サービスの登録（順序が重要）
    builder.Services.AddSingleton<OperationLogService>(sp =>
    {
        return new OperationLogService(fileBridgeOptions);
    });

    builder.Services.AddSingleton<ProcessLauncherService>(sp =>
    {
        var logService = sp.GetRequiredService<OperationLogService>();
        var logger = sp.GetService<ILogger<ProcessLauncherService>>();
        return new ProcessLauncherService(fileBridgeOptions, logService, logger);
    });

    builder.Services.AddSingleton<FileWatcherService>(sp =>
    {
        var logService = sp.GetRequiredService<OperationLogService>();
        var processLauncher = sp.GetRequiredService<ProcessLauncherService>();
        var logger = sp.GetService<ILogger<FileWatcherService>>();
        return new FileWatcherService(fileBridgeOptions, logService, processLauncher, logger);
    });

    // Workerの登録
    builder.Services.AddHostedService<Worker>();

    Log.Information("アプリケーションを起動しています...");

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "アプリケーションの起動中に致命的なエラーが発生しました");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
