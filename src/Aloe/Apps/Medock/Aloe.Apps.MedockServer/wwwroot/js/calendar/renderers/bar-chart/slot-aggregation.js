/**
 * Slot Aggregation Utilities
 *
 * スロットの集約処理
 */

/**
 * スロットを集約（count/capを合算）
 * @param {Array} slots - 集約するスロットの配列
 * @returns {object|null} 集約されたスロットオブジェクト、またはnull
 */
export function aggregateSlots(slots) {
    if (!slots || slots.length === 0) {
        return null;
    }

    let totalCount = 0;
    let totalCap = 0;
    let hasGrayedOut = false;

    slots.forEach(slot => {
        const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
        const cap = (slot.cap !== undefined && slot.cap !== null && slot.cap > 0) ? slot.cap : 0;
        totalCount += count;
        totalCap += cap;
        if (slot.isGrayedOut) {
            hasGrayedOut = true;
        }
    });

    return {
        count: totalCount,
        cap: totalCap,
        available: totalCap - totalCount,
        isGrayedOut: hasGrayedOut,
        isAggregated: true
    };
}

