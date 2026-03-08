/**
 * Theme Utilities - daisyUI テーマ対応
 *
 * daisyUI のCSS変数をカレンダーの色に変換
 * デバッグ出力を含む
 */

const DEBUG = false;

function log(...args) {
    if (DEBUG) {
        console.log('[ThemeUtils]', ...args);
    }
}

function warn(...args) {
    if (DEBUG) {
        console.warn('[ThemeUtils]', ...args);
    }
}


/**
 * daisyUIテーマに対応した現在のテーマを取得
 * @returns {string} テーマ名（'light', 'dark', 'cyberpunk' など）
 */
export function getCurrentTheme() {
    const theme = document.documentElement.getAttribute('data-theme') || 'light';
    log(`getCurrentTheme(): ${theme}`);
    return theme;
}

/**
 * テーマが変更されたかを監視し、コールバックを実行
 * @param {Function} callback - テーマが変更されたときに実行する関数
 * @returns {Function} 監視を停止する関数
 */
export function observeThemeChanges(callback) {
    log('observeThemeChanges: Starting theme observation');

    const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            if (mutation.type === 'attributes' && mutation.attributeName === 'data-theme') {
                const newTheme = getCurrentTheme();
                log(`Theme changed: ${newTheme}`);
                callback(newTheme);
            }
        });
    });

    observer.observe(document.documentElement, {
        attributes: true,
        attributeFilter: ['data-theme']
    });

    // 監視を停止する関数を返す
    return () => {
        log('observeThemeChanges: Stopping theme observation');
        observer.disconnect();
    };
}

/**
 * daisyUI テーマの色定義
 * 各テーマの公式色を定義
 */
const THEME_COLORS_MAP = {
    light: {
        primary: '#0d9488',
        secondary: '#f3f4f6',
        accent: '#10b981',
        neutral: '#1f2937',
        baseContent: '#1f2937',
        base100: '#ffffff',
        base200: '#f3f4f6',
        gridBg: '#f9fafb',
        gridLine: '#d1d5db',
        dayHeaderBg: '#f9fafb',
        dayHeaderText: '#374151',
        dayText: '#1f2937'
    },
    dark: {
        primary: '#14b8a6',
        secondary: '#374151',
        accent: '#10b981',
        neutral: '#f3f4f6',
        baseContent: '#f3f4f6',
        base100: '#1f2937',
        base200: '#111827',
        gridBg: '#111827',
        gridLine: '#6b7280',
        dayHeaderBg: '#111827',
        dayHeaderText: '#d1d5db',
        dayText: '#f3f4f6'
    },
    cyberpunk: {
        primary: '#ffbe0b',
        secondary: '#fb5607',
        accent: '#ff006e',
        neutral: '#000000',
        baseContent: '#ffffff',
        base100: '#0a0e27',
        base200: '#1a1a3a',
        gridBg: '#0a0e27',
        gridLine: '#ffbe0b',
        dayHeaderBg: '#1a1a3a',
        dayHeaderText: '#ffbe0b',
        dayText: '#ffffff'
    }
};

/**
 * カレンダー表示用に色をテーマに適応させる
 * daisyUIのテーマ定義からカレンダー用の色を生成
 */
export const ThemeColors = {
    /**
     * テーマに応じた基本色を取得
     * @returns {object} カレンダー用の色セット
     */
    getThemeColors() {
        const theme = getCurrentTheme();
        log(`ThemeColors.getThemeColors() for theme: ${theme}`);

        // テーマカラーを取得（定義されていないテーマは light をデフォルトにする）
        const themeColors = THEME_COLORS_MAP[theme] || THEME_COLORS_MAP['light'];

        const {
            primary,
            secondary,
            accent,
            neutral,
            baseContent,
            base100,
            base200,
            gridBg,
            gridLine,
            dayHeaderBg,
            dayHeaderText,
            dayText
        } = themeColors;

        log(`Theme colors loaded: primary=${primary}, accent=${accent}`);

        const colors = {
            primary,
            secondary,
            accent,
            neutral,
            baseContent,
            base100,
            base200,
            // カレンダー固有の色
            grid: gridLine,
            today: accent,
            weekend: {
                sun: theme === 'dark' || theme === 'dracula' || theme === 'nord' ? '#ef4444' : '#dc2626',
                sat: theme === 'dark' || theme === 'dracula' || theme === 'nord' ? '#3b82f6' : '#2563eb'
            },
            status: {
                0: theme === 'dark' || theme === 'dracula' || theme === 'nord' ? '#6b7280' : '#9ca3af', // Reserved (gray)
                1: theme === 'dark' || theme === 'dracula' || theme === 'nord' ? '#3b82f6' : '#60a5fa', // Waiting (info)
                2: theme === 'dark' || theme === 'dracula' || theme === 'nord' ? '#10b981' : '#34d399', // Visited (success)
                3: theme === 'dark' || theme === 'dracula' || theme === 'nord' ? '#ef4444' : '#f87171'  // Cancelled (error)
            },
            slot: {
                empty: primary,
                full: '#ef4444',
                partial: '#fbbf24',
                background: gridBg
            },
            am: {
                empty: gridBg,
                filled: accent,
                text: theme === 'light' ? '#065f46' : baseContent
            },
            pm: {
                empty: gridBg,
                filled: primary,
                text: theme === 'light' ? '#1e40af' : baseContent
            },
            hover: 'rgba(59, 130, 246, 0.15)',
            grayout: 'rgba(128, 128, 128, 0.5)',
            // UI色
            background: base100,
            text: baseContent,
            gridBackground: gridBg,
            dayHeaderBg,
            dayHeaderText,
            dayText
        };

        log('ThemeColors computed:', colors);
        return colors;
    }
};

/**
 * 色の明度を調整（テーマ対応）
 * @param {string} hexColor - 16進数カラーコード
 * @param {number} factor - 明度係数（0-1）
 * @returns {string} 調整後のカラーコード
 */
function adjustBrightness(hexColor, factor) {
    try {
        const r = parseInt(hexColor.slice(1, 3), 16);
        const g = parseInt(hexColor.slice(3, 5), 16);
        const b = parseInt(hexColor.slice(5, 7), 16);

        return '#' + [r, g, b].map(x => {
            const adjusted = Math.round(x * factor);
            const hex = adjusted.toString(16);
            return hex.length === 1 ? '0' + hex : hex;
        }).join('').toUpperCase();
    } catch (e) {
        warn('adjustBrightness failed:', e);
        return hexColor;
    }
}

/**
 * 現在のテーマに対応した色を一度だけ取得
 * CONFIG の colors をリアルタイム更新する
 * @param {object} config - CONFIG オブジェクト
 */
export function updateConfigColorsForCurrentTheme(config) {
    log('updateConfigColorsForCurrentTheme: Updating CONFIG colors');
    const themeColors = ThemeColors.getThemeColors();

    if (config && config.colors) {
        // config.colors を themeColors でマージ
        config.colors = {
            ...config.colors,
            ...themeColors
        };
        log('CONFIG.colors updated:', config.colors);
    } else {
        warn('CONFIG object is invalid');
    }
}

/**
 * サポートするdaisyUI テーマ一覧
 * 完全対応しているテーマのみ含める
 */
export const AVAILABLE_THEMES = [
    { name: 'light', label: 'ライト' },
    { name: 'dark', label: 'ダーク' },
    { name: 'cyberpunk', label: 'サイバーパンク' }
];

/**
 * テーマを設定
 * @param {string} themeName - テーマ名（light, dark, cyberpunk など）
 */
export function setTheme(themeName) {
    if (!themeName) {
        warn('setTheme called with empty theme name');
        return;
    }
    log(`setTheme('${themeName}')`);
    document.documentElement.setAttribute('data-theme', themeName);

    // localStorage に保存
    try {
        localStorage.setItem('medock-theme', themeName);
        log(`Theme saved to localStorage: ${themeName}`);
    } catch (e) {
        warn('Failed to save theme to localStorage:', e);
    }
}

/**
 * 保存されたテーマを読み込む
 * @returns {string} 保存されたテーマ名、ない場合は 'light'
 */
export function loadSavedTheme() {
    try {
        const saved = localStorage.getItem('medock-theme');
        log(`loadSavedTheme: Found '${saved}' in localStorage`);

        if (saved && AVAILABLE_THEMES.some(t => t.name === saved)) {
            log(`loadSavedTheme: Valid theme found: ${saved}`);
            return saved;
        }
        log('loadSavedTheme: Saved theme is invalid or not found, using default');
    } catch (e) {
        warn('Failed to load theme from localStorage:', e);
    }
    return 'light';
}

/**
 * 起動時に保存されたテーマを自動的に適用
 */
export function initializeTheme() {
    log('initializeTheme: Starting initialization');
    const saved = loadSavedTheme();
    setTheme(saved);
    log('initializeTheme: Initialization complete');
}
