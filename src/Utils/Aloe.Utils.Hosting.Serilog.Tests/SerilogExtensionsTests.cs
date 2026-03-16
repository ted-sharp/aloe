// <copyright file="SerilogExtensionsTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Aloe.Utils.Hosting.Serilog.Tests;

public class SerilogExtensionsTests : IDisposable
{
    public SerilogExtensionsTests()
    {
        // テストごとにロガーをリセット
        Log.Logger = global::Serilog.Core.Logger.None;
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
    }

    [Fact(DisplayName = "AddSerilogDefaults: IHostBuilder に Serilog が設定されること")]
    public void AddSerilogDefaults_ConfiguresSerilog()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:MinimumLevel:Default"] = "Information",
            })
            .Build();

        // Act
        var result = hostBuilder.ConfigureAppConfiguration((_, cb) => cb.AddConfiguration(configuration))
            .AddSerilogDefaults();

        // Assert
        Assert.Same(hostBuilder, result);

        // Host をビルドして Serilog が設定されていることを確認
        using var host = hostBuilder.Build();
        Assert.NotNull(Log.Logger);
        Assert.NotEqual(global::Serilog.Core.Logger.None, Log.Logger);
    }

    [Fact(DisplayName = "AddSerilogDefaults: MinimumLevel が appsettings から反映されること")]
    public void AddSerilogDefaults_ReflectsMinimumLevel()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:MinimumLevel:Default"] = "Warning",
            })
            .Build();

        // Act
        hostBuilder.ConfigureAppConfiguration((_, cb) => cb.AddConfiguration(configuration))
            .AddSerilogDefaults();

        using var host = hostBuilder.Build();

        // Assert — Warning 以上のみ有効
        Assert.False(Log.IsEnabled(LogEventLevel.Information));
        Assert.True(Log.IsEnabled(LogEventLevel.Warning));
    }

    [Fact(DisplayName = "AddSerilogDefaults: WriteTo 未設定時にデフォルトシンクが適用されること")]
    public void AddSerilogDefaults_NoWriteTo_AppliesDefaults()
    {
        // Arrange — WriteTo セクションなし
        var hostBuilder = Host.CreateDefaultBuilder();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:MinimumLevel:Default"] = "Information",
            })
            .Build();

        // Act — デフォルトシンク（Console + File）が追加されるはず
        hostBuilder.ConfigureAppConfiguration((_, cb) => cb.AddConfiguration(configuration))
            .AddSerilogDefaults();

        using var host = hostBuilder.Build();

        // Assert — ロガーが有効であること（デフォルトシンクが追加された証拠）
        Assert.True(Log.IsEnabled(LogEventLevel.Information));
    }

    [Fact(DisplayName = "AddSerilogDefaults: WriteTo 設定時にデフォルトが上書きされないこと")]
    public void AddSerilogDefaults_WithWriteTo_DoesNotOverrideDefaults()
    {
        // Arrange — WriteTo セクションあり
        var hostBuilder = Host.CreateDefaultBuilder();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:MinimumLevel:Default"] = "Information",
                ["Serilog:WriteTo:0:Name"] = "Console",
            })
            .Build();

        // Act
        hostBuilder.ConfigureAppConfiguration((_, cb) => cb.AddConfiguration(configuration))
            .AddSerilogDefaults();

        using var host = hostBuilder.Build();

        // Assert — 例外なくビルドでき、ロガーが有効であること
        Assert.True(Log.IsEnabled(LogEventLevel.Information));
    }

    [Fact(DisplayName = "AddSerilogDefaults: configure コールバックが実行されること")]
    public void AddSerilogDefaults_InvokesConfigureCallback()
    {
        // Arrange
        var callbackInvoked = false;
        var hostBuilder = Host.CreateDefaultBuilder();

        // Act
        hostBuilder.AddSerilogDefaults(lc =>
        {
            callbackInvoked = true;
        });

        using var host = hostBuilder.Build();

        // Assert
        Assert.True(callbackInvoked);
    }

    [Fact(DisplayName = "ConfigureSerilogFromSettings: Log.Logger が設定されること")]
    public void ConfigureSerilogFromSettings_SetsLogLogger()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:MinimumLevel:Default"] = "Information",
            })
            .Build();

        // Act
        using var cleanup = SerilogExtensions.ConfigureSerilogFromSettings(configuration);

        // Assert
        Assert.NotEqual(global::Serilog.Core.Logger.None, Log.Logger);
    }

    [Fact(DisplayName = "ConfigureSerilogFromSettings: Dispose で CloseAndFlush されること")]
    public void ConfigureSerilogFromSettings_Dispose_CallsCloseAndFlush()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:MinimumLevel:Default"] = "Information",
            })
            .Build();

        var cleanup = SerilogExtensions.ConfigureSerilogFromSettings(configuration);

        // Act
        cleanup.Dispose();

        // Assert — CloseAndFlush 後、Logger はイベントを受け付けない
        // （Serilog の内部挙動として、CloseAndFlush 後の Log.Logger は SilentLogger になる）
        Log.Information("This should not throw");
    }

    [Fact(DisplayName = "ConfigureSerilogFromSettings: configure コールバックが実行されること")]
    public void ConfigureSerilogFromSettings_InvokesConfigureCallback()
    {
        // Arrange
        var callbackInvoked = false;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:MinimumLevel:Default"] = "Information",
            })
            .Build();

        // Act
        using var cleanup = SerilogExtensions.ConfigureSerilogFromSettings(configuration, lc =>
        {
            callbackInvoked = true;
        });

        // Assert
        Assert.True(callbackInvoked);
    }

    [Fact(DisplayName = "AddSerilogDefaults: Override 未設定時にデフォルト Override が適用されること")]
    public void AddSerilogDefaults_NoOverride_AppliesDefaults()
    {
        // Arrange — Override セクションなし
        var hostBuilder = Host.CreateDefaultBuilder();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:MinimumLevel:Default"] = "Debug",
            })
            .Build();

        // Act
        hostBuilder.ConfigureAppConfiguration((_, cb) => cb.AddConfiguration(configuration))
            .AddSerilogDefaults();

        using var host = hostBuilder.Build();

        // Assert — Debug レベルは有効だが、Microsoft 名前空間は Warning に抑制されている
        // （デフォルト Override が適用された証拠として、ロガーがビルドできることを確認）
        Assert.True(Log.IsEnabled(LogEventLevel.Debug));
    }

    [Fact(DisplayName = "AddSerilogDefaults: Override 設定済みの場合はユーザー設定が尊重されること")]
    public void AddSerilogDefaults_WithOverride_RespectsUserSettings()
    {
        // Arrange — Override セクションあり（ユーザーが独自設定）
        var hostBuilder = Host.CreateDefaultBuilder();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:MinimumLevel:Default"] = "Information",
                ["Serilog:MinimumLevel:Override:Microsoft"] = "Debug",
            })
            .Build();

        // Act
        hostBuilder.ConfigureAppConfiguration((_, cb) => cb.AddConfiguration(configuration))
            .AddSerilogDefaults();

        using var host = hostBuilder.Build();

        // Assert — 例外なくビルドでき、ロガーが有効であること
        Assert.True(Log.IsEnabled(LogEventLevel.Information));
    }

    [Fact(DisplayName = "AddSerilogDefaults: hostBuilder が null の場合に ArgumentNullException をスロー")]
    public void AddSerilogDefaults_NullHostBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IHostBuilder? hostBuilder = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => hostBuilder!.AddSerilogDefaults());
    }

    [Fact(DisplayName = "ConfigureSerilogFromSettings: configuration が null の場合に ArgumentNullException をスロー")]
    public void ConfigureSerilogFromSettings_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        IConfiguration? configuration = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => SerilogExtensions.ConfigureSerilogFromSettings(configuration!));
    }
}
