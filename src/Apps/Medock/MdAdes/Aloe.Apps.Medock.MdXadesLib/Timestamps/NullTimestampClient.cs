// <copyright file="NullTimestampClient.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdXadesLib.Abstractions;

namespace Aloe.Apps.Medock.MdXadesLib.Timestamps;

/// <summary>
/// テスト用の No-op タイムスタンプクライアント。空のトークンを返す。
/// </summary>
public class NullTimestampClient : ITimestampClient
{
    /// <inheritdoc/>
    public Task<byte[]> GetTimestampAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<byte>());
    }
}
