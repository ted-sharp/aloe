// <copyright file="HttpSourceFetcherTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.IO.Compression;
using System.Net;
using Aloe.Apps.CsvImporterLib.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace Aloe.Apps.CsvImporterLib.Tests.Services;

public class HttpSourceFetcherTests
{
    [Fact]
    public async Task FetchAsync_非ZIPで直接ストリームを返す()
    {
        // Arrange
        var content = "col1,col2\nval1,val2\n"u8.ToArray();
        var handler = CreateMockHandler(content);
        var httpClient = new HttpClient(handler);
        var fetcher = new HttpSourceFetcher(httpClient);

        // Act
        using var stream = await fetcher.FetchAsync("http://test/data.csv", zipEntryPattern: null);

        // Assert
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        text.Should().Contain("col1,col2");
    }

    [Fact]
    public async Task FetchAsync_ZIPパターン指定でエントリを展開して返す()
    {
        // Arrange
        var zipBytes = CreateZipWithEntry("data.csv", "zip_code,city\n1234567,Tokyo\n");
        var handler = CreateMockHandler(zipBytes);
        var httpClient = new HttpClient(handler);
        var fetcher = new HttpSourceFetcher(httpClient);

        // Act
        using var stream = await fetcher.FetchAsync("http://test/data.zip", zipEntryPattern: "*.csv");

        // Assert
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        text.Should().Contain("zip_code,city");
        text.Should().Contain("1234567,Tokyo");
    }

    [Fact]
    public async Task FetchAsync_一致するZIPエントリがない場合に例外をスローする()
    {
        // Arrange
        var zipBytes = CreateZipWithEntry("data.csv", "content");
        var handler = CreateMockHandler(zipBytes);
        var httpClient = new HttpClient(handler);
        var fetcher = new HttpSourceFetcher(httpClient);

        // Act
        var act = async () => await fetcher.FetchAsync("http://test/data.zip", zipEntryPattern: "*.xml");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ZIP エントリ*");
    }

    private static HttpMessageHandler CreateMockHandler(byte[] responseBytes)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(responseBytes),
            });
        return mock.Object;
    }

    private static byte[] CreateZipWithEntry(string entryName, string content)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = zip.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(content);
        }

        return ms.ToArray();
    }
}
