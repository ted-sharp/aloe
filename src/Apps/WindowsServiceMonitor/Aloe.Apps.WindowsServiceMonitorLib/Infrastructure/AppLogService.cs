using Aloe.Apps.WindowsServiceMonitorLib.Interfaces;
using Aloe.Apps.WindowsServiceMonitorLib.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.WindowsServiceMonitorLib.Infrastructure;

public class AppLogService : IAppLogService
{
    private readonly List<AppLogConfig> _configs;
    private readonly ILogger<AppLogService> _logger;

    public AppLogService(IOptions<List<AppLogConfig>> configs, ILogger<AppLogService> logger)
    {
        _configs = configs.Value;
        _logger = logger;
    }

    public List<AppLogConfig> GetAppConfigs() => _configs;

    public List<string> GetLogFiles(string appName)
    {
        var config = _configs.FirstOrDefault(c => c.Name == appName);
        if (config == null || !Directory.Exists(config.LogDirectory))
        {
            return [];
        }

        return Directory.GetFiles(config.LogDirectory, config.FilePattern)
            .OrderByDescending(f => File.GetLastWriteTime(f))
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>()
            .ToList();
    }

    public (List<string> Lines, int TotalLineCount) ReadLogFile(string appName, string fileName, int page, int pageSize)
    {
        var config = _configs.FirstOrDefault(c => c.Name == appName);
        if (config == null)
        {
            return ([], 0);
        }

        var filePath = Path.GetFullPath(Path.Combine(config.LogDirectory, fileName));
        var logDirFullPath = Path.GetFullPath(config.LogDirectory);

        // パストラバーサル防止
        if (!filePath.StartsWith(logDirFullPath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("パストラバーサルの試行を検出: {FileName}", fileName);
            return ([], 0);
        }

        if (!File.Exists(filePath))
        {
            return ([], 0);
        }

        try
        {
            var allLines = File.ReadAllLines(filePath);
            var reversed = allLines.Reverse().ToArray();
            var totalLineCount = reversed.Length;

            var skip = page * pageSize;
            var lines = reversed.Skip(skip).Take(pageSize).ToList();

            return (lines, totalLineCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ログファイルの読み込みに失敗: {FilePath}", filePath);
            return ([], 0);
        }
    }
}
