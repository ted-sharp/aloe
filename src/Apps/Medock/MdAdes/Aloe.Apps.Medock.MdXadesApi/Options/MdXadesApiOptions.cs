// <copyright file="MdXadesApiOptions.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.Medock.MdXadesApi.Options;

/// <summary>
/// MdXades API サーバー固有の設定オプション。
/// </summary>
public class MdXadesApiOptions
{
    /// <summary>署名 XML の出力ディレクトリパス。</summary>
    public string OutputPath { get; set; } = "signatures";

    /// <summary>署名の最大保持時間（分）。</summary>
    public int MaxSignatureAgeMins { get; set; } = 1440;
}
