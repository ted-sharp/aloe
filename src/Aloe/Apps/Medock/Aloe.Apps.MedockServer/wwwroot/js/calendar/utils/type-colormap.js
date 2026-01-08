/**
 * Type Colormap Utilities
 *
 * リソースタイプとプランタイプに応じた色マッピング
 * タイプごとに基本色を定義し、利用可能数に応じた色調整を適用
 */

/**
 * リソースタイプ別の基本色マッピング
 * 1: Main, 2: Equipment, 3: Environment, 99: Others
 */
const RESOURCE_TYPE_COLORS = {
    1: '#3b82f6',  // Main: 青 (blue-500)
    2: '#f59e0b',  // Equipment: オレンジ (amber-500)
    3: '#10b981',  // Environment: 緑 (emerald-500)
    99: '#9ca3af'  // Others: グレー (gray-400)
};

/**
 * プランタイプ別の基本色マッピング
 * 1: Plan, 2: Option, 99: Others
 */
const PLAN_TYPE_COLORS = {
    1: '#8b5cf6',  // Plan: 紫 (violet-500)
    2: '#ec4899',  // Option: ピンク (pink-500)
    99: '#9ca3af'  // Others: グレー (gray-400)
};

/**
 * デフォルト色（タイプが不明な場合）
 */
const DEFAULT_COLOR = '#6b7280'; // gray-500

/**
 * RGB値を16進数カラーコードに変換
 * @param {number} r - 赤（0-255）
 * @param {number} g - 緑（0-255）
 * @param {number} b - 青（0-255）
 * @returns {string} 16進数カラーコード（#RRGGBB）
 */
function rgbToHex(r, g, b) {
    return '#' + [r, g, b].map(x => {
        const hex = x.toString(16);
        return hex.length === 1 ? '0' + hex : hex;
    }).join('');
}

/**
 * 16進数カラーコードをRGBに変換
 * @param {string} hex - 16進数カラーコード（#RRGGBB）
 * @returns {number[]} [r, g, b] (0-255)
 */
function hexToRgb(hex) {
    const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? [
        parseInt(result[1], 16),
        parseInt(result[2], 16),
        parseInt(result[3], 16)
    ] : [107, 114, 128]; // デフォルト: gray-500
}

/**
 * RGB値をHSLに変換
 * @param {number} r - 赤（0-255）
 * @param {number} g - 緑（0-255）
 * @param {number} b - 青（0-255）
 * @returns {number[]} [h, s, l] (h: 0-360, s: 0-100, l: 0-100)
 */
function rgbToHsl(r, g, b) {
    r /= 255;
    g /= 255;
    b /= 255;

    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    let h, s, l = (max + min) / 2;

    if (max === min) {
        h = s = 0; // achromatic
    } else {
        const d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        switch (max) {
            case r: h = ((g - b) / d + (g < b ? 6 : 0)) / 6; break;
            case g: h = ((b - r) / d + 2) / 6; break;
            case b: h = ((r - g) / d + 4) / 6; break;
        }
    }

    return [h * 360, s * 100, l * 100];
}

/**
 * HSL値をRGBに変換
 * @param {number} h - 色相（0-360）
 * @param {number} s - 彩度（0-100）
 * @param {number} l - 明度（0-100）
 * @returns {number[]} [r, g, b] (0-255)
 */
function hslToRgb(h, s, l) {
    h /= 360;
    s /= 100;
    l /= 100;

    let r, g, b;

    if (s === 0) {
        r = g = b = l; // achromatic
    } else {
        const hue2rgb = (p, q, t) => {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1/6) return p + (q - p) * 6 * t;
            if (t < 1/2) return q;
            if (t < 2/3) return p + (q - p) * (2/3 - t) * 6;
            return p;
        };

        const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        const p = 2 * l - q;
        r = hue2rgb(p, q, h + 1/3);
        g = hue2rgb(p, q, h);
        b = hue2rgb(p, q, h - 1/3);
    }

    return [Math.round(r * 255), Math.round(g * 255), Math.round(b * 255)];
}

/**
 * リソースタイプに応じた基本色を取得
 * @param {number} resourceTypeCode - リソースタイプコード
 * @returns {string} 16進数カラーコード（#RRGGBB）
 */
export function getResourceTypeColor(resourceTypeCode) {
    return RESOURCE_TYPE_COLORS[resourceTypeCode] || DEFAULT_COLOR;
}

/**
 * プランタイプに応じた基本色を取得
 * @param {number} planTypeCode - プランタイプコード
 * @returns {string} 16進数カラーコード（#RRGGBB）
 */
export function getPlanTypeColor(planTypeCode) {
    return PLAN_TYPE_COLORS[planTypeCode] || DEFAULT_COLOR;
}

/**
 * タイプに応じた基本色を取得（リソースタイプ優先、プランタイプはオプション）
 * @param {number} resourceTypeCode - リソースタイプコード
 * @param {number|null} planTypeCode - プランタイプコード（オプション）
 * @returns {string} 16進数カラーコード（#RRGGBB）
 */
export function getTypeBaseColor(resourceTypeCode, planTypeCode = null) {
    // リソースタイプを優先（プランタイプは将来の拡張用）
    return getResourceTypeColor(resourceTypeCode);
}

/**
 * 利用可能数に応じて色の明度を調整
 * @param {string} baseColor - 基本色（16進数カラーコード）
 * @param {number} available - 空き数
 * @param {number} cap - キャパシティ
 * @returns {string} 調整後の色（16進数カラーコード）
 */
export function adjustColorByAvailability(baseColor, available, cap) {
    if (cap <= 0) return baseColor;
    
    // マイナス値（オーバーキャパシティ）の場合は赤色を返す
    if (available < 0) {
        return '#ef4444'; // red-500
    }
    
    const vacancyRatio = Math.max(0, Math.min(1, available / cap));
    
    // 基本色をRGBに変換
    const [r, g, b] = hexToRgb(baseColor);
    
    // HSLに変換
    const [h, s, l] = rgbToHsl(r, g, b);
    
    // 空き率に応じて明度を調整
    // 空き率1.0（空室）→明度を上げる（明るく）、空き率0.0（満室）→明度を下げる（暗く）
    // ただし、基本色の明度を保ちつつ、空き率に応じて微調整
    const adjustedL = l + (vacancyRatio - 0.5) * 20; // -10% ～ +10% の範囲で調整
    const clampedL = Math.max(20, Math.min(80, adjustedL)); // 20-80%の範囲にクランプ
    
    // RGBに戻す
    const [newR, newG, newB] = hslToRgb(h, s, clampedL);
    
    return rgbToHex(newR, newG, newB);
}
