// <copyright file="LauncherConfigService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text.Json;
using Aloe.Apps.Medock.MdLauncherLib.Models;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.Medock.MdLauncherLib.Services;

/// <summary>
/// JSON ファイルからナビゲーション設定を読み取り、変更を監視するサービス。
/// </summary>
public class LauncherConfigService : ILauncherConfigService, IDisposable
{
    private readonly string _configFilePath;
    private readonly ILogger<LauncherConfigService> _logger;
    private readonly FileSystemWatcher? _watcher;
    private LauncherConfig _cachedConfig;
    private bool _disposed;

    /// <inheritdoc/>
    public event EventHandler? ConfigChanged;

    /// <summary>
    /// <see cref="LauncherConfigService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public LauncherConfigService(string configFilePath, ILogger<LauncherConfigService> logger)
    {
        _configFilePath = configFilePath;
        _logger = logger;
        _cachedConfig = LoadConfig();

        var directory = Path.GetDirectoryName(Path.GetFullPath(configFilePath));
        var fileName = Path.GetFileName(configFilePath);

        if (directory != null && Directory.Exists(directory))
        {
            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                EnableRaisingEvents = true,
            };
            _watcher.Changed += this.OnConfigFileChanged;
            _watcher.Created += this.OnConfigFileChanged;
        }
    }

    /// <inheritdoc/>
    public LauncherConfig GetConfig() => _cachedConfig;

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            // FileSystemWatcher は短時間に複数回発火することがあるため少し待つ
            Thread.Sleep(100);
            _cachedConfig = LoadConfig();
            _logger.LogInformation("設定ファイルが更新されました: {Path}", _configFilePath);
            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定ファイルの再読み込みに失敗しました: {Path}", _configFilePath);
        }
    }

    private LauncherConfig LoadConfig()
    {
        if (!File.Exists(_configFilePath))
        {
            _logger.LogWarning("設定ファイルが見つかりません: {Path}", _configFilePath);
            return new LauncherConfig();
        }

        try
        {
            var json = File.ReadAllText(_configFilePath);
            return JsonSerializer.Deserialize<LauncherConfig>(json) ?? new LauncherConfig();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "設定ファイルの解析に失敗しました: {Path}", _configFilePath);
            return new LauncherConfig();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _watcher?.Dispose();
        _disposed = true;
    }
}
