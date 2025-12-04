/**
 * Date Utility Functions
 *
 * 日付の変換、判定、計算を行うユーティリティ関数
 */

/**
 * Date オブジェクトを YYYY-MM-DD 形式の文字列に変換
 * @param {Date|string} date - 変換する日付
 * @returns {string} YYYY-MM-DD 形式の日付文字列
 */
export function dateToString(date) {
    if (typeof date === 'string') return date.split('T')[0];
    const d = new Date(date);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

/**
 * YYYY-MM-DD 形式の文字列を Date オブジェクトに変換
 * @param {string} dateStr - 変換する日付文字列
 * @returns {Date} Date オブジェクト
 */
export function parseDate(dateStr) {
    const [year, month, day] = dateStr.split('-').map(Number);
    return new Date(year, month - 1, day);
}

/**
 * 指定された日付が今日かどうかを判定
 * @param {Date|string} date - 判定する日付
 * @returns {boolean} 今日の場合 true
 */
export function isToday(date) {
    const today = new Date();
    return dateToString(date) === dateToString(today);
}

/**
 * 指定された日付が週末（土日）かどうかを判定
 * @param {Date|string} date - 判定する日付
 * @returns {boolean} 週末の場合 true
 */
export function isWeekend(date) {
    const d = new Date(date);
    return d.getDay() === 0 || d.getDay() === 6;
}

/**
 * 指定された日付の曜日を取得
 * @param {Date|string} date - 判定する日付
 * @returns {number} 曜日（0: 日曜日 ～ 6: 土曜日）
 */
export function getDayOfWeek(date) {
    return new Date(date).getDay();
}

/**
 * 指定された年月の月初日を取得
 * @param {number} year - 年
 * @param {number} month - 月（0-11）
 * @returns {Date} 月初日の Date オブジェクト
 */
export function getFirstDayOfMonth(year, month) {
    return new Date(year, month, 1);
}

/**
 * 指定された年月の月末日を取得
 * @param {number} year - 年
 * @param {number} month - 月（0-11）
 * @returns {Date} 月末日の Date オブジェクト
 */
export function getLastDayOfMonth(year, month) {
    return new Date(year, month + 1, 0);
}

/**
 * 指定された日付を含む週の日曜日を取得
 * @param {Date} date - 基準となる日付
 * @returns {Date} 週の開始日（日曜日）
 */
export function getStartOfWeek(date) {
    const d = new Date(date);
    const day = d.getDay();
    d.setDate(d.getDate() - day);
    return d;
}

/**
 * 指定された日付が範囲内にあるかを判定
 * @param {Date|string} date - 判定する日付
 * @param {Date|string} start - 範囲の開始日
 * @param {Date|string} end - 範囲の終了日
 * @returns {boolean} 範囲内の場合 true
 */
export function isDateInRange(date, start, end) {
    const dateStr = dateToString(date);
    const startStr = dateToString(start);
    const endStr = dateToString(end);
    return dateStr >= startStr && dateStr <= endStr;
}
