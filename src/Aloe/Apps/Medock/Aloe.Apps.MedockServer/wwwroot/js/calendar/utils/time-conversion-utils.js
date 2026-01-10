/**
 * 時間変換ユーティリティ
 * 営業時間、昼休み、タイムスロットなどの時間計算を統一的に処理します
 */

/**
 * 業務時間の設定をパース（文字列 "HH:mm" 形式から時間単位に変換）
 * @param {Object} businessHours - 営業時間設定 {workStartTime, workEndTime, lunchStartTime, lunchEndTime}
 * @returns {Object} {startHour, endHour, lunchStartHour, lunchEndHour}
 */
export function parseBusinessHours(businessHours) {
    const parseTime = (timeStr) => {
        if (!timeStr) return null;
        const parts = timeStr.split(':');
        return parseInt(parts[0], 10) + (parseInt(parts[1] || 0, 10) / 60);
    };

    return {
        startHour: parseTime(businessHours?.workStartTime) ?? 9,
        endHour: parseTime(businessHours?.workEndTime) ?? 17,
        lunchStartHour: parseTime(businessHours?.lunchStartTime) ?? null,
        lunchEndHour: parseTime(businessHours?.lunchEndTime) ?? null
    };
}

/**
 * 昼休み時間（時間単位）を計算
 * @param {number} lunchStartHour
 * @param {number} lunchEndHour
 * @returns {number} 昼休み時間（時間）
 */
export function calculateLunchDuration(lunchStartHour, lunchEndHour) {
    if (lunchStartHour === null || lunchEndHour === null) {
        return 0;
    }
    return lunchEndHour - lunchStartHour;
}

/**
 * 実営業時間（昼休みを除く）を計算
 * @param {number} startHour
 * @param {number} endHour
 * @param {number} lunchStartHour
 * @param {number} lunchEndHour
 * @returns {number} 実営業時間（時間）
 */
export function calculateEffectiveWorkHours(startHour, endHour, lunchStartHour, lunchEndHour) {
    const totalHours = endHour - startHour;
    const lunchDuration = calculateLunchDuration(lunchStartHour, lunchEndHour);
    return Math.max(0, totalHours - lunchDuration);
}

/**
 * 時間座標変換関数を生成
 * 時刻を横座標に変換する関数を返す。昼休み時間の詰め込みに対応
 *
 * @param {number} startHour
 * @param {number} endHour
 * @param {number} lunchStartHour
 * @param {number} lunchEndHour
 * @param {number} businessStartX - 営業時間開始時の X 座標
 * @param {number} barAreaWidth - 営業時間帯のバー描画領域の幅
 * @returns {Function} timeInHours => x座標 の関数
 */
export function createTimeToXConverter(startHour, endHour, lunchStartHour, lunchEndHour, businessStartX, barAreaWidth) {
    const totalHours = endHour - startHour;
    const lunchDuration = calculateLunchDuration(lunchStartHour, lunchEndHour);
    const effectiveTotalHours = totalHours - lunchDuration;

    return function timeToX(timeInHours) {
        if (lunchStartHour !== null && lunchEndHour !== null && effectiveTotalHours > 0) {
            // 昼休み時間が定義されている場合、詰め込み処理を行う
            const morningHours = lunchStartHour - startHour;
            const afternoonHours = endHour - lunchEndHour;
            const morningRatio = morningHours / effectiveTotalHours;
            const afternoonRatio = afternoonHours / effectiveTotalHours;

            if (timeInHours < lunchStartHour) {
                // 昼休み前
                const relativePosition = (timeInHours - startHour) / morningHours;
                return businessStartX + (relativePosition * barAreaWidth * morningRatio);
            } else if (timeInHours >= lunchEndHour) {
                // 昼休み後
                const afternoonOffset = barAreaWidth * morningRatio;
                const relativePosition = (timeInHours - lunchEndHour) / afternoonHours;
                return businessStartX + afternoonOffset + (relativePosition * barAreaWidth * afternoonRatio);
            } else {
                // 昼休み時間内 → 午前の終わり位置を返す（スロット端点計算用）
                // バー描画は別途フィルタリングされるため、正確な座標が必要
                return businessStartX + (barAreaWidth * morningRatio);
            }
        } else {
            // 昼休み時間が定義されていない場合
            if (effectiveTotalHours === 0) return businessStartX;
            const relativePosition = (timeInHours - startHour) / effectiveTotalHours;
            return businessStartX + (relativePosition * barAreaWidth);
        }
    };
}

/**
 * 複数の業務時間ラインの X 座標を計算
 * @param {number} timeHour - 時刻（時間単位）
 * @param {Function} timeToX - timeToX 変換関数
 * @returns {number|null} X 座標、または昼休み時間の場合は null
 */
export function getTimelineXPosition(timeHour, timeToX) {
    return timeToX(timeHour);
}
