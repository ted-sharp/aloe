/**
 * Calendar Configuration Constants
 *
 * カレンダー表示に使用する色設定、フォント設定、アニメーション設定
 * テーマ対応：daisyUIのテーマ変更に自動的に対応
 */

import { ThemeColors } from './utils/theme-utils.js';

// テーマ対応の色を取得
const themeColors = ThemeColors.getThemeColors();

export const CONFIG = {
    colors: {
        am: {
            empty: themeColors.am.empty,
            filled: themeColors.am.filled,
            text: themeColors.am.text
        },
        pm: {
            empty: themeColors.pm.empty,
            filled: themeColors.pm.filled,
            text: themeColors.pm.text
        },
        // 時間帯枠用の色（空き→青、満杯→赤）
        slot: {
            empty: themeColors.slot.empty,           // Primary色
            full: themeColors.slot.full,             // 満杯（赤）
            partial: themeColors.slot.partial,       // 一部埋まり（黄）
            background: themeColors.slot.background  // 背景（グレー）
        },
        status: {
            0: themeColors.status[0], // Reserved (gray)
            1: themeColors.status[1], // Waiting (info)
            2: themeColors.status[2], // Visited (success)
            3: themeColors.status[3]  // Cancelled (error)
        },
        grid: themeColors.grid,
        today: themeColors.today,
        weekend: {
            sun: themeColors.weekend.sun,
            sat: themeColors.weekend.sat
        },
        hover: themeColors.hover,
        grayout: themeColors.grayout
    },
    font: {
        family: '"M PLUS Rounded 1c", system-ui, -apple-system, sans-serif',
        numberFamily: '"Playwrite Norge", cursive',
        sizeSmall: 10,
        sizeMedium: 12,
        sizeLarge: 14,
        sizeDateYear: 12,   // 年間カレンダー用の日付数字フォントサイズ
        sizeDateMonth: 16,  // 月間カレンダー用の日付数字フォントサイズ
        labelYear: 6,       // 年間ビューのスロットラベルフォントサイズ
        labelMonth: 7       // 月間ビューのスロットラベルフォントサイズ
    },
    spacing: {
        barHPadding: 4,     // 棒グラフの左右の余白（px）
        barXOffset: 2,      // 棒グラフの開始位置のオフセット（px）
        dayTextMargin: 4,   // 日付テキスト下のマージン（px）
        labelMargin: 2      // ラベルと棒グラフの間のマージン（px）
    },
    stroke: {
        normal: 1,          // 通常の線の太さ（px）
        alert: 2            // アラート（業務時間外）の線の太さ（px）
    },
    animation: {
        duration: 150
    }
};
