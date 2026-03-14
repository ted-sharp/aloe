// <copyright file="ImportResultTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.CsvImporterLib.Models;
using FluentAssertions;

namespace Aloe.Apps.CsvImporterLib.Tests.Models;

public class ImportResultTests
{
    [Fact]
    public void ImportResult_成功結果を生成できる()
    {
        // Arrange
        var started = DateTimeOffset.UtcNow;
        var finished = started.AddSeconds(5);

        // Act
        var result = new ImportResult(true, 1000, started, finished);

        // Assert
        result.Success.Should().BeTrue();
        result.RowsLoaded.Should().Be(1000);
        result.StartedAt.Should().Be(started);
        result.FinishedAt.Should().Be(finished);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ImportResult_失敗結果を生成できる()
    {
        // Arrange
        var started = DateTimeOffset.UtcNow;
        var finished = started.AddSeconds(1);

        // Act
        var result = new ImportResult(false, 0, started, finished, "接続エラー");

        // Assert
        result.Success.Should().BeFalse();
        result.RowsLoaded.Should().Be(0);
        result.ErrorMessage.Should().Be("接続エラー");
    }

    [Fact]
    public void ImportOptions_レコード等価比較が機能する()
    {
        // Arrange
        var opt1 = new ImportOptions(ImportMode.Full, null, "Host=localhost;Database=test");
        var opt2 = new ImportOptions(ImportMode.Full, null, "Host=localhost;Database=test");

        // Assert
        opt1.Should().Be(opt2);
    }
}
