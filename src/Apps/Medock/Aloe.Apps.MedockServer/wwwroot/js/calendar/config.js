/**
 * Calendar Configuration Constants
 *
 * カレンダー表示に使用する色設定、フォント設定、アニメーション設定
 * テーマ対応：daisyUIのテーマ変更に自動的に対応
 */

export const CONFIG = {
    colors: {},
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
