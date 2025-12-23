/**
 * Slot Time Utilities
 *
 * スロットの時刻変換・解析ユーティリティ
 */

/**
 * 時刻をX座標に変換（セル内の相対位置）
 * @param {number} timeInHours - 時刻（時間単位、例：8.5 = 8:30）
 * @param {number} startHour - 業務開始時刻
 * @param {number} endHour - 業務終了時刻
 * @param {number} cellLeft - セルの左端X座標
 * @param {number} barAreaWidth - 棒グラフエリアの幅
 * @returns {number} X座標
 */
export function timeToX(timeInHours, startHour, endHour, cellLeft, barAreaWidth) {
    const totalHours = endHour - startHour;
    const relativePosition = Math.max(0, Math.min(1, (timeInHours - startHour) / totalHours));
    return cellLeft + 2 + relativePosition * barAreaWidth;
}

/**
 * スロットの時刻文字列を時刻（時間単位）に変換
 * @param {string|object} timeData - 時刻データ（"HH:mm-HH:mm"形式の文字列、または{start, end}オブジェクト）
 * @param {number} startHour - 業務開始時刻（フォールバック用）
 * @param {number} endHour - 業務終了時刻（フォールバック用）
 * @returns {number} 時刻（時間単位）
 */
export function parseTimeSlot(timeData, startHour, endHour) {
    // オブジェクト形式の場合（{start: "HH:mm", end: "HH:mm"}）
    if (typeof timeData === 'object' && timeData.start) {
        const startParts = timeData.start.split(':');
        const hour = parseInt(startParts[0], 10);
        const minute = startParts[1] ? parseInt(startParts[1], 10) : 0;
        return hour + minute / 60;
    }

    // 文字列形式の場合
    const timeStr = typeof timeData === 'string' ? timeData : '';
    // "HH:mm-HH:mm"形式の場合は開始時刻を取得
    const timePart = timeStr.split('-')[0].trim();

    // "HH:MM"形式の時刻を解析
    if (timePart.includes(':')) {
        const parts = timePart.split(':');
        const hour = parseInt(parts[0], 10);
        const minute = parts[1] ? parseInt(parts[1], 10) : 0;
        return hour + minute / 60;
    }

    // その他の場合は開始時刻を返す
    return startHour;
}

/**
 * スロットの開始・終了時刻を取得
 * @param {object} slot - スロットオブジェクト（{start, end}または{time}）
 * @param {number} startHour - 業務開始時刻（フォールバック用）
 * @param {number} endHour - 業務終了時刻（フォールバック用）
 * @returns {{start: number, end: number}} 開始時刻と終了時刻（時間単位）
 */
export function parseSlotTimeRange(slot, startHour, endHour) {
    // オブジェクト形式でstart/endがある場合
    if (slot.start && slot.end) {
        const parseTime = (timeStr) => {
            const parts = timeStr.split(':');
            const hour = parseInt(parts[0], 10);
            const minute = parts[1] ? parseInt(parts[1], 10) : 0;
            return hour + minute / 60;
        };
        return {
            start: parseTime(slot.start),
            end: parseTime(slot.end)
        };
    }

    // timeプロパティがある場合（"HH:mm-HH:mm"形式）
    if (slot.time) {
        const timeStr = typeof slot.time === 'string' ? slot.time : '';
        const parts = timeStr.split('-');
        if (parts.length >= 2) {
            const parseTime = (timeStr) => {
                const timePart = timeStr.trim();
                if (timePart.includes(':')) {
                    const timeParts = timePart.split(':');
                    const hour = parseInt(timeParts[0], 10);
                    const minute = timeParts[1] ? parseInt(timeParts[1], 10) : 0;
                    return hour + minute / 60;
                }
                return startHour;
            };
            return {
                start: parseTime(parts[0]),
                end: parseTime(parts[1])
            };
        }
    }

    // フォールバック: 開始時刻のみ取得
    const start = parseTimeSlot(slot.time || slot, startHour, endHour);
    // 終了時刻は開始時刻+1時間と仮定（デフォルト）
    return {
        start: start,
        end: start + 1
    };
}

