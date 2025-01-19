using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Services;

public interface ILogLevelService
{
    void SetLogLevel(LogLevel logLevel);
}

public class SerilogLogLevelService : ILogLevelService
{
    public static Serilog.Core.LoggingLevelSwitch Switch { get; } = new();

    public void SetLogLevel(LogLevel logLevel)
    {
        SerilogLogLevelService.Switch.MinimumLevel = logLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            // Serilogには「None」に対応するレベルがないため、最も高いレベルにマッピング
            LogLevel.None => LogEventLevel.Fatal,
            _ => LogEventLevel.Fatal,
        };
    }
}
