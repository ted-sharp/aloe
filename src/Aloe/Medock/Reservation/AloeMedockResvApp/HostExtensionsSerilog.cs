using Aloe.Medock.Reservation.AloeMedockResvLib.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Windows.Controls;
using Serilog.Sinks.RichTextBox.Themes;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

public static class HostExtensionsSerilog
{
    /// <summary>
    /// Serilog を有効にします。
    /// </summary>
    public static IHostApplicationBuilder AddSerilog(this IHostApplicationBuilder builder, RichTextBox? logTextBox)
    {
        var innerLogger = HostExtensionsSerilog.CreateOutputLogger(logTextBox);

        Serilog.Log.Logger = HostExtensionsSerilog.CreateBufferingLogger(innerLogger);

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog();

        return builder;
    }

    /// <summary>
    /// バッファリングを行うための中間ロガーです。
    /// </summary>
    private static Serilog.Core.Logger CreateBufferingLogger(Serilog.ILogger innerLogger)
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

    private static Serilog.Core.Logger CreateOutputLogger(RichTextBox? logTextBox)
    {
        //var template = "{SourceContext} [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (TID: {ThreadId}){NewLine}{Exception}";
        var template = "[{Timestamp:HH:mm:ss}][{Level:u3}] {Message:lj} (TID: {ThreadId}){NewLine}{Exception}";

        var serilogConfiguration = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            //.MinimumLevel.ControlledBy(logLevelSwitch)
            .WriteTo.Debug(
                restrictedToMinimumLevel: LogEventLevel.Debug,
                outputTemplate: template)
            //.WriteTo.Console(
            //    theme: AnsiConsoleTheme.Literate,
            //    outputTemplate: template)
            .WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                outputTemplate: template,
                shared: true);

        if (logTextBox is not null)
        {
            serilogConfiguration.WriteTo.RichTextBox(
                logTextBox,
                theme: RichTextBoxConsoleTheme.Literate);
        }

        return serilogConfiguration.CreateLogger();
    }

}
