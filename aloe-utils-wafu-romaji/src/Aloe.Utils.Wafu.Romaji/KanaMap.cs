// <copyright file="KanaMap.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

namespace Aloe.Utils.Wafu.Romaji;

/// <summary>
/// かなとローマ字の関連付け
/// </summary>
internal static class KanaMap
{
    /// <summary>きゃ</summary>
    internal const string KYA = "きゃ";

    /// <summary>きゅ</summary>
    internal const string KYU = "きゅ";

    /// <summary>きょ</summary>
    internal const string KYO = "きょ";

    /// <summary>しゃ</summary>
    internal const string SHA = "しゃ";

    /// <summary>しゅ</summary>
    internal const string SHU = "しゅ";

    /// <summary>しょ</summary>
    internal const string SHO = "しょ";

    /// <summary>ちゃ</summary>
    internal const string CHA = "ちゃ";

    /// <summary>ちゅ</summary>
    internal const string CHU = "ちゅ";

    /// <summary>ちょ</summary>
    internal const string CHO = "ちょ";

    /// <summary>にゃ</summary>
    internal const string NYA = "にゃ";

    /// <summary>にゅ</summary>
    internal const string NYU = "にゅ";

    /// <summary>にょ</summary>
    internal const string NYO = "にょ";

    /// <summary>ひゃ</summary>
    internal const string HYA = "ひゃ";

    /// <summary>ひゅ</summary>
    internal const string HYU = "ひゅ";

    /// <summary>ひょ</summary>
    internal const string HYO = "ひょ";

    /// <summary>みゃ</summary>
    internal const string MYA = "みゃ";

    /// <summary>みゅ</summary>
    internal const string MYU = "みゅ";

    /// <summary>みょ</summary>
    internal const string MYO = "みょ";

    /// <summary>りゃ</summary>
    internal const string RYA = "りゃ";

    /// <summary>りゅ</summary>
    internal const string RYU = "りゅ";

    /// <summary>りょ</summary>
    internal const string RYO = "りょ";

    /// <summary>ぎゃ</summary>
    internal const string GYA = "ぎゃ";

    /// <summary>ぎゅ</summary>
    internal const string GYU = "ぎゅ";

    /// <summary>ぎょ</summary>
    internal const string GYO = "ぎょ";

    /// <summary>じゃ</summary>
    internal const string JA = "じゃ";

    /// <summary>じゅ</summary>
    internal const string JU = "じゅ";

    /// <summary>じょ</summary>
    internal const string JO = "じょ";

    /// <summary>びゃ</summary>
    internal const string BYA = "びゃ";

    /// <summary>びゅ</summary>
    internal const string BYU = "びゅ";

    /// <summary>びょ</summary>
    internal const string BYO = "びょ";

    /// <summary>ぴゃ</summary>
    internal const string PYA = "ぴゃ";

    /// <summary>ぴゅ</summary>
    internal const string PYU = "ぴゅ";

    /// <summary>ぴょ</summary>
    internal const string PYO = "ぴょ";

    /// <summary>しぇ</summary>
    internal const string SHE = "しぇ";

    /// <summary>ちぇ</summary>
    internal const string CHE = "ちぇ";

    /// <summary>じぇ</summary>
    internal const string JE = "じぇ";

    /// <summary>てぃ</summary>
    internal const string TI = "てぃ";

    /// <summary>でぃ</summary>
    internal const string DI = "でぃ";

    /// <summary>とぅ</summary>
    internal const string TU = "とぅ";

    /// <summary>うぃ</summary>
    internal const string WI = "うぃ";

    /// <summary>くぁ</summary>
    internal const string KWA = "くぁ";

    /// <summary>ぐぁ</summary>
    internal const string GWA = "ぐぁ";

    /// <summary>ヴァ</summary>
    internal const string VA = "ゔぁ";

    /// <summary>ヴィ</summary>
    internal const string VI = "ゔぃ";

    /// <summary>ヴェ</summary>
    internal const string VE = "ゔぇ";

    /// <summary>ヴォ</summary>
    internal const string VO = "ゔぉ";

    /// <summary>ヴャ</summary>
    internal const string VYA = "ゔゃ";

    /// <summary>ヴュ</summary>
    internal const string VYU = "ゔゅ";

    /// <summary>ヴョ</summary>
    internal const string VYO = "ゔょ";

    /// <summary>
    /// ひらがな1文字をローマ字に変換します。
    /// </summary>
    /// <param name="hira">変換対象のひらがな1文字</param>
    /// <returns>ローマ字文字列</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string MapSingle(char hira) => hira switch
    {
        'あ' => "a",
        'い' => "i",
        'う' => "u",
        'え' => "e",
        'お' => "o",
        'か' => "ka",
        'き' => "ki",
        'く' => "ku",
        'け' => "ke",
        'こ' => "ko",
        'さ' => "sa",
        'し' => "shi",
        'す' => "su",
        'せ' => "se",
        'そ' => "so",
        'た' => "ta",
        'ち' => "chi",
        'つ' => "tsu",
        'て' => "te",
        'と' => "to",
        'な' => "na",
        'に' => "ni",
        'ぬ' => "nu",
        'ね' => "ne",
        'の' => "no",
        'は' => "ha",
        'ひ' => "hi",
        'ふ' => "fu",
        'へ' => "he",
        'ほ' => "ho",
        'ま' => "ma",
        'み' => "mi",
        'む' => "mu",
        'め' => "me",
        'も' => "mo",
        'や' => "ya",
        'ゆ' => "yu",
        'よ' => "yo",
        'ら' => "ra",
        'り' => "ri",
        'る' => "ru",
        'れ' => "re",
        'ろ' => "ro",
        'わ' => "wa",
        'を' => "wo",
        'ん' => "n",
        'が' => "ga",
        'ぎ' => "gi",
        'ぐ' => "gu",
        'げ' => "ge",
        'ご' => "go",
        'ざ' => "za",
        'じ' => "ji",
        'ず' => "zu",
        'ぜ' => "ze",
        'ぞ' => "zo",
        'だ' => "da",
        'ぢ' => "ji",
        'づ' => "zu",
        'で' => "de",
        'ど' => "do",
        'ば' => "ba",
        'び' => "bi",
        'ぶ' => "bu",
        'べ' => "be",
        'ぼ' => "bo",
        'ぱ' => "pa",
        'ぴ' => "pi",
        'ぷ' => "pu",
        'ぺ' => "pe",
        'ぽ' => "po",
        'ぁ' => "a",
        'ぃ' => "i",
        'ぅ' => "u",
        'ぇ' => "e",
        'ぉ' => "o",
        'ゃ' => "ya",
        'ゅ' => "yu",
        'ょ' => "yo",
        'ゎ' => "wa",
        'ゐ' => "i",
        'ゑ' => "e",
        'ゔ' => "vu",
        'ヷ' => "va",
        'ヸ' => "vi",
        'ヴ' => "vu",
        'ヹ' => "ve",
        'ヺ' => "vo",
        _ => hira.ToString(),
    };
}
