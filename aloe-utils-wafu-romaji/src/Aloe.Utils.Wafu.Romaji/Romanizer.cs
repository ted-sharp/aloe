// <copyright file="Romanizer.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using static Aloe.Utils.Wafu.Romaji.KanaMap;

namespace Aloe.Utils.Wafu.Romaji;

/// <summary>
/// ひらがな・カタカナ文字列をローマ字（ヘボン式）に変換するユーティリティクラスです。
/// </summary>
public static class Romanizer
{
    /// <summary>
    /// ひらがな・カタカナ混在文字列をヘボン式ローマ字に変換して返します。
    /// </summary>
    /// <param name="input">変換対象の文字列</param>
    /// <returns>ローマ字文字列</returns>
    public static string Convert(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
        {
            return String.Empty;
        }

        // ■ カタカナ→ひらがな
        Span<char> temp = stackalloc char[input.Length];
        var len = 0;
        for (var i = 0; i < input.Length; i++)
        {
            // 30A1 ァ → 3041 ぁ
            // 30F6 ヶ → 3096 ゖ
            // 30F7 ヷ
            // 30F8 ヸ
            // 30F9 ヹ
            // 30FA ヺ
            var c = input[i];
            temp[len++] = (c is >= 'ァ' and <= 'ヶ') ? (char)(c - 0x60) : c;
        }

        var vsb = new ValueStringBuilder(stackalloc char[256]);

        for (var i = 0; i < len; i++)
        {
            // ■ 拗音（二文字）チェック
            if (i + 1 < len)
            {
                var two = temp.Slice(i, 2);
#pragma warning disable SA1107 // Code should not contain multiple statements on one line
#pragma warning disable SA1501 // Statement should not be on a single line
                if (two.SequenceEqual(KYA)) { vsb.Append("kya"); i++; continue; }
                if (two.SequenceEqual(KYU)) { vsb.Append("kyu"); i++; continue; }
                if (two.SequenceEqual(KYO)) { vsb.Append("kyo"); i++; continue; }
                if (two.SequenceEqual(SHA)) { vsb.Append("sha"); i++; continue; }
                if (two.SequenceEqual(SHU)) { vsb.Append("shu"); i++; continue; }
                if (two.SequenceEqual(SHO)) { vsb.Append("sho"); i++; continue; }
                if (two.SequenceEqual(CHA)) { vsb.Append("cha"); i++; continue; }
                if (two.SequenceEqual(CHU)) { vsb.Append("chu"); i++; continue; }
                if (two.SequenceEqual(CHO)) { vsb.Append("cho"); i++; continue; }
                if (two.SequenceEqual(NYA)) { vsb.Append("nya"); i++; continue; }
                if (two.SequenceEqual(NYU)) { vsb.Append("nyu"); i++; continue; }
                if (two.SequenceEqual(NYO)) { vsb.Append("nyo"); i++; continue; }
                if (two.SequenceEqual(HYA)) { vsb.Append("hya"); i++; continue; }
                if (two.SequenceEqual(HYU)) { vsb.Append("hyu"); i++; continue; }
                if (two.SequenceEqual(HYO)) { vsb.Append("hyo"); i++; continue; }
                if (two.SequenceEqual(MYA)) { vsb.Append("mya"); i++; continue; }
                if (two.SequenceEqual(MYU)) { vsb.Append("myu"); i++; continue; }
                if (two.SequenceEqual(MYO)) { vsb.Append("myo"); i++; continue; }
                if (two.SequenceEqual(RYA)) { vsb.Append("rya"); i++; continue; }
                if (two.SequenceEqual(RYU)) { vsb.Append("ryu"); i++; continue; }
                if (two.SequenceEqual(RYO)) { vsb.Append("ryo"); i++; continue; }
                if (two.SequenceEqual(GYA)) { vsb.Append("gya"); i++; continue; }
                if (two.SequenceEqual(GYU)) { vsb.Append("gyu"); i++; continue; }
                if (two.SequenceEqual(GYO)) { vsb.Append("gyo"); i++; continue; }
                if (two.SequenceEqual(JA)) { vsb.Append("ja"); i++; continue; }
                if (two.SequenceEqual(JU)) { vsb.Append("ju"); i++; continue; }
                if (two.SequenceEqual(JO)) { vsb.Append("jo"); i++; continue; }
                if (two.SequenceEqual(BYA)) { vsb.Append("bya"); i++; continue; }
                if (two.SequenceEqual(BYU)) { vsb.Append("byu"); i++; continue; }
                if (two.SequenceEqual(BYO)) { vsb.Append("byo"); i++; continue; }
                if (two.SequenceEqual(PYA)) { vsb.Append("pya"); i++; continue; }
                if (two.SequenceEqual(PYU)) { vsb.Append("pyu"); i++; continue; }
                if (two.SequenceEqual(PYO)) { vsb.Append("pyo"); i++; continue; }

                // 特殊な外来語の変換
                if (two.SequenceEqual(SHE)) { vsb.Append("she"); i++; continue; }
                if (two.SequenceEqual(CHE)) { vsb.Append("che"); i++; continue; }
                if (two.SequenceEqual(JE)) { vsb.Append("je"); i++; continue; }
                if (two.SequenceEqual(TI)) { vsb.Append("ti"); i++; continue; }
                if (two.SequenceEqual(DI)) { vsb.Append("di"); i++; continue; }
                if (two.SequenceEqual(TU)) { vsb.Append("tu"); i++; continue; }
                if (two.SequenceEqual(WI)) { vsb.Append("wi"); i++; continue; }
                if (two.SequenceEqual(KWA)) { vsb.Append("kwa"); i++; continue; }
                if (two.SequenceEqual(GWA)) { vsb.Append("gwa"); i++; continue; }
                if (two.SequenceEqual(VA)) { vsb.Append("va"); i++; continue; }
                if (two.SequenceEqual(VI)) { vsb.Append("vi"); i++; continue; }
                if (two.SequenceEqual(VE)) { vsb.Append("ve"); i++; continue; }
                if (two.SequenceEqual(VO)) { vsb.Append("vo"); i++; continue; }
                if (two.SequenceEqual(VYA)) { vsb.Append("vya"); i++; continue; }
                if (two.SequenceEqual(VYU)) { vsb.Append("vyu"); i++; continue; }
                if (two.SequenceEqual(VYO)) { vsb.Append("vyo"); i++; continue; }
#pragma warning restore SA1501 // Statement should not be on a single line
#pragma warning restore SA1107 // Code should not contain multiple statements on one line
            }

            var c0 = temp[i];

            // ■ 促音「っ」
            // 次の子音を二重にする
            if (c0 == 'っ' && i + 1 < len)
            {
                var next = temp[i + 1];
                var m = MapSingle(next);
                if (!String.IsNullOrEmpty(m))
                {
                    vsb.Append(m[0]);
                }

                continue;
            }

            // ■ 長音記号「ー」
            // ひとつ前のローマ字を繰り返す
            if (c0 == 'ー')
            {
                if (vsb.Length > 0)
                {
                    vsb.Append(vsb[vsb.Length - 1]);
                }

                continue;
            }

            // ■ 単音・濁音・半濁音
            vsb.Append(MapSingle(c0));
        }

        return vsb.ToString();
    }
}
