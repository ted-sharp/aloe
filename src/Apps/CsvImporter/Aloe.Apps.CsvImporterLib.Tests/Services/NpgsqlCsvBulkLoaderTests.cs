// <copyright file="NpgsqlCsvBulkLoaderTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text;
using Aloe.Apps.CsvImporterLib.Services;
using FluentAssertions;

namespace Aloe.Apps.CsvImporterLib.Tests.Services;

/// <summary>
/// NpgsqlCsvBulkLoader のテスト。
/// PostgreSQL への実接続が必要なテストはスキップ条件を設ける。
/// </summary>
public class NpgsqlCsvBulkLoaderTests
{
    [Fact]
    public void NpgsqlCsvBulkLoader_インスタンス生成できる()
    {
        // Arrange & Act
        var loader = new NpgsqlCsvBulkLoader();

        // Assert
        loader.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadAsync_接続文字列が無効な場合に例外をスローする()
    {
        // Arrange
        var loader = new NpgsqlCsvBulkLoader();
        var csvContent = "a,b,c\n1,2,3\n"u8.ToArray();
        using var stream = new MemoryStream(csvContent);

        // Act
        var act = async () => await loader.LoadAsync(
            connectionString: "Host=invalid-host-that-does-not-exist;Database=test;Username=test;Password=test;Timeout=1",
            stagingTable: "ext.test_staged",
            csvStream: stream,
            encoding: Encoding.UTF8);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
