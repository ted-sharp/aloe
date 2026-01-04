/**
 * Canvas Bar Chart Renderer
 * 
 * 日付セルのバーチャート描画（Canvas API版）
 * 簡易表示モード（記号）と詳細表示モード（棒グラフ）の両方をサポート
 */

import { CONFIG } from '../config.js';
import { drawRect, drawLine, drawText, drawCircle, drawPolygon } from '../utils/canvas-utils.js';
import { getWinterColorFromAvailable } from '../utils/winter-colormap.js';
import { getRenderState } from './canvas-render-state.js';
import { isDateInRange } from '../utils/date-utils.js';
import {
    getSymbolFromVacancyRatio,
    calculateVacancyRatio,
    drawSymbol,
    drawSlotInfoText,
    drawNoDataDash
} from '../utils/slot-display-utils.js';

/**
 * 簡易表示モードで記号を描画（Canvas版）
 * @param {CanvasRenderingContext2D} contentCtx - コンテンツレイヤーコンテキスト
 * @param {object} params - パラメータ
 */
export function renderCanvasSimpleViewSymbol(contentCtx, params) {
    const {
        cellLeft, cellTop, cellWidth, cellHeight,
        dateStr, symbolType, isDateGrayed,
        vacancyRatio = 0, available = 0, capacity = 0,
        isYearView = false
    } = params;

    // 日付テキストの高さ
    const dateFontSize = isYearView ? CONFIG.font.sizeDateYear : CONFIG.font.sizeDateMonth;
    const dayTextHeight = dateFontSize + 4;

    // セルサイズが小さい場合は「n/m」表示を非表示にする
    const shouldShowAvailableText = cellWidth >= 50 && cellHeight >= 40;
    const availableTextHeight = shouldShowAvailableText ? 14 : 0;

    // 利用可能な領域を計算
    const availableWidth = cellWidth - 4;
    const availableHeight = cellHeight - dayTextHeight - (shouldShowAvailableText ? availableTextHeight + 4 : 4);

    // 記号サイズを計算
    const baseSize = Math.min(availableWidth, availableHeight);
    const symbolSize = Math.max(6, baseSize * 0.7);

    // 記号を中央に配置
    const symbolCenterX = cellLeft + cellWidth / 2;
    const symbolCenterY = cellTop + dayTextHeight + (availableHeight / 2);

    const opacity = isDateGrayed ? 0.4 : 1;

    // データがない場合は「–」を表示
    const availableNum = available ?? 0;
    const capacityNum = capacity ?? 0;
    if (availableNum === 0 && capacityNum === 0) {
        drawNoDataDash(contentCtx, cellLeft, symbolCenterY, cellWidth, isDateGrayed, opacity);
        return;
    }

    // 記号を描画（統一されたロジックを使用）
    drawSymbol(contentCtx, symbolCenterX, symbolCenterY, symbolSize, symbolType, opacity);

    // 「n/m」テキストを表示
    if (shouldShowAvailableText && capacity > 0) {
        const textY = symbolCenterY + symbolSize / 2 + 4;
        drawSlotInfoText(contentCtx, cellLeft, textY, cellWidth, available, capacity, isDateGrayed, opacity);
    }
}

/**
 * 詳細表示モードでバーチャートを描画（Canvas版 - 並列配列対応）
 * @param {CanvasRenderingContext2D} contentCtx - コンテンツレイヤーコンテキスト
 * @param {object} params - パラメータ
 */
export function renderCanvasBarChart(contentCtx, params) {
    const {
        cellLeft, cellTop, cellWidth, cellHeight,
        dateStr, barAreaTop, barAreaHeight, labelAreaHeight,
        isDateGrayed, slotStarts, slotEnds, slotCounts, slotCaps, slotAvailables, slotFlags,
        startHour, endHour,
        lunchStartHour, lunchEndHour, isYearView
    } = params;

    const renderState = getRenderState();
    const slotCount = slotStarts.length;

    // データがない場合は描画しない
    const validIndices = [];
    for (let i = 0; i < slotCount; i++) {
        const cap = (slotCaps[i] !== undefined && slotCaps[i] !== null && slotCaps[i] > 0) ? slotCaps[i] : 0;
        const count = (slotCounts[i] !== undefined && slotCounts[i] !== null) ? slotCounts[i] : 0;
        if (cap > 0 || count > 0) {
            validIndices.push(i);
        }
    }

    if (validIndices.length === 0) {
        return false;
    }

    const totalHours = endHour - startHour;
    const barAreaWidth = cellWidth - 4;

    const businessStartX = cellLeft + 2;
    const businessEndX = cellLeft + 2 + barAreaWidth;

    // 昼休み時間帯の長さを計算
    const lunchDuration = (lunchStartHour !== null && lunchEndHour !== null)
        ? (lunchEndHour - lunchStartHour)
        : 0;
    const effectiveTotalHours = totalHours - lunchDuration;

    // 時刻をX座標に変換する関数
    const timeToX = (timeInHours) => {
        let relativePosition;
        if (lunchStartHour !== null && lunchEndHour !== null && effectiveTotalHours > 0) {
            const morningHours = lunchStartHour - startHour;
            const afternoonHours = endHour - lunchEndHour;

            if (timeInHours < lunchStartHour) {
                const morningRatio = (timeInHours - startHour) / morningHours;
                relativePosition = morningRatio * (morningHours / effectiveTotalHours);
            } else if (timeInHours >= lunchEndHour) {
                const afternoonRatio = (timeInHours - lunchEndHour) / afternoonHours;
                const morningWidth = morningHours / effectiveTotalHours;
                relativePosition = morningWidth + afternoonRatio * (afternoonHours / effectiveTotalHours);
            } else {
                relativePosition = morningHours / effectiveTotalHours;
            }
        } else {
            relativePosition = Math.max(0, Math.min(1, (timeInHours - startHour) / totalHours));
        }
        return businessStartX + relativePosition * barAreaWidth;
    };

    // スロットを分類
    const slotsInBusiness = [];
    let hasOutsideHoursBefore = false;
    let hasOutsideHoursAfter = false;
    let hasOutsideHoursLunch = false;

    for (const idx of validIndices) {
        const slotStart = slotStarts[idx];
        const slotEnd = slotEnds[idx];
        const count = (slotCounts[idx] !== undefined && slotCounts[idx] !== null) ? slotCounts[idx] : 0;

        // フラグからisOutsideHoursを取得（ビット1）
        const isOutsideHours = slotFlags ? (slotFlags[idx] & 0b010) !== 0 : false;

        // 時間範囲を解析
        const timeRange = parseSlotTimeRangeFromStrings(slotStart, slotEnd, startHour, endHour);
        const slotStartHour = timeRange.start;
        const slotEndHour = timeRange.end;

        if (isOutsideHours) {
            if (count > 0) {
                if (slotEndHour <= startHour) {
                    hasOutsideHoursBefore = true;
                } else if (slotStartHour >= endHour) {
                    hasOutsideHoursAfter = true;
                } else if (lunchStartHour !== null && lunchEndHour !== null &&
                    slotStartHour >= lunchStartHour && slotEndHour <= lunchEndHour) {
                    hasOutsideHoursLunch = true;
                }
            }
            continue;
        }

        // 昼休み時間帯かどうかを判定
        const isInLunchTime = lunchStartHour !== null && lunchEndHour !== null &&
            ((slotStartHour >= lunchStartHour && slotStartHour < lunchEndHour) ||
                (slotEndHour > lunchStartHour && slotEndHour <= lunchEndHour) ||
                (slotStartHour <= lunchStartHour && slotEndHour >= lunchEndHour));

        if (slotEndHour < startHour || slotStartHour < startHour) {
            // ビジネスアワー前
        } else if (slotStartHour >= endHour) {
            // ビジネスアワー後
        } else if (isInLunchTime) {
            // 昼休み時間帯
        } else {
            slotsInBusiness.push(idx);
        }
    }

    // 最大値を計算
    let maxValue = 0;
    for (const idx of slotsInBusiness) {
        const cap = (slotCaps[idx] !== undefined && slotCaps[idx] !== null && slotCaps[idx] > 0) ? slotCaps[idx] : 0;
        maxValue = Math.max(maxValue, cap);
    }

    if (maxValue <= 0) {
        return false;
    }

    const baselineY = barAreaTop + barAreaHeight;

    // スロットを描画
    for (const idx of slotsInBusiness) {
        const count = (slotCounts[idx] !== undefined && slotCounts[idx] !== null) ? slotCounts[idx] : 0;
        const cap = (slotCaps[idx] !== undefined && slotCaps[idx] !== null && slotCaps[idx] > 0) ? slotCaps[idx] : 0;
        const available = cap - count;

        // フラグからisSlotGrayedを取得（ビット0: IsGrayedOut）
        const isSlotGrayed = (slotFlags && (slotFlags[idx] & 0b001) !== 0) || isDateGrayed;

        const slotStartStr = slotStarts[idx];
        const slotEndStr = slotEnds[idx];
        const timeRange = parseSlotTimeRangeFromStrings(slotStartStr, slotEndStr, startHour, endHour);
        let slotStart = Math.max(startHour, timeRange.start);
        let slotEnd = Math.min(endHour, timeRange.end);

        // 昼休み時間帯と重なる場合は調整
        if (lunchStartHour !== null && lunchEndHour !== null) {
            if (slotStart < lunchStartHour && slotEnd > lunchEndHour) {
                slotEnd = lunchStartHour;
            } else if (slotStart >= lunchStartHour && slotEnd <= lunchEndHour) {
                return;
            } else if (slotStart < lunchStartHour && slotEnd > lunchStartHour) {
                slotEnd = lunchStartHour;
            } else if (slotStart < lunchEndHour && slotEnd > lunchEndHour) {
                slotStart = lunchEndHour;
            }
        }

        const slotStartX = timeToX(slotStart);
        const slotEndX = timeToX(slotEnd);
        const barX = slotStartX;
        const barWidth = Math.max(1, slotEndX - slotStartX);

        // キャパシティライン
        if (cap > 0) {
            const capacityY = baselineY - (cap / maxValue) * barAreaHeight;
            drawLine(contentCtx, {
                points: [barX, capacityY, barX + barWidth, capacityY],
                stroke: '#3b82f6',
                strokeWidth: 1,
                opacity: isSlotGrayed ? 0.4 : 0.8
            });
        }

        // 棒グラフ
        if (available > 0) {
            const barHeight = (available / maxValue) * barAreaHeight;
            const barY = baselineY - barHeight;
            const slotColor = isSlotGrayed ? '#9ca3af' : getWinterColorFromAvailable(available, cap);

            drawRect(contentCtx, {
                x: barX,
                y: barY,
                width: Math.max(1, barWidth),
                height: barHeight,
                fill: slotColor,
                cornerRadius: Math.min(1, barHeight / 2),
                opacity: isSlotGrayed ? 0.4 : 1
            });

            // Hit Test用にバー情報を登録（並列配列インデックスを含める）
            renderState.addBar(dateStr, {
                x: barX,
                y: barY,
                width: barWidth,
                height: barHeight,
                slotIndex: idx,
                slot: { start: slotStartStr, end: slotEndStr, count, cap, available },
                color: slotColor
            });
        } else if (available < 0) {
            const overflowAmount = Math.abs(available);
            const barHeight = (overflowAmount / maxValue) * barAreaHeight;
            const barY = baselineY;

            drawRect(contentCtx, {
                x: barX,
                y: barY,
                width: Math.max(1, barWidth),
                height: barHeight,
                fill: '#ef4444',
                cornerRadius: Math.min(1, barHeight / 2),
                opacity: isSlotGrayed ? 0.4 : 1
            });

            // オーバーフローバーも登録
            renderState.addBar(dateStr, {
                x: barX,
                y: barY,
                width: barWidth,
                height: barHeight,
                slotIndex: idx,
                slot: { start: slotStartStr, end: slotEndStr, count, cap, available },
                color: '#ef4444',
                isOverflow: true
            });
        }

        // ラベル
        if (labelAreaHeight > 0) {
            // 最初と最後のスロットを判定
            const isFirstSlot = idx === slotsInBusiness[0];
            const isLastSlot = idx === slotsInBusiness[slotsInBusiness.length - 1];

            // barWidthが狭い場合は最初と最後のみ表示
            const shouldShowLabel = barWidth >= 10 || isFirstSlot || isLastSlot;

            if (shouldShowLabel) {
                const hourValue = Math.floor(timeRange.start);
                const labelFontSize = isYearView ? 6 : 7;
                // barWidthが10px以上の場合は2桁/1桁を判定、10px未満は必ず2桁表示
                const canShowTwoDigits = barWidth >= 15 || (barWidth < 10 && (isFirstSlot || isLastSlot));
                const labelText = canShowTwoDigits ? String(hourValue) : String(hourValue % 10);
                const labelY = baselineY + 2;
                const labelColor = isSlotGrayed ? '#d1d5db' : '#9ca3af';

                drawText(contentCtx, {
                    text: labelText,
                    x: barX,
                    y: labelY,
                    width: barWidth,
                    fill: labelColor,
                    fontSize: labelFontSize,
                    fontFamily: CONFIG.font.numberFamily,
                    align: 'center',
                    opacity: isSlotGrayed ? 0.6 : 1
                });
            }
        }
    }

    // 業務時間の縦ライン
    const beforeLineColor = hasOutsideHoursBefore ? '#ef4444' : '#d1d5db';
    const beforeLineWidth = hasOutsideHoursBefore ? 2 : 1;
    drawLine(contentCtx, {
        points: [businessStartX, barAreaTop, businessStartX, barAreaTop + barAreaHeight],
        stroke: beforeLineColor,
        strokeWidth: beforeLineWidth,
        opacity: 0.8
    });

    const afterLineColor = hasOutsideHoursAfter ? '#ef4444' : '#d1d5db';
    const afterLineWidth = hasOutsideHoursAfter ? 2 : 1;
    drawLine(contentCtx, {
        points: [businessEndX, barAreaTop, businessEndX, barAreaTop + barAreaHeight],
        stroke: afterLineColor,
        strokeWidth: afterLineWidth,
        opacity: 0.8
    });

    // 昼休みライン
    if (lunchStartHour !== null && lunchEndHour !== null) {
        const lunchStartX = timeToX(lunchStartHour);
        const lunchLineColor = hasOutsideHoursLunch ? '#ef4444' : '#d1d5db';
        const lunchLineWidth = hasOutsideHoursLunch ? 2 : 1;

        drawLine(contentCtx, {
            points: [lunchStartX, barAreaTop, lunchStartX, barAreaTop + barAreaHeight],
            stroke: lunchLineColor,
            strokeWidth: lunchLineWidth,
            opacity: 0.8
        });
    }

    return true;
}

/**
 * スロットの時間範囲を解析（並列配列用）
 * @param {number} startMinutes - 開始時刻（分単位）
 * @param {number} endMinutes - 終了時刻（分単位）
 * @param {number} startHour - デフォルト開始時刻
 * @param {number} endHour - デフォルト終了時刻
 * @returns {{start: number, end: number}}
 */
function parseSlotTimeRangeFromStrings(startMinutes, endMinutes, startHour, endHour) {
    if (typeof startMinutes === 'number' && typeof endMinutes === 'number') {
        return {
            start: startMinutes / 60,  // 分を時間に変換
            end: endMinutes / 60
        };
    }

    // デフォルト: 業務時間全体
    return {
        start: startHour,
        end: endHour
    };
}

/**
 * スロットの時間範囲を解析（旧形式・互換性用）
 * @param {object} slot - スロット
 * @param {number} startHour - 開始時刻
 * @param {number} endHour - 終了時刻
 * @returns {{start: number, end: number}}
 */
function parseSlotTimeRange(slot, startHour, endHour) {
    // slotに時刻情報がある場合はそれを使用
    if (slot.start !== undefined && slot.end !== undefined) {
        return parseSlotTimeRangeFromStrings(slot.start, slot.end, startHour, endHour);
    }

    // デフォルト: 業務時間全体
    return {
        start: startHour,
        end: endHour
    };
}

/**
 * 日付セルのバーチャートを描画（統合関数）
 * @param {Map<string, CanvasRenderingContext2D>} contexts - レイヤーコンテキストのマップ
 * @param {object} state - アプリケーション状態
 * @param {object} params - パラメータ
 */
export function renderCanvasDayBarChart(contexts, state, params) {
    const {
        cellLeft, cellTop, cellWidth, cellHeight,
        dateStr, dayNumber, isHoliday
    } = params;

    const contentCtx = contexts.get('content');
    const backgroundCtx = contexts.get('background');

    // セルサイズが小さすぎる場合はスキップ
    const isTooSmall = cellWidth < 10 || cellHeight < 10;
    if (isTooSmall) {
        return;
    }

    // 時間帯枠データを取得（並列配列形式）
    const stats = state.mainStats.get(dateStr);
    const slotStarts = stats?.slotStarts || [];
    const slotEnds = stats?.slotEnds || [];
    const slotCounts = stats?.slotCounts || [];
    const slotCaps = stats?.slotCaps || [];
    const slotAvailables = stats?.slotAvailables || [];
    const slotFlags = stats?.slotFlags || null;
    const slotCount = slotStarts.length;
    // 営業時間情報を取得
    const businessHours = state.options?.businessHours;

    let lunchStartHour = null;
    let lunchEndHour = null;
    if (businessHours && businessHours.lunchStartTime && businessHours.lunchEndTime) {
        const parseTime = (timeStr) => {
            const parts = timeStr.split(':');
            return parseInt(parts[0], 10) + (parseInt(parts[1] || 0, 10) / 60);
        };
        lunchStartHour = parseTime(businessHours.lunchStartTime);
        lunchEndHour = parseTime(businessHours.lunchEndTime);
    }

    // グレーアウト判定
    let isDateGrayed = false;
    if (state.confirmedDateRange) {
        isDateGrayed = !isDateInRange(dateStr, state.confirmedDateRange.start, state.confirmedDateRange.end);
    } else {
        isDateGrayed = stats?.isDayGrayedOut || false;
    }

    // ラベル表示用のスペースを確保
    const isYearView = state.currentView === 'year';
    const dateFontSize = isYearView ? CONFIG.font.sizeDateYear : CONFIG.font.sizeDateMonth;
    const dayTextHeight = dateFontSize + 4;
    const barAreaTop = cellTop + dayTextHeight;
    const labelAreaHeight = (cellWidth >= 40 && cellHeight >= 50) ? (isYearView ? 10 : 12) : 0;
    const barAreaHeight = Math.max(0, cellHeight - dayTextHeight - 4 - labelAreaHeight);

    // 背景矩形
    const bgWidth = Math.max(0, cellWidth - 2);
    const bgHeight = Math.max(0, cellHeight - 2);
    const bgCornerRadius = Math.min(2, bgWidth / 2, bgHeight / 2);

    drawRect(backgroundCtx, {
        x: cellLeft + 1,
        y: cellTop + 1,
        width: bgWidth,
        height: bgHeight,
        fill: isDateGrayed ? '#f3f4f6' : CONFIG.colors.slot.background,
        cornerRadius: Math.max(0, bgCornerRadius),
        opacity: isDateGrayed ? 0.6 : 1
    });

    // 日付テキスト
    const dayOfWeek = new Date(dateStr).getDay();
    let textColor;
    if (isDateGrayed) {
        // グレーアウト時でも土日を区別
        if (isHoliday || dayOfWeek === 0) {
            textColor = '#d1a3a3';  // 赤みがかったグレー（日曜日）
        } else if (dayOfWeek === 6) {
            textColor = '#a3b3d1';  // 青みがかったグレー（土曜日）
        } else {
            textColor = '#9ca3af';  // 通常のグレー（平日）
        }
    } else if (isHoliday || dayOfWeek === 0) {
        textColor = CONFIG.colors.weekend.sun;
    } else if (dayOfWeek === 6) {
        textColor = CONFIG.colors.weekend.sat;
    } else {
        textColor = '#374151';
    }

    drawText(contentCtx, {
        text: String(dayNumber),
        x: cellLeft + 1,
        y: cellTop + 2,
        width: cellWidth - 2,
        fill: textColor,
        fontSize: dateFontSize,
        fontFamily: CONFIG.font.numberFamily,
        align: 'center'
    });

    // 祝日名を表示
    const holidayName = state.holidays?.get(dateStr);
    if (holidayName) {
        const holidayFontSize = isYearView ? 7 : 12;
        const maxHolidayChars = Math.floor(cellWidth / (holidayFontSize * 0.6)); // 文字幅の推定
        const displayHolidayName = holidayName.length > maxHolidayChars
            ? holidayName.substring(0, Math.max(1, maxHolidayChars - 1)) + '…'
            : holidayName;

        drawText(contentCtx, {
            text: displayHolidayName,
            x: cellLeft + 1,
            y: cellTop + dateFontSize + 4,
            width: cellWidth - 2,
            fill: textColor,
            fontSize: holidayFontSize,
            fontFamily: CONFIG.font.numberFamily,
            align: 'center',
            opacity: 0.9
        });
    }

    // 簡易表示モードの判定
    const showSimpleView = state.options?.showSimpleView ?? false;

    if (showSimpleView) {
        // 簡易表示モード - 統一されたロジックで空き率を計算
        const { vacancyRatio: overallVacancyRatio, available: totalAvailable, capacity: totalCapacity } =
            calculateVacancyRatio(slotCaps, slotCounts, slotFlags, isDateGrayed);

        const symbolType = getSymbolFromVacancyRatio(overallVacancyRatio);
        renderCanvasSimpleViewSymbol(contentCtx, {
            cellLeft, cellTop, cellWidth, cellHeight,
            dateStr, symbolType, isDateGrayed,
            vacancyRatio: overallVacancyRatio,
            available: totalAvailable,
            capacity: totalCapacity,
            isYearView
        });
    } else if (slotCount > 0) {
        // 詳細表示モード
        const startHour = state.options.startHour || 8;
        const endHour = state.options.endHour || 18;

        renderCanvasBarChart(contentCtx, {
            cellLeft, cellTop, cellWidth, cellHeight,
            dateStr, barAreaTop, barAreaHeight, labelAreaHeight,
            isDateGrayed,
            slotStarts, slotEnds, slotCounts, slotCaps, slotAvailables, slotFlags,
            startHour, endHour,
            lunchStartHour, lunchEndHour, isYearView
        });
    }
}


