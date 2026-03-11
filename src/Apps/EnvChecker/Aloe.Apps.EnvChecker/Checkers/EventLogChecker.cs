using System.Diagnostics;

namespace Aloe.Apps.EnvChecker.Checkers;

internal sealed class EventLogChecker : IChecker
{
    public string SectionKey => "eventlog";

    public string SectionTitle => "Windows Event Log";

    public Task RunAsync(TextWriter writer, CheckProfile profile)
    {
        var config = profile.EventLog;
        var since = DateTime.Now.AddHours(-config.Hours);

        foreach (var logName in config.LogNames)
        {
            writer.WriteLine($"  [{logName}] (past {config.Hours} hours, errors only)");

            try
            {
                using var log = new System.Diagnostics.EventLog(logName);
                var errorEntries = log.Entries.Cast<EventLogEntry>()
                    .Where(e => e.TimeGenerated >= since
                             && e.EntryType == EventLogEntryType.Error
                             && !config.IgnoreSources.Contains(e.Source, StringComparer.OrdinalIgnoreCase))
                    .OrderByDescending(e => e.TimeGenerated)
                    .ToList();

                writer.WriteLine($"    Error count: {errorEntries.Count}");

                var top = errorEntries.Take(config.MaxEntries);
                foreach (var entry in top)
                {
                    var msg = Truncate(entry.Message, 120);
                    writer.WriteLine($"    {entry.TimeGenerated:yyyy-MM-dd HH:mm:ss}  [{entry.Source}]");
                    writer.WriteLine($"      {msg}");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"    Failed to read: {ex.Message}");
            }

            writer.WriteLine();
        }

        return Task.CompletedTask;
    }

    private static string Truncate(string text, int maxLength)
    {
        var singleLine = text.ReplaceLineEndings(" ");
        return singleLine.Length <= maxLength
            ? singleLine
            : string.Concat(singleLine.AsSpan(0, maxLength), "...");
    }
}
