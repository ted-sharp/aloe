// <copyright file="ISourceFetcher.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.CsvImporterLib.Abstractions;

/// <summary>
/// HTTP ダウンロードと ZIP 展開を行いデータストリームを返す契約。
/// </summary>
public interface ISourceFetcher
{
    /// <summary>
    /// 指定 URL からデータを取得する。
    /// zipEntryPattern が null でない場合は ZIP として展開し、パターンに一致するエントリを返す。
    /// </summary>
    /// <param name="url">ダウンロード URL。</param>
    /// <param name="zipEntryPattern">ZIP 内エントリのファイル名パターン（glob 形式、null なら非 ZIP として扱う）。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>CSVデータストリーム（呼び出し元で Dispose すること）。</returns>
    Task<Stream> FetchAsync(string url, string? zipEntryPattern, CancellationToken cancellationToken = default);
}
