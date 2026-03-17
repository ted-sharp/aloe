// <copyright file="LauncherConfigServiceTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Aloe.Apps.Medock.MdLauncherLib.Tests;

public class LauncherConfigServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<ILogger<LauncherConfigService>> _loggerMock;

    public LauncherConfigServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"MdLauncherTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _loggerMock = new Mock<ILogger<LauncherConfigService>>();
    }

    [Fact]
    public void GetConfig_ValidJson_ReturnsDeserializedConfig()
    {
        // Arrange
        var configPath = Path.Combine(_tempDir, "launcher-config.json");
        var json = """
            {
              "version": 1,
              "categories": [
                {
                  "id": "medock",
                  "name": "Medock",
                  "sortOrder": 1,
                  "apps": [
                    {
                      "id": "app1",
                      "name": "テストアプリ",
                      "description": "テスト用",
                      "exePath": "C:/test.exe",
                      "arguments": "",
                      "workingDirectory": "",
                      "iconPath": "",
                      "pipeName": "TestPipe",
                      "sortOrder": 1,
                      "enabled": true
                    }
                  ]
                }
              ]
            }
            """;
        File.WriteAllText(configPath, json);

        using var service = new LauncherConfigService(configPath, _loggerMock.Object);

        // Act
        var config = service.GetConfig();

        // Assert
        config.Version.Should().Be(1);
        config.Categories.Should().HaveCount(1);
        config.Categories[0].Id.Should().Be("medock");
        config.Categories[0].Name.Should().Be("Medock");
        config.Categories[0].Apps.Should().HaveCount(1);
        config.Categories[0].Apps[0].Id.Should().Be("app1");
        config.Categories[0].Apps[0].Name.Should().Be("テストアプリ");
        config.Categories[0].Apps[0].PipeName.Should().Be("TestPipe");
        config.Categories[0].Apps[0].Enabled.Should().BeTrue();
    }

    [Fact]
    public void GetConfig_FileNotExists_ReturnsEmptyConfig()
    {
        // Arrange
        var configPath = Path.Combine(_tempDir, "nonexistent.json");
        using var service = new LauncherConfigService(configPath, _loggerMock.Object);

        // Act
        var config = service.GetConfig();

        // Assert
        config.Version.Should().Be(1);
        config.Categories.Should().BeEmpty();
    }

    [Fact]
    public void GetConfig_EmptyCategories_ReturnsConfigWithEmptyList()
    {
        // Arrange
        var configPath = Path.Combine(_tempDir, "launcher-config.json");
        var json = """
            {
              "version": 2,
              "categories": []
            }
            """;
        File.WriteAllText(configPath, json);

        using var service = new LauncherConfigService(configPath, _loggerMock.Object);

        // Act
        var config = service.GetConfig();

        // Assert
        config.Version.Should().Be(2);
        config.Categories.Should().BeEmpty();
    }

    [Fact]
    public void GetConfig_InvalidJson_ReturnsEmptyConfig()
    {
        // Arrange
        var configPath = Path.Combine(_tempDir, "launcher-config.json");
        File.WriteAllText(configPath, "{ invalid json }");

        using var service = new LauncherConfigService(configPath, _loggerMock.Object);

        // Act
        var config = service.GetConfig();

        // Assert
        config.Categories.Should().BeEmpty();
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // テスト後のクリーンアップ失敗は無視
        }
    }
}
