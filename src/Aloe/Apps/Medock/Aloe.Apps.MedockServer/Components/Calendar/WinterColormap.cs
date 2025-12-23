using System;

namespace Aloe.Apps.MedockServer.Components.Calendar;

/// <summary>
/// Winterカラーマップのユーティリティ
/// 空室率（0.0-1.0）に基づいて色を取得
/// 空室率1.0（空室）→青、空室率0.0（満室）→緑
/// キャパオーバー（マイナス値）→赤
/// 
/// Winterカラーマップは、青から緑への線形補間を提供するカラーマップです。
/// matplotlibの標準的なWinterカラーマップの実装に基づいています。
/// </summary>
public static class WinterColormap
{
    /// <summary>
    /// Winterカラーマップの256色RGBルックアップテーブル
    /// 値は0.0（青）から1.0（緑）への連続的な色変化を提供
    /// R = 0, G = ratio, B = 1 - ratio の線形補間
    /// </summary>
    private static readonly (double R, double G, double B)[] Colormap;

    static WinterColormap()
    {
        // 256色のルックアップテーブルを生成
        Colormap = new (double, double, double)[256];
        for (int i = 0; i < 256; i++)
        {
            double ratio = i / 255.0;
            Colormap[i] = (0.0, ratio, 1.0 - ratio);
        }
    }

    /// <summary>
    /// Winterカラーマップから色を取得
    /// </summary>
    /// <param name="ratio">値（0.0-1.0、範囲外の値も許容）</param>
    /// <returns>16進数カラーコード（#RRGGBB）</returns>
    public static string GetWinterColor(double ratio)
    {
        // 値を0-1の範囲にクランプ
        var clampedRatio = Math.Max(0.0, Math.Min(1.0, ratio));
        
        // 0-255のインデックスに変換（配列の範囲内に保証）
        var index = Math.Max(0, Math.Min(255, (int)Math.Floor(clampedRatio * 255)));
        
        // ルックアップテーブルからRGB値を取得（安全性チェック）
        if (index < 0 || index >= Colormap.Length)
        {
            // フォールバック: エラー時はグレーを返す
            return "#9ca3af";
        }
        
        var (r, g, b) = Colormap[index];
        
        // 16進数カラーコードに変換
        return RgbToHex(r, g, b);
    }

    /// <summary>
    /// 空室率からWinterカラーマップの色を取得
    /// 空室率1.0（空室）→青、空室率0.0（満室）→緑
    /// </summary>
    /// <param name="vacancyRatio">空室率（0.0-1.0）</param>
    /// <returns>16進数カラーコード（#RRGGBB）</returns>
    public static string GetWinterColorFromVacancyRatio(double vacancyRatio)
    {
        // 空室率を反転して使用（空室率1.0→Winter 0.0→青、空室率0.0→Winter 1.0→緑）
        return GetWinterColor(1.0 - vacancyRatio);
    }

    /// <summary>
    /// 空き数とキャパシティからWinterカラーマップの色を取得
    /// 空室率1.0（空室）→青、空室率0.0（満室）→緑
    /// マイナス値（オーバーキャパシティ）の場合は赤色を返す
    /// </summary>
    /// <param name="available">空き数（負の値も許容：オーバーキャパシティ）</param>
    /// <param name="cap">キャパシティ</param>
    /// <returns>16進数カラーコード（#RRGGBB）</returns>
    public static string GetWinterColorFromAvailable(int available, int cap)
    {
        if (cap <= 0) return "#9ca3af"; // キャパシティが0以下の場合はグレー
        
        // マイナス値（オーバーキャパシティ）の場合は赤色を返す
        if (available < 0)
        {
            return "#FF0000"; // 赤
        }
        
        var vacancyRatio = (double)available / cap;
        // 空室率が1.0を超える場合もクランプされるが、明示的に処理
        return GetWinterColorFromVacancyRatio(Math.Max(0.0, Math.Min(1.0, vacancyRatio)));
    }

    /// <summary>
    /// 使用率からWinterカラーマップの色を取得
    /// 使用率0.0（空室）→青、使用率1.0（満室）→緑
    /// </summary>
    /// <param name="usageRatio">使用率（0.0-1.0）</param>
    /// <returns>16進数カラーコード（#RRGGBB）</returns>
    public static string GetWinterColorFromUsageRatio(double usageRatio)
    {
        // 使用率をそのまま使用（使用率0.0→Winter 0.0→青、使用率1.0→Winter 1.0→緑）
        // 使用率0.0 = 空室率1.0 → 青、使用率1.0 = 空室率0.0 → 緑
        return GetWinterColor(usageRatio);
    }

    /// <summary>
    /// RGB値を16進数カラーコードに変換
    /// </summary>
    /// <param name="r">赤（0-1）</param>
    /// <param name="g">緑（0-1）</param>
    /// <param name="b">青（0-1）</param>
    /// <returns>16進数カラーコード（#RRGGBB）</returns>
    private static string RgbToHex(double r, double g, double b)
    {
        var rInt = (int)Math.Round(r * 255);
        var gInt = (int)Math.Round(g * 255);
        var bInt = (int)Math.Round(b * 255);
        return $"#{rInt:X2}{gInt:X2}{bInt:X2}";
    }
}

