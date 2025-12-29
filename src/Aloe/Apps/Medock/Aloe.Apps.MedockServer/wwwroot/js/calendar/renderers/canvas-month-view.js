/**
 * Canvas Month View Renderer
 * 
 * 月間カレンダー表示（Canvas API版）
 * 1ヶ月全体を7列グリッドで表示
 */

import { clearCanvas } from '../utils/canvas-utils.js';
import { renderCanvasMonthCalendar } from './canvas-month-calendar.js';
import { renderCanvasDayBarChart } from './canvas-bar-chart.js';
import { renderCanvasLineChart } from './canvas-line-chart.js';
import { getRenderState, resetRenderState } from './canvas-render-state.js';

/**
 * 月間カレンダーを描画（Canvas API版）
 * @param {object} canvasManager - CanvasManagerインスタンス
 * @param {object} state - アプリケーション状態
 */
export function renderCanvasMonthView(canvasManager, state) {
    // オフスクリーンバッファに描画（ダブルバッファリング）
    const contexts = canvasManager.getAllOffscreenContexts();
    const width = canvasManager.width;
    const height = canvasManager.height;

    // オフスクリーンレイヤーをクリア
    canvasManager.clearAllOffscreen();

    // Render Stateをリセット
    resetRenderState();
    const renderState = getRenderState();
    renderState.setViewType('month');

    const year = state.currentDate.getFullYear();
    const month = state.currentDate.getMonth();

    // 月カレンダーのグリッドとヘッダーを描画
    const monthInfo = renderCanvasMonthCalendar(contexts, state, year, month, 0, 0, width, height, {
        showMonthHeader: false,
        headerHeight: 40,
        dayHeaderStyle: 'large',
        showEmptyCells: true,
        gridStyle: 'full'
    });

    if (!monthInfo) {
        return;
    }

    const { dayGridTop, dayGridHeight, cellWidth, cellHeight, rows, startDayOfWeek, daysInMonth } = monthInfo;

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
    const startHour = state.options.startHour || 8;
    const endHour = state.options.endHour || 18;

    // 各日付セルのバーチャートを描画
    let day = 1;
    for (let row = 0; row < rows; row++) {
        for (let col = 0; col < 7; col++) {
            const cellIndex = row * 7 + col;
            if (cellIndex < startDayOfWeek || day > daysInMonth) {
                continue;
            }

            const cellLeft = col * cellWidth;
            const cellTop = dayGridTop + row * cellHeight;
            const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            const isHoliday = state.holidays.has(dateStr);

            // バーチャートを描画
            renderCanvasDayBarChart(contexts, state, {
                cellLeft,
                cellTop,
                cellWidth,
                cellHeight,
                dateStr,
                dayNumber: day,
                isHoliday
            });

            // Equipment折れ線グラフを描画（各セル内、時間軸に沿って）
            const equipmentStats = state.equipmentStats.get(dateStr);
            if (equipmentStats) {
                // 日付テキストの高さを計算（バーチャートと同じ）
                const dateFontSize = 12;
                const dayTextHeight = dateFontSize + 4;
                const barAreaTop = cellTop + dayTextHeight;
                const labelAreaHeight = (cellWidth >= 40 && cellHeight >= 50) ? 12 : 0;
                const barAreaHeight = Math.max(0, cellHeight - dayTextHeight - 4 - labelAreaHeight);

                renderCanvasLineChart(contexts.get('content'), {
                    cellLeft,
                    cellTop,
                    cellWidth,
                    cellHeight,
                    dateStr,
                    barAreaTop,
                    barAreaHeight,
                    equipmentStats,
                    startHour,
                    endHour,
                    lunchStartHour,
                    lunchEndHour,
                    isYearView: false
                });
            }

            day++;
        }
    }

    // すべての描画が完了したら、オフスクリーンバッファをメインCanvasに一括転送
    canvasManager.commitAll();
}


