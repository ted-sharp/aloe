// <copyright file="ICsvBulkLoader.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Text;

namespace Aloe.Apps.CsvImporterLib.Abstractions;

/// <summary>
/// PostgreSQL COPY FROM STDIN でステージングテーブルへ CSV を一括ロードする契約。
/// </summary>
public interface ICsvBulkLoader
{
    /// <summary>
    /// CSV ストリームをステージングテーブルへ一括ロードし、ロードした行数を返す。
    /// </summary>
    /// <param name="connectionString">接続文字列。</param>
    /// <param name="stagingTable">ロード先ステージングテーブル名（例: "ext.postal_codes_staged"）。</param>
    /// <param name="csvStream">CSV データストリーム。</param>
    /// <param name="encoding">CSV エンコーディング。null の場合は UTF-8。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>ロードした行数。</returns>
    Task<long> LoadAsync(
        string connectionString,
        string stagingTable,
        Stream csvStream,
        Encoding? encoding,
        CancellationToken cancellationToken = default);
}
