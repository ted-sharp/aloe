// <copyright file="SignatureStore.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using Aloe.Apps.Medock.MdXadesApi.Models;
using Aloe.Apps.Medock.MdXadesApi.Options;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.Medock.MdXadesApi.Services;

/// <summary>
/// 署名情報をインメモリで管理するストア。
/// </summary>
public class SignatureStore
{
    private readonly ConcurrentDictionary<Guid, SignatureInfo> _signatures = new();
    private readonly MdXadesApiOptions _options;

    /// <summary>
    /// <see cref="SignatureStore"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public SignatureStore(IOptions<MdXadesApiOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>署名情報を追加する。</summary>
    public void Add(SignatureInfo info)
    {
        _signatures[info.SignatureId] = info;
        Cleanup();
    }

    /// <summary>署名情報を取得する。</summary>
    public SignatureInfo? Get(Guid signatureId)
    {
        _signatures.TryGetValue(signatureId, out var info);
        return info;
    }

    /// <summary>全署名情報を取得する。</summary>
    public IReadOnlyList<SignatureInfo> GetAll()
    {
        return [.. _signatures.Values.OrderByDescending(s => s.SignedAt)];
    }

    private void Cleanup()
    {
        var threshold = DateTimeOffset.UtcNow.AddMinutes(-_options.MaxSignatureAgeMins);
        foreach (var kvp in _signatures)
        {
            if (kvp.Value.SignedAt < threshold)
            {
                _signatures.TryRemove(kvp.Key, out _);
            }
        }
    }
}
