/**
 * スロット検証・パースユーティリティ
 * スロット時間範囲の解析、フラグの処理、有効性判定を統一的に処理します
 */

/**
 * スロット時間の文字列表現からオブジェクトに変換
 * @param {number} startMin - スロット開始時刻（分単位: 0-1439）
 * @param {number} endMin - スロット終了時刻（分単位: 0-1439）
 * @param {number} defaultStartMin - デフォルト開始時刻（分単位）
 * @param {number} defaultEndMin - デフォルト終了時刻（分単位）
 * @returns {Object} {startHour, startMinute, endHour, endMinute, startMin, endMin, start, end}
 */
export function parseSlotTimeRangeFromMinutes(startMin, endMin, defaultStartMin = 540, defaultEndMin = 570) {
    const start = startMin !== undefined && startMin !== null ? startMin : defaultStartMin;
    const end = endMin !== undefined && endMin !== null ? endMin : defaultEndMin;

    const startHour = Math.floor(start / 60);
    const startMinute = start % 60;
    const endHour = Math.floor(end / 60);
    const endMinute = end % 60;

    return {
        startHour,
        startMinute,
        endHour,
        endMinute,
        startMin: start,
        endMin: end,
        start: startHour + (startMinute / 60),  // 時間.分 形式
        end: endHour + (endMinute / 60)         // 時間.分 形式
    };
}

/**
 * スロット時間の文字列表現からオブジェクトに変換（後方互換性用）
 * @param {string} startStr - スロット開始時刻（"HH:mm" 形式）
 * @param {string} endStr - スロット終了時刻（"HH:mm" 形式）
 * @param {string} defaultStartStr - デフォルト開始時刻（"HH:mm" 形式）
 * @param {string} defaultEndStr - デフォルト終了時刻（"HH:mm" 形式）
 * @returns {Object} {startHour, startMinute, endHour, endMinute, startMin, endMin, start, end}
 */
export function parseSlotTimeRangeFromStrings(startStr, endStr, defaultStartStr = "09:00", defaultEndStr = "09:30") {
    const timeStringToMinutes = (timeStr) => {
        const parts = timeStr.split(':');
        return parseInt(parts[0], 10) * 60 + (parseInt(parts[1] || 0, 10));
    };

    const start = startStr ? timeStringToMinutes(startStr) : timeStringToMinutes(defaultStartStr);
    const end = endStr ? timeStringToMinutes(endStr) : timeStringToMinutes(defaultEndStr);

    return parseSlotTimeRangeFromMinutes(start, end);
}

/**
 * スロット情報を解析
 * @param {Object} slot - スロット情報 {slot_start_min, slot_end_min, ...}
 * @param {number} startHour - 営業開始時刻（時間）
 * @param {number} endHour - 営業終了時刻（時間）
 * @returns {Object} 解析済みスロット情報 {startHour, startMinute, endHour, endMinute, startMin, endMin, start, end}
 */
export function parseSlotTimeRange(slot, startHour = 9, endHour = 17) {
    const startMin = slot.slot_start_min || (startHour * 60);
    const endMin = slot.slot_end_min || ((startHour + 0.5) * 60);

    return parseSlotTimeRangeFromMinutes(startMin, endMin);
}

/**
 * スロットフラグをパース
 * ビット0 (0b0001): IsGrayedOut
 * ビット1 (0b0010): IsOutsideHoursBefore
 * ビット2 (0b0100): IsOutsideHoursAfter
 * ビット3 (0b1000): IsOutsideHoursLunch
 *
 * @param {number} flags - フラグバイト値
 * @returns {Object} {isGrayed, isOutsideBefore, isOutsideAfter, isOutsideLunch, isOutsideHours}
 */
export function parseSlotFlags(flags) {
    const isGrayed = (flags & 0b0001) !== 0;
    const isOutsideBefore = (flags & 0b0010) !== 0;
    const isOutsideAfter = (flags & 0b0100) !== 0;
    const isOutsideLunch = (flags & 0b1000) !== 0;
    const isOutsideHours = isOutsideBefore || isOutsideAfter || isOutsideLunch;

    return {
        isGrayed,
        isOutsideBefore,
        isOutsideAfter,
        isOutsideLunch,
        isOutsideHours
    };
}

/**
 * 有効なスロット インデックスのリストを取得
 * 容量がある、または実績があるスロットのみを対象
 *
 * @param {Array<number>} slotCaps - スロット容量の配列
 * @param {Array<number>} slotCounts - スロット実績（予約数）の配列
 * @returns {Array<number>} 有効なスロットのインデックス配列
 */
export function getValidSlotIndices(slotCaps, slotCounts) {
    const validIndices = [];

    for (let i = 0; i < Math.max(slotCaps?.length || 0, slotCounts?.length || 0); i++) {
        const cap = (slotCaps?.[i] !== undefined && slotCaps?.[i] !== null && slotCaps?.[i] > 0) ? slotCaps?.[i] : 0;
        const count = (slotCounts?.[i] !== undefined && slotCounts?.[i] !== null) ? slotCounts?.[i] : 0;

        if (cap > 0 || count > 0) {
            validIndices.push(i);
        }
    }

    return validIndices;
}

/**
 * スロットが有効か判定
 * 容量または実績がある場合、有効
 *
 * @param {number} capacity - スロット容量
 * @param {number} count - スロット実績
 * @returns {boolean} 有効な場合 true
 */
export function isValidSlot(capacity, count) {
    return (capacity > 0 || (count > 0 && count !== undefined && count !== null));
}

/**
 * スロット配列の有効性をフィルタ
 * @param {Object} slotData - {slotCaps, slotCounts, slotFlags, ...}
 * @returns {Object} 有効スロットのデータ
 */
export function filterValidSlots(slotData) {
    const validIndices = getValidSlotIndices(slotData.slotCaps, slotData.slotCounts);

    return {
        indices: validIndices,
        caps: validIndices.map(i => slotData.slotCaps?.[i]),
        counts: validIndices.map(i => slotData.slotCounts?.[i]),
        flags: validIndices.map(i => slotData.slotFlags?.[i]),
        starts: validIndices.map(i => slotData.slotStarts?.[i]),
        ends: validIndices.map(i => slotData.slotEnds?.[i])
    };
}
