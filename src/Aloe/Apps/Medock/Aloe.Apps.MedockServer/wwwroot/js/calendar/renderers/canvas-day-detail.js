/**
 * Canvas Day Detail Renderer
 * 
 * 日詳細ポップアップ用の描画（Canvas API版）
 * 1日の詳細な棒グラフを表示
 */

import { CONFIG } from '../config.js';
import { drawRect, drawLine, drawText } from '../utils/canvas-utils.js';
import { getWinterColorFromAvailable } from '../utils/winter-colormap.js';
import { isDateInRange } from '../utils/date-utils.js';
import { renderCanvasLineChart } from './canvas-line-chart.js';

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
 */
function parseSlotTimeRange(slot, startHour, endHour) {
    if (slot.start !== undefined && slot.end !== undefined) {
        return parseSlotTimeRangeFromStrings(slot.start, slot.end, startHour, endHour);
    }
    return {
        start: startHour,
        end: endHour
    };
}

/**
 * 日詳細ポップアップを描画（Canvas API版）
 * @param {object} canvasManager - CanvasManagerインスタンス
 * @param {object} state - アプリケーション状態
 * @param {string} dateStr - 日付文字列
 * @param {number} dayNumber - 日にち
 * @param {boolean} isHoliday - 祝日フラグ
 * @param {boolean} showDateText - 日付テキストを表示するか
 * @param {object} equipmentStats - Equipment統計データ（オプション）
 */
export function renderCanvasDayDetail(canvasManager, state, dateStr, dayNumber, isHoliday, showDateText = true, equipmentStats = null) {
    // オフスクリーンバッファに描画（ダブルバッファリング）
    const contexts = canvasManager.getAllOffscreenContexts();
    const width = canvasManager.width;
    const height = canvasManager.height;

    // オフスクリーンレイヤーをクリア
    canvasManager.clearAllOffscreen();

    const backgroundCtx = contexts.get('background');
    const gridCtx = contexts.get('grid');
    const contentCtx = contexts.get('content');

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

    const startHour = state.options.startHour || 8;
    const endHour = state.options.endHour || 18;
    const totalHours = endHour - startHour;

    // レイアウト計算
    const yAxisWidth = 50;
    const xAxisHeight = 30;
    const topPadding = showDateText ? 50 : 30;
    // Equipmentデータがある場合は右Y軸用のスペースを確保
    const hasEquipmentData = equipmentStats && Object.keys(equipmentStats).length > 0;
    const rightAxisWidth = hasEquipmentData ? 50 : 10;
    const graphWidth = width - yAxisWidth - rightAxisWidth;
    const graphHeight = height - topPadding - xAxisHeight;

    // 背景
    drawRect(backgroundCtx, {
        x: 0,
        y: 0,
        width: width,
        height: height,
        fill: '#ffffff'
    });

    // 日付テキストの色を計算
    const dayOfWeek = new Date(dateStr).getDay();
    let textColor;
    if (isDateGrayed) {
        textColor = '#9ca3af';
    } else if (isHoliday || dayOfWeek === 0) {
        textColor = CONFIG.colors.weekend.sun;
    } else if (dayOfWeek === 6) {
        textColor = CONFIG.colors.weekend.sat;
    } else {
        textColor = '#374151';
    }

    // 日付テキスト
    if (showDateText) {
        drawText(contentCtx, {
            text: `${dateStr} (${dayNumber}日)`,
            x: yAxisWidth,
            y: 10,
            width: graphWidth,
            fill: textColor,
            fontSize: CONFIG.font.sizeLarge,
            fontStyle: 'bold',
            align: 'center'
        });
    }

    // データがない場合
    if (slotCount === 0) {
        drawText(contentCtx, {
            text: 'データなし',
            x: yAxisWidth,
            y: topPadding + graphHeight / 2,
            width: graphWidth,
            fill: '#9ca3af',
            fontSize: CONFIG.font.sizeLarge,
            align: 'center'
        });
        return;
    }

    // 最大値を計算
    let maxValue = 0;
    for (let i = 0; i < slotCount; i++) {
        const cap = (slotCaps[i] !== undefined && slotCaps[i] !== null && slotCaps[i] > 0) ? slotCaps[i] : 0;
        maxValue = Math.max(maxValue, cap);
    }

    if (maxValue <= 0) {
        maxValue = 10; // デフォルト
    }

    // Y軸の目盛り
    const tickCount = Math.min(5, maxValue);
    for (let i = 0; i <= tickCount; i++) {
        const value = Math.round((maxValue / tickCount) * i);
        const y = topPadding + graphHeight - (i / tickCount) * graphHeight;

        // 目盛り線
        drawLine(gridCtx, {
            points: [yAxisWidth, y, yAxisWidth + graphWidth, y],
            stroke: '#e5e7eb',
            strokeWidth: 1
        });

        // ラベル
        drawText(gridCtx, {
            text: String(value),
            x: 0,
            y: y - 6,
            width: yAxisWidth - 5,
            fill: '#6b7280',
            fontSize: CONFIG.font.sizeSmall,
            align: 'right'
        });
    }

    // ベースライン
    const baselineY = topPadding + graphHeight;
    drawLine(gridCtx, {
        points: [yAxisWidth, baselineY, yAxisWidth + graphWidth, baselineY],
        stroke: '#374151',
        strokeWidth: 2
    });

    // Y軸の縦線
    drawLine(gridCtx, {
        points: [yAxisWidth, topPadding, yAxisWidth, baselineY],
        stroke: '#374151',
        strokeWidth: 2
    });

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
        return yAxisWidth + relativePosition * graphWidth;
    };

    // スロットを描画（並列配列対応）
    for (let i = 0; i < slotCount; i++) {
        const count = (slotCounts[i] !== undefined && slotCounts[i] !== null) ? slotCounts[i] : 0;
        const cap = (slotCaps[i] !== undefined && slotCaps[i] !== null && slotCaps[i] > 0) ? slotCaps[i] : 0;
        const available = cap - count;

        // フラグからisSlotGrayedを取得（ビット0: IsGrayedOut）
        const isSlotGrayed = (slotFlags && (slotFlags[i] & 0b001) !== 0) || isDateGrayed;

        if (cap <= 0) continue;

        const slotStartStr = slotStarts[i];
        const slotEndStr = slotEnds[i];
        const timeRange = parseSlotTimeRangeFromStrings(slotStartStr, slotEndStr, startHour, endHour);
        const slotStart = Math.max(startHour, timeRange.start);
        const slotEnd = Math.min(endHour, timeRange.end);

        const slotStartX = timeToX(slotStart);
        const slotEndX = timeToX(slotEnd);
        const barX = slotStartX;
        const barWidth = Math.max(2, slotEndX - slotStartX);

        // キャパシティライン
        const capacityY = baselineY - (cap / maxValue) * graphHeight;
        drawLine(contentCtx, {
            points: [barX, capacityY, barX + barWidth, capacityY],
            stroke: '#3b82f6',
            strokeWidth: 2,
            opacity: isSlotGrayed ? 0.4 : 0.8
        });

        // 棒グラフ
        if (available > 0) {
            const barHeight = (available / maxValue) * graphHeight;
            const barY = baselineY - barHeight;
            const slotColor = isSlotGrayed ? '#9ca3af' : getWinterColorFromAvailable(available, cap);

            drawRect(contentCtx, {
                x: barX,
                y: barY,
                width: barWidth,
                height: barHeight,
                fill: slotColor,
                cornerRadius: 2,
                opacity: isSlotGrayed ? 0.4 : 0.9
            });

            // ラベル
            if (barWidth > 20) {
                drawText(contentCtx, {
                    text: `${Math.round(available)}`,
                    x: barX,
                    y: barY - 15,
                    width: barWidth,
                    fill: slotColor,
                    fontSize: CONFIG.font.sizeSmall,
                    fontStyle: 'bold',
                    align: 'center'
                });
            }
        } else if (available < 0) {
            const overflowAmount = Math.abs(available);
            const barHeight = (overflowAmount / maxValue) * graphHeight;
            const barY = baselineY;

            drawRect(contentCtx, {
                x: barX,
                y: barY,
                width: barWidth,
                height: barHeight,
                fill: '#ef4444',
                cornerRadius: 2,
                opacity: isSlotGrayed ? 0.4 : 0.9
            });
        }

        // 時刻ラベル
        if (barWidth > 15) {
            const hourValue = Math.floor(timeRange.start);
            drawText(contentCtx, {
                text: String(hourValue),
                x: barX,
                y: baselineY + 5,
                width: barWidth,
                fill: '#6b7280',
                fontSize: CONFIG.font.sizeSmall,
                align: 'center'
            });
        }
    }

    // 昼休みライン
    if (lunchStartHour !== null && lunchEndHour !== null) {
        const lunchStartX = timeToX(lunchStartHour);
        drawLine(contentCtx, {
            points: [lunchStartX, topPadding, lunchStartX, baselineY],
            stroke: '#d1d5db',
            strokeWidth: 2,
            opacity: 0.8
        });
    }

    // 右Y軸（第二軸: Equipment空き率 0-100%）を描画
    if (hasEquipmentData) {
        const rightAxisX = yAxisWidth + graphWidth;
        const equipmentColor = '#f59e0b'; // amber-500

        // 右Y軸の縦線
        drawLine(gridCtx, {
            points: [rightAxisX, topPadding, rightAxisX, baselineY],
            stroke: equipmentColor,
            strokeWidth: 1,
            opacity: 0.6
        });

        // 0%, 25%, 50%, 75%, 100% の目盛り
        const percentTicks = [0, 25, 50, 75, 100];
        percentTicks.forEach(percent => {
            const y = baselineY - (percent / 100) * graphHeight;

            // 目盛り線（短い横線）
            drawLine(gridCtx, {
                points: [rightAxisX, y, rightAxisX + 5, y],
                stroke: equipmentColor,
                strokeWidth: 1,
                opacity: 0.6
            });

            // ラベル
            drawText(gridCtx, {
                text: `${percent}%`,
                x: rightAxisX + 8,
                y: y - 5,
                width: 40,
                fill: equipmentColor,
                fontSize: CONFIG.font.sizeSmall,
                align: 'left'
            });
        });
    }

    // Equipment折れ線グラフを描画（棒グラフの上に重ねて表示）
    if (hasEquipmentData) {
        renderCanvasLineChart(contentCtx, {
            cellLeft: yAxisWidth,
            cellTop: topPadding,
            cellWidth: graphWidth,
            cellHeight: graphHeight,
            dateStr,
            barAreaTop: topPadding,
            barAreaHeight: graphHeight,
            equipmentStats,
            startHour,
            endHour,
            lunchStartHour,
            lunchEndHour,
            isYearView: false
        });
    }

    // すべての描画が完了したら、オフスクリーンバッファをメインCanvasに一括転送（フェード有効）
    canvasManager.commitAll(true, 150);
}


