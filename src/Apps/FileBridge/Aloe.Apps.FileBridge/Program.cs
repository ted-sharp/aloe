using Aloe.Apps.FileBridge.Extensions;
using Aloe.Utils.Hosting.Serilog;

// WindowsサービスはSCM起動時に作業ディレクトリがSystem32になるため、
// 実行ファイルのディレクトリに明示的に変更する
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = Host.CreateApplicationBuilder(args);
builder.AddSerilogDefaults();

// Windows Serviceとして動作するための設定
builder.Services.AddWindowsService(options => { options.ServiceName = "FileBridge"; });

// FileBridge サービスの登録
builder.Services.AddFileBridgeServices(builder.Configuration);

var host = builder.Build();
host.Run();
