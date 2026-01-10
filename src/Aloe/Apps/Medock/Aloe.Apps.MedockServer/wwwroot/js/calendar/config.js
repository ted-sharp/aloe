/**
 * Calendar Configuration Constants
 *
 * カレンダー表示に使用する色設定、フォント設定、アニメーション設定
 */

export const CONFIG = {
    colors: {
        am: { empty: '#e5e7eb', filled: '#10b981', text: '#065f46' },
        pm: { empty: '#e5e7eb', filled: '#3b82f6', text: '#1e40af' },
        // 時間帯枠用の色（空き→青、満杯→赤）
        slot: {
            empty: '#3b82f6',      // 空き（青）
            full: '#ef4444',       // 満杯（赤）
            partial: '#fbbf24',    // 一部埋まり（黄）
            background: '#e5e7eb'  // 背景（グレー）
        },
        status: {
            0: '#9ca3af', // Reserved (gray)
            1: '#60a5fa', // Waiting (info)
            2: '#34d399', // Visited (success)
            3: '#f87171'  // Cancelled (error)
        },
        grid: '#d1d5db',
        today: '#10b981',
        weekend: { sun: '#ef4444', sat: '#3b82f6' },
        hover: 'rgba(16, 185, 129, 0.1)',
        grayout: 'rgba(128, 128, 128, 0.5)'
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
