using Aloe.Common.AloeCoreLib.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

public static class HostExtensionsSerilog
{
    /// <summary>
    /// Serilog を有効にします。
    /// </summary>
    public static IHostApplicationBuilder AddSerilog(this IHostApplicationBuilder builder)
    {
        var innerLogger = CreateOutputLogger();

        Serilog.Log.Logger = CreateBufferingLogger(innerLogger);

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog();

        return builder;
    }

    /// <summary>
    /// バッファリングを行うための中間ロガーです。
    /// </summary>
    private static Logger CreateBufferingLogger(Serilog.ILogger innerLogger)
    {
        var customSink = new BufferingSink(innerLogger, new BufferingSinkOptions
        {
            BatchSize = 1000,
            FlushInterval = TimeSpan.FromSeconds(5),
            EagerlyEmitFirstEvent = true,
        });

        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .WriteTo.Sink(customSink)
            .CreateLogger();
    }

    private static Logger CreateOutputLogger()
    {
        var configuration = new ConfigurationBuilder()
            // 優先の serilog 設定(あれば)
            .AddJsonFile("appsettings.serilog.json", optional: true, reloadOnChange: true)
            // フォールバックの設定(あれば)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var serilogConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration);

        // 設定がなかったときのフォールバック
        serilogConfiguration
            .MinimumLevel.Verbose()
            .WriteTo.Debug(
                restrictedToMinimumLevel: LogEventLevel.Debug,
                outputTemplate: SerilogDefault.Template)
            .WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                outputTemplate: SerilogDefault.Template,
                shared: true);

        if (!Console.IsOutputRedirected)
        {
            // Consoleアプリの場合のみ
            serilogConfiguration.WriteTo.Console(
                theme: AnsiConsoleTheme.Literate,
                outputTemplate: SerilogDefault.Template);
        }

        // TODO: クリティカルのときは Email を送りたい

        // TODO: 必要であればDBにも出力できるようにする？

        //if (logTextBox is not null)
        //{
        //    // json 設定には未対応
        //    serilogConfiguration.WriteTo.RichTextBox(
        //        logTextBox,
        //        theme: RichTextBoxConsoleTheme.Literate);
        //}

        return serilogConfiguration.CreateLogger();
    }
}
