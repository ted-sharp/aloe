/**
 * Slot Color Utilities
 *
 * スロットの空き率に基づく色計算
 */

/**
 * 時間帯枠の空き率から色を取得
 * 空きが多い→青、空きが少ない（満杯に近い）→緑
 * @param {number} vacancyRatio - 空き率（0.0 ～ 1.0）
 * @returns {string} カラーコード
 */
export function getSlotColor(vacancyRatio) {
    if (vacancyRatio >= 0.7) {
        // 空き率70%以上 - 空きが多い → 青色
        return '#3b82f6'; // blue
    } else if (vacancyRatio >= 0.3) {
        // 空き率30-70% - 空きあり → シアン/ティール（青と緑の中間）
        return '#14b8a6'; // teal
    } else if (vacancyRatio > 0) {
        // 空き率0-30% - 空き少ない（満杯に近い）→ 緑色
        return '#10b981'; // green
    } else {
        // 空き率0% - 満杯 → 描画しない（この関数は呼ばれない想定）
        return '#10b981'; // green
    }
}

/**
 * 空き数から色を取得
 * 空きが多い→青、空きが少ない（満杯に近い）→緑
 * @param {number} available - 空き数（正の値）
 * @param {number} cap - キャパシティ
 * @returns {string} カラーコード
 */
export function getSlotColorFromAvailable(available, cap) {
    if (cap <= 0) return '#9ca3af'; // キャパシティが0以下の場合はグレー

    const vacancyRatio = available / cap;
    if (vacancyRatio >= 0.7) {
        // 空き率70%以上 - 空きが多い → 青色
        return '#3b82f6'; // blue
    } else if (vacancyRatio >= 0.3) {
        // 空き率30-70% - 空きあり → シアン/ティール（青と緑の中間）
        return '#14b8a6'; // teal
    } else if (vacancyRatio > 0) {
        // 空き率0-30% - 空き少ない（満杯に近い）→ 緑色
        return '#10b981'; // green
    } else {
        // 空き率0% - 満杯 → 緑色
        return '#10b981'; // green
    }
}

