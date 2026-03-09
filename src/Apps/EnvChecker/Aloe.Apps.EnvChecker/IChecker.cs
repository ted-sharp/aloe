namespace Aloe.Apps.EnvChecker;

/// <summary>
/// Each environment checker implements this interface.
/// </summary>
internal interface IChecker
{
    /// <summary>
    /// Section key used for profile/CLI filtering (e.g. "system", "cpu", "disk").
    /// </summary>
    string SectionKey { get; }

    /// <summary>
    /// Human-readable section title displayed in output (e.g. "System Information").
    /// </summary>
    string SectionTitle { get; }

    /// <summary>
    /// Run the check and write results to the given writer.
    /// </summary>
    Task RunAsync(TextWriter writer, CheckProfile profile);
}
