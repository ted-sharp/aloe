// <copyright file="HttpSourceFetcher.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.IO.Compression;
using Aloe.Apps.CsvImporterLib.Abstractions;

namespace Aloe.Apps.CsvImporterLib.Services;

/// <summary>
/// HTTP ダウンロードと ZIP 展開を行う <see cref="ISourceFetcher"/> 実装。
/// </summary>
public sealed class HttpSourceFetcher : ISourceFetcher
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// コンストラクター。
    /// </summary>
    public HttpSourceFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<Stream> FetchAsync(string url, string? zipEntryPattern, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        if (zipEntryPattern is null)
        {
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        // ZIP として展開し、パターンに一致するエントリを返す
        var zipBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        using var zipArchive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);

        var entry = FindEntry(zipArchive, zipEntryPattern)
            ?? throw new InvalidOperationException($"ZIP エントリ '{zipEntryPattern}' が見つかりません。URL: {url}");

        var resultStream = new MemoryStream();
        using var entryStream = entry.Open();
        await entryStream.CopyToAsync(resultStream, cancellationToken);
        resultStream.Position = 0;
        return resultStream;
    }

    private static ZipArchiveEntry? FindEntry(ZipArchive archive, string pattern)
    {
        foreach (var entry in archive.Entries)
        {
            if (IsMatch(entry.Name, pattern))
            {
                return entry;
            }
        }

        return null;
    }

    private static bool IsMatch(string name, string pattern)
    {
        if (!pattern.Contains('*'))
        {
            return name.Equals(pattern, StringComparison.OrdinalIgnoreCase);
        }

        var parts = pattern.Split('*', 2);
        return name.StartsWith(parts[0], StringComparison.OrdinalIgnoreCase)
            && name.EndsWith(parts[1], StringComparison.OrdinalIgnoreCase);
    }
}
