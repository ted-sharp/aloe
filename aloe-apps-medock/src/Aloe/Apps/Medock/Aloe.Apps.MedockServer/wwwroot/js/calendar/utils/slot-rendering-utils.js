/**
 * スロットレンダリングユーティリティ
 * グレーアウト状態、日付の色、ビジネスラインの描画などを統一的に処理します
 */

import { isDateInRange } from './date-utils.js';

/**
 * 日付のグレーアウト状態を取得
 * @param {string} dateStr - 日付文字列（"YYYY-MM-DD"）
 * @param {Object} confirmedDateRange - 確認済み日付範囲 {start, end}
 * @param {Object} stats - 統計情報
 * @returns {boolean} グレーアウトされている場合 true
 */
export function getDateGrayOutStatus(dateStr, confirmedDateRange, stats) {
    let isDateGrayed = false;

    if (confirmedDateRange) {
        isDateGrayed = !isDateInRange(dateStr, confirmedDateRange.start, confirmedDateRange.end);
    } else {
        isDateGrayed = stats?.isDayGrayedOut || false;
    }

    return isDateGrayed;
}

/**
 * 日付表示用のテキスト色を取得
 * @param {boolean} isDateGrayed - グレーアウト中か
 * @param {boolean} isHoliday - 祝日か
 * @param {number} dayOfWeek - 曜日（0=日, 6=土）
 * @returns {string} CSS 色コード
 */
export function getDateTextColor(isDateGrayed, isHoliday, dayOfWeek) {
    let textColor;

    if (isDateGrayed) {
        if (isHoliday || dayOfWeek === 0) {
            textColor = '#d1a3a3';
        } else if (dayOfWeek === 6) {
            textColor = '#a3b3d1';
        } else {
            textColor = '#9ca3af';
        }
    } else if (isHoliday || dayOfWeek === 0) {
        textColor = '#ef4444'; // 日曜日・祝日は赤
    } else if (dayOfWeek === 6) {
        textColor = '#3b82f6'; // 土曜日は青
    } else {
        textColor = '#374151'; // 平日は濃いグレー
    }

    return textColor;
}

/**
 * 時間帯のボーダーラインの描画情報を計算
 * @param {number} timeHour - 時刻（時間単位、例：9.5 = 09:30）
 * @param {Function} timeToX - 時間 => X座標 の変換関数
 * @param {number} lineStartY - ライン開始 Y 座標
 * @param {number} lineEndY - ライン終了 Y 座標
 * @param {Object} options - {alertColor, normalColor, alertWidth, normalWidth} 色・幅オプション
 * @returns {Object|null} {x, stroke, strokeWidth} または null（昼休み時間の場合）
 */
export function calculateBusinessLineDraw(timeHour, timeToX, lineStartY, lineEndY, options = {}) {
    const defaultOptions = {
        alertColor: '#ef4444',
        normalColor: '#d1d5db',
        alertWidth: 2,
        normalWidth: 1
    };

    const opts = { ...defaultOptions, ...options };
    const x = timeToX(timeHour);

    if (x === null) {
        return null; // 昼休み時間
    }

    return {
        x,
        startY: lineStartY,
        endY: lineEndY,
        stroke: opts.normalColor,
        strokeWidth: opts.normalWidth,
        opacity: 0.8
    };
}

/**
 * 複数のビジネス時間ラインを描画
 * @param {Object} timeConverter - {timeToX, startHour, endHour, lunchStartHour, lunchEndHour}
 * @param {number} barAreaTop - バー領域の上端 Y 座標
 * @param {number} barAreaHeight - バー領域の高さ
 * @param {Object} hasOutside - {before, after, lunch} 営業外時間フラグ
 * @returns {Array<Object>} 描画情報の配列
 */
export function getBusinessHoursLinesToDraw(timeConverter, barAreaTop, barAreaHeight, hasOutside) {
    const { timeToX, startHour, endHour, lunchStartHour, lunchEndHour } = timeConverter;
    const lines = [];

    const addLine = (timeHour, isAlert) => {
        const lineInfo = calculateBusinessLineDraw(
            timeHour,
            timeToX,
            barAreaTop,
            barAreaTop + barAreaHeight,
            {
                alertColor: isAlert ? '#ef4444' : '#d1d5db',
                normalColor: isAlert ? '#ef4444' : '#d1d5db',
                alertWidth: 2,
                normalWidth: 1
            }
        );
        if (lineInfo) lines.push(lineInfo);
    };

    // 営業開始ライン
    if (hasOutside?.before) {
        addLine(startHour, true);
    }

    // 昼休み開始ライン
    if (lunchStartHour !== null) {
        addLine(lunchStartHour, false);
    }

    // 昼休み終了ライン
    if (lunchEndHour !== null) {
        addLine(lunchEndHour, false);
    }

    // 営業終了ライン
    if (hasOutside?.after) {
        addLine(endHour, true);
    }

    return lines;
}

