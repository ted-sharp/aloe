using Aloe.Apps.FileBridge.Extensions;
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

    // FileBridge サービスの登録
    builder.Services.AddFileBridgeServices(builder.Configuration);

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
