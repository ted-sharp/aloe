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
    public async Task<Stream> FetchAsync(
        string url,
        string? zipEntryPattern,
        IProgress<int>? downloadProgress = null,
        string? workDir = null,
        CancellationToken cancellationToken = default)
    {
        var zipBytes = await GetZipBytesAsync(url, downloadProgress, workDir, cancellationToken);

        if (zipEntryPattern is null)
        {
            return new MemoryStream(zipBytes);
        }

        using var zipArchive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);

        var entry = FindEntry(zipArchive, zipEntryPattern)
            ?? throw new InvalidOperationException($"ZIP エントリ '{zipEntryPattern}' が見つかりません。URL: {url}");

        var resultStream = new MemoryStream();
        using var entryStream = entry.Open();
        await entryStream.CopyToAsync(resultStream, cancellationToken);
        resultStream.Position = 0;
        return resultStream;
    }

    private async Task<byte[]> GetZipBytesAsync(
        string url,
        IProgress<int>? downloadProgress,
        string? workDir,
        CancellationToken cancellationToken)
    {
        if (workDir is not null)
        {
            var cachedPath = GetCachedFilePath(workDir, url);
            if (File.Exists(cachedPath))
            {
                return await File.ReadAllBytesAsync(cachedPath, cancellationToken);
            }

            var bytes = await DownloadWithProgressAsync(url, downloadProgress, cancellationToken);
            Directory.CreateDirectory(workDir);
            await File.WriteAllBytesAsync(cachedPath, bytes, cancellationToken);
            return bytes;
        }

        return await DownloadWithProgressAsync(url, downloadProgress, cancellationToken);
    }

    private async Task<byte[]> DownloadWithProgressAsync(
        string url,
        IProgress<int>? downloadProgress,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var buffer = new MemoryStream();

        if (totalBytes is null || downloadProgress is null)
        {
            await contentStream.CopyToAsync(buffer, cancellationToken);
        }
        else
        {
            var chunk = new byte[8192];
            long downloaded = 0;
            int read;

            while ((read = await contentStream.ReadAsync(chunk, cancellationToken)) > 0)
            {
                await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
                downloaded += read;
                var percent = (int)(downloaded * 100 / totalBytes.Value);
                downloadProgress.Report(percent);
            }
        }

        return buffer.ToArray();
    }

    private static string GetCachedFilePath(string workDir, string url)
    {
        var fileName = Path.GetFileName(new Uri(url).LocalPath);
        return Path.Combine(workDir, fileName);
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
