namespace Aloe.Apps.WindowsServiceMonitorLib.Models;

public class AppLogConfig
{
    public string Name { get; set; } = string.Empty;

    public string LogDirectory { get; set; } = string.Empty;

    public string FilePattern { get; set; } = "*.log";
}
