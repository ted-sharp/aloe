using Serilog.Core;
using Serilog.Events;

namespace Aloe.Common.AloeCoreLib.Logging;

public class ActionSink : ILogEventSink
{
    private readonly Action<string> _logAction;

    public ActionSink(Action<string> logAction)
    {
        ArgumentNullException.ThrowIfNull(logAction, nameof(logAction));

        this._logAction = logAction;
    }

    public void Emit(LogEvent logEvent)
    {
        var renderedMessage = logEvent.RenderMessage();
        this._logAction(renderedMessage);
    }
}
