/**
 * Winter Colormap Utilities
 *
 * Winterカラーマップの実装
 * 空室率（0.0-1.0）に基づいて色を取得
 * 空室率1.0（空室）→青、空室率0.0（満室）→緑
 * キャパオーバー（マイナス値）→赤
 *
 * Winterカラーマップは、青から緑への線形補間を提供するカラーマップです。
 * matplotlibの標準的なWinterカラーマップの実装に基づいています。
 */

import { getTypeBaseColor, adjustColorByAvailability } from './type-colormap.js';
import { rgbToHex } from './canvas-utils.js';

/**
 * Winterカラーマップの256色RGBルックアップテーブル
 * 値は0.0（青）から1.0（緑）への連続的な色変化を提供
 * R = 0, G = ratio, B = 1 - ratio の線形補間
 */
const WINTER_COLORMAP = [];

// 256色のルックアップテーブルを生成
for (let i = 0; i < 256; i++) {
    const ratio = i / 255.0;
    WINTER_COLORMAP[i] = [0.0, ratio, 1.0 - ratio];
}

/**
 * RGB値（0-1範囲）を16進数カラーコードに変換
 * @param {number} r - 赤（0-1）
 * @param {number} g - 緑（0-1）
 * @param {number} b - 青（0-1）
 * @returns {string} 16進数カラーコード（#RRGGBB）
 */
function rgbToHexFromFloat(r, g, b) {
    const rInt = Math.round(r * 255);
    const gInt = Math.round(g * 255);
    const bInt = Math.round(b * 255);
    return rgbToHex(rInt, gInt, bInt);
}

/**
 * Winterカラーマップから色を取得
 * @param {number} ratio - 値（0.0-1.0、範囲外の値も許容）
 * @returns {string} 16進数カラーコード（#RRGGBB）
 */
export function getWinterColor(ratio) {
    // 値を0-1の範囲にクランプ
    const clampedRatio = Math.max(0, Math.min(1, ratio));
    
    // 0-255のインデックスに変換（配列の範囲内に保証）
    const index = Math.max(0, Math.min(255, Math.floor(clampedRatio * 255)));
    
    // ルックアップテーブルからRGB値を取得（安全性チェック）
    const color = WINTER_COLORMAP[index];
    if (!color || color.length !== 3) {
        // フォールバック: エラー時はグレーを返す
        return '#9ca3af';
    }
    
    const [r, g, b] = color;
    
    // 16進数カラーコードに変換
    return rgbToHexFromFloat(r, g, b);
}

/**
 * 空室率からWinterカラーマップの色を取得
 * 空室率1.0（空室）→青、空室率0.0（満室）→緑
 * @param {number} vacancyRatio - 空室率（0.0-1.0）
 * @returns {string} 16進数カラーコード（#RRGGBB）
 */
export function getWinterColorFromVacancyRatio(vacancyRatio) {
    // 空室率を反転して使用（空室率1.0→Winter 0.0→青、空室率0.0→Winter 1.0→緑）
    return getWinterColor(1.0 - vacancyRatio);
}

/**
 * 空き数とキャパシティからWinterカラーマップの色を取得
 * 空室率1.0（空室）→青、空室率0.0（満室）→緑
 * マイナス値（オーバーキャパシティ）の場合は赤色を返す
 * @param {number} available - 空き数（負の値も許容：オーバーキャパシティ）
 * @param {number} cap - キャパシティ
 * @returns {string} 16進数カラーコード（#RRGGBB）
 */
export function getWinterColorFromAvailable(available, cap) {
    if (cap <= 0) return '#9ca3af'; // キャパシティが0以下の場合はグレー
    
    // マイナス値（オーバーキャパシティ）の場合は赤色を返す
    if (available < 0) {
        return '#FF0000'; // 赤
    }
    
    const vacancyRatio = available / cap;
    // 空室率が1.0を超える場合もクランプされるが、明示的に処理
    return getWinterColorFromVacancyRatio(Math.max(0, Math.min(1, vacancyRatio)));
}

/**
 * 使用率からWinterカラーマップの色を取得
 * 使用率0.0（空室）→青、使用率1.0（満室）→緑
 * @param {number} usageRatio - 使用率（0.0-1.0）
 * @returns {string} 16進数カラーコード（#RRGGBB）
 */
export function getWinterColorFromUsageRatio(usageRatio) {
    // 使用率をそのまま使用（使用率0.0→Winter 0.0→青、使用率1.0→Winter 1.0→緑）
    // 使用率0.0 = 空室率1.0 → 青、使用率1.0 = 空室率0.0 → 緑
    return getWinterColor(usageRatio);
}

/**
 * タイプ情報と利用可能数から色を取得
 * タイプの基本色をベースに、利用可能数に応じた色調整を適用
 * グレーアウト時はタイプの色を保ったまま透明度を下げる（色は変更しない）
 * @param {number} resourceTypeCode - リソースタイプコード
 * @param {number|null} planTypeCode - プランタイプコード（オプション）
 * @param {number} available - 空き数（負の値も許容：オーバーキャパシティ）
 * @param {number} cap - キャパシティ
 * @param {boolean} isGrayed - グレーアウト中かどうか
 * @returns {string} 16進数カラーコード（#RRGGBB）
 */
export function getColorFromTypeAndAvailable(resourceTypeCode, planTypeCode, available, cap, isGrayed) {
    let color;

    // タイプ情報が存在しない場合は従来のWinterカラーマップを使用（後方互換性）
    if (!resourceTypeCode || resourceTypeCode === 0) {
        color = getWinterColorFromAvailable(available, cap);
    } else {
        // タイプの基本色を取得
        const baseColor = getTypeBaseColor(resourceTypeCode, planTypeCode);

        // 利用可能数に応じて色を調整
        color = adjustColorByAvailability(baseColor, available, cap);
    }

    // グレーアウト時は色をグレーに寄せる（タイプの有無を問わず適用）
    if (isGrayed) {
        const grayColor = desaturateColor(color, 0.3);  // 元の色30% + グレー70%でグレーに寄せる
        return grayColor;
    }

    return color;
}

/**
 * 色の彩度を下げる（グレーに寄せる）
 * @param {string} hexColor - 16進数カラーコード（#RRGGBB）
 * @param {number} saturationFactor - 彩度の倍率（0.0 = グレースケール、1.0 = 元の色）
 * @returns {string} 彩度を調整した16進数カラーコード
 */
function desaturateColor(hexColor, saturationFactor) {
    // RGB値に変換
    const r = parseInt(hexColor.slice(1, 3), 16);
    const g = parseInt(hexColor.slice(3, 5), 16);
    const b = parseInt(hexColor.slice(5, 7), 16);

    // グレー値（明度）を計算（人間の目の感度を考慮した加重平均）
    const gray = Math.round(0.299 * r + 0.587 * g + 0.114 * b);

    // 彩度を調整（元の色とグレーをブレンド）
    const newR = Math.round(gray + (r - gray) * saturationFactor);
    const newG = Math.round(gray + (g - gray) * saturationFactor);
    const newB = Math.round(gray + (b - gray) * saturationFactor);

    // 16進数に戻す
    return `#${newR.toString(16).padStart(2, '0')}${newG.toString(16).padStart(2, '0')}${newB.toString(16).padStart(2, '0')}`;
}
