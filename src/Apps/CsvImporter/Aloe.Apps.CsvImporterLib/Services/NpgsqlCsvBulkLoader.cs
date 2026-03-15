// <copyright file="NpgsqlCsvBulkLoader.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text;
using Aloe.Apps.CsvImporterLib.Abstractions;
using Npgsql;

namespace Aloe.Apps.CsvImporterLib.Services;

/// <summary>
/// PostgreSQL COPY FROM STDIN を使用した <see cref="ICsvBulkLoader"/> 実装。
/// </summary>
public sealed class NpgsqlCsvBulkLoader : ICsvBulkLoader
{
    /// <inheritdoc />
    public async Task<long> LoadAsync(
        string connectionString,
        string stagingTable,
        Stream csvStream,
        Encoding? encoding,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var writer = await conn.BeginTextImportAsync(
            $"COPY {stagingTable} FROM STDIN (FORMAT CSV)",
            cancellationToken);

        using var reader = new StreamReader(csvStream, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        long rowCount = 0;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            await writer.WriteLineAsync(line);
            rowCount++;
        }

        return rowCount;
    }
}
