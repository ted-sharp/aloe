// <copyright file="ITimestampClient.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.Medock.MdXadesLib.Abstractions;

/// <summary>
/// RFC 3161 タイムスタンプクライアントのインターフェース。
/// </summary>
public interface ITimestampClient
{
    /// <summary>指定データのタイムスタンプトークンを取得する。</summary>
    /// <param name="data">タイムスタンプ対象のデータ。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>DER エンコードされたタイムスタンプトークン。</returns>
    Task<byte[]> GetTimestampAsync(byte[] data, CancellationToken cancellationToken = default);
}
