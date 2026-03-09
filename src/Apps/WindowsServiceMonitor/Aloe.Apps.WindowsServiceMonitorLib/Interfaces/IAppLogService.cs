using Aloe.Apps.WindowsServiceMonitorLib.Models;

namespace Aloe.Apps.WindowsServiceMonitorLib.Interfaces;

public interface IAppLogService
{
    List<AppLogConfig> GetAppConfigs();

    List<string> GetLogFiles(string appName);

    (List<string> Lines, int TotalLineCount) ReadLogFile(string appName, string fileName, int page, int pageSize);
}
