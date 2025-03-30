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
        return builder.AddSerilog(null!);
    }

    /// <summary>
    /// Serilog を有効にします。追加の Sink を指定できます。
    /// </summary>
    public static IHostApplicationBuilder AddSerilog(this IHostApplicationBuilder builder, params ILogEventSink[] additionalSinks)
    {
        var innerLogger = CreateOutputLogger(additionalSinks);

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
            //.ReadFrom.Configuration()
            //.MinimumLevel.ControlledBy(logLevelSwitch)
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .WriteTo.Sink(customSink)
            .CreateLogger();
    }

    private static Logger CreateOutputLogger(params ILogEventSink[] additionalSinks)
    {
        var template = "[{Timestamp:HH:mm:ss}][{Level:u3}] {Message:lj} (TID: {ThreadId}){NewLine}{Exception}";
        //var template = "{SourceContext} [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (TID: {ThreadId}){NewLine}{Exception}";

        var serilogConfiguration = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            //.MinimumLevel.ControlledBy(logLevelSwitch)
            .WriteTo.Debug(
                restrictedToMinimumLevel: LogEventLevel.Debug,
                outputTemplate: template)
            .WriteTo.Console(
                theme: AnsiConsoleTheme.Literate,
                outputTemplate: template)
            .WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                outputTemplate: template,
                shared: true);

        // TODO: クリティカルのときは Email を送りたい

        // TODO: 必要であればDBにも出力できるようにする？

        foreach (var sink in additionalSinks ?? [])
        {
            serilogConfiguration.WriteTo.Sink(sink);
        }

        return serilogConfiguration.CreateLogger();
    }

}
